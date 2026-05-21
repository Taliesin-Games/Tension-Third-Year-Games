# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Boss dying | When a boss enemy dies, does the blackhole level transition prefab spawn? | The level transition portal does spawn | ✅ | 
| Black hole VFX | blackhole prefab spawns without any clear visual disconnect from the scene | the black hole appears correctly and renders without issues | ✅ | 


---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| GameManager | modifications to game manager now disable the wincondition being triggered by defeating all enemies | N/A | ✅ |


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
- To test this, go to level 1 and drop a boss enemy into the scene then kill it and see what happens

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
