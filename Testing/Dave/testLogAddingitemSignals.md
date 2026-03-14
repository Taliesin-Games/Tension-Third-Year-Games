# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Create a new item effect | Right click asset browser and create, inventory, item effect, pickup. | Creates item effect asset |  ✅


---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Character | Do characters work without changed behaviour | Player and enemies function normally | ✅ |
| CharacterWeapon | Try to attack enemy with sword | Enemy is damged | ✅ |
| Health | Try to attack enemy with sword | Enemy is damged | ✅ |

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
| Physics | Does the wall block impact | Walls block player | ✅ |

---

## Notes & Known Issues
- 
## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
