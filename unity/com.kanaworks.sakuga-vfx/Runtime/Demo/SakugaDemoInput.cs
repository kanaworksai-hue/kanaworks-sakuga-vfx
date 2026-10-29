using UnityEngine;

namespace KanaWorks.SakugaVFX.Demo
{
    /// <summary>Keyboard for the demos: works with the Input System package, the old Input Manager, or both.</summary>
    public static class SakugaDemoInput
    {
        public enum Key { Left, Right, Up, Down, Space, One, Two, Three, R }

        public static bool Pressed(Key k)
        {
#if SAKUGA_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return false;
            switch (k)
            {
                case Key.Left: return kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
                case Key.Right: return kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
                case Key.Up: return kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
                case Key.Down: return kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
                case Key.Space: return kb.spaceKey.wasPressedThisFrame;
                case Key.One: return kb.digit1Key.wasPressedThisFrame;
                case Key.Two: return kb.digit2Key.wasPressedThisFrame;
                case Key.Three: return kb.digit3Key.wasPressedThisFrame;
                case Key.R: return kb.rKey.wasPressedThisFrame;
            }
            return false;
#else
            try
            {
                switch (k)
                {
                    case Key.Left: return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
                    case Key.Right: return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
                    case Key.Up: return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
                    case Key.Down: return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
                    case Key.Space: return Input.GetKeyDown(KeyCode.Space);
                    case Key.One: return Input.GetKeyDown(KeyCode.Alpha1);
                    case Key.Two: return Input.GetKeyDown(KeyCode.Alpha2);
                    case Key.Three: return Input.GetKeyDown(KeyCode.Alpha3);
                    case Key.R: return Input.GetKeyDown(KeyCode.R);
                }
            }
            catch (System.InvalidOperationException) { }          // the old Input Manager is switched off
            return false;
#endif
        }
    }
}
