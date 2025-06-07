using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Core
{
    /// <summary>
    /// Base class for all ScriptableObject game data definitions.
    /// Provides a unique identifier, display name, description and tag list.
    /// </summary>
    public abstract class GameDataSO : ScriptableObject
    {
        [Tooltip("Unique identifier for this game data entry. E.g., ITEM_FLINT, ENTITY_POP_GEN1.")]
        public string uniqueID;

        [Tooltip("Display name for UI purposes.")]
        public string displayName;

        [TextArea]
        public string description;

        public List<Tag_SO> tags = new List<Tag_SO>();
    }
}
