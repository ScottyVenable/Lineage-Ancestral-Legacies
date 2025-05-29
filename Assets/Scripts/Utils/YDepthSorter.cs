using UnityEngine;

namespace Lineage.Ancestral.Legacies.Utils
{
    /// <summary>
    /// Adjusts the SpriteRenderer's sorting order based on the GameObject's Y position.
    /// Attach this to any 2D object to ensure proper depth sorting.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class YDepthSorter : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        [Tooltip("Multiplier for Y position to sorting order calculation.")]
        public int sortingMultiplier = 100;

        [Tooltip("Optional base offset added to the computed sorting order.")]
        public int baseSortingOrder = 0;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            // Calculate sorting order so lower Y positions appear in front
            int order = baseSortingOrder - Mathf.RoundToInt(transform.position.y * sortingMultiplier);
            spriteRenderer.sortingOrder = order;
        }
    }
}
