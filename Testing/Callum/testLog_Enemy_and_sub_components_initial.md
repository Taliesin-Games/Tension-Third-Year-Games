# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Enemy component | Does the enemy component correctly add all additional needed components when placed on an object? | Enemy component adds all required components and is functional automatically | ✅ |
| Enemy AI Component | Does the AI component correctly flow through its state machine? | Enemy Ai component patrols around and then when a target enters its "vision" it persues and "attacks" them | ✅ |
| Enemy Navigation Component | Does the Navigation component correctly provide pathing data and allow the enemy to operate its statemachine | Navigation component correctly returns the necessary pathing information to the AI component when requested and enables movement on the Enemy | ✅ |
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
- As always with these things, there is a broad spectrum of things that could go wrong. Currently under the testing i conducted it operated as expected, but in actual game conditions it may show issues.

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
