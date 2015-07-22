using SlimDX.DirectInput;
using SlimDX.RawInput;
using System.Collections.Generic;

namespace RomanEngine.Input
{
    /// <summary>
    /// Class that holds you information about KeyStates
    /// </summary>
    public struct KeyboardState
    {
        public Dictionary<Key, KeyState> KeyStates;

        public KeyState GetKeyState(Key key)
        {
            if (KeyStates == null)
                KeyStates = new Dictionary<Key, KeyState>();
            if (KeyStates.ContainsKey(key))
                return KeyStates[key];
            else
                return KeyState.Released;
        }

        public bool IsKeyDown(Key key)
        {
            if (KeyStates == null)
                KeyStates = new Dictionary<Key, KeyState>();
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
            return !IsKeyDown(key);
        }

        public static bool operator ==(KeyboardState a, KeyboardState b)
        {
            if (a.KeyStates == null)
            {
                if (b.KeyStates == null)
                    return true;
                else 
                    return false;
            }
            foreach (Key k1 in a.KeyStates.Keys)
            {
                if (a.KeyStates[k1] != b.KeyStates[k1])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(KeyboardState a, KeyboardState b)
        {
            if (a.KeyStates == null)
            {
                if (b.KeyStates == null)
                    return false;
                else
                    return true;
            }
            foreach (Key k1 in a.KeyStates.Keys)
            {
                if (a.KeyStates[k1] != b.KeyStates[k1])
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
