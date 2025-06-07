using UnityEngine;
using Lineage.Managers;
using Lineage.Debug;

namespace Lineage
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

            if (cameraManagerPrefab != null && FindFirstObjectByType<CameraManager>() == null)
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
        }        private void Start()
        {
            Log.Info("Game Initialized! Core systems ready.", Log.LogCategory.General);
            Log.Info("- Click on pops to select them", Log.LogCategory.General);
            Log.Info("- Hold Ctrl and click to select multiple pops", Log.LogCategory.General);
            Log.Info("- Right-click to move selected pops", Log.LogCategory.General);
            Log.Info("- Use miracle buttons to spend faith", Log.LogCategory.General);
            Log.Info("- Manage your population and resources!", Log.LogCategory.General);
            Log.Info("- Press SPACE to pause/unpause time", Log.LogCategory.General);
            Log.Info("- Use 1-4 keys to change game speed", Log.LogCategory.General);
            Log.Info("- Press F2 to toggle debug console", Log.LogCategory.General);
        }
    }
}
