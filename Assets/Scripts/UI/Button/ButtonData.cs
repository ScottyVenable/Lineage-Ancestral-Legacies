using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ButtonData", menuName = "Scriptable Objects/ButtonData")]
public class ButtonData : ScriptableObject
{
    [Header("Basic Properties")]
    public string caption;
    public string description;
    public Sprite icon;
    public int cost;
    public UnityEvent onClickEvent; // Or a reference to a method ID for more complex actions

    [Header("Visual States")]
    public Sprite defaultSprite; // Default sprite if none is assigned
    public Sprite pressedSprite; // Sprite to show when button is pressed
    
    [Header("Press Animation")]
    [Tooltip("How long the pressed sprite is shown (in seconds)")]
    [Range(0.1f, 2f)]
    public float pressDuration = 0.2f;
}
