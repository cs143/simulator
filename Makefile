CSC=mcs

all: Simulator.exe

Simulator.exe: *.cs
	$(CSC) -debug+ -o Simulator.exe *.cs
	chmod +x Simulator.exe # Mono compiler doesn't seem to set it executable

run: Simulator.exe
	./Simulator.exe
