using System;

namespace _3DEmulator
{
    public partial class SimulateBouncingRectangleWindow
    {
        class Parameter
        {
            public override string ToString()
            {
                return $"height={height}, angle={angle}, WHratio={WHratio}";
            }
            public override int GetHashCode()
            {
                return (height.GetHashCode()<<1)^angle.GetHashCode()^(WHratio.GetHashCode()>>1);
            }
            public override bool Equals(object obj)
            {
                var v = (Parameter)obj;
                return height == v.height && angle == v.angle && WHratio == v.WHratio;
            }
            public double height, angle, WHratio;
            SimulateBouncingRectangleWindow parent;
            public Parameter(SimulateBouncingRectangleWindow p,double h,double a,double r) { parent = p; height = h;angle = a;WHratio = r; }
            public double x
            {
                get
                {
                    switch (parent.ParameterType)
                    {
                        case ParameterTypeEnum.WHratio:
                        case ParameterTypeEnum.Angle:
                            return height;
                        case ParameterTypeEnum.Height:
                            return angle;
                        default: throw new Exception($"Unknown parent.ParameterType: {parent.ParameterType}");
                    }
                }
                set
                {
                    switch (parent.ParameterType)
                    {
                        case ParameterTypeEnum.WHratio:
                        case ParameterTypeEnum.Angle:
                            height = value; break;
                        case ParameterTypeEnum.Height:
                            angle = value; break;
                        default: throw new Exception($"Unknown parent.ParameterType: {parent.ParameterType}");
                    }
                }
            }
            public double y
            {
                get
                {
                    switch (parent.ParameterType)
                    {
                        case ParameterTypeEnum.WHratio:
                            return angle;
                        case ParameterTypeEnum.Angle:
                        case ParameterTypeEnum.Height:
                            return WHratio;
                        default:throw new Exception($"Unknown parent.ParameterType: {parent.ParameterType}");
                    }
                }
                set
                {
                    switch (parent.ParameterType)
                    {
                        case ParameterTypeEnum.WHratio:
                            angle = value; break;
                        case ParameterTypeEnum.Angle:
                        case ParameterTypeEnum.Height:
                            WHratio = value;break;
                        default: throw new Exception($"Unknown parent.ParameterType: {parent.ParameterType}");
                    }
                }
            }
            public double z
            {
                get
                {
                    switch (parent.ParameterType)
                    {
                        case ParameterTypeEnum.WHratio:
                            return WHratio;
                        case ParameterTypeEnum.Angle:
                            return angle;
                        case ParameterTypeEnum.Height:
                            return height;
                        default: throw new Exception($"Unknown parent.ParameterType: {parent.ParameterType}");
                    }
                }
                set
                {
                    switch (parent.ParameterType)
                    {
                        case ParameterTypeEnum.WHratio:
                            WHratio = value; break;
                        case ParameterTypeEnum.Angle:
                            angle = value; break;
                        case ParameterTypeEnum.Height:
                            height = value; break;
                        default: throw new Exception($"Unknown parent.ParameterType: {parent.ParameterType}");
                    }
                }
            }
        }
    }
}
