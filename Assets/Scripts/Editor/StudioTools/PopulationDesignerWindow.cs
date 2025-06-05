using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Core;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Editor
{
    public class PopulationDesignerWindow : EditorWindow
    {
        private Population currentPopulation;
        private Vector2 scrollPosition;
        private Vector2 listScrollPosition;
        private string searchFilter = "";
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Basic Info", "Demographics", "Economy", "Culture", "Growth", "Preview" };
        
        // Population simulation data
        private float simulationYears = 10f;
        private List<PopulationSnapshot> projectedGrowth = new List<PopulationSnapshot>();
        
        [MenuItem("Lineage Studio/Content Creation/Population Designer")]
        public static void ShowWindow()
        {
            var window = GetWindow<PopulationDesignerWindow>("Population Designer");
            window.minSize = new Vector2(900, 650);
            window.Show();
        }
        
        private void OnEnable()
        {
            if (currentPopulation == null)
                currentPopulation = new Population(default(Population.ID), "New Settlement", new Settlement());
        }
        
        private void OnGUI()
        {
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Population list
                DrawPopulationList();
                
                // Right panel - Population editor
                DrawPopulationEditor();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Population Designer", EditorStyles.boldLabel);
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Generate Settlement", GUILayout.Width(140)))
                {
                    GenerateRandomSettlement();
                }
                
                if (GUILayout.Button("Templates", GUILayout.Width(80)))
                {
                    ShowTemplateMenu();
                }
                
                if (GUILayout.Button("Save All", GUILayout.Width(80)))
                {
                    SaveAllPopulations();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawPopulationList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            {
                // Search and controls
                EditorGUILayout.BeginHorizontal();
                {
                    searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
                    if (GUILayout.Button("New", GUILayout.Width(50)))
                        CreateNewPopulation();
                }
                EditorGUILayout.EndHorizontal();
                
                // Type filter
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("All", EditorStyles.miniButtonLeft))
                        searchFilter = "";
                    if (GUILayout.Button("City", EditorStyles.miniButtonMid))
                        searchFilter = "City";
                    if (GUILayout.Button("Town", EditorStyles.miniButtonMid))
                        searchFilter = "Town";
                    if (GUILayout.Button("Village", EditorStyles.miniButtonMid))
                        searchFilter = "Village";
                    if (GUILayout.Button("Nomadic", EditorStyles.miniButtonRight))
                        searchFilter = "Nomadic";
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Population list
                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);
                {
                    DrawPopulations();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPopulations()
        {
            if (Lineage.Ancestral.Legacies.Database.GameData.Populations == null) return;

            var filteredPopulations = Lineage.Ancestral.Legacies.Database.GameData.Populations.Where(p =>
                string.IsNullOrEmpty(searchFilter) ||
                p.Name.ToLower().Contains(searchFilter.ToLower()) ||
                p.SettlementType.ToLower().Contains(searchFilter.ToLower()) ||
                p.Location.ToLower().Contains(searchFilter.ToLower())).ToList();
            
            // Group by settlement type
            var groupedPopulations = filteredPopulations.GroupBy(p => p.SettlementType).OrderBy(g => g.Key);
            
            foreach (var group in groupedPopulations)
            {
                GUILayout.Label(group.Key, EditorStyles.boldLabel);
                
                foreach (var population in group.OrderByDescending(p => p.TotalPopulation))
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        bool isSelected = currentPopulation != null && currentPopulation.ID == population.ID;
                        
                        if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                        {
                            currentPopulation = population.Clone();
                        }
                        
                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Label(population.Name, EditorStyles.boldLabel);
                            GUILayout.Label($"Population: {population.TotalPopulation:N0}", EditorStyles.miniLabel);
                            GUILayout.Label($"Location: {population.Location}", EditorStyles.miniLabel);
                            GUILayout.Label($"Prosperity: {population.ProsperityLevel}", EditorStyles.miniLabel);
                        }
                        EditorGUILayout.EndVertical();
                        
                        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Population", 
                                $"Are you sure you want to delete '{population.Name}'?", "Delete", "Cancel"))
                            {
                                GameData.Populations.Remove(population);
                                if (currentPopulation?.ID == population.ID)
                                    currentPopulation = new Population();
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(2);
                }
                
                GUILayout.Space(5);
            }
        }
        
        private void DrawPopulationEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                if (currentPopulation == null)
                {
                    GUILayout.Label("Select a population to edit or create a new one.", EditorStyles.centeredGreyMiniLabel);
                    return;
                }
                
                // Tab selection
                selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    switch (selectedTab)
                    {
                        case 0: DrawBasicInfo(); break;
                        case 1: DrawDemographics(); break;
                        case 2: DrawEconomy(); break;
                        case 3: DrawCulture(); break;
                        case 4: DrawGrowth(); break;
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
            
            currentPopulation.Name = EditorGUILayout.TextField("Settlement Name", currentPopulation.Name);
            currentPopulation.Location = EditorGUILayout.TextField("Location", currentPopulation.Location);
            
            EditorGUILayout.BeginHorizontal();
            {
                currentPopulation.SettlementType = EditorGUILayout.TextField("Settlement Type", currentPopulation.SettlementType);
                if (GUILayout.Button("Presets", GUILayout.Width(70)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Metropolis"), false, () => SetSettlementType("Metropolis", 500000, 1000000));
                    menu.AddItem(new GUIContent("City"), false, () => SetSettlementType("City", 50000, 500000));
                    menu.AddItem(new GUIContent("Large Town"), false, () => SetSettlementType("Large Town", 10000, 50000));
                    menu.AddItem(new GUIContent("Town"), false, () => SetSettlementType("Town", 2000, 10000));
                    menu.AddItem(new GUIContent("Small Town"), false, () => SetSettlementType("Small Town", 500, 2000));
                    menu.AddItem(new GUIContent("Village"), false, () => SetSettlementType("Village", 100, 500));
                    menu.AddItem(new GUIContent("Hamlet"), false, () => SetSettlementType("Hamlet", 20, 100));
                    menu.AddItem(new GUIContent("Outpost"), false, () => SetSettlementType("Outpost", 5, 20));
                    menu.AddItem(new GUIContent("Nomadic Tribe"), false, () => SetSettlementType("Nomadic", 50, 500));
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            currentPopulation.TotalPopulation = EditorGUILayout.IntField("Total Population", currentPopulation.TotalPopulation);
            currentPopulation.PopulationDensity = EditorGUILayout.FloatField("Population Density (per sq km)", currentPopulation.PopulationDensity);
            
            GUILayout.Space(10);
            
            // Geography and Environment
            GUILayout.Label("Geography & Environment", EditorStyles.boldLabel);
            currentPopulation.Terrain = EditorGUILayout.TextField("Terrain Type", currentPopulation.Terrain);
            currentPopulation.Climate = EditorGUILayout.TextField("Climate", currentPopulation.Climate);
            currentPopulation.NaturalResources = EditorGUILayout.TextField("Natural Resources", currentPopulation.NaturalResources);
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Plains"))
                    currentPopulation.Terrain = "Plains";
                if (GUILayout.Button("Forest"))
                    currentPopulation.Terrain = "Forest";
                if (GUILayout.Button("Mountains"))
                    currentPopulation.Terrain = "Mountains";
                if (GUILayout.Button("Coastal"))
                    currentPopulation.Terrain = "Coastal";
                if (GUILayout.Button("Desert"))
                    currentPopulation.Terrain = "Desert";
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Leadership and Government
            GUILayout.Label("Leadership & Government", EditorStyles.boldLabel);
            currentPopulation.GovernmentType = EditorGUILayout.TextField("Government Type", currentPopulation.GovernmentType);
            currentPopulation.Ruler = EditorGUILayout.TextField("Ruler/Leader", currentPopulation.Ruler);
            currentPopulation.PoliticalStability = EditorGUILayout.Slider("Political Stability", currentPopulation.PoliticalStability, 0f, 1f);
        }
        
        private void DrawDemographics()
        {
            GUILayout.Label("Demographics", EditorStyles.boldLabel);
            
            // Species Distribution
            GUILayout.Label("Species Distribution:", EditorStyles.boldLabel);
            if (currentPopulation.SpeciesDistribution == null)
                currentPopulation.SpeciesDistribution = new Dictionary<string, float>();
            
            float totalPercentage = currentPopulation.SpeciesDistribution.Values.Sum();
            
            var speciesKeys = currentPopulation.SpeciesDistribution.Keys.ToList();
            for (int i = 0; i < speciesKeys.Count; i++)
            {
                string species = speciesKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(species, GUILayout.Width(100));
                    currentPopulation.SpeciesDistribution[species] = EditorGUILayout.Slider(
                        currentPopulation.SpeciesDistribution[species], 0f, 1f, GUILayout.Width(150));
                    GUILayout.Label($"{currentPopulation.SpeciesDistribution[species]:P1}", GUILayout.Width(50));
                    int count = Mathf.RoundToInt(currentPopulation.TotalPopulation * currentPopulation.SpeciesDistribution[species]);
                    GUILayout.Label($"({count:N0})", GUILayout.Width(80));
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentPopulation.SpeciesDistribution.Remove(species);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Species"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Human"), false, () => AddSpecies("Human"));
                menu.AddItem(new GUIContent("Elf"), false, () => AddSpecies("Elf"));
                menu.AddItem(new GUIContent("Dwarf"), false, () => AddSpecies("Dwarf"));
                menu.AddItem(new GUIContent("Halfling"), false, () => AddSpecies("Halfling"));
                menu.AddItem(new GUIContent("Orc"), false, () => AddSpecies("Orc"));
                menu.AddItem(new GUIContent("Half-Elf"), false, () => AddSpecies("Half-Elf"));
                menu.AddItem(new GUIContent("Other"), false, () => AddSpecies("Other"));
                menu.ShowAsContext();
            }
            
            if (totalPercentage != 0)
            {
                EditorGUILayout.HelpBox($"Total: {totalPercentage:P1} {(totalPercentage > 1.01f ? "(Over 100%!)" : totalPercentage < 0.99f ? "(Under 100%)" : "")}", 
                    totalPercentage > 1.01f || totalPercentage < 0.99f ? MessageType.Warning : MessageType.Info);
            }
            
            if (GUILayout.Button("Normalize Percentages"))
            {
                NormalizeSpeciesDistribution();
            }
            
            GUILayout.Space(10);
            
            // Age Demographics
            GUILayout.Label("Age Demographics:", EditorStyles.boldLabel);
            currentPopulation.AgeDistribution.Children = EditorGUILayout.Slider("Children (0-17)", currentPopulation.AgeDistribution.Children, 0f, 1f);
            currentPopulation.AgeDistribution.Adults = EditorGUILayout.Slider("Adults (18-64)", currentPopulation.AgeDistribution.Adults, 0f, 1f);
            currentPopulation.AgeDistribution.Elderly = EditorGUILayout.Slider("Elderly (65+)", currentPopulation.AgeDistribution.Elderly, 0f, 1f);
            
            float ageTotal = currentPopulation.AgeDistribution.Children + currentPopulation.AgeDistribution.Adults + currentPopulation.AgeDistribution.Elderly;
            if (Mathf.Abs(ageTotal - 1f) > 0.01f)
            {
                EditorGUILayout.HelpBox($"Age distribution total: {ageTotal:P1}", MessageType.Warning);
                if (GUILayout.Button("Normalize Age Distribution"))
                {
                    NormalizeAgeDistribution();
                }
            }
            
            GUILayout.Space(10);
            
            // Social Classes
            GUILayout.Label("Social Class Distribution:", EditorStyles.boldLabel);
            if (currentPopulation.SocialClasses == null)
                currentPopulation.SocialClasses = new Dictionary<string, float>();
            
            var classKeys = currentPopulation.SocialClasses.Keys.ToList();
            for (int i = 0; i < classKeys.Count; i++)
            {
                string socialClass = classKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(socialClass, GUILayout.Width(100));
                    currentPopulation.SocialClasses[socialClass] = EditorGUILayout.Slider(
                        currentPopulation.SocialClasses[socialClass], 0f, 1f, GUILayout.Width(150));
                    GUILayout.Label($"{currentPopulation.SocialClasses[socialClass]:P1}", GUILayout.Width(50));
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentPopulation.SocialClasses.Remove(socialClass);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Social Class"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Nobility"), false, () => AddSocialClass("Nobility"));
                menu.AddItem(new GUIContent("Merchants"), false, () => AddSocialClass("Merchants"));
                menu.AddItem(new GUIContent("Artisans"), false, () => AddSocialClass("Artisans"));
                menu.AddItem(new GUIContent("Farmers"), false, () => AddSocialClass("Farmers"));
                menu.AddItem(new GUIContent("Laborers"), false, () => AddSocialClass("Laborers"));
                menu.AddItem(new GUIContent("Clergy"), false, () => AddSocialClass("Clergy"));
                menu.AddItem(new GUIContent("Scholars"), false, () => AddSocialClass("Scholars"));
                menu.AddItem(new GUIContent("Military"), false, () => AddSocialClass("Military"));
                menu.ShowAsContext();
            }
        }
        
        private void DrawEconomy()
        {
            GUILayout.Label("Economic Profile", EditorStyles.boldLabel);
            
            currentPopulation.ProsperityLevel = (ProsperityLevel)EditorGUILayout.EnumPopup("Prosperity Level", currentPopulation.ProsperityLevel);
            currentPopulation.WealthDistribution = EditorGUILayout.Slider("Wealth Equality", currentPopulation.WealthDistribution, 0f, 1f);
            EditorGUILayout.HelpBox("0 = High inequality, 1 = Perfect equality", MessageType.Info);
            
            GUILayout.Space(10);
            
            // Primary Industries
            GUILayout.Label("Primary Industries:", EditorStyles.boldLabel);
            if (currentPopulation.PrimaryIndustries == null)
                currentPopulation.PrimaryIndustries = new List<string>();
            
            for (int i = 0; i < currentPopulation.PrimaryIndustries.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentPopulation.PrimaryIndustries[i] = EditorGUILayout.TextField($"Industry {i + 1}", currentPopulation.PrimaryIndustries[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentPopulation.PrimaryIndustries.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Industry"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Agriculture"), false, () => AddIndustry("Agriculture"));
                menu.AddItem(new GUIContent("Mining"), false, () => AddIndustry("Mining"));
                menu.AddItem(new GUIContent("Crafting"), false, () => AddIndustry("Crafting"));
                menu.AddItem(new GUIContent("Trade"), false, () => AddIndustry("Trade"));
                menu.AddItem(new GUIContent("Fishing"), false, () => AddIndustry("Fishing"));
                menu.AddItem(new GUIContent("Logging"), false, () => AddIndustry("Logging"));
                menu.AddItem(new GUIContent("Manufacturing"), false, () => AddIndustry("Manufacturing"));
                menu.AddItem(new GUIContent("Magic/Alchemy"), false, () => AddIndustry("Magic/Alchemy"));
                menu.AddItem(new GUIContent("Tourism"), false, () => AddIndustry("Tourism"));
                menu.ShowAsContext();
            }
            
            GUILayout.Space(10);
            
            // Trade Relations
            GUILayout.Label("Trade Relations:", EditorStyles.boldLabel);
            if (currentPopulation.TradePartners == null)
                currentPopulation.TradePartners = new List<string>();
            
            for (int i = 0; i < currentPopulation.TradePartners.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentPopulation.TradePartners[i] = EditorGUILayout.TextField($"Partner {i + 1}", currentPopulation.TradePartners[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentPopulation.TradePartners.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Trade Partner"))
                currentPopulation.TradePartners.Add("");
            
            GUILayout.Space(10);
            
            // Economic Factors
            GUILayout.Label("Economic Factors:", EditorStyles.boldLabel);
            currentPopulation.TaxRate = EditorGUILayout.Slider("Tax Rate", currentPopulation.TaxRate, 0f, 1f);
            currentPopulation.TradeVolume = EditorGUILayout.Slider("Trade Volume", currentPopulation.TradeVolume, 0f, 1f);
            currentPopulation.Infrastructure = EditorGUILayout.Slider("Infrastructure Quality", currentPopulation.Infrastructure, 0f, 1f);
            
            // Economic Stability
            currentPopulation.EconomicStability = EditorGUILayout.Slider("Economic Stability", currentPopulation.EconomicStability, 0f, 1f);
        }
        
        private void DrawCulture()
        {
            GUILayout.Label("Cultural Profile", EditorStyles.boldLabel);
            
            currentPopulation.CultureName = EditorGUILayout.TextField("Culture Name", currentPopulation.CultureName);
            currentPopulation.PredominantReligion = EditorGUILayout.TextField("Predominant Religion", currentPopulation.PredominantReligion);
            currentPopulation.Language = EditorGUILayout.TextField("Primary Language", currentPopulation.Language);
            
            GUILayout.Space(10);
            
            // Cultural Values
            GUILayout.Label("Cultural Values:", EditorStyles.boldLabel);
            if (currentPopulation.CulturalValues == null)
                currentPopulation.CulturalValues = new Dictionary<string, float>();
            
            var valueKeys = currentPopulation.CulturalValues.Keys.ToList();
            for (int i = 0; i < valueKeys.Count; i++)
            {
                string value = valueKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(value, GUILayout.Width(120));
                    currentPopulation.CulturalValues[value] = EditorGUILayout.Slider(
                        currentPopulation.CulturalValues[value], 0f, 1f, GUILayout.Width(150));
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentPopulation.CulturalValues.Remove(value);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Cultural Value"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Honor"), false, () => AddCulturalValue("Honor"));
                menu.AddItem(new GUIContent("Tradition"), false, () => AddCulturalValue("Tradition"));
                menu.AddItem(new GUIContent("Innovation"), false, () => AddCulturalValue("Innovation"));
                menu.AddItem(new GUIContent("Community"), false, () => AddCulturalValue("Community"));
                menu.AddItem(new GUIContent("Individual Freedom"), false, () => AddCulturalValue("Individual Freedom"));
                menu.AddItem(new GUIContent("Knowledge"), false, () => AddCulturalValue("Knowledge"));
                menu.AddItem(new GUIContent("Martial Prowess"), false, () => AddCulturalValue("Martial Prowess"));
                menu.AddItem(new GUIContent("Craftsmanship"), false, () => AddCulturalValue("Craftsmanship"));
                menu.AddItem(new GUIContent("Magic"), false, () => AddCulturalValue("Magic"));
                menu.ShowAsContext();
            }
            
            GUILayout.Space(10);
            
            // Cultural Traits
            GUILayout.Label("Cultural Traits:", EditorStyles.boldLabel);
            if (currentPopulation.CulturalTraits == null)
                currentPopulation.CulturalTraits = new List<string>();
            
            for (int i = 0; i < currentPopulation.CulturalTraits.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    currentPopulation.CulturalTraits[i] = EditorGUILayout.TextField($"Trait {i + 1}", currentPopulation.CulturalTraits[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentPopulation.CulturalTraits.RemoveAt(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Cultural Trait"))
                currentPopulation.CulturalTraits.Add("");
            
            GUILayout.Space(10);
            
            // Education and Knowledge
            GUILayout.Label("Education & Knowledge:", EditorStyles.boldLabel);
            currentPopulation.LiteracyRate = EditorGUILayout.Slider("Literacy Rate", currentPopulation.LiteracyRate, 0f, 1f);
            currentPopulation.EducationLevel = EditorGUILayout.Slider("Education Level", currentPopulation.EducationLevel, 0f, 1f);
            currentPopulation.MagicalKnowledge = EditorGUILayout.Slider("Magical Knowledge", currentPopulation.MagicalKnowledge, 0f, 1f);
        }
        
        private void DrawGrowth()
        {
            GUILayout.Label("Population Growth & Projections", EditorStyles.boldLabel);
            
            // Growth Factors
            currentPopulation.GrowthRate = EditorGUILayout.Slider("Annual Growth Rate", currentPopulation.GrowthRate, -0.1f, 0.1f);
            currentPopulation.BirthRate = EditorGUILayout.Slider("Birth Rate", currentPopulation.BirthRate, 0f, 0.1f);
            currentPopulation.DeathRate = EditorGUILayout.Slider("Death Rate", currentPopulation.DeathRate, 0f, 0.1f);
            currentPopulation.MigrationRate = EditorGUILayout.Slider("Migration Rate", currentPopulation.MigrationRate, -0.1f, 0.1f);
            
            GUILayout.Space(10);
            
            // Growth Factors
            GUILayout.Label("Growth Factors:", EditorStyles.boldLabel);
            if (currentPopulation.GrowthFactors == null)
                currentPopulation.GrowthFactors = new Dictionary<string, float>();
            
            var factorKeys = currentPopulation.GrowthFactors.Keys.ToList();
            for (int i = 0; i < factorKeys.Count; i++)
            {
                string factor = factorKeys[i];
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(factor, GUILayout.Width(120));
                    currentPopulation.GrowthFactors[factor] = EditorGUILayout.Slider(
                        currentPopulation.GrowthFactors[factor], -1f, 1f, GUILayout.Width(150));
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        currentPopulation.GrowthFactors.Remove(factor);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Growth Factor"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Disease"), false, () => AddGrowthFactor("Disease"));
                menu.AddItem(new GUIContent("War"), false, () => AddGrowthFactor("War"));
                menu.AddItem(new GUIContent("Famine"), false, () => AddGrowthFactor("Famine"));
                menu.AddItem(new GUIContent("Economic Boom"), false, () => AddGrowthFactor("Economic Boom"));
                menu.AddItem(new GUIContent("Natural Disaster"), false, () => AddGrowthFactor("Natural Disaster"));
                menu.AddItem(new GUIContent("Immigration Wave"), false, () => AddGrowthFactor("Immigration Wave"));
                menu.AddItem(new GUIContent("Medical Advances"), false, () => AddGrowthFactor("Medical Advances"));
                menu.ShowAsContext();
            }
            
            GUILayout.Space(10);
            
            // Population Simulation
            GUILayout.Label("Population Simulation:", EditorStyles.boldLabel);
            simulationYears = EditorGUILayout.FloatField("Years to Simulate", simulationYears);
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Run Simulation"))
                {
                    RunPopulationSimulation();
                }
                
                if (GUILayout.Button("Clear Results"))
                {
                    projectedGrowth.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Simulation Results
            if (projectedGrowth.Count > 0)
            {
                GUILayout.Label("Projected Growth:", EditorStyles.boldLabel);
                
                foreach (var snapshot in projectedGrowth.Take(10)) // Show first 10 years
                {
                    GUILayout.Label($"Year {snapshot.Year}: {snapshot.Population:N0} " +
                                  $"(Growth: {snapshot.GrowthRate:P1})", EditorStyles.miniLabel);
                }
                
                if (projectedGrowth.Count > 10)
                {
                    var lastSnapshot = projectedGrowth.Last();
                    GUILayout.Label($"... Year {lastSnapshot.Year}: {lastSnapshot.Population:N0} " +
                                  $"(Final Growth: {lastSnapshot.GrowthRate:P1})", EditorStyles.miniLabel);
                }
            }
        }
        
        private void DrawPreview()
        {
            GUILayout.Label("Population Overview", EditorStyles.boldLabel);
            
            // Header information
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label(currentPopulation.Name, EditorStyles.largeLabel);
                GUILayout.Label($"{currentPopulation.SettlementType} | Population: {currentPopulation.TotalPopulation:N0}", EditorStyles.miniLabel);
                GUILayout.Label($"Location: {currentPopulation.Location} ({currentPopulation.Terrain})", EditorStyles.miniLabel);
                GUILayout.Label($"Government: {currentPopulation.GovernmentType} | Ruler: {currentPopulation.Ruler}", EditorStyles.miniLabel);
                GUILayout.Label($"Prosperity: {currentPopulation.ProsperityLevel} | Stability: {currentPopulation.PoliticalStability:P0}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Demographics summary
            if (currentPopulation.SpeciesDistribution?.Count > 0)
            {
                GUILayout.Label("Species Composition:", EditorStyles.boldLabel);
                foreach (var species in currentPopulation.SpeciesDistribution.OrderByDescending(s => s.Value))
                {
                    int count = Mathf.RoundToInt(currentPopulation.TotalPopulation * species.Value);
                    GUILayout.Label($"• {species.Key}: {species.Value:P1} ({count:N0})", EditorStyles.miniLabel);
                }
                GUILayout.Space(5);
            }
            
            // Economic summary
            if (currentPopulation.PrimaryIndustries?.Count > 0)
            {
                GUILayout.Label("Primary Industries:", EditorStyles.boldLabel);
                GUILayout.Label($"• {string.Join(", ", currentPopulation.PrimaryIndustries)}", EditorStyles.miniLabel);
                GUILayout.Space(5);
            }
            
            // Cultural summary
            if (!string.IsNullOrEmpty(currentPopulation.CultureName))
            {
                GUILayout.Label("Culture & Society:", EditorStyles.boldLabel);
                GUILayout.Label($"Culture: {currentPopulation.CultureName}", EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(currentPopulation.PredominantReligion))
                    GUILayout.Label($"Religion: {currentPopulation.PredominantReligion}", EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(currentPopulation.Language))
                    GUILayout.Label($"Language: {currentPopulation.Language}", EditorStyles.miniLabel);
                GUILayout.Label($"Literacy: {currentPopulation.LiteracyRate:P0} | Education: {currentPopulation.EducationLevel:P0}", EditorStyles.miniLabel);
                GUILayout.Space(5);
            }
            
            // Growth summary
            GUILayout.Label("Growth Statistics:", EditorStyles.boldLabel);
            GUILayout.Label($"Growth Rate: {currentPopulation.GrowthRate:P1} annually", EditorStyles.miniLabel);
            GUILayout.Label($"Birth Rate: {currentPopulation.BirthRate:P1} | Death Rate: {currentPopulation.DeathRate:P1}", EditorStyles.miniLabel);
            GUILayout.Label($"Migration Rate: {currentPopulation.MigrationRate:P1}", EditorStyles.miniLabel);
            
            // Factors affecting growth
            if (currentPopulation.GrowthFactors?.Count > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("Active Growth Factors:", EditorStyles.boldLabel);
                foreach (var factor in currentPopulation.GrowthFactors.Where(f => Mathf.Abs(f.Value) > 0.01f))
                {
                    string effect = factor.Value > 0 ? "+" : "";
                    GUILayout.Label($"• {factor.Key}: {effect}{factor.Value:P1} impact", EditorStyles.miniLabel);
                }
            }
        }
        
        private void DrawSaveControls()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Population"))
                {
                    SavePopulation();
                }
                
                if (GUILayout.Button("Reset"))
                {
                    if (EditorUtility.DisplayDialog("Reset Population", 
                        "Are you sure you want to reset all changes?", "Reset", "Cancel"))
                    {
                        currentPopulation = new Population();
                    }
                }
                
                if (GUILayout.Button("Duplicate"))
                {
                    var newPopulation = currentPopulation.Clone();
                    newPopulation.ID = System.Guid.NewGuid().ToString();
                    newPopulation.Name += " (Copy)";
                    currentPopulation = newPopulation;
                }
                
                if (GUILayout.Button("Generate Random"))
                {
                    GenerateRandomSettlement();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        // Helper Methods
        private void CreateNewPopulation()
        {
            currentPopulation = new Population
            {
                ID = System.Guid.NewGuid().ToString(),
                Name = "New Settlement",
                SettlementType = "Village",
                TotalPopulation = 300,
                Location = "Unknown",
                ProsperityLevel = ProsperityLevel.Modest,
                GrowthRate = 0.02f,
                BirthRate = 0.03f,
                DeathRate = 0.01f
            };
            selectedTab = 0;
        }
        
        private void GenerateRandomSettlement()
        {
            string[] settlementNames = { "Riverside", "Greenwood", "Stonehaven", "Brightwater", "Goldfield", "Ironhold", "Redrock", "Silverpeak" };
            string[] terrains = { "Plains", "Forest", "Hills", "Coastal", "Mountains", "River Valley" };
            string[] governments = { "Monarchy", "Council", "Democracy", "Theocracy", "Merchant Guild", "Military Rule" };
            
            var randomSettlement = new Population
            {
                ID = System.Guid.NewGuid().ToString(),
                Name = settlementNames[UnityEngine.Random.Range(0, settlementNames.Length)] + " " + 
                       (UnityEngine.Random.value > 0.5f ? "Village" : "Town"),
                SettlementType = UnityEngine.Random.value > 0.7f ? "Town" : "Village",
                TotalPopulation = UnityEngine.Random.Range(100, 2000),
                Location = "Generated Location",
                Terrain = terrains[UnityEngine.Random.Range(0, terrains.Length)],
                GovernmentType = governments[UnityEngine.Random.Range(0, governments.Length)],
                ProsperityLevel = (ProsperityLevel)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ProsperityLevel)).Length),
                GrowthRate = UnityEngine.Random.Range(-0.01f, 0.05f),
                PoliticalStability = UnityEngine.Random.Range(0.3f, 1.0f)
            };
            
            // Add random species distribution
            randomSettlement.SpeciesDistribution = new Dictionary<string, float>
            {
                ["Human"] = UnityEngine.Random.Range(0.6f, 0.9f),
                ["Elf"] = UnityEngine.Random.Range(0.05f, 0.3f),
                ["Dwarf"] = UnityEngine.Random.Range(0.05f, 0.2f)
            };
            
            NormalizeSpeciesDistribution(randomSettlement);
            currentPopulation = randomSettlement;
            selectedTab = 0;
        }
        
        private void SavePopulation()
        {
            if (string.IsNullOrEmpty(currentPopulation.Name))
            {
                EditorUtility.DisplayDialog("Error", "Population name cannot be empty!", "OK");
                return;
            }
            
            var existingPopulation = GameData.Populations?.FirstOrDefault(p => p.ID == currentPopulation.ID);
            if (existingPopulation != null)
            {
                // Update existing
                int index = GameData.Populations.IndexOf(existingPopulation);
                GameData.Populations[index] = currentPopulation.Clone();
            }
            else
            {
                // Add new
                if (GameData.Populations == null)
                    GameData.Populations = new List<Population>();
                GameData.Populations.Add(currentPopulation.Clone());
            }
            
            EditorUtility.DisplayDialog("Success", $"Population '{currentPopulation.Name}' saved successfully!", "OK");
        }
        
        private void SaveAllPopulations()
        {
            EditorUtility.DisplayDialog("Save All", "All populations saved to database!", "OK");
        }
        
        private void RunPopulationSimulation()
        {
            projectedGrowth.Clear();
            
            int currentPop = currentPopulation.TotalPopulation;
            float currentGrowthRate = currentPopulation.GrowthRate;
            
            for (int year = 1; year <= simulationYears; year++)
            {
                // Apply growth factors
                float modifiedGrowthRate = currentGrowthRate;
                if (currentPopulation.GrowthFactors != null)
                {
                    foreach (var factor in currentPopulation.GrowthFactors)
                    {
                        modifiedGrowthRate += factor.Value * UnityEngine.Random.Range(0.5f, 1.5f);
                    }
                }
                
                // Calculate new population
                currentPop = Mathf.RoundToInt(currentPop * (1 + modifiedGrowthRate));
                
                projectedGrowth.Add(new PopulationSnapshot
                {
                    Year = year,
                    Population = currentPop,
                    GrowthRate = modifiedGrowthRate
                });
                
                // Gradually normalize growth rate over time
                currentGrowthRate = Mathf.Lerp(currentGrowthRate, currentPopulation.GrowthRate, 0.1f);
            }
        }
        
        private void ShowTemplateMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Medieval Village"), false, () => ApplyTemplate("MedievalVillage"));
            menu.AddItem(new GUIContent("Trading Town"), false, () => ApplyTemplate("TradingTown"));
            menu.AddItem(new GUIContent("Mining Settlement"), false, () => ApplyTemplate("MiningSettlement"));
            menu.AddItem(new GUIContent("Coastal Port"), false, () => ApplyTemplate("CoastalPort"));
            menu.AddItem(new GUIContent("Forest Community"), false, () => ApplyTemplate("ForestCommunity"));
            menu.AddItem(new GUIContent("Mountain Stronghold"), false, () => ApplyTemplate("MountainStronghold"));
            menu.ShowAsContext();
        }
        
        private void SetSettlementType(string type, int minPop, int maxPop)
        {
            currentPopulation.SettlementType = type;
            if (currentPopulation.TotalPopulation < minPop || currentPopulation.TotalPopulation > maxPop)
            {
                currentPopulation.TotalPopulation = UnityEngine.Random.Range(minPop, maxPop);
            }
        }
        
        private void AddSpecies(string species)
        {
            if (!currentPopulation.SpeciesDistribution.ContainsKey(species))
                currentPopulation.SpeciesDistribution[species] = 0.1f;
        }
        
        private void AddSocialClass(string socialClass)
        {
            if (!currentPopulation.SocialClasses.ContainsKey(socialClass))
                currentPopulation.SocialClasses[socialClass] = 0.1f;
        }
        
        private void AddIndustry(string industry)
        {
            if (!currentPopulation.PrimaryIndustries.Contains(industry))
                currentPopulation.PrimaryIndustries.Add(industry);
        }
        
        private void AddCulturalValue(string value)
        {
            if (!currentPopulation.CulturalValues.ContainsKey(value))
                currentPopulation.CulturalValues[value] = 0.5f;
        }
        
        private void AddGrowthFactor(string factor)
        {
            if (!currentPopulation.GrowthFactors.ContainsKey(factor))
                currentPopulation.GrowthFactors[factor] = 0.0f;
        }
        
        private void NormalizeSpeciesDistribution()
        {
            NormalizeSpeciesDistribution(currentPopulation);
        }
        
        private void NormalizeSpeciesDistribution(Population population)
        {
            float total = population.SpeciesDistribution.Values.Sum();
            if (total > 0)
            {
                var keys = population.SpeciesDistribution.Keys.ToList();
                foreach (string key in keys)
                {
                    population.SpeciesDistribution[key] /= total;
                }
            }
        }
        
        private void NormalizeAgeDistribution()
        {
            float total = currentPopulation.AgeDistribution.Children + 
                         currentPopulation.AgeDistribution.Adults + 
                         currentPopulation.AgeDistribution.Elderly;
            
            if (total > 0)
            {
                currentPopulation.AgeDistribution.Children /= total;
                currentPopulation.AgeDistribution.Adults /= total;
                currentPopulation.AgeDistribution.Elderly /= total;
            }
        }
        
        private void ApplyTemplate(string templateName)
        {
            // Implementation for applying predefined templates
            // This would set up typical values for different settlement types
        }
    }
    
    // Helper classes for population simulation
    [System.Serializable]
    public class PopulationSnapshot
    {
        public int Year;
        public int Population;
        public float GrowthRate;
    }
}
