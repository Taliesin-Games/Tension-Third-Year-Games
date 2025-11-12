# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Game Manager - Kill player | Can I lose the game | Forcing player death transitions to LoseScreen |  ✅
| Game Manager - Kill enemy | Can I win the game | Forcing enemy death transitions to Win Screen |  ✅
| Options - volume | Can I alter the music or master volume | Open options menu, change volume, music volume changes |  ✅


---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Input system | Nothing changed, simple unity version update | Player character responds to inputs | ✅ |
| Music | Changed to use new music files as placeholder | Music plays in any scene with a game manager or music manager | ✅ |
| Enemy | Added an on destroy to manually remove from list | Not tested, waiting on enemy spawner improvements | ⏳ |
| Enemy Spawner | Added new interfaces for local variables and game state | Spawning note tested, waiting on future changes | ⏳ |
| Enemy Spawner | Added new interfaces for local variables and game state | New interfaces read ok and game ends | ✅ |
| Resource | Access level promotion, no real change | Health class operates as expected, health reduced | ✅ |

---

## Full Game Loop Testing
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Game Start | Player enters game world correctly | No issues on load | ✅ |
| Core Mechanics | Main gameplay mechanics function as intended | No critical failures | ✅ |
| UI & Menus | All menus function correctly and transitions work smoothly | UI elements are visible and interactive | ⏳ |
| Win Condition | Game correctly identifies win state and triggers victory screen | Player receives proper feedback | ✅ |
| Fail Condition | Game correctly identifies failure state and triggers game over | Player receives proper feedback | ✅ |
| Performance | Frame rate remains stable throughout play session | No significant FPS drops | ✅ |
| Physics | Does the wall block impact | Walls block player | ✖ |

---

## Notes & Known Issues
- Not fully tested enemy spawner due to pending planned changes

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
