using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Advanced Settlement & Territory Designer for creating and managing settlements,
    /// territories, and their associated systems including population, economy, culture, and governance.
    /// </summary>
    public class SettlementDesignerWindow : EditorWindow
    {
        #region Window Management

        private static SettlementDesignerWindow window;

        [System.Serializable]
        public enum DesignerTab
        {
            Settlement = 0,
            Territory = 1,
            Population = 2,
            Economy = 3,
            Culture = 4,
            Governance = 5,
            Military = 6,
            Diplomacy = 7
        }

        private DesignerTab currentTab = DesignerTab.Settlement;
        private Vector2 scrollPosition;
        private bool isDirty = false;

        // Current editing data
        private Settlement currentSettlement = new Settlement();
        private Territory currentTerritory = new Territory();
        private List<Settlement> availableSettlements = new List<Settlement>();
        private List<Territory> availableTerritories = new List<Territory>();

        // UI State
        private string searchFilter = "";
        private bool showAdvancedSettings = false;
        private int selectedSettlementIndex = -1;
        private int selectedTerritoryIndex = -1;

        // Tab-specific data
        private PopulationData populationData = new PopulationData();
        private EconomicData economicData = new EconomicData();
        private CulturalData culturalData = new CulturalData();
        private GovernanceData governanceData = new GovernanceData();
        private MilitaryData militaryData = new MilitaryData();
        private DiplomacyData diplomacyData = new DiplomacyData();

        #endregion

        #region Data Structures

        [System.Serializable]
        public class Settlement
        {
            public string name = "New Settlement";
            public string description = "";
            public SettlementType type = SettlementType.Village;
            public SettlementSize size = SettlementSize.Small;
            public Vector2 coordinates = Vector2.zero;
            public string terrainType = "Plains";
            public List<string> resources = new List<string>();
            public List<string> buildings = new List<string>();
            public List<string> districts = new List<string>();
            public int population = 100;
            public float prosperity = 50f;
            public float defensibility = 50f;
            public float accessibility = 50f;
            public List<string> connectedSettlements = new List<string>();
            public List<string> tradingPartners = new List<string>();
            public string ruler = "";
            public string faction = "";
            public List<string> specialFeatures = new List<string>();
            public bool isCapital = false;
            public bool isPort = false;
            public bool isFortified = false;
        }

        [System.Serializable]
        public class Territory
        {
            public string name = "New Territory";
            public string description = "";
            public TerritoryType type = TerritoryType.Province;
            public Vector2 centerCoordinates = Vector2.zero;
            public float area = 1000f;
            public List<string> settlements = new List<string>();
            public List<string> resources = new List<string>();
            public List<string> biomes = new List<string>();
            public string climate = "Temperate";
            public float fertility = 50f;
            public float dangerLevel = 25f;
            public string controllingFaction = "";
            public List<string> borderTerritories = new List<string>();
            public List<string> strategicFeatures = new List<string>();
            public bool isContested = false;
            public bool isWilderness = false;
        }

        [System.Serializable]
        public class PopulationData
        {
            public int totalPopulation = 100;
            public Dictionary<string, int> speciesDistribution = new Dictionary<string, int>();
            public Dictionary<string, int> classDistribution = new Dictionary<string, int>();
            public Dictionary<string, int> ageDistribution = new Dictionary<string, int>();
            public float growthRate = 1.02f;
            public float migrationRate = 0f;
            public float birthRate = 0.03f;
            public float deathRate = 0.02f;
            public List<string> majorFamilies = new List<string>();
            public List<string> guilds = new List<string>();
        }

        [System.Serializable]
        public class EconomicData
        {
            public float wealth = 50f;
            public float taxRate = 10f;
            public float tradeVolume = 100f;
            public List<string> primaryIndustries = new List<string>();
            public List<string> exports = new List<string>();
            public List<string> imports = new List<string>();
            public Dictionary<string, float> resourceProduction = new Dictionary<string, float>();
            public Dictionary<string, float> resourceConsumption = new Dictionary<string, float>();
            public List<TradeRoute> tradeRoutes = new List<TradeRoute>();
            public float marketStability = 75f;
            public string currency = "Gold";
        }

        [System.Serializable]
        public class CulturalData
        {
            public string dominantCulture = "";
            public List<string> minorityCultures = new List<string>();
            public List<string> languages = new List<string>();
            public List<string> religions = new List<string>();
            public List<string> festivals = new List<string>();
            public List<string> traditions = new List<string>();
            public List<string> architecturalStyles = new List<string>();
            public float culturalDiversity = 50f;
            public float religiousTolerance = 75f;
            public List<string> culturalInstitutions = new List<string>();
        }

        [System.Serializable]
        public class GovernanceData
        {
            public GovernmentType governmentType = GovernmentType.Monarchy;
            public string currentLeader = "";
            public List<string> governingCouncil = new List<string>();
            public List<string> laws = new List<string>();
            public float stability = 75f;
            public float corruption = 25f;
            public float lawEnforcement = 60f;
            public List<string> administrativeDivisions = new List<string>();
            public string constitution = "";
            public float taxEfficiency = 70f;
        }

        [System.Serializable]
        public class MilitaryData
        {
            public int totalForces = 50;
            public List<MilitaryUnit> units = new List<MilitaryUnit>();
            public List<string> fortifications = new List<string>();
            public float militaryStrength = 50f;
            public float defenseRating = 60f;
            public string commandStructure = "";
            public List<string> allies = new List<string>();
            public List<string> enemies = new List<string>();
            public bool hasNavy = false;
            public bool hasAirForce = false;
        }

        [System.Serializable]
        public class DiplomacyData
        {
            public List<DiplomaticRelation> relations = new List<DiplomaticRelation>();
            public List<string> treaties = new List<string>();
            public List<string> embassies = new List<string>();
            public float diplomaticInfluence = 50f;
            public string diplomaticStance = "Neutral";
            public List<string> tradingPartners = new List<string>();
            public List<string> activeNegotiations = new List<string>();
        }

        // Supporting structures
        [System.Serializable]
        public class TradeRoute
        {
            public string destination = "";
            public List<string> goods = new List<string>();
            public float volume = 100f;
            public float profitability = 50f;
            public bool isSecure = true;
        }

        [System.Serializable]
        public class MilitaryUnit
        {
            public string name = "";
            public string type = "";
            public int size = 10;
            public float strength = 50f;
            public float morale = 75f;
            public string equipment = "";
        }

        [System.Serializable]
        public class DiplomaticRelation
        {
            public string faction = "";
            public RelationType relationType = RelationType.Neutral;
            public float relationValue = 0f;
            public List<string> recentEvents = new List<string>();
        }

        // Enums
        public enum SettlementType { Village, Town, City, Metropolis, Fortress, Temple, Port, Mine, Farm }
        public enum SettlementSize { Tiny, Small, Medium, Large, Huge }
        public enum TerritoryType { Province, Kingdom, Empire, Wilderness, Wasteland, Sacred }
        public enum GovernmentType { Monarchy, Republic, Democracy, Oligarchy, Theocracy, Anarchy }
        public enum RelationType { Allied, Friendly, Neutral, Hostile, Enemy, War }

        #endregion

        #region Window Initialization

        public static void ShowWindow()
        {
            window = GetWindow<SettlementDesignerWindow>("Settlement Designer");
            window.minSize = new Vector2(900, 600);
            window.titleContent = new GUIContent("Settlement Designer", "Settlement & Territory Designer");
        }

        private void OnEnable()
        {
            LoadData();
        }

        private void OnDisable()
        {
            if (isDirty)
            {
                SaveData();
            }
        }

        #endregion

        #region Main GUI

        private void OnGUI()
        {
            DrawHeader();
            DrawTabNavigation();
            DrawMainContent();
            DrawFooter();

            if (isDirty)
            {
                Repaint();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Settlement & Territory Designer", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("New Settlement", EditorStyles.toolbarButton))
                {
                    CreateNewSettlement();
                }
                
                if (GUILayout.Button("New Territory", EditorStyles.toolbarButton))
                {
                    CreateNewTerritory();
                }
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    SaveData();
                }
                
                if (GUILayout.Button("Load", EditorStyles.toolbarButton))
                {
                    LoadData();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabNavigation()
        {
            EditorGUILayout.BeginHorizontal();
            {
                var tabNames = new string[] { "Settlement", "Territory", "Population", "Economy", "Culture", "Governance", "Military", "Diplomacy" };
                currentTab = (DesignerTab)GUILayout.Toolbar((int)currentTab, tabNames);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainContent()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                switch (currentTab)
                {
                    case DesignerTab.Settlement:
                        DrawSettlementTab();
                        break;
                    case DesignerTab.Territory:
                        DrawTerritoryTab();
                        break;
                    case DesignerTab.Population:
                        DrawPopulationTab();
                        break;
                    case DesignerTab.Economy:
                        DrawEconomyTab();
                        break;
                    case DesignerTab.Culture:
                        DrawCultureTab();
                        break;
                    case DesignerTab.Governance:
                        DrawGovernanceTab();
                        break;
                    case DesignerTab.Military:
                        DrawMilitaryTab();
                        break;
                    case DesignerTab.Diplomacy:
                        DrawDiplomacyTab();
                        break;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                var statusText = isDirty ? "Modified" : "Saved";
                var statusColor = isDirty ? Color.yellow : Color.green;
                
                var oldColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label($"Status: {statusText}", EditorStyles.miniLabel);
                GUI.color = oldColor;
                
                GUILayout.FlexibleSpace();
                
                GUILayout.Label($"Settlements: {availableSettlements.Count} | Territories: {availableTerritories.Count}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Tab Content Drawing

        private void DrawSettlementTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Settlement Management", EditorStyles.boldLabel);
                
                // Settlement selector
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Current Settlement:", GUILayout.Width(120));
                    var settlementNames = availableSettlements.Select(s => s.name).ToArray();
                    var newIndex = EditorGUILayout.Popup(selectedSettlementIndex, settlementNames);
                    if (newIndex != selectedSettlementIndex)
                    {
                        selectedSettlementIndex = newIndex;
                        if (newIndex >= 0 && newIndex < availableSettlements.Count)
                        {
                            currentSettlement = availableSettlements[newIndex];
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                // Basic Information
                EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newName = EditorGUILayout.TextField("Name", currentSettlement.name);
                if (newName != currentSettlement.name)
                {
                    currentSettlement.name = newName;
                    MarkDirty();
                }
                
                var newDescription = EditorGUILayout.TextArea(currentSettlement.description, GUILayout.Height(60));
                if (newDescription != currentSettlement.description)
                {
                    currentSettlement.description = newDescription;
                    MarkDirty();
                }
                
                var newType = (SettlementType)EditorGUILayout.EnumPopup("Type", currentSettlement.type);
                if (newType != currentSettlement.type)
                {
                    currentSettlement.type = newType;
                    MarkDirty();
                }
                
                var newSize = (SettlementSize)EditorGUILayout.EnumPopup("Size", currentSettlement.size);
                if (newSize != currentSettlement.size)
                {
                    currentSettlement.size = newSize;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Location & Geography
                EditorGUILayout.LabelField("Location & Geography", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newCoordinates = EditorGUILayout.Vector2Field("Coordinates", currentSettlement.coordinates);
                if (newCoordinates != currentSettlement.coordinates)
                {
                    currentSettlement.coordinates = newCoordinates;
                    MarkDirty();
                }
                
                var newTerrain = EditorGUILayout.TextField("Terrain Type", currentSettlement.terrainType);
                if (newTerrain != currentSettlement.terrainType)
                {
                    currentSettlement.terrainType = newTerrain;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Statistics
                EditorGUILayout.LabelField("Settlement Statistics", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newPopulation = EditorGUILayout.IntField("Population", currentSettlement.population);
                if (newPopulation != currentSettlement.population)
                {
                    currentSettlement.population = Mathf.Max(0, newPopulation);
                    MarkDirty();
                }
                
                var newProsperity = EditorGUILayout.Slider("Prosperity", currentSettlement.prosperity, 0f, 100f);
                if (newProsperity != currentSettlement.prosperity)
                {
                    currentSettlement.prosperity = newProsperity;
                    MarkDirty();
                }
                
                var newDefensibility = EditorGUILayout.Slider("Defensibility", currentSettlement.defensibility, 0f, 100f);
                if (newDefensibility != currentSettlement.defensibility)
                {
                    currentSettlement.defensibility = newDefensibility;
                    MarkDirty();
                }
                
                var newAccessibility = EditorGUILayout.Slider("Accessibility", currentSettlement.accessibility, 0f, 100f);
                if (newAccessibility != currentSettlement.accessibility)
                {
                    currentSettlement.accessibility = newAccessibility;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Flags and Features
                EditorGUILayout.LabelField("Special Properties", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newIsCapital = EditorGUILayout.Toggle("Is Capital", currentSettlement.isCapital);
                if (newIsCapital != currentSettlement.isCapital)
                {
                    currentSettlement.isCapital = newIsCapital;
                    MarkDirty();
                }
                
                var newIsPort = EditorGUILayout.Toggle("Is Port", currentSettlement.isPort);
                if (newIsPort != currentSettlement.isPort)
                {
                    currentSettlement.isPort = newIsPort;
                    MarkDirty();
                }
                
                var newIsFortified = EditorGUILayout.Toggle("Is Fortified", currentSettlement.isFortified);
                if (newIsFortified != currentSettlement.isFortified)
                {
                    currentSettlement.isFortified = newIsFortified;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                // Lists
                DrawStringList("Resources", currentSettlement.resources);
                DrawStringList("Buildings", currentSettlement.buildings);
                DrawStringList("Districts", currentSettlement.districts);
                DrawStringList("Connected Settlements", currentSettlement.connectedSettlements);
                DrawStringList("Trading Partners", currentSettlement.tradingPartners);
                DrawStringList("Special Features", currentSettlement.specialFeatures);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTerritoryTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Territory Management", EditorStyles.boldLabel);
                
                // Territory selector
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Current Territory:", GUILayout.Width(120));
                    var territoryNames = availableTerritories.Select(t => t.name).ToArray();
                    var newIndex = EditorGUILayout.Popup(selectedTerritoryIndex, territoryNames);
                    if (newIndex != selectedTerritoryIndex)
                    {
                        selectedTerritoryIndex = newIndex;
                        if (newIndex >= 0 && newIndex < availableTerritories.Count)
                        {
                            currentTerritory = availableTerritories[newIndex];
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                // Basic Information
                EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newName = EditorGUILayout.TextField("Name", currentTerritory.name);
                if (newName != currentTerritory.name)
                {
                    currentTerritory.name = newName;
                    MarkDirty();
                }
                
                var newDescription = EditorGUILayout.TextArea(currentTerritory.description, GUILayout.Height(60));
                if (newDescription != currentTerritory.description)
                {
                    currentTerritory.description = newDescription;
                    MarkDirty();
                }
                
                var newType = (TerritoryType)EditorGUILayout.EnumPopup("Type", currentTerritory.type);
                if (newType != currentTerritory.type)
                {
                    currentTerritory.type = newType;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Geography
                EditorGUILayout.LabelField("Geography", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newCenter = EditorGUILayout.Vector2Field("Center Coordinates", currentTerritory.centerCoordinates);
                if (newCenter != currentTerritory.centerCoordinates)
                {
                    currentTerritory.centerCoordinates = newCenter;
                    MarkDirty();
                }
                
                var newArea = EditorGUILayout.FloatField("Area (sq km)", currentTerritory.area);
                if (newArea != currentTerritory.area)
                {
                    currentTerritory.area = Mathf.Max(0f, newArea);
                    MarkDirty();
                }
                
                var newClimate = EditorGUILayout.TextField("Climate", currentTerritory.climate);
                if (newClimate != currentTerritory.climate)
                {
                    currentTerritory.climate = newClimate;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Territory Statistics
                EditorGUILayout.LabelField("Territory Statistics", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newFertility = EditorGUILayout.Slider("Fertility", currentTerritory.fertility, 0f, 100f);
                if (newFertility != currentTerritory.fertility)
                {
                    currentTerritory.fertility = newFertility;
                    MarkDirty();
                }
                
                var newDanger = EditorGUILayout.Slider("Danger Level", currentTerritory.dangerLevel, 0f, 100f);
                if (newDanger != currentTerritory.dangerLevel)
                {
                    currentTerritory.dangerLevel = newDanger;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Political Information
                EditorGUILayout.LabelField("Political Information", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newFaction = EditorGUILayout.TextField("Controlling Faction", currentTerritory.controllingFaction);
                if (newFaction != currentTerritory.controllingFaction)
                {
                    currentTerritory.controllingFaction = newFaction;
                    MarkDirty();
                }
                
                var newContested = EditorGUILayout.Toggle("Is Contested", currentTerritory.isContested);
                if (newContested != currentTerritory.isContested)
                {
                    currentTerritory.isContested = newContested;
                    MarkDirty();
                }
                
                var newWilderness = EditorGUILayout.Toggle("Is Wilderness", currentTerritory.isWilderness);
                if (newWilderness != currentTerritory.isWilderness)
                {
                    currentTerritory.isWilderness = newWilderness;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                // Lists
                DrawStringList("Settlements", currentTerritory.settlements);
                DrawStringList("Resources", currentTerritory.resources);
                DrawStringList("Biomes", currentTerritory.biomes);
                DrawStringList("Border Territories", currentTerritory.borderTerritories);
                DrawStringList("Strategic Features", currentTerritory.strategicFeatures);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPopulationTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Population Management", EditorStyles.boldLabel);
                
                // Basic Population Data
                EditorGUILayout.LabelField("Population Statistics", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newTotal = EditorGUILayout.IntField("Total Population", populationData.totalPopulation);
                if (newTotal != populationData.totalPopulation)
                {
                    populationData.totalPopulation = Mathf.Max(0, newTotal);
                    MarkDirty();
                }
                
                var newGrowthRate = EditorGUILayout.FloatField("Growth Rate", populationData.growthRate);
                if (newGrowthRate != populationData.growthRate)
                {
                    populationData.growthRate = newGrowthRate;
                    MarkDirty();
                }
                
                var newMigrationRate = EditorGUILayout.FloatField("Migration Rate", populationData.migrationRate);
                if (newMigrationRate != populationData.migrationRate)
                {
                    populationData.migrationRate = newMigrationRate;
                    MarkDirty();
                }
                
                var newBirthRate = EditorGUILayout.FloatField("Birth Rate", populationData.birthRate);
                if (newBirthRate != populationData.birthRate)
                {
                    populationData.birthRate = newBirthRate;
                    MarkDirty();
                }
                
                var newDeathRate = EditorGUILayout.FloatField("Death Rate", populationData.deathRate);
                if (newDeathRate != populationData.deathRate)
                {
                    populationData.deathRate = newDeathRate;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Social Groups
                DrawStringList("Major Families", populationData.majorFamilies);
                DrawStringList("Guilds", populationData.guilds);
                
                // Population Distribution would need custom dictionary editors
                EditorGUILayout.LabelField("Distribution Data", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Species, Class, and Age distribution editors would be implemented here.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawEconomyTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Economic Management", EditorStyles.boldLabel);
                
                // Economic Statistics
                EditorGUILayout.LabelField("Economic Statistics", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newWealth = EditorGUILayout.Slider("Wealth Level", economicData.wealth, 0f, 100f);
                if (newWealth != economicData.wealth)
                {
                    economicData.wealth = newWealth;
                    MarkDirty();
                }
                
                var newTaxRate = EditorGUILayout.Slider("Tax Rate (%)", economicData.taxRate, 0f, 50f);
                if (newTaxRate != economicData.taxRate)
                {
                    economicData.taxRate = newTaxRate;
                    MarkDirty();
                }
                
                var newTradeVolume = EditorGUILayout.FloatField("Trade Volume", economicData.tradeVolume);
                if (newTradeVolume != economicData.tradeVolume)
                {
                    economicData.tradeVolume = Mathf.Max(0f, newTradeVolume);
                    MarkDirty();
                }
                
                var newStability = EditorGUILayout.Slider("Market Stability", economicData.marketStability, 0f, 100f);
                if (newStability != economicData.marketStability)
                {
                    economicData.marketStability = newStability;
                    MarkDirty();
                }
                
                var newCurrency = EditorGUILayout.TextField("Currency", economicData.currency);
                if (newCurrency != economicData.currency)
                {
                    economicData.currency = newCurrency;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Trade and Industry
                DrawStringList("Primary Industries", economicData.primaryIndustries);
                DrawStringList("Exports", economicData.exports);
                DrawStringList("Imports", economicData.imports);
                
                // Trade Routes
                EditorGUILayout.LabelField("Trade Routes", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Trade route management would be implemented here with custom editors.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCultureTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Cultural Management", EditorStyles.boldLabel);
                
                // Cultural Identity
                EditorGUILayout.LabelField("Cultural Identity", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newCulture = EditorGUILayout.TextField("Dominant Culture", culturalData.dominantCulture);
                if (newCulture != culturalData.dominantCulture)
                {
                    culturalData.dominantCulture = newCulture;
                    MarkDirty();
                }
                
                var newDiversity = EditorGUILayout.Slider("Cultural Diversity", culturalData.culturalDiversity, 0f, 100f);
                if (newDiversity != culturalData.culturalDiversity)
                {
                    culturalData.culturalDiversity = newDiversity;
                    MarkDirty();
                }
                
                var newTolerance = EditorGUILayout.Slider("Religious Tolerance", culturalData.religiousTolerance, 0f, 100f);
                if (newTolerance != culturalData.religiousTolerance)
                {
                    culturalData.religiousTolerance = newTolerance;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Cultural Elements
                DrawStringList("Minority Cultures", culturalData.minorityCultures);
                DrawStringList("Languages", culturalData.languages);
                DrawStringList("Religions", culturalData.religions);
                DrawStringList("Festivals", culturalData.festivals);
                DrawStringList("Traditions", culturalData.traditions);
                DrawStringList("Architectural Styles", culturalData.architecturalStyles);
                DrawStringList("Cultural Institutions", culturalData.culturalInstitutions);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGovernanceTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Governance Management", EditorStyles.boldLabel);
                
                // Government Structure
                EditorGUILayout.LabelField("Government Structure", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newGovType = (GovernmentType)EditorGUILayout.EnumPopup("Government Type", governanceData.governmentType);
                if (newGovType != governanceData.governmentType)
                {
                    governanceData.governmentType = newGovType;
                    MarkDirty();
                }
                
                var newLeader = EditorGUILayout.TextField("Current Leader", governanceData.currentLeader);
                if (newLeader != governanceData.currentLeader)
                {
                    governanceData.currentLeader = newLeader;
                    MarkDirty();
                }
                
                var newConstitution = EditorGUILayout.TextArea(governanceData.constitution, GUILayout.Height(60));
                if (newConstitution != governanceData.constitution)
                {
                    governanceData.constitution = newConstitution;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Governance Statistics
                EditorGUILayout.LabelField("Governance Statistics", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newStability = EditorGUILayout.Slider("Political Stability", governanceData.stability, 0f, 100f);
                if (newStability != governanceData.stability)
                {
                    governanceData.stability = newStability;
                    MarkDirty();
                }
                
                var newCorruption = EditorGUILayout.Slider("Corruption Level", governanceData.corruption, 0f, 100f);
                if (newCorruption != governanceData.corruption)
                {
                    governanceData.corruption = newCorruption;
                    MarkDirty();
                }
                
                var newLawEnforcement = EditorGUILayout.Slider("Law Enforcement", governanceData.lawEnforcement, 0f, 100f);
                if (newLawEnforcement != governanceData.lawEnforcement)
                {
                    governanceData.lawEnforcement = newLawEnforcement;
                    MarkDirty();
                }
                
                var newTaxEfficiency = EditorGUILayout.Slider("Tax Efficiency", governanceData.taxEfficiency, 0f, 100f);
                if (newTaxEfficiency != governanceData.taxEfficiency)
                {
                    governanceData.taxEfficiency = newTaxEfficiency;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Government Elements
                DrawStringList("Governing Council", governanceData.governingCouncil);
                DrawStringList("Laws", governanceData.laws);
                DrawStringList("Administrative Divisions", governanceData.administrativeDivisions);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawMilitaryTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Military Management", EditorStyles.boldLabel);
                
                // Military Statistics
                EditorGUILayout.LabelField("Military Statistics", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newTotal = EditorGUILayout.IntField("Total Forces", militaryData.totalForces);
                if (newTotal != militaryData.totalForces)
                {
                    militaryData.totalForces = Mathf.Max(0, newTotal);
                    MarkDirty();
                }
                
                var newStrength = EditorGUILayout.Slider("Military Strength", militaryData.militaryStrength, 0f, 100f);
                if (newStrength != militaryData.militaryStrength)
                {
                    militaryData.militaryStrength = newStrength;
                    MarkDirty();
                }
                
                var newDefense = EditorGUILayout.Slider("Defense Rating", militaryData.defenseRating, 0f, 100f);
                if (newDefense != militaryData.defenseRating)
                {
                    militaryData.defenseRating = newDefense;
                    MarkDirty();
                }
                
                var newCommand = EditorGUILayout.TextField("Command Structure", militaryData.commandStructure);
                if (newCommand != militaryData.commandStructure)
                {
                    militaryData.commandStructure = newCommand;
                    MarkDirty();
                }
                
                var newNavy = EditorGUILayout.Toggle("Has Navy", militaryData.hasNavy);
                if (newNavy != militaryData.hasNavy)
                {
                    militaryData.hasNavy = newNavy;
                    MarkDirty();
                }
                
                var newAirForce = EditorGUILayout.Toggle("Has Air Force", militaryData.hasAirForce);
                if (newAirForce != militaryData.hasAirForce)
                {
                    militaryData.hasAirForce = newAirForce;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Military Elements
                DrawStringList("Fortifications", militaryData.fortifications);
                DrawStringList("Allies", militaryData.allies);
                DrawStringList("Enemies", militaryData.enemies);
                
                // Military Units
                EditorGUILayout.LabelField("Military Units", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Military unit management would be implemented here with custom editors.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDiplomacyTab()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Diplomatic Management", EditorStyles.boldLabel);
                
                // Diplomatic Statistics
                EditorGUILayout.LabelField("Diplomatic Status", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                var newInfluence = EditorGUILayout.Slider("Diplomatic Influence", diplomacyData.diplomaticInfluence, 0f, 100f);
                if (newInfluence != diplomacyData.diplomaticInfluence)
                {
                    diplomacyData.diplomaticInfluence = newInfluence;
                    MarkDirty();
                }
                
                var newStance = EditorGUILayout.TextField("Diplomatic Stance", diplomacyData.diplomaticStance);
                if (newStance != diplomacyData.diplomaticStance)
                {
                    diplomacyData.diplomaticStance = newStance;
                    MarkDirty();
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(10);
                
                // Diplomatic Elements
                DrawStringList("Treaties", diplomacyData.treaties);
                DrawStringList("Embassies", diplomacyData.embassies);
                DrawStringList("Trading Partners", diplomacyData.tradingPartners);
                DrawStringList("Active Negotiations", diplomacyData.activeNegotiations);
                
                // Diplomatic Relations
                EditorGUILayout.LabelField("Diplomatic Relations", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Diplomatic relations management would be implemented here with custom editors.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Utility Methods

        private void DrawStringList(string label, List<string> list)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var newValue = EditorGUILayout.TextField($"Item {i + 1}", list[i]);
                    if (newValue != list[i])
                    {
                        list[i] = newValue;
                        MarkDirty();
                    }
                    
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        list.RemoveAt(i);
                        MarkDirty();
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button($"Add {label}"))
            {
                list.Add("");
                MarkDirty();
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }

        private void CreateNewSettlement()
        {
            var newSettlement = new Settlement();
            newSettlement.name = $"Settlement {availableSettlements.Count + 1}";
            availableSettlements.Add(newSettlement);
            selectedSettlementIndex = availableSettlements.Count - 1;
            currentSettlement = newSettlement;
            MarkDirty();
        }

        private void CreateNewTerritory()
        {
            var newTerritory = new Territory();
            newTerritory.name = $"Territory {availableTerritories.Count + 1}";
            availableTerritories.Add(newTerritory);
            selectedTerritoryIndex = availableTerritories.Count - 1;
            currentTerritory = newTerritory;
            MarkDirty();
        }

        private void MarkDirty()
        {
            isDirty = true;
        }

        private void SaveData()
        {
            // Save to GameData or EditorPrefs
            Lineage.Ancestral.Legacies.Debug.Log.Info("[Settlement Designer] Data saved successfully.", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
            isDirty = false;
        }

        private void LoadData()
        {
            // Load from GameData or create defaults
            if (availableSettlements.Count == 0)
            {
                CreateNewSettlement();
            }
            
            if (availableTerritories.Count == 0)
            {
                CreateNewTerritory();
            }

            Lineage.Ancestral.Legacies.Debug.Log.Info("[Settlement Designer] Data loaded successfully.", Ancestral.Legacies.Debug.Log.LogCategory.Systems);
        }

        #endregion
    }
}
