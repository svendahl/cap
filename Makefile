MCS=mcs
SOURCES='*.cs'
cap.exe : cap.cs constant.cs decap.cs esa.cs interval.cs model.cs path.cs Program.cs sais.cs state.cs tobinary.cs
	$(MCS) -out:$@ -recurse:$(SOURCES)
clean :
	rm -f cap.exe
.PHONY : clean
