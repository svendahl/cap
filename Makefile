MCS=mcs
SOURCES='*.cs'
cap.exe : cap.cs constant.cs decap.cs edge.cs model.cs path.cs pqueue.cs Program.cs saaux.cs sae.cs sais.cs state.cs tobinary.cs
	$(MCS) -out:$@ -recurse:$(SOURCES)
clean :
	rm -f cap.exe
.PHONY : clean
