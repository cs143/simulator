CSC=mcs

all: simulator.exe

simulator.exe: simulator.cs
	$(CSC) simulator.cs
	chmod +x simulator.exe # Mono compiler doesn't seem to set it executable

run: simulator.exe
	./simulator.exe