using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PisoEsLava
{
    // HUD (GDD 5.1.11): nombre arriba a la derecha,
    // barra de lava en el borde derecho, mensajes y paneles de pausa y fin de nivel.
    // La fuente ARCADECLASSIC solo tiene letras, numeros y "!", por eso los textos no llevan tildes.
    public class HUD : MonoBehaviour
    {
        public GameManager game;

        [Header("Textos")]
        [Tooltip("Texto bajo la cuenta atras del principio")]
        public string introText = "LA LAVA VA A SUBIR";
        public Text centerText;
        public Text subText;

        [Header("Barra de lava")]
        [Tooltip("Imagen con Image Type = Filled: se llena cuando la lava se acerca")]
        public Image lavaBar;
        [Tooltip("A esta distancia (o mas) la barra esta vacia")]
        public float lavaBarDistance = 15f;

        [Header("Panel de pausa y fin de nivel")]
        public GameObject panel;
        public Text panelTitle;
        public Text panelBody;
        public Text panelHint;
        [Tooltip("Botones del panel: cada estado muestra solo los suyos")]
        public Button continueButton;
        public Button retryButton;
        public Button quitButton;

        float messageTimer;

        const string ChooseHint = "ELIGE CON LAS FLECHAS Y ENTER O CON EL RATON";

        void OnEnable()
        {
            if (game == null)
            {
                return;
            }
            game.StateChanged += OnStateChanged;
            game.PauseChanged += OnPause;
        }

        void OnDisable()
        {
            if (game == null)
            {
                return;
            }
            game.StateChanged -= OnStateChanged;
            game.PauseChanged -= OnPause;
        }

        void Start()
        {
            panel.SetActive(false);
        }

        void ShowMessage(string title, string sub, float seconds)
        {
            centerText.text = title;
            subText.text = sub;
            messageTimer = seconds;
        }

        // Pausa (Esc): continuar, repetir el nivel o salir del juego
        void OnPause(bool paused)
        {
            if (paused)
            {
                ShowPanel("PAUSA", "", ChooseHint + "     ESC CONTINUAR", continueButton, retryButton, quitButton);
            }
            else
            {
                panel.SetActive(false);
            }
        }

        void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    ShowMessage("YA!", "", 0.8f);
                    break;
                case GameState.Won:
                    ShowMessage("", "", 0f);
                    ShowPanel("LLEGASTE ARRIBA\n¡Te salvaste de morir quemadito!", "TIEMPO " + Mathf.CeilToInt(game.ElapsedTime) + " SEGUNDOS", ChooseHint, retryButton, quitButton);
                    break;
                case GameState.Lost:
                    ShowMessage("", "", 0f);
                    ShowPanel(game.LastLoseReason == LoseReason.Fell ? "TE CAISTE QUE PENA!" : "HAS MUERTO QUEMADITO!", "INTENTALO OTRA VEZ", ChooseHint, retryButton, quitButton);
                    break;
            }
        }

        // Muestra el panel con los botones indicados, en ese orden, y selecciona el primero (teclado y mando)
        void ShowPanel(string title, string body, string hint, params Button[] buttons)
        {
            panel.SetActive(true);
            panelTitle.text = title;
            panelBody.text = body;
            panelHint.text = hint;
            foreach (Button b in new[] { continueButton, retryButton, quitButton })
            {
                b.gameObject.SetActive(false);
            }
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].transform.SetSiblingIndex(i);
            }
            if (buttons.Length > 0 && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            }
        }

        void Update()
        {
            if (game.State == GameState.Countdown && !game.IsPaused)
            {
                centerText.text = Mathf.CeilToInt(game.CountdownRemaining).ToString();
                subText.text = introText;
                messageTimer = 0.01f;
            }
            else if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f)
                {
                    centerText.text = "";
                    subText.text = "";
                }
            }

            // Vacia si la lava esta lejos, llena si esta encima
            lavaBar.fillAmount = 1f - Mathf.Clamp01(game.LavaGap / lavaBarDistance);
        }
    }
}
