using UnityEngine;
using System;

namespace AmishSimulator
{
    public enum GameState { MainMenu, Playing, Paused, GameOver, ScoreScreen }
    public enum FailReason { Starvation, SpouseDeath, ChildDeath }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        public event Action<FailReason> OnGameOver;
        public event Action OnGameWin;
        public event Action<GameState> OnStateChanged;

        // References to major systems (assigned in Inspector or via FindObjectOfType)
        public TimeSystem TimeSystem { get; private set; }
        public EnergySystem EnergySystem { get; private set; }
        public HungerSystem HungerSystem { get; private set; }
        public AgingSystem AgingSystem { get; private set; }
        public DifficultyManager DifficultyManager { get; private set; }
        public RelationshipSystem RelationshipSystem { get; private set; }
        public OrdnungSystem OrdnungSystem { get; private set; }
        public GameStats GameStats { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameStats = new GameStats();
        }

        private void Start()
        {
            FindSystems();
        }

        private void FindSystems()
        {
            TimeSystem = FindFirstObjectByType<TimeSystem>();
            EnergySystem = FindFirstObjectByType<EnergySystem>();
            HungerSystem = FindFirstObjectByType<HungerSystem>();
            AgingSystem = FindFirstObjectByType<AgingSystem>();
            DifficultyManager = FindFirstObjectByType<DifficultyManager>();
            RelationshipSystem = FindFirstObjectByType<RelationshipSystem>();
            OrdnungSystem = FindFirstObjectByType<OrdnungSystem>();
        }

        public void StartGame(Gender gender)
        {
            GameStats = new GameStats();
            SetState(GameState.Playing);
            if (AgingSystem != null) AgingSystem.Initialize(gender);
            if (TimeSystem != null) TimeSystem.BeginDayCycle();
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
                SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
                SetState(GameState.Playing);
        }

        public void TriggerFailState(FailReason reason)
        {
            if (CurrentState == GameState.GameOver) return;
            SetState(GameState.GameOver);
            OnGameOver?.Invoke(reason);
        }

        public void TriggerWinState()
        {
            SetState(GameState.ScoreScreen);
            OnGameWin?.Invoke();
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public bool IsPlaying => CurrentState == GameState.Playing;
    }
}
