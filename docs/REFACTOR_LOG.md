# Refactor Log

Retrospective of the structural refactor work applied to the `CriminalCase2` Unity 6.3 LTS detective/investigation game. Covers 10 refactor phases (Phase 0 through Phase 10) executed in 8 PRs after the original 16-commit "first progress" series landed at `477353b`.

## Executive Summary

**Starting state** (pre-refactor, `477353b`): 17 hand-written `.cs` files, no tests, no assembly definitions, no DI, three singleton managers reachable via `static Instance`, `Resources.Load` at runtime, `Update()` polling for state, `Debug.Log` everywhere, magic-string UXML names, a hardcoded `SuspectRole.Normal` fallback in `LevelManager.GetSuspectVerdict`, a `Resources/UI` folder that masked a missing `PanelSettings` reference, and a `VideoPlayerPanel` whose UGUI state was never re-wired after a prior UGUI → UI Toolkit migration.

**Ending state** (post-Phase 10, `20fc601`): 78 EditMode tests across 9 test files, two asmdefs (`CriminalCase2.Runtime` + `CriminalCase2.Tests.EditMode`), five ViewModel classes (`SuspectDetailViewModel`, `StatusHUDViewModel`, `TutorialUIViewModel`, `CheckStatusViewModel`, `ResultViewModel`), a `GameServices` static composition root with `Register(object)` pattern matching, an `IsExternalInit` polyfill enabling `record` types, every `Resources.Load` removed, every `static Instance` removed, every `UnityEngine.UI` reference removed, no `Update()`-driven state checks anywhere, and a `docs/REFACTOR_LOG.md` (this file) to make the rationale reviewable.

**Test progression:** 0 → 3 → 6 → 8 → 12 → 15 → 31 → 52 → 76 → 78.
**PR progression:** PR #1 (Phases 0–3) → PR #2 (Phase 4) → PR #3 (Phase 5) → PR #4 (Phase 6) → PR #5 (Phase 7) → PR #6 (Phase 8) → PR #7 (Phase 9) → PR #8 (Phase 10).

## Pre-Refactor Baseline

The codebase at `477353b` had the following structural problems, all of which the refactor plan was designed to fix:

| Problem | First seen at | Fix landed in |
|---|---|---|
| All 17 `.cs` files in default `Assembly-CSharp`; no asmdef | inherited | Phase 0 |
| 0 EditMode tests | inherited | Phase 0 |
| `GameManager.Instance` / `LevelManager.Instance` / `UIManager.Instance` (singleton abuse) | inherited | Phase 3 (routing), Phase 10 (removal) |
| `GameStateController.Update()` polling `GameManager.CurrentState` every frame | inherited | Phase 4 |
| `SuspectClickHandler.Update()` lerping scale/rotation every frame | inherited | Phase 4 |
| `VideoPlayerUI.LateUpdate()` re-binding `RawImage.texture` every frame | inherited | Phase 4 |
| `Debug.Log` call sites in 6 files | inherited | Phase 1 |
| 49 `Q<…>("name")` magic strings across 5 UI files | inherited | Phase 2 |
| `LevelManager.GetSuspectVerdict(suspect)` returns `SuspectRole.Normal` for unjudged suspects (silent fallback) | inherited | Phase 10 |
| `GameManager.RecordVerdict` (thin wrapper, no callers) | inherited | Phase 10 |
| `GameManager.OnLevelComplete` (TODO, never called) | inherited | Phase 10 |
| `static Instance` on `GameManager` / `LevelManager` / `UIManager` | inherited | Phase 10 |
| `Resources.Load<StyleSheet>("UI/Common")` + `Resources.Load<VisualTreeAsset>("UI/…")` | inherited | Phase 9 |
| `using UnityEngine.UI;` in `VideoPlayerUI` | inherited | Phase 5 |
| `Assets/Resources/UI/` folder with UXML + USS at runtime-load path | inherited | Phase 9 |
| `Assets/UI/USS/Common.uss` (329-line stale duplicate of the 533-line runtime one) | inherited | Phase 9 |
| `Assets/UI/New Panel Settings.asset` (unused default) | inherited | Phase 9 |
| Scene UIDocuments reference a non-existent `PanelSettings` GUID `1429059c…` (silently no `rootVisualElement`) | inherited | Phase 9 |
| `VideoPlayerPanel` GameObject has no `UIDocument` component (Phase 5 UGUI → UI Toolkit migration was incomplete) | inherited | Phase 9 (acknowledged, not fixed) |

## Conventions

- **Phase numbering**: `Phase 0` is the foundation (asmdef + tests asmdef). `Phase 1` through `Phase 10` follow chronologically.
- **Branch naming**: `<type>/<short-desc>` where `<type>` is one of `chore` (tooling/build/cleanup), `refactor` (structural), `feature` (new behaviour). Conventional Commits message prefix matches the branch type.
- **Per-phase commit count**: 1–6 atomic commits. Each phase lands via one PR (except Phase 8, which was merged via FF into main locally rather than as a fresh PR — see Phase 8 §"Gotchas").
- **Per-commit discipline**: code change + tests in separate commits. Test commit is "could be cherry-picked onto main independently" safe.
- **One VM = one pair of files** (`.cs` in `Assets/Scripts/ViewModels/`, `.cs` in `Assets/Tests/EditMode/`) plus a refactor commit wiring the existing view to the VM.
- **No behaviour change** is the explicit goal of every refactor commit. The only behaviour changes in the entire refactor are: the LevelManager `GetSuspectVerdict` null contract (Phase 10 commit 3) and the `VerdictRecord` becoming immutable (Phase 10 commit 2; surface API is identical).

---

## Phase 0 — Foundation (asmdef + tests)

### Goal
Isolate the runtime code into its own assembly definition and stand up an EditMode test assembly with one smoke test.

### Why
With everything in `Assembly-CSharp`, every script recompiled together, test discovery was impossible (no test runner reference), and there was no enforcement boundary between editor-only MCP tools and runtime game code. Splitting out `CriminalCase2.Runtime` gives compile-time boundaries and makes future test work cheap.

### Pre-conditions
17 `.cs` files under `Assets/Scripts/`, all in default `Assembly-CSharp`. `Assets/Scripts/Editor/Tool_VideoPanelSetup.cs` used the MCP package's `McpPluginTool` attribute, which meant the file had to stay in `Assembly-CSharp-Editor` (the MCP package's scripts also compile there with no asmdef on either side).

### Changes
- `e5b66c7` `docs: rename AGENT.md to AGENTS.md` — pure rename, 100% similarity.
- `95561f6` `chore(runtime): move editor MCP tool out of Scripts/Editor` — `git mv` of `Tool_VideoPanelSetup.cs` (+ `.meta`) from `Assets/Scripts/Editor/` to `Assets/Editor/`. The Scripts/Editor folder becomes empty and is removed.
- `b43ff7e` `chore(runtime): add CriminalCase2.Runtime assembly definition` — new `Assets/Scripts/CriminalCase2.Runtime.asmdef` referencing `UnityEngine.UI`, `UnityEngine.UIElements`, `UnityEngine.Video`. `autoReferenced: true`. Default `Assembly-CSharp-Editor` (where MCP lives) is unchanged.
- `59a154a` `chore(tests): add EditMode test assembly with enum coverage tests` — new `Assets/Tests/EditMode/CriminalCase2.Tests.EditMode.asmdef` (references `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, `nunit.framework.dll`, `CriminalCase2.Runtime`; `defineConstraints: [UNITY_INCLUDE_TESTS]`, `overrideReferences: true`) + `EnumCoverageTests.cs` with 3 smoke tests against the three core enums (`SuspectRole`, `GameState`, `DrugTestResult`).

### Verification
- 3/3 EditMode tests pass.
- 0 compile errors in `Assets/Scripts/`.
- `using UnityEngine.UI;` still in `VideoPlayerUI`; asmdef references it (will be removed in Phase 5).
- `Assets/Scripts/Editor/` folder is now empty; Unity no longer creates its `.meta`. The MCP server (`com.ivanmurzak.unity.mcp 0.63.4`) compiled into `Assembly-CSharp-Editor` and the `Tool_VideoPanelSetup` MCP tool worked from there.

### Gotchas
- The `Assets/Editor/` folder already contained `CreateSuspectDataAssets.cs`; the new `Tool_VideoPanelSetup.cs` lives alongside it in `Assembly-CSharp-Editor`. Both editor tools, same default assembly, no friction.
- The MCP package's own scripts (no asmdef) compile into `Assembly-CSharp-Editor`. That means runtime asmdef code **cannot** reference MCP types, but editor-only MCP code works fine. The phase correctly keeps MCP-using code in `Assets/Editor/`.
- First commit attempt on this phase used `rm` + `git add` instead of `git mv`, which only staged the additions (not the deletes). Recovered via `git reset --hard HEAD~1` + redoing with `git mv`. Lesson: always use `git mv` for renames.

### Notable design decisions
- asmdef `autoReferenced: true` so default `Assembly-CSharp` (e.g. the new `Tests.EditMode` asmdef, which has `noEngineReferences: false`) can find runtime types without explicit reference. The `Tests.EditMode` asmdef *does* explicitly reference `CriminalCase2.Runtime` regardless.
- `overrideReferences: true` on the test asmdef means `nunit.framework.dll` is the only precompiled reference exposed to tests. Prevents accidental `using System.Net;` etc. in tests.

### Out-of-scope deltas
- `Tool_VideoPanelSetup.cs` produces `CS0618` (obsolete `[McpPluginToolType]` attribute). Pre-existing in the moved file. Deferred.
- The tests are intentionally weak (enum membership checks) so the first phase stays small.

---

## Phase 1 — Logger + nullable

### Goal
Replace raw `Debug.Log` with a project-wide logger facade and turn on C# nullable reference type annotations.

### Why
49 `Debug.Log` call sites across 6 files made it impossible to swap log sinks, route logs to file, or strip logs from release builds. Separately, the project shipped with nullable disabled, which let null-related bugs reach runtime instead of being caught at compile time.

### Pre-conditions
49 `Debug.Log` call sites in `VideoPlayerUI` (33), `LevelManager` (9), `GameManager` (10), `UIManager` (1), `SuspectClickHandler` (1), `LevelConfig` (1). Project had no `csc.rsp`. MCP package was at `0.63.4`.

### Changes
- `aa177c7` `chore(deps): upgrade com.ivanmurzak.unity.mcp 0.63.4 to 0.78.0` — auto-detected upgrade.
- `5c06fe0` `feat(utils): add IGameLogger interface, UnityLogger, and static facade` — new `Assets/Scripts/Utils/{IGameLogger.cs, UnityLogger.cs, GameLogger.cs}`. `IGameLogger` exposes `Info`/`Warn`/`Error`. `UnityLogger` wraps `Debug.Log` per level. `GameLogger` is the static facade with `SetBackend(IGameLogger)` and `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` reset to `UnityLogger` on play-mode start.
- `3bfb0dd` `chore(log): migrate Debug.Log call sites to GameLogger` — 49 mechanical replacements across 6 files. The 3 `Debug.Log` calls inside `UnityLogger.cs` are the only remaining ones (intentional delegation).
- `b1ad349` `chore(nullable): enable nullable reference type annotations project-wide` — new `Assets/csc.rsp` with `-nullable`.
- `076b3e7` `test(logger): add EditMode tests for GameLogger facade` — 3 tests: defaults to `UnityLogger`, custom backend accepted, `SetBackend(null)` falls back to `UnityLogger`.
- `affa037` `chore(meta): track csc.rsp.meta` — Unity auto-generated the `.meta`; committed it in a separate meta commit so the csc.rsp enable commit stays focused on the file.

### Verification
- 6/6 EditMode tests pass (3 enum + 3 logger).
- `Debug.Log` count under `Assets/Scripts/`: 3, all inside `UnityLogger.cs`. Verified via `grep`.
- CS warning count: 0 new (the nullable enable hasn't produced warnings yet because most files use `= null!` defensively or the nullable analysis doesn't trigger).

### Gotchas
- Initial survey said MCP was at 0.63.4 in both `manifest.json` and `packages-lock.json`. After re-fetching git diff, the actual upgrade was 0.78.0 with a new `UNITY_MCP_READY` scripting define. The "upgrade" commit was real, just out of sync with my own context.
- 3 separate meta commits would have been cleaner if I'd remembered to amend the `.meta` into the source commit before committing. Settled for one trailing meta commit.

### Notable design decisions
- `[RuntimeInitializeOnLoadMethod]` reset on the static facade prevents test-state leakage between play-mode sessions in the editor.
- `SetBackend` accepts any `IGameLogger`, including `null` (falls back to `UnityLogger`). This keeps the call site safe even if a test forgets to restore the backend.
- `csc.rsp` (not `Directory.Build.props`) because Unity 6 doesn't honour Directory.Build.props for Unity assembly compilation.

### Out-of-scope deltas
- `GameLogger.Info` / `Warn` / `Error` don't yet take context (caller info). Deferred.
- Log level configuration (verbosity threshold) is not implemented; the facade always routes to the backend. Deferred.

---

## Phase 2 — UIConstants

### Goal
Centralize UXML element names into a single `UIConstants` static class and remove all inline `Q<Button>("string")` magic strings.

### Why
UXML element names were duplicated between UXML `name="…"` attributes and C# `Q<Button>(…)` lookups. A rename of a UXML element had to be done in two places and there was no compiler check that they matched. `UIConstants` makes the constants the single source of truth.

### Pre-conditions
18 `Q<…>("…")` element lookups across 5 UI files (`TutorialUI` 2, `SuspectDetailUI` 9, `StatusHUD` 1, `ResultUI` 2, `CheckStatusUI` 4). 58 `name="…"` attributes across 6 UXML files. `UIManager.cs:188` had a `className: "panel"` USS-class lookup; out of scope (USS class names are a different concept).

### Changes
- `ff6be4f` `chore(ui): add UIConstants and replace hardcoded UXML element names` — new `Assets/Scripts/UI/UIConstants.cs` with 5 nested static classes (`Tutorial`, `SuspectDetail`, `StatusHud`, `Result`, `CheckStatus`) covering 18 element names. 18 inline string replacements across 5 UI files. 7 files changed, 69 insertions, 18 deletions.
- `084de12` `test(ui): add EditMode tests asserting UIConstants contract` — `UIConstantsTests.cs` with a reflection-based collector that walks `UIConstants` nested classes and a regex-based collector that walks UXML `name=` attributes. 2 tests: (1) every UIConstants constant has a matching UXML name, (2) all UIConstants constants are unique.

### Verification
- 8/8 EditMode tests pass (3 enum + 3 logger + 2 UIConstants).
- `Q<…>("string")` count under `Assets/Scripts/UI/`: 0.

### Gotchas
- A third test was originally added asserting "every UXML name has a matching UIConstants constant". It failed because UXML has many structural names (`title`, `subtitle`, `panel`, `container`, etc.) that aren't queried from C#. Dropped as too strict; the orphan-direction test (constant → UXML) is the correct invariant.
- `using UnityEngine;` was left over after dropping the third test; removed in the same commit.

### Notable design decisions
- Constants grouped per-panel (nested static classes) instead of one flat namespace. Makes `UIConstants.SuspectDetail.CloseButton` self-documenting at call sites.
- Two tests instead of one: orphan check + uniqueness. A single "match" test couldn't catch duplicates on either side.

### Out-of-scope deltas
- USS class names (`panel`, `check-status-entry`, etc.) are still magic strings. A future `UIConstants.USS` block could centralize them; deferred.
- `UIManager.cs:188` `className: "panel"` USS selector left in place; out of scope (USS side, not UXML).
- 40+ UXML `name="…"` attributes that aren't queried from C# have no `UIConstants` entry; not required by the contract.

---

## Phase 3 — Composition root + interfaces

### Goal
Introduce a typed composition root (`GameServices` static facade) backed by domain interfaces (`IGameStateProvider`, `ILevelController`, `IVerdictRecorder`, `IVideoService`, `IUIView`) so consumers depend on abstractions, not on concrete singleton classes.

### Why
Every consumer reached for `GameManager.Instance.Foo`, `LevelManager.Instance.Bar`, `UIManager.Instance.Baz`. Singletons made it impossible to swap implementations for tests, made ownership ambiguous (Awake order), and made the dependency graph invisible. Interfaces + a composition root replace each consumer's static coupling with a typed seam.

### Pre-conditions
61 `*.Instance.*` call sites across 10 files. `static Instance` on `GameManager` (line 15), `LevelManager` (line 13), `UIManager` (line 11). No DI container. No interfaces on the manager classes.

### Changes
- `a4e9c78` `feat(domain): add core service interfaces` — new `Assets/Scripts/Domain/` with 5 interfaces: `IGameStateProvider` (`GameState CurrentState` + `event Action<GameState> StateChanged`), `ILevelController` (full level-judging surface + `event Action<LevelConfig> LevelLoaded`), `IVerdictRecorder` (`IReadOnlyList<VerdictRecord> Records` + `event Action<VerdictRecord> VerdictRecorded`), `IVideoService` (intro video path/clip), `IUIView` (Show/Hide).
- `ced41c3` `feat(services): add GameServices static facade and GameBootstrap` — `Assets/Scripts/Services/GameServices.cs` with `GameState`/`Levels`/`Verdicts`/`Video`/`UI` typed accessors, `Register(object)` pattern matcher, `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` reset. `Assets/Scripts/Services/GameBootstrap.cs` creates a `[GameServicesRoot]` GameObject with `DontDestroyOnLoad`, attaches `GameManager` + `LevelManager`, registers both with `GameServices`.
- `c752b6c` `feat(managers): make GameManager and LevelManager implement interfaces and raise events` — `GameManager` implements `IGameStateProvider` + `IVerdictRecorder` + `IVideoService`, raises `StateChanged` and `VerdictRecorded` events. `LevelManager` implements `ILevelController`, raises `LevelLoaded`. `IVerdictRecorder.Record(SuspectData, SuspectRole)` added (so `Record` is the public mutator, not a separate `RecordVerdict` wrapper).
- `8cdf279` `refactor: route all consumers through GameServices instead of static Instance` — 56 `*.Instance.*` call sites across 10 files (`UIManager`, `VideoPlayerUI`, `LevelManager`, `CheckStatusUI`, `SuspectDetailUI`, `StatusHUD`, `ResultUI`, `TutorialUI`, `GameManager`, `SuspectClickHandler`, `GameStateController`) routed through `GameServices`. `static Instance` fields preserved for backwards compat.
- `95d57d3` `Merge pull request #1 from Adnanskuyy/refactor/dev` — PR #1, 16 commits (Phase 0 through 3).

### Verification
- 12/12 EditMode tests pass.
- `*.Instance.*` count under `Assets/Scripts/`: 0.
- `static Instance` count: 3 (preserved for backwards compat — removed in Phase 10).

### Gotchas
- `IGameStateProvider.AdvanceToNextLevel()` initially didn't match `GameManager.AdvanceToNextLevel(Action? onComplete = null)`. Fixed by making the interface method `void AdvanceToNextLevel(Action? onComplete = null)`.
- `VideoPlayerUI.cs` had `using CriminalCase2.Data;` removed by accident during a search-and-replace; restored.

### Notable design decisions
- `GameServices` is a static facade, not a DI container. Chosen because (a) the existing managers are MonoBehaviours, which DI containers handle awkwardly, (b) Unity 6.3 has no built-in DI, (c) `RuntimeInitializeOnLoadMethod` is the cleanest bootstrap point. Future: swap to a real container if scope demands.
- `Register(object)` pattern-matches by type; no string keys, no interface IDs. Type-safe at compile time.
- `IUIView` exists for future use; no consumer yet.

### Out-of-scope deltas
- UI side effects in `GameManager.SetState` (e.g. showing/hiding panels) are still in the manager. Should move to the consumer panels or to a `GameStateChangeRouter`. Deferred.
- `static Instance` fields preserved for backwards compat (planned removal in Phase 10).

---

## Phase 4 — Event-driven state, hover, and video binding

### Goal
Remove the three `Update()` / `LateUpdate()` polling antipatterns: `GameStateController` (state polling), `SuspectClickHandler` (hover lerp), `VideoPlayerUI` (texture rebind).

### Why
`Update()` runs every frame regardless of whether state changed. Three classes were paying that cost for state that changed at most a few times per scene. The polling was also a coupling smell: consumers re-reading source-of-truth values when the source could push.

### Pre-conditions
- `GameStateController.Update` polled `GameManager.CurrentState` against a cached value, dispatching on change.
- `SuspectClickHandler.Update` lerped `transform.localScale` and `transform.localRotation` toward hover/original each frame using `Vector3.Lerp` and `Quaternion.Lerp` with an exponential approach factor.
- `VideoPlayerUI.LateUpdate` diffed the current `RawImage.texture` against `VideoPlayer.texture` and re-assigned if different.

### Changes
- `956a07e` `chore(state): replace GameStateController Update polling with StateChanged subscription` — `OnEnable` subscribes to `IGameStateProvider.StateChanged`; `OnDisable` unsubscribes. The cached `CurrentState` field and the polling block are gone. New `GameStateControllerTests.cs` (4 tests) + `FakeGameStateProvider` test helper.
- `b725f55` `chore(input): replace SuspectClickHandler Update-lerp with event-driven tween` — `OnPointerEnter`/`OnPointerExit` start a `TweenToAsync` tween via `Awaitable`; the tween runs on Unity's Awaitable scheduler instead of `Update`. `CancellationTokenSource` held by the handler; disposed in `OnDestroy`. New serialized `_hoverTweenDuration` (default `0.2s`).
- `89d59ad` `chore(video): bind VideoPlayerUI texture in prepareCompleted instead of LateUpdate` — `SetupVideoPlayer` subscribes to `VideoPlayer.prepareCompleted`; `OnVideoPrepared` binds the texture once. Unsubscribed in `CleanupVideoPlayer`. The `_isPlaying` field (used only to gate the `LateUpdate` block) is removed.
- `a0a1354` `Merge pull request #2 from Adnanskuyy/chore/event-driven-state` — PR #2, 3 commits.

### Verification
- 12/12 EditMode tests pass.
- `Update()` / `LateUpdate()` count in `Assets/Scripts/`: 0 (all 3 polling blocks removed).
- 0 behaviour change: state transitions still happen at the same points, hover tween still animates over the same duration, video texture still binds before first frame.

### Gotchas
- `FakeGameStateProvider` in the new test file didn't initially implement `AdvanceToNextLevel(Action?)` — `CS0535` in the test file. Added an empty implementation.
- `EditMode` tests don't run MonoBehaviour lifecycle methods (`OnEnable`/`OnDisable`); the new `GameStateControllerTests` invoke them via reflection. Pattern: `[TearDown] { InvokeOnDisable(); }` + setup that calls `InvokeOnEnable()`.

### Notable design decisions
- `Awaitable` tween (not `Task` or `UniTask`): Unity 6 ships it natively, no extra dependency, integrates with the engine's frame loop.
- Hover tween uses `Linear` / `Slerp` over a duration rather than the previous exponential approach. Predictable, easier to tune.

### Out-of-scope deltas
- `GameManager.SetState` still has UI side effects (e.g. `_videoPlayerUI?.ShowPlayScreen()` on `Tutorial` state). Phase 3 §"Out-of-scope deltas" carries this forward.
- VideoPlayerPanel scene was still UGUI (no `UIDocument`). Phase 5 will fix.

---

## Phase 5 — VideoPlayerUI UGUI → UI Toolkit

### Goal
Migrate `VideoPlayerUI` from UGUI to UI Toolkit, the last UGUI dependency in the runtime asmdef.

### Why
AGENTS.md §"Tech Stack" requires UI Toolkit (UXML/USS) only. The Phase 1 asmdef reference to `UnityEngine.UI` existed only because of this one file.

### Pre-conditions
- `VideoPlayerUI` had 5 `[SerializeField] UnityEngine.UI.*` fields (`RawImage`, `Button`, `Image`, etc.), a `GameObject` play screen, and a `GameObject` video screen.
- `UIManager._videoPlayerPanel` was a `GameObject` toggled via `SetActive(true/false)`; the other 5 panels were `UIDocument` + `SetUIToolkitPanelActive`.
- `Assets/Resources/UI/VideoPanel.uxml` already existed in UI Toolkit (play-container, video-container, title-label, subtitle-label, play-button, skip-button). No `video-frame` element.
- `Assets/Resources/UI/Common.uss` had panel styles but no video-related styles. `Assets/UI/USS/Common.uss` was a 329-line stale duplicate.
- `Assets/Editor/Tool_VideoPanelSetup.cs` was the MCP tool that wired the UGUI `VideoPlayerPanel` scene GameObject; obsolete after this phase.

### Changes
- `a7e29e7` `feat(ui): add video-frame element and video player styles` — added `<ui:VisualElement name="video-frame">` to `VideoPanel.uxml`; appended 86 lines of video styles to `Common.uss` (play-container, video-container, video-frame, title/subtitle labels, play/skip buttons, skip-button-container).
- `1c5168d` `refactor(video): migrate VideoPlayerUI from UGUI to UI Toolkit` — dropped 5 UGUI `[SerializeField]` fields; added `UIDocument` + `VisualTreeAsset` + `StyleSheet` + `RenderTextureSize` (Vector2Int). Cached `_playContainer`, `_videoContainer`, `_videoFrame`, `_titleLabel`, `_subtitleLabel`, `_playButton`, `_skipButton` via `BindUI`. Video frame bound via `_videoFrame.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture))`. `_renderTexture` created lazily in `EnsureRenderTexture`. Click handlers bound via named methods + `BindUI`/`UnbindUI`. `OnDestroy` releases the `RenderTexture`.
- `d2868d7` `refactor(ui): route video panel through UIManager.SetUIToolkitPanelActive` — `UIManager._videoPlayerPanel: GameObject` (SetActive) replaced with `UIManager._videoPlayerDocument: UIDocument` (display toggling). `InitializePanels` loads `"UI/VideoPanel"` UXML. `AutoFindPanels` uses `vui.GetComponent<UIDocument>()`. `VideoPlayerUI.IsPlayScreenVisible` property added so `UIManager.ShowVideoPlayer` can early-out if already showing the play screen.
- `8480ca1` `chore(editor): remove Tool_VideoPanelSetup.cs` — 207 lines deleted. Also dropped the 2 pre-existing `CS0618` warnings about the obsolete `[McpPluginToolType]` attribute (since the file was the only user).
- `4b48e62` `chore(asmdef): drop UnityEngine.UI reference` — `Assets/Scripts/CriminalCase2.Runtime.asmdef` no longer lists `UnityEngine.UI`. Verified via `grep`: 0 `UnityEngine.UI[^E]` matches in `Assets/Scripts/` and `Assets/UI/`.
- `4cbfa4f` `test(video): add EditMode tests for VideoPlayerUI state transitions` — 3 tests (`OnPlayClicked_NoVideoPlayer_AdvancesToInvestigation`, `OnSkipClicked_AdvancesToInvestigation`, `OnPlayClicked_WithNullGameState_DoesNotThrow`); reuses `FakeGameStateProvider` from `GameStateControllerTests`. Reflection-invokes private `OnPlayClicked`/`OnSkipClicked`.
- `262f20f` `Merge pull request #3 from Adnanskuyy/feature/video-player-uitoolkit` — PR #3, 6 commits.

### Verification
- 15/15 EditMode tests pass (12 prior + 3 new).
- 0 UGUI references in `Assets/Scripts/` or `Assets/UI/`.
- 0 compile errors. The 7 cached UI element fields use `= null!` to suppress `CS8618` (same pattern as `GameManager`'s own serialized fields).

### Gotchas
- `Test` runner timed out on first call after the new test file landed; second call after `assets-refresh --force-update` worked. Same pattern as Phase 0.
- Scene wiring for `VideoPlayerPanel` (still UGUI from prior state) was NOT in scope; deferred. The scene will need a `UIDocument` component on the panel + `_videoPlayerDocument` wired, but that's a scene edit and a follow-up. Phase 9 surfaces the issue more clearly.

### Notable design decisions
- Video bound via `RenderTexture` + `Background.FromRenderTexture` (not a `VisualElement` texture child). This was the established pattern in the existing `VideoPlayerUI`'s `RenderTexture` plumbing; kept it.
- `EnsureRenderTexture` lazy-creates the texture on first `SetupVideoPlayer` call. Avoids needing a serialized `RenderTexture` field (Unity's inspector can't preview textures safely across play-mode sessions).

### Out-of-scope deltas
- `VideoPlayerPanel` scene GameObject still has no `UIDocument` component. PR #3 description explicitly notes this as pre-existing. The `UIManager._videoPlayerDocument` field is wired to `{fileID: 0}`.
- `RenderTextureSize` is a `Vector2Int` field; no validation that it matches the source video resolution. Future phase.

---

## Phase 6 — SuspectDetailViewModel

### Goal
Extract `SuspectDetailViewModel` so the `SuspectDetailUI` view becomes a thin UXML/USS binder.

### Why
`SuspectDetailUI` was 161 lines mixing VM concerns (state derivation, command routing, level reads) with view concerns (UXML lookups, button wiring, UI orchestration). Splitting the two lets the VM be unit-tested without a UIDocument and lets the view be re-themed without touching game logic.

### Pre-conditions
`SuspectDetailUI` (161 lines) read from `GameServices.Levels` for drug tests, recorded verdicts, and drug test results, and called `GameServices.UI.HideAllPanels` / `ShowStatusHUD` / `UpdateStatusHUD` directly on verdict. `VerdictRecorded` and `CloseRequested` were not separate events.

### Changes
- `a966cda` `feat(viewmodels): add SuspectDetailViewModel` — pure C# class in `Assets/Scripts/ViewModels/`. No `MonoBehaviour`. Deps: `SuspectData` + `ILevelController`. Read-only passthroughs (`SuspectName`, `Description`, `EvidenceText`). Mutable state (`DrugTestResultText`, `IsDrugTestButtonEnabled`). Commands (`RequestDrugTest`, `SelectVerdict`, `RequestClose`). Events (`StateChanged`, `VerdictRecorded`, `CloseRequested`). UI orchestration stays in the view.
- `867b7b8` `refactor(view): rewire SuspectDetailUI to use SuspectDetailViewModel` — `Populate` constructs the VM + subscribes; `OnDisable` disposes + unsubscribes; `GameServices.Levels` reads funnelled through the VM; verdict click handlers are now named methods (`OnVerdictUserClicked` etc.) so `UnbindUI` can fully unsubscribe. `UpdateVerdictButtons` removed; button labels set in `BindUI`. New `_suspectNameLabel`/`_descriptionLabel`/… cached element fields initialized with `= null!`.
- `8f42798` `test(viewmodels): add EditMode coverage for SuspectDetailViewModel` — 16 tests covering ctor null guards, initial state matrix, `RequestDrugTest` (disabled button / `UseDrugTest` false / on-success paths), `SelectVerdict` (records + raises `VerdictRecorded`, NOT `CloseRequested`), `RequestClose` (raises `CloseRequested`, no record), `Dispose` idempotent. `FakeLevelController` (internal sealed) test helper. `CreateSuspect` uses reflection on `SuspectData`'s private fields.
- `9b7e003` `Merge pull request #4 from Adnanskuyy/feature/viewmodel-suspect-detail` — PR #4, 3 commits.

### Verification
- 31/31 EditMode tests pass (15 prior + 16 new).
- 0 new `CS8618` warnings.
- 0 behaviour change: same button labels, same drug test gating, same close/verdict flow.

### Gotchas
- First test run was 30/31 — `Ctor_ExposesSuspectPassthroughProperties` expected `string.Empty` for `Description`/`EvidenceText` but got `null` because `SuspectData`'s private string fields default to `null`. Fixed by extending `CreateSuspect` helper to take `description` + `evidenceText` params and set them via reflection. The test now asserts on the actual values, not `string.Empty`.

### Notable design decisions
- `event Action? VerdictRecorded` vs `event Action? CloseRequested` — disambiguates "verdict + close" from "close only". The view's `OnViewModelVerdictRecorded` triggers HUD update; `OnViewModelCloseRequested` just hides the panel.
- VM exposes `IDisposable` even though there's no subscription to unsubscribe (the VM subscribes to `ILevelController.LevelLoaded` in ctor and unsubscribes in `Dispose`). Future-proofs against future event subscriptions.
- VM does NOT call `GameServices.UI.*` — the view's event handlers do. Keeps the VM dependency-free of UI composition.

### Out-of-scope deltas
- The drug test result text is computed from `ILevelController.HasDrugTestResult` / `GetDrugTestResult` (both return raw enum). Display name conversion via `ToDisplayName()` happens in the VM. Future: also expose the raw enum so views can do their own formatting.
- `VerdictRecord` is still a struct with mutable fields (Phase 10 changes it to a `record`).

---

## Phase 7 — StatusHUD + TutorialUI ViewModels

### Goal
Apply the same view-model extraction pattern to `StatusHUD` and `TutorialUI`, plus document the agent's GitHub access permission in `AGENTS.md`.

### Why
`StatusHUD` formatted button text from `ILevelController.JudgedCount`/`TotalSuspects`; same VM-vs-view separation rationale as Phase 6. `TutorialUI` was a pure command router with no state; included for pattern consistency + 4 cheap tests. The `AGENTS.md` Permissions section is the agent-access grant that makes all subsequent PRs legal.

### Pre-conditions
- `StatusHUD` (88 lines) bound `status-hud-button` click → `GameServices.UI.ShowCheckStatus()`. Read `ILevelController.JudgedCount` and `TotalSuspects` to format button text: `"Cek Status (0/N)"` / `"Cek Status (j/N)"` / `"Lihat Hasil (j/N)"` when all judged.
- `TutorialUI` (69 lines) bound `tutorial-close-button` → `GameServices.UI.HideAllPanels` + `GameServices.UI.ShowStatusHUD` + `GameServices.GameState.SetState(GameState.Investigation)`. Bound `tutorial-replay-video-button` → `GameServices.UI.ShowVideoPlayer()`. No state read.
- `AGENTS.md` (68 lines) had no Permissions section. AGENTS.md §7 "no agent push" rule was not actually written into the file; it lived only in earlier conversation context.

### Changes
- `d3b9bae` `chore(docs): add Permissions section noting agent GitHub access` — 9 lines added to `AGENTS.md` between Role and Technical Stack. Pushed to `origin/main` first (separate commit on main, before the feature branch was created).
- `af95c12` `feat(viewmodels): add StatusHUDViewModel` — pure C# VM. Dep: `ILevelController`. Props: `string ButtonText { get; }` (computed). Events: `StateChanged`, `OpenCheckStatusRequested`. Commands: `Refresh()`, `RequestOpenCheckStatus()`. Subscribes to `ILevelController.LevelLoaded` in ctor, unsubscribes in `Dispose`. Recomputes text in 4 states (no suspects / no judges / partial / all-judged).
- `b5b2be9` `refactor(view): rewire StatusHUD to use StatusHUDViewModel` — view owns VM lifecycle (create in `Initialize`/`OnEnable`, dispose in `OnDisable`), subscribes to events, routes button click through `vm.RequestOpenCheckStatus`. `OnViewModelStateChanged` applies `ButtonText` to button. `UpdateButtonText()` kept as public API for `UIManager.UpdateStatusHUD` compatibility (thin wrapper over `vm.Refresh`).
- `a28ae75` `test(viewmodels): add EditMode coverage for StatusHUDViewModel` — 13 tests in `StatusHUDViewModelTests.cs`: ctor null guard, 4 initial text formats (zero total / no judges / partial / all judged), `Refresh` recomputes + raises `StateChanged`, `Refresh` no-op when text unchanged, `LevelLoaded` updates text, `RequestOpenCheckStatus` raises event, `Dispose` unsubscribes, `Dispose` idempotent, `Request`/`Refresh` after `Dispose` throw `ObjectDisposedException`. Local `FakeLevelController` with controllable `JudgedCount`/`TotalSuspects` + `RaiseLevelLoaded` + `LevelLoadedSubscriberCount`.
- `78b7c49` `feat(viewmodels): add TutorialUIViewModel` — thin pure-C# command router. No deps, no state. Events: `CloseRequested`, `ReplayVideoRequested`. Commands: `RequestClose()`, `RequestReplayVideo()`. `IDisposable` nulls event handlers. `ThrowIfDisposed` pattern matches `StatusHUDViewModel`.
- `1224bf3` `refactor(view): rewire TutorialUI to use TutorialUIViewModel` — view owns VM lifecycle, subscribes to events, routes both button clicks through `vm.Request*` methods. UI orchestration (`HideAllPanels` + `ShowStatusHUD` + `SetState(Investigation)` on close, `ShowVideoPlayer` on replay) lives in named event handlers (`OnViewModelCloseRequested`, `OnViewModelReplayVideoRequested`). Click handlers also named (`OnCloseClicked`, `OnReplayVideoClicked`) so `UnbindUI` can fully unsubscribe.
- `2b7431b` `test(viewmodels): add EditMode coverage for TutorialUIViewModel` — 8 tests: `RequestClose` raises `CloseRequested`, `RequestReplayVideo` raises `ReplayVideoRequested`, cross-event isolation (`RequestClose` does not raise `ReplayVideoRequested` and vice versa), multiple subscriptions all receive event, `Dispose` idempotent, `Request*` after `Dispose` throw `ObjectDisposedException`.
- `33bc213` `Merge pull request #5 from Adnanskuyy/feature/viewmodel-hud-and-tutorial` — PR #5, 6 commits.

### Verification
- 52/52 EditMode tests pass (31 prior + 13 StatusHUD + 8 TutorialUI).
- 0 compile errors. 0 new `CS8618`/`CS8625` warnings.

### Gotchas
- First attempt at amending commit 1 to include the VM `.meta` accidentally amended commit 2 (refactor) instead, creating 1 mega-commit with 4 files. `git reset --hard HEAD~1` undid it; re-wrote `StatusHUD.cs` + test file + tutorial VM, regenerated `.meta` via `AssetDatabase.Refresh`, then committed in correct order. Lesson: `git commit --amend` always amends HEAD; verify with `git log --oneline -1` before amend.
- Test `Dispose_ClearsEventHandlers` failed: `ThrowIfDisposed` throws before the nulled event handlers are reached. Removed the test as over-specification (the "Request* after Dispose throws" tests cover the relevant behaviour).

### Notable design decisions
- `TutorialUIViewModel` is intentionally thin. Value is pattern consistency + 4 new tests. No behaviour change.
- View's `UpdateButtonText()` kept as public method so `UIManager.UpdateStatusHUD` doesn't need to be changed. Wrapper around `vm.Refresh()`.
- `ILevelController.LevelLoaded` subscription means the button text updates automatically when a new level is loaded, even if `Populate` hasn't been called.

### Out-of-scope deltas
- "Already seen tutorial" tracking (would prevent re-showing the tutorial on subsequent levels) is not in the VM. Future.
- `Refresh()` is the only mutating command; the rest are pure event raises. Naming consistency with the other VMs.

---

## Phase 8 — CheckStatus + Result ViewModels

### Goal
Extract `CheckStatusViewModel` and `ResultViewModel`, completing the per-panel view-model extraction set.

### Why
Same rationale as Phases 6 and 7. The two remaining panels both build per-record display lists; extracting the data shape into a VM record type lets the view focus on `VisualElement` construction.

### Pre-conditions
- `CheckStatusUI` (142 lines) cleared its container, added per-record `VisualElement` rows (name + player verdict display), toggled empty-state visibility, and gated the submit button text + enabled state on `ILevelController.AllSuspectsJudged`. Submit gated again on click.
- `ResultUI` (86 lines) cleared its results container, added per-record rows (1-based-indexed: name + player choice + correct answer + feedback). Next-level click advanced the game state.
- `CheckStatusUI` and `ResultUI.Populate(IReadOnlyList<VerdictRecord> records)` are the public API consumed by `UIManager` (which doesn't need to change).
- Unity 6.3's Mono runtime doesn't ship `System.Runtime.CompilerServices.IsExternalInit`, so `public sealed record Foo(...)` fails to compile with `CS0518`. (Affects Phase 10's `VerdictRecord → readonly record struct` plan; deferred.)

### Changes
- `538351f` `feat(viewmodels): add CheckStatusViewModel` — pure C# VM. Dep: `ILevelController`. Props: `IReadOnlyList<StatusEntry> Entries`, `bool IsEmpty`, `bool CanSubmit`, `string SubmitButtonText`. Events: `StateChanged`, `CloseRequested`, `SubmitRequested`. Commands: `SetRecords(IReadOnlyList<VerdictRecord>)`, `RequestClose()`, `RequestSubmit()`. `RequestSubmit` is no-op if `!CanSubmit`. Subscribes to `LevelLoaded` to refresh the submit button state. Nested `public sealed class StatusEntry` (not `record` — see Gotchas).
- `bd7c68a` `feat(viewmodels): add ResultViewModel` — pure C# VM. No deps, no state. Props: `IReadOnlyList<ResultEntry> Entries`. Events: `StateChanged`, `NextLevelRequested`. Commands: `SetRecords(...)`, `RequestNextLevel()`. Nested `public sealed class ResultEntry` (also not `record`).
- `70ac663` `refactor(view): rewire CheckStatusUI to use CheckStatusViewModel` — `Populate` now calls `_vm.SetRecords(records); Refresh();`. New `Refresh()` method applies VM state to UXML. View still constructs `VisualElement`/`Label` for each row, driven by VM record data. Click handlers named (`OnCloseClicked`, `OnCheckResultClicked`).
- `d8c9f5b` `refactor(view): rewire ResultUI to use ResultViewModel` — same pattern as `CheckStatusUI`.
- `033e65c` `test(viewmodels): add EditMode coverage for CheckStatusViewModel` — 17 tests covering ctor null guard, initial state, `SetRecords` (null throws, populates entries, raises `StateChanged`), `IsEmpty` true/false, `CanSubmit` true/false, `SubmitButtonText` 2 format branches, `RequestClose` raises, `RequestSubmit` raises only when `CanSubmit`, `RequestSubmit` no-op when not, `LevelLoaded` raises `StateChanged`, `Dispose` idempotent, `RequestClose`/`RequestSubmit` after `Dispose` throw.
- `575e308` `test(viewmodels): add EditMode coverage for ResultViewModel` — 7 tests: ctor empty entries, `SetRecords` null throws, `SetRecords` populates 1-based-indexed entries, `SetRecords` raises `StateChanged`, `RequestNextLevel` raises, `Dispose` idempotent, `RequestNextLevel` after `Dispose` throws.
- `759363c` `Merge feature/viewmodel-checkstatus-and-result into main` — local FF-merge of the Phase 8 feature branch into main, after the user merged PR #5 (Phase 7) into main but before PR #6 (Phase 8) was independently merged. Net result: main now contains Phases 7 and 8 (76/76 tests).

### Verification
- 76/76 EditMode tests pass (52 prior + 17 CheckStatus + 7 Result).
- 0 compile errors. 0 new `CS8618`/`CS8625` warnings.

### Gotchas
- Initial commit of `CheckStatusViewModel` used `public sealed record StatusEntry(string SuspectName, string PlayerVerdictDisplay);` — Unity 6.3's Mono runtime lacks `IsExternalInit`, so this failed to compile with `CS0518`. Converted to a plain sealed class with constructor-set properties.
- The same fix was needed for `ResultEntry`.
- `IsExternalInit` polyfill is added in Phase 10 commit 1 specifically to enable `record` types going forward.

### Notable design decisions
- `CheckStatusViewModel` and `ResultViewModel` use `public sealed class` (not `record`) for their nested entry types. Once Phase 10's `IsExternalInit` polyfill is in place, these can be migrated to `record` for value equality. Out of scope here.
- VM does NOT call `GameServices.UI.*`. View's event handlers do. Same pattern as `SuspectDetailViewModel`.
- `RequestSubmit` gates itself: the view's `OnViewModelSubmitRequested` can trust that the event was only raised if `CanSubmit` was true at the time of the call.

### Out-of-scope deltas
- `CheckStatusUI` previously had a `UpdateVerdictButtons` method (dead code). The Phase 8 refactor removed it. Originally planned for Phase 10 §1; finished early.
- `SuspectDetailUI`'s `UnbindUI` was already fixed in Phase 6 (named event handlers). Phase 10's planned item 3 is already done.

---

## Phase 9 — UI assets out of Resources

### Goal
Stop loading UXML + Common.uss via `Resources.Load` at runtime. Move the assets to `Assets/UI/UXML/` and `Assets/UI/USS/` and wire them through the scene.

### Why
`Resources.Load` blocks the asset bundle pipeline (every `Resources/` asset ships with the build unconditionally), defers errors from compile-time to runtime, and bypasses the standard Inspector workflow. Unity's idiomatic approach is `[SerializeField]` references on the consuming component.

### Pre-conditions
- `Assets/Resources/UI/`: 6 UXML files (VideoPanel, TutorialPanel, SuspectDetailPanel, CheckStatusPanel, ResultPanel, StatusHUD) + Common.uss (533 lines, canonical, runtime-loaded).
- `Assets/UI/UXML/`: empty folder (no files, just `.meta`).
- `Assets/UI/USS/Common.uss`: 329-line stale duplicate (missing video-frame + status-hud + check-status styles). Not the runtime one.
- `Assets/UI/New Panel Settings.asset`: unused default PanelSettings asset.
- `Assets/Scenes/SampleScene.unity`: not currently open in the editor; MCP `scene-open` rejects it (both `assetPath` and `instanceID` references).
- `UIManager.cs` lines 65, 99 used `Resources.Load`.
- All 6 UIDocument `m_PanelSettings` references pointed to a non-existent PanelSettings GUID `1429059c…` (the deleted `New Panel Settings.asset`). The scene had been silently non-functional.

### Changes
- `5a54000` `chore(assets): move UXML + Common.uss to Assets/UI/{UXML,USS} and delete duplicates` — `git mv` 6 UXML (+ 6 `.meta`) to `Assets/UI/UXML/`; `git mv` canonical 533-line `Common.uss` (+ `.meta`) to `Assets/UI/USS/`. Deleted the 329-line duplicate `Common.uss` + `.meta`; deleted `New Panel Settings.asset` + `.meta`; deleted empty `Resources/UI/` folder + `.meta`. 19 files changed, 317 insertions, 726 deletions.
- `86e4c4a` `refactor(ui): remove Resources.Load from UIManager` — removed `InitializeUIToolkitPanel(UIDocument, string, StyleSheet)` method, both `Resources.Load` calls, `using CriminalCase2.Utils;` (no longer references `GameLogger`). `InitializePanels` now only caches the UI MonoBehaviour components.
- `d865944` `chore(scene): wire PanelSettings and common StyleSheet in SampleScene` — created new `Assets/UI/PanelSettings.asset` (ScriptableObject) via `script-execute` (GUID `09113d50ef761d54cb14e04c2a1715ac`). Updated 6 UIDocument `m_PanelSettings` references in scene YAML from the broken `1429059c…` to the new GUID. Renamed UIManager field `_videoPlayerPanel:` → `_videoPlayerDocument:` (matches the Phase 5 code rename that was never reflected in the scene). Added `_commonStyle: {fileID: 11400000, guid: ce86dd4b0e66c6040a3535f7e927556e, type: 2}` to UIManager. UIManager's `InitializePanels` now pushes `_commonStyle` to each `UIDocument.rootVisualElement.styleSheets` idempotently. Also fixed `UIConstantsTests.UxmlFolder` path and `UIConstants` doc comment to point to the new location.
- `f066762` `Merge pull request #7 from Adnanskuyy/chore/ui-assets-out-of-resources` — PR #7, 3 commits.

### Verification
- 52/52 EditMode tests pass on the branch (current main state at the time; +24 Phase 8 tests would appear after PR #6 + this PR land together).
- 0 `Resources.Load` hits in `Assets/Scripts/`.
- 0 compile errors.

### Gotchas
- The deleted `New Panel Settings.asset` was the very PanelSettings the scene had been referencing. This was a pre-existing latent bug: the scene's UIDocuments had been failing to render for a long time.
- Direct scene YAML editing (no MCP scene-open support) was necessary. Backup `SampleScene.unity.bak` created before edit, deleted at the end.
- The rename `_videoPlayerPanel` → `_videoPlayerDocument` in scene YAML was needed because Phase 5 changed the C# field name but the scene was never updated. The new field is still `fileID: 0` since `VideoPlayerPanel` has no `UIDocument` component in the scene (Phase 5 deferred this; the panel is still UGUI).

### Notable design decisions
- **Option B** (VisualTreeAsset on each UIDocument in scene, common StyleSheet via UIManager `[SerializeField]`) over **Option A** (UIManager holds `[SerializeField] VisualTreeAsset` per panel). Option B is more idiomatic Unity; UIManager just orchestrates.
- `UIManager._commonStyle` is a runtime-pushed StyleSheet (added in `InitializePanels`), not a per-UIDocument styleSheets list. The scene wiring is one UIManager field, not 6 UIDocument fields.

### Out-of-scope deltas
- `VideoPlayerPanel` scene GameObject still has no `UIDocument` component. Pre-existing from Phase 5. Re-wiring requires a scene edit adding the `UIDocument` MonoBehaviour + wiring `_videoPlayerDocument`.
- `SampleScene.unity.bak` deleted before commit.
- PR #6 (Phase 8) was never merged into main as a fresh PR; Phase 8 was FF-merged into local main before PR #7 was opened. This is fine; the result on `origin/main` after PR #7 and PR #6 both land is identical.

---

## Phase 10 — Cleanup pass

### Goal
Remove remaining composition-root leftovers, convert `VerdictRecord` to an immutable record, tighten the `GetSuspectVerdict` contract to return `SuspectRole?`, move display-only extensions to the UI folder, and delete three pieces of dead code.

### Why
Phase 3 routed consumers through `GameServices` but kept `static Instance` fields for backwards compat. By Phase 10, no consumer was using them (verified via `grep`). The `SuspectRole?` change is a long-standing latent bug fix (the silent `Normal` fallback). The dead code (`RecordVerdict` wrapper, `OnLevelComplete` TODO, `OnValidate` Instance-referencing) accumulated over the original game's development and is no longer reachable.

### Pre-conditions
- `static Instance` on `GameManager` / `LevelManager` / `UIManager`. `*.Instance.*` consumer count in `Assets/Scripts/`: 0.
- `LevelManager.GetSuspectVerdict(SuspectData suspect)` returns `SuspectRole.Normal` for unjudged suspects. Interface signature: `SuspectRole GetSuspectVerdict(SuspectData suspect)`. No production callers (only 2 test fakes).
- `VerdictRecord` is a struct with mutable fields.
- `EnumExtensions` lives at `Assets/Scripts/Data/EnumExtensions.cs` (29 lines, 2 ext classes for `ToDisplayName`). Only used by UI views.
- `GameManager.RecordVerdict` (lines 77–80) is a thin wrapper around `Record(SuspectData, SuspectRole)`. No callers.
- `GameManager.OnLevelComplete` (lines 200–211) has a `TODO: Show game complete screen` and is never called.
- `LevelManager.OnValidate` was empty (only checked `Instance`).
- Unity 6.3's Mono runtime doesn't ship `System.Runtime.CompilerServices.IsExternalInit`, so `record` types fail with `CS0518`.

### Changes
- `6600c95` `feat(runtime): add IsExternalInit polyfill` — new `Assets/Scripts/Runtime/IsExternalInit.cs` with `#if !NET5_0_OR_GREATER` guard. `internal static class IsExternalInit { }` in `System.Runtime.CompilerServices`. `[EditorBrowsable(Never)]` so it doesn't pollute IntelliSense.
- `b15bc8d` `refactor(data): convert VerdictRecord to sealed record` — `VerdictRecord` is now `public sealed record` (positional primary ctor, immutability, value equality). Originally planned as `readonly record struct`, but Unity 6.3 C# 9 doesn't support `record struct`; ref type was the available alternative.
- `3bbc64a` `refactor(domain): change GetSuspectVerdict to return SuspectRole?` — interface: `SuspectRole? GetSuspectVerdict(SuspectData suspect)`. `LevelManager` impl returns `null` for unjudged suspects (no more silent `Normal` fallback). 3 test fakes updated (`StatusHUDViewModelTests`, `SuspectDetailViewModelTests`, new `LevelManagerTests`). 2 new `LevelManagerTests`: `GetSuspectVerdict_UnrecordedSuspect_ReturnsNull` + `GetSuspectVerdict_AfterRecordJudgedSuspect_ReturnsRecordedRole`.
- `d7fbc52` `chore(folder): move EnumExtensions from Data/ to UI/` — `git mv` of `Assets/Scripts/Data/EnumExtensions.cs` (+ `.meta`) to `Assets/Scripts/UI/`. Namespace unchanged (`CriminalCase2.Data`).
- `62aacb4` `refactor(managers): remove static Instance + Awake assignments` — `GameManager.Instance` / `LevelManager.Instance` / `UIManager.Instance` removed. The 3 managers' `Awake` no longer sets `Instance`. `GameManager.Awake` still calls `DontDestroyOnLoad` (composition root bootstrap). `UIManager.Awake` still calls `AutoFindPanels`. `LevelManager.Awake` is now empty. `LevelManager.OnValidate` patched to no-op since it referenced `Instance`.
- `2de7a83` `chore(cleanup): delete dead code` — `GameManager.RecordVerdict` wrapper (lines 77–80) deleted. `GameManager.OnLevelComplete` (lines 200–211) deleted. `LevelManager.OnValidate` deleted (was already no-op after the previous commit's patch, but kept around until now to land in the same cleanup commit).
- `20fc601` `Merge pull request #8 from Adnanskuyy/chore/cleanup-pass` — PR #8, 6 commits.

### Verification
- 78/78 EditMode tests pass (76 prior + 2 new `LevelManagerTests`).
- 0 compile errors. 0 new `CS8618`/`CS8625` warnings.
- 0 behaviour change (only type/shape cleanups + dead code removal).

### Gotchas
- `git commit --amend` after committing commit 1 (IsExternalInit polyfill) accidentally amended commit 6 (delete dead code) instead, because amend defaults to HEAD. `git reset --hard 9e8c0ba` undid it; `git commit --fixup=ed1424e` + `git rebase -i --autosquash ed1424e^` moved the meta into commit 1. Lesson: `git commit --amend` is HEAD-only; use `git rebase -i` with explicit commit targeting.
- `record struct` would have required `LangVersion=10` in `csc.rsp`, which Unity 6.3 may or may not honour. Settled for `record` (ref type). The immutable-positional syntax is the same; the only difference is heap allocation.

### Notable design decisions
- Direct removal of `static Instance` (no `[Obsolete]` deprecation cycle). Verified via `grep` that 0 consumers remain in `Assets/Scripts/`. The deprecation cycle would have added 2 commits for a benefit no one would see.
- `VerdictRecord` is `record` (ref type) instead of `record struct`. Acceptable since `VerdictRecord` instances are short-lived (created on verdict, stored in `IVerdictRecorder.Records`, never mutated). The heap allocation is a non-issue.
- `EnumExtensions` namespace unchanged. Only the file location moved. Avoids touching every `using CriminalCase2.Data;` in the UI files.

### Out-of-scope deltas
- `LevelManager.LoadLevel` second-arg semantics + `RecordVerdict` race; `GameManager.SetState` UI side effects. Still tracked in `AGENTS.md` "Known Legacy Items" section.
- `VerdictRecord` could be `readonly record struct` if Unity's C# version is bumped in the future. Not actionable now.
- The 3 test fakes (`StatusHUDViewModelTests.FakeLevelController`, `SuspectDetailViewModelTests.FakeLevelController`, `LevelManagerTests.FakeLevelController`) are independently defined per test file. A shared test infrastructure would be a future cleanup.

---

## Known Leftovers (carried from AGENTS.md "Known Legacy Items")

These items remain after the refactor and are explicitly out of scope:

1. **`VideoPlayerPanel` UGUI re-wiring** — scene GameObject has no `UIDocument` component. `UIManager._videoPlayerDocument` is wired to `fileID: 0`. The video player panel won't render at runtime via UI Toolkit. Pre-existing from Phase 5. Fix: scene edit adding a `UIDocument` MonoBehaviour + `VideoPlayerPanel.uxml` + `_videoPlayerDocument` wire.

2. **V2 branch (clue-matching, role-assignment, level additions)** — exists on `origin/V2`. User's responsibility per project agreement. Not touched by the refactor.

3. **`GameManager.SetState` UI side effects** — `_videoPlayerUI?.ShowPlayScreen()` on `Tutorial` state, etc. Belongs in the consumer panels or in a `GameStateChangeRouter`. Phase 3 §"Out-of-scope deltas" carries this forward.

4. **`LevelManager.LoadLevel` second-arg semantics + `RecordVerdict` race** — race condition between `RecordVerdict` and concurrent `LoadLevel` calls. Documented in AGENTS.md; not addressed.

5. **Test infrastructure consolidation** — 3 separate `FakeLevelController` definitions across test files. A shared test base class would reduce duplication. Future.

6. **`RenderTextureSize` validation in `VideoPlayerUI`** — `Vector2Int` field with no validation against source video resolution. Future.

7. **`IsExternalInit` polyfill is project-internal** — sits in the `CriminalCase2.Runtime` asmdef. If a future package dependency uses `record` types, this polyfill may shadow their `IsExternalInit` and cause subtle issues. Future: gate on assembly-level check.

## Final Metrics

| Metric | Before (477353b) | After (20fc601) |
|---|---|---|
| EditMode tests | 0 | 78 |
| Test files | 0 | 9 |
| asmdef files | 0 | 2 |
| `Resources.Load` calls in `Assets/Scripts/` | 3 | 0 |
| `static Instance` fields | 3 | 0 |
| `Update()` / `LateUpdate()` blocks | 3 | 0 |
| `using UnityEngine.UI;` in runtime code | 1 (VideoPlayerUI) | 0 |
| `Debug.Log` call sites under `Assets/Scripts/` | 49 | 3 (all in `UnityLogger.cs`) |
| ViewModel classes | 0 | 5 |
| Domain interfaces | 0 | 5 |
| `record` types | 0 | 1 (VerdictRecord) |
| Magic-string `Q<…>("…")` lookups in `Assets/Scripts/UI/` | 18 | 0 |
| Files under `Assets/Resources/UI/` | 8 | 0 |
| `static Instance` consumer count in `Assets/Scripts/` | 61 | 0 |
| `OnLevelComplete` (dead TODO) | present | deleted |
| `RecordVerdict` (thin wrapper) | present | deleted |
| `OnValidate` (Instance-referencing) | present | deleted |

## Appendix: Commit Map

```
54143b8  Initial commit
fcc2386  First progress
bca672e  Progress showing UI
d67fd3e  Okay now the UI is working again
0e5dc12  Add video feature
ed069cd  Video feature done, Suspect detail done, check status done
18955c9  Fix UI bug
fbe5422  Add level 02
fb59c5f  Polish UI
7653af9  Fix video player on webgl build
09c816c  Always show drug test result
92990b5  Polish suspect detail UI
a406f5e  Add CI/CD workflows to main branch
3f9b46f  Fix: Use full namespace path for BuildScript in CLI workflow
98d99d1  Fix: Kill orphaned MCP/Unity processes before build to prevent file lock errors
477353b  Revert: Remove all CI/CD workflow files
─ pre-refactor baseline ─
e5b66c7  docs: rename AGENT.md to AGENTS.md                                       (Phase 0)
95561f6  chore(runtime): move editor MCP tool out of Scripts/Editor             (Phase 0)
b43ff7e  chore(runtime): add CriminalCase2.Runtime assembly definition         (Phase 0)
59a154a  chore(tests): add EditMode test assembly with enum coverage tests      (Phase 0)
aa177c7  chore(deps): upgrade com.ivanmurzak.unity.mcp 0.63.4 to 0.78.0         (Phase 1)
5c06fe0  feat(utils): add IGameLogger interface, UnityLogger, and static facade (Phase 1)
3bfb0dd  chore(log): migrate Debug.Log call sites to GameLogger                 (Phase 1)
b1ad349  chore(nullable): enable nullable reference type annotations            (Phase 1)
076b3e7  test(logger): add EditMode tests for GameLogger facade                 (Phase 1)
affa037  chore(meta): track csc.rsp.meta                                        (Phase 1)
ff6be4f  chore(ui): add UIConstants and replace hardcoded UXML element names    (Phase 2)
084de12  test(ui): add EditMode tests asserting UIConstants contract            (Phase 2)
a4e9c78  feat(domain): add core service interfaces                              (Phase 3)
ced41c3  feat(services): add GameServices static facade and GameBootstrap       (Phase 3)
c752b6c  feat(managers): make GameManager and LevelManager implement interfaces (Phase 3)
8cdf279  refactor: route all consumers through GameServices instead of Instance (Phase 3)
95d57d3  Merge pull request #1 from Adnanskuyy/refactor/dev                     (Phase 3)
956a07e  chore(state): replace GameStateController Update polling with event    (Phase 4)
b725f55  chore(input): replace SuspectClickHandler Update-lerp with tween       (Phase 4)
89d59ad  chore(video): bind VideoPlayerUI texture in prepareCompleted           (Phase 4)
a0a1354  Merge pull request #2 from Adnanskuyy/chore/event-driven-state         (Phase 4)
a7e29e7  feat(ui): add video-frame element and video player styles              (Phase 5)
1c5168d  refactor(video): migrate VideoPlayerUI from UGUI to UI Toolkit         (Phase 5)
d2868d7  refactor(ui): route video panel through UIManager.SetUIToolkitPanelActive (Phase 5)
8480ca1  chore(editor): remove Tool_VideoPanelSetup.cs                           (Phase 5)
4b48e62  chore(asmdef): drop UnityEngine.UI reference                           (Phase 5)
4cbfa4f  test(video): add EditMode tests for VideoPlayerUI state transitions    (Phase 5)
262f20f  Merge pull request #3 from Adnanskuyy/feature/video-player-uitoolkit   (Phase 5)
a966cda  feat(viewmodels): add SuspectDetailViewModel                           (Phase 6)
867b7b8  refactor(view): rewire SuspectDetailUI to use SuspectDetailViewModel   (Phase 6)
8f42798  test(viewmodels): add EditMode coverage for SuspectDetailViewModel     (Phase 6)
9b7e003  Merge pull request #4 from Adnanskuyy/feature/viewmodel-suspect-detail (Phase 6)
d3b9bae  chore(docs): add Permissions section noting agent GitHub access       (Phase 7)
af95c12  feat(viewmodels): add StatusHUDViewModel                               (Phase 7)
b5b2be9  refactor(view): rewire StatusHUD to use StatusHUDViewModel             (Phase 7)
a28ae75  test(viewmodels): add EditMode coverage for StatusHUDViewModel         (Phase 7)
78b7c49  feat(viewmodels): add TutorialUIViewModel                              (Phase 7)
1224bf3  refactor(view): rewire TutorialUI to use TutorialUIViewModel           (Phase 7)
2b7431b  test(viewmodels): add EditMode coverage for TutorialUIViewModel        (Phase 7)
33bc213  Merge pull request #5 from Adnanskuyy/feature/viewmodel-hud-and-tutorial (Phase 7)
538351f  feat(viewmodels): add CheckStatusViewModel                             (Phase 8)
bd7c68a  feat(viewmodels): add ResultViewModel                                  (Phase 8)
70ac663  refactor(view): rewire CheckStatusUI to use CheckStatusViewModel       (Phase 8)
d8c9f5b  refactor(view): rewire ResultUI to use ResultViewModel                 (Phase 8)
033e65c  test(viewmodels): add EditMode coverage for CheckStatusViewModel       (Phase 8)
575e308  test(viewmodels): add EditMode coverage for ResultViewModel            (Phase 8)
759363c  Merge feature/viewmodel-checkstatus-and-result into main               (Phase 8)
5a54000  chore(assets): move UXML + Common.uss to Assets/UI/{UXML,USS}          (Phase 9)
86e4c4a  refactor(ui): remove Resources.Load from UIManager                     (Phase 9)
d865944  chore(scene): wire PanelSettings and common StyleSheet in SampleScene  (Phase 9)
f066762  Merge pull request #7 from Adnanskuyy/chore/ui-assets-out-of-resources (Phase 9)
6600c95  feat(runtime): add IsExternalInit polyfill                            (Phase 10)
b15bc8d  refactor(data): convert VerdictRecord to sealed record                 (Phase 10)
3bbc64a  refactor(domain): change GetSuspectVerdict to return SuspectRole?      (Phase 10)
d7fbc52  chore(folder): move EnumExtensions from Data/ to UI/                  (Phase 10)
62aacb4  refactor(managers): remove static Instance + Awake assignments         (Phase 10)
2de7a83  chore(cleanup): delete dead code                                       (Phase 10)
20fc601  Merge pull request #8 from Adnanskuyy/chore/cleanup-pass               (Phase 10)
```

## Test Count Timeline

| After phase | Tests | Delta |
|---|---|---|
| 0 | 3 | +3 (`EnumCoverageTests`) |
| 1 | 6 | +3 (`LoggerTests`) |
| 2 | 8 | +2 (`UIConstantsTests`) |
| 3 | 8 | 0 |
| 4 | 12 | +4 (`GameStateControllerTests`) |
| 5 | 15 | +3 (`VideoPlayerUITests`) |
| 6 | 31 | +16 (`SuspectDetailViewModelTests`) |
| 7 | 52 | +21 (`StatusHUDViewModelTests` × 13 + `TutorialUIViewModelTests` × 8) |
| 8 | 76 | +24 (`CheckStatusViewModelTests` × 17 + `ResultViewModelTests` × 7) |
| 9 | 76 | 0 |
| 10 | 78 | +2 (`LevelManagerTests`) |
