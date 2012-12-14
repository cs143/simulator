Network Simulator for CS 143, WI 2012
=====================================
By Michael Hirshleifer, Roy Koczela, Kijun Seo

# What have we implemented?

1. TCPReno (Fast Retransmit) and FastTCP congestion control 
2. Link-state protocol routing (Dijkstra) with dynamic link cost and message passing
3. Flow simulator
4. Log analyzer

# Running
You run the simulator and the visualization separately. First run the simulator on the config files. Then run the log analyzer visualization on the output log files.

## Simulator

### On Linux & OS X

  1. install mono if necessary <http://mono-project.com>
  2. run `make all`
  3. run <code>mono Simulator.exe <var>(config_file)</var></code>

Or, to run all 4 test cases at once, run `make run_all_test_cases`.

### On Windows

  1. Open simulator.csproj in Visual Studio
  2. Choose Start Debugging from Debug menu, or just build and run
  3. Enter config file you want to run

## Log Analyzer

### On Linux & OS X

  0. run `make runsimgrapher`
  0. Select a log file (in `logs/` subdirectory) to view it

### On Windows

  1. Compile and run SimGrapher.csproj
  2. select a logfile

Some Stats
----------
https://github.com/cs143/simulator/graphs/contributors
