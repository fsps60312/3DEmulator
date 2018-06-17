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
    class TuneValueView:StackPanel
    {
        public event Libraries.Events.MyEventHandler<double> ValueChanged;
        new string Name;
        Label Label = new Label();
        Slider Slider = new Slider();
        public double Value { get { return IsExponential ? Math.Pow(10, Slider.Value) : Slider.Value; } }
        void SliderValueChanged()
        {
            Label.Content = (Name == null ? "" : $"{Name}: ") + Value.ToString();
            ValueChanged?.Invoke(this, Slider.Value);
        }
        void InitializeViews()
        {
            {
                this.Children.Add(Label);
            }
            {
                this.Children.Add(Slider);
                Slider.ValueChanged += (sender, e) => { SliderValueChanged(); };
            }
            SliderValueChanged();
        }
        public bool IsExponential = false;
        public TuneValueView(double value,double minimum,double maximum,string name=null)
        {
            Name = name;
            Slider.Minimum = minimum;
            Slider.Maximum = maximum;
            Slider.Value = value;
            InitializeViews();
        }
    }
}
