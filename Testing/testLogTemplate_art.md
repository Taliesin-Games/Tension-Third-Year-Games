# Art Testing Guide - Overscoped

This document outlines the testing plan for 3D models, UVs, textures, and design aesthetics.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Model Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Is the model overwriting anything? | Model does not replace existing assets unless intended | ⏳ |
| Is the model true to reference? | Model matches provided reference images and proportions | ⏳ |
| Does the model fit the style guide? | Model aligns with the project's art style | ⏳ |
| Does the model have a reasonable poly count? | Model maintains an optimized poly count for performance | ⏳ |
| Is the model going to be animated? | If animated, topology supports clean deformation | ⏳ |
| Are there any back-facing polygons, N-gons, or bad geometry? | Model is clean and optimized with no geometry issues | ⏳ |
| Are the pivot points in the correct places | Bottom of mesh to place on floor in the scene | ⏳ |
---

## UV Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Are UVs well optimized in the space? | UVs efficiently use texture space without excessive empty areas | ⏳ |
| Are the UVs stretched? | No visible stretching or distortion in UV mapping | ⏳ |
| Do they need to be separate? | Islands are separated logically where needed | ⏳ |

---

## Texture Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Are the textures an appropriate size? | Textures maintain quality while being optimized for performance | ⏳ |
| Does it overwrite different maps? | Textures do not override other important texture maps | ⏳ |
| Did you turn off sRGB on the ARM map? | sRGB is disabled for the Ambient Occlusion, Roughness, and Metallic map | ⏳ |

---

## Design Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Does it fit the scene? | Model integrates seamlessly into the game environment | ⏳ |
| Does it look good in the scene? | Model contributes positively to visual aesthetics | ⏳ |
| Could it be better? | Model could be improved without compromising performance | ⏳ |
| Do colors match the style guide? | Colors are consistent with the project’s palette | ⏳ |

---

## Notes & Known Issues
- [Describe any notable issues, bugs, or areas requiring further testing]

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
