using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace _3DEmulator
{
    public partial class SimulateBouncingRectangleWindow:Window
    {
        enum ParameterTypeEnum { Height, Angle, WHratio }
        ParameterTypeEnum ParameterType=ParameterTypeEnum.WHratio;
        Parameter minValue, maxValue, currentValue;
        int kase_counter = 0;
        object imageUpdateSyncRoot = new object();
        Action updateInfo = new Action(()=> { });
        async void Draw()
        {
            int kase;
            lock (imageUpdateSyncRoot) kase = System.Threading.Interlocked.Increment(ref kase_counter);
            this.Title = $"Drawing... #{kase}";
            updateInfo();
            await Task.Run(async() =>
            {
                for (int _n = 1 << 6; _n <= 10000; _n <<= 1)
                {
                    //System.Diagnostics.Debug.WriteLine($"kase={kase}, n={n}");
                    if (kase != kase_counter) return;
                    {
                        int n = (int)(_n / Math.Sqrt(2));
                        var result = await Simulate(n, n, kase);
                        if (kase != kase_counter) return;
                        var source = await ToImageSource(result);
                        lock (imageUpdateSyncRoot)
                        {
                            if (kase != kase_counter) return;
                            Dispatcher.Invoke(new Action(() =>
                            {
                                this.Title = $"#{kase} n={n}";
                                IMGmap.Source = source;
                            }));
                        }
                    }
                    if (kase != kase_counter) return;
                    {
                        int n = _n;
                        var result = await Simulate(n, n, kase);
                        if (kase != kase_counter) return;
                        var source = await ToImageSource(result);
                        lock (imageUpdateSyncRoot)
                        {
                            if (kase != kase_counter) return;
                            Dispatcher.Invoke(new Action(() =>
                            {
                                this.Title = $"#{kase} n={n}";
                                IMGmap.Source = source;
                            }));
                        }
                    }
                }
            });
        }
        async Task<int[,]> Simulate(int n, int m, int kase)//return n*m array
        {
            //System.Diagnostics.Debug.Write($"Simulate({n},{m},{kase})...");
            if (kase != kase_counter) return null;
            n--; m--;
            int[,] result = new int[n + 1, m + 1];
            int progress = 0, preprogress = -1;
            var startTime = DateTime.Now;
            await Task.Run(() =>
            {
                System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;
                Parallel.For(0, n + 1, new ParallelOptions { MaxDegreeOfParallelism = 3 }, new Action<int>(i =>
                       {
                           System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;
                           if (kase != kase_counter) return;
                           for (int j = 0; j <= m && kase == kase_counter; j++)
                           {
                               if (System.Threading.Interlocked.Increment(ref progress) * 100 / (n + 1) / (m + 1) > preprogress)
                               {
                                   Dispatcher.Invoke(new Action(() => this.Title = $"#{kase} Drawing n={n} m={m} {++preprogress} %"));
                               }
                               Parameter p = new Parameter(this, 0, 0, 0);
                               p.z = currentValue.z;
                               p.y = (minValue.y * (n - i) + maxValue.y * (i)) / n;
                               p.x = (minValue.x * (m - j) + maxValue.x * (j)) / m;
                               result[i, j] = Query(p);
                           }
                       }));
            });
            if(kase==kase_counter&&(DateTime.Now-startTime).TotalSeconds>10)
            {
                await SaveImage(await ToImageSource(result), $"{kase}.bmp");
            }
            //System.Diagnostics.Debug.WriteLine("OK");
            return result;
        }
        async Task SaveImage(BitmapSource bmp,string fileName)
        {
            //MessageBox.Show("OK");
            using (var stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
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
        }
        async Task<BitmapSource> ToImageSource(int[,]result)
        {
            int n = result.GetLength(0), m = result.GetLength(1);
            var c = new byte[4, 4]
            {
                    {255,255,0,0 },
                    //{255,255,128+128/3,0 },
                    //{255,128+128/3,255,0 },
                    { 255,255,255,0},
                    {255,0,255,0 },
                    //{255,255,0,0 },
                    //{255,0,255,0 },
                    {255,0,0,255 }
                    //{255,255,0,255 }
            };
            var format = PixelFormats.Bgra32;
            var stride = (m * format.BitsPerPixel + 7) / 8;
            var arr = new byte[n * stride];
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
            var ans = BitmapSource.Create(m, n, 96, 96, format, null, arr, stride);
            ans.Freeze();
            return ans;
        }
        int Simulate(Parameter v)
        {
            //System.Diagnostics.Debug.WriteLine($"Simulate(h={v.height},a={v.angle},r={v.WHratio}){{w={Math.Sin(Math.Atan(v.WHratio))*2},h={Math.Cos(Math.Atan(v.WHratio)) * 2}}}");
            var t = new BouncingRectangle(Math.Sin(Math.Atan(v.WHratio))*2,Math.Cos(Math.Atan(v.WHratio))*2, false);
            var ans= t.Start(v.height, v.angle/180*Math.PI,false).Result;
            //System.Diagnostics.Debug.WriteLine("OK");
            return ans;
        }
        public SimulateBouncingRectangleWindow()
        {
            minValue = new Parameter(this, 1, 0, 1);
            maxValue = new Parameter(this, 2, 360, 3);
            currentValue = new Parameter(this, 2, 30, 1);
            InitializeViews();
            triangleButtonClick();
        }
        //BouncingRectangle br;
        //ModelVisual3D Triangle;
        DirectionalLight EnvironmentLight = new DirectionalLight(Colors.White, new Vector3D(2, 3, 1));
        PerspectiveCamera Camera = new PerspectiveCamera(new Point3D(0, -20, 1), new Vector3D(0, 20, -1), new Vector3D(0, 0, 1), 75);
        Viewport3D MainViewPort;
        Image IMGmap;
        UIElement SetGridPosition(int column, int row, UIElement uI)
        {
            Grid.SetColumn(uI, column);
            Grid.SetRow(uI, row);
            return uI;
        }
        UIElement SetGridPosition(int column, int row, UIElement uI, int columnSpan, int rowSpan)
        {
            Grid.SetColumn(uI, column);
            Grid.SetRow(uI, row);
            Grid.SetColumnSpan(uI, columnSpan);
            Grid.SetRowSpan(uI, rowSpan);
            return uI;
        }
        RadioButton NewRadioButton(string text,Action action,bool check=false)
        {
            var rb = new RadioButton { Content = text, IsChecked = check };
            rb.Click += delegate { action(); };
            return rb;
        }
        //int maxDictSize = 1000000;
        //Dictionary<Parameter, int> dict = new Dictionary<Parameter, int>();
        //Random rand = new Random();
        int Query(Parameter v)
        {
            return Simulate(v);
            //lock (dict)
            //{
            //    if (!dict.ContainsKey(v))
            //    {
            //        if (dict.Count >= maxDictSize) dict.Remove(dict.ElementAt(rand.Next(dict.Count)).Key);
            //        dict.Add(v, Simulate(v));
            //    }
            //    return dict[v];
            //}
        }
        Button NewButton(string text,Action action)
        {
            var b = new Button { Content = text };
            b.Click += delegate { action(); };
            return b;
        }
        Label LBstatus;
        void InitializeViews()
        {
            double moveRatio = 0.1, zoomRatio = Math.Pow(2,0.5);
            this.Width = 1200;
            this.Height = this.Width * 2 / 3;
            this.Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition{Height=new GridLength(3,GridUnitType.Star)},
                    new RowDefinition{Height=new GridLength(1,GridUnitType.Star)}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width=new GridLength(2,GridUnitType.Star)},
                    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)}
                },
                Children =
                {
                    SetGridPosition(0,0,IMGmap= new Image(),1,2),
                    SetGridPosition(1,0,MainViewPort = new Viewport3D { ClipToBounds = true }),
                    SetGridPosition(1,1,new Func<UIElement>(()=>
                    {
                        Label l1=new Label(),l2=new Label(),lx=new Label(),ly=new Label();
                        TextBox txbFixed=new TextBox();
                        txbFixed.AcceptsReturn=false;
                        txbFixed.TextInput+=async delegate
                        {
                            double v;
                            if(double.TryParse(txbFixed.Text,out v))
                            {
                                currentValue.z=v;
                                txbFixed.Background=Brushes.LightGreen;
                                Draw();
                            }
                            else txbFixed.Background=Brushes.Red;
                            await Task.Delay(500);
                            txbFixed.Background=Brushes.White;
                        };
                        updateInfo=new Action(()=>
                        {
                            lx.Content=$"{minValue.x} ~ {maxValue.x}";
                            ly.Content=$"{minValue.y} ~ {maxValue.y}";
                            txbFixed.Text=currentValue.z.ToString();
                        });
                        string[] tx={"Height","Angle","WHratio"};
                        return new Grid
                        {
                            RowDefinitions=
                            {
                                new RowDefinition{Height=new GridLength(1,GridUnitType.Star)},
                                new RowDefinition{Height=new GridLength(1,GridUnitType.Star)},
                                new RowDefinition{Height=new GridLength(1,GridUnitType.Star)},
                                new RowDefinition{Height=new GridLength(1,GridUnitType.Star)},
                                new RowDefinition{Height=new GridLength(1,GridUnitType.Star)},
                                new RowDefinition{Height=new GridLength(1,GridUnitType.Star)}
                            },
                            ColumnDefinitions=
                            {
                                new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
                                new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
                                new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
                                new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
                                new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)}
                            },
                            Children=
                            {
                                SetGridPosition(0,0,LBstatus=new Label{Content="Hello world!"},5,1),
                                SetGridPosition(0,1,NewRadioButton(tx[0],delegate{ParameterType=ParameterTypeEnum.Height; l1.Content=tx[1];l2.Content=tx[2];Draw(); })),
                                SetGridPosition(1,1,NewRadioButton(tx[1],delegate{ParameterType=ParameterTypeEnum.Angle;l1.Content=tx[0];l2.Content=tx[2];Draw(); })),
                                SetGridPosition(2,1,NewRadioButton(tx[2],delegate{ParameterType=ParameterTypeEnum.WHratio;l1.Content=tx[0];l2.Content=tx[1];Draw(); },true)),
                                SetGridPosition(3,1,txbFixed,2,1),
                                SetGridPosition(0,2,l1,1,2),
                                SetGridPosition(0,4,l2,1,2),
                                SetGridPosition(1,2,NewButton("←→",new Action(()=>{maxValue.x=minValue.x+(maxValue.x-minValue.x)*zoomRatio;Draw(); }))),
                                SetGridPosition(2,2,NewButton("→←",new Action(()=>{maxValue.x=minValue.x+(maxValue.x-minValue.x)/zoomRatio;Draw(); }))),
                                SetGridPosition(3,2,NewButton("←",new Action(()=>{var v=(maxValue.x-minValue.x)*moveRatio;minValue.x-=v; maxValue.x-=v;Draw(); }))),
                                SetGridPosition(4,2,NewButton("→",new Action(()=>{var v=(maxValue.x-minValue.x)*moveRatio;minValue.x+=v; maxValue.x+=v;Draw(); }))),
                                SetGridPosition(1,3,lx,4,1),
                                SetGridPosition(1,4,NewButton("←→",new Action(()=>{maxValue.y=minValue.y+(maxValue.y-minValue.y)*zoomRatio;Draw(); }))),
                                SetGridPosition(2,4,NewButton("→←",new Action(()=>{maxValue.y=minValue.y+(maxValue.y-minValue.y)/zoomRatio;Draw(); }))),
                                SetGridPosition(3,4,NewButton("↓",new Action(()=>{var v=(maxValue.y-minValue.y)*moveRatio;minValue.y-=v; maxValue.y-=v;Draw(); }))),
                                SetGridPosition(4,4,NewButton("↑",new Action(()=>{var v=(maxValue.y-minValue.y)*moveRatio;minValue.y+=v; maxValue.y+=v;Draw(); }))),
                                SetGridPosition(1,5,ly,4,1)
                            }
                        };
                    })())
                }
            };
            IMGmap.MouseLeftButtonDown += IMGmap_MouseLeftButtonDown;
            //default(Grid).colu
            //new TuneWindow(Camera, EnvironmentLight).Show();
            //new PictureWindow(Constants.picturePort).Show();
        }

        BouncingRectangle br=null;
        private async void IMGmap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (br != null)
            {
                br.Cancel();
                br = null;
            }
            var p = new Func<Point, Point>(_ => new Point(_.X / IMGmap.ActualWidth, _.Y / IMGmap.ActualHeight))(e.GetPosition(IMGmap));
            currentValue.x = minValue.x * (1 - p.X) + maxValue.x * p.X;
            currentValue.y = minValue.y * (1 - p.Y) + maxValue.y * p.Y;
            LBstatus.Content = currentValue.ToString();
            this.MainViewPort.Children.Clear();
            this.MainViewPort.Children.Add(BouncingRectangle.CreatePlane());
            this.MainViewPort.Children.Add(new ModelVisual3D { Content = EnvironmentLight });
            br = new BouncingRectangle(Math.Sin(Math.Atan(currentValue.WHratio)) * 2, Math.Cos(Math.Atan(currentValue.WHratio)) * 2, true);
            this.MainViewPort.Children.Add(new ModelVisual3D { Content = br.model });
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
            await br.Start(currentValue.height, currentValue.angle / 180 * Math.PI);
            //System.Diagnostics.Debug.WriteLine("OK");
        }

        async void triangleButtonClick()
        {
            ModelVisual3D Triangle;
            this.MainViewPort.Children.Add(Triangle=Get3DTriangle());
            this.MainViewPort.Children.Add(BouncingRectangle.CreatePlane());
            ModelVisual3D modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = EnvironmentLight;
            this.MainViewPort.Children.Add(modelVisual3D);
            Camera.FarPlaneDistance = 100;
            Camera.NearPlaneDistance = 1;
            this.MainViewPort.Camera = Camera;
            while (true)
            {
                //for(int i=0;i<60;i++)
                //{
                //    await Task.Delay(10);
                //    EnvironmentLight.Direction = new Vector3D(Math.Cos(Math.PI * 2 * i / 60), Math.Sin(Math.PI * 2 * i / 60), -1);
                //}
                for (int i = 0; i < 60*4; i++)
                {
                    await Task.Delay(10);
                    var v = Triangle.Transform.Value;
                    v.Rotate(new Quaternion(new Vector3D(0, 0, 1), 360 * 0.5 / 60));// = new RotateTransform3D(new AxisAngleRotation3D());
                    Triangle.Transform = new MatrixTransform3D(v);
                }
                break;
            }
            Draw();
        }
        ModelVisual3D Get3DTriangle()
        {
            double WHratio = 3, R = 1;
            //br = new BouncingRectangle(W = R * Math.Sin(Math.Atan(WHratio)), H = R * Math.Cos(Math.Atan(WHratio)), true);
            BouncingHexagon br;
            br = new BouncingHexagon(WHratio, R, true);
            ModelVisual3D Model = new ModelVisual3D();
            Model.Content = br.model;
            return Model;
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
        static Model3DGroup CreateSurface(double[,] data, double xMin, double xMax, double yMin, double yMax)
        {
            int n = data.GetLength(0) - 1, m = data.GetLength(1) - 1;
            {
                xMin = 0; xMax = 10;
                yMin = 0; yMax = 10;
                double vMin = double.MaxValue, vMax = double.MinValue;
                foreach (var i in data)
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
            for (int i = 0; i < n; i++)
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
