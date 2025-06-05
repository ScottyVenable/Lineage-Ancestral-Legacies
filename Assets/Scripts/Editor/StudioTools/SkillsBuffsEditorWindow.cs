using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Core;
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
        private readonly string[] skillTabNames = { "Basic Info", "Requirements", "Effects", "Progression", "Preview" };
        
        // Buffs Data
        private Buff currentBuff;
        private Vector2 buffScrollPosition;
        private string buffSearchFilter = "";
        private int selectedBuffTab = 0;
        private readonly string[] buffTabNames = { "Basic Info", "Effects", "Conditions", "Stacking", "Preview" };
        
        // Common
        private Vector2 listScrollPosition;
        
        [MenuItem("Lineage Studio/Content Creation/Skills & Buffs Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillsBuffsEditorWindow>("Skills & Buffs Editor");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            currentSkill = new Skill();
            currentBuff = new Buff();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Content list
                DrawContentList();
                
                // Right panel - Content editor
                if (currentContentType == ContentType.Skills)
                    DrawSkillEditor();
                else
                    DrawBuffEditor();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Skills & Buffs Editor", EditorStyles.boldLabel);
                
                GUILayout.FlexibleSpace();
                
                // Content type toggle
                ContentType newType = (ContentType)GUILayout.Toolbar((int)currentContentType, 
                    new string[] { "Skills", "Buffs" }, GUILayout.Width(200));
                
                if (newType != currentContentType)
                {
                    currentContentType = newType;
                    ResetCurrentContent();
                }
                
                if (GUILayout.Button("Save All", GUILayout.Width(80)))
                {
                    SaveAllContent();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawContentList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                // Search and controls
                EditorGUILayout.BeginHorizontal();
                {
                    if (currentContentType == ContentType.Skills)
                    {
                        skillSearchFilter = EditorGUILayout.TextField(skillSearchFilter, EditorStyles.toolbarSearchField);
                        if (GUILayout.Button("New Skill", GUILayout.Width(80)))
                            CreateNewSkill();
                    }
                    else
                    {
                        buffSearchFilter = EditorGUILayout.TextField(buffSearchFilter, EditorStyles.toolbarSearchField);
                        if (GUILayout.Button("New Buff", GUILayout.Width(80)))
                            CreateNewBuff();
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Content list
                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);
                {
                    if (currentContentType == ContentType.Skills)
                        DrawSkillsList();
                    else
                        DrawBuffsList();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSkillsList()
        {
            if (GameData.Skills == null) return;

            var filteredSkills = GameData.Skills.Where(s =>
                string.IsNullOrEmpty(skillSearchFilter) ||
                s.skillName.ToString().ToLower().Contains(skillSearchFilter.ToLower())).ToList();
            
            foreach (var skill in filteredSkills)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    bool isSelected = currentSkill.skillID == skill.skillID;
                    
                    if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                    {
                        currentSkill = CreateSkillCopy(skill);
                    }
                    
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.Label(skill.skillName.ToString(), EditorStyles.boldLabel);
                        GUILayout.Label($"Category: {skill.Category}", EditorStyles.miniLabel);
                        GUILayout.Label($"Level: {skill.Level} | XP: {skill.ExperiencePoints}", EditorStyles.miniLabel);
                    }
                    EditorGUILayout.EndVertical();
                    
                    if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Skill", 
                            $"Are you sure you want to delete the skill '{skill.skillName}'?", "Delete", "Cancel"))
                        {
                            GameData.Skills.Remove(skill);
                            if (currentSkill?.skillID == skill.skillID)
                                currentSkill = new Skill();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(2);
            }
        }
          private void DrawBuffsList()
        {
            if (GameData.Buffs == null) return;
            
            var filteredBuffs = GameData.Buffs.Where(b => 
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
                        currentBuff = CreateBuffCopy(buff);
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
                            GameData.Buffs.Remove(buff);
                            if (currentBuff.buffID == buff.buffID)
                                currentBuff = new Buff();
                        }
                    }
                }
                
                GUILayout.Space(2);
            }
        }
        
        private void DrawSkillEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                if (currentSkill == null)
                {
                    GUILayout.Label("Select a skill to edit or create a new one.", EditorStyles.centeredGreyMiniLabel);
                    return;
                }
                
                // Tab selection
                selectedSkillTab = GUILayout.Toolbar(selectedSkillTab, skillTabNames);
                
                skillScrollPosition = EditorGUILayout.BeginScrollView(skillScrollPosition);
                {
                    switch (selectedSkillTab)
                    {
                        case 0: DrawSkillBasicInfo(); break;
                        case 1: DrawSkillRequirements(); break;
                        case 2: DrawSkillEffects(); break;
                        case 3: DrawSkillProgression(); break;
                        case 4: DrawSkillPreview(); break;
                    }
                }
                EditorGUILayout.EndScrollView();
                
                DrawSkillSaveControls();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSkillBasicInfo()
        {
            GUILayout.Label("Basic Information", EditorStyles.boldLabel);

            currentSkill.skillName = (Skill.SkillType)EditorGUILayout.EnumPopup("Name", currentSkill.skillName);
            currentSkill.skillDescription = EditorGUILayout.TextArea(currentSkill.skillDescription, GUILayout.Height(60));

            // Category dropdown with presets
            EditorGUILayout.BeginHorizontal();
            {
                currentSkill.skillType = (Skill.SkillType)EditorGUILayout.EnumPopup("Skill Type", currentSkill.skillType);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                {
                    currentSkill.level = EditorGUILayout.IntField("Level", currentSkill.level);
                    currentSkill.experience = EditorGUILayout.IntField("XP", currentSkill.ExperiencePoints);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawSkillRequirements()
        {
            GUILayout.Label("Requirements", EditorStyles.boldLabel);
            
            // Prerequisite Skills
            GUILayout.Label("Prerequisite Skills:", EditorStyles.boldLabel);
            for (int i = 0; i < currentSkill.PrerequisiteSkills.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentSkill.PrerequisiteSkills[i] = EditorGUILayout.TextField($"Skill {i + 1}", currentSkill.PrerequisiteSkills[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentSkill.PrerequisiteSkills.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Prerequisite Skill"))
                currentSkill.PrerequisiteSkills.Add("");
            
            GUILayout.Space(10);
            
            // Stat Requirements
            GUILayout.Label("Stat Requirements:", EditorStyles.boldLabel);
            if (currentSkill.StatRequirements == null)
                currentSkill.StatRequirements = new Dictionary<string, int>();
            
            var statKeys = currentSkill.StatRequirements.Keys.ToList();
            for (int i = 0; i < statKeys.Count; i++)
            {
                string stat = statKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(stat, GUILayout.Width(100));
                    currentSkill.StatRequirements[stat] = EditorGUILayout.IntField(currentSkill.StatRequirements[stat]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentSkill.StatRequirements.Remove(stat);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add Stat Requirement"))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Strength"), false, () => AddStatRequirement("Strength"));
                    menu.AddItem(new GUIContent("Dexterity"), false, () => AddStatRequirement("Dexterity"));
                    menu.AddItem(new GUIContent("Intelligence"), false, () => AddStatRequirement("Intelligence"));
                    menu.AddItem(new GUIContent("Wisdom"), false, () => AddStatRequirement("Wisdom"));
                    menu.AddItem(new GUIContent("Constitution"), false, () => AddStatRequirement("Constitution"));
                    menu.AddItem(new GUIContent("Charisma"), false, () => AddStatRequirement("Charisma"));
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSkillEffects()
        {
            GUILayout.Label("Skill Effects", EditorStyles.boldLabel);
            
            // Stat Modifiers
            GUILayout.Label("Stat Modifiers:", EditorStyles.boldLabel);
            if (currentSkill.StatModifiers == null)
                currentSkill.StatModifiers = new Dictionary<string, float>();
            
            var modifierKeys = currentSkill.StatModifiers.Keys.ToList();
            for (int i = 0; i < modifierKeys.Count; i++)
            {
                string stat = modifierKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(stat, GUILayout.Width(100));
                    currentSkill.StatModifiers[stat] = EditorGUILayout.FloatField(currentSkill.StatModifiers[stat]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentSkill.StatModifiers.Remove(stat);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Stat Modifier"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Damage Bonus"), false, () => AddStatModifier("DamageBonus"));
                menu.AddItem(new GUIContent("Accuracy Bonus"), false, () => AddStatModifier("AccuracyBonus"));
                menu.AddItem(new GUIContent("Defense Bonus"), false, () => AddStatModifier("DefenseBonus"));
                menu.AddItem(new GUIContent("Speed Bonus"), false, () => AddStatModifier("SpeedBonus"));
                menu.ShowAsContext();
            }
            
            GUILayout.Space(10);
            
            // Applied Buffs
            GUILayout.Label("Applied Buffs:", EditorStyles.boldLabel);
            for (int i = 0; i < currentSkill.AppliedBuffs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentSkill.AppliedBuffs[i] = EditorGUILayout.TextField($"Buff {i + 1}", currentSkill.AppliedBuffs[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentSkill.AppliedBuffs.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Applied Buff"))
                currentSkill.AppliedBuffs.Add("");
        }
        
        private void DrawSkillProgression()
        {
            GUILayout.Label("Skill Progression", EditorStyles.boldLabel);
            
            currentSkill.MaxLevel = EditorGUILayout.IntField("Max Level", currentSkill.MaxLevel);
            currentSkill.XpToNextLevel = EditorGUILayout.IntField("XP to Next Level", currentSkill.XpToNextLevel);
            
            GUILayout.Space(10);
            
            // XP Scaling
            GUILayout.Label("XP Scaling:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Define how XP requirements scale with level", MessageType.Info);
            
            // Simple progression system
            currentSkill.BaseXpCost = EditorGUILayout.IntField("Base XP Cost", currentSkill.BaseXpCost);
            currentSkill.XpScalingFactor = EditorGUILayout.FloatField("XP Scaling Factor", currentSkill.XpScalingFactor);
            
            GUILayout.Space(10);
            
            // Level Benefits Preview
            GUILayout.Label("Level Benefits Preview:", EditorStyles.boldLabel);
            for (int level = 1; level <= Mathf.Min(currentSkill.MaxLevel, 10); level++)
            {
                int xpRequired = Mathf.RoundToInt(currentSkill.BaseXpCost * Mathf.Pow(currentSkill.XpScalingFactor, level - 1));
                GUILayout.Label($"Level {level}: {xpRequired} XP required", EditorStyles.miniLabel);
            }
        }
        
        private void DrawSkillPreview()
        {
            GUILayout.Label("Skill Preview", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox($"Skill: {currentskill.skillName}\n" +
            EditorGUILayout.HelpBox($"Skill: {currentSkill.skillName}\n" +
                                  $"Category: {currentSkill.Category}\n" +
                                  $"Level: {currentSkill.Level}/{currentSkill.MaxLevel}\n" +
                                  $"XP: {currentSkill.ExperiencePoints}/{currentSkill.XpToNextLevel}\n" +
                                  $"Type: {(currentSkill.IsActive ? "Active" : "Passive")}\n" +
                                  (currentSkill.IsActive ? $"Mana Cost: {currentSkill.ManaCost}\nCooldown: {currentSkill.Cooldown}s\n" : "") +
                                  $"Description: {currentSkill.Description}", MessageType.Info);
            if (currentSkill.PrerequisiteSkills.Count > 0)
            {
                GUILayout.Label("Prerequisites:", EditorStyles.boldLabel);
                foreach (var prereq in currentSkill.PrerequisiteSkills.Where(p => !string.IsNullOrEmpty(p)))
                {
                    GUILayout.Label($"• {prereq}", EditorStyles.miniLabel);
                }
            }
            
            if (currentSkill.StatModifiers?.Count > 0)
            {
                GUILayout.Label("Stat Modifiers:", EditorStyles.boldLabel);
                foreach (var modifier in currentSkill.StatModifiers)
                {
                    GUILayout.Label($"• {modifier.Key}: +{modifier.Value}", EditorStyles.miniLabel);
                }
            }
        }
        
        private void DrawBuffEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                if (currentBuff == null)
                {
                    GUILayout.Label("Select a buff to edit or create a new one.", EditorStyles.centeredGreyMiniLabel);
                    return;
                }
                
                // Tab selection
                selectedBuffTab = GUILayout.Toolbar(selectedBuffTab, buffTabNames);
                
                buffScrollPosition = EditorGUILayout.BeginScrollView(buffScrollPosition);
                {
                    switch (selectedBuffTab)
                    {
                        case 0: DrawBuffBasicInfo(); break;
                        case 1: DrawBuffEffects(); break;
                        case 2: DrawBuffConditions(); break;
                        case 3: DrawBuffStacking(); break;
                        case 4: DrawBuffPreview(); break;
                    }
                }
                EditorGUILayout.EndScrollView();
                
                DrawBuffSaveControls();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBuffBasicInfo()
        private void DrawBuffBasicInfo()
        {
            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            
            currentBuff.buffName = EditorGUILayout.TextField("Name", currentBuff.buffName);
            currentBuff.buffDescription = EditorGUILayout.TextArea(currentBuff.buffDescription, GUILayout.Height(60));
            
            currentBuff.buffType = (BuffType)EditorGUILayout.EnumPopup("Type", currentBuff.buffType);
            
            EditorGUILayout.BeginHorizontal();
            {
                currentBuff.duration = EditorGUILayout.FloatField("Duration (seconds)", currentBuff.duration);
            }
            EditorGUILayout.EndHorizontal();
            
            currentBuff.strength = EditorGUILayout.FloatField("Strength", currentBuff.strength);
        }
        private void DrawBuffEffects()
        {
            GUILayout.Label("Buff Effects", EditorStyles.boldLabel);
            
            // Stat Modifiers
            GUILayout.Label("Stat Modifiers:", EditorStyles.boldLabel);
            if (currentBuff.StatModifiers == null)
                currentBuff.StatModifiers = new Dictionary<string, float>();
            
            var modifierKeys = currentBuff.StatModifiers.Keys.ToList();
            for (int i = 0; i < modifierKeys.Count; i++)
            {
                string stat = modifierKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(stat, GUILayout.Width(100));
                    currentBuff.StatModifiers[stat] = EditorGUILayout.FloatField(currentBuff.StatModifiers[stat]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentBuff.StatModifiers.Remove(stat);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Stat Modifier"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Health"), false, () => AddBuffStatModifier("Health"));
                menu.AddItem(new GUIContent("Mana"), false, () => AddBuffStatModifier("Mana"));
                menu.AddItem(new GUIContent("Strength"), false, () => AddBuffStatModifier("Strength"));
                menu.AddItem(new GUIContent("Dexterity"), false, () => AddBuffStatModifier("Dexterity"));
                menu.AddItem(new GUIContent("Intelligence"), false, () => AddBuffStatModifier("Intelligence"));
                menu.AddItem(new GUIContent("Speed"), false, () => AddBuffStatModifier("Speed"));
                menu.AddItem(new GUIContent("Defense"), false, () => AddBuffStatModifier("Defense"));
                menu.ShowAsContext();
            }
            
            GUILayout.Space(10);
            
            // Tick Effects
            GUILayout.Label("Tick Effects:", EditorStyles.boldLabel);
            currentBuff.HealthPerTick = EditorGUILayout.FloatField("Health per Tick", currentBuff.HealthPerTick);
            currentBuff.ManaPerTick = EditorGUILayout.FloatField("Mana per Tick", currentBuff.ManaPerTick);
        }
        
        private void DrawBuffConditions()
        {
            GUILayout.Label("Application Conditions", EditorStyles.boldLabel);
            
            // Source Conditions
            GUILayout.Label("Source Requirements:", EditorStyles.boldLabel);
            if (currentBuff.SourceRequirements == null)
                currentBuff.SourceRequirements = new List<string>();
            
            for (int i = 0; i < currentBuff.SourceRequirements.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentBuff.SourceRequirements[i] = EditorGUILayout.TextField($"Requirement {i + 1}", currentBuff.SourceRequirements[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentBuff.SourceRequirements.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Source Requirement"))
                currentBuff.SourceRequirements.Add("");
            
            GUILayout.Space(10);
            
            // Target Conditions
            GUILayout.Label("Target Conditions:", EditorStyles.boldLabel);
            if (currentBuff.TargetConditions == null)
                currentBuff.TargetConditions = new List<string>();
            
            for (int i = 0; i < currentBuff.TargetConditions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentBuff.TargetConditions[i] = EditorGUILayout.TextField($"Condition {i + 1}", currentBuff.TargetConditions[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentBuff.TargetConditions.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Target Condition"))
                currentBuff.TargetConditions.Add("");
        }
        
        private void DrawBuffStacking()
        {
            GUILayout.Label("Stacking Rules", EditorStyles.boldLabel);
            
            currentBuff.MaxStacks = EditorGUILayout.IntField("Max Stacks", currentBuff.MaxStacks);
            currentBuff.StackingType = (StackingType)EditorGUILayout.EnumPopup("Stacking Type", currentBuff.StackingType);
            
            EditorGUILayout.HelpBox(GetStackingTypeDescription(currentBuff.StackingType), MessageType.Info);
            
            if (currentBuff.MaxStacks > 1)
            {
                currentBuff.StackDurationRefresh = EditorGUILayout.Toggle("Refresh Duration on Stack", currentBuff.StackDurationRefresh);
                currentBuff.StackEffectMultiplier = EditorGUILayout.FloatField("Effect Multiplier per Stack", currentBuff.StackEffectMultiplier);
            }
        }
        
        private void DrawBuffPreview()
        {
            GUILayout.Label("Buff Preview", EditorStyles.boldLabel);
            
            string durationText = currentBuff.IsPermanent ? "Permanent" : $"{currentBuff.Duration} seconds";
            string typeText = currentBuff.IsDebuff ? "Debuff" : "Buff";
            
            EditorGUILayout.HelpBox($"Name: {currentBuff.Name}\n" +
                                  $"Type: {typeText} ({currentBuff.Type})\n" +
                                  $"Duration: {durationText}\n" +
                                  $"Max Stacks: {currentBuff.MaxStacks}\n" +
                                  $"Tick Interval: {currentBuff.TickInterval}s\n" +
                                  $"Description: {currentBuff.Description}", MessageType.Info);
            
            if (currentBuff.StatModifiers?.Count > 0)
            {
                GUILayout.Label("Stat Effects:", EditorStyles.boldLabel);
                foreach (var modifier in currentBuff.StatModifiers)
                {
                    string sign = modifier.Value >= 0 ? "+" : "";
                    GUILayout.Label($"• {modifier.Key}: {sign}{modifier.Value}", EditorStyles.miniLabel);
                }
            }
            
            if (currentBuff.HealthPerTick != 0 || currentBuff.ManaPerTick != 0)
            {
                GUILayout.Label("Tick Effects:", EditorStyles.boldLabel);
                if (currentBuff.HealthPerTick != 0)
                    GUILayout.Label($"• Health: {(currentBuff.HealthPerTick >= 0 ? "+" : "")}{currentBuff.HealthPerTick} per tick", EditorStyles.miniLabel);
                if (currentBuff.ManaPerTick != 0)
                    GUILayout.Label($"• Mana: {(currentBuff.ManaPerTick >= 0 ? "+" : "")}{currentBuff.ManaPerTick} per tick", EditorStyles.miniLabel);
            }
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
                        currentSkill = new Skill();
                    }
                }
                
                if (GUILayout.Button("Duplicate"))
                {
                    var newSkill = CreateSkillCopy(currentSkill);
                    newSkill.ID = System.Guid.NewGuid().ToString();
                    newSkill.skillName = currentSkill.skillName; // Enums can't be concatenated
                    currentSkill = newSkill;
                }
            }
            EditorGUILayout.EndHorizontal();
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
                        currentBuff = new Buff();
                    }
                }
                
                if (GUILayout.Button("Duplicate"))
                {
                    var newBuff = CreateBuffCopy(currentBuff);
                    newBuff.ID = System.Guid.NewGuid().ToString();
                    newBuff.Name += " (Copy)";
                    currentBuff = newBuff;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewSkill()
        {
            currentSkill = new Skill
            {
                skillID = System.Guid.NewGuid().GetHashCode(),
                skillName = "Skill Name",
                skillDescription = "Description of the new skill.",
                skillType = Skill.SkillType.Swordsmanship, //todo: make this add to the enum
                level = 1,
                maxLevel = 100,
                experience = 0,
                xpToNextLevel = 100,
                baseXpCost = 100,
                xpScalingFactor = 1.2f,
                isActive = true
            };
            selectedSkillTab = 0;
        }
        private void CreateNewSkill()
        {
            currentSkill = new Skill
            {
                skillID = System.Guid.NewGuid().GetHashCode(),
                skillName = Skill.SkillType.Swordsmanship,
                Category = "Combat",
                Level = 1,
                MaxLevel = 100,
                BaseXpCost = 100,
                XpScalingFactor = 1.2f
            };
            selectedSkillTab = 0;
        }
                Name = "New Buff",
        private void CreateNewBuff()
        {
            currentBuff = new Buff
            {
                buffID = System.Guid.NewGuid().GetHashCode(),
                buffName = "New Buff",
                buffType = BuffType.Temporary,
                duration = 30f,
                strength = 1f
            };
            selectedBuffTab = 0;
        }
            if (existingSkill != null)
            {
                // Update existing
                int index = GameData.Skills.IndexOf(existingSkill);
                GameData.Skills[index] = CreateSkillCopy(currentSkill);
            }
            else
            {
                // Add new
                if (GameData.Skills == null)
                    GameData.Skills = new List<Skill>();
                GameData.Skills.Add(CreateSkillCopy(currentSkill));
            }
            
            EditorUtility.DisplayDialog("Success", $"Skill '{currentskill.skillName}' saved successfully!", "OK");
        }
        
        private void SaveBuff()
            EditorUtility.DisplayDialog("Success", $"Skill '{currentSkill.skillName}' saved successfully!", "OK");
            if (string.IsNullOrEmpty(currentBuff.Name))
            {
            EditorUtility.DisplayDialog("Success", $"Skill '{currentSkill.skillName}' saved successfully!", "OK");
                return;
            }
        private void SaveBuff()
        {
            if (string.IsNullOrEmpty(currentBuff.buffName))
            {
                EditorUtility.DisplayDialog("Error", "Buff name cannot be empty!", "OK");
                return;
            }
            
            var existingBuff = GameData.Buffs?.FirstOrDefault(b => b.buffID == currentBuff.buffID);
            if (existingBuff != null)
            {
                // Update existing
                int index = GameData.Buffs.IndexOf(existingBuff);
                GameData.Buffs[index] = CreateBuffCopy(currentBuff);
            }
            else
            {
                // Add new
                if (GameData.Buffs == null)
                    GameData.Buffs = new List<Buff>();
                GameData.Buffs.Add(CreateBuffCopy(currentBuff));
            }
            
            EditorUtility.DisplayDialog("Success", $"Buff '{currentBuff.buffName}' saved successfully!", "OK");
        }
        private void ResetCurrentContent()
        {
            if (currentContentType == ContentType.Skills)
            {
                currentSkill = new Skill();
                selectedSkillTab = 0;
            }
            else
            {
                currentBuff = new Buff();
                selectedBuffTab = 0;
            }
        }
        
        private void AddStatRequirement(string statName)
        {
            if (!currentSkill.StatRequirements.ContainsKey(statName))
                currentSkill.StatRequirements[statName] = 10;
        }
        
        private void AddStatModifier(string statName)
        {
            if (!currentSkill.StatModifiers.ContainsKey(statName))
                currentSkill.StatModifiers[statName] = 1.0f;
        }
        
        private void AddBuffStatModifier(string statName)
        {
            if (!currentBuff.StatModifiers.ContainsKey(statName))
                currentBuff.StatModifiers[statName] = 5.0f;
        }
        
        private string GetStackingTypeDescription(StackingType stackingType)
        {
            switch (stackingType)
            {
                case StackingType.None:
                    return "Does not stack. New applications replace the old one.";
                case StackingType.Duration:
                    return "Stacks duration only. Multiple applications extend the duration.";
                case StackingType.Intensity:
                    return "Stacks intensity only. Effects multiply with each stack.";
                case StackingType.Both:
                    return "Stacks both duration and intensity.";
                default:
                    return "";
            }
        }
        
        private Skill CreateSkillCopy(Skill original)
        {
            return new Skill
            {
                ID = original.ID,
                Name = original.Name,
                Description = original.Description,
                Category = original.Category,
                Level = original.Level,
                MaxLevel = original.MaxLevel,
                ExperiencePoints = original.ExperiencePoints,
                XpToNextLevel = original.XpToNextLevel,
                BaseXpCost = original.BaseXpCost,
                XpScalingFactor = original.XpScalingFactor,
                IsActive = original.IsActive,
                ManaCost = original.ManaCost,
                Cooldown = original.Cooldown,
                PrerequisiteSkills = new List<string>(original.PrerequisiteSkills ?? new List<string>()),
                StatRequirements = new Dictionary<string, int>(original.StatRequirements ?? new Dictionary<string, int>()),
                StatModifiers = new Dictionary<string, float>(original.StatModifiers ?? new Dictionary<string, float>()),
                AppliedBuffs = new List<string>(original.AppliedBuffs ?? new List<string>())
            };
        }
        
        private Buff CreateBuffCopy(Buff original)
        {
            return new Buff
            {
                ID = original.ID,
                Name = original.Name,
                Description = original.Description,
                Type = original.Type,
                Duration = original.Duration,
                IsPermanent = original.IsPermanent,
                TickInterval = original.TickInterval,
                IsDebuff = original.IsDebuff,
                MaxStacks = original.MaxStacks,
                StackingType = original.StackingType,
                StackDurationRefresh = original.StackDurationRefresh,
                StackEffectMultiplier = original.StackEffectMultiplier,
                StatModifiers = new Dictionary<string, float>(original.StatModifiers ?? new Dictionary<string, float>()),
                HealthPerTick = original.HealthPerTick,
                ManaPerTick = original.ManaPerTick,
                SourceRequirements = new List<string>(original.SourceRequirements ?? new List<string>()),
                TargetConditions = new List<string>(original.TargetConditions ?? new List<string>())
            };
        }
    }
}
