using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region World System

    /// <summary>
    /// Represents a location in the game world.
    /// </summary>
    public class Location
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Vector3 Position { get; set; }
        public Population population { get; set; }
        public bool hasSettlement { get; set; }
        public int Seed { get; set; }
        public LoreEntry lore { get; set; }
        public List<NPC> NPCs { get; set; } = new List<NPC>();
    }

    /// <summary>
    /// Represents a chunk of the game world, which can contain multiple locations.
    /// </summary>
    public class Chunk
    {
        public int ID { get; set; }
        public int Seed { get; set; }
        public List<Location> Locations { get; set; } = new List<Location>();
        public List<Settlement> Settlements { get; set; } = new List<Settlement>();
        public List<Population> Populations { get; set; } = new List<Population>();
    }

    #endregion
}
