using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing genetic data.
    /// </summary>
    public static class GeneticsRepository
    {
        #region Database
        public static List<Genetics> AllGenetics { get; private set; } = new List<Genetics>();
        #endregion

        #region Genetics Methods
        /// <summary>
        /// Gets genetics by their type.
        /// </summary>
        /// <param name="geneType">The type of genes to retrieve.</param>
        /// <returns>A list of genetics of the specified type.</returns>
        public static List<Genetics> GetGeneticsByType(GeneType geneType)
        {
            return AllGenetics.Where(g => g.geneType == geneType).ToList();
        }

        /// <summary>
        /// Gets dominant genetics.
        /// </summary>
        /// <returns>A list of dominant genetics.</returns>
        public static List<Genetics> GetDominantGenetics()
        {
            return AllGenetics.Where(g => g.isDominant).ToList();
        }

        /// <summary>
        /// Gets recessive genetics.
        /// </summary>
        /// <returns>A list of recessive genetics.</returns>
        public static List<Genetics> GetRecessiveGenetics()
        {
            return AllGenetics.Where(g => !g.isDominant).ToList();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the genetics database with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            AllGenetics.Clear();
            // TODO: Add default genetics data initialization

            Log.Info("GeneticsRepository: Database initialized successfully.", Log.LogCategory.Systems);
        }
        #endregion
    }
}
