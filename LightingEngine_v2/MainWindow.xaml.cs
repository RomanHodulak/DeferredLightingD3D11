using LightingEngine_v2.LightingD3D11.Shading;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightingEngine_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Storyboard st = (this.FindResource("infoAnimation") as Storyboard);
            st.Completed += MainWindow_Completed;
            Viewport.LayoutUpdated += Viewport_LayoutUpdated;
            numLights.Text = "Count of lights: " + 1;
            ToggleButton_Click(forwardButton, new RoutedEventArgs());
            lightSlider.Value = 1;
            st.Begin();
        }

        void Viewport_LayoutUpdated(object sender, EventArgs e)
        {
            if (Viewport.FramesPerSecond < 20)
                fps.Foreground = Brushes.Red;
            else if (Viewport.FramesPerSecond < 40)
                fps.Foreground = Brushes.OrangeRed;
            else if (Viewport.FramesPerSecond < 60)
                fps.Foreground = Brushes.DarkGreen;
            else
                fps.Foreground = Brushes.DarkGreen;
            fps.Text = Viewport.FrameDelta + "ms (FPS: " + ((int)Viewport.FramesPerSecond).ToString() + ")";
        }

        void MainWindow_Completed(object sender, EventArgs e)
        {
            //info.Text = Viewport.FramesPerSecond.ToString();
            //(this.FindResource("infoAnimation") as Storyboard).Begin();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Viewport.Dispose();
            base.OnClosing(e);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 0)
            {
                lightSlider.Value = 1;
                return;
            }
            GenerateLights((int)e.NewValue);
            numLights.Text = "Count of lights: " + Viewport.Renderer.LightingSystem.Lights.Count();
            int stride = 0;
            if (Viewport.Renderer.LightingSystem is ForwardLighting)
                stride = (Viewport.Renderer.LightingSystem as ForwardLighting).pLightBuffer.StructureByteStride;
            else if (Viewport.Renderer.LightingSystem is DeferredLighting)
                stride = (Viewport.Renderer.LightingSystem as DeferredLighting).pLightBuffer.StructureByteStride;
            else if (Viewport.Renderer.LightingSystem is DeferredQuadLighting)
                stride = (Viewport.Renderer.LightingSystem as DeferredQuadLighting).pLightBuffer.StructureByteStride;
            else if (Viewport.Renderer.LightingSystem is DeferredTileBasedLighting)
                stride = (Viewport.Renderer.LightingSystem as DeferredTileBasedLighting).pLightBuffer.StructureByteStride;
            mem.Text = (stride * Viewport.Renderer.LightingSystem.Lights.Count()).ToString() + " B";
        }

        void GenerateLights(int count)
        {
            int index = Viewport.Renderer.LightingSystem.Lights.Count();
            Random rnd = new Random();
            if (index < count)
            {
                for (int i = Viewport.Renderer.LightingSystem.Lights.Count(); i < count; i++)
                {
                    Viewport.Renderer.LightingSystem.Lights.Add(new PointLight()
                    {
                        Position = new Vector3(((float)rnd.NextDouble() * 20) - 10, ((float)rnd.NextDouble() * 20) - 10, ((float)rnd.NextDouble() * 20) - 10),
                        Color = new Vector3(((float)rnd.NextDouble() * 1), ((float)rnd.NextDouble() * 0.5f), ((float)rnd.NextDouble() * 0)) * 2,
                        Radius = ((float)rnd.NextDouble() * 10) + 6,
                    });
                }
            }
            else
            {
                Viewport.Renderer.LightingSystem.Lights.RemoveRange(count, index - count);
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainGrid.ColumnDefinitions[1].MaxWidth = this.RenderSize.Width - SystemParameters.ResizeFrameVerticalBorderWidth * 4;
            mainGrid.ColumnDefinitions[1].Width = new GridLength(mainGrid.ColumnDefinitions[1].Width.Value);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            forwardButton.IsChecked = sender == forwardButton;
            deferredButton.IsChecked = sender == deferredButton;
            deferredQuadButton.IsChecked = sender == deferredQuadButton;
            deferredTiledButton.IsChecked = sender == deferredTiledButton;
            LightingSystem newLS = null;
            string name = "";
            if (forwardButton.IsChecked.Value)
            {
                newLS = new ForwardLighting(Viewport.Renderer);
                name = "Forward Rendering";
            }
            else if (deferredButton.IsChecked.Value)
            {
                newLS = new DeferredLighting(Viewport.Renderer);
                name = "Deferred Rendering";
            }
            else if (deferredQuadButton.IsChecked.Value)
            {
                newLS = new DeferredQuadLighting(Viewport.Renderer);
                name = "Deferred Quad-Cull Rendering";
            }
            else if (deferredTiledButton.IsChecked.Value)
            {
                newLS = new DeferredTileBasedLighting(Viewport.Renderer);
                name = "Deferred Tile-Based Rendering";
            }
            SetLightingSystem(name, newLS);
        }

        void SetLightingSystem(string name, LightingSystem newLS)
        {
            if (info.Text != name)
            {
                List<Light> ls = Viewport.Renderer.LightingSystem.Lights;
                Storyboard st = (this.FindResource("infoAnimation") as Storyboard);
                st.Completed += MainWindow_Completed;
                Viewport.Renderer.LightingSystem.Dispose();
                Viewport.Renderer.LightingSystem = newLS;
                Viewport.Renderer.LightingSystem.Initialize();
                //GenerateLights((int)lightSlider.Value);
                Viewport.Renderer.LightingSystem.Lights = ls;
                info.Text = name;
                st.Begin();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Viewport != null)
            {
                int samples = int.Parse(((e.Source as ComboBox).SelectedItem as ComboBoxItem).Tag.ToString());
                Viewport.Renderer.BufferSampleDescription = new SlimDX.DXGI.SampleDescription(samples, 0);
                Viewport.RefreshImage();
            }
        }
    }
}
