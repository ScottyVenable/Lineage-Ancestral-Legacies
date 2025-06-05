using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Static repository for managing and accessing entity definitions and operations.
    /// Provides CRUD operations, entity templates, and utility methods for entity management.
    /// </summary>
    public static class EntityRepository
    {
        #region Private Fields

        private static Dictionary<int, Entity> _entityTemplates;
        private static Dictionary<string, int> _entityNameLookup;
        private static Dictionary<EntityType, List<int>> _entitiesByType;
        private static Dictionary<string, List<int>> _entitiesByFaction;
        private static int _nextEntityID = 1;
        private static bool _isInitialized = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a read-only collection of all entity templates.
        /// </summary>
        public static IReadOnlyDictionary<int, Entity> AllEntities => _entityTemplates;

        /// <summary>
        /// Gets the total number of entity templates.
        /// </summary>
        public static int EntityCount => _entityTemplates?.Count ?? 0;

        /// <summary>
        /// Gets whether the repository has been initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the entity repository with default templates.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Lineage.Ancestral.Legacies.Debug.Log("EntityRepository already initialized.");
                return;
            }

            _entityTemplates = new Dictionary<int, Entity>();
            _entityNameLookup = new Dictionary<string, int>();
            _entitiesByType = new Dictionary<EntityType, List<int>>();
            _entitiesByFaction = new Dictionary<string, List<int>>();

            // Initialize entity type collections
            foreach (EntityType entityType in Enum.GetValues<EntityType>())
            {
                _entitiesByType[entityType] = new List<int>();
            }

            CreateDefaultEntityTemplates();
            
            _isInitialized = true;
            Lineage.Ancestral.Legacies.Debug.Log($"EntityRepository initialized with {EntityCount} entity templates.");
        }

        /// <summary>
        /// Creates default entity templates for the game.
        /// </summary>
        private static void CreateDefaultEntityTemplates()
        {
            // Create default Pop entities
            CreatePopTemplates();
            
            // Create default Animal entities
            CreateAnimalTemplates();
            
            // Create default Monster entities
            CreateMonsterTemplates();
            
            // Create default NPC entities
            CreateNPCTemplates();
        }

        #endregion

        #region Template Creation Methods

        /// <summary>
        /// Creates default Pop entity templates.
        /// </summary>
        private static void CreatePopTemplates()
        {
            var pop = CreateEntityTemplate("Pop", EntityType.PlayerControlled, "Neutral");
            pop.entityDescription = "A basic population unit capable of crafting and socializing.";
            pop.health = new Health(80f);
            pop.stamina = Stat.CreateStamina(100f);
            pop.intelligence = Stat.CreateStat(StatID.Intelligence, "Intelligence", 12f);
            pop.charisma = Stat.CreateStat(StatID.Charisma, "Charisma", 10f);
            RegisterEntityTemplate(pop);
        }

        /// <summary>
        /// Creates default Animal entity templates.
        /// </summary>
        private static void CreateAnimalTemplates()
        {
            // Wolf template
            var wolf = CreateEntityTemplate("Wolf", EntityType.Animal, "Wild");
            wolf.entityDescription = "A fierce pack hunter with territorial instincts.";
            wolf.health = new Health(120f);
            wolf.attack = Stat.CreateStat(StatID.Attack, "Attack", 15f);
            wolf.speed = Stat.CreateStat(StatID.Speed, "Speed", 8f);
            wolf.aggressionType = AggressionType.Aggressive;
            wolf.entitySize = EntitySize.Medium;
            RegisterEntityTemplate(wolf);

            // Bear template
            var bear = CreateEntityTemplate("Bear", EntityType.Animal, "Wild");
            bear.entityDescription = "A powerful solitary creature defending its territory.";
            bear.health = new Health(200f);
            bear.attack = Stat.CreateStat(StatID.Attack, "Attack", 20f);
            bear.defense = Stat.CreateStat(StatID.Defense, "Defense", 15f);
            bear.speed = Stat.CreateStat(StatID.Speed, "Speed", 4f);
            bear.aggressionType = AggressionType.Neutral;
            bear.entitySize = EntitySize.Large;
            RegisterEntityTemplate(bear);

            // Deer template
            var deer = CreateEntityTemplate("Deer", EntityType.Animal, "Wild");
            deer.entityDescription = "A peaceful herbivore that flees from danger.";
            deer.health = new Health(60f);
            deer.speed = Stat.CreateStat(StatID.Speed, "Speed", 12f);
            deer.agility = Stat.CreateStat(StatID.Agility, "Agility", 15f);
            deer.aggressionType = AggressionType.Passive;
            deer.entitySize = EntitySize.Medium;
            RegisterEntityTemplate(deer);
        }

        /// <summary>
        /// Creates default Monster entity templates.
        /// </summary>
        private static void CreateMonsterTemplates()
        {
            // Bandit template
            var bandit = CreateEntityTemplate("Bandit", EntityType.Monster, "Hostile");
            bandit.entityDescription = "A dangerous humanoid raider seeking loot and causing trouble.";
            bandit.health = new Health(100f);
            bandit.attack = Stat.CreateStat(StatID.Attack, "Attack", 18f);
            bandit.defense = Stat.CreateStat(StatID.Defense, "Defense", 12f);
            bandit.aggressionType = AggressionType.Aggressive;
            bandit.intelligence = Stat.CreateStat(StatID.Intelligence, "Intelligence", 8f);
            RegisterEntityTemplate(bandit);

            // Goblin template
            var goblin = CreateEntityTemplate("Goblin", EntityType.Monster, "Hostile");
            goblin.entityDescription = "A small but cunning creature that attacks in groups.";
            goblin.health = new Health(40f);
            goblin.attack = Stat.CreateStat(StatID.Attack, "Attack", 8f);
            goblin.speed = Stat.CreateStat(StatID.Speed, "Speed", 10f);
            goblin.aggressionType = AggressionType.Aggressive;
            goblin.entitySize = EntitySize.Small;
            RegisterEntityTemplate(goblin);
        }

        /// <summary>
        /// Creates default NPC entity templates.
        /// </summary>
        private static void CreateNPCTemplates()
        {
            // Merchant template
            var merchant = CreateEntityTemplate("Merchant", EntityType.NPC, "Trader");
            merchant.entityDescription = "A traveling trader offering goods and services.";
            merchant.health = new Health(90f);
            merchant.charisma = Stat.CreateStat(StatID.Charisma, "Charisma", 18f);
            merchant.intelligence = Stat.CreateStat(StatID.Intelligence, "Intelligence", 15f);
            merchant.aggressionType = AggressionType.Passive;
            RegisterEntityTemplate(merchant);

            // Guard template
            var guard = CreateEntityTemplate("Guard", EntityType.NPC, "Settlement");
            guard.entityDescription = "A protective warrior defending settlements and trade routes.";
            guard.health = new Health(150f);
            guard.attack = Stat.CreateStat(StatID.Attack, "Attack", 16f);
            guard.defense = Stat.CreateStat(StatID.Defense, "Defense", 18f);
            guard.aggressionType = AggressionType.Neutral;
            RegisterEntityTemplate(guard);
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new entity template with basic properties.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="entityType">The type classification of the entity.</param>
        /// <param name="faction">The faction the entity belongs to.</param>
        /// <returns>A new entity template.</returns>
        public static Entity CreateEntityTemplate(string name, EntityType entityType, string faction = "Neutral")
        {
            var entity = new Entity(name, _nextEntityID++);
            entity.entityType = new List<EntityType> { entityType };
            entity.entityFaction = faction;
            entity.rarity = Rarity.Common;
            entity.spawnChance = 100f;
            entity.level = 1;
            
            return entity;
        }

        /// <summary>
        /// Registers an entity template in the repository.
        /// </summary>
        /// <param name="entity">The entity template to register.</param>
        /// <returns>True if registration was successful.</returns>
        public static bool RegisterEntityTemplate(Entity entity)
        {
            if (_entityTemplates.ContainsKey(entity.entityID))
            {
                Lineage.Ancestral.Legacies.Debug.Log($"Entity ID {entity.entityID} already exists. Update instead of register.");
                return UpdateEntityTemplate(entity);
            }

            _entityTemplates[entity.entityID] = entity;
            _entityNameLookup[entity.entityName.ToLower()] = entity.entityID;

            // Add to type and faction collections
            foreach (var type in entity.entityType)
            {
                if (_entitiesByType.ContainsKey(type))
                {
                    _entitiesByType[type].Add(entity.entityID);
                }
            }

            if (!string.IsNullOrEmpty(entity.entityFaction))
            {
                if (!_entitiesByFaction.ContainsKey(entity.entityFaction))
                {
                    _entitiesByFaction[entity.entityFaction] = new List<int>();
                }
                _entitiesByFaction[entity.entityFaction].Add(entity.entityID);
            }

            Lineage.Ancestral.Legacies.Debug.Log($"Registered entity template: {entity.entityName} (ID: {entity.entityID})");
            return true;
        }

        /// <summary>
        /// Updates an existing entity template.
        /// </summary>
        /// <param name="entity">The updated entity template.</param>
        /// <returns>True if update was successful.</returns>
        public static bool UpdateEntityTemplate(Entity entity)
        {
            if (!_entityTemplates.ContainsKey(entity.entityID))
            {
                Lineage.Ancestral.Legacies.Debug.Log($"Entity ID {entity.entityID} not found for update.");
                return false;
            }

            var oldEntity = _entityTemplates[entity.entityID];
            _entityTemplates[entity.entityID] = entity;

            // Update name lookup if name changed
            if (oldEntity.entityName != entity.entityName)
            {
                _entityNameLookup.Remove(oldEntity.entityName.ToLower());
                _entityNameLookup[entity.entityName.ToLower()] = entity.entityID;
            }

            Lineage.Ancestral.Legacies.Debug.Log($"Updated entity template: {entity.entityName} (ID: {entity.entityID})");
            return true;
        }

        /// <summary>
        /// Removes an entity template from the repository.
        /// </summary>
        /// <param name="entityID">The ID of the entity to remove.</param>
        /// <returns>True if removal was successful.</returns>
        public static bool RemoveEntityTemplate(int entityID)
        {
            if (!_entityTemplates.TryGetValue(entityID, out var entity))
            {
                Lineage.Ancestral.Legacies.Debug.Log($"Entity ID {entityID} not found for removal.");
                return false;
            }

            _entityTemplates.Remove(entityID);
            _entityNameLookup.Remove(entity.entityName.ToLower());

            // Remove from type and faction collections
            foreach (var type in entity.entityType)
            {
                if (_entitiesByType.ContainsKey(type))
                {
                    _entitiesByType[type].Remove(entityID);
                }
            }

            if (!string.IsNullOrEmpty(entity.entityFaction) && _entitiesByFaction.ContainsKey(entity.entityFaction))
            {
                _entitiesByFaction[entity.entityFaction].Remove(entityID);
            }

            Lineage.Ancestral.Legacies.Debug.Log($"Removed entity template: {entity.entityName} (ID: {entityID})");
            return true;
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Gets an entity template by ID.
        /// </summary>
        /// <param name="entityID">The ID of the entity to retrieve.</param>
        /// <returns>The entity template if found, default otherwise.</returns>
        public static Entity GetEntityByID(int entityID)
        {
            return _entityTemplates.TryGetValue(entityID, out var entity) ? entity : default;
        }

        /// <summary>
        /// Gets an entity template by name.
        /// </summary>
        /// <param name="entityName">The name of the entity to retrieve.</param>
        /// <returns>The entity template if found, default otherwise.</returns>
        public static Entity GetEntityByName(string entityName)
        {
            if (_entityNameLookup.TryGetValue(entityName.ToLower(), out int entityID))
            {
                return GetEntityByID(entityID);
            }
            return default;
        }

        /// <summary>
        /// Gets all entities of a specific type.
        /// </summary>
        /// <param name="entityType">The type of entities to retrieve.</param>
        /// <returns>A list of entities of the specified type.</returns>
        public static List<Entity> GetEntitiesByType(EntityType entityType)
        {
            if (_entitiesByType.TryGetValue(entityType, out var entityIDs))
            {
                return entityIDs.Select(id => _entityTemplates[id]).ToList();
            }
            return new List<Entity>();
        }

        /// <summary>
        /// Gets all entities belonging to a specific faction.
        /// </summary>
        /// <param name="faction">The faction to search for.</param>
        /// <returns>A list of entities belonging to the specified faction.</returns>
        public static List<Entity> GetEntitiesByFaction(string faction)
        {
            if (_entitiesByFaction.TryGetValue(faction, out var entityIDs))
            {
                return entityIDs.Select(id => _entityTemplates[id]).ToList();
            }
            return new List<Entity>();
        }

        /// <summary>
        /// Gets entities filtered by rarity.
        /// </summary>
        /// <param name="rarity">The rarity level to filter by.</param>
        /// <returns>A list of entities with the specified rarity.</returns>
        public static List<Entity> GetEntitiesByRarity(Rarity rarity)
        {
            return _entityTemplates.Values.Where(e => e.rarity == rarity).ToList();
        }

        /// <summary>
        /// Gets entities within a specific level range.
        /// </summary>
        /// <param name="minLevel">The minimum level (inclusive).</param>
        /// <param name="maxLevel">The maximum level (inclusive).</param>
        /// <returns>A list of entities within the specified level range.</returns>
        public static List<Entity> GetEntitiesByLevelRange(int minLevel, int maxLevel)
        {
            return _entityTemplates.Values.Where(e => e.level >= minLevel && e.level <= maxLevel).ToList();
        }

        /// <summary>
        /// Searches for entities by name pattern.
        /// </summary>
        /// <param name="searchPattern">The pattern to search for in entity names.</param>
        /// <returns>A list of entities matching the search pattern.</returns>
        public static List<Entity> SearchEntitiesByName(string searchPattern)
        {
            var pattern = searchPattern.ToLower();
            return _entityTemplates.Values.Where(e => e.entityName.ToLower().Contains(pattern)).ToList();
        }

        /// <summary>
        /// Gets a random entity from the repository.
        /// </summary>
        /// <returns>A random entity template.</returns>
        public static Entity GetRandomEntity()
        {
            if (EntityCount == 0) return default;
            
            var entityIDs = _entityTemplates.Keys.ToArray();
            var randomIndex = UnityEngine.Random.Range(0, entityIDs.Length);
            return _entityTemplates[entityIDs[randomIndex]];
        }

        /// <summary>
        /// Gets a random entity of a specific type.
        /// </summary>
        /// <param name="entityType">The type of entity to randomly select.</param>
        /// <returns>A random entity of the specified type.</returns>
        public static Entity GetRandomEntityByType(EntityType entityType)
        {
            var entitiesOfType = GetEntitiesByType(entityType);
            if (entitiesOfType.Count == 0) return default;
            
            var randomIndex = UnityEngine.Random.Range(0, entitiesOfType.Count);
            return entitiesOfType[randomIndex];
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if an entity with the specified ID exists.
        /// </summary>
        /// <param name="entityID">The ID to check.</param>
        /// <returns>True if the entity exists.</returns>
        public static bool EntityExists(int entityID)
        {
            return _entityTemplates.ContainsKey(entityID);
        }

        /// <summary>
        /// Checks if an entity with the specified name exists.
        /// </summary>
        /// <param name="entityName">The name to check.</param>
        /// <returns>True if the entity exists.</returns>
        public static bool EntityExists(string entityName)
        {
            return _entityNameLookup.ContainsKey(entityName.ToLower());
        }

        /// <summary>
        /// Gets the next available entity ID.
        /// </summary>
        /// <returns>The next available entity ID.</returns>
        public static int GetNextEntityID()
        {
            return _nextEntityID;
        }

        /// <summary>
        /// Clears all entity templates from the repository.
        /// </summary>
        public static void ClearAllTemplates()
        {
            _entityTemplates?.Clear();
            _entityNameLookup?.Clear();
            _entitiesByType?.Clear();
            _entitiesByFaction?.Clear();
            _nextEntityID = 1;
            
            Lineage.Ancestral.Legacies.Debug.Log("Cleared all entity templates from repository.");
        }

        /// <summary>
        /// Gets statistics about the entity repository.
        /// </summary>
        /// <returns>A formatted string with repository statistics.</returns>
        public static string GetRepositoryStats()
        {
            var stats = $"Entity Repository Statistics:\n";
            stats += $"Total Entities: {EntityCount}\n";
            
            foreach (var kvp in _entitiesByType)
            {
                if (kvp.Value.Count > 0)
                {
                    stats += $"{kvp.Key}: {kvp.Value.Count}\n";
                }
            }
            
            stats += $"Factions: {_entitiesByFaction.Count}\n";
            return stats;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates the integrity of the entity repository.
        /// </summary>
        /// <returns>True if the repository is valid.</returns>
        public static bool ValidateRepository()
        {
            bool isValid = true;
            
            // Check for orphaned entries in lookup tables
            foreach (var kvp in _entityNameLookup)
            {
                if (!_entityTemplates.ContainsKey(kvp.Value))
                {
                    Lineage.Ancestral.Legacies.Debug.Log($"Orphaned name lookup entry: {kvp.Key} -> {kvp.Value}");
                    isValid = false;
                }
            }
            
            // Check for inconsistent type categorizations
            foreach (var kvp in _entitiesByType)
            {
                foreach (var entityID in kvp.Value)
                {
                    if (_entityTemplates.TryGetValue(entityID, out var entity))
                    {
                        if (!entity.entityType.Contains(kvp.Key))
                        {
                            Lineage.Ancestral.Legacies.Debug.Log($"Entity {entity.entityName} categorized incorrectly in type {kvp.Key}");
                            isValid = false;
                        }
                    }
                    else
                    {
                        Lineage.Ancestral.Legacies.Debug.Log($"Orphaned type entry: {kvp.Key} -> {entityID}");
                        isValid = false;
                    }
                }
            }
            
            if (isValid)
            {
                Lineage.Ancestral.Legacies.Debug.Log("Entity repository validation passed.");
            }
            else
            {
                Lineage.Ancestral.Legacies.Debug.Log("Entity repository validation failed!");
            }
            
            return isValid;
        }

        #endregion
    }
}
