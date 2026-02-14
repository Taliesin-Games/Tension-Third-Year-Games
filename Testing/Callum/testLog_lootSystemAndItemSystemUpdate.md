# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Item System GUI | Does the item system gui open | the item system gui opens a window that displays a series of tabs related to the item system | ✅ |
| Item System GUI | Can i set the database in the GUI and view its content? | Once a database is set and i open the database tab i can view the item contents of the database | ✅ |
| Item System GUI | Can i remove an item from the database? | clicking the delete buttons in the gui on an item in the database removes the item from the database and the assets folder | ✅ |
| Item System GUI | Can i add/create an item to the database? | Using the item creation tabs i can add an item of any kind to the set database | ✅ |
| Item System GUI | Can i edit an item in the database? | Using the edit button on an item in the database tab i can modify the parameters of an item and then save them to the database | ✅ |
| Item System GUI | Can i remove a tag? | clicking the delete buttons in the gui on a tag removes the tag assets folder | ✅ |
| Item System GUI | Can i add/create a tag? | Using the Tags tabs i can add a tag | ✅ |
| Item System GUI | Can i edit a Tag? | Using the edit button on a tag in the tag tab i can modify the parameters of a tag then save them | ✅ |
| Item System GUI | Can i remove a Loot table from the database in the loot table tab? | clicking the delete buttons in the gui on a loot table in the database removes the loot table from the database and the assets folder | ✅ |
| Item System GUI | Can i add/create a loot to the database? | Using the loot table tab i can add a loot table to the set database | ✅ |
| Item System GUI | Can i edit a loot table in the database? | Using the edit button on a loot table in the loot table tab i can modify the parameters of a loot table and then save them to the database | ✅ |

| Loot Generation | Does loot drop from an enemie's loot table when they are killed? | a dropped item appears in the scene when an enemy dies (they dont necessarily all have loot) | ✅ |
| Dropped item VFX | Does the dropped item vfx appear on the dropped item | the item vfx appear on a dropped item, its colour matching that of its rarity (colours of rarities can be found on the droppedItem prefab and all items currently in the game are set to cosmic rarity) | ✅ |

---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Enemy modified slightly to drop all items on death, beyond that there is no other coupling | See full game loop testing | N/A | ✅ |

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
- [Describe any notable issues, bugs, or areas requiring further testing]

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
