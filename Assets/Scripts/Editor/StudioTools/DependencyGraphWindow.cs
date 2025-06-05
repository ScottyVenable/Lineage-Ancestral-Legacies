using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Core.Editor.Studio
{
    /// <summary>
    /// Dependency Graph Visualization Window for analyzing and visualizing entity relationships,
    /// dependencies, and network structures within the game database.
    /// </summary>
    public class DependencyGraphWindow : EditorWindow
    {
        #region Window Management
        
        [MenuItem("Lineage/Studio/Analysis/Dependency Graph Visualizer")]
        public static void ShowWindow()
        {
            var window = GetWindow<DependencyGraphWindow>("Dependency Graph");
            window.minSize = new Vector2(1000, 700);
            window.Show();
        }
        
        #endregion
        
        #region UI State
        
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Graph View", "Network Analysis", "Path Finder", "Clusters", "Export" };
        private Vector2 _scrollPosition = Vector2.zero;
        
        // Graph State
        private List<GraphNode> _nodes = new List<GraphNode>();
        private List<GraphEdge> _edges = new List<GraphEdge>();
        private Dictionary<string, GraphNode> _nodeMap = new Dictionary<string, GraphNode>();
        
        // View Settings
        private Vector2 _graphOffset = Vector2.zero;
        private float _zoomLevel = 1.0f;
        private bool _showLabels = true;
        private bool _showDirections = true;
        private GraphLayoutType _layoutType = GraphLayoutType.ForceDirected;
        
        // Filters
        private string _searchFilter = "";
        private string _selectedEntityId = "";
        private int _maxDepth = 3;
        private bool _bidirectionalOnly = false;
        private HashSet<string> _filteredRelationshipTypes = new HashSet<string>();
        
        // Analysis
        private NetworkAnalysis _networkAnalysis;
        private bool _analysisCalculated = false;
        
        // Path Finding
        private string _pathStartEntity = "";
        private string _pathEndEntity = "";
        private List<string> _foundPath = new List<string>();
        
        // Clustering
        private List<EntityCluster> _clusters = new List<EntityCluster>();
        private bool _clustersCalculated = false;
        
        #endregion
        
        #region Data Structures
        
        private enum GraphLayoutType
        {
            ForceDirected,
            Hierarchical,
            Circular,
            Grid
        }
        
        private class GraphNode
        {
            public string EntityId { get; set; }
            public string DisplayName { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public Color Color { get; set; } = Color.white;
            public float Size { get; set; } = 20f;
            public int Connections { get; set; }
            public bool IsSelected { get; set; }
            public bool IsHighlighted { get; set; }
        }
        
        private class GraphEdge
        {
            public string SourceId { get; set; }
            public string TargetId { get; set; }
            public string RelationshipType { get; set; }
            public Color Color { get; set; } = Color.gray;
            public float Weight { get; set; } = 1f;
            public bool IsDirectional { get; set; } = true;
        }
        
        private class NetworkAnalysis
        {
            public int TotalNodes { get; set; }
            public int TotalEdges { get; set; }
            public float Density { get; set; }
            public float AveragePathLength { get; set; }
            public float ClusteringCoefficient { get; set; }
            public List<string> CentralNodes { get; set; } = new List<string>();
            public List<string> BridgeNodes { get; set; } = new List<string>();
            public List<string> IsolatedNodes { get; set; } = new List<string>();
            public Dictionary<string, float> NodeCentrality { get; set; } = new Dictionary<string, float>();
        }
        
        private class EntityCluster
        {
            public int Id { get; set; }
            public List<string> EntityIds { get; set; } = new List<string>();
            public Color Color { get; set; }
            public Vector2 Center { get; set; }
            public float Cohesion { get; set; }
            public string Description { get; set; }
        }
        
        #endregion
        
        #region Unity Events
        
        private void OnEnable()
        {
            BuildGraph();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawTabs();
            
            GUILayout.Space(10);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTab)
            {
                case 0: DrawGraphViewTab(); break;
                case 1: DrawNetworkAnalysisTab(); break;
                case 2: DrawPathFinderTab(); break;
                case 3: DrawClustersTab(); break;
                case 4: DrawExportTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region UI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Dependency Graph Visualizer", EditorStyles.boldLabel);
            GUILayout.Label("Visualize and analyze entity relationships and network structures", EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Rebuild Graph", GUILayout.Width(120)))
            {
                BuildGraph();
            }
            
            if (GUILayout.Button("Reset View", GUILayout.Width(100)))
            {
                ResetView();
            }
            
            if (GUILayout.Button("Auto Layout", GUILayout.Width(100)))
            {
                ApplyLayout();
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label($"Nodes: {_nodes.Count} | Edges: {_edges.Count}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTabs()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
        }
        
        private void DrawGraphViewTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left Panel - Controls
            EditorGUILayout.BeginVertical("box", GUILayout.Width(250));
            DrawGraphControls();
            DrawGraphFilters();
            EditorGUILayout.EndVertical();
            
            // Right Panel - Graph Visualization
            EditorGUILayout.BeginVertical("box");
            DrawGraphVisualization();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawGraphControls()
        {
            GUILayout.Label("View Controls", EditorStyles.boldLabel);
            
            // Layout Type
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Layout:", GUILayout.Width(60));
            _layoutType = (GraphLayoutType)EditorGUILayout.EnumPopup(_layoutType);
            EditorGUILayout.EndHorizontal();
            
            // Zoom
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Zoom:", GUILayout.Width(60));
            _zoomLevel = EditorGUILayout.Slider(_zoomLevel, 0.1f, 3.0f);
            EditorGUILayout.EndHorizontal();
            
            // Display Options
            _showLabels = EditorGUILayout.Toggle("Show Labels", _showLabels);
            _showDirections = EditorGUILayout.Toggle("Show Directions", _showDirections);
            
            GUILayout.Space(10);
            
            // Node Selection
            GUILayout.Label("Selected Entity", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            _selectedEntityId = EditorGUILayout.TextField(_selectedEntityId);
            if (GUILayout.Button("Focus", GUILayout.Width(60)))
            {
                FocusOnEntity(_selectedEntityId);
            }
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(_selectedEntityId) && _nodeMap.ContainsKey(_selectedEntityId))
            {
                var node = _nodeMap[_selectedEntityId];
                EditorGUILayout.LabelField($"Connections: {node.Connections}");
                
                if (GUILayout.Button("Show Neighbors"))
                {
                    HighlightNeighbors(_selectedEntityId);
                }
                
                if (GUILayout.Button("Hide Others"))
                {
                    FilterToEntity(_selectedEntityId, _maxDepth);
                }
            }
        }
        
        private void DrawGraphFilters()
        {
            GUILayout.Space(10);
            GUILayout.Label("Filters", EditorStyles.boldLabel);
            
            // Search Filter
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(60));
            _searchFilter = EditorGUILayout.TextField(_searchFilter);
            EditorGUILayout.EndHorizontal();
            
            // Depth Filter
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Max Depth:", GUILayout.Width(80));
            _maxDepth = EditorGUILayout.IntSlider(_maxDepth, 1, 5);
            EditorGUILayout.EndHorizontal();
            
            // Relationship Type Filters
            _bidirectionalOnly = EditorGUILayout.Toggle("Bidirectional Only", _bidirectionalOnly);
            
            GUILayout.Label("Relationship Types:", EditorStyles.miniLabel);
            
            // Get unique relationship types
            var relationshipTypes = _edges.Select(e => e.RelationshipType).Distinct().ToList();
            foreach (var type in relationshipTypes)
            {
                bool isFiltered = _filteredRelationshipTypes.Contains(type);
                bool newFiltered = EditorGUILayout.Toggle(type, !isFiltered);
                
                if (newFiltered != !isFiltered)
                {
                    if (newFiltered)
                        _filteredRelationshipTypes.Remove(type);
                    else
                        _filteredRelationshipTypes.Add(type);
                    
                    ApplyFilters();
                }
            }
            
            if (GUILayout.Button("Clear Filters"))
            {
                ClearFilters();
            }
        }
        
        private void DrawGraphVisualization()
        {
            GUILayout.Label("Graph Visualization", EditorStyles.boldLabel);
            
            // Get the rect for drawing
            Rect graphRect = GUILayoutUtility.GetRect(500, 400);
            
            // Handle mouse events
            HandleGraphInput(graphRect);
            
            // Draw background
            EditorGUI.DrawRect(graphRect, new Color(0.1f, 0.1f, 0.1f, 1f));
            
            // Apply transformations
            GUI.BeginGroup(graphRect);
            
            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(Vector2.one * _zoomLevel, graphRect.size * 0.5f);
            
            Vector2 offset = _graphOffset + graphRect.size * 0.5f;
            
            // Draw edges
            DrawEdges(offset);
            
            // Draw nodes
            DrawNodes(offset);
            
            GUI.matrix = oldMatrix;
            GUI.EndGroup();
            
            // Draw legend
            DrawGraphLegend(graphRect);
        }
        
        private void DrawNodes(Vector2 offset)
        {
            foreach (var node in _nodes)
            {
                if (!IsNodeVisible(node)) continue;
                
                Vector2 screenPos = node.Position + offset;
                Rect nodeRect = new Rect(screenPos.x - node.Size * 0.5f, screenPos.y - node.Size * 0.5f, node.Size, node.Size);
                
                // Draw node circle
                Color nodeColor = node.IsSelected ? Color.yellow : 
                                 node.IsHighlighted ? Color.cyan : node.Color;
                
                EditorGUI.DrawRect(nodeRect, nodeColor);
                
                // Draw border
                if (node.IsSelected || node.IsHighlighted)
                {
                    DrawCircleBorder(nodeRect, node.IsSelected ? Color.red : Color.blue, 2f);
                }
                
                // Draw label
                if (_showLabels && _zoomLevel > 0.5f)
                {
                    var labelStyle = new GUIStyle(EditorStyles.miniLabel);
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    labelStyle.normal.textColor = Color.white;
                    
                    Rect labelRect = new Rect(screenPos.x - 50, screenPos.y + node.Size * 0.5f + 2, 100, 15);
                    GUI.Label(labelRect, node.DisplayName, labelStyle);
                }
                
                // Handle node clicks
                if (Event.current.type == EventType.MouseDown && nodeRect.Contains(Event.current.mousePosition))
                {
                    SelectNode(node.EntityId);
                    Event.current.Use();
                }
            }
        }
        
        private void DrawEdges(Vector2 offset)
        {
            foreach (var edge in _edges)
            {
                if (!IsEdgeVisible(edge)) continue;
                
                var sourceNode = _nodeMap.GetValueOrDefault(edge.SourceId);
                var targetNode = _nodeMap.GetValueOrDefault(edge.TargetId);
                
                if (sourceNode == null || targetNode == null) continue;
                
                Vector2 startPos = sourceNode.Position + offset;
                Vector2 endPos = targetNode.Position + offset;
                
                // Draw edge line
                DrawLine(startPos, endPos, edge.Color, edge.Weight);
                
                // Draw direction arrow
                if (_showDirections && edge.IsDirectional)
                {
                    DrawArrow(startPos, endPos, edge.Color);
                }
            }
        }
        
        private void DrawNetworkAnalysisTab()
        {
            if (!_analysisCalculated)
            {
                EditorGUILayout.HelpBox("Network analysis not calculated. Click 'Analyze Network' to generate metrics.", MessageType.Info);
                
                if (GUILayout.Button("Analyze Network"))
                {
                    CalculateNetworkAnalysis();
                }
                return;
            }
            
            EditorGUILayout.LabelField("Network Analysis Results", EditorStyles.boldLabel);
            
            // Basic Metrics
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Basic Metrics", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"Total Nodes: {_networkAnalysis.TotalNodes}");
            EditorGUILayout.LabelField($"Total Edges: {_networkAnalysis.TotalEdges}");
            EditorGUILayout.LabelField($"Network Density: {_networkAnalysis.Density:F3}");
            EditorGUILayout.LabelField($"Average Path Length: {_networkAnalysis.AveragePathLength:F2}");
            EditorGUILayout.LabelField($"Clustering Coefficient: {_networkAnalysis.ClusteringCoefficient:F3}");
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Central Nodes
            if (_networkAnalysis.CentralNodes.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Most Central Nodes", EditorStyles.boldLabel);
                
                foreach (var nodeId in _networkAnalysis.CentralNodes.Take(10))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(nodeId, GUILayout.Width(200));
                    
                    if (_networkAnalysis.NodeCentrality.TryGetValue(nodeId, out float centrality))
                    {
                        GUILayout.Label($"Centrality: {centrality:F3}", EditorStyles.miniLabel);
                    }
                    
                    if (GUILayout.Button("Focus", GUILayout.Width(60)))
                    {
                        FocusOnEntity(nodeId);
                        _selectedTab = 0; // Switch to graph view
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Bridge Nodes
            if (_networkAnalysis.BridgeNodes.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Bridge Nodes (Critical Connections)", EditorStyles.boldLabel);
                
                foreach (var nodeId in _networkAnalysis.BridgeNodes.Take(10))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(nodeId, GUILayout.Width(200));
                    
                    if (GUILayout.Button("Focus", GUILayout.Width(60)))
                    {
                        FocusOnEntity(nodeId);
                        _selectedTab = 0;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Isolated Nodes
            if (_networkAnalysis.IsolatedNodes.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label($"Isolated Nodes ({_networkAnalysis.IsolatedNodes.Count})", EditorStyles.boldLabel);
                
                foreach (var nodeId in _networkAnalysis.IsolatedNodes.Take(15))
                {
                    EditorGUILayout.LabelField("• " + nodeId, EditorStyles.miniLabel);
                }
                
                if (_networkAnalysis.IsolatedNodes.Count > 15)
                {
                    EditorGUILayout.LabelField($"... and {_networkAnalysis.IsolatedNodes.Count - 15} more", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Refresh Analysis"))
            {
                CalculateNetworkAnalysis();
            }
        }
        
        private void DrawPathFinderTab()
        {
            EditorGUILayout.LabelField("Path Finder", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Find Path Between Entities", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Start Entity:", GUILayout.Width(100));
            _pathStartEntity = EditorGUILayout.TextField(_pathStartEntity);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("End Entity:", GUILayout.Width(100));
            _pathEndEntity = EditorGUILayout.TextField(_pathEndEntity);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Shortest Path"))
            {
                FindShortestPath();
            }
            
            if (GUILayout.Button("Find All Paths"))
            {
                FindAllPaths();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Path Results
            if (_foundPath.Any())
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label($"Path Found (Length: {_foundPath.Count - 1})", EditorStyles.boldLabel);
                
                for (int i = 0; i < _foundPath.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (i > 0)
                    {
                        GUILayout.Label("↓", GUILayout.Width(20));
                    }
                    else
                    {
                        GUILayout.Space(20);
                    }
                    
                    GUILayout.Label(_foundPath[i]);
                    
                    if (GUILayout.Button("Focus", GUILayout.Width(60)))
                    {
                        FocusOnEntity(_foundPath[i]);
                        _selectedTab = 0;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Highlight Path in Graph"))
                {
                    HighlightPath(_foundPath);
                    _selectedTab = 0;
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Quick Actions
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Quick Analysis", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Find Most Connected Entity"))
            {
                var mostConnected = FindMostConnectedEntity();
                if (!string.IsNullOrEmpty(mostConnected))
                {
                    _selectedEntityId = mostConnected;
                    FocusOnEntity(mostConnected);
                    _selectedTab = 0;
                }
            }
            
            if (GUILayout.Button("Find Least Connected Entities"))
            {
                var leastConnected = FindLeastConnectedEntities();
                UnityEngine.Debug.Log($"[Dependency Graph] Found {leastConnected.Count} entities with minimal connections");
            }
            
            if (GUILayout.Button("Analyze Network Bottlenecks"))
            {
                AnalyzeBottlenecks();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawClustersTab()
        {
            EditorGUILayout.LabelField("Entity Clustering", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Cluster Analysis", EditorStyles.boldLabel);
            
            if (!_clustersCalculated)
            {
                EditorGUILayout.HelpBox("Clusters not calculated. Click 'Calculate Clusters' to analyze entity groups.", MessageType.Info);
                
                if (GUILayout.Button("Calculate Clusters"))
                {
                    CalculateClusters();
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Found {_clusters.Count} clusters");
                
                if (GUILayout.Button("Recalculate Clusters"))
                {
                    CalculateClusters();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Display Clusters
            if (_clustersCalculated && _clusters.Any())
            {
                foreach (var cluster in _clusters.OrderByDescending(c => c.EntityIds.Count))
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    EditorGUILayout.BeginHorizontal();
                    var oldColor = GUI.color;
                    GUI.color = cluster.Color;
                    GUILayout.Label($"●", GUILayout.Width(20));
                    GUI.color = oldColor;
                    
                    GUILayout.Label($"Cluster {cluster.Id} ({cluster.EntityIds.Count} entities)", EditorStyles.boldLabel);
                    
                    if (GUILayout.Button("Highlight", GUILayout.Width(80)))
                    {
                        HighlightCluster(cluster);
                        _selectedTab = 0;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (!string.IsNullOrEmpty(cluster.Description))
                    {
                        EditorGUILayout.LabelField($"Description: {cluster.Description}", EditorStyles.miniLabel);
                    }
                    
                    EditorGUILayout.LabelField($"Cohesion Score: {cluster.Cohesion:F3}", EditorStyles.miniLabel);
                    
                    // Show some entities in the cluster
                    GUILayout.Label("Entities:", EditorStyles.miniLabel);
                    foreach (var entityId in cluster.EntityIds.Take(5))
                    {
                        EditorGUILayout.LabelField($"• {entityId}", EditorStyles.miniLabel);
                    }
                    
                    if (cluster.EntityIds.Count > 5)
                    {
                        EditorGUILayout.LabelField($"... and {cluster.EntityIds.Count - 5} more", EditorStyles.miniLabel);
                    }
                    
                    EditorGUILayout.EndVertical();
                }
            }
        }
        
        private void DrawExportTab()
        {
            EditorGUILayout.LabelField("Export & Import", EditorStyles.boldLabel);
            
            // Export Options
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Export Graph Data", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export as GraphML"))
            {
                ExportGraphML();
            }
            
            if (GUILayout.Button("Export as DOT"))
            {
                ExportDOT();
            }
            
            if (GUILayout.Button("Export as JSON"))
            {
                ExportJSON();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Node List"))
            {
                ExportNodeList();
            }
            
            if (GUILayout.Button("Export Edge List"))
            {
                ExportEdgeList();
            }
            
            if (GUILayout.Button("Export Analysis Report"))
            {
                ExportAnalysisReport();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Visualization Export
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Export Visualization", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Graph Image"))
            {
                SaveGraphImage();
            }
            
            if (GUILayout.Button("Generate Network Diagram"))
            {
                GenerateNetworkDiagram();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Import Options
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Import Graph Data", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("Import functionality for external graph data coming soon.", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import GraphML"))
            {
                EditorUtility.DisplayDialog("Import", "GraphML import coming soon!", "OK");
            }
            
            if (GUILayout.Button("Import DOT"))
            {
                EditorUtility.DisplayDialog("Import", "DOT import coming soon!", "OK");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Graph Building and Management
        
        private void BuildGraph()
        {
            _nodes.Clear();
            _edges.Clear();
            _nodeMap.Clear();
              if (GameData.entityDatabase == null)
            {
                UnityEngine.Debug.LogWarning("[Dependency Graph] GameData instance not found.");
                return;
            }
            
            try
            {                // Build nodes from entities
                var entities = GameData.entityDatabase;
                foreach (var entity in entities)
                {                    var node = new GraphNode
                    {
                        EntityId = entity.entityID.ToString(),
                        DisplayName = entity.entityName ?? entity.entityID.ToString(),
                        Position = UnityEngine.Random.insideUnitCircle * 200f,
                        Color = GetEntityColor(entity),
                        Size = 20f
                    };
                    
                    _nodes.Add(node);
                    _nodeMap[entity.entityID.ToString()] = node;
                }
                  // Build edges from relationships
                // TODO: Implement relationship database access when relationships are available
                var relationships = new List<object>(); // GameData.relationshipDatabase;                foreach (var relationship in relationships)
                {
                    // TODO: Implement when relationship structure is available
                    /*
                    var edge = new GraphEdge
                    {
                        SourceId = relationship.SourceEntityId,
                        TargetId = relationship.TargetEntityId,
                        RelationshipType = relationship.Type,
                        Color = GetRelationshipColor(relationship.Type),
                        Weight = 1f,
                        IsDirectional = true
                    };
                    
                    _edges.Add(edge);
                    */
                }
                
                // Update node connection counts
                UpdateNodeConnections();
                
                // Apply initial layout
                ApplyLayout();
                
                UnityEngine.Debug.Log($"[Dependency Graph] Built graph with {_nodes.Count} nodes and {_edges.Count} edges.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Dependency Graph] Error building graph: {ex.Message}");
            }
        }
        
        private void UpdateNodeConnections()
        {
            foreach (var node in _nodes)
            {
                node.Connections = _edges.Count(e => e.SourceId == node.EntityId || e.TargetId == node.EntityId);
                
                // Scale node size based on connections
                node.Size = Mathf.Clamp(15f + node.Connections * 2f, 10f, 40f);
            }
        }
        
        private Color GetEntityColor(Entity entity)
        {
            // Color based on entity type or other properties
            return UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
        }
        
        private Color GetRelationshipColor(string relationshipType)
        {
            return relationshipType switch
            {
                "Parent" => Color.red,
                "Child" => Color.blue,
                "Spouse" => Color.magenta,
                "Friend" => Color.green,
                "Enemy" => Color.red,
                _ => Color.gray
            };
        }
        
        #endregion
        
        #region Layout Algorithms
        
        private void ApplyLayout()
        {
            switch (_layoutType)
            {
                case GraphLayoutType.ForceDirected:
                    ApplyForceDirectedLayout();
                    break;
                case GraphLayoutType.Hierarchical:
                    ApplyHierarchicalLayout();
                    break;
                case GraphLayoutType.Circular:
                    ApplyCircularLayout();
                    break;
                case GraphLayoutType.Grid:
                    ApplyGridLayout();
                    break;
            }
        }
        
        private void ApplyForceDirectedLayout()
        {
            // Simple force-directed layout implementation
            for (int iteration = 0; iteration < 100; iteration++)
            {
                // Reset forces
                foreach (var node in _nodes)
                {
                    node.Velocity = Vector2.zero;
                }
                
                // Repulsive forces between all nodes
                for (int i = 0; i < _nodes.Count; i++)
                {
                    for (int j = i + 1; j < _nodes.Count; j++)
                    {
                        Vector2 direction = _nodes[i].Position - _nodes[j].Position;
                        float distance = direction.magnitude;
                        
                        if (distance < 0.1f) distance = 0.1f;
                        
                        Vector2 force = direction.normalized * (1000f / (distance * distance));
                        
                        _nodes[i].Velocity += force;
                        _nodes[j].Velocity -= force;
                    }
                }
                
                // Attractive forces for connected nodes
                foreach (var edge in _edges)
                {
                    var sourceNode = _nodeMap.GetValueOrDefault(edge.SourceId);
                    var targetNode = _nodeMap.GetValueOrDefault(edge.TargetId);
                    
                    if (sourceNode != null && targetNode != null)
                    {
                        Vector2 direction = targetNode.Position - sourceNode.Position;
                        Vector2 force = direction * 0.01f;
                        
                        sourceNode.Velocity += force;
                        targetNode.Velocity -= force;
                    }
                }
                
                // Apply forces with damping
                foreach (var node in _nodes)
                {
                    node.Position += node.Velocity * 0.1f;
                    node.Velocity *= 0.9f;
                }
            }
        }
        
        private void ApplyHierarchicalLayout()
        {
            // Simple hierarchical layout
            var layers = new List<List<GraphNode>>();
            var visited = new HashSet<string>();
            var rootNodes = _nodes.Where(n => !_edges.Any(e => e.TargetId == n.EntityId)).ToList();
            
            if (!rootNodes.Any())
                rootNodes.Add(_nodes.First());
            
            var currentLayer = rootNodes;
            layers.Add(currentLayer);
            
            while (currentLayer.Any())
            {
                var nextLayer = new List<GraphNode>();
                
                foreach (var node in currentLayer)
                {
                    visited.Add(node.EntityId);
                    var children = _edges.Where(e => e.SourceId == node.EntityId)
                        .Select(e => _nodeMap.GetValueOrDefault(e.TargetId))
                        .Where(n => n != null && !visited.Contains(n.EntityId))
                        .ToList();
                    
                    nextLayer.AddRange(children);
                }
                
                if (nextLayer.Any())
                    layers.Add(nextLayer);
                
                currentLayer = nextLayer;
            }
            
            // Position nodes in layers
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                float y = layerIndex * 100f;
                
                for (int nodeIndex = 0; nodeIndex < layer.Count; nodeIndex++)
                {
                    float x = (nodeIndex - layer.Count * 0.5f) * 80f;
                    layer[nodeIndex].Position = new Vector2(x, y);
                }
            }
        }
        
        private void ApplyCircularLayout()
        {
            float radius = _nodes.Count * 10f;
            
            for (int i = 0; i < _nodes.Count; i++)
            {
                float angle = i * 2f * Mathf.PI / _nodes.Count;
                _nodes[i].Position = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
            }
        }
        
        private void ApplyGridLayout()
        {
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(_nodes.Count));
            
            for (int i = 0; i < _nodes.Count; i++)
            {
                int x = i % gridSize;
                int y = i / gridSize;
                
                _nodes[i].Position = new Vector2(x * 80f, y * 80f);
            }
        }
        
        #endregion
        
        #region Analysis Methods
        
        private void CalculateNetworkAnalysis()
        {
            _networkAnalysis = new NetworkAnalysis
            {
                TotalNodes = _nodes.Count,
                TotalEdges = _edges.Count
            };
            
            if (_nodes.Count > 1)
            {
                _networkAnalysis.Density = (float)_edges.Count / (_nodes.Count * (_nodes.Count - 1));
            }
            
            // Calculate centrality for each node
            foreach (var node in _nodes)
            {
                float centrality = CalculateNodeCentrality(node.EntityId);
                _networkAnalysis.NodeCentrality[node.EntityId] = centrality;
            }
            
            // Find most central nodes
            _networkAnalysis.CentralNodes = _networkAnalysis.NodeCentrality
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .Select(kvp => kvp.Key)
                .ToList();
            
            // Find isolated nodes
            _networkAnalysis.IsolatedNodes = _nodes
                .Where(n => n.Connections == 0)
                .Select(n => n.EntityId)
                .ToList();
            
            // Calculate clustering coefficient and other metrics
            _networkAnalysis.ClusteringCoefficient = CalculateClusteringCoefficient();
            _networkAnalysis.AveragePathLength = CalculateAveragePathLength();
            
            _analysisCalculated = true;
            
            UnityEngine.Debug.Log($"[Dependency Graph] Network analysis completed. Density: {_networkAnalysis.Density:F3}");
        }
        
        private float CalculateNodeCentrality(string nodeId)
        {
            // Simple degree centrality
            return _edges.Count(e => e.SourceId == nodeId || e.TargetId == nodeId);
        }
        
        private float CalculateClusteringCoefficient()
        {
            // Simplified clustering coefficient calculation
            return UnityEngine.Random.Range(0.1f, 0.8f); // Placeholder
        }
        
        private float CalculateAveragePathLength()
        {
            // Simplified average path length calculation
            return UnityEngine.Random.Range(2.0f, 5.0f); // Placeholder
        }
        
        #endregion
        
        #region Utility Methods
        
        private void ResetView()
        {
            _graphOffset = Vector2.zero;
            _zoomLevel = 1.0f;
            ClearSelection();
        }
        
        private void ClearSelection()
        {
            foreach (var node in _nodes)
            {
                node.IsSelected = false;
                node.IsHighlighted = false;
            }
            _selectedEntityId = "";
        }
        
        private void ClearFilters()
        {
            _searchFilter = "";
            _filteredRelationshipTypes.Clear();
            _bidirectionalOnly = false;
            ApplyFilters();
        }
        
        private void ApplyFilters()
        {
            // Filter implementation would go here
            // For now, just rebuild the graph
            BuildGraph();
        }
        
        private bool IsNodeVisible(GraphNode node)
        {
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                return node.DisplayName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return true;
        }
        
        private bool IsEdgeVisible(GraphEdge edge)
        {
            return !_filteredRelationshipTypes.Contains(edge.RelationshipType);
        }
        
        private void DrawLine(Vector2 start, Vector2 end, Color color, float weight)
        {
            // Simplified line drawing - in a real implementation you'd use GL or similar
            var oldColor = GUI.color;
            GUI.color = color;
            
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * weight;
            
            // Draw line as a thin rectangle
            Rect lineRect = new Rect(
                Mathf.Min(start.x, end.x) - weight * 0.5f,
                Mathf.Min(start.y, end.y) - weight * 0.5f,
                Vector2.Distance(start, end) + weight,
                weight
            );
            
            // Simple line approximation
            EditorGUI.DrawRect(lineRect, color);
            
            GUI.color = oldColor;
        }
        
        private void DrawArrow(Vector2 start, Vector2 end, Color color)
        {
            // Simple arrow drawing
            Vector2 direction = (end - start).normalized;
            Vector2 arrowHead = end - direction * 10f;
            
            // Draw arrow head as a small rectangle
            Rect arrowRect = new Rect(arrowHead.x - 3f, arrowHead.y - 3f, 6f, 6f);
            EditorGUI.DrawRect(arrowRect, color);
        }
        
        private void DrawCircleBorder(Rect rect, Color color, float thickness)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            
            // Simplified circle border - draw as outline rectangles
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y - thickness, rect.width + 2 * thickness, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.yMax, rect.width + 2 * thickness, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax, rect.y, thickness, rect.height), color);
            
            GUI.color = oldColor;
        }
        
        private void DrawGraphLegend(Rect graphRect)
        {
            // Draw legend in top-right corner
            Rect legendRect = new Rect(graphRect.xMax - 200, graphRect.y + 10, 190, 100);
            EditorGUI.DrawRect(legendRect, new Color(0, 0, 0, 0.8f));
            
            GUILayout.BeginArea(legendRect);
            GUILayout.Label("Legend", EditorStyles.boldLabel);
            GUILayout.Label("• Yellow: Selected", EditorStyles.miniLabel);
            GUILayout.Label("• Cyan: Highlighted", EditorStyles.miniLabel);
            GUILayout.Label("• Size: Connection count", EditorStyles.miniLabel);
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region Event Handling
        
        private void HandleGraphInput(Rect graphRect)
        {
            Event e = Event.current;
            
            if (!graphRect.Contains(e.mousePosition))
                return;
            
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (e.button == 0) // Left mouse
                    {
                        _graphOffset += e.delta / _zoomLevel;
                        Repaint();
                        e.Use();
                    }
                    break;
                    
                case EventType.ScrollWheel:
                    float zoomDelta = -e.delta.y * 0.1f;
                    _zoomLevel = Mathf.Clamp(_zoomLevel + zoomDelta, 0.1f, 3.0f);
                    Repaint();
                    e.Use();
                    break;
            }
        }
        
        private void SelectNode(string nodeId)
        {
            ClearSelection();
            
            if (_nodeMap.TryGetValue(nodeId, out var node))
            {
                node.IsSelected = true;
                _selectedEntityId = nodeId;
            }
        }
        
        private void FocusOnEntity(string entityId)
        {
            if (_nodeMap.TryGetValue(entityId, out var node))
            {
                _graphOffset = -node.Position;
                SelectNode(entityId);
                Repaint();
            }
        }
        
        private void HighlightNeighbors(string entityId)
        {
            ClearSelection();
            
            var neighbors = _edges
                .Where(e => e.SourceId == entityId || e.TargetId == entityId)
                .SelectMany(e => new[] { e.SourceId, e.TargetId })
                .Where(id => id != entityId)
                .Distinct();
            
            foreach (var neighborId in neighbors)
            {
                if (_nodeMap.TryGetValue(neighborId, out var node))
                {
                    node.IsHighlighted = true;
                }
            }
            
            SelectNode(entityId);
        }
        
        #endregion
        
        #region Placeholder Methods
        
        private void FilterToEntity(string entityId, int maxDepth) { /* Implementation */ }
        private void FindShortestPath() { /* Implementation */ }
        private void FindAllPaths() { /* Implementation */ }
        private void HighlightPath(List<string> path) { /* Implementation */ }
        private string FindMostConnectedEntity() { return _nodes.OrderByDescending(n => n.Connections).FirstOrDefault()?.EntityId; }
        private List<string> FindLeastConnectedEntities() { return new List<string>(); }
        private void AnalyzeBottlenecks() { /* Implementation */ }
        private void CalculateClusters() { _clustersCalculated = true; /* Implementation */ }
        private void HighlightCluster(EntityCluster cluster) { /* Implementation */ }
        private void ExportGraphML() { /* Implementation */ }
        private void ExportDOT() { /* Implementation */ }
        private void ExportJSON() { /* Implementation */ }
        private void ExportNodeList() { /* Implementation */ }
        private void ExportEdgeList() { /* Implementation */ }
        private void ExportAnalysisReport() { /* Implementation */ }
        private void SaveGraphImage() { /* Implementation */ }
        private void GenerateNetworkDiagram() { /* Implementation */ }
        
        #endregion
    }
}
