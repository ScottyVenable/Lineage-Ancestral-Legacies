using UnityEngine;

namespace Lineage.Core.Items
{
    /// <summary>
    /// Definition for an item.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemDef", menuName = "GameData/Items/Item Definition")]
    public class ItemDefinitionSO : Lineage.Core.GameDataSO
    {
        public int maxStackSize = 1;
        public Sprite itemIcon;
    }
}
