using LightingEngine_v2.LightingD3D11;
using LightingEngine_v2.LightingD3D11.Shading;
using RomanEngine.Input;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightingEngine_v2
{
    public class ViewportFrame : Image, IDisposable
    {
        D3DImageSource ImageContainer { get { return this.Source as D3DImageSource; } set { this.Source = value; } }
        public Renderer Renderer { get; set; }
        Stopwatch Timer;

        float c;
        bool up = true;
        TestTriangle tri;
        GeometricPrimitive prim;
        GeometricPrimitive prim2;
        GeometricPrimitive prim3;

        MouseState prevMouse;
        bool IsCursorVisible = true;
        Point cursorLock;
        Vector2 mouseVelocity;
        private long CurMilliseconds;
        private long PrevMilliseconds;
        private int Frames;
        private float ElapsedTime;
        float mouseWheelDelta = 0;
        int prevTimestamp = 0;

        static ViewportFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewportFrame), new FrameworkPropertyMetadata(typeof(ViewportFrame)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.SizeChanged += Viewport_SizeChanged;

            ImageContainer = new D3DImageSource();
            ImageContainer.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;

            Renderer = new Renderer((int)this.ActualWidth, (int)this.ActualHeight);
            tri = new TestTriangle(Renderer.Device);
            prim2 = new PlanePrimitive(Renderer, 2000, 2000, 10, 10);
            prim2.MatrixWorld = SlimDX.Matrix.Translation(new Vector3(0, -1, 1));
            prim3 = new SpherePrimitive(Renderer, 0.1f, 16);
            prim = new SpherePrimitive(Renderer, 1f, 32);
            Renderer.Camera = new Camera(CameraProjectionType.Perspective, CameraViewType.Orbital);
            Renderer.Camera.Offset = 15;
            Renderer.Camera.zNear = 0.001f;
            Renderer.Camera.zFar = 1000f;
            Renderer.Camera.Rotation = new Vector3(MathHelper.PiOver2*1.5f, MathHelper.PiOver2*0.75f, 0);
            Renderer.LightingSystem.Lights.Add(new PointLight()
                {
                    Position = new Vector3(1,3,1),
                    Color = new Vector3(1,1,1),
                    Radius = 15,
                });
            prim3.MatrixWorld = SlimDX.Matrix.Translation(Renderer.LightingSystem.Lights[0].Position);
            MaterialShader ms;
            using (Texture2D tex = ContentHelper.TextureFromColor(Renderer.Device, new Color4(1, 1, 1, 0)))
                ms = MaterialShader.BasicMaterial(Renderer, tex);
            prim3.MaterialShader = ms;
            ImageContainer.SetBackBufferSlimDX(Renderer.SharedTexture);
            Timer = new Stopwatch();
            MouseWheel += ViewportFrame_MouseWheel;
            BeginRenderingScene();
        }

        void ViewportFrame_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mouseWheelDelta += e.Delta * Math.Abs((1 / (e.Timestamp - prevTimestamp + .001f)) * 0.005f + 1);
            prevTimestamp = e.Timestamp;
        }

        void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Renderer.Resize((int)this.ActualWidth, (int)this.ActualHeight);
            ImageContainer.SetBackBufferSlimDX(Renderer.SharedTexture);
        }

        public void RefreshImage()
        {
            ImageContainer.SetBackBufferSlimDX(Renderer.SharedTexture);
        }

        public float FramesPerSecond = 0;
        public float FrameDelta = 0;
        void OnRendering(object sender, EventArgs e)
        {
            if (c > 1)
                up = false;
            if (c <= 0)
                up = true;
            if (up)
                c += ((float)(Timer.Elapsed.Milliseconds % 1000)) / 999.0f * 0.005f;
            else
                c -= ((float)(Timer.Elapsed.Milliseconds % 1000)) / 999.0f * 0.005f;
            c = 0.0f;

            CurMilliseconds = Timer.ElapsedMilliseconds;
            FrameDelta = Math.Abs(PrevMilliseconds - CurMilliseconds);
            PrevMilliseconds = CurMilliseconds;
            Frames++;
            ElapsedTime += FrameDelta;
            FramesPerSecond = Frames / (ElapsedTime * 0.001f);
            if (ElapsedTime > 1000)
            {
                ElapsedTime = 0;
                Frames = 0;
            }

            for (int i = 0; i < Renderer.LightingSystem.Lights.Count(); i++)
            {
                Vector3 rot = MathHelper.ToVector3(Vector3.Transform(new Vector3(0, 1, 0), SlimDX.Matrix.RotationYawPitchRoll((1f*i) + Timer.ElapsedMilliseconds * 0.00025f, MathHelper.PiOver2, MathHelper.PiOver2)));
                Renderer.LightingSystem.Lights[i].Position = rot * ((1f*i) + 2);
            }
            if (Renderer.LightingSystem.Lights.Count() > 0)
                prim3.MatrixWorld = SlimDX.Matrix.Translation(Renderer.LightingSystem.Lights[0].Position);
            UpdateInput();

            Renderer.Begin();
            Renderer.Clear(new Color4(1.0f, c, c * 1, c * 1));
            //Renderer.DrawPrimitive(tri);
            Renderer.DrawPrimitive(prim2);
            //Renderer.DrawPrimitive(prim3);
            Renderer.DrawPrimitive(prim);
            Renderer.End();
            Renderer.Present();
            ImageContainer.InvalidateD3DImage();
        }

        void UpdateInput()
        {
            Point mousePos = Helper.GetMousePosition();
            MouseState curMouse = new MouseState()
            {
                LeftButton = (ButtonState)Mouse.LeftButton,
                RightButton = (ButtonState)Mouse.RightButton,
                MiddleButton = (ButtonState)Mouse.MiddleButton,
                WheelDelta = (int)mouseWheelDelta,
                Button4 = (ButtonState)Mouse.XButton1,
                Button5 = (ButtonState)Mouse.XButton2,
                X = (int)mousePos.X,
                Y = (int)mousePos.Y,
            };
            Point screenCenter = new Point((this.ActualWidth / 2), (this.ActualHeight / 2));
            Renderer.Camera.Offset -= curMouse.WheelDelta * 0.001f;
            if (Renderer.Camera.Offset < 1.05f)
                Renderer.Camera.Offset = 1.05f;
            if (Renderer.Camera.Offset > 500)
                Renderer.Camera.zFar = Renderer.Camera.Offset * 10;
            else
                Renderer.Camera.zFar = 1000;
            if (curMouse.RightButton == ButtonState.Pressed)
            {
                if (IsCursorVisible)
                {
                    IsCursorVisible = false;
                    cursorLock = new Point(curMouse.X, curMouse.Y);
                    curMouse.X = (int)(this.ActualWidth / 2);
                    curMouse.Y = (int)(this.ActualHeight / 2);
                    Mouse.OverrideCursor = Cursors.None;
                    if (!IsMouseCaptured)
                        CaptureMouse();
                }
                mouseVelocity += new Vector2(((int)screenCenter.X - curMouse.X) * -Renderer.Camera.Up.Y, ((int)screenCenter.Y - curMouse.Y)) * 0.25f;
                Helper.SetMousePosition(screenCenter);
            }
            else
            {
                if (!IsCursorVisible)
                {
                    IsCursorVisible = true;
                    Mouse.OverrideCursor = null;
                    if (IsMouseCaptured)
                        ReleaseMouseCapture();
                    Helper.SetMousePosition(cursorLock);
                }
            }
            mouseVelocity *= 0.85f;
            float sensitivity = 0.001f;
            Renderer.Camera.Rotation += new Vector3(mouseVelocity * sensitivity, 0);
            float m = MathHelper.PiOver2 * 0.99f + (float)Math.Asin(1 / Renderer.Camera.Offset);
            if (Renderer.Camera.Rotation.Y > m)
                Renderer.Camera.Rotation = new Vector3(Renderer.Camera.Rotation.X, m, 0);
            if (Renderer.Camera.Rotation.Y < -m)
                Renderer.Camera.Rotation = new Vector3(Renderer.Camera.Rotation.X, -m, 0);
            mouseWheelDelta *= 0.9f;
            prevMouse = curMouse;
        }

        void BeginRenderingScene()
        {
            if (ImageContainer.IsFrontBufferAvailable)
            {
                foreach (var item in SlimDX.ObjectTable.Objects)
                {
                }

                ImageContainer.SetBackBufferSlimDX(Renderer.SharedTexture);
                CompositionTarget.Rendering += OnRendering;

                Timer.Start();
            }
        }

        void StopRenderingScene()
        {
            Timer.Stop();
            CompositionTarget.Rendering -= OnRendering;
        }

        void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // This fires when the screensaver kicks in, the machine goes into sleep or hibernate
            // and any other catastrophic losses of the d3d device from WPF's point of view
            if (ImageContainer.IsFrontBufferAvailable)
            {
                BeginRenderingScene();
            }
            else
            {
                StopRenderingScene();
            }
        }

        public void Dispose()
        {
            Helper.Dispose(tri, prim, prim2, prim3, ImageContainer, Renderer);
        }
    }
}
