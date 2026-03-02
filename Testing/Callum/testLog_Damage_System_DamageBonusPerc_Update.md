# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Damage System | Can I deal damage to targets | Damage Can be applied to health using calculations from damage system. | ✅ |
| Damage Calculations complete | Does the resulting final damage match the manually calculated damage | Damage matches manually calculated damage | ✅ | 
---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Health Component | Does the health component correctly apply resistances to incoming damage? | incoming damage is reduced (positive resistance) or increased (negative resistance) | ✅ |
| Player Stats | Are the player stats correctly used in the damage calculation, do the numbers match when stats change? |  Resulting damage numbers are correct when stats are adjusted | ✅ | 
---

## Full Game Loop Testing
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Game Start | Player enters game world correctly | No issues on load | ✅ |
| Core Mechanics | Main gameplay mechanics function as intended | No critical failures | ✅ |
| UI & Menus | All menus function correctly and transitions work smoothly | UI elements are visible and interactive | ⏳ |
| Win Condition | Game correctly identifies win state and triggers victory screen | Player receives proper feedback | ⏳ |
| Fail Condition | Game correctly identifies failure state and triggers game over | Player receives proper feedback | ⏳ |
| Performance | Frame rate remains stable throughout play session | No significant FPS drops | ⏳ |

---

## Notes & Known Issues
- 

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
