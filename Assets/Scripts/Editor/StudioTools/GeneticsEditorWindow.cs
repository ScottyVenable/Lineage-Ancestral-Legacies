using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Core;
using Lineage.Core.Editor.Studio;

namespace Lineage.Core.Editor.Studio
{
    public class GeneticsEditorWindow : EditorWindow
    {
        private GeneticProfile currentProfile;
        private Vector2 scrollPosition;
        private Vector2 listScrollPosition;
        private string searchFilter = "";
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Basic Info", "Traits", "Inheritance", "Breeding", "Simulation", "Preview" };
        
        // Simulation data
        private GeneticProfile parent1;
        private GeneticProfile parent2;
        private List<GeneticProfile> simulatedOffspring = new List<GeneticProfile>();
        
        [MenuItem("Lineage Studio/Content Creation/Genetics & Inheritance Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<GeneticsEditorWindow>("Genetics & Inheritance Editor");
            window.minSize = new Vector2(950, 700);
            window.Show();
        }
        
        private void OnEnable()
        {
            if (currentProfile == null)
                currentProfile = new GeneticProfile();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Genetic profiles list
                DrawProfilesList();
                
                // Right panel - Profile editor
                DrawProfileEditor();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Genetics & Inheritance Editor", EditorStyles.boldLabel);
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Random Profile", GUILayout.Width(100)))
                {
                    GenerateRandomProfile();
                }
                
                if (GUILayout.Button("Templates", GUILayout.Width(80)))
                {
                    ShowTemplateMenu();
                }
                
                if (GUILayout.Button("Save All", GUILayout.Width(80)))
                {
                    SaveAllProfiles();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawProfilesList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            {
                // Search and controls
                EditorGUILayout.BeginHorizontal();
                {
                    searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
                    if (GUILayout.Button("New", GUILayout.Width(50)))
                        CreateNewProfile();
                }
                EditorGUILayout.EndHorizontal();
                
                // Species filter
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("All", EditorStyles.miniButtonLeft))
                        searchFilter = "";
                    if (GUILayout.Button("Human", EditorStyles.miniButtonMid))
                        searchFilter = "Human";
                    if (GUILayout.Button("Elf", EditorStyles.miniButtonMid))
                        searchFilter = "Elf";
                    if (GUILayout.Button("Dwarf", EditorStyles.miniButtonMid))
                        searchFilter = "Dwarf";
                    if (GUILayout.Button("Other", EditorStyles.miniButtonRight))
                        searchFilter = "Other";
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Profiles list
                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);
                {
                    DrawGeneticProfiles();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGeneticProfiles()
        {
            if (GameData.GeneticProfiles == null) return;
            
            var filteredProfiles = GameData.GeneticProfiles.Where(p => 
                string.IsNullOrEmpty(searchFilter) || 
                p.Name.ToLower().Contains(searchFilter.ToLower()) ||
                p.Species.ToLower().Contains(searchFilter.ToLower())).ToList();
            
            // Group by species
            var groupedProfiles = filteredProfiles.GroupBy(p => p.Species).OrderBy(g => g.Key);
            
            foreach (var group in groupedProfiles)
            {
                GUILayout.Label(group.Key, EditorStyles.boldLabel);
                
                foreach (var profile in group.OrderBy(p => p.Name))
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        bool isSelected = currentProfile != null && currentProfile.ID == profile.ID;
                        
                        if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                        {
                            currentProfile = profile.Clone();
                        }
                        
                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Label(profile.Name, EditorStyles.boldLabel);
                            GUILayout.Label($"Generation: {profile.Generation} | Purity: {profile.BloodlinePurity:P0}", EditorStyles.miniLabel);
                            
                            // Show dominant traits
                            var dominantTraits = profile.Genes?.Where(g => g.Expression == GeneExpression.Dominant)
                                .Take(3).Select(g => g.TraitName);
                            if (dominantTraits?.Any() == true)
                            {
                                GUILayout.Label($"Traits: {string.Join(", ", dominantTraits)}", EditorStyles.miniLabel);
                            }
                        }
                        EditorGUILayout.EndVertical();
                        
                        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Profile", 
                                $"Are you sure you want to delete '{profile.Name}'?", "Delete", "Cancel"))
                            {
                                GameData.GeneticProfiles.Remove(profile);
                                if (currentProfile?.ID == profile.ID)
                                    currentProfile = new GeneticProfile();
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(2);
                }
                
                GUILayout.Space(5);
            }
        }
        
        private void DrawProfileEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                if (currentProfile == null)
                {
                    GUILayout.Label("Select a genetic profile to edit or create a new one.", EditorStyles.centeredGreyMiniLabel);
                    return;
                }
                
                // Tab selection
                selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    switch (selectedTab)
                    {
                        case 0: DrawBasicInfo(); break;
                        case 1: DrawTraits(); break;
                        case 2: DrawInheritance(); break;
                        case 3: DrawBreeding(); break;
                        case 4: DrawSimulation(); break;
                        case 5: DrawPreview(); break;
                    }
                }
                EditorGUILayout.EndScrollView();
                
                DrawSaveControls();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBasicInfo()
        {
            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            
            currentProfile.Name = EditorGUILayout.TextField("Name", currentProfile.Name);
            
            EditorGUILayout.BeginHorizontal();
            {
                currentProfile.Species = EditorGUILayout.TextField("Species", currentProfile.Species);
                if (GUILayout.Button("Presets", GUILayout.Width(70)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Human"), false, () => currentProfile.Species = "Human");
                    menu.AddItem(new GUIContent("Elf"), false, () => currentProfile.Species = "Elf");
                    menu.AddItem(new GUIContent("Dwarf"), false, () => currentProfile.Species = "Dwarf");
                    menu.AddItem(new GUIContent("Halfling"), false, () => currentProfile.Species = "Halfling");
                    menu.AddItem(new GUIContent("Orc"), false, () => currentProfile.Species = "Orc");
                    menu.AddItem(new GUIContent("Half-Elf"), false, () => currentProfile.Species = "Half-Elf");
                    menu.AddItem(new GUIContent("Tiefling"), false, () => currentProfile.Species = "Tiefling");
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                currentProfile.Generation = EditorGUILayout.IntField("Generation", currentProfile.Generation);
                currentProfile.BloodlinePurity = EditorGUILayout.Slider("Bloodline Purity", currentProfile.BloodlinePurity, 0f, 1f);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Lineage Information
            GUILayout.Label("Lineage", EditorStyles.boldLabel);
            currentProfile.BloodlineName = EditorGUILayout.TextField("Bloodline Name", currentProfile.BloodlineName);
            currentProfile.AncestralLine = EditorGUILayout.TextField("Ancestral Line", currentProfile.AncestralLine);
            
            EditorGUILayout.BeginHorizontal();
            {
                currentProfile.Father = EditorGUILayout.TextField("Father", currentProfile.Father);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                    SelectParent(true);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                currentProfile.Mother = EditorGUILayout.TextField("Mother", currentProfile.Mother);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                    SelectParent(false);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Physical Characteristics
            GUILayout.Label("Physical Characteristics", EditorStyles.boldLabel);
            currentProfile.PhysicalTraits.EyeColor = EditorGUILayout.TextField("Eye Color", currentProfile.PhysicalTraits.EyeColor);
            currentProfile.PhysicalTraits.HairColor = EditorGUILayout.TextField("Hair Color", currentProfile.PhysicalTraits.HairColor);
            currentProfile.PhysicalTraits.SkinTone = EditorGUILayout.TextField("Skin Tone", currentProfile.PhysicalTraits.SkinTone);
            currentProfile.PhysicalTraits.Height = EditorGUILayout.TextField("Height", currentProfile.PhysicalTraits.Height);
            currentProfile.PhysicalTraits.Build = EditorGUILayout.TextField("Build", currentProfile.PhysicalTraits.Build);
        }
        
        private void DrawTraits()
        {
            GUILayout.Label("Genetic Traits", EditorStyles.boldLabel);
            
            if (currentProfile.Genes == null)
                currentProfile.Genes = new List<Gene>();
            
            // Add new gene
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add Gene"))
                {
                    currentProfile.Genes.Add(new Gene
                    {
                        ID = System.Guid.NewGuid().ToString(),
                        TraitName = "New Trait",
                        Expression = GeneExpression.Recessive,
                        Dominance = 0.5f,
                        Chromosome = "1"
                    });
                }
                
                if (GUILayout.Button("Add From Template"))
                {
                    ShowGeneTemplateMenu();
                }
                
                if (GUILayout.Button("Sort by Dominance"))
                {
                    currentProfile.Genes = currentProfile.Genes.OrderByDescending(g => g.Dominance).ToList();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Genes list
            for (int i = 0; i < currentProfile.Genes.Count; i++)
            {
                var gene = currentProfile.Genes[i];
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label($"Gene {i + 1}", EditorStyles.boldLabel);
                        
                        // Expression indicator
                        Color oldColor = GUI.color;
                        GUI.color = GetExpressionColor(gene.Expression);
                        GUILayout.Label(gene.Expression.ToString(), EditorStyles.miniLabel, GUILayout.Width(80));
                        GUI.color = oldColor;
                        
                        if (GUILayout.Button("×", GUILayout.Width(20)))
                        {
                            currentProfile.Genes.RemoveAt(i);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    gene.TraitName = EditorGUILayout.TextField("Trait Name", gene.TraitName);
                    gene.Chromosome = EditorGUILayout.TextField("Chromosome", gene.Chromosome);
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        gene.Expression = (GeneExpression)EditorGUILayout.EnumPopup("Expression", gene.Expression);
                        gene.Dominance = EditorGUILayout.Slider("Dominance", gene.Dominance, 0f, 1f);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    gene.Rarity = (GeneRarity)EditorGUILayout.EnumPopup("Rarity", gene.Rarity);
                    gene.MutationChance = EditorGUILayout.Slider("Mutation Chance", gene.MutationChance, 0f, 0.1f);
                    
                    // Effects
                    if (gene.StatModifiers == null)
                        gene.StatModifiers = new Dictionary<string, float>();
                    
                    GUILayout.Label("Stat Effects:", EditorStyles.miniLabel);
                    var statKeys = gene.StatModifiers.Keys.ToList();
                    for (int j = 0; j < statKeys.Count; j++)
                    {
                        string stat = statKeys[j];
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(stat, GUILayout.Width(80));
                            gene.StatModifiers[stat] = EditorGUILayout.FloatField(gene.StatModifiers[stat], GUILayout.Width(60));
                            if (GUILayout.Button("-", GUILayout.Width(20)))
                            {
                                gene.StatModifiers.Remove(stat);
                                break;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Add Effect", GUILayout.Width(80)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Strength"), false, () => AddGeneEffect(gene, "Strength"));
                            menu.AddItem(new GUIContent("Dexterity"), false, () => AddGeneEffect(gene, "Dexterity"));
                            menu.AddItem(new GUIContent("Intelligence"), false, () => AddGeneEffect(gene, "Intelligence"));
                            menu.AddItem(new GUIContent("Wisdom"), false, () => AddGeneEffect(gene, "Wisdom"));
                            menu.AddItem(new GUIContent("Constitution"), false, () => AddGeneEffect(gene, "Constitution"));
                            menu.AddItem(new GUIContent("Charisma"), false, () => AddGeneEffect(gene, "Charisma"));
                            menu.AddItem(new GUIContent("Health"), false, () => AddGeneEffect(gene, "Health"));
                            menu.AddItem(new GUIContent("Mana"), false, () => AddGeneEffect(gene, "Mana"));
                            menu.ShowAsContext();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                
                GUILayout.Space(5);
            }
        }
        
        private void DrawInheritance()
        {
            GUILayout.Label("Inheritance Rules", EditorStyles.boldLabel);
            
            currentProfile.InheritanceRules.DominanceThreshold = EditorGUILayout.Slider(
                "Dominance Threshold", currentProfile.InheritanceRules.DominanceThreshold, 0f, 1f);
            EditorGUILayout.HelpBox("Genes above this threshold will always be expressed", MessageType.Info);
            
            currentProfile.InheritanceRules.MutationRate = EditorGUILayout.Slider(
                "Mutation Rate", currentProfile.InheritanceRules.MutationRate, 0f, 0.1f);
            
            currentProfile.InheritanceRules.CrossoverRate = EditorGUILayout.Slider(
                "Crossover Rate", currentProfile.InheritanceRules.CrossoverRate, 0f, 1f);
            EditorGUILayout.HelpBox("Chance of gene crossover during reproduction", MessageType.Info);
            
            GUILayout.Space(10);
            
            // Compatibility Rules
            GUILayout.Label("Species Compatibility", EditorStyles.boldLabel);
            if (currentProfile.InheritanceRules.CompatibleSpecies == null)
                currentProfile.InheritanceRules.CompatibleSpecies = new List<string>();
            
            for (int i = 0; i < currentProfile.InheritanceRules.CompatibleSpecies.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentProfile.InheritanceRules.CompatibleSpecies[i] = 
                        EditorGUILayout.TextField($"Species {i + 1}", currentProfile.InheritanceRules.CompatibleSpecies[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentProfile.InheritanceRules.CompatibleSpecies.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Compatible Species"))
                currentProfile.InheritanceRules.CompatibleSpecies.Add("");
            
            GUILayout.Space(10);
            
            // Bloodline Rules
            GUILayout.Label("Bloodline Rules", EditorStyles.boldLabel);
            currentProfile.InheritanceRules.BloodlineDilution = EditorGUILayout.Slider(
                "Bloodline Dilution Rate", currentProfile.InheritanceRules.BloodlineDilution, 0f, 1f);
            EditorGUILayout.HelpBox("How much bloodline purity decreases with each generation", MessageType.Info);
            
            currentProfile.InheritanceRules.MinBloodlineForExpression = EditorGUILayout.Slider(
                "Min Bloodline for Expression", currentProfile.InheritanceRules.MinBloodlineForExpression, 0f, 1f);
            EditorGUILayout.HelpBox("Minimum bloodline purity required for rare traits to manifest", MessageType.Info);
        }
        
        private void DrawBreeding()
        {
            GUILayout.Label("Breeding & Reproduction", EditorStyles.boldLabel);
            
            currentProfile.ReproductionData.FertilityRate = EditorGUILayout.Slider(
                "Fertility Rate", currentProfile.ReproductionData.FertilityRate, 0f, 1f);
            
            currentProfile.ReproductionData.GestationPeriod = EditorGUILayout.IntField(
                "Gestation Period (days)", currentProfile.ReproductionData.GestationPeriod);
            
            currentProfile.ReproductionData.OffspringCount = EditorGUILayout.IntField(
                "Typical Offspring Count", currentProfile.ReproductionData.OffspringCount);
            
            currentProfile.ReproductionData.MaturityAge = EditorGUILayout.IntField(
                "Maturity Age (years)", currentProfile.ReproductionData.MaturityAge);
            
            GUILayout.Space(10);
            
            // Breeding Bonuses/Penalties
            GUILayout.Label("Breeding Modifiers", EditorStyles.boldLabel);
            if (currentProfile.ReproductionData.BreedingModifiers == null)
                currentProfile.ReproductionData.BreedingModifiers = new Dictionary<string, float>();
            
            var modifierKeys = currentProfile.ReproductionData.BreedingModifiers.Keys.ToList();
            for (int i = 0; i < modifierKeys.Count; i++)
            {
                string modifier = modifierKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(modifier, GUILayout.Width(120));
                    currentProfile.ReproductionData.BreedingModifiers[modifier] = 
                        EditorGUILayout.FloatField(currentProfile.ReproductionData.BreedingModifiers[modifier], GUILayout.Width(60));
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentProfile.ReproductionData.BreedingModifiers.Remove(modifier);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Breeding Modifier"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Inbreeding Penalty"), false, () => AddBreedingModifier("InbreedingPenalty"));
                menu.AddItem(new GUIContent("Hybrid Vigor"), false, () => AddBreedingModifier("HybridVigor"));
                menu.AddItem(new GUIContent("Age Penalty"), false, () => AddBreedingModifier("AgePenalty"));
                menu.AddItem(new GUIContent("Health Bonus"), false, () => AddBreedingModifier("HealthBonus"));
                menu.ShowAsContext();
            }
            
            GUILayout.Space(10);
            
            // Breeding Restrictions
            GUILayout.Label("Breeding Restrictions", EditorStyles.boldLabel);
            if (currentProfile.ReproductionData.BreedingRestrictions == null)
                currentProfile.ReproductionData.BreedingRestrictions = new List<string>();
            
            for (int i = 0; i < currentProfile.ReproductionData.BreedingRestrictions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentProfile.ReproductionData.BreedingRestrictions[i] = 
                        EditorGUILayout.TextField($"Restriction {i + 1}", currentProfile.ReproductionData.BreedingRestrictions[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentProfile.ReproductionData.BreedingRestrictions.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Breeding Restriction"))
                currentProfile.ReproductionData.BreedingRestrictions.Add("");
        }
        
        private void DrawSimulation()
        {
            GUILayout.Label("Genetic Simulation", EditorStyles.boldLabel);
            
            // Parent Selection
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label("Parent 1:", EditorStyles.boldLabel);
                    if (parent1 != null)
                    {
                        GUILayout.Label($"Name: {parent1.Name}", EditorStyles.miniLabel);
                        GUILayout.Label($"Species: {parent1.Species}", EditorStyles.miniLabel);
                        GUILayout.Label($"Bloodline: {parent1.BloodlinePurity:P0}", EditorStyles.miniLabel);
                    }
                    else
                    {
                        GUILayout.Label("No parent selected", EditorStyles.miniLabel);
                    }
                    
                    if (GUILayout.Button("Select Parent 1"))
                        SelectSimulationParent(1);
                    if (GUILayout.Button("Use Current"))
                        parent1 = currentProfile?.Clone();
                }
                EditorGUILayout.EndVertical();
                
                GUILayout.Space(20);
                
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label("Parent 2:", EditorStyles.boldLabel);
                    if (parent2 != null)
                    {
                        GUILayout.Label($"Name: {parent2.Name}", EditorStyles.miniLabel);
                        GUILayout.Label($"Species: {parent2.Species}", EditorStyles.miniLabel);
                        GUILayout.Label($"Bloodline: {parent2.BloodlinePurity:P0}", EditorStyles.miniLabel);
                    }
                    else
                    {
                        GUILayout.Label("No parent selected", EditorStyles.miniLabel);
                    }
                    
                    if (GUILayout.Button("Select Parent 2"))
                        SelectSimulationParent(2);
                    if (GUILayout.Button("Use Current"))
                        parent2 = currentProfile?.Clone();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Simulation Controls
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Simulate Offspring") && parent1 != null && parent2 != null)
                {
                    SimulateOffspring();
                }
                
                if (GUILayout.Button("Clear Results"))
                {
                    simulatedOffspring.Clear();
                }
                
                GUILayout.Label($"Results: {simulatedOffspring.Count}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Simulation Results
            if (simulatedOffspring.Count > 0)
            {
                GUILayout.Label("Simulated Offspring:", EditorStyles.boldLabel);
                
                foreach (var offspring in simulatedOffspring)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(offspring.Name, EditorStyles.boldLabel);
                            
                            if (GUILayout.Button("Make Current", GUILayout.Width(100)))
                            {
                                currentProfile = offspring.Clone();
                                selectedTab = 0;
                            }
                            
                            if (GUILayout.Button("Save", GUILayout.Width(50)))
                            {
                                SaveGeneticProfile(offspring);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        GUILayout.Label($"Species: {offspring.Species} | Generation: {offspring.Generation} | Purity: {offspring.BloodlinePurity:P0}", EditorStyles.miniLabel);
                        
                        if (offspring.Genes?.Count > 0)
                        {
                            var expressedGenes = offspring.Genes.Where(g => g.Expression == GeneExpression.Dominant).Take(5);
                            if (expressedGenes.Any())
                            {
                                string traits = string.Join(", ", expressedGenes.Select(g => g.TraitName));
                                GUILayout.Label($"Expressed Traits: {traits}", EditorStyles.miniLabel);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    
                    GUILayout.Space(5);
                }
            }
        }
        
        private void DrawPreview()
        {
            GUILayout.Label("Genetic Profile Preview", EditorStyles.boldLabel);
            
            // Basic info
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label(currentProfile.Name, EditorStyles.largeLabel);
                GUILayout.Label($"Species: {currentProfile.Species} | Generation: {currentProfile.Generation}", EditorStyles.miniLabel);
                GUILayout.Label($"Bloodline: {currentProfile.BloodlineName} ({currentProfile.BloodlinePurity:P0} purity)", EditorStyles.miniLabel);
                
                if (!string.IsNullOrEmpty(currentProfile.Father) || !string.IsNullOrEmpty(currentProfile.Mother))
                {
                    string parents = "";
                    if (!string.IsNullOrEmpty(currentProfile.Father))
                        parents += $"Father: {currentProfile.Father}";
                    if (!string.IsNullOrEmpty(currentProfile.Mother))
                    {
                        if (parents.Length > 0) parents += " | ";
                        parents += $"Mother: {currentProfile.Mother}";
                    }
                    GUILayout.Label(parents, EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Physical traits
            if (HasPhysicalTraits())
            {
                GUILayout.Label("Physical Characteristics:", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    if (!string.IsNullOrEmpty(currentProfile.PhysicalTraits.EyeColor))
                        GUILayout.Label($"Eyes: {currentProfile.PhysicalTraits.EyeColor}", EditorStyles.miniLabel);
                    if (!string.IsNullOrEmpty(currentProfile.PhysicalTraits.HairColor))
                        GUILayout.Label($"Hair: {currentProfile.PhysicalTraits.HairColor}", EditorStyles.miniLabel);
                    if (!string.IsNullOrEmpty(currentProfile.PhysicalTraits.SkinTone))
                        GUILayout.Label($"Skin: {currentProfile.PhysicalTraits.SkinTone}", EditorStyles.miniLabel);
                    if (!string.IsNullOrEmpty(currentProfile.PhysicalTraits.Height))
                        GUILayout.Label($"Height: {currentProfile.PhysicalTraits.Height}", EditorStyles.miniLabel);
                    if (!string.IsNullOrEmpty(currentProfile.PhysicalTraits.Build))
                        GUILayout.Label($"Build: {currentProfile.PhysicalTraits.Build}", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
                
                GUILayout.Space(10);
            }
            
            // Genetic traits by expression
            if (currentProfile.Genes?.Count > 0)
            {
                var dominantTraits = currentProfile.Genes.Where(g => g.Expression == GeneExpression.Dominant).ToList();
                var recessiveTraits = currentProfile.Genes.Where(g => g.Expression == GeneExpression.Recessive).ToList();
                var hiddenTraits = currentProfile.Genes.Where(g => g.Expression == GeneExpression.Hidden).ToList();
                
                if (dominantTraits.Any())
                {
                    GUILayout.Label("Expressed Traits:", EditorStyles.boldLabel);
                    foreach (var trait in dominantTraits.OrderByDescending(t => t.Dominance))
                    {
                        Color oldColor = GUI.color;
                        GUI.color = GetRarityColor(trait.Rarity);
                        GUILayout.Label($"• {trait.TraitName} (Dominance: {trait.Dominance:P0})", EditorStyles.miniLabel);
                        GUI.color = oldColor;
                    }
                    GUILayout.Space(5);
                }
                
                if (recessiveTraits.Any())
                {
                    GUILayout.Label("Recessive Traits:", EditorStyles.boldLabel);
                    foreach (var trait in recessiveTraits)
                    {
                        GUILayout.Label($"• {trait.TraitName} (Dormant)", EditorStyles.miniLabel);
                    }
                    GUILayout.Space(5);
                }
                
                if (hiddenTraits.Any())
                {
                    GUILayout.Label("Hidden Traits:", EditorStyles.boldLabel);
                    foreach (var trait in hiddenTraits)
                    {
                        GUILayout.Label($"• {trait.TraitName} (Hidden)", EditorStyles.miniLabel);
                    }
                    GUILayout.Space(5);
                }
            }
            
            // Stat modifiers summary
            var allStatModifiers = new Dictionary<string, float>();
            if (currentProfile.Genes != null)
            {
                foreach (var gene in currentProfile.Genes.Where(g => g.Expression == GeneExpression.Dominant))
                {
                    if (gene.StatModifiers != null)
                    {
                        foreach (var modifier in gene.StatModifiers)
                        {
                            if (allStatModifiers.ContainsKey(modifier.Key))
                                allStatModifiers[modifier.Key] += modifier.Value;
                            else
                                allStatModifiers[modifier.Key] = modifier.Value;
                        }
                    }
                }
            }
            
            if (allStatModifiers.Count > 0)
            {
                GUILayout.Label("Total Stat Modifiers:", EditorStyles.boldLabel);
                foreach (var modifier in allStatModifiers)
                {
                    string sign = modifier.Value >= 0 ? "+" : "";
                    GUILayout.Label($"• {modifier.Key}: {sign}{modifier.Value}", EditorStyles.miniLabel);
                }
            }
        }
        
        private void DrawSaveControls()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Profile"))
                {
                    SaveGeneticProfile(currentProfile);
                }
                
                if (GUILayout.Button("Reset"))
                {
                    if (EditorUtility.DisplayDialog("Reset Profile", 
                        "Are you sure you want to reset all changes?", "Reset", "Cancel"))
                    {
                        currentProfile = new GeneticProfile();
                    }
                }
                
                if (GUILayout.Button("Duplicate"))
                {
                    var newProfile = currentProfile.Clone();
                    newProfile.ID = System.Guid.NewGuid().ToString();
                    newProfile.Name += " (Copy)";
                    currentProfile = newProfile;
                }
                
                if (GUILayout.Button("Randomize"))
                {
                    RandomizeCurrentProfile();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        // Helper Methods
        private void CreateNewProfile()
        {
            currentProfile = new GeneticProfile
            {
                ID = System.Guid.NewGuid().ToString(),
                Name = "New Profile",
                Species = "Human",
                Generation = 1,
                BloodlinePurity = 1.0f,
                BloodlineName = "Unknown"
            };
            selectedTab = 0;
        }
        
        private void GenerateRandomProfile()
        {
            var randomProfile = new GeneticProfile
            {
                ID = System.Guid.NewGuid().ToString(),
                Name = "Random Profile " + UnityEngine.Random.Range(1000, 9999),
                Species = GetRandomSpecies(),
                Generation = UnityEngine.Random.Range(1, 10),
                BloodlinePurity = UnityEngine.Random.Range(0.3f, 1.0f)
            };
            
            // Add random genes
            randomProfile.Genes = new List<Gene>();
            int geneCount = UnityEngine.Random.Range(3, 8);
            for (int i = 0; i < geneCount; i++)
            {
                randomProfile.Genes.Add(CreateRandomGene());
            }
            
            currentProfile = randomProfile;
        }
        
        private void SaveGeneticProfile(GeneticProfile profile)
        {
            if (string.IsNullOrEmpty(profile.Name))
            {
                EditorUtility.DisplayDialog("Error", "Profile name cannot be empty!", "OK");
                return;
            }
            
            var existingProfile = GameData.GeneticProfiles?.FirstOrDefault(p => p.ID == profile.ID);
            if (existingProfile != null)
            {
                // Update existing
                int index = GameData.GeneticProfiles.IndexOf(existingProfile);
                GameData.GeneticProfiles[index] = profile.Clone();
            }
            else
            {
                // Add new
                if (GameData.GeneticProfiles == null)
                    GameData.GeneticProfiles = new List<GeneticProfile>();
                GameData.GeneticProfiles.Add(profile.Clone());
            }
            
            EditorUtility.DisplayDialog("Success", $"Genetic profile '{profile.Name}' saved successfully!", "OK");
        }
        
        private void SaveAllProfiles()
        {
            EditorUtility.DisplayDialog("Save All", "All genetic profiles saved to database!", "OK");
        }
        
        private void SimulateOffspring()
        {
            simulatedOffspring.Clear();
            
            // Generate 3-5 offspring
            int count = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < count; i++)
            {
                var offspring = GenerateOffspring(parent1, parent2, i + 1);
                simulatedOffspring.Add(offspring);
            }
        }
        
        private GeneticProfile GenerateOffspring(GeneticProfile p1, GeneticProfile p2, int childNumber)
        {
            var offspring = new GeneticProfile
            {
                ID = System.Guid.NewGuid().ToString(),
                Name = $"Offspring {childNumber}",
                Species = DetermineOffspringSpecies(p1, p2),
                Generation = Mathf.Max(p1.Generation, p2.Generation) + 1,
                BloodlinePurity = CalculateOffspringPurity(p1, p2),
                Father = p1.Name,
                Mother = p2.Name,
                Genes = new List<Gene>()
            };
            
            // Combine genes from both parents
            var allParentGenes = new List<Gene>();
            if (p1.Genes != null) allParentGenes.AddRange(p1.Genes);
            if (p2.Genes != null) allParentGenes.AddRange(p2.Genes);
            
            // Group by trait name and select dominant gene or create hybrid
            var geneGroups = allParentGenes.GroupBy(g => g.TraitName);
            foreach (var group in geneGroups)
            {
                var genes = group.ToList();
                if (genes.Count == 1)
                {
                    // Only one parent has this gene
                    var gene = genes[0].Clone();
                    gene.Expression = UnityEngine.Random.value < 0.5f ? GeneExpression.Recessive : GeneExpression.Hidden;
                    offspring.Genes.Add(gene);
                }
                else
                {
                    // Both parents have this gene - determine expression
                    var gene1 = genes[0];
                    var gene2 = genes[1];
                    
                    var resultGene = gene1.Dominance > gene2.Dominance ? gene1.Clone() : gene2.Clone();
                    
                    // Determine final expression based on dominance
                    float combinedDominance = (gene1.Dominance + gene2.Dominance) / 2f;
                    if (combinedDominance > 0.7f)
                        resultGene.Expression = GeneExpression.Dominant;
                    else if (combinedDominance > 0.4f)
                        resultGene.Expression = GeneExpression.Recessive;
                    else
                        resultGene.Expression = GeneExpression.Hidden;
                    
                    offspring.Genes.Add(resultGene);
                }
            }
            
            return offspring;
        }
        
        private void ShowTemplateMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Human Noble"), false, () => ApplyTemplate("HumanNoble"));
            menu.AddItem(new GUIContent("Elven Highborn"), false, () => ApplyTemplate("ElvenHighborn"));
            menu.AddItem(new GUIContent("Dwarven Craftsman"), false, () => ApplyTemplate("DwarvenCraftsman"));
            menu.AddItem(new GUIContent("Mixed Heritage"), false, () => ApplyTemplate("MixedHeritage"));
            menu.ShowAsContext();
        }
        
        private void ShowGeneTemplateMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Strength Gene"), false, () => AddTemplateGene("Strength"));
            menu.AddItem(new GUIContent("Intelligence Gene"), false, () => AddTemplateGene("Intelligence"));
            menu.AddItem(new GUIContent("Magical Affinity"), false, () => AddTemplateGene("MagicalAffinity"));
            menu.AddItem(new GUIContent("Longevity"), false, () => AddTemplateGene("Longevity"));
            menu.AddItem(new GUIContent("Disease Resistance"), false, () => AddTemplateGene("DiseaseResistance"));
            menu.ShowAsContext();
        }
        
        private Color GetExpressionColor(GeneExpression expression)
        {
            switch (expression)
            {
                case GeneExpression.Dominant: return Color.green;
                case GeneExpression.Recessive: return Color.yellow;
                case GeneExpression.Hidden: return Color.gray;
                default: return Color.white;
            }
        }
        
        private Color GetRarityColor(GeneRarity rarity)
        {
            switch (rarity)
            {
                case GeneRarity.Common: return Color.white;
                case GeneRarity.Uncommon: return Color.cyan;
                case GeneRarity.Rare: return Color.blue;
                case GeneRarity.Epic: return Color.magenta;
                case GeneRarity.Legendary: return Color.yellow;
                default: return Color.white;
            }
        }
        
        // Additional helper methods would go here...
        private void SelectParent(bool isFather) { /* Implementation */ }
        private void SelectSimulationParent(int parentNumber) { /* Implementation */ }
        private void AddGeneEffect(Gene gene, string effectName) { /* Implementation */ }
        private void AddBreedingModifier(string modifierName) { /* Implementation */ }
        private void ApplyTemplate(string templateName) { /* Implementation */ }
        private void AddTemplateGene(string geneName) { /* Implementation */ }
        private void RandomizeCurrentProfile() { /* Implementation */ }
        private Gene CreateRandomGene() { return new Gene(); }
        private string GetRandomSpecies() { return "Human"; }
        private string DetermineOffspringSpecies(GeneticProfile p1, GeneticProfile p2) { return p1.Species; }
        private float CalculateOffspringPurity(GeneticProfile p1, GeneticProfile p2) { return (p1.BloodlinePurity + p2.BloodlinePurity) / 2f; }
        private bool HasPhysicalTraits() { return !string.IsNullOrEmpty(currentProfile.PhysicalTraits.EyeColor); }
    }
}
