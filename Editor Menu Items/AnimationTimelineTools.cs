using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Lineage.Editor
{
    public static class AnimationTimelineTools
    {
        [MenuItem("Lineage/Animation & Timeline/Animation Clip Analyzer", priority = 800)]
        public static void AnimationClipAnalyzer()
        {
            var window = EditorWindow.GetWindow<AnimationAnalyzerWindow>();
            window.titleContent = new GUIContent("Animation Clip Analyzer");
            window.Show();
        }
        
        [MenuItem("Lineage/Animation & Timeline/Timeline Asset Creator", priority = 801)]
        public static void TimelineAssetCreator()
        {
            var window = EditorWindow.GetWindow<TimelineCreatorWindow>();
            window.titleContent = new GUIContent("Timeline Asset Creator");
            window.Show();
        }
        
        [MenuItem("Lineage/Animation & Timeline/Animation Event Generator", priority = 802)]
        public static void AnimationEventGenerator()
        {
            var selectedClips = Selection.objects.OfType<AnimationClip>().ToArray();
            if (selectedClips.Length == 0)
            {
                EditorUtility.DisplayDialog("No Animation Clips", 
                    "Please select one or more Animation Clips to add events to.", "OK");
                return;
            }
            
            var window = EditorWindow.GetWindow<AnimationEventWindow>();
            window.titleContent = new GUIContent("Animation Event Generator");
            window.SetClips(selectedClips);
            window.Show();
        }
        
        [MenuItem("Lineage/Animation & Timeline/Batch Animation Optimizer", priority = 803)]
        public static void BatchAnimationOptimizer()
        {
            var clips = GetAllAnimationClips();
            int optimized = 0;
            
            foreach (var clip in clips)
            {
                if (OptimizeAnimationClip(clip))
                {
                    optimized++;
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Batch Animation Optimizer: Optimized {optimized} animation clips");
            EditorUtility.DisplayDialog("Optimization Complete", 
                $"Optimized {optimized} animation clips for better performance", "OK");
        }
        
        [MenuItem("Lineage/Animation & Timeline/Animation Curve Smoother", priority = 804)]
        public static void AnimationCurveSmoother()
        {
            var selectedClips = Selection.objects.OfType<AnimationClip>().ToArray();
            if (selectedClips.Length == 0)
            {
                EditorUtility.DisplayDialog("No Animation Clips", 
                    "Please select one or more Animation Clips to smooth.", "OK");
                return;
            }
            
            int smoothed = 0;
            foreach (var clip in selectedClips)
            {
                if (SmoothAnimationCurves(clip))
                {
                    smoothed++;
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Animation Curve Smoother: Smoothed curves in {smoothed} clips");
            EditorUtility.DisplayDialog("Smoothing Complete", 
                $"Smoothed animation curves in {smoothed} clips", "OK");
        }
        
        [MenuItem("Lineage/Animation & Timeline/Generate Animation Report", priority = 805)]
        public static void GenerateAnimationReport()
        {
            var clips = GetAllAnimationClips();
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== LINEAGE ANIMATION REPORT ===");
            report.AppendLine($"Generated: {System.DateTime.Now}");
            report.AppendLine();
            
            report.AppendLine($"ANIMATION CLIPS ({clips.Length}):");
            
            var totalLength = 0f;
            var totalKeyframes = 0;
            
            foreach (var clip in clips)
            {
                var keyframeCount = GetKeyframeCount(clip);
                totalLength += clip.length;
                totalKeyframes += keyframeCount;
                
                report.AppendLine($"  {clip.name}:");
                report.AppendLine($"    Length: {clip.length:F2}s");
                report.AppendLine($"    Keyframes: {keyframeCount}");
                report.AppendLine($"    Loop: {clip.isLooping}");
                report.AppendLine($"    Events: {clip.events.Length}");
                report.AppendLine();
            }
            
            report.AppendLine("SUMMARY:");
            report.AppendLine($"  Total Animation Length: {totalLength:F2}s");
            report.AppendLine($"  Average Clip Length: {(totalLength / clips.Length):F2}s");
            report.AppendLine($"  Total Keyframes: {totalKeyframes}");
            report.AppendLine($"  Average Keyframes per Clip: {(totalKeyframes / clips.Length):F1}");
            
            // Save report
            var reportPath = "Assets/AnimationReport.txt";
            File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            
            Debug.Log($"Animation Report generated: {reportPath}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(reportPath);
        }
        
        // Helper methods
        private static AnimationClip[] GetAllAnimationClips()
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(clip => clip != null)
                .ToArray();
        }
        
        private static bool OptimizeAnimationClip(AnimationClip clip)
        {
            bool changed = false;
            
            // Set compression if not already set
            if (clip.compressionLevel != AnimationClipCompressionLevel.Optimal)
            {
                clip.compressionLevel = AnimationClipCompressionLevel.Optimal;
                changed = true;
            }
            
            // Remove scale curves if they're all 1,1,1
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName.Contains("m_LocalScale"))
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (IsConstantCurve(curve, 1.0f))
                    {
                        AnimationUtility.SetEditorCurve(clip, binding, null);
                        changed = true;
                    }
                }
            }
            
            if (changed)
            {
                EditorUtility.SetDirty(clip);
            }
            
            return changed;
        }
        
        private static bool SmoothAnimationCurves(AnimationClip clip)
        {
            bool changed = false;
            var bindings = AnimationUtility.GetCurveBindings(clip);
            
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve != null && curve.keys.Length > 2)
                {
                    // Smooth the curve by adjusting tangents
                    for (int i = 1; i < curve.keys.Length - 1; i++)
                    {
                        curve.SmoothTangents(i, 0.5f);
                    }
                    
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                    changed = true;
                }
            }
            
            if (changed)
            {
                EditorUtility.SetDirty(clip);
            }
            
            return changed;
        }
        
        private static bool IsConstantCurve(AnimationCurve curve, float value)
        {
            if (curve == null || curve.keys.Length == 0) return false;
            
            foreach (var key in curve.keys)
            {
                if (Mathf.Abs(key.value - value) > 0.001f)
                    return false;
            }
            return true;
        }
        
        private static int GetKeyframeCount(AnimationClip clip)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            int totalKeyframes = 0;
            
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve != null)
                {
                    totalKeyframes += curve.keys.Length;
                }
            }
            
            return totalKeyframes;
        }
    }
    
    // Animation Analyzer Window
    public class AnimationAnalyzerWindow : EditorWindow
    {
        private AnimationClip selectedClip;
        private Vector2 scrollPosition;
        private List<CurveInfo> curveInfos = new List<CurveInfo>();
        
        private struct CurveInfo
        {
            public string path;
            public string property;
            public int keyframes;
            public float duration;
            public bool isConstant;
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Animation Clip Analyzer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            selectedClip = EditorGUILayout.ObjectField("Animation Clip:", selectedClip, typeof(AnimationClip), false) as AnimationClip;
            
            if (GUILayout.Button("Analyze Clip"))
            {
                AnalyzeClip();
            }
            
            if (curveInfos.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Analysis Results for: {selectedClip.name}", EditorStyles.boldLabel);
                GUILayout.Label($"Duration: {selectedClip.length:F2}s | Events: {selectedClip.events.Length} | Looping: {selectedClip.isLooping}");
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var info in curveInfos)
                {
                    var color = info.isConstant ? Color.yellow : Color.white;
                    GUI.color = color;
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{info.path}/{info.property}", GUILayout.Width(200));
                    GUILayout.Label($"{info.keyframes} keys", GUILayout.Width(60));
                    GUILayout.Label($"{info.duration:F2}s", GUILayout.Width(50));
                    if (info.isConstant)
                        GUILayout.Label("CONSTANT", GUILayout.Width(80));
                    GUILayout.EndHorizontal();
                    
                    GUI.color = Color.white;
                }
                
                EditorGUILayout.EndScrollView();
                
                GUILayout.Space(10);
                if (GUILayout.Button("Remove Constant Curves"))
                {
                    RemoveConstantCurves();
                }
            }
        }
        
        private void AnalyzeClip()
        {
            if (selectedClip == null) return;
            
            curveInfos.Clear();
            var bindings = AnimationUtility.GetCurveBindings(selectedClip);
            
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(selectedClip, binding);
                if (curve != null)
                {
                    var info = new CurveInfo
                    {
                        path = binding.path,
                        property = binding.propertyName,
                        keyframes = curve.keys.Length,
                        duration = curve.keys.Length > 0 ? curve.keys[curve.keys.Length - 1].time : 0f,
                        isConstant = IsConstantCurve(curve)
                    };
                    curveInfos.Add(info);
                }
            }
            
            curveInfos = curveInfos.OrderBy(c => c.path).ThenBy(c => c.property).ToList();
        }
        
        private bool IsConstantCurve(AnimationCurve curve)
        {
            if (curve.keys.Length <= 1) return true;
            
            var firstValue = curve.keys[0].value;
            foreach (var key in curve.keys)
            {
                if (Mathf.Abs(key.value - firstValue) > 0.001f)
                    return false;
            }
            return true;
        }
        
        private void RemoveConstantCurves()
        {
            if (selectedClip == null) return;
            
            int removed = 0;
            var bindings = AnimationUtility.GetCurveBindings(selectedClip);
            
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(selectedClip, binding);
                if (curve != null && IsConstantCurve(curve))
                {
                    AnimationUtility.SetEditorCurve(selectedClip, binding, null);
                    removed++;
                }
            }
            
            EditorUtility.SetDirty(selectedClip);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Removed {removed} constant curves from {selectedClip.name}");
            AnalyzeClip(); // Refresh analysis
        }
    }
    
    // Timeline Creator Window
    public class TimelineCreatorWindow : EditorWindow
    {
        private string timelineName = "NewTimeline";
        private GameObject targetGameObject;
        private float duration = 10f;
        private List<string> trackTypes = new List<string> { "Animation", "Audio", "Activation", "Control" };
        private bool[] selectedTracks = new bool[4];
        
        private void OnGUI()
        {
            GUILayout.Label("Timeline Asset Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            timelineName = EditorGUILayout.TextField("Timeline Name:", timelineName);
            targetGameObject = EditorGUILayout.ObjectField("Target GameObject:", targetGameObject, typeof(GameObject), true) as GameObject;
            duration = EditorGUILayout.FloatField("Duration (seconds):", duration);
            
            GUILayout.Space(10);
            GUILayout.Label("Tracks to Create:", EditorStyles.boldLabel);
            
            for (int i = 0; i < trackTypes.Count; i++)
            {
                selectedTracks[i] = EditorGUILayout.Toggle(trackTypes[i] + " Track", selectedTracks[i]);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Timeline"))
            {
                CreateTimeline();
            }
        }
        
        private void CreateTimeline()
        {
            if (string.IsNullOrEmpty(timelineName))
            {
                EditorUtility.DisplayDialog("Invalid Name", "Please enter a valid timeline name", "OK");
                return;
            }
            
            // Create Timeline Asset
            var timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            var assetPath = $"Assets/{timelineName}.playable";
            AssetDatabase.CreateAsset(timelineAsset, assetPath);
            
            // Add selected tracks
            if (selectedTracks[0]) // Animation Track
            {
                var track = timelineAsset.CreateTrack<UnityEngine.Timeline.AnimationTrack>(null, "Animation Track");
            }
            
            if (selectedTracks[1]) // Audio Track
            {
                var track = timelineAsset.CreateTrack<UnityEngine.Timeline.AudioTrack>(null, "Audio Track");
            }
            
            if (selectedTracks[2]) // Activation Track
            {
                var track = timelineAsset.CreateTrack<UnityEngine.Timeline.ActivationTrack>(null, "Activation Track");
            }
            
            if (selectedTracks[3]) // Control Track
            {
                var track = timelineAsset.CreateTrack<UnityEngine.Timeline.ControlTrack>(null, "Control Track");
            }
            
            // Set duration
            timelineAsset.fixedDuration = duration;
            
            // Create PlayableDirector if target GameObject is specified
            if (targetGameObject != null)
            {
                var director = targetGameObject.GetComponent<PlayableDirector>();
                if (director == null)
                {
                    director = targetGameObject.AddComponent<PlayableDirector>();
                }
                director.playableAsset = timelineAsset;
            }
            
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = timelineAsset;
            
            Debug.Log($"Created Timeline: {assetPath}");
            EditorUtility.DisplayDialog("Timeline Created", $"Successfully created timeline: {timelineName}", "OK");
        }
    }
    
    // Animation Event Window
    public class AnimationEventWindow : EditorWindow
    {
        private AnimationClip[] clips;
        private string eventFunctionName = "OnAnimationEvent";
        private float eventTime = 0f;
        private string eventParameter = "";
        private bool addToAllClips = false;
        
        public void SetClips(AnimationClip[] clips)
        {
            this.clips = clips;
        }
        
        private void OnGUI()
        {
            if (clips == null || clips.Length == 0)
            {
                GUILayout.Label("No animation clips selected", EditorStyles.boldLabel);
                return;
            }
            
            GUILayout.Label("Animation Event Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label($"Selected Clips: {clips.Length}");
            foreach (var clip in clips)
            {
                GUILayout.Label($"  • {clip.name} ({clip.length:F2}s)");
            }
            
            GUILayout.Space(10);
            
            eventFunctionName = EditorGUILayout.TextField("Function Name:", eventFunctionName);
            eventTime = EditorGUILayout.FloatField("Event Time:", eventTime);
            eventParameter = EditorGUILayout.TextField("String Parameter:", eventParameter);
            addToAllClips = EditorGUILayout.Toggle("Add to All Clips:", addToAllClips);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Add Animation Event"))
            {
                AddAnimationEvents();
            }
            
            if (GUILayout.Button("Clear All Events"))
            {
                ClearAllEvents();
            }
        }
        
        private void AddAnimationEvents()
        {
            int eventsAdded = 0;
            
            foreach (var clip in clips)
            {
                if (!addToAllClips && clips.Length > 1)
                {
                    // Only add to first clip if not adding to all
                    if (clip != clips[0]) continue;
                }
                
                var animEvent = new AnimationEvent
                {
                    functionName = eventFunctionName,
                    time = Mathf.Clamp(eventTime, 0f, clip.length),
                    stringParameter = eventParameter
                };
                
                // Check if event already exists at this time
                bool eventExists = false;
                foreach (var existingEvent in clip.events)
                {
                    if (Mathf.Abs(existingEvent.time - animEvent.time) < 0.01f && 
                        existingEvent.functionName == animEvent.functionName)
                    {
                        eventExists = true;
                        break;
                    }
                }
                
                if (!eventExists)
                {
                    var events = clip.events.ToList();
                    events.Add(animEvent);
                    AnimationUtility.SetAnimationEvents(clip, events.ToArray());
                    eventsAdded++;
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Added {eventsAdded} animation events");
            EditorUtility.DisplayDialog("Events Added", $"Added {eventsAdded} animation events", "OK");
        }
        
        private void ClearAllEvents()
        {
            int eventsCleared = 0;
            
            foreach (var clip in clips)
            {
                eventsCleared += clip.events.Length;
                AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Cleared {eventsCleared} animation events");
            EditorUtility.DisplayDialog("Events Cleared", $"Cleared {eventsCleared} animation events", "OK");
        }
    }
}
