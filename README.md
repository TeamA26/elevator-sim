# Elevator System Simulation

Discrete-time **Destination Dispatch** elevator simulator in C# (.NET 9). Passengers provide origin and destination at request time; the system assigns a car immediately.

## Prerequisites

Install the **.NET 9 SDK** (includes the runtime and `dotnet` CLI):

| Platform | How to install |
| -------- | -------------- |
| **macOS** | [Download the macOS installer](https://dotnet.microsoft.com/download/dotnet/9.0), or with Homebrew: `brew install dotnet` |
| **Windows** | [Download the Windows installer](https://dotnet.microsoft.com/download/dotnet/9.0) (x64 or Arm64 to match your machine) |

Confirm the SDK is available:

```bash
dotnet --version
```

You should see a `9.x` version.

## Clone, build & run

```bash
git clone https://github.com/TeamA26/elevator-sim.git
cd elevator-sim
dotnet restore ElevatorSim.sln
dotnet test ElevatorSim.sln
```

### macOS / Linux / Git Bash

```bash
dotnet run --project src/ElevatorSim -- \
  --input samples/requests.csv \
  --floors 55 \
  --elevators 4 \
  --capacity 8 \
  --algorithm cost
```

### Windows (PowerShell or Command Prompt)

Put the arguments on one line (line continuation with `\` is a bash feature):

```powershell
dotnet run --project src/ElevatorSim -- --input samples/requests.csv --floors 55 --elevators 4 --capacity 8 --algorithm cost
```

### Options

| Flag          | Default                | Description                         |
| ------------- | ---------------------- | ----------------------------------- |
| `--input`     | `samples/requests.csv` | Request CSV (`time,id,source,dest`) |
| `--floors`    | `55`                   | Building height                     |
| `--elevators` | `4`                    | Number of cars                      |
| `--capacity`  | `8`                    | Max passengers per car              |
| `--algorithm` | `cost`                 | `cost`, `roundrobin`, or `compare`  |
| `--log`       | (stdout)               | Write position log to a file        |

Compare algorithms on the same input:

```bash
dotnet run --project src/ElevatorSim -- --input samples/requests.csv --algorithm compare
```

## How it works

- **Time:** 1 tick = 1 floor of travel. The clock advances one tick at a time; requests are admitted only when `time == now` (no peeking at future rows for scheduling decisions).
- **Cars:** Keep current direction while stops remain that way, then reverse or go idle.
- **Tick order:** admit & assign → alight/board → log floors → move.
- **Dispatchers:**
  - `**cost`** — assign to the car with lowest estimated `ticks_to_pickup + trip_distance`, preferring cars with spare reserved capacity.
  - `**roundrobin`** — cycle through cars with spare capacity (baseline for comparison).

## Assumptions & trade-offs

- Boarding and alighting take **0 ticks.** 
- Elevators start **idle at floor 1**.
- Assigned-but-waiting passengers **reserve a capacity slot** so we do not over-assign when avoidable; if every car is reserved-full, assignment still happens (immediate assignment is required) and boarding waits for free space.
- Floors are numbered `1..N`.

## Project layout

```
src/ElevatorSim/        # domain, simulation, dispatchers, CSV loader, CLI
tests/ElevatorSim.Tests/
samples/requests.csv
```

