CSC=dmcs

all: Simulator.exe
	make -C SimGrapher simgrapher

Simulator.exe: *.cs
	$(CSC) -debug+ -d:DEBUG -r:MoreLinq.dll -o Simulator.exe *.cs
	chmod +x Simulator.exe # Mono compiler doesn't seem to set it executable

# To run simulator:
# make && MONO_TRACE_LISTENER=Console.Out mono --debug ./Simulator.exe <config_file>

runsimgrapher: all
	mono ./SimGrapher/bin/Debug/SimGrapher.exe
