using UnityEngine;
using System.Collections.Generic;
using System;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Lore System

    /// <summary>
    /// Represents a timeline event within a lore entry.
    /// </summary>
    [System.Serializable]
    public class TimelineEvent
    {
        public string Title { get; set; } = "";
        public string Date { get; set; } = "";
        public string Description { get; set; } = "";
        public EventImportance Importance { get; set; } = EventImportance.Minor;
    }

    /// <summary>
    /// Represents lore entries for world-building and immersion.
    /// </summary>
    [System.Serializable]
    public class LoreEntry
    {
        public string ID { get; set; } = System.Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Category { get; set; } = "History";
        public string Content { get; set; } = "";
        public string Summary { get; set; } = "";
        public LoreImportance Importance { get; set; } = LoreImportance.Minor;
        public bool IsPublic { get; set; } = true;
        public List<string> Tags { get; set; } = new List<string>();
        public string Author { get; set; } = "";
        public string DateWritten { get; set; } = "";
        public string Source { get; set; } = "";
        public List<string> RelatedCharacters { get; set; } = new List<string>();
        public List<string> RelatedLocations { get; set; } = new List<string>();
        public List<string> RelatedEvents { get; set; } = new List<string>();
        public List<string> CrossReferences { get; set; } = new List<string>();
        public string TimelinePosition { get; set; } = "";
        public string Duration { get; set; } = "";
        public List<TimelineEvent> TimelineEvents { get; set; } = new List<TimelineEvent>();
        public string Era { get; set; } = "";
        public string Age { get; set; } = "";
        
        // Legacy properties for compatibility
        public string title 
        { 
            get => Title; 
            set => Title = value; 
        }
        
        public string content 
        { 
            get => Content; 
            set => Content = value; 
        }
        
        public bool isDiscovered { get; set; } = false;
        
        public List<string> relatedEntries 
        { 
            get => CrossReferences; 
            set => CrossReferences = value; 
        }
        
        public LegacyCategory category
        {
            get 
            {
                if (Enum.TryParse<LegacyCategory>(Category, out var result))
                    return result;
                return LegacyCategory.History;
            }
            set 
            {
                Category = value.ToString();
            }
        }
    }

    #endregion

    #region Journal System

    /// <summary>
    /// Represents a journal entry for tracking events and discoveries.
    /// </summary>
    public struct JournalEntry
    {
        public string title;
        public EntryType type;
        public string content;
        public DateTime timestamp;
        public bool isImportant;
    }

    #endregion
}
