MCS=mcs
SOURCES='*.cs'
cap.exe : cap.cs decap.cs edge.cs model.cs path.cs pqueue.cs Program.cs state.cs
	$(MCS) -out:$@ -recurse:$(SOURCES)
clean :
	rm -f cap.exe
.PHONY : clean
