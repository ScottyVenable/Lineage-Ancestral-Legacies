using UnityEngine;

namespace Lineage.Core.Entities
{
    /// <summary>
    /// Definition for an entity such as a Pop, Animal, or Structure.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEntityDef", menuName = "GameData/Entities/Entity Definition")]
    public class EntityDefinitionSO : Lineage.Core.GameDataSO
    {
        public GameObject prefab;
        // Additional fields like base stats can be added here later
    }
}
