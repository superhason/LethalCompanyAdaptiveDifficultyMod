# Lethal Company Adaptive Difficulty Mod

A BepInEx mod for *Lethal Company* that studies game balance by tracking
player performance and dynamically adjusting enemy spawn pressure and the
in-game economy in response — a small, self-contained case study in adaptive
difficulty design, built as a portfolio project.

**Status:** M0-M6 core loop complete (telemetry → scoring → gameplay effects
→ persistence → config). See `docs/ARCHITECTURE.md` for full milestone
history and verified game hooks.

## What it does

- Watches round starts/ends, player deaths, and quota clears via Harmony
  patches (no game files are modified — everything is runtime patching).
- Feeds those events into a single running **difficulty score** (`0.0`-`1.0`).
- That score scales two independent systems: enemy spawn power (harder
  fights as players do well) and scrap sell value (bigger payouts at higher
  difficulty — a risk/reward curve, not a punishment mechanic).
- The score persists per save file and survives game restarts.
- Every tunable number is exposed via a BepInEx config file, editable
  without recompiling.

## Design philosophy

The goal is to make the game *more interesting when players are doing
well*, not to punish failure. Concretely, that shaped two decisions:

- The score is bidirectional (it can go back down after deaths), but that's
  framed as *easing off* to stay appropriately challenging — not a penalty.
  This mirrors how systems like Left 4 Dead's AI Director work: one running
  estimate of player state, driving multiple subsystems in the same
  direction.
- Economy scaling was deliberately flipped from the original plan (reduce
  scrap value as difficulty rises) to the opposite (increase it) at the
  project owner's direction mid-build — higher difficulty means higher
  stakes *and* higher reward, not just more punishment stacked on more
  punishment. The mod currently has no signal for outright quota failure, so
  there is effectively no "punish bad play" path at all — only "reward good
  play," at varying intensity.

## The algorithm

A single float, `DifficultyScore`, starts at `0.5` (neutral) and is nudged
by two independent events:

- **Round end** — `Deaths / PlayerCount` (a crew-size-adjusted death ratio,
  so 2 deaths in a 4-player round hits harder than 2 deaths in an 8-player
  round) is multiplied by a penalty weight, clamped, and applied as a
  fraction of the current score.
- **Quota cleared** — a flat bonus applied independently, the moment the
  game's own `SetNewProfitQuota()` fires (proven via testing to run
  *before* `EndOfGameClientRpc`, which is why quota handling is a fully
  separate code path rather than bundled into round-end metrics — see
  "Lessons learned" below).

The score then drives two `Lerp`-based multipliers — enemy spawn power and
scrap value — read directly from BepInEx config, so the mapping curve can
be retuned without touching code.

## Lessons learned (the interesting bugs)

A few things surfaced during development that are worth calling out
explicitly, since finding and fixing them was as much the point of this
project as the final feature set:

- **RPC methods can fire twice.** Unity Netcode weaves "send" and "local
  receive" logic into the same compiled `ClientRpc` method, so a Harmony
  patch on one can run twice per logical event, even solo-hosting. Harmless
  for logging, but a real bug for anything stateful (like score mutation) —
  handled with a per-frame debounce guard where it mattered, and recognized
  as safe-to-ignore where the underlying operation was idempotent (e.g.
  multiplying a value that gets freshly reset before each multiplication).
- **Hook ordering isn't always what you'd guess.** An early design bundled
  the quota-clear signal into the same `RoundMetrics` object as round-end
  death data, assuming quota resolution happened *before* round-end. Testing
  proved the opposite — which would have silently misattributed every
  quota clear to the wrong day. Fixed by decoupling quota-clear into its own
  independent score update, rather than trying to coordinate timing between
  two hooks.
- **Field names can be misleading.** `connectedPlayersAmount` turned out to
  mean "players besides the host," not total crew size — caught by
  cross-referencing a field's actual usage elsewhere in the decompiled
  source (`connectedPlayersAmount + 1 - livingPlayers`), not by trusting the
  name at face value.
- **All game-side hooks were found via dnSpy, not guessed.** Every
  patched method/field in this project (`RoundManager.RefreshEnemiesList`,
  `StartOfRound.EndOfGameClientRpc`, `TimeOfDay.SetNewProfitQuota`, etc.) was
  located by tracing real decompiled source and call graphs, and re-verified
  when behavior didn't match assumptions — necessary since none of this is a
  stable public API and can change between game updates.

## Tech stack

- C# / .NET Framework 4.7.2
- Unity (Mono build of Lethal Company)
- BepInEx 5.4.x
- Harmony (via BepInEx's bundled `0Harmony.dll`)
- dnSpyEx for inspecting game assemblies
- MSTest for unit testing the difficulty-scoring logic in isolation from Unity

## Project structure

```
.
├── README.md
├── LICENSE
├── .gitignore
├── docs/
│   └── ARCHITECTURE.md          # design rationale, verified hooks, milestone history
└── src/
    ├── AdaptiveDifficultyMod.slnx
    ├── AdaptiveDifficultyMod/
    │   ├── Plugin.cs             # BepInEx entry point
    │   ├── Core/
    │   │   ├── ModState.cs       # holds the running DifficultyState, lazy per-save loading
    │   │   ├── PluginConfig.cs   # BepInEx config bindings for all tunables
    │   │   └── DifficultyPersistence.cs  # save/load score per save file
    │   ├── Metrics/
    │   │   ├── RoundMetrics.cs       # per-round data: deaths, player count
    │   │   └── PerformanceTracker.cs # accumulates a round's raw counters
    │   ├── Difficulty/
    │   │   ├── DifficultyCalculator.cs # pure, engine-independent scoring logic
    │   │   └── DifficultyState.cs      # stateful wrapper, bridges config -> calculator
    │   └── Patches/
    │       ├── RoundManagerPatches.cs   # round-start log, enemy spawn scaling, scrap value scaling
    │       ├── StartOfRoundPatches.cs   # round-end telemetry, feeds DifficultyState
    │       ├── TimeOfDayPatches.cs      # quota-clear telemetry, feeds DifficultyState
    │       └── PlayerControllerBPatches.cs # death telemetry
    └── AdaptiveDifficultyMod.Tests/
        └── DifficultyCalculatorTests.cs  # unit tests, no Unity/BepInEx dependency
```

## Building

1. Open `src/AdaptiveDifficultyMod.slnx` in Visual Studio 2022.
2. The project references BepInEx, Harmony, and Unity/game assemblies from a
   local Lethal Company install (`BepInEx/core` and
   `Lethal Company_Data/Managed`). Update these references if your game is
   installed in a different location.
3. Build (Ctrl+Shift+B). A post-build step copies the compiled DLL into the
   game's `BepInEx/plugins/AdaptiveDifficultyMod/` folder automatically.
   **Note:** the post-build copy path is currently hardcoded to one machine's
   install path (see project properties → Build Events) — update it to match
   your own game install location.

## Configuration

On first run, a config file is generated at
`BepInEx/config/com.hanson.lethalcompany.adaptivedifficulty.cfg` with the
following tunables (all editable, all take effect on next launch):

| Setting | Default | Meaning |
|---|---|---|
| `Difficulty.QuotaMetBonus` | `0.3` | Score bonus when a quota is cleared |
| `Difficulty.DeathRatioPenaltyWeight` | `-0.6` | Penalty weight for a full-crew wipe |
| `Difficulty.AdjustmentRate` | `0.35` | How strongly each event nudges the score |
| `EnemySpawning.MinMultiplier` | `0.75` | Enemy spawn power at minimum difficulty |
| `EnemySpawning.MaxMultiplier` | `1.5` | Enemy spawn power at maximum difficulty |
| `Economy.MinMultiplier` | `1.0` | Scrap value at minimum difficulty |
| `Economy.MaxMultiplier` | `1.5` | Scrap value at maximum difficulty |

## Testing

`AdaptiveDifficultyMod.Tests` covers the core scoring logic (`DifficultyCalculator`)
with unit tests that run with no dependency on Unity, BepInEx, or the game —
by design, so the balance math can be verified in isolation. Run via Visual
Studio's Test Explorer.

## License

MIT — see `LICENSE`.
