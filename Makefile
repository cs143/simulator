CSC=dmcs

all: Simulator.exe

Simulator.exe: *.cs
	$(CSC) -debug+ -d:DEBUG -r:MoreLinq.dll -o Simulator.exe *.cs
	chmod +x Simulator.exe # Mono compiler doesn't seem to set it executable

# To run:
# make && MONO_TRACE_LISTENER=Console.Out mono --debug ./Simulator.exe <config_file>