using UnityEngine;

namespace Lineage.Systems.Resource
{
    /// <summary>
    /// Represents a resource node in the world (e.g., berry bush, tree).
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        public string resourceId;
        public int quantity = 10;
        public float regrowthTime = 30f; // seconds

        private int currentQuantity;
        private float timer;

        private void Start()
        {
            currentQuantity = quantity;
        }

        private void Update()
        {
            if (currentQuantity <= 0)
            {
                timer += Time.deltaTime;
                if (timer >= regrowthTime)
                {
                    currentQuantity = quantity;
                    timer = 0;
                }
            }
        }

        public bool Harvest(int amount)
        {
            if (currentQuantity <= 0) return false;

            int harvested = Mathf.Min(amount, currentQuantity);
            currentQuantity -= harvested;
            return true;
        }
    }
}
