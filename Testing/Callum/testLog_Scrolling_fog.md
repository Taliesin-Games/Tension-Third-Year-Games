# Art Testing Guide - Overscoped

This document outlines the testing plan for 3D fogs, UVs, textures, and design aesthetics.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ❎ - Not Applicable

---

## fog Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Is the fog overwriting anything? | fog does not replace existing assets unless intended | ✅ |
| Is the fog true to reference? | fog matches provided reference images and proportions | ❎ |
| Does the fog fit the style guide? | fog aligns with the project's art style | ✅ |
| Does the fog have a reasonable poly count? | fog maintains an optimized poly count for performance | ✅ |
| Are the pivot points in the correct places | Bottom of mesh to place on floor in the scene | ✅ |
---


## Texture Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Are the textures an appropriate size? | Textures maintain quality while being optimized for performance | ✅ |
| Does it overwrite different maps? | Textures do not override other important texture maps | ✅ |
| Did you turn off sRGB on the ARM map? | sRGB is disabled for the Ambient Occlusion, Roughness, and Metallic map | ❎ |

---

## Design Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Does it fit the scene? | Fog seamlessly into the game environment | ✅ |
| Does it look good in the scene? | Fog contributes positively to visual aesthetics | ✅ |
| Could it be better? | Fog could be improved without compromising performance | ⏳ |
| Do colors match the style guide? | Colors are consistent with the project’s palette | ✅ |

---

## Notes & Known Issues
- Fog is fully customisable, the look is entirely determined by how the user sets it up. 

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
