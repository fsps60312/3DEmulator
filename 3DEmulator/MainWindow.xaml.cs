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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace _3DEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private Vector3D CalculateTraingleNormal(Point3D p0, Point3D p1, Point3D p2)
        //{
        //    Vector3D v0 = new Vector3D(
        //        p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
        //    Vector3D v1 = new Vector3D(
        //        p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        //    return Vector3D.CrossProduct(v0, v1);
        //}
        ModelVisual3D Get3DTriangle()
        {
            ModelVisual3D Model = new ModelVisual3D();
            Model.Content = PropelCreator.CreatePropel();
            return Model;
        }
        ModelVisual3D Triangle;
        void triangleButtonClick()
        {
            this.MainViewPort.Children.Add(Triangle= Get3DTriangle());
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = EnvironmentLight;
            this.MainViewPort.Children.Add(modelVisual3D);
            Camera.FarPlaneDistance = 100;
            Camera.NearPlaneDistance = 1;
            this.MainViewPort.Camera = Camera;
        }
        DirectionalLight EnvironmentLight= new DirectionalLight(Colors.White, new Vector3D(2, 3, 1));
        PerspectiveCamera Camera = new PerspectiveCamera(new Point3D(0,-20,1), new Vector3D(0,20,-1), new Vector3D(0, 0, 1), 75);
        async void StartAnimation()
        {
            while (true)
            {
                //for(int i=0;i<60;i++)
                //{
                //    await Task.Delay(10);
                //    EnvironmentLight.Direction = new Vector3D(Math.Cos(Math.PI * 2 * i / 60), Math.Sin(Math.PI * 2 * i / 60), -1);
                //}
                for (int i = 0; i < 60; i++)
                {
                    await Task.Delay(10);
                    var v = Triangle.Transform.Value;
                    v.Rotate(new Quaternion(new Vector3D(0, 0, 1), 360 * 0.5 / 60));// = new RotateTransform3D(new AxisAngleRotation3D());
                    Triangle.Transform = new MatrixTransform3D(v);
                }
            }
        }
        void InitializeViews()
        {
            new TuneWindow(Camera, EnvironmentLight).Show();
            new SimulateBouncingRectangleWindow().Show();
        }
        public MainWindow()
        {
            InitializeComponent();
            InitializeViews();
            triangleButtonClick();
            StartAnimation();
        }
        #region model
        static Model3DGroup CreateModel(Brush brush, Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mymesh = new MeshGeometry3D();
            mymesh.Positions.Add(p0);
            mymesh.Positions.Add(p1);
            mymesh.Positions.Add(p2);
            mymesh.TriangleIndices.Add(0);
            mymesh.TriangleIndices.Add(1);
            mymesh.TriangleIndices.Add(2);
            //Vector3D Normal = CalculateTraingleNormal(p0, p1, p2);
            //mymesh.Normals.Add(Normal);
            //mymesh.Normals.Add(Normal);
            //mymesh.Normals.Add(Normal);
            Material Material = new DiffuseMaterial(brush);
            //new SolidColorBrush(Colors.BlueViolet));
            GeometryModel3D model = new GeometryModel3D(
                mymesh, Material);
            Model3DGroup Group = new Model3DGroup();
            Group.Children.Add(model);
            return Group;
        }
        static Model3DGroup CreateModel(Brush brush, Point3D p0, Point3D p1, Point3D p2, params Point3D[] p3)
        {
            Model3DGroup ans = new Model3DGroup();
            ans.Children.Add(CreateModel(brush, p0, p1, p2));
            ans.Children.Add(CreateModel(brush, p0, p2, p3[0]));
            for (int i = 1; i < p3.Length; i++) ans.Children.Add(CreateModel(brush, p0, p3[i - 1], p3[i]));
            return ans;
        }
        static Model3DGroup CreateSurface(double[,]data,double xMin,double xMax,double yMin,double yMax)
        {
            int n = data.GetLength(0) - 1, m = data.GetLength(1) - 1;
            {
                xMin = 0;xMax = 10;
                yMin = 0;yMax = 10;
                double vMin = double.MaxValue,vMax=double.MinValue;
                foreach(var i in data)
                {
                    vMin = Math.Min(vMin, i);
                    vMax = Math.Max(vMax, i);
                }
                for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) data[i, j] = (data[i, j] - vMin) / (vMax - vMin);
            }
            Model3DGroup ans = new Model3DGroup();
            var brush = new SolidColorBrush[4]
            {
                new SolidColorBrush(Color.FromArgb(128,255,0,0)),
                new SolidColorBrush(Color.FromArgb(128,128,128,0)),
                new SolidColorBrush(Color.FromArgb(128,0,255,255)),
                new SolidColorBrush(Color.FromArgb(128,0,0,255))
            };
            for (int i=0;i<n;i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Point3D a = new Point3D((xMin * (n - i) + xMax * i) / n, (yMin * (m - j) + yMax * j) / m, 0); i++;
                    Point3D b = new Point3D((xMin * (n - i) + xMax * i) / n, (yMin * (m - j) + yMax * j) / m, 0); i--; j++;
                    Point3D c = new Point3D((xMin * (n - i) + xMax * i) / n, (yMin * (m - j) + yMax * j) / m, 0); i++;
                    Point3D d = new Point3D((xMin * (n - i) + xMax * i) / n, (yMin * (m - j) + yMax * j) / m, 0); i--; j--;
                    ans.Children.Add(CreateModel(brush[(int)(data[i, j] * 3 + 0.5)], a, b, d, c));
                    ans.Children.Add(CreateModel(brush[(int)(data[i, j] * 3 + 0.5)], a, c, d, b));
                }
            }
            return ans;
        }
        #endregion
    }
}
