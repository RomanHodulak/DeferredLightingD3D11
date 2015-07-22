using SlimDX.RawInput;
using System;

namespace RomanEngine.Input
{
    public class MouseInput
    {
        public MouseMode MouseMode = MouseMode.RelativeMovement;
        public ButtonState LeftButton = ButtonState.Released;
        public ButtonState RightButton = ButtonState.Released;
        public ButtonState MiddleButton = ButtonState.Released;
        public ButtonState Button4 = ButtonState.Released;
        public ButtonState Button5 = ButtonState.Released;
        public int X = 0;
        public int Y = 0;
        public int WheelDelta = 0;

        public MouseInput()
        {
            Device.RegisterDevice(SlimDX.Multimedia.UsagePage.Generic, SlimDX.Multimedia.UsageId.Mouse, SlimDX.RawInput.DeviceFlags.None);
            Device.MouseInput += new EventHandler<SlimDX.RawInput.MouseInputEventArgs>(UpdateInput);
        }

        public void UpdateInput(object sender, MouseInputEventArgs e)
        {
            if (MouseMode != e.Mode)
                e.Mode = MouseMode;
            X -= e.X;
            Y -= e.Y;
            WheelDelta += e.WheelDelta;
            if (e.ButtonFlags == MouseButtonFlags.LeftDown)
                LeftButton = ButtonState.Pressed;
            if (e.ButtonFlags == MouseButtonFlags.LeftUp)
                LeftButton = ButtonState.Released;
            if (e.ButtonFlags == MouseButtonFlags.RightDown)
                RightButton = ButtonState.Pressed;
            if (e.ButtonFlags == MouseButtonFlags.RightUp)
                RightButton = ButtonState.Released;
            if (e.ButtonFlags == MouseButtonFlags.MiddleDown)
                MiddleButton = ButtonState.Pressed;
            if (e.ButtonFlags == MouseButtonFlags.MiddleUp)
                MiddleButton = ButtonState.Released;
            if (e.ButtonFlags == MouseButtonFlags.Button4Down)
                Button4 = ButtonState.Pressed;
            if (e.ButtonFlags == MouseButtonFlags.Button4Up)
                Button4 = ButtonState.Released;
            if (e.ButtonFlags == MouseButtonFlags.Button5Down)
                Button5 = ButtonState.Pressed;
            if (e.ButtonFlags == MouseButtonFlags.Button5Up)
                Button5 = ButtonState.Released;
        }

        public MouseState GetState()
        {
            MouseState state = new MouseState()
            {
                LeftButton = this.LeftButton,
                RightButton = this.RightButton,
                Button4 = this.Button4,
                Button5 = this.Button5,
                MiddleButton = this.MiddleButton,
                MouseMode = this.MouseMode,
                WheelDelta = this.WheelDelta,
                X = this.X,
                Y = this.Y,
            };
            return state;
        }
    }
    public enum ButtonState
    {
        Released,
        Pressed,
    }
}
