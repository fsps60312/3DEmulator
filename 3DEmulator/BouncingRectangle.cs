using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DEmulator
{
    class BouncingRectangle
    {
        public Model3DGroup model=null;
        bool canceled = false;
        double M = 1;
        double I;
        double /*X=0,*/ Y=0,T=0;
        double W, H;
        double /*speedX = 0,*/ speedY = 0, speedT = 0;
        double bounceCoe = 0.7;
        MyTrans origin;
        public void Reset(double x, double z, bool createModel)
        {
            W = x; H = z;
            //https://zh.wikipedia.org/wiki/轉動慣量列表
            I = M / 12 * (W * W + H * H);
            var cube = CreateCube(x, 2, z);
            cube.Transform = new MyTrans(cube).Translate(new Vector3D(-x / 2, -1, -z / 2)).Value;
            if (createModel)
            {
                model = new Model3DGroup();
                model.Children.Add(cube);
                model.Children.Add(CreateModel(new SolidColorBrush(Colors.Red), new Point3D(x * 0.2, 0, -z * 0.6), new Point3D(-x * 0.2, 0, -z * 0.6), new Point3D(0, 0, -z * 0.8)));
                origin = new MyTrans(model);
            }
        }
        public BouncingRectangle() { }
        public BouncingRectangle(double x, double z,bool createModel)
        {
            Reset(x, z, createModel);
        }
        double GetNsToStop(double x,double y)
        {
            ///speed: speedY + (x sin(t) + y cos(t))'
            ///     = speedY + t x cos(T) - t y sin(T)
            ///     = speedY + t (x cos(T) - y sin(T))
            ///unknown: z = F*time (衝量)
            ///r(力矩) = z/time*(x cos(T) - y sin(T))
            ///dt = r*time/I = z * (x cos(T) - y sin(T)) / I
            ///equation: z/M + z * (x cos(T) - y sin(T)) / I * (x cos(T) - y sin(T))
            ///        = z/M + z * (x cos(T) - y sin(T))^2 / I
            ///        = - (speedY + t (x cos(T) - y sin(T)))
            return -(speedY + speedT * (x * Math.Cos(T) - y * Math.Sin(T))) /
                (1 / M + Math.Pow(x * Math.Cos(T) - y * Math.Sin(T), 2) / I);
        }
        double GetNs(double x,double y)
        {
            return (1.0 + bounceCoe) * GetNsToStop(x, y);
        }
        void ApplyNs(double x,double y,double ns)
        {
            speedY += ns / M;
            speedT += ns * (x * Math.Cos(T) - y * Math.Sin(T)) / I;
        }
        void Bounce(double x,double y)
        {
            double ns = GetNs(x, y);
            ApplyNs(x, y, ns);
        }
        public void DropFrom(double h,double t)
        {
            /*X = 0;*/ Y = h;T = t;
            /*speedX =*/ speedY = speedT = 0;
        }
        double SpeedUp(double x,double y,double r)
        {
            ///Y+t*speedY-gt^2/2+ x*Math.Sin(T+speedT*t) + y*Math.Cos(T+speedT*t) = 0
            ///speedY-gt+ speedT*x*cos(T+speedT*t) - speedT*y*sin(T+speedT*t) = 0
            ///-g - speedT^2*x*sin(T+speedT*t) - speedT^2*y*cos(T+speedT*t) = 0
            ///-speedT^3*x*cos(T+speedT*t) + speedT^3*y*sin(T+speedT*t) = 0
            ///x*cos(T+speedT*t) = y*sin(T+speedT*t)
            //System.Diagnostics.Trace.Assert(Y <= r && speedY < 0);
            ///sy=speedY-r*speedT
            double sy = speedY- r * Math.Abs(speedT);//negative
            double oy = Y + x * Math.Sin(T) + y * Math.Cos(T);
            ///oy+t*sy-gt^2/2=0
            ///-gt^2/2+sy*t+oy=0
            ///t=(-sy+-sqrt(sy^2+2*g*oy))/(-g)
            /// =(sy+-sqrt(sy^2+2*g*oy))/g
            double ans = (sy + Math.Sqrt(sy * sy + 2 * G * oy)) / G;
            return ans <= dt ? 0 : ans;
        }
        double SpeedUp()
        {
            double r = Math.Sqrt(W * W + H * H) / 2;
            if (Y >= r && speedY > 0)
            {
                ///Y+t*speedY-gt^2/2=r
                ///-gt^2/2+speedY*t+(Y-r)=0
                ///t=(-speedY+-sqrt(speedY^2+2g(Y-r)))/(-g)
                /// =(speedY+-sqrt(speedY^2+2g(Y-r)))/(g)
                return (speedY + Math.Sqrt(Math.Pow(speedY, 2) + 2 * G * (Y - r))) / G;
            }
            else
            {
                return new double[4]
                {
                    SpeedUp(-W/2,-H/2,r),
                    SpeedUp(-W/2,H/2,r),
                    SpeedUp(W/2,-H/2,r),
                    SpeedUp(W/2,H/2,r)
                }.Min();
            }
        }
        bool IsCollide(double x,double y)
        {
            return Y + x * Math.Sin(T) + y * Math.Cos(T) < 0 &&
                speedY + speedT * (x * Math.Cos(T) - y * Math.Sin(T)) < 0;
        }
        const double G = 9.8;
        double[,] corners;
        void Simulate(double dt)
        {
            for (int i = 0; i < 4; i++)
            {
                double x = corners[i, 0], y = corners[i, 1];
                if (IsCollide(x, y)) Bounce(x, y);
            }
            Y += speedY * dt;
            T += speedT * dt;
            if (T < 0) T += 2 * Math.PI;
            else if(T >= 2 * Math.PI) T -= 2 * Math.PI;
            speedY -= G * dt;
        }
        double dt = 0.0001;
        double Energy()
        {
            return M * G * Y + 0.5 * M * speedY * speedY + 0.5 * I * speedT * speedT;
        }
        double dropFromHeight, dropFromAngle;
        int AngleToZeroThree(double angle)
        {
            if(!(0 <= angle && angle < 2.0 * Math.PI))
            {
                System.Windows.MessageBox.Show($"angle: {angle}\r\n2 pi - angle: {2.0 * Math.PI - angle}\r\n" +
                    $"from h: {dropFromHeight}\r\nfrom a: {dropFromAngle}");
            }
            System.Diagnostics.Trace.Assert(0 <= angle && angle < 2.0 * Math.PI);
            double a = 0.5 * Math.PI - Math.Atan2(H, W);
            if (angle <= a) return 0;
            if (angle <= Math.PI - a) return 1;
            if (angle <= Math.PI + a) return 2;
            if (angle <= 2.0 * Math.PI - a) return 3;
            return 0;
        }
        public void Cancel() { canceled = true; }
        public async Task<int> Start(double height,double angle,bool infinite=true)
        {
            canceled = false;
            dropFromHeight = height;dropFromAngle = angle;
            DropFrom(height, angle);
            //DropFrom(10, 30.0 / 180 * Math.PI);
            corners = new double[4, 2]
            {
                {W/2,H/2 },
                {W/2,-H/2 },
                {-W/2,H/2 },
                {-W/2,-H/2 }
            };
            //int kkase = 0;
            while (true)
            {
                if (canceled) return 0;
                //if (kkase++ % 100 == 0) System.Diagnostics.Debug.WriteLine($"Y={Y},T={T}");
                double su = SpeedUp();
                if(su>0)
                {
                    //System.Diagnostics.Debug.Write($"{DateTime.Now}\tSimulating...");
                    double fy = Y + su * speedY - G * su * su / 2, fsy = speedY - G * su, ft = (T + su * speedT) % (2 * Math.PI);
                    if (ft < 0) ft += 2 * Math.PI;
                    double tdt = dt * 1;
                    if (model != null)
                    {
                        for (; su - tdt > 0 && !canceled; su -= tdt)
                        {
                            Simulate(tdt);
                            await Show();
                        }
                    }
                    Y = fy;speedY = fsy; T = ft;
                    //System.Diagnostics.Debug.WriteLine($"OK");
                }
                else Simulate(dt);
                await Show();
                if(!infinite&&Energy()<M*G*Math.Sqrt(W*W+H*H)/2)
                {
                    return AngleToZeroThree(T);
                }
            }
        }
        int kase = 0;
        async Task Show()
        {
            if (model == null) return;
            if(kase++==100)
            {
                await Task.Delay(10);
                model.Transform = origin.Copy().Rotate(new Vector3D(0, -1, 0), T / Math.PI * 180).Translate(new Vector3D(0, 0, Y)).Value;
                kase = 0;
            }
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
        public static Model3DGroup CreateCube(double x, double y, double z)
        {
            Model3DGroup cube = new Model3DGroup();
            Point3D[] p = new Point3D[8];
            for (int i = 0; i < 8; i++) p[i] = new Point3D(i % 2 * x, i / 2 % 2 * y, i / 4 * z);
            var b = new SolidColorBrush(Colors.SlateGray);
            ///1 3 4 2
            ///5 6 8 7
            ///1 2 6 5
            ///2 4 8 6
            ///3 7 8 4
            ///1 5 7 3
            cube.Children.Add(CreateModel(b, p[0], p[2], p[3], p[1]));
            cube.Children.Add(CreateModel(b, p[4], p[5], p[7], p[6]));
            cube.Children.Add(CreateModel(b, p[0], p[1], p[5], p[4]));
            cube.Children.Add(CreateModel(b, p[1], p[3], p[7], p[5]));
            cube.Children.Add(CreateModel(b, p[2], p[6], p[7], p[3]));
            cube.Children.Add(CreateModel(b, p[0], p[4], p[6], p[2]));
            return cube;
        }
        public static ModelVisual3D CreatePlane()
        {
            Model3DGroup triangle = new Model3DGroup();
            Point3D p1 = new Point3D(-5, -5, 0);
            Point3D p2 = new Point3D(5, -5, 0);
            Point3D p3 = new Point3D(-5, 5, 0);
            Point3D p4 = new Point3D(5, 5, 0);
            triangle.Children.Add(CreateModel(new SolidColorBrush(Colors.BlueViolet), p1, p2, p4, p3));
            triangle.Children.Add(CreateModel(new SolidColorBrush(Colors.SlateBlue), p1, p3, p4, p2));
            return new ModelVisual3D { Content = triangle };
        }
        #endregion
    }
}
