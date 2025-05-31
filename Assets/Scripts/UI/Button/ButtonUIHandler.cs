// ButtonUIHandler.cs
// Enhanced button handler with smooth sprite and scale animations
// Features:
// - Smooth scale animation on press
// - Sprite transition animation (original -> pressed -> original)
// - Customizable animation curves and timing
// - Public methods for manual animation triggering
// - Proper cleanup and state management
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ButtonUIHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ButtonData buttonData; // The ScriptableObject to assign
    [SerializeField] private TextMeshProUGUI captionText; // The TextMeshPro child component

    [Header("Visuals")]
    [Tooltip("The image to use for the button background. If not set, a default will be used.")]
    [SerializeField] private Image buttonImage; // The Image component for the button background
      [Header("Settings")]
    [Tooltip("If true, the caption text will be set to the GameObject's name.")]
    [SerializeField] private bool useGameObjectAsCaption = true;
    [SerializeField] private string buttonName; // Optional name for the button, can be used for debugging or identification

    [Header("Animation Settings")]
    [Tooltip("Duration of the press animation in seconds")]
    [SerializeField] private float animationDuration = 0.1f;
    [Tooltip("Animation curve for the press effect")]
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Scale factor when button is pressed")]
    [SerializeField] private float pressedScale = 0.95f;
    [Tooltip("Whether to use smooth sprite transition")]
    [SerializeField] private bool useSmoothSpriteTransition = true;

    // Private variables for sprite management
    private Sprite originalSprite;
    private bool isPressed = false;
    private Coroutine pressCoroutine;
    private Vector3 originalScale;
    private RectTransform rectTransform;    void Awake()
    {
        // Get RectTransform and store original scale
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
        }

        // Basic error checking
        if (captionText == null)
        {
            // Try to find it automatically if not assigned
            captionText = GetComponentInChildren<TextMeshProUGUI>();
            if (captionText == null)
            {
                Debug.LogWarning($"ButtonUIHandler on '{gameObject.name}': No TextMeshProUGUI component found as a child. Please assign it in the Inspector or ensure it exists.", this);
                return;
            }
        }

        // Get or assign button image
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        // Set the button's background image if a sprite is provided
        if (buttonImage != null)
        {
            // Store the original sprite for later use
            originalSprite = buttonImage.sprite ?? buttonData?.defaultSprite;
            if (originalSprite != null)
            {
                buttonImage.sprite = originalSprite;
            }
            else
            {
                Debug.LogWarning($"ButtonUIHandler on '{gameObject.name}': No sprite assigned. Please assign a sprite in the ButtonData or set a default sprite.", this);
            }
            
            buttonImage.type = Image.Type.Sliced; // Better for UI scaling than Tiled
        }

        // Set caption text
        if (useGameObjectAsCaption)
        {
            // Set the TextMeshPro text to the name of this GameObject (removing first 4 characters)
            string caption = gameObject.name.Length > 4 ? gameObject.name.Substring(4) : gameObject.name;
            captionText.text = caption;
        }
        else
        {
            captionText.text = buttonName;
        }

        // Hook up the button's onClick event
        Button unityUIButton = GetComponent<Button>();
        if (unityUIButton != null)
        {
            unityUIButton.onClick.AddListener(OnButtonClicked);
        }
    }    private void OnButtonClicked()
    {
        // Prevent multiple press animations from overlapping
        if (isPressed) return;

        // Start the press animation
        if (buttonData != null && buttonImage != null)
        {
            if (pressCoroutine != null)
            {
                StopCoroutine(pressCoroutine);
            }
            pressCoroutine = StartCoroutine(HandleButtonPress());
        }

        // Execute button logic
        Debug.Log($"Button '{gameObject.name}' clicked! Animation duration: {animationDuration}s, Scale: {pressedScale}");
        
        // Invoke button data events if available
        buttonData?.onClickEvent?.Invoke();
    }private IEnumerator HandleButtonPress()
    {
        isPressed = true;
        
        // Store the starting values
        Vector3 startScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        Vector3 targetScale = startScale * pressedScale;
        Sprite startSprite = buttonImage.sprite;
        Sprite targetSprite = buttonData.pressedSprite ?? startSprite;
        
        // Animate to pressed state
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            float normalizedTime = elapsedTime / animationDuration;
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            // Scale animation
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            }
            
            // Sprite transition (if enabled and sprites are different)
            if (useSmoothSpriteTransition && targetSprite != startSprite && buttonImage != null)
            {
                // For smooth sprite transition, we can use alpha blending or just switch at halfway point
                if (normalizedTime >= 0.5f && buttonImage.sprite != targetSprite)
                {
                    buttonImage.sprite = targetSprite;
                }
            }
            else if (buttonImage != null && normalizedTime >= 0.5f && buttonImage.sprite != targetSprite)
            {
                // Immediate sprite change at halfway point
                buttonImage.sprite = targetSprite;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we're at the target state
        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
        }
        if (buttonImage != null && targetSprite != null)
        {
            buttonImage.sprite = targetSprite;
        }
        
        // Wait for the button data's press duration (if specified)
        if (buttonData != null && buttonData.pressDuration > 0)
        {
            yield return new WaitForSeconds(buttonData.pressDuration);
        }
        else
        {
            // Default hold time
            yield return new WaitForSeconds(0.05f);
        }
        
        // Animate back to original state
        elapsedTime = 0f;
        Vector3 currentScale = rectTransform != null ? rectTransform.localScale : targetScale;
        Sprite currentSprite = buttonImage != null ? buttonImage.sprite : targetSprite;
        
        while (elapsedTime < animationDuration)
        {
            float normalizedTime = elapsedTime / animationDuration;
            float curveValue = animationCurve.Evaluate(1f - normalizedTime); // Reverse the curve
            
            // Scale animation back to original
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(originalScale, currentScale, curveValue);
            }
            
            // Sprite transition back to original
            if (normalizedTime >= 0.5f && buttonImage != null && buttonImage.sprite != originalSprite)
            {
                buttonImage.sprite = originalSprite;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we're back to the original state
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
        if (buttonImage != null)
        {
            buttonImage.sprite = originalSprite;
        }
          isPressed = false;
        pressCoroutine = null;
    }

    /// <summary>
    /// Manually trigger the button press animation without invoking the click event
    /// </summary>
    public void PlayPressAnimation()
    {
        if (isPressed) return;

        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
        }
        pressCoroutine = StartCoroutine(HandleButtonPress());
    }

    /// <summary>
    /// Reset the button to its original state immediately
    /// </summary>
    public void ResetToOriginalState()
    {
        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }

        isPressed = false;

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }

        if (buttonImage != null && originalSprite != null)
        {
            buttonImage.sprite = originalSprite;
        }
    }

    /// <summary>
    /// Update the button data at runtime
    /// </summary>
    /// <param name="newButtonData">The new button data to use</param>
    public void UpdateButtonData(ButtonData newButtonData)
    {
        buttonData = newButtonData;
        
        // Update original sprite if needed
        if (buttonImage != null && buttonData != null && buttonData.defaultSprite != null)
        {
            originalSprite = buttonData.defaultSprite;
            if (!isPressed)
            {
                buttonImage.sprite = originalSprite;
            }
        }
    }    private void OnDestroy()
    {
        // Clean up coroutine if the object is destroyed
        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
        }

        // Reset to original state before destruction
        ResetToOriginalState();
    }

    private void OnValidate()
    {
        // Ensure animation duration is positive
        if (animationDuration <= 0f)
        {
            animationDuration = 0.1f;
        }

        // Ensure pressed scale is reasonable
        if (pressedScale <= 0f || pressedScale > 1f)
        {
            pressedScale = 0.95f;
        }

        // Initialize animation curve if it's empty
        if (animationCurve == null || animationCurve.keys.Length == 0)
        {
            animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }
}