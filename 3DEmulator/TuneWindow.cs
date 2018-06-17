using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Controls;

namespace _3DEmulator
{
    public class TuneWindow: Window
    {
        PerspectiveCamera Camera;
        DirectionalLight Light;
        static double initialY = 0;
        void InitializeViews()
        {
            this.Height = this.Width = 300;
            this.Left = 1000;
            this.Top = initialY; initialY += this.Height;
            this.Content = new TuneCameraView(Camera);
        }
        public TuneWindow(PerspectiveCamera camera,DirectionalLight light)
        {
            Camera = camera;
            Light = light;
            InitializeViews();
        }
    }
}
