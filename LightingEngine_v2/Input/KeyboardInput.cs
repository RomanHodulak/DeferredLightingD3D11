using SlimDX.DirectInput;
using SlimDX.RawInput;
using System;
using System.Collections.Generic;

namespace RomanEngine.Input
{
    /// <summary>
    /// Class that gets you information about KeyStates
    /// </summary>
    public class KeyboardInput
    {
        public Dictionary<Key, KeyState> KeyStates = new Dictionary<Key, KeyState>();

        public KeyboardInput()
        {
            SlimDX.RawInput.Device.RegisterDevice(SlimDX.Multimedia.UsagePage.Generic, SlimDX.Multimedia.UsageId.Keyboard, SlimDX.RawInput.DeviceFlags.None);
            SlimDX.RawInput.Device.KeyboardInput += new EventHandler<KeyboardInputEventArgs>(this.UpdateInput);
        }

        /// <summary>
        /// This method needs to be overriden by SlimDX.RawInput.Device.
        /// </summary>
        public void UpdateInput(object sender, KeyboardInputEventArgs e)
        {
            if (!KeyStates.ContainsKey((Key)e.MakeCode))
            {
                KeyStates.Add((Key)e.MakeCode, e.State);
            }
            else
            {
                KeyStates[(Key)e.MakeCode] = e.State;
            }
        }

        /// <summary>
        /// Copies all key states to a new KeyboardState
        /// </summary>
        /// <returns>RomanEngine.Input.KeyboardState</returns>
        public KeyboardState GetState()
        {
            KeyboardState state = new KeyboardState()
            {
                KeyStates = new Dictionary<Key, KeyState>(KeyStates)
            };
            return state;
        }

        public KeyState GetKeyState(Key key)
        {
            if (KeyStates.ContainsKey(key))
                return KeyStates[key];
            else
                return KeyState.Released;
        }

        public bool IsKeyDown(Key key)
        {
            if (KeyStates.ContainsKey(key))
            {
                if (KeyStates[key] == KeyState.Pressed || KeyStates[key] == KeyState.SystemKeyPressed)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public bool IsKeyUp(Key key)
        {
            if (KeyStates.ContainsKey(key))
            {
                if (KeyStates[key] == KeyState.Released || KeyStates[key] == KeyState.SystemKeyReleased)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
    }
}
