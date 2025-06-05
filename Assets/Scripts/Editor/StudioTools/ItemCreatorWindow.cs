using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Database.GameData;
using Lineage.Ancestral.Legacies.Editor.StudioTools.Core;
using Lineage.Ancestral.Legacies.Systems.Inventory;

namespace Lineage.Ancestral.Legacies.Editor.StudioTools
{
    /// <summary>
    /// Item Creator & Editor window for designing and managing items in the Lineage game.
    /// Provides comprehensive tools for creating, editing, and configuring all item properties.
    /// </summary>
    public class ItemCreatorWindow : BaseStudioEditorWindow
    {
        private static ItemCreatorWindow window;
        private Vector2 scrollPosition;
        private Vector2 previewScrollPosition;
        
        // Item being edited
        private Item currentItem;
        private bool isEditingExisting = false;
        private int editingItemID = -1;
        
        // UI State
        private int selectedTabIndex = 0;
        private readonly string[] tabNames = { "Basic Info", "Properties", "Tags & Effects", "Preview" };
        
        // Temporary editing variables
        private string itemName = "New Item";
        private Item.ID selectedItemID = Item.ID.IronSword;
        private Item.ItemType selectedItemType = Item.ItemType.Miscellaneous;
        private Item.ItemRarity selectedRarity = Item.ItemRarity.Common;
        private Item.ItemQuality selectedQuality = Item.ItemQuality.Fair;
        private Item.ItemSlot selectedSlot = Item.ItemSlot.Weapon;
        
        // Properties
        private float itemWeight = 1f;
        private int itemQuantity = 1;
        private int itemValue = 10;
        private bool isStackable = false;
        private bool isEquippable = false;
        private bool isConsumable = false;
        private bool isQuestItem = false;

        // ScriptableObject persistence demo
        private ItemSO itemAsset;
        
        // Equipment Properties (if equippable)
        private float attackBonus = 0f;
        private float defenseBonus = 0f;
        private float speedBonus = 0f;
        private float magicPowerBonus = 0f;
        private float magicDefenseBonus = 0f;
        private float durability = 100f;
        private float maxDurability = 100f;
        
        // Consumable Properties (if consumable)
        private float healthRestore = 0f;
        private float manaRestore = 0f;
        private float hungerRestore = 0f;
        private float thirstRestore = 0f;
        private float energyRestore = 0f;
        private bool hasBuffEffect = false;
        private Buff.ID buffToApply = Buff.ID.HealthRegen;
        
        // Visual & Description
        private Sprite itemIcon = null;
        private string itemDescription = "";
        private Color itemColor = Color.white;
        
        // Tags
        private List<string> itemTags = new List<string>();
        private string newTag = "";
        
        // Crafting
        private bool isCraftable = false;
        private List<CraftingIngredient> craftingIngredients = new List<CraftingIngredient>();
        
        // Styles handled by BaseStudioEditorWindow

        [System.Serializable]
        public class CraftingIngredient
        {
            public Item.ID itemID;
            public int quantity;
            
            public CraftingIngredient(Item.ID id, int qty)
            {
                itemID = id;
                quantity = qty;
            }
        }

        [MenuItem("Lineage Studio/Content Creation Tools/Item Creator")]
        public static void ShowWindow()
        {
            ShowWindow(-1);
        }
        
        public static void ShowWindow(int itemID = -1)
        {
            window = GetWindow<ItemCreatorWindow>("Item Creator");
            window.minSize = new Vector2(800, 700);
            
            if (itemID >= 0)
            {
                window.LoadItemForEditing(itemID);
            }
            else
            {
                window.ResetToDefaults();
            }
            
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Item Creator", "Create and edit game items");
            InitializeStyles();
            
            if (!isEditingExisting)
            {
                ResetToDefaults();
            }
        }



        private void ResetToDefaults()
        {
            itemName = "New Item";
            selectedItemID = Item.ID.IronSword;
            selectedItemType = Item.ItemType.Miscellaneous;
            selectedRarity = Item.ItemRarity.Common;
            selectedQuality = Item.ItemQuality.Fair;
            selectedSlot = Item.ItemSlot.Weapon;
            
            itemWeight = 1f;
            itemQuantity = 1;
            itemValue = 10;
            isStackable = false;
            isEquippable = false;
            isConsumable = false;
            isQuestItem = false;
            
            attackBonus = 0f;
            defenseBonus = 0f;
            speedBonus = 0f;
            magicPowerBonus = 0f;
            magicDefenseBonus = 0f;
            durability = 100f;
            maxDurability = 100f;
            
            healthRestore = 0f;
            manaRestore = 0f;
            hungerRestore = 0f;
            thirstRestore = 0f;
            energyRestore = 0f;
            hasBuffEffect = false;
            buffToApply = Buff.ID.HealthRegen;
            
            itemIcon = null;
            itemDescription = "";
            itemColor = Color.white;
            
            itemTags.Clear();
            newTag = "";
            
            isCraftable = false;
            craftingIngredients.Clear();
            
            isEditingExisting = false;
            editingItemID = -1;
        }

        private void LoadItemForEditing(int itemID)
        {
            var item = GameData.GetItemByID((Item.ID)itemID);
            if (item.itemID == itemID)
            {
                isEditingExisting = true;
                editingItemID = itemID;
                currentItem = item;
                
                // Load item data into editing variables
                itemName = item.itemName;
                selectedItemID = (Item.ID)item.itemID;
                selectedItemType = item.itemType;
                selectedRarity = item.itemRarity;
                selectedQuality = item.itemQuality;
                
                itemWeight = item.weight;
                itemQuantity = item.quantity;
                itemValue = item.value;
                
                // Determine item properties based on type and tags
                isStackable = item.quantity > 1;
                isEquippable = item.itemType == Item.ItemType.Weapon || item.itemType == Item.ItemType.Armor;
                isConsumable = item.itemType == Item.ItemType.Consumable;
                isQuestItem = item.itemType == Item.ItemType.QuestItem;
                
                // Load tags
                itemTags = item.tags != null ? new List<string>(item.tags) : new List<string>();
                
                // Set appropriate defaults based on item type
                if (isEquippable)
                {
                    durability = 100f;
                    maxDurability = 100f;
                    
                    if (item.itemType == Item.ItemType.Weapon)
                    {
                        attackBonus = 10f; // Default weapon attack bonus
                        selectedSlot = Item.ItemSlot.Weapon;
                    }
                    else if (item.itemType == Item.ItemType.Armor)
                    {
                        defenseBonus = 5f; // Default armor defense bonus
                        selectedSlot = Item.ItemSlot.Chest;
                    }
                }
                
                if (isConsumable)
                {
                    // Set defaults based on item name or tags
                    if (itemName.ToLower().Contains("health") || itemTags.Contains("Healing"))
                    {
                        healthRestore = 25f;
                    }
                    else if (itemName.ToLower().Contains("mana") || itemTags.Contains("Mana"))
                    {
                        manaRestore = 25f;
                    }
                    else if (itemName.ToLower().Contains("food") || itemTags.Contains("Food"))
                    {
                        hungerRestore = 20f;
                    }
                }
            }
        }

        private void OnGUI()
        {
            InitializeStyles();
            DrawHeader();
            DrawStatusBar();

            itemAsset = (ItemSO)EditorGUILayout.ObjectField("Item Asset", itemAsset, typeof(ItemSO), false);
            if (itemAsset != null)
            {
                EditorGUILayout.Space(5);
                GenericEditorUIDrawer.DrawObjectFields(itemAsset);
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(itemAsset);
                }
                EditorGUILayout.Space(10);
            }
            DrawTabs();
            DrawContent();
            DrawButtons();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            string title = isEditingExisting ? $"Editing Item: {itemName}" : "Create New Item";
            EditorGUILayout.LabelField(title, headerStyle);
            EditorGUILayout.Space(10);
        }

        private void DrawTabs()
        {
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);
        }

        private void DrawContent()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTabIndex)
            {
                case 0: DrawBasicInfoTab(); break;
                case 1: DrawPropertiesTab(); break;
                case 2: DrawTagsAndEffectsTab(); break;
                case 3: DrawPreviewTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawBasicInfoTab()
        {
            EditorGUILayout.LabelField("Basic Item Information", subHeaderStyle);
            EditorGUILayout.Space(10);
            
            // Identity
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            itemName = EditorGUILayout.TextField("Item Name", itemName);
            selectedItemID = (Item.ID)EditorGUILayout.EnumPopup("Item ID", selectedItemID);
            itemDescription = EditorGUILayout.TextArea(itemDescription, GUILayout.Height(60));
            itemIcon = (Sprite)EditorGUILayout.ObjectField("Icon", itemIcon, typeof(Sprite), false);
            itemColor = EditorGUILayout.ColorField("Item Color", itemColor);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Classification
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Classification", EditorStyles.boldLabel);
            selectedItemType = (Item.ItemType)EditorGUILayout.EnumPopup("Item Type", selectedItemType);
            selectedRarity = (Item.ItemRarity)EditorGUILayout.EnumPopup("Rarity", selectedRarity);
            selectedQuality = (Item.ItemQuality)EditorGUILayout.EnumPopup("Quality", selectedQuality);
            
            if (selectedItemType == Item.ItemType.Weapon || selectedItemType == Item.ItemType.Armor)
            {
                selectedSlot = (Item.ItemSlot)EditorGUILayout.EnumPopup("Equipment Slot", selectedSlot);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Basic Properties
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Basic Properties", EditorStyles.boldLabel);
            itemWeight = EditorGUILayout.FloatField("Weight", itemWeight);
            itemQuantity = EditorGUILayout.IntField("Quantity", itemQuantity);
            itemValue = EditorGUILayout.IntField("Value (Gold)", itemValue);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Item Categories
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Item Categories", EditorStyles.boldLabel);
            isStackable = EditorGUILayout.Toggle("Stackable", isStackable);
            isEquippable = EditorGUILayout.Toggle("Equippable", isEquippable);
            isConsumable = EditorGUILayout.Toggle("Consumable", isConsumable);
            isQuestItem = EditorGUILayout.Toggle("Quest Item", isQuestItem);
            isCraftable = EditorGUILayout.Toggle("Craftable", isCraftable);
            EditorGUILayout.EndVertical();
        }

        private void DrawPropertiesTab()
        {
            EditorGUILayout.LabelField("Item Properties", subHeaderStyle);
            EditorGUILayout.Space(10);
            
            if (isEquippable)
            {
                DrawEquipmentProperties();
            }
            
            if (isConsumable)
            {
                DrawConsumableProperties();
            }
            
            if (isCraftable)
            {
                DrawCraftingProperties();
            }
            
            if (!isEquippable && !isConsumable && !isCraftable)
            {
                EditorGUILayout.HelpBox("Select item categories in the Basic Info tab to configure properties.", MessageType.Info);
            }
        }

        private void DrawEquipmentProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Equipment Properties", EditorStyles.boldLabel);
            
            attackBonus = EditorGUILayout.FloatField("Attack Bonus", attackBonus);
            defenseBonus = EditorGUILayout.FloatField("Defense Bonus", defenseBonus);
            speedBonus = EditorGUILayout.FloatField("Speed Bonus", speedBonus);
            magicPowerBonus = EditorGUILayout.FloatField("Magic Power Bonus", magicPowerBonus);
            magicDefenseBonus = EditorGUILayout.FloatField("Magic Defense Bonus", magicDefenseBonus);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Durability", EditorStyles.boldLabel);
            maxDurability = EditorGUILayout.FloatField("Max Durability", maxDurability);
            durability = EditorGUILayout.Slider("Current Durability", durability, 0f, maxDurability);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawConsumableProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Consumable Properties", EditorStyles.boldLabel);
            
            healthRestore = EditorGUILayout.FloatField("Health Restore", healthRestore);
            manaRestore = EditorGUILayout.FloatField("Mana Restore", manaRestore);
            hungerRestore = EditorGUILayout.FloatField("Hunger Restore", hungerRestore);
            thirstRestore = EditorGUILayout.FloatField("Thirst Restore", thirstRestore);
            energyRestore = EditorGUILayout.FloatField("Energy Restore", energyRestore);
            
            EditorGUILayout.Space(5);
            hasBuffEffect = EditorGUILayout.Toggle("Apply Buff Effect", hasBuffEffect);
            if (hasBuffEffect)
            {
                buffToApply = (Buff.ID)EditorGUILayout.EnumPopup("Buff to Apply", buffToApply);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawCraftingProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Crafting Recipe", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Required Ingredients:");
            
            for (int i = 0; i < craftingIngredients.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                craftingIngredients[i].itemID = (Item.ID)EditorGUILayout.EnumPopup(craftingIngredients[i].itemID);
                craftingIngredients[i].quantity = EditorGUILayout.IntField(craftingIngredients[i].quantity, GUILayout.Width(60));
                
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    craftingIngredients.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Ingredient"))
            {
                craftingIngredients.Add(new CraftingIngredient(Item.ID.IronSword, 1));
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawTagsAndEffectsTab()
        {
            EditorGUILayout.LabelField("Tags & Special Effects", subHeaderStyle);
            EditorGUILayout.Space(10);
            
            // Tags Management
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            
            // Add new tag
            EditorGUILayout.BeginHorizontal();
            newTag = EditorGUILayout.TextField("New Tag", newTag);
            if (GUILayout.Button("Add Tag", GUILayout.Width(80)) && !string.IsNullOrEmpty(newTag))
            {
                if (!itemTags.Contains(newTag))
                {
                    itemTags.Add(newTag);
                }
                newTag = "";
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Display existing tags
            for (int i = itemTags.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(itemTags[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    itemTags.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Quick Tag Presets based on item type
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Quick Tag Presets", EditorStyles.boldLabel);
            
            if (selectedItemType == Item.ItemType.Weapon)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Melee")) AddTagIfNotExists("Melee");
                if (GUILayout.Button("Ranged")) AddTagIfNotExists("Ranged");
                if (GUILayout.Button("Magic")) AddTagIfNotExists("Magic");
                if (GUILayout.Button("Two-Handed")) AddTagIfNotExists("Two-Handed");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Sword")) AddTagIfNotExists("Sword");
                if (GUILayout.Button("Axe")) AddTagIfNotExists("Axe");
                if (GUILayout.Button("Bow")) AddTagIfNotExists("Bow");
                if (GUILayout.Button("Staff")) AddTagIfNotExists("Staff");
                EditorGUILayout.EndHorizontal();
            }
            else if (selectedItemType == Item.ItemType.Armor)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Light")) AddTagIfNotExists("Light");
                if (GUILayout.Button("Medium")) AddTagIfNotExists("Medium");
                if (GUILayout.Button("Heavy")) AddTagIfNotExists("Heavy");
                if (GUILayout.Button("Magical")) AddTagIfNotExists("Magical");
                EditorGUILayout.EndHorizontal();
            }
            else if (selectedItemType == Item.ItemType.Consumable)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Potion")) AddTagIfNotExists("Potion");
                if (GUILayout.Button("Food")) AddTagIfNotExists("Food");
                if (GUILayout.Button("Healing")) AddTagIfNotExists("Healing");
                if (GUILayout.Button("Mana")) AddTagIfNotExists("Mana");
                EditorGUILayout.EndHorizontal();
            }
            
            // Universal tags
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rare")) AddTagIfNotExists("Rare");
            if (GUILayout.Button("Enchanted")) AddTagIfNotExists("Enchanted");
            if (GUILayout.Button("Cursed")) AddTagIfNotExists("Cursed");
            if (GUILayout.Button("Ancient")) AddTagIfNotExists("Ancient");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewTab()
        {
            EditorGUILayout.LabelField("Item Preview", subHeaderStyle);
            EditorGUILayout.Space(10);
            
            previewScrollPosition = EditorGUILayout.BeginScrollView(previewScrollPosition);
            
            var previewItem = BuildItemFromCurrentSettings();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Generated Item Data", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"Name: {previewItem.itemName}");
            EditorGUILayout.LabelField($"ID: {previewItem.itemID} ({(Item.ID)previewItem.itemID})");
            EditorGUILayout.LabelField($"Type: {previewItem.itemType}");
            EditorGUILayout.LabelField($"Rarity: {previewItem.itemRarity} | Quality: {previewItem.itemQuality}");
            EditorGUILayout.LabelField($"Weight: {previewItem.weight} | Value: {previewItem.value} gold");
            EditorGUILayout.LabelField($"Quantity: {previewItem.quantity}");
            
            if (!string.IsNullOrEmpty(itemDescription))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(itemDescription, EditorStyles.wordWrappedLabel);
            }
            
            if (isEquippable)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Equipment Bonuses:", EditorStyles.boldLabel);
                if (attackBonus > 0) EditorGUILayout.LabelField($"  Attack: +{attackBonus}");
                if (defenseBonus > 0) EditorGUILayout.LabelField($"  Defense: +{defenseBonus}");
                if (speedBonus > 0) EditorGUILayout.LabelField($"  Speed: +{speedBonus}");
                if (magicPowerBonus > 0) EditorGUILayout.LabelField($"  Magic Power: +{magicPowerBonus}");
                if (magicDefenseBonus > 0) EditorGUILayout.LabelField($"  Magic Defense: +{magicDefenseBonus}");
                EditorGUILayout.LabelField($"  Durability: {durability}/{maxDurability}");
            }
            
            if (isConsumable)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Consumable Effects:", EditorStyles.boldLabel);
                if (healthRestore > 0) EditorGUILayout.LabelField($"  Restores {healthRestore} Health");
                if (manaRestore > 0) EditorGUILayout.LabelField($"  Restores {manaRestore} Mana");
                if (hungerRestore > 0) EditorGUILayout.LabelField($"  Restores {hungerRestore} Hunger");
                if (thirstRestore > 0) EditorGUILayout.LabelField($"  Restores {thirstRestore} Thirst");
                if (energyRestore > 0) EditorGUILayout.LabelField($"  Restores {energyRestore} Energy");
                if (hasBuffEffect) EditorGUILayout.LabelField($"  Applies buff: {buffToApply}");
            }
            
            if (isCraftable && craftingIngredients.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Crafting Recipe:", EditorStyles.boldLabel);
                foreach (var ingredient in craftingIngredients)
                {
                    EditorGUILayout.LabelField($"  {ingredient.quantity}x {ingredient.itemID}");
                }
            }
            
            if (previewItem.tags != null && previewItem.tags.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Tags: {string.Join(", ", previewItem.tags)}");
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Item", 
                    "This will reset all values to defaults. Continue?", "Yes", "Cancel"))
                {
                    ResetToDefaults();
                }
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Close();
            }
            
            string buttonText = isEditingExisting ? "Update Item" : "Create Item";
            if (GUILayout.Button(buttonText, GUILayout.Width(120), GUILayout.Height(30)))
            {
                if (ValidateItem())
                {
                    if (isEditingExisting)
                    {
                        UpdateItem();
                    }
                    else
                    {
                        CreateItem();
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void AddTagIfNotExists(string tag)
        {
            if (!itemTags.Contains(tag))
            {
                itemTags.Add(tag);
            }
        }

        private bool ValidateItem()
        {
            if (string.IsNullOrEmpty(itemName))
            {
                EditorUtility.DisplayDialog("Validation Error", "Item name cannot be empty.", "OK");
                return false;
            }
            
            if (itemWeight < 0)
            {
                EditorUtility.DisplayDialog("Validation Error", "Weight cannot be negative.", "OK");
                return false;
            }
            
            if (itemValue < 0)
            {
                EditorUtility.DisplayDialog("Validation Error", "Value cannot be negative.", "OK");
                return false;
            }
            
            if (itemQuantity <= 0)
            {
                EditorUtility.DisplayDialog("Validation Error", "Quantity must be greater than 0.", "OK");
                return false;
            }
            
            if (isEquippable && maxDurability <= 0)
            {
                EditorUtility.DisplayDialog("Validation Error", "Max durability must be greater than 0 for equipment.", "OK");
                return false;
            }
            
            return true;
        }

        private Item BuildItemFromCurrentSettings()
        {
            var item = new Item(itemName, selectedItemID, selectedItemType, 
                itemWeight, itemQuantity, itemValue, selectedRarity, selectedQuality);
            
            // Set tags
            item.tags = new List<string>(itemTags);
            
            // Add automatic tags based on properties
            if (isStackable) item.tags.Add("Stackable");
            if (isEquippable) item.tags.Add("Equippable");
            if (isConsumable) item.tags.Add("Consumable");
            if (isQuestItem) item.tags.Add("QuestItem");
            if (isCraftable) item.tags.Add("Craftable");
            
            // Add equipment slot tag if equippable
            if (isEquippable)
            {
                item.tags.Add(selectedSlot.ToString());
            }
            
            // Add effect tags for consumables
            if (isConsumable)
            {
                if (healthRestore > 0) item.tags.Add("Healing");
                if (manaRestore > 0) item.tags.Add("ManaRestoration");
                if (hungerRestore > 0) item.tags.Add("Food");
                if (thirstRestore > 0) item.tags.Add("Drink");
                if (hasBuffEffect) item.tags.Add("BuffEffect");
            }
            
            return item;
        }

        private void CreateItem()
        {
            var item = BuildItemFromCurrentSettings();
            
            // Check for duplicate ID
            if (GameData.itemDatabase.Any(i => i.itemID == item.itemID))
            {
                if (!EditorUtility.DisplayDialog("Duplicate ID", 
                    $"An item with ID {item.itemID} already exists. Overwrite?", "Yes", "Cancel"))
                {
                    return;
                }
                
                GameData.itemDatabase.RemoveAll(i => i.itemID == item.itemID);
            }
            
            GameData.itemDatabase.Add(item);
            
            Debug.Log.Info($"Item '{item.itemName}' created successfully with ID {item.itemID}!", Debug.Log.LogCategory.Systems);
            EditorUtility.DisplayDialog("Success", $"Item '{item.itemName}' created successfully!", "OK");
            ShowStatus($"Item '{item.itemName}' created.", MessageType.Info);
            
            ResetToDefaults();
        }

        private void UpdateItem()
        {
            var item = BuildItemFromCurrentSettings();
            
            // Remove old item
            GameData.itemDatabase.RemoveAll(i => i.itemID == editingItemID);
            
            // Add updated item
            GameData.itemDatabase.Add(item);
            
            Debug.Log.Info($"Item '{item.itemName}' updated successfully!", Debug.Log.LogCategory.Systems);
            EditorUtility.DisplayDialog("Success", $"Item '{item.itemName}' updated successfully!", "OK");
            ShowStatus($"Item '{item.itemName}' updated.", MessageType.Info);
            
            Close();
        }
    }
}
