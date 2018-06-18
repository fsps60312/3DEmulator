using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DEmulator
{
    class BouncingHexagon
    {
        public Model3DGroup model = null;
        double M = 1;
        double I;
        double /*X=0,*/ Y = 0, T = 0;
        List<double> endPoints;
        double R;
        double /*speedX = 0,*/ speedY = 0, speedT = 0;
        double bounceCoe = 0.7;
        MyTrans origin;
        public BouncingHexagon(double ratio, double r, bool createModel)
        {
            R = r;
            //https://zh.wikipedia.org/wiki/轉動慣量列表
            endPoints = new List<double>();
            endPoints.Add(0);
            endPoints.Add(Math.PI / (2 + ratio));
            endPoints.Add(Math.PI * (1 + ratio) / (2 + ratio));
            endPoints = endPoints.Concat(endPoints.Select(a => a + Math.PI)).ToList();
            I = M * GetI(endPoints.Select(a => new Tuple<double, double>(r * Math.Cos(a), r * Math.Sin(a))).ToList());
            if (createModel)
            {
                model = CreateHex(endPoints, r, r / 2);
                origin = new MyTrans(model);
            }
        }
        static double Cross(Tuple<double, double> a, Tuple<double, double> b) { return a.Item1 * b.Item2 - b.Item1 * a.Item2; }
        static double Dot(Tuple<double, double> a, Tuple<double, double> b) { return a.Item1 * b.Item1 + a.Item2 * b.Item2; }
        static double GetI(List<Tuple<double, double>> p)
        {
            int n = p.Count;
            p.Add(p[0]);
            double a = 0, b = 0;
            for (int i = 0; i < n; i++)
            {
                double c = Math.Abs(Cross(p[i + 1], p[i]));
                a += c * (Dot(p[i + 1], p[i + 1]) + Dot(p[i + 1], p[i]) + Dot(p[i], p[i]));
                b += c;
            }
            return a / b / 6;
        }
        double GetNsToStop(double x, double y)
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
        double GetNs(double x, double y)
        {
            return (1.0 + bounceCoe) * GetNsToStop(x, y);
        }
        void ApplyNs(double x, double y, double ns)
        {
            speedY += ns / M;
            speedT += ns * (x * Math.Cos(T) - y * Math.Sin(T)) / I;
        }
        void Bounce(double x, double y)
        {
            double ns = GetNs(x, y);
            ApplyNs(x, y, ns);
        }
        public void DropFrom(double h, double t)
        {
            /*X = 0;*/
            Y = h; T = t;
            /*speedX =*/
            speedY = speedT = 0;
        }
        double SpeedUp(double x, double y, double r)
        {
            ///Y+t*speedY-gt^2/2+ x*Math.Sin(T+speedT*t) + y*Math.Cos(T+speedT*t) = 0
            ///speedY-gt+ speedT*x*cos(T+speedT*t) - speedT*y*sin(T+speedT*t) = 0
            ///-g - speedT^2*x*sin(T+speedT*t) - speedT^2*y*cos(T+speedT*t) = 0
            ///-speedT^3*x*cos(T+speedT*t) + speedT^3*y*sin(T+speedT*t) = 0
            ///x*cos(T+speedT*t) = y*sin(T+speedT*t)
            //System.Diagnostics.Trace.Assert(Y <= r && speedY < 0);
            ///sy=speedY-r*speedT
            double sy = speedY - r * Math.Abs(speedT);//negative
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
            if (Y >= R && speedY > 0)
            {
                ///Y+t*speedY-gt^2/2=r
                ///-gt^2/2+speedY*t+(Y-r)=0
                ///t=(-speedY+-sqrt(speedY^2+2g(Y-r)))/(-g)
                /// =(speedY+-sqrt(speedY^2+2g(Y-r)))/(g)
                return (speedY + Math.Sqrt(Math.Pow(speedY, 2) + 2 * G * (Y - R))) / G;
            }
            else
            {
                return endPoints.Select(a => SpeedUp(R * Math.Cos(a), R * Math.Sin(a), R)).Min();
            }
        }
        bool IsCollide(double x, double y)
        {
            return Y + x * Math.Sin(T) + y * Math.Cos(T) < 0 &&
                speedY + speedT * (x * Math.Cos(T) - y * Math.Sin(T)) < 0;
        }
        const double G = 9.8;
        void Simulate(double dt)
        {
            foreach(var a in endPoints)
            {
                double x = R * Math.Cos(a), y = R * Math.Sin(a);
                if (IsCollide(x, y)) Bounce(x, y);
            }
            Y += speedY * dt;
            T += speedT * dt;
            if (T < 0) T += 2 * Math.PI;
            else if (T >= 2 * Math.PI) T -= 2 * Math.PI;
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
            if (!(0 <= angle && angle < 2.0 * Math.PI))
            {
                System.Windows.MessageBox.Show($"angle: {angle}\r\n2 pi - angle: {2.0 * Math.PI - angle}\r\n" +
                    $"from h: {dropFromHeight}\r\nfrom a: {dropFromAngle}");
            }
            System.Diagnostics.Trace.Assert(0 <= angle && angle < 2.0 * Math.PI);
            angle -= Math.PI / 2;
            if (angle < 0) angle += 2 * Math.PI;
            for(int i=endPoints.Count()-1;i>=0;i--)
            {
                if (endPoints[i] <= angle) return i;
            }
            System.Windows.MessageBox.Show($"endPoints: {string.Join(", ", endPoints.Select(a => a / Math.PI * 180))}\r\nangle: {angle}");
            throw new Exception();
        }
        public async Task<int> Start(double height, double angle, bool infinite = true)
        {
            dropFromHeight = height; dropFromAngle = angle;
            DropFrom(height, angle);
            //DropFrom(10, 30.0 / 180 * Math.PI);
            //int kkase = 0;
            while (true)
            {
                //if (kkase++ % 100 == 0) System.Diagnostics.Debug.WriteLine($"Y={Y},T={T}");
                double su = SpeedUp();
                if (su > 0)
                {
                    //System.Diagnostics.Debug.Write($"{DateTime.Now}\tSimulating...");
                    double fy = Y + su * speedY - G * su * su / 2, fsy = speedY - G * su, ft = (T + su * speedT) % (2 * Math.PI);
                    if (ft < 0) ft += 2 * Math.PI;
                    double tdt = dt * 1;
                    if (model != null)
                    {
                        for (; su - tdt > 0; su -= tdt)
                        {
                            Simulate(tdt);
                            await Show();
                        }
                    }
                    Y = fy; speedY = fsy; T = ft;
                    //System.Diagnostics.Debug.WriteLine($"OK");
                }
                else Simulate(dt);
                await Show();
                if (!infinite && Energy() < M * G * R)
                {
                    return AngleToZeroThree(T);
                }
            }
        }
        int kase = 0;
        async Task Show()
        {
            if (model == null) return;
            if (kase++ == 50)
            {
                await Task.Delay(5);
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
        public static Model3DGroup CreateHex(List<double>angles, double r, double depth)
        {
            //System.Windows.MessageBox.Show(string.Join(", ", angles.Select(a => a / Math.PI * 180)));
            Model3DGroup hex = new Model3DGroup();
            var p=angles.Select(a=> new Point3D(r*Math.Cos(a),0,r*Math.Sin(a))).ToArray();
            p = p.Concat(p.Select(v => new Point3D(v.X, depth, v.Z))).ToArray();
            var b = new SolidColorBrush(Colors.SlateGray);
            System.Diagnostics.Trace.Assert(angles.Count == 6);
            hex.Children.Add(CreateModel(b, p[0], p[1], p[2], p[3], p[4], p[5]));
            hex.Children.Add(CreateModel(b, p[11], p[10], p[9], p[8], p[7], p[6]));
            int n = angles.Count();
            for (int i = 0; i < n; i++) hex.Children.Add(CreateModel(b, p[i], p[n + i], p[n + (i + 1) % n], p[(i + 1) % n]));
            return hex;
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
