using UnityEngine;

namespace Lineage.Systems.TraitSystem
{
    /// <summary>
    /// ScriptableObject representing a genetic or social trait.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrait", menuName = "Lineage/Trait/TraitSO")]
    public class TraitSO : ScriptableObject
    {
        [Header("Trait Identification")]
        public string traitId;
        public string displayName;
        [TextArea]
        public string description;

        [Header("Trait Effects")]
        public float effectValue;
        // Additional effect parameters can be added here
    }

    // TODO: Get data from Database to populate traits.
}
