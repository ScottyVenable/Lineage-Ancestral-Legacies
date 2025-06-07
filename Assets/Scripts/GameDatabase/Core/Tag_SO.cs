using UnityEngine;

namespace Lineage.Core
{
    /// <summary>
    /// Simple ScriptableObject used for tagging GameData definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTag", menuName = "GameData/Core/Tag Definition")]
    public class Tag_SO : ScriptableObject
    {
        [Tooltip("The actual tag value. Should be unique.")]
        public string tagName;
    }
}
