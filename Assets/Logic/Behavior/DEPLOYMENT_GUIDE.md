# Lineage Behavior System - Complete Integration Guide

## 🎯 Quick Setup (For Existing Pop Entities)

### Step 1: Auto-Setup Pop Behavior Components
1. Select your Pop prefab or existing Pop entities in the scene
2. Add the `PopBehaviorAutoSetup` component 
3. Check `Auto Setup On Start` in the inspector
4. The system will automatically add required components and configure behavior

### Step 2: Verify Required Components
Each Pop entity needs these components (auto-added by PopBehaviorAutoSetup):
- **BehaviorTreeAgent** (Unity Behavior package component)
- **PopBehaviorIntegrator** (our integration layer)
- **BehaviorVisualFeedback** (optional - for debugging)

### Step 3: Configure Behavior Manager
1. Create an empty GameObject named "BehaviorSystemManager"
2. Add the `BehaviorSystemSetup` component
3. Assign behavior tree assets in the inspector:
   - Survivalist Behavior: `Assets/Logic/Behavior/Entity/Templates/SurvivalistBehavior.asset`
   - Warrior Behavior: `Assets/Logic/Behavior/Entity/Templates/WarriorBehavior.asset`
   - Crafter Behavior: `Assets/Logic/Behavior/Entity/Templates/CrafterBehavior.asset`
   - Explorer Behavior: `Assets/Logic/Behavior/Entity/Templates/ExplorerBehavior.asset`
   - Socialite Behavior: `Assets/Logic/Behavior/Entity/Templates/SocialiteBehavior.asset`
4. Click "Setup Behavior System" button in the inspector

## 🔧 Manual Setup (Advanced Users)

### Prerequisites
- Unity Behavior package installed
- NavMesh baked in your scenes
- Pop entities with EntityDataComponent working

### Required Components Per Entity
```csharp
// Core Components (already exist)
Pop
EntityDataComponent
NavMeshAgent

// New Behavior Components (add these)
BehaviorTreeAgent
PopBehaviorIntegrator
BehaviorVisualFeedback (optional)
```

### Behavior Tree Assets Assignment
In PopBehaviorIntegrator inspector:
- **Current Archetype**: Choose from Survivalist, Warrior, Crafter, Explorer, Socialite, Adaptive
- **Auto Assign Behavior**: Enable for automatic assignment
- **Adapt Behavior To Stats**: Enable for dynamic behavior switching

## 🎮 Behavior Archetypes

### 1. Survivalist (Default)
- **Focus**: Basic needs management
- **Behaviors**: Find food/water, rest when tired, avoid danger
- **Best For**: General population, new entities

### 2. Warrior
- **Focus**: Combat and defense
- **Behaviors**: Patrol, attack enemies, defend allies
- **Best For**: Guards, military units

### 3. Crafter
- **Focus**: Building and crafting
- **Behaviors**: Gather materials, work on projects, collaborate
- **Best For**: Builders, artisans

### 4. Explorer
- **Focus**: Discovery and movement
- **Behaviors**: Explore areas, investigate resources, map terrain
- **Best For**: Scouts, researchers

### 5. Socialite
- **Focus**: Social interactions
- **Behaviors**: Seek companions, organize events, resolve conflicts
- **Best For**: Leaders, diplomats

### 6. Adaptive
- **Focus**: Dynamic behavior switching
- **Behaviors**: Changes archetype based on stats and situation
- **Best For**: Advanced AI entities

## 📊 Performance Features

### Level of Detail (LOD) System
- **Close Range**: Full behavior complexity
- **Medium Range**: Reduced behavior updates
- **Far Range**: Minimal behavior processing
- **Off-Screen**: Paused behaviors

### Memory Management
- Automatic blackboard variable cleanup
- Component pooling for large populations
- Optimized stat checking intervals

## 🔍 Debug Features

### Visual Feedback
- Real-time behavior state indicators
- World-space behavior trails
- Debug UI panels showing entity stats
- Color-coded behavior states

### Testing Tools
- `BehaviorIntegrationTester`: Automated test scenarios
- `BehaviorSystemTester`: Performance and stress testing
- Manual behavior triggering for testing

## 💾 Persistence System

### Save/Load Features
- Behavior states persist across sessions
- Blackboard variables saved/restored
- Time-based stat degradation for offline entities
- Compressed save data format

### Configuration
Enable persistence in `BehaviorPersistenceManager`:
```csharp
public bool enablePersistence = true;
public string saveFileName = "behavior_data.json";
public bool enableCompression = true;
```

## 🚀 Advanced Features

### Dynamic Behavior Switching
```csharp
// Change archetype at runtime
popBehaviorIntegrator.ChangeArchetype(BehaviorArchetype.Warrior);

// Enable adaptive mode
popBehaviorIntegrator.SetAdaptiveBehaviorMode(true);
```

### Custom Priority Weights
```csharp
// Adjust behavior priorities
popBehaviorIntegrator.SetPriorityWeight("combat", 0.8f);
popBehaviorIntegrator.SetPriorityWeight("social", 0.3f);
```

### Performance Optimization
```csharp
// Adjust update intervals
behaviorPerformanceManager.SetComplexityLevel(entity, ComplexityLevel.Medium);
```

## 🏷️ Required Game Object Tags

For optimal behavior performance, tag your game objects:
- **"Food"** - Food resources for gathering
- **"Water"** - Water sources for drinking
- **"Enemy"** - Hostile entities for combat
- **"Ally"** - Friendly entities for social interactions
- **"Gatherable"** - General resources for collection
- **"BuildingProject"** - Construction sites for crafters
- **"RestArea"** - Designated rest zones

## 🐛 Troubleshooting

### Common Issues

1. **Entities not moving/behaving**
   - Check NavMeshAgent is enabled and NavMesh is baked
   - Verify BehaviorTreeAgent has a behavior asset assigned
   - Check PopBehaviorIntegrator configuration

2. **Performance issues with many entities**
   - Enable performance optimization in BehaviorPerformanceManager
   - Reduce behavior update intervals
   - Use LOD system for distant entities

3. **Behavior not adapting to stats**
   - Enable "Adapt Behavior To Stats" in PopBehaviorIntegrator
   - Check EntityDataComponent stat values are updating
   - Verify blackboard variables are being set correctly

### Debug Console Commands
```csharp
// Force behavior update
GetComponent<PopBehaviorIntegrator>().ForceUpdateBehavior();

// Check current archetype
Debug.Log(GetComponent<PopBehaviorIntegrator>().GetCurrentArchetype());

// Enable debug logging
GetComponent<PopBehaviorIntegrator>().enableDebugLogging = true;
```

## 📈 Next Steps

1. **Test with a few entities** - Start small and verify everything works
2. **Scale up gradually** - Add more entities and monitor performance
3. **Customize behaviors** - Modify behavior tree assets for your specific needs
4. **Add custom actions** - Create new behavior nodes for unique game mechanics

## 🔗 Related Files

### Core System Files
- `PopBehaviorIntegrator.cs` - Main integration component
- `BehaviorSystemSetup.cs` - Setup and deployment helper
- `BehaviorPerformanceManager.cs` - Performance optimization
- `BehaviorVisualFeedback.cs` - Debug visualization
- `BehaviorPersistenceManager.cs` - Save/load system

### Behavior Tree Assets
- `SurvivalistBehavior.asset` - Basic survival behavior
- `WarriorBehavior.asset` - Combat-focused behavior
- `CrafterBehavior.asset` - Building and crafting behavior
- `ExplorerBehavior.asset` - Exploration behavior
- `SocialiteBehavior.asset` - Social interaction behavior

### Behavior Components
- `/Components/` folder - All Unity Behavior actions and conditions
- `SpecializedBehaviors.cs` - Advanced archetype behaviors
- `CollaborativeActions.cs` - Multi-entity coordination
- `UtilityAI.cs` - Utility-based decision making

This comprehensive behavior system provides intelligent AI for your Lineage Ancestral Legacies entities with full integration to your existing Pop and EntityDataComponent systems.
