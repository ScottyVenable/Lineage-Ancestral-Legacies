# Comprehensive GameData System Plan for Unity (with Buffs, Debuffs, Tags, Tiles, Structures, Recipes)

This document provides a robust plan for a scalable, designer/modder-friendly GameData system for *Lineages: Ancestral Legacies*, using Unity’s ScriptableObjects and JSON mod support. Now includes: Buffs/Debuffs, expanded tags, tile data, structure/building data, recipes, and extensibility for modding.

---

## 1. Core Philosophy: Data-Driven Design with ScriptableObjects
- Decoupled, reusable, performant data assets
- Designer-friendly editing in the Inspector
- Version control support
- Extensible and easy to reference

---

## 2. Recommended Project Folder Structure

```plaintext
Assets/
└── GameData/
    ├── _Core/
    │   ├── Tags/
    │   └── Enums/
    ├── Items/
    │   ├── Definitions/
    │   │   ├── Weapons/
    │   │   ├── Armor/
    │   │   ├── Consumables/
    │   │   └── Resources/
    │   └── ItemProperties/
    ├── Entities/
    │   ├── Definitions/
    │   │   ├── Enemies/
    │   │   ├── PlayerUnits/
    │   │   └── NPCs/
    │   └── EntityProperties/
    ├── World/
    │   ├── ResourceNodes/
    │   ├── Tiles/
    │   └── Structures/
    ├── Generation/
    │   └── NameLists/
    ├── Abilities/
    ├── Status/
    ├── Quests/
    ├── Recipes/
    └── LootTables/
```

---

## 3. Foundational System: The Tag System
// See previous version for tag classes and explanations. All new data types below include tag arrays for flexible filtering and modding.

---

## 4. Item System (with tags)
// See previous version for base class and how tags are included. Items may now include construction/crafting tags.

---

## 5. Name Generation System (with tags)
// See previous version.

---

## 6. Entity System (with tags)
// See previous version.

---

## 7. World Resource Node System (with tags)
// See previous version.

---

## 8. Buffs & Debuffs System (with tags)
// See previous version.

---

## 9. Tile Data System

### Tile_SO.cs
```csharp
// A tile definition for tilemap systems (terrain, environment, etc.)
[CreateAssetMenu(fileName = "NewTile", menuName = "GameData/World/Tile Definition")]
public class Tile_SO : TaggableScriptableObject {
    public string tileID; // Unique (e.g., "GRASS", "STONE", "WATER")
    public string displayName;
    public Sprite tileSprite; // Main visual
    public bool isWalkable = true; // Can entities walk over it?
    public bool blocksLOS = false; // Blocks line of sight?
    public float movementCost = 1.0f; // Movement penalty
    public List<Tag_SO> terrainTags; // E.g., "Forest", "Mountain", "Wet"
}
```

#### Example JSON for Tile
```json
{
  "tileID": "GRASS",
  "displayName": "Grassland",
  "tileSprite": null,
  "isWalkable": true,
  "blocksLOS": false,
  "movementCost": 1.0,
  "tags": ["Grass", "Nature"]
}
```

---

## 10. Structure/Building Data System

### Structure_SO.cs
```csharp
// A structure/building definition (houses, farms, special sites)
[CreateAssetMenu(fileName = "NewStructure", menuName = "GameData/World/Structure Definition")]
public class Structure_SO : TaggableScriptableObject {
    public string structureID;
    public string displayName;
    public Sprite icon;
    public GameObject prefab; // 3D/2D prefab for world
    [TextArea(2,4)] public string description;
    public int maxHealth = 100;
    public List<Item_SO> constructionMaterials; // List of required items
    public float buildTimeSeconds = 30.0f;
    public List<Tag_SO> allowedBiomeTags; // Where it can be placed (e.g., "Forest", "Plains")
    public List<Tag_SO> structureTags; // E.g., "Housing", "Farm", "Military"
}
```

#### Example JSON for Structure
```json
{
  "structureID": "HOUSE_WOOD",
  "displayName": "Wooden House",
  "icon": null,
  "prefab": null,
  "description": "A simple wooden dwelling for villagers.",
  "maxHealth": 120,
  "constructionMaterials": ["WOOD_PLANK", "STONE"],
  "buildTimeSeconds": 20.0,
  "allowedBiomeTags": ["Grass", "Forest"],
  "tags": ["Housing", "Residential"]
}
```

---

## 11. Recipe Data System

### Recipe_SO.cs
```csharp
// Crafting/construction recipe definition (for items, structures, etc.)
[CreateAssetMenu(fileName = "NewRecipe", menuName = "GameData/Recipes/Recipe Definition")]
public class Recipe_SO : TaggableScriptableObject {
    public string recipeID;
    public string displayName;
    public List<Item_SO> inputItems; // Ingredients required
    public List<int> inputAmounts; // Amount per input item (same order as above)
    public List<Item_SO> outputItems; // Output(s)
    public List<int> outputAmounts; // Amount per output item
    public float craftTimeSeconds = 5.0f;
    public List<Tag_SO> requiredStationTags; // e.g., "Forge", "AlchemyLab"
    public List<Tag_SO> recipeTags; // e.g., "Weapon", "Potion", "Beginner"
}
```

#### Example JSON for Recipe
```json
{
  "recipeID": "RECIPE_IRON_SWORD",
  "displayName": "Forge Iron Sword",
  "inputItems": ["IRON_INGOT", "WOOD_PLANK"],
  "inputAmounts": [2, 1],
  "outputItems": ["WEAPON_IRON_SWORD"],
  "outputAmounts": [1],
  "craftTimeSeconds": 10.0,
  "requiredStationTags": ["Forge"],
  "tags": ["Weapon", "Craftable"]
}
```

---

## 12. JSON-Based Mod Support (with all new data types)
// Modders can add tiles, structures, recipes as JSON in the Mods directory, using the schema above.

---

## 13. Using Tags in Logic (Expanded Examples)
- **Tiles**: Pathfinding checks `isWalkable` and movement cost; biomes/quests filter by terrain tags.
- **Structures**: Placement logic uses allowedBiomeTags; upgrades, abilities, or events can query structureTags.
- **Recipes**: Crafting UI filters by tags; stations check requiredStationTags to show available recipes.
- **Other systems**: As before, tags are used for AI, abilities, loot, triggers, and mod content discovery.

---

## 14. Further Expansion
- **Decorative/Environmental Data**: Trees, rocks, flora—could follow similar SO and tag pattern.
- **Event/Trigger Data**: Events triggered by tags, tiles, structures, etc.
- **NPC Job/Role Data**: Tagged SOs for roles, professions, social structures.
- **Procedural Generation**: Tags drive biome/structure/item/world generation rules.
- **UI/UX**: Tag-based filtering, sorting, grouping everywhere (for player and modder usability).

---

This unified system is modular, scalable, and modder-friendly. You can now cover nearly every gameplay system or future expansion using this flexible data approach!
