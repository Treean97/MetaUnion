using UnityEngine;

namespace SimplePieMenu
{
    public class MouseAndKeyboard_NEW_INPUT_SYSTEM : MonoBehaviour, IInputDevice
    {
        private PieMenuControls pieMenuControls;

        private Vector2 cursorPosition;
        private PieMenuControls.MouseAndKeyboardActions input;

        private void Awake()
        {
            pieMenuControls = new PieMenuControls();
            pieMenuControls.MouseAndKeyboard.PointerPosition.performed += ctx => OnCursorPositionPerformed(ctx.ReadValue<Vector2>());
            input = pieMenuControls.MouseAndKeyboard;
            pieMenuControls.Enable();
        }

        private void OnDestroy()
        {
            pieMenuControls.Disable();
        }

        public Vector2 GetPosition(Vector2 anchoredPosition)
        {
            Vector2 position;

            position.x = cursorPosition.x - (Screen.width / 2f) - anchoredPosition.x;
            position.y = cursorPosition.y - (Screen.height / 2f) - anchoredPosition.y;

            return position;
        }

        public bool IsSelectionButtonPressed()
        {
            return input.Selection.WasPressedThisFrame();
        }

        public bool IsCloseButtonPressed()
        {
            return input.Close.WasPressedThisFrame();
        }

        private void OnCursorPositionPerformed(Vector2 pointerPosition)
        {
            cursorPosition = pointerPosition;
        }
    }
}