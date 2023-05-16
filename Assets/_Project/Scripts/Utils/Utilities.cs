using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PT
{
    /// <summary>
    /// Pure static class with helper functions.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Deletes all child gameobjects of Transform t, works both for in ExecuteMode and during runtime.
        /// </summary>
        /// <param name="t">The transform.</param>
        public static void DestroyChildren(this Transform t)
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < t.childCount; ++i)
                {
                    Object.Destroy(t.GetChild(i).gameObject);
                }
            }
            else
            {
                for (int i = t.childCount; i > 0; --i)
                {
                    Object.DestroyImmediate(t.GetChild(0).gameObject);
                }
            }
        }
    }
}
