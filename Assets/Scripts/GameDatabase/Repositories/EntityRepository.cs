using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing entity-related data including entities, settlements, NPCs, and populations.
    /// </summary>
    public static class EntityRepository
    {
        #region Databases
        public static List<Entity> AllEntities { get; private set; } = new List<Entity>();
        public static List<Settlement> AllSettlements { get; private set; } = new List<Settlement>();
        public static List<NPC> AllNPCs { get; private set; } = new List<NPC>();
        public static List<Population> AllPopulations { get; private set; } = new List<Population>();
        #endregion

        #region Entity Methods
        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="entityID">The ID of the entity to retrieve.</param>
        /// <returns>The entity with the specified ID, or default if not found.</returns>
        public static Entity GetEntityByID(int entityID)
        {
            return AllEntities.FirstOrDefault(e => e.entityID == entityID);
        }

        /// <summary>
        /// Gets entities by their type.
        /// </summary>
        /// <param name="entityType">The type of entities to retrieve.</param>
        /// <returns>A list of entities of the specified type.</returns>
        public static List<Entity> GetEntitiesByType(Entity.EntityType entityType)
        {
            return AllEntities.Where(e => e.entityType.Contains(entityType)).ToList();
        }

        /// <summary>
        /// Gets entities by their rarity.
        /// </summary>
        /// <param name="rarity">The rarity level to filter by.</param>
        /// <returns>A list of entities with the specified rarity.</returns>
        public static List<Entity> GetEntitiesByRarity(Rarity rarity)
        {
            return AllEntities.Where(e => e.rarity == rarity).ToList();
        }

        /// <summary>
        /// Gets entities by their faction.
        /// </summary>
        /// <param name="faction">The faction to filter by.</param>
        /// <returns>A list of entities from the specified faction.</returns>
        public static List<Entity> GetEntitiesByFaction(string faction)
        {
            return AllEntities.Where(e => e.entityFaction == faction).ToList();
        }
        #endregion

        #region Settlement Methods
        /// <summary>
        /// Gets a settlement by its ID.
        /// </summary>
        /// <param name="settlementID">The ID of the settlement to retrieve.</param>
        /// <returns>The settlement with the specified ID, or default if not found.</returns>
        public static Settlement GetSettlementByID(int settlementID)
        {
            return AllSettlements.FirstOrDefault(s => s.settlementID == settlementID);
        }

        /// <summary>
        /// Gets settlements controlled by the player.
        /// </summary>
        /// <returns>A list of player-controlled settlements.</returns>
        public static List<Settlement> GetPlayerControlledSettlements()
        {
            return AllSettlements.Where(s => s.isPlayerControlled).ToList();
        }
        #endregion

        #region NPC Methods
        /// <summary>
        /// Gets an NPC by name.
        /// </summary>
        /// <param name="npcName">The name of the NPC to retrieve.</param>
        /// <returns>The NPC with the specified name, or default if not found.</returns>
        public static NPC GetNPCByName(string npcName)
        {
            return AllNPCs.FirstOrDefault(n => n.npcName == npcName);
        }

        /// <summary>
        /// Gets NPCs by their archetype.
        /// </summary>
        /// <param name="archetype">The archetype to filter by.</param>
        /// <returns>A list of NPCs with the specified archetype.</returns>
        public static List<NPC> GetNPCsByArchetype(Archetype archetype)
        {
            return AllNPCs.Where(n => n.archetype == archetype).ToList();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes all entity-related databases with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            InitializeEntities();
            InitializeSettlements();
            InitializeNPCs();
            InitializePopulations();

            Log.Info("EntityRepository: Database initialized successfully.", Log.LogCategory.Systems);
        }

        private static void InitializeEntities()
        {
            // TODO: Add default entity data initialization
            AllEntities.Clear();
            
            // Example entity initialization
            var defaultEntity = new Entity
            {
                entityName = "Default Entity",
                entityID = 0,
                entityFaction = "Neutral",
                entityDescription = "A basic entity for testing purposes.",
                rarity = Rarity.Common,
                spawnChance = 100f,
                level = 1,
                entitySize = EntitySize.Medium,
                aggressionType = Entity.AggressionType.Neutral,
                health = new Health(100f),
                isAlive = true,
                tags = new List<string> { "Default" }
            };
            
            AllEntities.Add(defaultEntity);
        }

        private static void InitializeSettlements()
        {
            // TODO: Add default settlement data initialization
            AllSettlements.Clear();
        }

        private static void InitializeNPCs()
        {
            // TODO: Add default NPC data initialization
            AllNPCs.Clear();
        }

        private static void InitializePopulations()
        {
            // TODO: Add default population data initialization
            AllPopulations.Clear();
        }
        #endregion
    }
}
