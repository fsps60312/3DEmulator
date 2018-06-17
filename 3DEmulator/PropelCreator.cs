using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DEmulator
{
    static class PropelCreator
    {
        static Model3DGroup CreateModel(Brush brush,Point3D p0, Point3D p1, Point3D p2)
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
        static Model3DGroup CreateBearing()
        {
            Model3DGroup cube = CreateCube(0.2, 0.2, 5);
            cube.Transform = new MyTrans(cube).Translate(new Vector3D(-0.1, -0.1, -5)).Value;
            Model3DGroup ans = new Model3DGroup();
            ans.Children.Add(cube);
            return ans;
        }
        static Model3DGroup CreateBlade()
        {
            Model3DGroup triangle = new Model3DGroup();
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(1, -1, 0);
            Point3D p3 = new Point3D(5, 0, 0);
            triangle.Children.Add(CreateModel(new SolidColorBrush(Colors.BlueViolet),p1, p2, p3));
            triangle.Children.Add(CreateModel(new SolidColorBrush(Colors.SlateBlue),p1, p3, p2));
            triangle.Transform = new MyTrans(triangle).Rotate(new Vector3D(1, 0, 0), 20).Value;
            return triangle;
        }
        static Model3DGroup CreateBlades()
        {
            Model3DGroup blades = new Model3DGroup();
            int n = 3;
            for(int i=0;i<n;i++)
            {
                double angle = 360.0 * i / n;
                var b = CreateBlade();
                b.Transform = new MyTrans(b).Rotate(new Vector3D(0, 0, 1), angle).Value;
                blades.Children.Add(b);
            }
            return blades;
        }
        public static Model3DGroup CreatePropel()
        {
            Model3DGroup ans = new Model3DGroup();
            var bs = CreateBlades();
            var bg = CreateBearing();
            ans.Children.Add(bs);
            ans.Children.Add(bg);
            return ans;
        }
    }
}
