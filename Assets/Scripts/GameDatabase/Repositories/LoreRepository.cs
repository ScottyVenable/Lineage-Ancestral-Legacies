using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing lore and journal data.
    /// </summary>
    public static class LoreRepository
    {
        #region Databases
        public static List<LoreEntry> AllLoreEntries { get; private set; } = new List<LoreEntry>();
        public static List<JournalEntry> AllJournalEntries { get; private set; } = new List<JournalEntry>();
        #endregion

        #region Lore Methods
        /// <summary>
        /// Gets a lore entry by its ID.
        /// </summary>
        /// <param name="loreID">The ID of the lore entry to retrieve.</param>
        /// <returns>The lore entry with the specified ID, or null if not found.</returns>
        public static LoreEntry GetLoreEntryByID(string loreID)
        {
            return AllLoreEntries.FirstOrDefault(l => l.ID == loreID);
        }

        /// <summary>
        /// Gets lore entries by their category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>A list of lore entries in the specified category.</returns>
        public static List<LoreEntry> GetLoreEntriesByCategory(string category)
        {
            return AllLoreEntries.Where(l => l.Category == category).ToList();
        }

        /// <summary>
        /// Gets lore entries by their importance level.
        /// </summary>
        /// <param name="importance">The importance level to filter by.</param>
        /// <returns>A list of lore entries with the specified importance.</returns>
        public static List<LoreEntry> GetLoreEntriesByImportance(LoreImportance importance)
        {
            return AllLoreEntries.Where(l => l.Importance == importance).ToList();
        }

        /// <summary>
        /// Gets public lore entries (discoverable by players).
        /// </summary>
        /// <returns>A list of public lore entries.</returns>
        public static List<LoreEntry> GetPublicLoreEntries()
        {
            return AllLoreEntries.Where(l => l.IsPublic).ToList();
        }

        /// <summary>
        /// Gets discovered lore entries.
        /// </summary>
        /// <returns>A list of discovered lore entries.</returns>
        public static List<LoreEntry> GetDiscoveredLoreEntries()
        {
            return AllLoreEntries.Where(l => l.isDiscovered).ToList();
        }

        /// <summary>
        /// Gets lore entries by tag.
        /// </summary>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>A list of lore entries with the specified tag.</returns>
        public static List<LoreEntry> GetLoreEntriesByTag(string tag)
        {
            return AllLoreEntries.Where(l => l.Tags.Contains(tag)).ToList();
        }

        /// <summary>
        /// Legacy property for backward compatibility.
        /// </summary>
        public static List<LoreEntry> LoreEntries
        {
            get => AllLoreEntries;
            set => AllLoreEntries = value ?? new List<LoreEntry>();
        }
        #endregion

        #region Journal Methods
        /// <summary>
        /// Gets journal entries by their type.
        /// </summary>
        /// <param name="entryType">The type of journal entries to retrieve.</param>
        /// <returns>A list of journal entries of the specified type.</returns>
        public static List<JournalEntry> GetJournalEntriesByType(EntryType entryType)
        {
            return AllJournalEntries.Where(j => j.type == entryType).ToList();
        }

        /// <summary>
        /// Gets important journal entries.
        /// </summary>
        /// <returns>A list of important journal entries.</returns>
        public static List<JournalEntry> GetImportantJournalEntries()
        {
            return AllJournalEntries.Where(j => j.isImportant).ToList();
        }

        /// <summary>
        /// Gets journal entries by title search.
        /// </summary>
        /// <param name="searchTerm">The search term to look for in titles.</param>
        /// <returns>A list of journal entries with matching titles.</returns>
        public static List<JournalEntry> SearchJournalEntriesByTitle(string searchTerm)
        {
            return AllJournalEntries.Where(j => j.title.ToLower().Contains(searchTerm.ToLower())).ToList();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes all lore-related databases with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            InitializeLoreEntries();
            InitializeJournalEntries();

            Log.Info("LoreRepository: Database initialized successfully.", Log.LogCategory.Systems);
        }

        private static void InitializeLoreEntries()
        {
            AllLoreEntries.Clear();
            // TODO: Add default lore entry data initialization
        }

        private static void InitializeJournalEntries()
        {
            AllJournalEntries.Clear();
            // TODO: Add default journal entry data initialization
        }
        #endregion
    }
}
