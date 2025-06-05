using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Genetics System

    /// <summary>
    /// Represents genetic information for hereditary traits.
    /// </summary>
    public struct Genetics
    {
        public GeneType geneType;
        public float dominantValue;
        public float recessiveValue;
        public bool isDominant;
    }

    #endregion
}
