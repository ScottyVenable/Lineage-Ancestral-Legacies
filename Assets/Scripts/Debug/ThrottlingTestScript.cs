using UnityEngine;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// Test script to demonstrate the debug message throttling functionality.
    /// Attach this to any GameObject to test the throttling system.
    /// </summary>
    public class ThrottlingTestScript : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableSpamTest = false;
        [SerializeField] private string testMessage = "Test debug message from Update";
        [SerializeField] private int messagesPerFrame = 1;
        
        private int frameCount = 0;
        
        void Update()
        {
            if (!enableSpamTest) return;
            
            frameCount++;
            
            // Send multiple debug messages per frame to test throttling
            for (int i = 0; i < messagesPerFrame; i++)
            {
                DebugConsoleManager.Instance?.LogToConsole(
                    $"{testMessage} (frame: {frameCount}, msg: {i})", 
                    DebugConsoleManager.LogType.Debug
                );
            }
        }
        
        void LateUpdate()
        {
            if (!enableSpamTest) return;
            
            // Test LateUpdate throttling
            DebugConsoleManager.Instance?.LogToConsole(
                $"LateUpdate message (frame: {frameCount})", 
                DebugConsoleManager.LogType.Debug
            );
        }
        
        void FixedUpdate()
        {
            if (!enableSpamTest) return;
            
            // Test FixedUpdate throttling
            DebugConsoleManager.Instance?.LogToConsole(
                $"FixedUpdate message (frame: {frameCount})", 
                DebugConsoleManager.LogType.Debug
            );
        }
        
        /// <summary>
        /// Test method that can be called from outside Update methods (should not be throttled)
        /// </summary>
        public void TestNonUpdateMessage()
        {
            DebugConsoleManager.Instance?.LogToConsole(
                "This message is NOT from an Update method and should appear immediately", 
                DebugConsoleManager.LogType.Debug
            );
        }
        
        /// <summary>
        /// Test method to send different message types (only Debug should be throttled)
        /// </summary>
        public void TestDifferentMessageTypes()
        {
            DebugConsoleManager.Instance?.LogToConsole(
                "Info message from Update - NOT throttled", 
                DebugConsoleManager.LogType.Info
            );
            
            DebugConsoleManager.Instance?.LogToConsole(
                "Warning message from Update - NOT throttled", 
                DebugConsoleManager.LogType.Warning
            );
            
            DebugConsoleManager.Instance?.LogToConsole(
                "Debug message from Update - THROTTLED", 
                DebugConsoleManager.LogType.Debug
            );
        }
        
        void OnGUI()
        {
            if (!enableSpamTest) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Throttling Test Active");
            GUILayout.Label($"Frame: {frameCount}");
            GUILayout.Label($"Messages per frame: {messagesPerFrame}");
            
            if (GUILayout.Button("Test Non-Update Message"))
            {
                TestNonUpdateMessage();
            }
            
            if (GUILayout.Button("Test Different Message Types"))
            {
                TestDifferentMessageTypes();
            }
            
            if (GUILayout.Button("Stop Test"))
            {
                enableSpamTest = false;
            }
            
            GUILayout.EndArea();
        }
    }
}
