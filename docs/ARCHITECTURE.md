# Architecture

## Goal

Adaptive Difficulty / Balance Tuning mod for Lethal Company. Tracks player
performance and dynamically adjusts enemy spawning and economy, as a study of
game balance design.

## Design principle

Separate three concerns so the core balance logic stays testable without the
game running:

- **Measure** what happened (Metrics)
- **Decide** what should change (Difficulty)
- **Apply** the change to the game (Patches)

## Difficulty philosophy

The goal is to ramp challenge up when players are doing well, not to punish
failure. Concretely: the score still decreases when the crew takes deaths —
but that's framed as *easing off* to keep the game appropriately challenging,
not a penalty. This matches how adaptive-difficulty systems generally work
(e.g. Left 4 Dead's AI Director both intensifies and backs off based on
player state) — the score is bidirectional, but the intent behind a decrease
is "help the player," not "punish the player." There is currently no signal
for outright quota failure (see M2 notes below), so the only genuinely
negative input is the crew-size-adjusted death ratio, not a failure event.

## Planned class structure

```
AdaptiveDifficultyMod/
├── Plugin.cs                   # BepInEx entry point
├── Core/
│   ├── PluginConfig.cs         # BepInEx config bindings
│   └── ModState.cs             # central runtime state, read by patches
├── Metrics/
│   ├── RoundMetrics.cs         # data model: one round's stats
│   └── PerformanceTracker.cs   # listens to game events, fills RoundMetrics
├── Difficulty/
│   ├── DifficultyCalculator.cs # pure logic: metrics history -> difficulty score
│   └── DifficultyState.cs      # current score + history, persisted across rounds
├── Patches/
│   ├── StartOfRoundPatches.cs  # round start/end hooks
│   ├── RoundManagerPatches.cs  # enemy spawn count/power hooks
│   └── TerminalPatches.cs      # economy hooks (scrap value, prices)
└── Utils/
    └── Log.cs                  # thin wrapper over BepInEx logger
```

`Plugin.cs` and `Patches/` exist so far (M0-M1). The rest is added as
milestones are implemented.

## Verified hook points (via dnSpyEx)

Found by tracing event usage and RPC callers in dnSpyEx's Analyzer, not
assumed from memory — see notes below on why that matters.

- **Round start**: `RoundManager.FinishGeneratingNewLevelClientRpc()` — fires
  after the server generates a new moon's layout; triggers the ship's
  door-opening sequence. Fires twice per round (see note below).
- **Round end**: `StartOfRound.EndOfGameClientRpc(int, int, int, int)` —
  called unconditionally at the end of `unloadSceneForAllPlayers()`, i.e.
  every time the crew returns to orbit, not just a full game-over. Also fires
  twice per round.
- **Player death**: `GameNetcodeStuff.PlayerControllerB.KillPlayer(...)` —
  fires once per death (not RPC-wrapped like the two above).

**Why the round hooks fire twice:** Unity Netcode weaves both the
"send" and "local receive" logic into the same compiled `ClientRpc` method
(visible in dnSpy as calls to `__beginSendClientRpc`/`__rpc_exec_stage`).
Both code paths invoke the same `MethodInfo`, so a Harmony patch on it runs
twice per logical event even solo-hosting. This must be accounted for once
these hooks feed into real scoring logic (M2+) — e.g. via a debounce or by
checking `__rpc_exec_stage`/`IsServer`/`IsClient` inside the patch.

## Milestones

- [x] **M0** – Scaffold: plugin builds, deploys, and loads in-game with a log message.
- [x] **M1** – Read-only telemetry: Harmony patches that log round start/end and player death, no behavior changes.
- [x] **M2** – Difficulty Calculator: pure, engine-independent logic that turns metrics history into a difficulty score. Implemented as `RoundMetrics` (Deaths, QuotaMet, PlayerCount), `DifficultyCalculator.UpdateScore` (pure, tested), and `DifficultyState` (stateful wrapper). Score range `0.0`-`1.0`, `adjustmentRate = 0.35`. Death penalty is proportional to crew size (`deathRatio = Deaths / PlayerCount`, weight `-0.6` = full-wipe penalty), so the same death count is penalized less on a larger crew. Covered by 8 unit tests in `AdaptiveDifficultyMod.Tests`. Wired to real
telemetry via `PerformanceTracker` (accumulates deaths per round) and
`ModState` (holds the running `DifficultyState`). Quota-clear is applied as
its own independent nudge (`TimeOfDayPatches` → `SetNewProfitQuota`), not
bundled into round-end metrics — `EndOfGameClientRpc` fires before
`SetNewProfitQuota` in the real game flow, so combining them into one
`RoundMetrics` object caused a one-round misattribution bug, caught via
in-game testing before it shipped.
- [x] **M3** – Apply difficulty to enemy spawning via `RoundManagerPatches`. `EnemySpawnScalingPatch` postfixes `RoundManager.RefreshEnemiesList()`, multiplying `currentMaxOutsidePower`/`currentMaxInsidePower` by `Lerp(0.5, 2.0, DifficultyScore)`. Deliberately not centered at `1.0x` at the default score — baseline is `1.25x`, matching the mod's goal of leaning toward more challenge. Verified in-game: multiplier correctly reflects the score *entering* each round, not mid-round changes.
- [x] **M4** – Apply difficulty to economy. Design pivoted from "punish failure" (reduce scrap value at high difficulty) to "reward risk" (increase it) at the user's direction, matching the mod's stated philosophy. `ScrapValueScalingPatch` prefixes `RoundManager.LoadNewLevel(int, SelectableLevel)`, setting `scrapValueMultiplier = Lerp(1.0, 2.0, DifficultyScore)` — floored at `1.0` (never penalizes), scaling up to `2.0x` at max difficulty. Lives in `RoundManagerPatches.cs`, not a `TerminalPatches.cs` — the real field (`RoundManager.scrapValueMultiplier`) turned out to live on `RoundManager`, not `Terminal`, so the file was named to match reality rather than the original guess. Verified in-game: log line and actual sale price both confirmed at the default score.
- [x] **M5** – Persist difficulty state across sessions. `DifficultyPersistence` reads/writes a plain-text score file keyed by `GameNetworkManager.currentSaveFileName`, stored under `BepInEx/plugins/AdaptiveDifficultyMod/`. `ModState.EnsureLoadedForSave` lazily (and idempotently) loads on first use per save slot; `ModState.PersistCurrentScore` writes after every score-mutating event. Verified in-game across a full game restart.
- [ ] **M6** – Polish: exposed config, debug overlay, README writeup of design rationale.

## Notes on game-side class/method names

Lethal Company's internal class and method names (e.g. `RoundManager`,
`StartOfRound`, `Terminal`) are not part of a stable public API and can change
between game updates. Always verify exact names and signatures in
dnSpyEx/ILSpy against the installed game version before writing a patch
against them.
