# Fix Common.uss Warnings Plan

## 3 Issues to Fix

### 1. Replace `-unity-font-weight: bold` → `-unity-font-style: bold` (17 occurrences)
Unity uses `-unity-font-style` not `-unity-font-weight`. Do a global replace-all.

**Lines affected:** 59, 77, 99, 161, 172, 232, 262, 288, 303, 334, 375, 396, 433, 443, 474, 493, 514

**Action:** In `Assets/Resources/UI/Common.uss`, replace all instances of `-unity-font-weight: bold` with `-unity-font-style: bold`

### 2. Remove `pointer-events` from CSS (2 occurrences)
**Lines:** 389 (`pointer-events: none;` on `.hud-panel`), 400 (`pointer-events: auto;` on `.status-hud-button`)

**Action:** Delete these 2 lines from Common.uss, then update StatusHUD.uxml to add `picking-mode="Ignore"` on the hud-panel VisualElement.

In `Assets/Resources/UI/StatusHUD.uxml`, change:
```xml
<ui:VisualElement name="status-hud-panel" class="hud-panel">
```
to:
```xml
<ui:VisualElement name="status-hud-panel" class="hud-panel" picking-mode="Ignore">
```

The button inside will still receive clicks because it has `picking-mode="Position` (default).

### 3. `gap` — Keep as-is (11 warnings, but works at runtime)
Unity 6 uses Yoga engine which supports `gap`. The warnings are from the USS parser but runtime works correctly. No changes needed.

## Summary
- **File 1:** `Assets/Resources/UI/Common.uss` — 17 replacements of `-unity-font-weight: bold` → `-unity-font-style: bold`; remove 2 lines with `pointer-events`
- **File 2:** `Assets/Resources/UI/StatusHUD.uxml` — add `picking-mode="Ignore"` to `.hud-panel` VisualElement
- **Result:** Warnings reduced from 29 → 11 (only `gap` warnings remain, which are runtime-safe)