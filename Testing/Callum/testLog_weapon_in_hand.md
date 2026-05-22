# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Weapon in hand | does a weapon item's world mesh prefab appear in the characters hand when equipped | The weapon appears in hand | ✅ |
| weapon in hand | does the weapon get removed when the weapon is unequipped | the weapon disappears from the hand | ✅ |
| weapon in hand | does the weapon get swapped correctly when different weapons are swapped in the hand slot | the weapon prefab is correctly swapped (see notes) | ✅ |

---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Character | Does the character still operate as normal with no issues? | The character operates as normal | ✅ |

---

## Full Game Loop Testing
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Game Start | Player enters game world correctly | No issues on load | ✅ |
| Core Mechanics | Main gameplay mechanics function as intended | No critical failures | ✅ |
| UI & Menus | All menus function correctly and transitions work smoothly | UI elements are visible and interactive | ✅ |
| Win Condition | Game correctly identifies win state and triggers victory screen | Player receives proper feedback | ✅ |
| Fail Condition | Game correctly identifies failure state and triggers game over | Player receives proper feedback | ✅ |
| Performance | Frame rate remains stable throughout play session | No significant FPS drops | ✅ |

---

## Notes & Known Issues
- No issues to note when used correctly.  Some weapons do not have a prefab set up or have weapon prefabs that do not use weapon components. WEAPON COMPONENT MUST BE ADDED TO THE WEAPON PREFAB BEFORE IT CAN BE USED 
## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
