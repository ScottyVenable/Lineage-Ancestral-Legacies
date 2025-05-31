using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Simple test script to demonstrate and test button animations using Unity's new Input System
/// 
/// Controls:
/// - Test Key (default: Space): Cycle through buttons one by one
/// - Test All Key (default: T): Test all buttons simultaneously
/// - Reset All Key (default: R): Reset all buttons to original state
/// - Auto Test: Automatically cycle through button animations
/// 
/// Context Menu Options:
/// - "Test All Buttons": Right-click in Inspector to test all buttons
/// - "Reset All Buttons": Right-click in Inspector to reset all buttons
/// </summary>
public class ButtonAnimationTester : MonoBehaviour
{    [Header("Test Controls")]
    [SerializeField] private ButtonUIHandler[] buttonsToTest;
    [SerializeField] private Key testKey = Key.Space;
    [SerializeField] private Key testAllKey = Key.T;
    [SerializeField] private Key resetAllKey = Key.R;
    [SerializeField] private bool autoTest = false;
    [SerializeField] private float autoTestInterval = 2f;

    private float lastAutoTestTime = 0f;
    private int currentButtonIndex = 0;
    private InputAction testAction;
    private InputAction testAllAction;
    private InputAction resetAllAction;void Start()
    {
        // Find all ButtonUIHandlers if none are assigned
        if (buttonsToTest == null || buttonsToTest.Length == 0)
        {
            buttonsToTest = FindObjectsByType<ButtonUIHandler>(FindObjectsSortMode.None);
            Debug.Log($"ButtonAnimationTester: Found {buttonsToTest.Length} buttons to test");
        }

        // Set up input action for test key
        SetupInputAction();
    }

    void Update()
    {
        // Auto test
        if (autoTest && Time.time - lastAutoTestTime >= autoTestInterval)
        {
            TestNextButton();
            lastAutoTestTime = Time.time;
        }
    }    private void SetupInputAction()
    {
        // Create input action for the test key (cycle through buttons)
        testAction = new InputAction("TestButton", InputActionType.Button);
        testAction.AddBinding($"<Keyboard>/{testKey.ToString().ToLower()}");
        testAction.performed += ctx => TestNextButton();
        testAction.Enable();

        // Create input action for testing all buttons at once
        testAllAction = new InputAction("TestAllButtons", InputActionType.Button);
        testAllAction.AddBinding($"<Keyboard>/{testAllKey.ToString().ToLower()}");
        testAllAction.performed += ctx => TestAllButtons();
        testAllAction.Enable();

        // Create input action for resetting all buttons
        resetAllAction = new InputAction("ResetAllButtons", InputActionType.Button);
        resetAllAction.AddBinding($"<Keyboard>/{resetAllKey.ToString().ToLower()}");
        resetAllAction.performed += ctx => ResetAllButtons();
        resetAllAction.Enable();

        Debug.Log($"ButtonAnimationTester: Input controls setup - Test: {testKey}, Test All: {testAllKey}, Reset All: {resetAllKey}");
    }

    private void TestNextButton()
    {
        if (buttonsToTest == null || buttonsToTest.Length == 0)
        {
            Debug.LogWarning("ButtonAnimationTester: No buttons to test!");
            return;
        }

        var button = buttonsToTest[currentButtonIndex];
        if (button != null)
        {
            Debug.Log($"Testing button animation on: {button.name}");
            button.PlayPressAnimation();
        }

        currentButtonIndex = (currentButtonIndex + 1) % buttonsToTest.Length;
    }

    /// <summary>
    /// Test all buttons at once
    /// </summary>
    [ContextMenu("Test All Buttons")]
    public void TestAllButtons()
    {
        if (buttonsToTest == null) return;

        foreach (var button in buttonsToTest)
        {
            if (button != null)
            {
                button.PlayPressAnimation();
            }
        }
    }

    /// <summary>
    /// Reset all buttons to their original state
    /// </summary>
    [ContextMenu("Reset All Buttons")]
    public void ResetAllButtons()
    {
        if (buttonsToTest == null) return;

        foreach (var button in buttonsToTest)
        {
            if (button != null)
            {
                button.ResetToOriginalState();            }
        }
    }    private void OnDestroy()
    {
        // Clean up input actions
        if (testAction != null)
        {
            testAction.Disable();
            testAction.Dispose();
        }
        if (testAllAction != null)
        {
            testAllAction.Disable();
            testAllAction.Dispose();
        }
        if (resetAllAction != null)
        {
            resetAllAction.Disable();
            resetAllAction.Dispose();
        }
    }

    private void OnDisable()
    {
        // Disable input actions when component is disabled
        testAction?.Disable();
        testAllAction?.Disable();
        resetAllAction?.Disable();
    }

    private void OnEnable()
    {
        // Re-enable input actions when component is enabled
        testAction?.Enable();
        testAllAction?.Enable();
        resetAllAction?.Enable();
    }
}
