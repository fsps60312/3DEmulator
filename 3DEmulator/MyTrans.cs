using System.Windows.Media.Media3D;

namespace _3DEmulator
{
    class MyTrans
    {
        Matrix3D v;
        public MyTrans(Model3DGroup a) { v = a.Transform.Value; }
        MyTrans(Matrix3D v) { this.v = v; }
        public MyTrans Rotate(Vector3D a,double b) { v.Rotate(new Quaternion(a, b)); return this; }
        public MyTrans Translate(Vector3D a) { v.Translate(a); return this; }
        public MatrixTransform3D Value { get { return new MatrixTransform3D(v); } }
        public MyTrans Copy() { return new MyTrans(v); }
    }
}
