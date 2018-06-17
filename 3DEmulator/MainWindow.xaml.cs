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
        const double WHratio = 3, R = 1;
        double W, H;
        BouncingRectangle br;
        ModelVisual3D Get3DTriangle()
        {
            br = new BouncingRectangle(W = R * Math.Sin(Math.Atan(WHratio)), H = R * Math.Cos(Math.Atan(WHratio)), true);
            //Model3DGroup triangle = new Model3DGroup();
            //Point3D p1 = new Point3D(0, 0, 0);
            //Point3D p2 = new Point3D(5, 0, 0);
            //Point3D p3 = new Point3D(0, 5, 0);
            //Point3D p4 = new Point3D(5, 5, 0);
            //Point3D p5 = new Point3D(0, 0, 5);
            //Point3D p6 = new Point3D(5, 0, 5);
            //Point3D p7 = new Point3D(0, 5, 5);
            //Point3D p8 = new Point3D(5, 5, 5);
            //triangle.Children.Add(CreateTriangleModel(p1, p2, p6));
            //triangle.Children.Add(CreateTriangleModel(p2, p4, p6));
            //triangle.Children.Add(CreateTriangleModel(p1, p6, p4));
            //triangle.Children.Add(CreateTriangleModel(p1, p4, p2));
            ModelVisual3D Model = new ModelVisual3D();
            Model.Content = br.model;// PropelCreator.CreatePropel();
            return Model;
        }
        ModelVisual3D Triangle;
        void triangleButtonClick()
        {
            this.MainViewPort.Children.Add(Triangle= Get3DTriangle());
            this.MainViewPort.Children.Add(BouncingRectangle.CreatePlane());
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
                break;
            }
            //await br.Start(10, 30.0 / 180.0 * Math.PI);
            //MessageBox.Show($"OK\r\nResult={await br.Start(6.3, 6.0 / 180.0 * Math.PI, false)}");
            int n = 360 * 6, m = 100 * 10;
            double minHeight = Math.Sqrt(W * W + H * H) / 2, maxHeight = minHeight * 4;
            int[,] result = new int[n, m + 1];
            int progress = 0,preprogress=-1;
            //int kase = 0;
            await Task.Run(() =>
            {
                Parallel.For(0, n, new Action<int>(i =>
                {
                    double angle = (double)(i*2+1) / n * Math.PI;
                    BouncingRectangle t = new BouncingRectangle(W, H, false);
                    for (int j = 0; j <= m; j++)
                    {
                        double height = (minHeight * (m - j) + maxHeight * (j)) / m;
                        result[i, j] = t.Start(height, angle, false).Result;
                        if(System.Threading.Interlocked.Increment(ref progress)*100.0/n/m>= preprogress)
                        {
                            preprogress++;
                            Dispatcher.Invoke(new Action(() => this.Title = $"{preprogress} %"));
                        }
                        //if(++kase%100==0)System.Diagnostics.Trace.WriteLine($"{System.Threading.Interlocked.Increment(ref progress)}/{n*m}\t[{i},{j}]={result[i, j]}");
                    }
                }));
            });
            this.Title = "OK";
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter("result.txt"))
            {
                for (int i = 0; i < n; i++)
                {
                    double angle = (double)(i * 2 + 1) / n * Math.PI;
                    for (int j = 0; j <= m; j++)
                    {
                        double height = (minHeight * (m - j) + maxHeight * (j)) / m;
                        writer.WriteLine($"{angle}\t{height}\t{result[i, j]}");
                    }
                }
            }
            {
                var c = new byte[4, 4]
                {
                    {255,255,0,0 },
                    {255,255,255,0 },
                    {255,0,255,0 },
                    {255,0,0,255 }
                };
                var format = PixelFormats.Bgra32;
                var stride = (m * format.BitsPerPixel + 7) / 8;
                var arr = new byte[n *stride];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        int k = i * stride + j * 4, v = result[i, j];
                        arr[k + 0] = c[v, 3];
                        arr[k + 1] = c[v, 2];
                        arr[k + 2] = c[v, 1];
                        arr[k + 3] = c[v, 0];
                    }
                }
                try
                {
                    var bmp = BitmapImage.Create(m, n, 96, 96, format, null, arr, stride);
                    //MessageBox.Show("OK");
                    using (var stream = new System.IO.FileStream("img.bmp", System.IO.FileMode.Create))
                    {

                        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                        System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                        BitmapImage bImg = new BitmapImage();

                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                        encoder.Save(memoryStream);

                        //memoryStream.Position = 0;
                        //bImg.BeginInit();
                        //bImg.StreamSource = memoryStream;
                        //bImg.EndInit();

                        //memoryStream.Close();
                        //bImg.Freeze();
                        //var s = bImg.StreamSource;
                        var buf = new byte[1024];
                        memoryStream.Position = 0;
                        for (int nn; (nn = await memoryStream.ReadAsync(buf, 0, buf.Length)) != 0;)
                        {
                            await stream.WriteAsync(buf, 0, nn);
                        }
                        stream.Close();
                    }
                    this.Content = new Image { Source = bmp };
                }
                catch (Exception error) { MessageBox.Show(error.ToString()); }
            }
            await Task.Delay(10000);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            //await br.Start(10, 30.0 / 180.0 * Math.PI, false);
            //try
            //{
            //    double[,] data = new double[n, m];
            //    for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) data[i, j] = result[i, j];
            //    this.MainViewPort.Children.Clear();
            //    this.MainViewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(-1, -2, -3)) });
            //    this.MainViewPort.Children.Add(new ModelVisual3D { Content = CreateSurface(data, 0 / 180.0 * Math.PI, (n - 1) / 180.0 * Math.PI, minHeight, maxHeight) });
            //}
            //catch(Exception error) { MessageBox.Show(error.ToString()); }
        }
        void InitializeViews()
        {
            //new TuneWindow(Camera, EnvironmentLight).Show();
            //new PictureWindow(Constants.picturePort).Show();
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
