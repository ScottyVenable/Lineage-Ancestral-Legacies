using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing world data including locations and chunks.
    /// </summary>
    public static class WorldRepository
    {
        #region Databases
        public static List<Location> AllLocations { get; private set; } = new List<Location>();
        public static List<Chunk> AllChunks { get; private set; } = new List<Chunk>();
        #endregion

        #region Location Methods
        /// <summary>
        /// Gets a location by its ID.
        /// </summary>
        /// <param name="locationID">The ID of the location to retrieve.</param>
        /// <returns>The location with the specified ID, or null if not found.</returns>
        public static Location GetLocationByID(int locationID)
        {
            return AllLocations.FirstOrDefault(l => l.ID == locationID);
        }

        /// <summary>
        /// Gets a location by its name.
        /// </summary>
        /// <param name="locationName">The name of the location to retrieve.</param>
        /// <returns>The location with the specified name, or null if not found.</returns>
        public static Location GetLocationByName(string locationName)
        {
            return AllLocations.FirstOrDefault(l => l.Name == locationName);
        }

        /// <summary>
        /// Gets locations that have settlements.
        /// </summary>
        /// <returns>A list of locations with settlements.</returns>
        public static List<Location> GetLocationsWithSettlements()
        {
            return AllLocations.Where(l => l.hasSettlement).ToList();
        }

        /// <summary>
        /// Gets locations by proximity to a given position.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="maxDistance">The maximum distance to search within.</param>
        /// <returns>A list of locations within the specified distance.</returns>
        public static List<Location> GetLocationsByProximity(Vector3 position, float maxDistance)
        {
            return AllLocations.Where(l => Vector3.Distance(l.Position, position) <= maxDistance).ToList();
        }
        #endregion

        #region Chunk Methods
        /// <summary>
        /// Gets a chunk by its ID.
        /// </summary>
        /// <param name="chunkID">The ID of the chunk to retrieve.</param>
        /// <returns>The chunk with the specified ID, or null if not found.</returns>
        public static Chunk GetChunkByID(int chunkID)
        {
            return AllChunks.FirstOrDefault(c => c.ID == chunkID);
        }

        /// <summary>
        /// Gets chunks that contain a specific location.
        /// </summary>
        /// <param name="location">The location to search for.</param>
        /// <returns>A list of chunks containing the specified location.</returns>
        public static List<Chunk> GetChunksContainingLocation(Location location)
        {
            return AllChunks.Where(c => c.Locations.Contains(location)).ToList();
        }

        /// <summary>
        /// Gets all locations across all chunks.
        /// </summary>
        /// <returns>A list of all locations in all chunks.</returns>
        public static List<Location> GetAllChunkLocations()
        {
            var allChunkLocations = new List<Location>();
            foreach (var chunk in AllChunks)
            {
                allChunkLocations.AddRange(chunk.Locations);
            }
            return allChunkLocations;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes all world-related databases with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            InitializeLocations();
            InitializeChunks();

            Log.Info("WorldRepository: Database initialized successfully.", Log.LogCategory.Systems);
        }

        private static void InitializeLocations()
        {
            AllLocations.Clear();
            // TODO: Add default location data initialization
        }

        private static void InitializeChunks()
        {
            AllChunks.Clear();
            // TODO: Add default chunk data initialization
        }
        #endregion
    }
}
