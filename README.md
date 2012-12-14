Network Simulator for CS 143, WI 2012
=====================================
By Michael Hirshleifer, Roy Koczela, Kijun Seo

What Have We Implemented?
-------------------------

1. TCPReno (Fast Retransmit) and FastTCP congestion control 
2. Link-state protocol routing (Dijkstra) with dynamic link cost
3. Flow simulator
4. Log analyzer

Running Simulator
-----------------

On Linux & OS X

  1. install mono <http://mono-project.com>
  2. run `make`
  3. run `mono Simulator.exe`

On Windows

  1. Open simulator.csproj in Visual Studio
  2. Choose Start Debugging from Debug menu, or just build and run

Running Log Analyzer
--------------------

On Windows

  1. Compile and run SimGrapher.csproj
  2. select a logfile

Some Stats
----------
https://github.com/cs143/simulator/graphs/contributors
