using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace _3DEmulator
{
    class TuneCameraView:StackPanel
    {
        CheckBox CHBmode;
        PerspectiveCamera Camera;
        TunePointView PositionTuner;
        TuneVectorView LookDirectionTuner, UpDirectionTuner;
        TunePointView CenterTuner;
        TuneValueView DistanceTuner,AngleTuner,ElevationTuner;
        void SetChildren(bool isNaive)
        {
            this.Children.Clear();
            this.Children.Add(CHBmode);
            if (isNaive)
            {
                this.Children.Add(PositionTuner);
                this.Children.Add(LookDirectionTuner);
                this.Children.Add(UpDirectionTuner);
            }
            else
            {
                this.Children.Add(CenterTuner);
                this.Children.Add(DistanceTuner);
                this.Children.Add(AngleTuner);
                this.Children.Add(ElevationTuner);
            }
        }
        Point3D CenterPosition = new Point3D();
        void SetParameters()
        {
            Camera.LookDirection = new Vector3D(Math.Cos(AngleTuner.Value / 180 * Math.PI) * Math.Cos(ElevationTuner.Value / 180 * Math.PI),
                Math.Sin(AngleTuner.Value / 180 * Math.PI) * Math.Cos(ElevationTuner.Value / 180 * Math.PI),
                Math.Sin(ElevationTuner.Value / 180 * Math.PI));
            Camera.Position = CenterPosition - Camera.LookDirection * DistanceTuner.Value;
            Camera.UpDirection = new Vector3D(0, 0, 1);
        }
        void InitializeViews()
        {
            {
                CHBmode = new CheckBox { Content = "Naive Parameters", IsChecked = true };
                CHBmode.Checked += (sender, e) => { SetChildren(true); };
                CHBmode.Unchecked += (sender, e) => { SetChildren(false); };
            }
            {
                PositionTuner = new TunePointView(Camera.Position, "Camera.Position");
                PositionTuner.PointChanged += (sender, value) => { Camera.Position = value; };
            }
            {
                LookDirectionTuner = new TuneVectorView(Camera.LookDirection, "Camera.LookDirection");
                LookDirectionTuner.VectorChanged += (sender, value) => { Camera.LookDirection = value; };
            }
            {
                UpDirectionTuner = new TuneVectorView(Camera.UpDirection, "Camera.UpDirection");
                UpDirectionTuner.VectorChanged += (sender, value) => { Camera.UpDirection = value; };
            }
            {
                CenterTuner = new TunePointView(CenterPosition, "Center Position");
                CenterTuner.PointChanged += (sender, value) => { CenterPosition = value; SetParameters(); };
            }
            {
                DistanceTuner = new TuneValueView(1, 0, 5, "Distance") { IsExponential = true };
                DistanceTuner.ValueChanged += (sender, e) => { SetParameters(); };
            }
            {
                AngleTuner = new TuneValueView(0, 0, 360, "Angle");
                AngleTuner.ValueChanged += (sender, e) => { SetParameters(); };
            }
            {
                ElevationTuner = new TuneValueView(0, -90, 90, "Elevation");
                ElevationTuner.ValueChanged += (sender, e) => { SetParameters(); };
            }
            SetChildren(false);
        }
        public TuneCameraView(PerspectiveCamera camera)
        {
            Camera = camera;
            InitializeViews();
        }
    }
}
