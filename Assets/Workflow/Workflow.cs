using UnityEngine;

namespace Workstation
{
    public class Workflow
    {
        public static bool MoveTransformsUnderParent(Transform newParent, params Transform[] transforms)
        {
            if (newParent == null) {
                Debug.LogErrorFormat("ParentTransform is null");
                return false;
            }
            else if (transforms == null || transforms.Length == 0) {
                Debug.LogWarningFormat("Transforms is null or empty");
                return false;
            }
            
            if (transforms.Length == 1) {
                Transform originalParent = transforms[0].parent;
                transforms[0].parent = newParent;
                if (originalParent)
                    newParent.parent = originalParent;
            }
            else {
                foreach (Transform t in transforms) {
                    t.parent = newParent;
                }
            }
            return true;
        }
    }
}
