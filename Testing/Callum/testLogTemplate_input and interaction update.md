# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Interaction Prompt | Does the interaction prompt appear when near the level change pedestal in the safehub level? | a prompt image appears above the pedestal when in interaction rangs | ✅ |
| Object interaction | Can you interact with the pedestal in the safehub when next to it, regardless of the direction the character is facing (interaction button is E) | object interaction works as expected but now doesnt require you to face the object | ✅ |

| input update | does Tab still open the inventory? | Tab does infact still open and close the inventory | ✅ |
| input update | does Q drop and item that is picked up from the inventory? | Q drops a selected item from the inventory | ✅ |


---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Interaction | See interaction prompt and object interaction in Features implemented | N/A | ✅ |

---

## Full Game Loop Testing
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Game Start | Player enters game world correctly | No issues on load | ✅ |
| Core Mechanics | Main gameplay mechanics function as intended | No critical failures | ✅ |
| UI & Menus | All menus function correctly and transitions work smoothly | UI elements are visible and interactive | ✅ |
| Win Condition | Game correctly identifies win state and triggers victory screen | Player receives proper feedback | ✖ |
| Fail Condition | Game correctly identifies failure state and triggers game over | Player receives proper feedback | ✖ |
| Performance | Frame rate remains stable throughout play session | No significant FPS drops | ⏳ |

---

## Notes & Known Issues
- [Describe any notable issues, bugs, or areas requiring further testing]

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
