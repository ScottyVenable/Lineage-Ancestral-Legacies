using UnityEngine;
using Lineage.Ancestral.Legacies.Managers;

namespace Lineage.Ancestral.Legacies
{
    /// <summary>
    /// Initializes core game systems and creates manager instances.
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("Manager Prefabs")]
        public GameObject resourceManagerPrefab;
        public GameObject populationManagerPrefab;
        public GameObject selectionManagerPrefab;
        public GameObject audioManagerPrefab;
        public GameObject saveManagerPrefab;
        public GameObject cameraManagerPrefab;
        public GameObject timeManagerPrefab;

        [Header("UI")]
        public GameObject gameUIPrefab;

        private void Awake()
        {
            // Create singleton managers if they don't exist
            if (ResourceManager.Instance == null && resourceManagerPrefab != null)
            {
                Instantiate(resourceManagerPrefab);
            }

            if (PopulationManager.Instance == null && populationManagerPrefab != null)
            {
                Instantiate(populationManagerPrefab);
            }

            if (SelectionManager.Instance == null && selectionManagerPrefab != null)
            {
                Instantiate(selectionManagerPrefab);
            }            // Create additional managers using FindFirstObjectByType to avoid singleton conflicts
            if (audioManagerPrefab != null && FindFirstObjectByType<Managers.AudioManager>() == null)
            {
                Instantiate(audioManagerPrefab);
            }

            if (saveManagerPrefab != null && FindFirstObjectByType<Managers.SaveManager>() == null)
            {
                Instantiate(saveManagerPrefab);
            }

            if (cameraManagerPrefab != null && FindFirstObjectByType<Managers.CameraManager>() == null)
            {
                Instantiate(cameraManagerPrefab);
            }

            if (timeManagerPrefab != null && FindFirstObjectByType<Managers.TimeManager>() == null)
            {
                Instantiate(timeManagerPrefab);
            }

            // Create UI
            if (gameUIPrefab != null && FindFirstObjectByType<UI.GameUI>() == null)
            {
                Instantiate(gameUIPrefab);
            }
        }

        private void Start()
        {
            UnityEngine.Debug.Log("Game Initialized! Core systems ready.");
            UnityEngine.Debug.Log("- Click on pops to select them");
            UnityEngine.Debug.Log("- Hold Ctrl and click to select multiple pops");
            UnityEngine.Debug.Log("- Right-click to move selected pops");
            UnityEngine.Debug.Log("- Use miracle buttons to spend faith");
            UnityEngine.Debug.Log("- Manage your population and resources!");
            UnityEngine.Debug.Log("- Press SPACE to pause/unpause time");
            UnityEngine.Debug.Log("- Use 1-4 keys to change game speed");
            UnityEngine.Debug.Log("- Press F2 to toggle debug console");
        }
    }
}
