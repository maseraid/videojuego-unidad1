using UnityEngine;
using UnityEngine.InputSystem;

namespace PisoEsLava
{
    // Controles 
    // Se definen por codigo para no depender de un archivo .inputactions.
    public static class GameInput
    {
        static InputAction move;
        static InputAction jump;
        static InputAction pause;
        static InputAction retry;

        public static Vector2 Move { get { Init(); return move.ReadValue<Vector2>(); } }
        public static bool JumpPressed { get { Init(); return jump.WasPressedThisFrame(); } }
        public static bool JumpHeld { get { Init(); return jump.IsPressed(); } }
        public static bool JumpReleased { get { Init(); return jump.WasReleasedThisFrame(); } }
        public static bool PausePressed { get { Init(); return pause.WasPressedThisFrame(); } }
        public static bool RetryPressed { get { Init(); return retry.WasPressedThisFrame(); } }

        static void Init()
        {
            if (move != null)
            {
                return;
            }

            move = new InputAction("Mover", InputActionType.Value, expectedControlType: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");

            jump = new InputAction("Saltar", InputActionType.Button);
            jump.AddBinding("<Keyboard>/space");
            jump.AddBinding("<Gamepad>/buttonSouth");

            pause = new InputAction("Pausa", InputActionType.Button);
            pause.AddBinding("<Keyboard>/escape");
            pause.AddBinding("<Gamepad>/start");

            retry = new InputAction("Reintentar", InputActionType.Button);
            // Solo R: Enter y el boton A del mando se usan para pulsar los botones de los paneles
            retry.AddBinding("<Keyboard>/r");

            move.Enable();
            jump.Enable();
            pause.Enable();
            retry.Enable();
        }

        // Por si el proyecto entra a Play sin recargar el dominio: se vuelven a crear las acciones
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            move?.Dispose();
            jump?.Dispose();
            pause?.Dispose();
            retry?.Dispose();
            move = jump = pause = retry = null;
        }
    }
}
