; configurable depacker
; use 64tass v1.5+

; configuration options
apd_conf_z_bitbuffer	= 1	; 0 = normal memory, 1 = zeropage
apd_conf_z_lwm	= 1	; 0 = normal memory, 1 = zeropage
apd_conf_z_length	= 1	; 0 = self modifying code, 1 = zeropage
apd_conf_z_offset	= 1	; 0 = self modifying code, 1 = zeropage
apd_conf_getbit_inline	= 1	; 0 = subroutine, 1 = inlined



; zeropage
.if apd_conf_z_bitbuffer = 1
apd_bitbuffer	= $f6
.fi

.if apd_conf_z_lwm = 1
apd_lwm	= $f7
.fi

.if apd_conf_z_length = 1
apd_length_l	= $f8
apd_length_h	= apd_length_l + 1
.fi

.if apd_conf_z_offset = 1
apd_offset_l	= $fa
apd_offset_h	= apd_offset_l + 1
.fi

apd_source	= $fc
apd_source_l	= apd_source
apd_source_h	= apd_source + 1

apd_dest	= $fe
apd_dest_l	= apd_dest
apd_dest_h	= apd_dest + 1

; -----------
; entry point
; -----------
	sta apd_getbyte + 1
	sty apd_getbyte + 2

	jsr apd_getbyte
	sta apd_dest_l
	jsr apd_getbyte
	sta apd_dest_h

	ldy #0
	sty apd_bitbuffer
apd_read_literal
	jsr apd_getbyte	; *ud.destination++ = *ud.source++;
apd_write_literal
	sta (apd_dest),y

	inc apd_dest_l
	bne apd_clear_lwm
	inc apd_dest_h
apd_clear_lwm
	sty apd_lwm	; lwm = 0;
apd_read_flag
	#apd_mgetbit	; if (aP_getbit(&ud))...
	bcc apd_read_literal
		; #1
	#apd_mgetbit
	bcc apd_large_match
		; #11
	#apd_mgetbit
	bcc apd_short_match
apd_mini_match		; #111
	lda #%11100000	; for (i = 4; i > 0; i--)
apd_mini_match_loop
	#apd_mgetbit
	rol	; offs = (offs << 1) + getBit();
	bcs apd_mini_match_loop
		; if (offs == 0)
	beq apd_write_literal	; destination[destinationOffset] = 0;
	sbc apd_dest_l	; else
	eor #%11111111	; destination[destinationOffset] = destination[destinationOffset - offs];
	sta apd_source_l
	tya
	sbc apd_dest_h
	eor #%11111111
	sta apd_source_h
	lda (apd_source),y
	bne apd_write_literal	; bra
apd_end
	rts
apd_short_match		; #110
	jsr apd_getbyte	; offs = source[sourceOffset++] & 0xff;
	lsr	; offs >>= 1;
	beq apd_end	; if (offs > 0) {
	sta apd_offset_l
	sty apd_offset_h
	lda #1	; len = 2 + (offs & 0x0001);
	rol
	sta apd_length_l
	sty apd_length_h
	bne apd_set_lwm	; bra
apd_large_match		; #10
	jsr apd_getgamma	; offs = getGamma();
	cmp #2	; if ((lwm == 0) && (offs == 2)) {
	bne apd_offset_match
.if apd_conf_z_lwm = 0
apd_conf_z_lwm = * + 1
.fi
	cpy apd_lwm
	bne apd_offset_match

; -------------------
; repeat offset match
; -------------------
apd_repeat_offset_match
		; r0 = offs;
	jsr apd_getgamma	; len = getGamma();
	bcc apd_set_lwm	; bra

; -------------------
; normal offset match
; -------------------
apd_offset_match
	lsr apd_lwm	; if (lwm == 0)
		; offs -= 3;
	sbc #2	; else
		; offs -= 2;
	sta apd_offset_h	; offs <<= 8;
	jsr apd_getbyte
	sta apd_offset_l	; offs += source[sourceOffset++] & 0xff;
		; r0 = offs;
	jsr apd_getgamma	; len = getGamma();

; -------------
; adjust length
; -------------
	lda apd_offset_h	; if (offs < 128) len += 2;
	bne apd_al_chkh
	bit apd_offset_l
	bmi apd_set_lwm
	lda #2
	bcc apd_al_add	; bra
apd_al_chkh
	cmp #>1280	; if (offs >= 1280) len++;
	bcc apd_set_lwm
	cmp #>32000	; if (offs >= 32000) len += 2;
	tya
	adc #1
apd_al_add
	adc apd_length_l
	sta apd_length_l
	bcc apd_set_lwm
	inc apd_length_h

; -------
; set lwm
; -------
apd_set_lwm
	lda #1
	sta apd_lwm	; lwm = 1

; ----------------
; calculate source
; ----------------
apd_calc_source
	sec	; source = dest - offset;
apd_calc_source_cs
	lda apd_dest_l
.if apd_conf_z_offset = 1
	sbc apd_offset_l
.else
	apd_offset_l = * + 1
	sbc #0
.fi
	sta apd_source_l
	lda apd_dest_h
.if apd_conf_z_offset = 1
	sbc apd_offset_h
.else
	apd_offset_h = * + 1
	sbc #0
.fi
	sta apd_source_h

; ----------
; copy match
; ----------

.if apd_conf_z_length = 1
	ldx apd_length_h
.else
	apd_length_h = * + 1
	ldx #0
.fi
	beq apd_cm_skiploop1
apd_cm_loop1
	lda (apd_source),y
	sta (apd_dest),y
	iny
	bne apd_cm_loop1

	inc apd_source_h
	inc apd_dest_h
	dex
	bne apd_cm_loop1
apd_cm_skiploop1
.if apd_conf_z_length = 1
	ldx apd_length_l
.else
	apd_length_l = * + 1
	ldx #0
.fi
	beq apd_cm_end
apd_cm_loop2	lda (apd_source),y
	sta (apd_dest),y
	iny
	dex
	bne apd_cm_loop2
	clc
	tya
	adc apd_dest_l
	sta apd_dest_l
	bcc apd_cm_end
	inc apd_dest_h
;	bne apd_destwrap
apd_cm_end
	ldy #0
	jmp apd_read_flag

; ------
; getbit
; ------
apd_getbit_refill
	pha
	jsr apd_getbyte
	sec
	rol
	sta apd_bitbuffer
	pla
	rts
.if apd_conf_z_bitbuffer = 0
apd_bitbuffer	.byte 0
.fi

.if apd_conf_getbit_inline = 0
apd_getbit
	asl apd_bitbuffer
	beq apd_getbit_refill
	rts
.fi

; ---------
; get gamma
; ---------
apd_getgamma
	sty apd_length_h
	lda #1
apd_getgamma_loop
	#apd_mgetbit
	rol
	rol apd_length_h
	#apd_mgetbit
+	bcs apd_getgamma_loop
	sta apd_length_l
	rts

; -------
; getbyte
; -------
apd_getbyte
	lda $ffff
	inc apd_getbyte + 1
	bne apd_getbyte_end
	inc apd_getbyte + 2
apd_getbyte_end
;	ldy #0	; Y must be 0 on exit
	rts

; ------------
; getbit macro
; ------------
.if apd_conf_getbit_inline = 1
apd_m_getbit	.macro
	asl apd_bitbuffer
	bne apd_m_getbit_end
	jsr apd_getbit_refill
apd_m_getbit_end
	.endm
.fi


apd_mgetbit	.macro
.if apd_conf_getbit_inline = 1
	#apd_m_getbit
.else
	jsr apd_getbit
.fi
apd_mgetbit_end
	.endm
