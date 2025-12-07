# Art Testing Guide - Overscoped

This document outlines the testing plan for 3D models, UVs, textures, and design aesthetics.

## Status Key
✅ - Passed | ⚠️ - Issues Found | ❌ - Failed | ⏳ - Not Tested | ✖ - Not Applicable

---

## Rig Testing
| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Does the rig have the correct number of joints? | Bespoke Rig created | ✅ |
| Are the joints named correctly? | Joints named to standardised convention *position*_*name*_*jnt* | ✅ |
| Are the joints rotations zeroed correctly? | All joints rotation attributes are zeroed | ✅ |
| Are the joints oriented correctly? | Joints orerinted to world as not a humanoid skeleton | ✅ |
| Did the mesh skin correctly? | One skin clusters created using Geodesic Voxel bind.| ✅ |
| Does the mesh deform in the right places? | There are some errors but not going to be too much of an issue at the blockout stage. | ⚠️  |
| Does the rig import into engine? | Imports into Engine, and rig joints deform as expected | ✅ |

---

---

## Notes & Known Issues
- [Describe any notable issues, bugs, or areas requiring further testing]

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
