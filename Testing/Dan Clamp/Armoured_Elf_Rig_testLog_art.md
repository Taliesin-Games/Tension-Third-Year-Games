# Art Testing Guide - Overscoped

This document outlines the testing plan for 3D models, UVs, textures, and design aesthetics.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Rig Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Does the rig have the correct number of joints? | Standard humanoid shape, built to mirror Mixamo rig | ✅ |
| Are the joints named correctly? | Joints named to standardised convention *position*_*name*_*jnt* | ✅ |
| Are the joints rotations zeroed correctly? | All joints rotation attributes are zeroed | ✅ |
| Are the joints oriented correctly? | Joints follow X down chain, Z forwards and Y horizontally. | ✅ |
| Did the mesh skin correctly? | Two skin clusters created due to two meshes. Head and Body - Added third skin cluster for armour using deformation | ✅ |
| Does the mesh deform in the right places? | The armour is clustered using wrap deformer - for clean results armour will have to be separated into individual components.| ⚠️ |
| Does the rig import into engine? | Rig Imports into engine without issue | ✅ |

---

---

## Notes & Known Issues
- [Describe any notable issues, bugs, or areas requiring further testing]

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]