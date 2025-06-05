using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Editor
{
    public class SkillsBuffsEditorWindow : EditorWindow
    {
        private enum ContentType { Skills, Buffs }
        private ContentType currentContentType = ContentType.Skills;
        
        // Skills Data
        private Skill currentSkill;
        private Vector2 skillScrollPosition;
        private string skillSearchFilter = "";
        private int selectedSkillTab = 0;
        private readonly string[] skillTabNames = { "Basic Info", "Preview" };
        
        // Buffs Data
        private Buff currentBuff;
        private Vector2 buffScrollPosition;
        private string buffSearchFilter = "";
        private int selectedBuffTab = 0;
        private readonly string[] buffTabNames = { "Basic Info", "Preview" };
        
        // Common
        private Vector2 listScrollPosition;
        private bool isDirty = false;
        
        [MenuItem("Lineage Studio/Content Creation/Skills & Buffs Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillsBuffsEditorWindow>("Skills & Buffs Editor");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            InitializeData();
        }
        
        private void InitializeData()
        {
            // Initialize with default values using actual constructors
            currentSkill = new Skill(Skill.ID.Combat, Skill.SkillType.Combat);
            currentBuff = new Buff(Buff.ID.HealthRegen, "Sample Buff", "Sample Description", Buff.BuffType.Temporary, 1f, 30f);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Content list
                EditorGUILayout.BeginVertical(GUILayout.Width(300));
                {
                    DrawContentTypeSelector();
                    DrawSearchField();
                    DrawContentList();
                    DrawListControls();
                }
                EditorGUILayout.EndVertical();
                
                // Right panel - Editor
                EditorGUILayout.BeginVertical();
                {
                    if (currentContentType == ContentType.Skills)
                        DrawSkillEditor();
                    else
                        DrawBuffEditor();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawContentTypeSelector()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Toggle(currentContentType == ContentType.Skills, "Skills", "Button"))
                    currentContentType = ContentType.Skills;
                if (GUILayout.Toggle(currentContentType == ContentType.Buffs, "Buffs", "Button"))
                    currentContentType = ContentType.Buffs;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        
        private void DrawSearchField()
        {
            if (currentContentType == ContentType.Skills)
            {
                skillSearchFilter = EditorGUILayout.TextField("Search Skills:", skillSearchFilter);
            }
            else
            {
                buffSearchFilter = EditorGUILayout.TextField("Search Buffs:", buffSearchFilter);
            }
            GUILayout.Space(5);
        }
        
        private void DrawContentList()
        {
            listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition, GUILayout.Height(400));
            {
                if (currentContentType == ContentType.Skills)
                    DrawSkillsList();
                else
                    DrawBuffsList();
            }
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawSkillsList()
        {
            if (GameData.skillDatabase == null) return;

            var filteredSkills = GameData.skillDatabase.Where(s =>
                string.IsNullOrEmpty(skillSearchFilter) ||
                s.skillType.ToString().ToLower().Contains(skillSearchFilter.ToLower())).ToList();
            
            foreach (var skill in filteredSkills)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    bool isSelected = currentSkill.skillID == skill.skillID;
                    
                    if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                    {
                        currentSkill = skill;
                    }
                    
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.Label(skill.skillType.ToString(), EditorStyles.boldLabel);
                        GUILayout.Label($"Level: {skill.level} | XP: {skill.experience:F1}", EditorStyles.miniLabel);
                    }
                    EditorGUILayout.EndVertical();
                    
                    if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Skill", 
                            $"Are you sure you want to delete the skill '{skill.skillType}'?", "Delete", "Cancel"))
                        {
                            GameData.skillDatabase.Remove(skill);
                            if (currentSkill.skillID == skill.skillID)
                                currentSkill = new Skill(Skill.ID.Combat, Skill.SkillType.Combat);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(2);
            }
        }
        
        private void DrawBuffsList()
        {
            if (GameData.buffDatabase == null) return;
            
            var filteredBuffs = GameData.buffDatabase.Where(b => 
                string.IsNullOrEmpty(buffSearchFilter) || 
                b.buffName.ToLower().Contains(buffSearchFilter.ToLower()) ||
                b.buffType.ToString().ToLower().Contains(buffSearchFilter.ToLower())).ToList();
                
            foreach (var buff in filteredBuffs)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    bool isSelected = currentBuff.buffID == buff.buffID;
                    
                    if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                    {
                        currentBuff = buff;
                    }
                    
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.Label(buff.buffName, EditorStyles.boldLabel);
                        GUILayout.Label($"Type: {buff.buffType}", EditorStyles.miniLabel);
                        GUILayout.Label($"Duration: {buff.duration}s | Strength: {buff.strength}", EditorStyles.miniLabel);
                    }
                    EditorGUILayout.EndVertical();
                    
                    if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Buff", 
                            $"Are you sure you want to delete the buff '{buff.buffName}'?", "Delete", "Cancel"))
                        {
                            GameData.buffDatabase.Remove(buff);
                            if (currentBuff.buffID == buff.buffID)
                                currentBuff = new Buff(Buff.ID.HealthRegen, "Sample Buff", "Sample Description", Buff.BuffType.Temporary, 1f, 30f);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(2);
            }
        }
        
        private void DrawListControls()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add New"))
                {
                    AddNewItem();
                }
                
                if (GUILayout.Button("Remove Current"))
                {
                    RemoveCurrentItem();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void AddNewItem()
        {
            if (currentContentType == ContentType.Skills)
            {
                var newSkill = new Skill(Skill.ID.Combat, Skill.SkillType.Combat, 0f, 1);
                
                if (GameData.skillDatabase == null)
                    GameData.skillDatabase = new List<Skill>();
                
                GameData.skillDatabase.Add(newSkill);
                currentSkill = newSkill;
                isDirty = true;
            }
            else
            {
                var newBuff = new Buff(Buff.ID.HealthRegen, "New Buff", "New buff description", Buff.BuffType.Temporary, 1f, 30f);
                
                if (GameData.buffDatabase == null)
                    GameData.buffDatabase = new List<Buff>();
                
                GameData.buffDatabase.Add(newBuff);
                currentBuff = newBuff;
                isDirty = true;
            }
        }
        
        private void RemoveCurrentItem()
        {
            if (currentContentType == ContentType.Skills)
            {
                if (GameData.skillDatabase != null)
                {
                    GameData.skillDatabase.RemoveAll(s => s.skillID == currentSkill.skillID);
                    isDirty = true;
                }
            }
            else
            {
                if (GameData.buffDatabase != null)
                {
                    GameData.buffDatabase.RemoveAll(b => b.buffID == currentBuff.buffID);
                    isDirty = true;
                }
            }
        }
        
        private void DrawSkillEditor()
        {
            skillScrollPosition = EditorGUILayout.BeginScrollView(skillScrollPosition);
            {
                GUILayout.Label("Skill Editor", EditorStyles.boldLabel);
                GUILayout.Space(10);
                
                selectedSkillTab = GUILayout.Toolbar(selectedSkillTab, skillTabNames);
                GUILayout.Space(10);
                
                switch (selectedSkillTab)
                {
                    case 0:
                        DrawSkillBasicInfo();
                        break;
                    case 1:
                        DrawSkillPreview();
                        break;
                }
                
                GUILayout.Space(20);
                DrawSkillSaveControls();
            }
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawSkillBasicInfo()
        {
            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            
            // Skill ID (read-only display)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup("Skill ID:", currentSkill.skillID);
            EditorGUI.EndDisabledGroup();
            
            // Skill Type
            var newSkillType = (Skill.SkillType)EditorGUILayout.EnumPopup("Skill Type:", currentSkill.skillType);
            if (newSkillType != currentSkill.skillType)
            {
                currentSkill = new Skill(currentSkill.skillID, newSkillType, currentSkill.experience, currentSkill.level);
            }
            
            // Description
            GUILayout.Label("Description:");
            var newDescription = EditorGUILayout.TextArea(currentSkill.skillDescription, GUILayout.Height(60));
            if (newDescription != currentSkill.skillDescription)
            {
                // Create new skill with updated description
                var updatedSkill = currentSkill;
                updatedSkill.skillDescription = newDescription;
                currentSkill = updatedSkill;
            }
            
            GUILayout.Space(10);
            
            // Experience and Level
            GUILayout.Label("Progression", EditorStyles.boldLabel);
            
            var newExperience = EditorGUILayout.FloatField("Experience:", currentSkill.experience);
            var newLevel = EditorGUILayout.IntField("Level:", currentSkill.level);
            
            if (newExperience != currentSkill.experience || newLevel != currentSkill.level)
            {
                currentSkill = new Skill(currentSkill.skillID, currentSkill.skillType, newExperience, newLevel);
            }
            
            GUILayout.Space(10);
            
            // Tags
            GUILayout.Label("Tags", EditorStyles.boldLabel);
            if (currentSkill.tags != null)
            {
                for (int i = 0; i < currentSkill.tags.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        currentSkill.tags[i] = EditorGUILayout.TextField(currentSkill.tags[i]);
                        if (GUILayout.Button("-", GUILayout.Width(25)))
                        {
                            currentSkill.tags.RemoveAt(i);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (GUILayout.Button("Add Tag"))
            {
                if (currentSkill.tags == null)
                {
                    var updatedSkill = currentSkill;
                    updatedSkill.tags = new List<string>();
                    currentSkill = updatedSkill;
                }
                currentSkill.tags.Add("New Tag");
            }
        }
        
        private void DrawSkillPreview()
        {
            GUILayout.Label("Skill Preview", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUI.BeginDisabledGroup(true);
            {
                GUILayout.Label($"Skill: {currentSkill.skillType}", EditorStyles.boldLabel);
                GUILayout.Label($"ID: {currentSkill.skillID}", EditorStyles.miniLabel);
                GUILayout.Label($"Level: {currentSkill.level}", EditorStyles.miniLabel);
                GUILayout.Label($"Experience: {currentSkill.experience:F1}", EditorStyles.miniLabel);
                
                if (!string.IsNullOrEmpty(currentSkill.skillDescription))
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Description:", EditorStyles.boldLabel);
                    GUILayout.Label(currentSkill.skillDescription, EditorStyles.wordWrappedLabel);
                }
                
                if (currentSkill.tags != null && currentSkill.tags.Count > 0)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Tags:", EditorStyles.boldLabel);
                    GUILayout.Label(string.Join(", ", currentSkill.tags), EditorStyles.miniLabel);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        
        private void DrawSkillSaveControls()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Skill"))
                {
                    SaveSkill();
                }
                
                if (GUILayout.Button("Reset"))
                {
                    if (EditorUtility.DisplayDialog("Reset Skill", "Are you sure you want to reset all changes?", "Reset", "Cancel"))
                    {
                        currentSkill = new Skill(Skill.ID.Combat, Skill.SkillType.Combat);
                    }
                }
                
                if (GUILayout.Button("Duplicate"))
                {
                    var newSkill = new Skill(currentSkill.skillID, currentSkill.skillType, currentSkill.experience, currentSkill.level);
                    if (GameData.skillDatabase != null)
                    {
                        GameData.skillDatabase.Add(newSkill);
                        currentSkill = newSkill;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawBuffEditor()
        {
            buffScrollPosition = EditorGUILayout.BeginScrollView(buffScrollPosition);
            {
                GUILayout.Label("Buff Editor", EditorStyles.boldLabel);
                GUILayout.Space(10);
                
                selectedBuffTab = GUILayout.Toolbar(selectedBuffTab, buffTabNames);
                GUILayout.Space(10);
                
                switch (selectedBuffTab)
                {
                    case 0:
                        DrawBuffBasicInfo();
                        break;
                    case 1:
                        DrawBuffPreview();
                        break;
                }
                
                GUILayout.Space(20);
                DrawBuffSaveControls();
            }
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawBuffBasicInfo()
        {
            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            
            // Buff ID (read-only display)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Buff ID:", currentBuff.buffID);
            EditorGUI.EndDisabledGroup();
            
            // Buff Name
            var newName = EditorGUILayout.TextField("Name:", currentBuff.buffName);
            if (newName != currentBuff.buffName)
            {
                var updatedBuff = currentBuff;
                updatedBuff.buffName = newName;
                currentBuff = updatedBuff;
            }
            
            // Buff Description
            GUILayout.Label("Description:");
            var newDescription = EditorGUILayout.TextArea(currentBuff.buffDescription, GUILayout.Height(60));
            if (newDescription != currentBuff.buffDescription)
            {
                var updatedBuff = currentBuff;
                updatedBuff.buffDescription = newDescription;
                currentBuff = updatedBuff;
            }
            
            // Buff Type
            var newType = (Buff.BuffType)EditorGUILayout.EnumPopup("Type:", currentBuff.buffType);
            if (newType != currentBuff.buffType)
            {
                var updatedBuff = currentBuff;
                updatedBuff.buffType = newType;
                currentBuff = updatedBuff;
            }
            
            GUILayout.Space(10);
            
            // Properties
            GUILayout.Label("Properties", EditorStyles.boldLabel);
            
            var newStrength = EditorGUILayout.FloatField("Strength:", currentBuff.strength);
            var newDuration = EditorGUILayout.FloatField("Duration (seconds):", currentBuff.duration);
            
            if (newStrength != currentBuff.strength || newDuration != currentBuff.duration)
            {
                var updatedBuff = currentBuff;
                updatedBuff.strength = newStrength;
                updatedBuff.duration = newDuration;
                currentBuff = updatedBuff;
            }
            
            GUILayout.Space(10);
            
            // Stat Modifiers
            GUILayout.Label("Stat Modifiers", EditorStyles.boldLabel);
            
            var newFlatModifier = EditorGUILayout.FloatField("Flat Modifier:", currentBuff.statModifiers.flatModifier);
            var newPercentageModifier = EditorGUILayout.FloatField("Percentage Modifier:", currentBuff.statModifiers.percentageModifier);
            
            if (newFlatModifier != currentBuff.statModifiers.flatModifier || newPercentageModifier != currentBuff.statModifiers.percentageModifier)
            {
                var updatedBuff = currentBuff;
                updatedBuff.statModifiers = new StatModifiers(newFlatModifier, newPercentageModifier);
                currentBuff = updatedBuff;
            }
            
            GUILayout.Space(10);
            
            // Tags
            GUILayout.Label("Tags", EditorStyles.boldLabel);
            if (currentBuff.tags != null)
            {
                for (int i = 0; i < currentBuff.tags.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        currentBuff.tags[i] = EditorGUILayout.TextField(currentBuff.tags[i]);
                        if (GUILayout.Button("-", GUILayout.Width(25)))
                        {
                            currentBuff.tags.RemoveAt(i);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            if (GUILayout.Button("Add Tag"))
            {
                if (currentBuff.tags == null)
                {
                    var updatedBuff = currentBuff;
                    updatedBuff.tags = new List<string>();
                    currentBuff = updatedBuff;
                }
                currentBuff.tags.Add("New Tag");
            }
        }
        
        private void DrawBuffPreview()
        {
            GUILayout.Label("Buff Preview", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUI.BeginDisabledGroup(true);
            {
                GUILayout.Label($"Buff: {currentBuff.buffName}", EditorStyles.boldLabel);
                GUILayout.Label($"ID: {currentBuff.buffID}", EditorStyles.miniLabel);
                GUILayout.Label($"Type: {currentBuff.buffType}", EditorStyles.miniLabel);
                GUILayout.Label($"Strength: {currentBuff.strength}", EditorStyles.miniLabel);
                GUILayout.Label($"Duration: {currentBuff.duration} seconds", EditorStyles.miniLabel);
                
                if (!string.IsNullOrEmpty(currentBuff.buffDescription))
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Description:", EditorStyles.boldLabel);
                    GUILayout.Label(currentBuff.buffDescription, EditorStyles.wordWrappedLabel);
                }
                
                GUILayout.Space(5);
                GUILayout.Label("Stat Modifiers:", EditorStyles.boldLabel);
                GUILayout.Label($"• Flat: {currentBuff.statModifiers.flatModifier}", EditorStyles.miniLabel);
                GUILayout.Label($"• Percentage: {currentBuff.statModifiers.percentageModifier:P1}", EditorStyles.miniLabel);
                
                if (currentBuff.tags != null && currentBuff.tags.Count > 0)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Tags:", EditorStyles.boldLabel);
                    GUILayout.Label(string.Join(", ", currentBuff.tags), EditorStyles.miniLabel);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        
        private void DrawBuffSaveControls()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Buff"))
                {
                    SaveBuff();
                }
                
                if (GUILayout.Button("Reset"))
                {
                    if (EditorUtility.DisplayDialog("Reset Buff", "Are you sure you want to reset all changes?", "Reset", "Cancel"))
                    {
                        currentBuff = new Buff(Buff.ID.HealthRegen, "Sample Buff", "Sample Description", Buff.BuffType.Temporary, 1f, 30f);
                    }
                }
                
                if (GUILayout.Button("Duplicate"))
                {
                    var newBuff = new Buff((Buff.ID)currentBuff.buffID, currentBuff.buffName + " (Copy)", currentBuff.buffDescription, currentBuff.buffType, currentBuff.strength, currentBuff.duration);
                    if (GameData.buffDatabase != null)
                    {
                        GameData.buffDatabase.Add(newBuff);
                        currentBuff = newBuff;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void SaveSkill()
        {
            if (GameData.skillDatabase == null)
                GameData.skillDatabase = new List<Skill>();
            
            // Find and update existing skill or add new one
            var existingIndex = GameData.skillDatabase.FindIndex(s => s.skillID == currentSkill.skillID);
            if (existingIndex >= 0)
            {
                GameData.skillDatabase[existingIndex] = currentSkill;
            }
            else
            {
                GameData.skillDatabase.Add(currentSkill);
            }
            
            isDirty = true;
            EditorUtility.DisplayDialog("Success", $"Skill '{currentSkill.skillType}' saved successfully!", "OK");
        }
        
        private void SaveBuff()
        {
            if (string.IsNullOrEmpty(currentBuff.buffName))
            {
                EditorUtility.DisplayDialog("Error", "Buff name cannot be empty!", "OK");
                return;
            }
            
            if (GameData.buffDatabase == null)
                GameData.buffDatabase = new List<Buff>();
            
            // Find and update existing buff or add new one
            var existingIndex = GameData.buffDatabase.FindIndex(b => b.buffID == currentBuff.buffID);
            if (existingIndex >= 0)
            {
                GameData.buffDatabase[existingIndex] = currentBuff;
            }
            else
            {
                GameData.buffDatabase.Add(currentBuff);
            }
            
            isDirty = true;
            EditorUtility.DisplayDialog("Success", $"Buff '{currentBuff.buffName}' saved successfully!", "OK");
        }
    }
}
