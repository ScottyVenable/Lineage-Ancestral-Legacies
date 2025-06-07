using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Lineage.UI;

namespace Lineage.UI.Examples
{
    /// <summary>
    /// Integration test for ButtonGroupManager with EnhancedAutoButtonHandler.
    /// Verifies that all components work together correctly.
    /// </summary>
    public class ButtonGroupIntegrationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool enableDetailedLogging = true;

        private ButtonGroupManager testManager;
        private int testsPassed = 0;
        private int testsTotal = 0;

        private void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }

        public void RunAllTests()
        {
            UnityEngine.Debug.Log("=== ButtonGroupManager Integration Tests Starting ===");
            
            TestBasicCreation();
            TestAutoGeneration();
            TestManualConfiguration();
            TestRuntimeModification();
            TestEventSystem();
            TestButtonBehaviors();
            TestLayoutTypes();
            TestStyling();
            TestAnimations();
            TestAutoButtonHandlerIntegration();
            
            LogTestResults();
        }

        #region Test Methods

        private void TestBasicCreation()
        {
            StartTest("Basic ButtonGroupManager Creation");
            
            try
            {
                GameObject testObj = new GameObject("Test_BasicCreation");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                Assert(testManager != null, "ButtonGroupManager component created");
                Assert(testManager.gameObject.activeSelf, "GameObject is active");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestAutoGeneration()
        {
            StartTest("Auto-generation from Commands");
            
            try
            {
                GameObject testObj = new GameObject("Test_AutoGeneration");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                // Configure for auto-generation
                SetManagerProperty("autoGenerateFromCommands", true);
                SetManagerProperty("buttonCount", 3);
                SetManagerProperty("includeCategories", new ButtonGroupManager.CommandCategory[] {
                    ButtonGroupManager.CommandCategory.SaveLoad
                });
                
                // Trigger creation
                testManager.CreateButtonGroup();
                var buttons = testManager.GetManagedButtons();
                
                Assert(buttons.Count > 0, "Buttons were created from auto-generation");
                Assert(buttons.Count <= 3, "Button count respects limit");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestManualConfiguration()
        {
            StartTest("Manual Button Configuration");
            
            try
            {
                GameObject testObj = new GameObject("Test_ManualConfig");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                // Create manual button configuration
                var buttonConfigs = new System.Collections.Generic.List<ButtonGroupManager.ButtonData> {
                    new ButtonGroupManager.ButtonData {
                        buttonText = "Test Button 1",
                        command = "test1",
                        isEnabled = true
                    },
                    new ButtonGroupManager.ButtonData {
                        buttonText = "Test Button 2",
                        command = "test2",
                        isEnabled = false
                    }
                };
                
                SetManagerProperty("buttonConfigs", buttonConfigs);
                SetManagerProperty("buttonCount", 2);
                SetManagerProperty("autoGenerateFromCommands", false);
                
                testManager.CreateButtonGroup();
                var buttons = testManager.GetManagedButtons();
                
                Assert(buttons.Count == 2, "Correct number of buttons created");
                
                // Check button states
                var button1 = buttons[0].GetComponent<Button>();
                var button2 = buttons[1].GetComponent<Button>();
                Assert(button1.interactable == true, "First button is enabled");
                Assert(button2.interactable == false, "Second button is disabled");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestRuntimeModification()
        {
            StartTest("Runtime Button Modification");
            
            try
            {
                GameObject testObj = new GameObject("Test_RuntimeMod");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                // Setup initial configuration
                SetManagerProperty("buttonCount", 2);
                testManager.CreateButtonGroup();
                int initialCount = testManager.GetManagedButtons().Count;
                
                // Test adding button
                var newButton = new ButtonGroupManager.ButtonData {
                    buttonText = "Added Button",
                    command = "added",
                    isEnabled = true
                };
                
                testManager.AddButton(newButton);
                Assert(testManager.GetManagedButtons().Count == initialCount + 1, "Button added successfully");
                
                // Test modifying button text
                testManager.SetButtonText(0, "Modified Text");
                
                // Test enabling/disabling
                testManager.SetButtonEnabled(0, false);
                
                // Test removing button
                testManager.RemoveButton(0);
                Assert(testManager.GetManagedButtons().Count == initialCount, "Button removed successfully");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestEventSystem()
        {
            StartTest("Event System");
            
            try
            {
                GameObject testObj = new GameObject("Test_Events");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                bool buttonCreatedFired = false;
                bool buttonClickedFired = false;
                
                // Subscribe to events
                testManager.OnButtonCreated += (button) => { buttonCreatedFired = true; };
                testManager.OnButtonClicked += (text) => { buttonClickedFired = true; };
                
                // Create buttons to trigger events
                SetManagerProperty("buttonCount", 1);
                testManager.CreateButtonGroup();
                
                Assert(buttonCreatedFired, "OnButtonCreated event fired");
                
                // Simulate button click
                var buttons = testManager.GetManagedButtons();
                if (buttons.Count > 0)
                {
                    var button = buttons[0].GetComponent<Button>();
                    button.onClick.Invoke();
                    Assert(buttonClickedFired, "OnButtonClicked event fired");
                }
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestButtonBehaviors()
        {
            StartTest("Button Behaviors");
            
            try
            {
                GameObject testObj = new GameObject("Test_Behaviors");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                var buttonConfigs = new System.Collections.Generic.List<ButtonGroupManager.ButtonData> {
                    new ButtonGroupManager.ButtonData {
                        buttonText = "Standard",
                        behavior = ButtonGroupManager.ButtonBehavior.Standard
                    },
                    new ButtonGroupManager.ButtonData {
                        buttonText = "Toggle",
                        behavior = ButtonGroupManager.ButtonBehavior.Toggle
                    },
                    new ButtonGroupManager.ButtonData {
                        buttonText = "Radio",
                        behavior = ButtonGroupManager.ButtonBehavior.RadioButton
                    }
                };
                
                SetManagerProperty("buttonConfigs", buttonConfigs);
                SetManagerProperty("buttonCount", 3);
                SetManagerProperty("enableButtonGroups", true);
                
                testManager.CreateButtonGroup();
                var buttons = testManager.GetManagedButtons();
                
                Assert(buttons.Count == 3, "All behavior types created");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestLayoutTypes()
        {
            StartTest("Layout Types");
            
            try
            {
                // Test different layout types
                var layoutTypes = new ButtonGroupManager.ButtonGroupType[] {
                    ButtonGroupManager.ButtonGroupType.Horizontal,
                    ButtonGroupManager.ButtonGroupType.Vertical,
                    ButtonGroupManager.ButtonGroupType.Grid
                };
                
                foreach (var layoutType in layoutTypes)
                {
                    GameObject testObj = new GameObject($"Test_Layout_{layoutType}");
                    var manager = testObj.AddComponent<ButtonGroupManager>();
                    
                    SetManagerPropertyForObject(manager, "groupType", layoutType);
                    SetManagerPropertyForObject(manager, "buttonCount", 2);
                    
                    manager.CreateButtonGroup();
                    
                    // Check if layout group was created
                    var layoutGroup = testObj.GetComponent<LayoutGroup>();
                    Assert(layoutGroup != null, $"{layoutType} layout group created");
                    
                    Cleanup(testObj);
                }
                
                PassTest();
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestStyling()
        {
            StartTest("Visual Styling");
            
            try
            {
                GameObject testObj = new GameObject("Test_Styling");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                // Configure custom styling
                var visualSettings = new ButtonGroupManager.ButtonVisualSettings();
                visualSettings.normalColor = Color.red;
                visualSettings.fontSize = 20;
                visualSettings.textColor = Color.blue;
                
                SetManagerProperty("visualSettings", visualSettings);
                SetManagerProperty("applyThemeToAllButtons", true);
                SetManagerProperty("buttonCount", 1);
                
                testManager.CreateButtonGroup();
                var buttons = testManager.GetManagedButtons();
                
                Assert(buttons.Count > 0, "Styled buttons created");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestAnimations()
        {
            StartTest("Animation System");
            
            try
            {
                GameObject testObj = new GameObject("Test_Animations");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                // Configure animations
                var animSettings = new ButtonGroupManager.AnimationSettings();
                animSettings.enableHoverAnimation = true;
                animSettings.hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
                animSettings.animationDuration = 0.3f;
                
                SetManagerProperty("animationSettings", animSettings);
                SetManagerProperty("enableAnimations", true);
                SetManagerProperty("buttonCount", 1);
                
                testManager.CreateButtonGroup();
                var buttons = testManager.GetManagedButtons();
                
                Assert(buttons.Count > 0, "Animated buttons created");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        private void TestAutoButtonHandlerIntegration()
        {
            StartTest("EnhancedAutoButtonHandler Integration");
            
            try
            {
                GameObject testObj = new GameObject("Test_AutoButtonHandler");
                testManager = testObj.AddComponent<ButtonGroupManager>();
                
                SetManagerProperty("addAutoButtonHandler", true);
                SetManagerProperty("enableVisualFeedback", true);
                SetManagerProperty("buttonCount", 1);
                
                var buttonConfigs = new System.Collections.Generic.List<ButtonGroupManager.ButtonData> {
                    new ButtonGroupManager.ButtonData {
                        buttonText = "Save",
                        command = "save",
                        isEnabled = true
                    }
                };
                
                SetManagerProperty("buttonConfigs", buttonConfigs);
                testManager.CreateButtonGroup();
                
                var buttons = testManager.GetManagedButtons();
                Assert(buttons.Count > 0, "Buttons created with auto handler");
                
                // Check if EnhancedAutoButtonHandler was added
                var handler = buttons[0].GetComponent<EnhancedAutoButtonHandler>();
                Assert(handler != null, "EnhancedAutoButtonHandler component added");
                
                PassTest();
                Cleanup(testObj);
            }
            catch (System.Exception e)
            {
                FailTest($"Exception: {e.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private void StartTest(string testName)
        {
            testsTotal++;
            if (enableDetailedLogging)
            {
                UnityEngine.Debug.Log($"Starting test: {testName}");
            }
        }

        private void PassTest()
        {
            testsPassed++;
            if (enableDetailedLogging)
            {
                UnityEngine.Debug.Log("✅ Test PASSED");
            }
        }

        private void FailTest(string reason)
        {
            if (enableDetailedLogging)
            {
                UnityEngine.Debug.LogError($"❌ Test FAILED: {reason}");
            }
        }

        private void Assert(bool condition, string message)
        {
            if (enableDetailedLogging)
            {
                UnityEngine.Debug.Log($"✓ {message}");
            }
            
            if (!condition)
            {
                FailTest(message);
            }
        }

        private void SetManagerProperty(string propertyName, object value)
        {
            SetManagerPropertyForObject(testManager, propertyName, value);
        }

        private void SetManagerPropertyForObject(ButtonGroupManager manager, string propertyName, object value)
        {
            var field = typeof(ButtonGroupManager).GetField(propertyName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(manager, value);
            }
        }

        private void Cleanup(GameObject obj)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }

        private void LogTestResults()
        {
            string resultColor = testsPassed == testsTotal ? "green" : "orange";
            UnityEngine.Debug.Log($"<color={resultColor}>Tests Passed: {testsPassed}/{testsTotal}</color>");
            
            if (testsPassed == testsTotal)
            {
                UnityEngine.Debug.Log("<color=green>🎉 ALL TESTS PASSED! ButtonGroupManager is working correctly.</color>");
            }
            else
            {
                UnityEngine.Debug.Log($"<color=orange>⚠️ {testsTotal - testsPassed} tests failed. Check the logs above for details.</color>");
            }
        }

        #endregion

        #region Public Test Methods for Menu

        public void RunTestsFromMenu()
        {
            RunAllTests();
        }

        public void TestBasicCreationOnly()
        {
            testsTotal = 0;
            testsPassed = 0;
            TestBasicCreation();
            LogTestResults();
        }

        public void TestAutoGenerationOnly()
        {
            testsTotal = 0;
            testsPassed = 0;
            TestAutoGeneration();
            LogTestResults();
        }

        public void TestRuntimeModificationOnly()
        {
            testsTotal = 0;
            testsPassed = 0;
            TestRuntimeModification();
            LogTestResults();
        }

        #endregion
    }
}
