# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Spell FireBall | Can I fire a fireball that damages enemies (press 1 in the demo scene) | I can fire a fireball that damages enemies | ✅ |
| Spell FireBall | Does the projectile destroy self after hitting an enemy of traveling for a its max lifetime? | Fireball projectile does destroy | ✅ | 
| Spell Chain lightning | Can I cast lightning that damages enemies (press 2 in the demo scene) | I can cast chain lightning and it damages enemies, chaining to nearby ones | ✅ | 
| Spell Chain lightning | Does the chain lightning bounce to the correct amount of targets? | Chainlightning bounces to a total of 5 targets in the demo scene | ✅ | 
| Spell Cone of Cold | Can I cast a cone of cold that damages enemies (press 3 in the demo scene) | I cast a cone of cold that damages enemies | ✅ | 

---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Object has zero coupling so no other features are impacted | See full game loop testing | N/A | ✅ |

---

## Full Game Loop Testing
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Game Start | Player enters game world correctly | No issues on load | ✅ |
| Core Mechanics | Main gameplay mechanics function as intended | No critical failures | ✅ |
| UI & Menus | All menus function correctly and transitions work smoothly | UI elements are visible and interactive | ⏳ |
| Win Condition | Game correctly identifies win state and triggers victory screen | Player receives proper feedback | ⏳ |
| Fail Condition | Game correctly identifies failure state and triggers game over | Player receives proper feedback | ⏳ |
| Performance | Frame rate remains stable throughout play session | No significant FPS drops | ✅ |

---

## Notes & Known Issues
- Might be coupled technically? not sure, doesnt modify any other class, however slight tweaks were made to some to add casting input or ensure mana tracking was functional.

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
