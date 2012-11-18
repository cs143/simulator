CSC=mcs

all: Simulator.exe

Simulator.exe: *.cs
	$(CSC) -debug+ -sdk:4 -r:C5.Mono.dll -o Simulator.exe *.cs
	chmod +x Simulator.exe # Mono compiler doesn't seem to set it executable

run: Simulator.exe
	./Simulator.exe
