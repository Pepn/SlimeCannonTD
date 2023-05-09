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
        /// Deletes all child gameobjects of Transform t.
        /// </summary>
        /// <param name="t">The transform.</param>
        public static void DestroyChildren(this Transform t)
        {
            for (int i = 0; i < t.childCount; ++i)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(t.GetChild(i).gameObject);
                }
                else
                {
                    Object.DestroyImmediate(t.GetChild(i).gameObject);
                }
            }
        }
    }
}
