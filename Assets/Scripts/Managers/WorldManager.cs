using Mono.Cecil.Cil;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public string worldName = "Default World";
    public string customWorldSeed = "Default Seed";
    public float worldSeed = 0f;
    public int worldSizeMax = 1000; // Size in units
    public int chunkSize = 16; // Size of each chunk in units
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateSeed()
    {
        if (!string.IsNullOrEmpty(customWorldSeed))
        {
            worldSeed = customWorldSeed.GetHashCode();
            Debug.Log($"Generated world seed from custom seed: {customWorldSeed} -> {worldSeed}");
        }
        else
        {
            // If a world seed already exists, log a warning
            // This prevents overwriting an existing seed
            // You might want to handle this differently based on your game logic

            Debug.LogWarning("World seed already exists.");

            // Use the custom seed if provided
            worldSeed = customWorldSeed.GetHashCode();
            Debug.Log($"Using custom world seed: {worldSeed}");
        }

    }
}
