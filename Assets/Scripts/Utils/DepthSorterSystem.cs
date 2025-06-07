using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Utils
{
    /// <summary>
    /// Global depth sorter that updates sortingOrder of all SpriteRenderers in the scene based on their Y position.
    /// Attach this once in the scene (e.g., to a GameManager object).
    /// </summary>
    public class DepthSorterSystem : MonoBehaviour
    {
        [Tooltip("Multiplier for Y position to sorting order calculation.")]
        public int sortingMultiplier = 100;

        [Tooltip("Optional base offset added to the computed sorting order.")]
        public int baseSortingOrder = 0;

        private List<SpriteRenderer> renderers;

        void Awake()
        {
            // Cache all SpriteRenderers in the scene
            renderers = new List<SpriteRenderer>(FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None));
        }

        void LateUpdate()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var sr = renderers[i];
                if (sr == null) continue;
                int order = baseSortingOrder - Mathf.RoundToInt(sr.transform.position.y * sortingMultiplier);
                sr.sortingOrder = order;
            }
        }
    }
}
