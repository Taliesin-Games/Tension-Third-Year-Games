# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Inventory & UI | Can i open the inventory UI by pressing tab while the game is running? | Inventory UI appears when tab is pressed while game is running | ✅ | 
| Inventory & UI | Does the inventory size match the one set in the Inventory component found on the InventoryDemoPlayer object in the inpsector? | Displayed inventory matches the size defined on the component | ✅ | 
| Inventory & UI | Do the correct starting items get added to the inventory when the game is started? (found on Inventory component found on the InventoryDemoPlayer object in the inpsector) | Displayed items match the starting items in the inspector | ✅ | 
| Inventory & UI | Can items be moved around in the inventory by dragging and dropping? | Yes items can be moved around in the inventory by dragging and dropping them into different slots | ✅ | 
| Inventory & UI | do items correctly stack when matching are placed into the same inventory slot? | Items stack correctly, stacking up to 9999 of the demo item | ✅ | 
| Inventory & UI | Can i drop and item held by the mouse by pressing V ? | Item is dropped into the world correctly and appears as a 3d model (or at least in the scene tree) | ✅ |
| Inventory & UI | Can the dropped item be picked up by walking into it? | item appears back in the inventory when collided with in the world, and the world representation is deleted | ✅ |  
| Inventory & UI | Do the stats of the player change correctly when an item is removed/placed into the equipment slots? (stats of the player found on the player stats component of the player, item stats found in the item object in the project > scripts > Inventory > items > equippable > weapon/armour folder)|  | ✅ | 
| Inventory & UI | Can only the correct equipment be placed into the correct slots? | Equipment can only be placed into inventory slots or their original slots in the equipment inventory, normal items cant be placed into equipment slots | ✅ | 



---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Object has zero coupling so no other features are impacted | SEE NOTES!!!!!!! | N/A | ✅ |

---

## Full Game Loop Testing
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| Game Start | Player enters game world correctly | No issues on load | ✅ |
| Core Mechanics | Main gameplay mechanics function as intended | No critical failures | ✅ |
| UI & Menus | All menus function correctly and transitions work smoothly | UI elements are visible and interactive | ⏳ |
| Win Condition | Game correctly identifies win state and triggers victory screen | Player receives proper feedback | ⏳ |
| Fail Condition | Game correctly identifies failure state and triggers game over | Player receives proper feedback | ⏳ |
| Performance | Frame rate remains stable throughout play session | No significant FPS drops | ⚠️ |

---

## Notes & Known Issues
- Please contact callum if any issues arise during testing, ill attempt to clear things up as best i can. Please also refer to the relevant document regarding the inventory system found in the "game systems" folder of the documentation folder. 

I appreciate this is a big big big PR and theres a lot to go over, so i expect something to fail somewhere.


Something weird with the mouseUI seemingly causing big frame drops when you move the camera quickly. Not sure if it is the mouseUi or something else but only noticed it after mouseUI implemented.

Not sure if technically coupled or not? doesnt modify any classes that were already in use, recreated Dave's character and player classes though on his request.

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
