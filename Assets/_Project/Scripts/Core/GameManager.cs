using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PisoEsLava
{
    public enum GameState { Countdown, Playing, Won, Lost }

    public enum LoseReason { Lava, Fell }

    // Estados de la partida (maquina de estados) y pausa. Hay un solo intento: tocar la lava o caerse es perder.
    // La interfaz escucha los eventos (observador).
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public PlayerController player;
        public CameraOnlyUp cameraRig;
        [Tooltip("Segundos de cuenta atras antes de que suba la lava")]
        public float countdown = 3f;
        [Tooltip("Margen bajo el borde inferior de la camara a partir del cual el jugador cuenta como caido")]
        public float fallMargin = 1.5f;

        [Header("Lava (TDD 5.2.6 a)")]
        [Tooltip("La lava: su posicion \"y\" es la superficie")]
        public Transform lava;
        [Tooltip("Velocidad inicial de la lava (u/s)")]
        public float baseSpeed = 0.45f;
        [Tooltip("Cuanto acelera la lava cada segundo (u/s2)")]
        public float acceleration = 0.25f;
        [Tooltip("Si el jugador se aleja mas que esto, la lava acelera")]
        public float maxGap = 10f;
        [Tooltip("Si el jugador esta mas cerca que esto, la lava frena")]
        public float minGap = 3f;
        public float catchUpMultiplier = 1.8f;
        public float mercyMultiplier = 0.7f;

        public GameState State { get; private set; } = GameState.Countdown;
        public LoseReason LastLoseReason { get; private set; }
        public float CountdownRemaining { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsPaused { get; private set; }

        // Durante la cuenta atras del principio el jugador no se puede mover
        public bool LocksInput => isActiveAndEnabled && State == GameState.Countdown;
        // Distancia entre el jugador y la lava
        public float LavaGap => player.transform.position.y - lava.position.y;

        public event Action<GameState> StateChanged;
        public event Action<bool> PauseChanged;

        float lavaStartTime;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            Time.timeScale = 1f;
        }

        void Start()
        {
            Time.timeScale = 1f;
            CountdownRemaining = countdown;
            SetState(GameState.Countdown);
        }

        void Update()
        {
            bool canPause = State == GameState.Countdown || State == GameState.Playing;
            if (canPause && GameInput.PausePressed)
            {
                SetPaused(!IsPaused);
            }
            if (IsPaused)
            {
                if (GameInput.RetryPressed)
                {
                    Reload();
                }
                return;
            }

            switch (State)
            {
                case GameState.Countdown:
                    CountdownRemaining -= Time.deltaTime;
                    if (CountdownRemaining <= 0f)
                    {
                        CountdownRemaining = 0f;
                        lavaStartTime = Time.time;
                        SetState(GameState.Playing);
                    }
                    CheckDanger();
                    break;

                case GameState.Playing:
                    ElapsedTime += Time.deltaTime;
                    MoveLava();
                    CheckDanger();
                    break;

                case GameState.Won:
                case GameState.Lost:
                    if (GameInput.RetryPressed)
                    {
                        Reload();
                    }
                    break;
            }
        }

        // La lava sube cada vez mas rapido. Si el jugador se aleja mucho acelera, y si esta cerca le da un respiro
        void MoveLava()
        {
            float speed = baseSpeed + acceleration * (Time.time - lavaStartTime);
            if (LavaGap > maxGap)
            {
                speed *= catchUpMultiplier;
            }
            else if (LavaGap < minGap)
            {
                speed *= mercyMultiplier;
            }
            lava.position += Vector3.up * speed * Time.deltaTime;
        }

        void CheckDanger()
        {
            Vector3 p = player.transform.position;
            if (p.y < lava.position.y)
            {
                Damage(LoseReason.Lava);
                return;
            }
            if (p.y < cameraRig.BottomY - fallMargin)
            {
                Damage(LoseReason.Fell);
            }
        }

        // Un solo intento: tocar la lava o caerse es perder el nivel
        void Damage(LoseReason reason)
        {
            if (!isActiveAndEnabled || (State != GameState.Playing && State != GameState.Countdown))
            {
                return;
            }
            LastLoseReason = reason;
            player.Die();
            SetState(GameState.Lost);
        }

        public void PlayerReachedGoal()
        {
            if (!isActiveAndEnabled || (State != GameState.Playing && State != GameState.Countdown))
            {
                return;
            }
            player.Freeze();
            SetState(GameState.Won);
        }

        // ---------- Botones de los paneles ----------

        public void Continue()
        {
            SetPaused(false);
        }

        // Salir del juego (en el editor, para el Play)
        public void Quit()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            PauseChanged?.Invoke(paused);
        }

        void SetState(GameState next)
        {
            State = next;
            StateChanged?.Invoke(next);
        }

        public void Reload()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0)
            {
                SceneManager.LoadScene(scene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(scene.name);
            }
        }
    }
}
