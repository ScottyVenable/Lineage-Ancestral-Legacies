using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Manages time progression, day/night cycles, seasons, and game speed.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] private float realTimeToGameDayRatio = 60f; // 1 real minute = 1 game day
        [SerializeField] private bool isPaused = false;
        [SerializeField] private float gameSpeed = 1f;
        [SerializeField] private float maxGameSpeed = 4f;

        [Header("Day/Night Cycle")]
        [SerializeField] private bool enableDayNightCycle = true;
        [SerializeField] private Light sunLight;
        [SerializeField] private Gradient dayNightColors;
        [SerializeField] private AnimationCurve lightIntensityCurve;
        [SerializeField] private float minLightIntensity = 0.1f;
        [SerializeField] private float maxLightIntensity = 1f;

        [Header("Seasons")]
        [SerializeField] private bool enableSeasons = true;
        [SerializeField] private int daysPerSeason = 30;
        [SerializeField] private Color[] seasonColors = new Color[4];

        // Time tracking
        private float currentGameTime = 0f; // In game hours (0-24)
        private int currentDay = 1;
        private int currentSeason = 0; // 0=Spring, 1=Summer, 2=Autumn, 3=Winter
        private int currentYear = 1;

        // Events
        public System.Action<int> OnDayChanged;
        public System.Action<int> OnSeasonChanged;
        public System.Action<int> OnYearChanged;
        public System.Action<float> OnTimeOfDayChanged;
        public System.Action<bool> OnGamePausedChanged;
        public System.Action<float> OnGameSpeedChanged;

        // Properties
        public float CurrentGameTime => currentGameTime;
        public int CurrentDay => currentDay;
        public int CurrentSeason => currentSeason;
        public int CurrentYear => currentYear;
        public bool IsPaused => isPaused;
        public float GameSpeed => gameSpeed;
        public string SeasonName => GetSeasonName(currentSeason);
        public bool IsNight => currentGameTime < 6f || currentGameTime > 18f;
        public bool IsDay => !IsNight;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTime();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (!isPaused)
            {
                UpdateGameTime();
                UpdateDayNightCycle();
            }

            HandleTimeInput();
        }

        private void InitializeTime()
        {
            if (sunLight == null)
                sunLight = FindFirstObjectByType<Light>();

            // Initialize day/night colors if not set
            if (dayNightColors.colorKeys.Length == 0)
            {
                GradientColorKey[] colorKeys = new GradientColorKey[4];
                colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0f);    // Night
                colorKeys[1] = new GradientColorKey(new Color(1f, 0.8f, 0.6f), 0.25f);   // Dawn
                colorKeys[2] = new GradientColorKey(new Color(1f, 1f, 1f), 0.5f);        // Day
                colorKeys[3] = new GradientColorKey(new Color(1f, 0.6f, 0.3f), 0.75f);   // Dusk

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                dayNightColors.SetKeys(colorKeys, alphaKeys);
            }

            // Initialize light intensity curve if not set
            if (lightIntensityCurve.keys.Length == 0)
            {
                lightIntensityCurve = new AnimationCurve();
                lightIntensityCurve.AddKey(0f, 0.1f);    // Night
                lightIntensityCurve.AddKey(0.25f, 0.7f); // Dawn
                lightIntensityCurve.AddKey(0.5f, 1f);    // Day
                lightIntensityCurve.AddKey(0.75f, 0.7f); // Dusk
                lightIntensityCurve.AddKey(1f, 0.1f);    // Night
            }

            // Initialize season colors if not set
            if (seasonColors.Length != 4 || seasonColors[0] == Color.clear)
            {
                seasonColors = new Color[4];
                seasonColors[0] = new Color(0.8f, 1f, 0.8f); // Spring - light green
                seasonColors[1] = new Color(1f, 1f, 0.8f);   // Summer - warm yellow
                seasonColors[2] = new Color(1f, 0.8f, 0.6f); // Autumn - orange
                seasonColors[3] = new Color(0.8f, 0.9f, 1f); // Winter - cool blue
            }
        }

        private void UpdateGameTime()
        {
            float deltaTime = Time.deltaTime * gameSpeed;
            float gameHoursPerSecond = 24f / realTimeToGameDayRatio;
            
            currentGameTime += deltaTime * gameHoursPerSecond;

            // Check for day change
            if (currentGameTime >= 24f)
            {
                currentGameTime -= 24f;
                AdvanceDay();
            }

            OnTimeOfDayChanged?.Invoke(currentGameTime);
        }

        private void AdvanceDay()
        {
            currentDay++;
            OnDayChanged?.Invoke(currentDay);

            // Check for season change
            if (enableSeasons && (currentDay - 1) % daysPerSeason == 0 && currentDay > 1)
            {
                AdvanceSeason();
            }

            UnityEngine.Debug.Log($"Day {currentDay} begins. Time: {GetFormattedTime()}");
        }

        private void AdvanceSeason()
        {
            currentSeason = (currentSeason + 1) % 4;
            OnSeasonChanged?.Invoke(currentSeason);

            // Check for year change
            if (currentSeason == 0) // Spring = new year
            {
                currentYear++;
                OnYearChanged?.Invoke(currentYear);
                UnityEngine.Debug.Log($"Year {currentYear} begins!");
            }

            UnityEngine.Debug.Log($"Season changed to {GetSeasonName(currentSeason)}");
        }

        private void UpdateDayNightCycle()
        {
            if (!enableDayNightCycle || sunLight == null)
                return;

            float timeOfDayNormalized = currentGameTime / 24f;

            // Update light color
            Color currentColor = dayNightColors.Evaluate(timeOfDayNormalized);
            if (enableSeasons)
            {
                // Blend with season color
                currentColor = Color.Lerp(currentColor, seasonColors[currentSeason], 0.3f);
            }
            sunLight.color = currentColor;

            // Update light intensity
            float intensity = lightIntensityCurve.Evaluate(timeOfDayNormalized);
            sunLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, intensity);

            // Update light rotation (sun position)
            float sunAngle = (timeOfDayNormalized - 0.25f) * 360f; // Start at dawn
            sunLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, 30f, 0f);
        }

        private void HandleTimeInput()
        {
            // Space to pause/unpause
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TogglePause();
            }

            // Number keys to change speed
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetGameSpeed(1f);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SetGameSpeed(2f);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SetGameSpeed(3f);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                SetGameSpeed(4f);

            // Plus/Minus to adjust speed
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
                IncreaseSpeed();
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                DecreaseSpeed();
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            OnGamePausedChanged?.Invoke(isPaused);
            UnityEngine.Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
            OnGamePausedChanged?.Invoke(isPaused);
        }

        public void SetGameSpeed(float speed)
        {
            gameSpeed = Mathf.Clamp(speed, 0.1f, maxGameSpeed);
            OnGameSpeedChanged?.Invoke(gameSpeed);
            UnityEngine.Debug.Log($"Game speed set to {gameSpeed}x");
        }

        public void IncreaseSpeed()
        {
            SetGameSpeed(gameSpeed + 0.5f);
        }

        public void DecreaseSpeed()
        {
            SetGameSpeed(gameSpeed - 0.5f);
        }

        public string GetFormattedTime()
        {
            int hours = Mathf.FloorToInt(currentGameTime);
            int minutes = Mathf.FloorToInt((currentGameTime - hours) * 60);
            return $"{hours:00}:{minutes:00}";
        }

        public string GetDateString()
        {
            return $"Year {currentYear}, Day {currentDay} ({GetSeasonName(currentSeason)})";
        }

        public string GetSeasonName(int season)
        {
            switch (season)
            {
                case 0: return "Spring";
                case 1: return "Summer";
                case 2: return "Autumn";
                case 3: return "Winter";
                default: return "Unknown";
            }
        }

        public void SkipToNextDay()
        {
            currentGameTime = 0f;
            AdvanceDay();
        }

        public void SkipToTime(float targetTime)
        {
            if (targetTime >= 0f && targetTime < 24f)
            {
                currentGameTime = targetTime;
                OnTimeOfDayChanged?.Invoke(currentGameTime);
            }
        }

        public void SetDate(int year, int day, int season)
        {
            currentYear = Mathf.Max(1, year);
            currentDay = Mathf.Max(1, day);
            currentSeason = Mathf.Clamp(season, 0, 3);
            
            OnYearChanged?.Invoke(currentYear);
            OnDayChanged?.Invoke(currentDay);
            OnSeasonChanged?.Invoke(currentSeason);
        }

        // Get time-based modifiers for gameplay
        public float GetSeasonalGrowthModifier()
        {
            switch (currentSeason)
            {
                case 0: return 1.2f; // Spring - faster growth
                case 1: return 1.0f; // Summer - normal
                case 2: return 0.8f; // Autumn - slower growth
                case 3: return 0.3f; // Winter - much slower
                default: return 1.0f;
            }
        }

        public float GetTimeOfDayProductivityModifier()
        {
            // People work better during the day
            if (IsDay)
                return 1.0f;
            else
                return 0.5f; // 50% productivity at night
        }

        public bool ShouldPopsSleep()
        {
            return currentGameTime >= 22f || currentGameTime <= 6f;
        }
    }
}
