using SlimDX.RawInput;

namespace RomanEngine.Input
{
    public struct MouseState
    {
        public MouseMode MouseMode;
        public ButtonState LeftButton;
        public ButtonState RightButton;
        public ButtonState MiddleButton;
        public ButtonState Button4;
        public ButtonState Button5;
        public int X;
        public int Y;
        public int WheelDelta;

        public static bool operator ==(MouseState a, MouseState b)
        {
            return a.X == b.X & a.Y == b.Y;
        }

        public static bool operator !=(MouseState a, MouseState b)
        {
            return !(a == b);
        }
    }
}
