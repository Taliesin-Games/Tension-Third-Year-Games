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
| Did the mesh skin correctly? | Two skin clusters created due to two meshes. Head and Body | ✅ |
| Does the mesh deform in the right places? | There are some errors but unsure why - to be discussed with Damo.| ⚠️  |
| Does the rig import into engine? | Not tested | ⏳ |

---

---

## Notes & Known Issues
- [Describe any notable issues, bugs, or areas requiring further testing]

## How to Update
- When a test is **completed**, replace ⏳ with ✅ (Pass), ⚠️ (Issues Found), or ❌ (Fail).
- Leave notes on failed tests or necessary fixes in the Git commit messages.

---

**Commit Reference:** [Insert commit hash here]
