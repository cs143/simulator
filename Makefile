CSC=dmcs

all: Simulator.exe

Simulator.exe: *.cs
	$(CSC) -debug+ -r:MoreLinq.dll -o Simulator.exe *.cs
	chmod +x Simulator.exe # Mono compiler doesn't seem to set it executable

run: Simulator.exe
	./Simulator.exe
