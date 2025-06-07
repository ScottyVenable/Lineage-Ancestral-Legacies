using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lineage.Core
{
    /// <summary>
    /// Singleton responsible for loading and providing access to game data definitions.
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        private static GameDataManager _instance;
        public static GameDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameDataManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GameDataManager");
                        _instance = go.AddComponent<GameDataManager>();
                        DontDestroyOnLoad(go);
                        _instance.LoadAllData();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, GameDataSO> _dataByID = new Dictionary<string, GameDataSO>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAllData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void LoadAllData()
        {
            _dataByID.Clear();
            var allData = Resources.LoadAll<GameDataSO>("GameData");
            foreach (var data in allData)
            {
                if (data != null && !string.IsNullOrEmpty(data.uniqueID))
                {
                    if (!_dataByID.ContainsKey(data.uniqueID))
                        _dataByID.Add(data.uniqueID, data);
                    else
                        Debug.LogWarning($"Duplicate GameData uniqueID {data.uniqueID} detected.");
                }
            }
        }

        public T GetDefinition<T>(string id) where T : GameDataSO
        {
            if (_dataByID.TryGetValue(id, out var data))
            {
                return data as T;
            }
            return null;
        }

        public List<T> GetAllDefinitionsOfType<T>() where T : GameDataSO
        {
            return _dataByID.Values.OfType<T>().ToList();
        }

        public List<T> GetDefinitionsWithTag<T>(Tag_SO tag) where T : GameDataSO
        {
            return _dataByID.Values.OfType<T>().Where(d => d.tags.Contains(tag)).ToList();
        }
    }
}
