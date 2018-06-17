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
    class TunePointView: StackPanel
    {
        Label Label = new Label();
        Slider SliderX = new Slider(), SliderY = new Slider(), SliderZ = new Slider();
        void InitializeSlider(Slider slider, string name)
        {
            slider.Name = name;
            slider.Minimum = -20;
            slider.Maximum = 20;
            slider.ValueChanged += Slider_ValueChanged;
        }
        void InitializeViews()
        {
            InitializeSlider(SliderX, "X");
            InitializeSlider(SliderZ, "Z");
            InitializeSlider(SliderY, "Y");
            this.Children.Add(Label);
            this.Children.Add(SliderX);
            this.Children.Add(SliderY);
            this.Children.Add(SliderZ);
        }
        Point3D Point;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch ((sender as Slider).Name)
            {
                case "X":
                    Point.X = e.NewValue;
                    break;
                case "Y":
                    Point.Y = e.NewValue;
                    break;
                case "Z":
                    Point.Z = e.NewValue;
                    break;
                default: throw new Exception($"Invalid Slider Name: {(sender as Slider).Name}");
            }
            Label.Content = (Name != null ? $"{Name}: " : "") + $"X={SliderX.Value}, Y={SliderY.Value}, Z={SliderZ.Value}";
            PointChanged?.Invoke(this, Point);
        }
        new string Name;
        public event Libraries.Events.MyEventHandler<Point3D> PointChanged;
        public void SetPoint(Point3D point)
        {
            Point = point;
            SliderX.Value = Point.X;
            SliderY.Value = Point.Y;
            SliderZ.Value = Point.Z;
        }
        public TunePointView(Point3D vector, string name = null)
        {
            Name = name;
            InitializeViews();
            SetPoint(vector);
        }
    }
}
