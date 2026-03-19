# Test Log - Unreal Engine Project

This document tracks the testing of core features in the Unreal Engine project.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Feature Implemented in This Pull Request
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| PlayerUI | Is the player HUD visible during gameplay | HUD is visible on startup of game | ✅ | 
| PlayerUI | does the HUD correctly update when the inventory is opened | the right side of the HUD moves over towards the left when the inventory is opened | ✅ |
| StatPanel | does the character stats on the UI show the correct values? | the stat panel on the left side of the HUD shows stats that match those found on the player component and character stats component of the elf | ✅ |
| StatPanel | does changing the mode correctly update the visible stats (find it in PlayerUI > LeftSideHUD > StatsPanel (statpanelUIController)) | None hides the stats, Basic shows STR, AGI, INT And crit stats, Advanced shows the same as basic but also shows Damage bonus percentages | ✅ |
| DPSPanel | does changing the mode correctly update the visible stats (find it in PlayerUI > RightSideHUD > StatsPanel (DPSpanelUIController)) | None hides the values, Basic shows a single value, Advanced shows the single value but also per element damage | ✅ |
| SpellPanel | Does the spellpanel ui (bottom right , square icons) show the spells the player has? | the spell panel shows all the spells the player has on their casting component | ✅ |
| ManaBar and HealthBar | do the bars update when their respective values are changed? | Mana bar drops when spells are cast and gradually rises back to full, health drops when taking damage (currently however the player doesnt take damage from any sources) | ✅ |

---

## Impacted Features
| Feature | Test Case | Expected Result | Status |
|---------|----------|----------------|--------|
| InventoryUI | Does the new hud affect the inventoryUI and prevent it from working in any way? | nothing is impacted on the inventory UI, everything still works | ✅ |

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
| Physics | Does teh wall block impact | Wall blocks the vehicle | ✖ |

---

## Notes & Known Issues
- DPS tracker is hard to test, even for me. It seems to be correct but will need to further test to make sure its accurate. currently its close enough.
- Effects panel is currently unused and has been disabled. 

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
