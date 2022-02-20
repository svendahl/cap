; executable depacker

jumpto	= $fce2

deploc	= $f3
	* = 2049
	.word bnl,0
	.null $9e,format("%4d", start)
start
	ldy #0
bnl
	sei
	inc $01
deplp
	lda apd_dep,y
	sta deploc,y
	iny
	bne deplp
	ldx #>dataend - >data + 1
datalp
	lda dataend-256,y;$945e -> $085e
	sta $ff00,y;$ff00 -> $7300
	iny
	bne datalp
	dec datalp+2
	dec datalp+5
	dex
	bne datalp
	jmp apd_read_literal
	
apd_dep
.logical deploc
apd_lwm	= * - 7

apd_length_l	= * - 6
apd_length_h	= apd_length_l + 1

apd_offset_l	= * - 4
apd_offset_h	= apd_offset_l + 1

apd_source	= * - 2
apd_source_l = apd_source
apd_source_h = apd_source + 1

apd_bitbuffer .byte 128
; -------
; getbyte
; -------
apd_getbyte
	lda $ffff - (dataend - data) + 1;skip loadaddress. should point @ 0c,29,49,20,01,77,9c,30
	inc apd_getbyte + 1
	bne +
	inc apd_getbyte + 2
	;beq apd_end
+	rts
apd_dest	.binary "data.cap",0,2	; loadaddress from binary
apd_dest_l = apd_dest
apd_dest_h = apd_dest + 1


; -----------
; entry point
; -----------
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
	jsr apd_getbit	; if (aP_getbit(&ud))...
	bcc apd_read_literal
		; #1
	jsr apd_getbit
	bcc apd_large_match
		; #11
	jsr apd_getbit
	bcc apd_short_match
apd_mini_match		; #111
	lda #%11100000	; for (i = 4; i > 0; i--)
apd_mini_match_loop
	jsr apd_getbit
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
	lda #$37	; io mode
	sta $01
	cli	; interrupt mode
	jmp jumpto	; start address
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
	sbc apd_offset_l
	sta apd_source_l
	lda apd_dest_h
	sbc apd_offset_h
	sta apd_source_h

; ----------
; copy match
; ----------

	ldx apd_length_h
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
	ldx apd_length_l
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
apd_getbit
	asl apd_bitbuffer
	bne apd_getbit_end
	pha
	jsr apd_getbyte
	;sec
	rol
	sta apd_bitbuffer
	pla
apd_getbit_end	rts


; ---------
; get gamma
; ---------
apd_getgamma
	sty apd_length_h
	lda #1
apd_getgamma_loop
	jsr apd_getbit
	rol
	rol apd_length_h
	jsr apd_getbit
+	bcs apd_getgamma_loop
	sta apd_length_l
	rts

.here

data
.binary	"data.cap",2;skip load address here
dataend
