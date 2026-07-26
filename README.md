# Elevator System Simulation

Discrete-time **Destination Dispatch** elevator simulator in C# (.NET 9). Passengers provide origin and destination at request time; the system assigns a car immediately and never changes that destination.

> The take-home brief mentions Python; this implementation uses **C#** by agreement.

## Build & run

```bash
dotnet restore ElevatorSim.sln
dotnet test ElevatorSim.sln
dotnet run --project src/ElevatorSim -- \
  --input samples/requests.csv \
  --floors 55 \
  --elevators 4 \
  --capacity 8 \
  --algorithm cost
```

### Options

| Flag | Default | Description |
|------|---------|-------------|
| `--input` | `samples/requests.csv` | Request CSV (`time,id,source,dest`) |
| `--floors` | `55` | Building height |
| `--elevators` | `4` | Number of cars |
| `--capacity` | `8` | Max passengers per car |
| `--algorithm` | `cost` | `cost`, `roundrobin`, or `compare` |
| `--log` | (stdout) | Write position log to a file |

Compare algorithms on the same input:

```bash
dotnet run --project src/ElevatorSim -- \
  --input samples/requests.csv \
  --algorithm compare
```

## How it works

- **Time:** 1 tick = 1 floor of travel. The clock advances one tick at a time; requests are admitted only when `time == now` (no peeking at future rows for scheduling decisions).
- **Cars:** SCAN-style motion — keep current direction while stops remain that way, then reverse or go idle.
- **Tick order:** admit & assign → alight/board → log floors → move.
- **Dispatchers:**
  - **`cost`** — assign to the car with lowest estimated `ticks_to_pickup + trip_distance`, preferring cars with spare reserved capacity.
  - **`roundrobin`** — cycle through cars with spare capacity (baseline for comparison).

## Assumptions & trade-offs

- Boarding and alighting take **0 ticks** (no door dwell).
- Elevators start **idle at floor 1**.
- Assigned-but-waiting passengers **reserve a capacity slot** so we do not over-assign when avoidable; if every car is reserved-full, assignment still happens (immediate assignment is required) and boarding waits for free space.
- Floors are numbered `1..N`.


## Project layout

```
src/ElevatorSim/        # domain, simulation, dispatchers, CSV loader, CLI
tests/ElevatorSim.Tests/
samples/requests.csv
```

