using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime;
using ILGPU;

namespace DispDICOMCMD
{
    //internal class PointComparer : IEqualityComparer<Point>
    //{
    //    public bool Equals(Point x, Point y)
    //    {
    //        if (x.Equals(y))
    //        {
    //            return true;
    //        }
    //        return false;
    //    }

    //    public int GetHashCode(Point obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}

    //public class HostHistoPyramid : IDisposable
    //{
    //    public MemoryBuffer2D<byte, Stride2D.DenseX> layer0;
    //    public MemoryBuffer2D<byte, Stride2D.DenseX> layer1;
    //    public MemoryBuffer2D<byte, Stride2D.DenseX> layer2;
    //    public MemoryBuffer2D<ushort, Stride2D.DenseX> layer3;
    //    public MemoryBuffer2D<ushort, Stride2D.DenseX> layer4;
    //    public MemoryBuffer2D<ushort, Stride2D.DenseX> layer5;
    //    public MemoryBuffer2D<ushort, Stride2D.DenseX> layer6;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer7;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer8;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer9;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer10;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer11;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer12;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer13;
    //    public MemoryBuffer2D<uint, Stride2D.DenseX> layer14;
    //    public MemoryBuffer2D<ulong, Stride2D.DenseX> layer15;
    //    public byte size;
    //    public HistoPyramid Histo;


    //    public HostHistoPyramid(int[][,] HP, int n, Accelerator acc)
    //    {
    //        size = (byte)n;
    //        byte[] tempBytes = HP[0].Cast<int>().Select(x => (byte)x).ToArray();
    //        byte[,] temp2Dbytes = new byte[HP[0].GetLength(0), HP[0].GetLength(0)];
    //        Buffer.BlockCopy(tempBytes, 0, temp2Dbytes, 0, tempBytes.Length);
    //        layer0 = acc.Allocate2DDenseX<byte>(temp2Dbytes);

    //        tempBytes = HP[1].Cast<int>().Select(x => (byte)x).ToArray();
    //        temp2Dbytes = new byte[HP[1].GetLength(0), HP[1].GetLength(1)];
    //        Buffer.BlockCopy(tempBytes, 0, temp2Dbytes, 0, tempBytes.Length);
    //        layer1 = acc.Allocate2DDenseX<byte>(temp2Dbytes);

    //        tempBytes = HP[2].Cast<int>().Select(x => (byte)x).ToArray();
    //        temp2Dbytes = new byte[HP[2].GetLength(0), HP[2].GetLength(0)];
    //        Buffer.BlockCopy(tempBytes, 0, temp2Dbytes, 0, tempBytes.Length);
    //        layer2 = acc.Allocate2DDenseX<byte>(temp2Dbytes);

    //        ushort[] tempUshorts = HP[3].Cast<int>().Select(x => (ushort)x).ToArray();
    //        ushort[,] temp2DUshorts = new ushort[HP[3].GetLength(0), HP[3].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, temp2DUshorts, 0, tempUshorts.Length);
    //        layer3 = acc.Allocate2DDenseX<ushort>(temp2DUshorts);

    //        tempUshorts = HP[4].Cast<int>().Select(x => (ushort)x).ToArray();
    //        temp2DUshorts = new ushort[HP[4].GetLength(0), HP[4].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, temp2DUshorts, 0, tempUshorts.Length);
    //        layer4 = acc.Allocate2DDenseX<ushort>(temp2DUshorts);

    //        tempUshorts = HP[5].Cast<int>().Select(x => (ushort)x).ToArray();
    //        temp2DUshorts = new ushort[HP[5].GetLength(0), HP[5].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, temp2DUshorts, 0, tempUshorts.Length);
    //        layer5 = acc.Allocate2DDenseX<ushort>(temp2DUshorts);

    //        tempUshorts = HP[6].Cast<int>().Select(x => (ushort)x).ToArray();
    //        temp2DUshorts = new ushort[HP[6].GetLength(0), HP[6].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, temp2DUshorts, 0, tempUshorts.Length);
    //        layer6 = acc.Allocate2DDenseX<ushort>(temp2DUshorts);

    //        uint[] tempUints = HP[7].Cast<int>().Select(x => (uint)x).ToArray();
    //        uint[,] temp2DUints = new uint[HP[7].GetLength(0), HP[7].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer7 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        tempUints = HP[8].Cast<int>().Select(x => (uint)x).ToArray();
    //        temp2DUints = new uint[HP[8].GetLength(0), HP[8].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer8 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        tempUints = HP[9].Cast<int>().Select(x => (uint)x).ToArray();
    //        temp2DUints = new uint[HP[9].GetLength(0), HP[9].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer9 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        tempUints = HP[10].Cast<int>().Select(x => (uint)x).ToArray();
    //        temp2DUints = new uint[HP[10].GetLength(0), HP[10].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer10 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        tempUints = HP[11].Cast<int>().Select(x => (uint)x).ToArray();
    //        temp2DUints = new uint[HP[11].GetLength(0), HP[11].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer11 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        tempUints = HP[12].Cast<int>().Select(x => (uint)x).ToArray();
    //        temp2DUints = new uint[HP[12].GetLength(0), HP[12].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer12 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        tempUints = HP[13].Cast<int>().Select(x => (uint)x).ToArray();
    //        temp2DUints = new uint[HP[13].GetLength(0), HP[13].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer13 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        tempUints = HP[14].Cast<int>().Select(x => (uint)x).ToArray();
    //        temp2DUints = new uint[HP[14].GetLength(0), HP[14].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
    //        layer14 = acc.Allocate2DDenseX<uint>(temp2DUints);

    //        ulong[] tempUlong = HP[15].Cast<int>().Select(x => (ulong)x).ToArray();
    //        ulong[,] temp2DUlong = new ulong[HP[15].GetLength(0), HP[15].GetLength(0)];
    //        Buffer.BlockCopy(tempUlong, 0, temp2DUlong, 0, tempUlong.Length);
    //        layer15 = acc.Allocate2DDenseX<ulong>(temp2DUlong);

    //        Histo = new HistoPyramid(layer0.View,
    //            layer1.View,
    //            layer2.View,
    //            layer3.View,
    //            layer4.View,
    //            layer5.View,
    //            layer6.View,
    //            layer7.View,
    //            layer8.View,
    //            layer9.View,
    //            layer10.View,
    //            layer11.View,
    //            layer12.View,
    //            layer13.View,
    //            layer14.View,
    //            layer15.View,
    //            size
    //            );
    //    }

    //    public void Dispose()
    //    {
    //        layer0.Dispose();
    //        layer1.Dispose();
    //        layer2.Dispose();
    //        layer3.Dispose();
    //        layer4.Dispose();
    //        layer5.Dispose();
    //        layer6.Dispose();
    //        layer7.Dispose();
    //        layer8.Dispose();
    //        layer9.Dispose();
    //        layer10.Dispose();
    //        layer11.Dispose();
    //        layer12.Dispose();
    //        layer13.Dispose();
    //        layer14.Dispose();
    //        layer15.Dispose();
    //    }
    //}

    //public struct HistoPyramid
    //{
    //    public byte[,] layer0;
    //    public byte[,] layer1;
    //    public byte[,] layer2;
    //    public ushort[,] layer3;
    //    public ushort[,] layer4;
    //    public ushort[,] layer5;
    //    public ushort[,] layer6;
    //    public uint[,] layer7;
    //    public uint[,] layer8;
    //    public uint[,] layer9;
    //    public uint[,] layer10;
    //    public uint[,] layer11;
    //    public uint[,] layer12;
    //    public uint[,] layer13;
    //    public uint[,] layer14;
    //    public ulong[,] layer15;
    //    public byte size;

    //    public HistoPyramid(int[][,] HP, int n)
    //    {
    //        size = (byte)n;
    //        byte[] tempBytes = HP[0].Cast<int>().Select(x => (byte)x).ToArray();
    //        layer0 = new byte[HP[0].GetLength(0), HP[0].GetLength(0)];
    //        Buffer.BlockCopy(tempBytes, 0, layer0, 0, tempBytes.Length);

    //        tempBytes = HP[1].Cast<int>().Select(x => (byte)x).ToArray();
    //        layer1 = new byte[HP[1].GetLength(0), HP[1].GetLength(1)];
    //        Buffer.BlockCopy(tempBytes, 0, layer1, 0, tempBytes.Length);

    //        tempBytes = HP[2].Cast<int>().Select(x => (byte)x).ToArray();
    //        layer2 = new byte[HP[2].GetLength(0), HP[2].GetLength(0)];
    //        Buffer.BlockCopy(tempBytes, 0, layer2, 0, tempBytes.Length);

    //        ushort[] tempUshorts = HP[3].Cast<int>().Select(x => (ushort)x).ToArray();
    //        layer3 = new ushort[HP[3].GetLength(0), HP[3].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, layer3, 0, tempUshorts.Length);

    //        tempUshorts = HP[4].Cast<int>().Select(x => (ushort)x).ToArray();
    //        layer4 = new ushort[HP[4].GetLength(0), HP[4].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, layer4, 0, tempUshorts.Length);

    //        tempUshorts = HP[5].Cast<int>().Select(x => (ushort)x).ToArray();
    //        layer5 = new ushort[HP[5].GetLength(0), HP[5].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, layer5, 0, tempUshorts.Length);

    //        tempUshorts = HP[6].Cast<int>().Select(x => (ushort)x).ToArray();
    //        layer6 = new ushort[HP[6].GetLength(0), HP[6].GetLength(0)];
    //        Buffer.BlockCopy(tempUshorts, 0, layer6, 0, tempUshorts.Length);

    //        uint[] tempUints = HP[7].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer7 = new uint[HP[7].GetLength(0), HP[7].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer7, 0, tempUints.Length);

    //        tempUints = HP[8].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer8 = new uint[HP[8].GetLength(0), HP[8].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer8, 0, tempUints.Length);

    //        tempUints = HP[9].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer9 = new uint[HP[9].GetLength(0), HP[9].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer9, 0, tempUints.Length);

    //        tempUints = HP[10].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer10 = new uint[HP[10].GetLength(0), HP[10].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer10, 0, tempUints.Length);

    //        tempUints = HP[11].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer11 = new uint[HP[11].GetLength(0), HP[11].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer11, 0, tempUints.Length);

    //        tempUints = HP[12].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer12 = new uint[HP[12].GetLength(0), HP[12].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer12, 0, tempUints.Length);

    //        tempUints = HP[13].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer13= new uint[HP[13].GetLength(0), HP[13].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer13, 0, tempUints.Length);

    //        tempUints = HP[14].Cast<int>().Select(x => (uint)x).ToArray();
    //        layer14 = new uint[HP[14].GetLength(0), HP[14].GetLength(0)];
    //        Buffer.BlockCopy(tempUints, 0, layer14, 0, tempUints.Length);

    //        ulong[] tempUlong = HP[15].Cast<int>().Select(x => (ulong)x).ToArray();
    //        layer15 = new ulong[HP[15].GetLength(0), HP[15].GetLength(0)];
    //        Buffer.BlockCopy(tempUlong, 0, layer15, 0, tempUlong.Length);

    //    }

    //    public uint this[int index, int i, int j]
    //    {
    //        get
    //        {
    //            switch (index)
    //            {
    //                case 0:
    //                    return layer0[i, j];
    //                case 1:
    //                    return layer1[i, j];
    //                case 2:
    //                    return layer2[i, j];
    //                case 3:
    //                    return layer3[i, j];
    //                case 4:
    //                    return layer4[i, j];
    //                case 5:
    //                    return layer5[i, j];
    //                case 6:
    //                    return layer6[i, j];
    //                case 7:
    //                    return layer7[i, j];
    //                case 8:
    //                    return layer8[i, j];
    //                case 9:
    //                    return layer9[i, j];
    //                case 10:
    //                    return layer10[i, j];
    //                case 11:
    //                    return layer11[i, j];
    //                case 12:
    //                    return layer12[i, j];
    //                case 13:
    //                    return layer13[i, j];
    //                case 14:
    //                    return layer14[i, j];
    //                case 15:
    //                    return (uint)layer15[i, j];
    //                default:
    //                    return 0;
    //            }
    //        }
    //    }
    //}

    public struct Triangle
    {
        public Vertex vertex1;
        public Vertex vertex2;
        public Vertex vertex3;

        public Triangle(Point i, Point j, Point k)
        {
            vertex1 = new Vertex(i.X, i.Y, i.Z, i.normal);
            vertex2 = new Vertex(j.X, j.Y, j.Z, j.normal);
            vertex3 = new Vertex(k.X, k.Y, k.Z, k.normal);
        }

        public Vertex[] getV() { return new Vertex[] { vertex1, vertex2, vertex3 }; }
    }

    public struct FlatPoint
    {
        public int X;
        public int Y;

        public FlatPoint(int i, int j)
        {
            this.X = i;
            this.Y = j;
        }
    }

    public struct Point
    {
        //const short epsilon = 0;
        public Normal normal;
        public float X;
        public float Y;
        public float Z;
        public short value;

        public Point(float i, float j, float k, short val, Normal norm)
        {
            X = i;
            Y = j;
            Z = k;
            value = val;
            normal = norm;
        }

        //public Point(double i, double j, float k, short val, short threshold, Normal norm)
        //{
        //    X = i;
        //    Y = j;
        //    Z = k;
        //    if (Math.Abs(threshold - val) < epsilon)
        //        val = threshold;
        //    value = val;
        //    normal = norm;
        //}

        public Point Interpolate(Point other, short threshold)
        {
            float p1 = other.value;
            float p2 = value;
            float x = ((threshold - p2) / (p1 - p2));
            //if (x < 0.001)
            //    x = 0;
            //if (x > 0.999)
            //    x = 1;

            Point vertex = this + (other - this) * x;

            vertex.normal = this.normal.Interpolate(other.normal, x);

            return vertex;
        }

        //public Point FullInterpolate(Point other, short threshold)
        //{
        //    //float p1 = other.value;
        //    //float p2 = value;
        //    //float x = ((threshold - p2) / (p1 - p2));

        //    //float z = (this - other) * threshold + other;
        //    return (this - other) * threshold + other;
        //}

        //public void setNormal(float x, float y, float z)
        //{
        //    if (x > 0)
        //        ;
        //    this.normal.X = x;
        //    this.normal.Y = y;
        //    this.normal.Z = z;
        //}


        //public override string ToString()
        //{
        //    return base.ToString();
        //}

        //public override bool Equals(object obj)
        //{
        //    return ((Point)obj).X == X && ((Point)obj).Y == Y && ((Point)obj).Z == Z;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

        public static Point operator +(Point pt1, Point pt2) => new Point(pt1.X + pt2.X, pt1.Y + pt2.Y, pt1.Z + pt2.Z, 0, new Normal());
        public static Point operator -(Point pt1, Point pt2) => new Point(pt1.X - pt2.X, pt1.Y - pt2.Y, pt1.Z - pt2.Z, 0, new Normal());
        public static Point operator *(Point pt1, float factor) => new Point(pt1.X * factor, pt1.Y * factor, pt1.Z * factor, 0, new Normal());
        //public static bool operator ==(Point pt1, Point pt2) => pt1.X == pt2.X && pt1.Y == pt2.Y && pt1.Z == pt2.Z;
        //public static bool operator !=(Point pt1, Point pt2) => !(pt1.X == pt2.X && pt1.Y == pt2.Y && pt1.Z == pt2.Z);
        //public static bool operator >(Point pt1, Point pt2) => pt1.value > pt2.value;
        //public static bool operator <(Point pt1, Point pt2) => pt1.value < pt2.value;
    }

    public struct Normal
    {
        public float X;
        public float Y;
        public float Z;

        public Normal(float i, float j, float k)
        {
            X = i;
            Y = j;
            Z = k;
        }

        public Normal Interpolate(Normal other, float interpolant)
        {
            float x = (other.X - this.X) * interpolant + this.X;
            float y = (other.Y - this.Y) * interpolant + this.Y;
            float z = (other.Z - this.Z) * interpolant + this.Z;
            return new Normal(x, y, z);
        }
    }

    public struct Vertex
    {
        public Normal normal;
        public float X;
        public float Y;
        public float Z;

        public Vertex(float i, float j, float k, Normal norm)
        {
            X = i;
            Y = j;
            Z = k;
            normal = norm;
        }

        public Vertex Interpolate(Vertex other, float interpolant)
        {
            float x = (other.X - this.X) * interpolant + this.X;
            float y = (other.Y - this.Y) * interpolant + this.Y;
            float z = (other.Z - this.Z) * interpolant + this.Z;
            return new Vertex(x, y, z, new Normal());
        }
    }

    public struct Edge
    {
        public sbyte E1;
        public sbyte E2;
        public sbyte E3;
        public sbyte E4;
        public sbyte E5;
        public sbyte E6;
        public sbyte E7;
        public sbyte E8;
        public sbyte E9;
        public sbyte E10;
        public sbyte E11;
        public sbyte E12;
        public sbyte E13;
        public sbyte E14;
        public sbyte E15;
        public sbyte E16;

        public Edge(sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7, sbyte e8, sbyte e9, sbyte e10, sbyte e11, sbyte e12, sbyte e13, sbyte e14, sbyte e15, sbyte e16)
        {
            E1 = e1;
            E2 = e2;
            E3 = e3;
            E4 = e4;
            E5 = e5;
            E6 = e6;
            E7 = e7;
            E8 = e8;
            E9 = e9;
            E10 = e10;
            E11 = e11;
            E12 = e12;
            E13 = e13;
            E14 = e14;
            E15 = e15;
            E16 = e16;
        }

        public sbyte[] getAsArray()
        {
            return new sbyte[] { E1, E2, E3, E4, E5, E6, E7, E8, E9, E10, E11, E12, E13, E14, E15, E16 };
        }

        public byte getn()
        {
            byte sum = 0;
            foreach (sbyte e in getAsArray())
            {
                if (e >= 0) sum++;
            }
            return sum;
        }
    }

    public struct Cube
    {
        public Point V1, V2, V3, V4, V5, V6, V7, V8;
        public byte config;

        public Cube(Point v1, Point v2, Point v3, Point v4, Point v5, Point v6, Point v7, Point v8)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            V4 = v4;
            V5 = v5;
            V6 = v6;
            V7 = v7;
            V8 = v8;
            config = 0x00;
        }

        public void Config(double threshold)
        {
            config = 0;
            config += (V1.value < threshold) ? (byte)bitMask.v1 : (byte)0;
            config += (V2.value < threshold) ? (byte)bitMask.v2 : (byte)0;
            config += (V3.value < threshold) ? (byte)bitMask.v3 : (byte)0;
            config += (V4.value < threshold) ? (byte)bitMask.v4 : (byte)0;
            config += (V5.value < threshold) ? (byte)bitMask.v5 : (byte)0;
            config += (V6.value < threshold) ? (byte)bitMask.v6 : (byte)0;
            config += (V7.value < threshold) ? (byte)bitMask.v7 : (byte)0;
            config += (V8.value < threshold) ? (byte)bitMask.v8 : (byte)0;
        }

        public Point[] getAsArray()
        {
            return new Point[] { V1, V2, V3, V4, V5, V6, V7, V8 };
        }

        public byte getConfig() { return config; }
        //public Point getMax() { return getAsArray().Max(); }
        //public Point getmin() { return getAsArray().Min(); }

        public Point[] March(short threshold, Edge config)
        {
            sbyte[] ed = config.getAsArray().Where(x => x >= 0).ToArray();
            if (ed.All(x => x == 0))
                ed = new sbyte[] { };
            //if (ed.Length > 0)
            //{

            //}
            Point[] points = new Point[ed.Length];
            if (ed.Length != 0)
            {
                ;
            }
            int i;
            for (i = 0; i < ed.Length; i++)
            {
                switch (ed[i])
                {
                    case (int)edgeMask.e1:
                        points[i] = V1.Interpolate(V2, threshold);
                        //points.Add(new Point3D(i + 0.5f, j, k));
                        break;
                    case (int)edgeMask.e2:
                        points[i] = V2.Interpolate(V3, threshold);
                        //points.Add(new Point3D(i + 1, j + 0.5f, k));
                        break;
                    case (int)edgeMask.e3:
                        points[i] = V4.Interpolate(V3, threshold);
                        //points.Add(new Point3D(i + 0.5f, j + 1, k));
                        break;
                    case (int)edgeMask.e4:
                        points[i] = V1.Interpolate(V4, threshold);
                        //points.Add(new Point3D(i, j + 0.5f, k));
                        break;
                    case (int)edgeMask.e5:
                        points[i] = V5.Interpolate(V6, threshold);
                        //points.Add(new Point3D(i + 0.5f, j, k + 1));
                        break;
                    case (int)edgeMask.e6:
                        points[i] = V6.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 1, j + 0.5f, k + 1));
                        break;
                    case (int)edgeMask.e7:
                        points[i] = V8.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 0.5f, j + 1, k + 1));
                        break;
                    case (int)edgeMask.e8:
                        points[i] = V5.Interpolate(V8, threshold);
                        //points.Add(new Point3D(i, j + 0.5f, k + 1));
                        break;
                    case (int)edgeMask.e9:
                        points[i] = V1.Interpolate(V5, threshold);
                        //points.Add(new Point3D(i, j, k+0.5f));
                        break;
                    case (int)edgeMask.e10:
                        points[i] = V2.Interpolate(V6, threshold);
                        //points.Add(new Point3D(i + 1, j, k+0.5f));
                        break;
                    case (int)edgeMask.e11:
                        points[i] = V3.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 1, j + 1, k+0.5f));
                        break;
                    case (int)edgeMask.e12:
                        points[i] = V4.Interpolate(V8, threshold);
                        //points.Add(new Point3D(i, j + 1, k+0.5f));
                        break;
                }
            }
            return points;
        }

        public Point[] MarchGPU(short threshold, Edge config)
        {
            sbyte[] ed = config.getAsArray();

            //if (ed.All(x => x == 0))
            //    ed = new short[] { };
            //if (ed.Length > 0)
            //{

            //}
            Point[] points = new Point[ed.Length];
            int i;
            for (i = 0; i < ed.Length; i++)
            {
                switch (ed[i])
                {
                    case 0:
                        points[i] = V1.Interpolate(V2, threshold);
                        //points.Add(new Point3D(i + 0.5f, j, k));
                        break;
                    case 1:
                        points[i] = V2.Interpolate(V3, threshold);
                        //points.Add(new Point3D(i + 1, j + 0.5f, k));
                        break;
                    case 2:
                        points[i] = V4.Interpolate(V3, threshold);
                        //points.Add(new Point3D(i + 0.5f, j + 1, k));
                        break;
                    case 3:
                        points[i] = V1.Interpolate(V4, threshold);
                        //points.Add(new Point3D(i, j + 0.5f, k));
                        break;
                    case 4:
                        points[i] = V5.Interpolate(V6, threshold);
                        //points.Add(new Point3D(i + 0.5f, j, k + 1));
                        break;
                    case 5:
                        points[i] = V6.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 1, j + 0.5f, k + 1));
                        break;
                    case 6:
                        points[i] = V8.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 0.5f, j + 1, k + 1));
                        break;
                    case 7:
                        points[i] = V5.Interpolate(V8, threshold);
                        //points.Add(new Point3D(i, j + 0.5f, k + 1));
                        break;
                    case 8:
                        points[i] = V1.Interpolate(V5, threshold);
                        //points.Add(new Point3D(i, j, k+0.5f));
                        break;
                    case 9:
                        points[i] = V2.Interpolate(V6, threshold);
                        //points.Add(new Point3D(i + 1, j, k+0.5f));
                        break;
                    case 10:
                        points[i] = V3.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 1, j + 1, k+0.5f));
                        break;
                    case 11:
                        points[i] = V4.Interpolate(V8, threshold);
                        //points.Add(new Point3D(i, j + 1, k+0.5f));
                        break;
                }
            }
            return points;
        }

        public Triangle MarchHP(short threshold, Edge config, int index)
        {
            sbyte[] ed = config.getAsArray();

            //if (ed.All(x => x == 0))
            //    ed = new short[] { };
            //if (ed.Length > 0)
            //{

            //}
            Point[] points = new Point[3];
            int i;
            for (i = 0; i < 3; i++)
            {
                switch (ed[index * 3 + i])
                {
                    case 0:
                        points[i] = V1.Interpolate(V2, threshold);
                        //points.Add(new Point3D(i + 0.5f, j, k));
                        break;
                    case 1:
                        points[i] = V2.Interpolate(V3, threshold);
                        //points.Add(new Point3D(i + 1, j + 0.5f, k));
                        break;
                    case 2:
                        points[i] = V4.Interpolate(V3, threshold);
                        //points.Add(new Point3D(i + 0.5f, j + 1, k));
                        break;
                    case 3:
                        points[i] = V1.Interpolate(V4, threshold);
                        //points.Add(new Point3D(i, j + 0.5f, k));
                        break;
                    case 4:
                        points[i] = V5.Interpolate(V6, threshold);
                        //points.Add(new Point3D(i + 0.5f, j, k + 1));
                        break;
                    case 5:
                        points[i] = V6.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 1, j + 0.5f, k + 1));
                        break;
                    case 6:
                        points[i] = V8.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 0.5f, j + 1, k + 1));
                        break;
                    case 7:
                        points[i] = V5.Interpolate(V8, threshold);
                        //points.Add(new Point3D(i, j + 0.5f, k + 1));
                        break;
                    case 8:
                        points[i] = V1.Interpolate(V5, threshold);
                        //points.Add(new Point3D(i, j, k+0.5f));
                        break;
                    case 9:
                        points[i] = V2.Interpolate(V6, threshold);
                        //points.Add(new Point3D(i + 1, j, k+0.5f));
                        break;
                    case 10:
                        points[i] = V3.Interpolate(V7, threshold);
                        //points.Add(new Point3D(i + 1, j + 1, k+0.5f));
                        break;
                    case 11:
                        points[i] = V4.Interpolate(V8, threshold);
                        //points.Add(new Point3D(i, j + 1, k+0.5f));
                        break;
                    default:
                        break;
                }
            }
            if ((points[0] - points[1]).X > 1)
                ;
            return new Triangle(points[0], points[1], points[2]);
        }

        //public int getNumber()
        //{
        //    return GetRow(triangleTable, config).Where(x => x >= 0).Count();
        //}

        int[] GetRow(int[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }
        private enum bitMask
        {
            v1 = 0x01,
            v2 = 0x02,
            v3 = 0x04,
            v4 = 0x08,
            v5 = 0x10,
            v6 = 0x20,
            v7 = 0x40,
            v8 = 0x80,
        }

        private enum edgeMask
        {
            e1 = 0,
            e2,
            e3,
            e4,
            e5,
            e6,
            e7,
            e8,
            e9,
            e10,
            e11,
            e12
        }

        //private static byte[] regularCellClass =
        //{
        //	0x00, 0x01, 0x01, 0x03, 0x01, 0x03, 0x02, 0x04, 0x01, 0x02, 0x03, 0x04, 0x03, 0x04, 0x04, 0x03,
        //	0x01, 0x03, 0x02, 0x04, 0x02, 0x04, 0x06, 0x0C, 0x02, 0x05, 0x05, 0x0B, 0x05, 0x0A, 0x07, 0x04,
        //	0x01, 0x02, 0x03, 0x04, 0x02, 0x05, 0x05, 0x0A, 0x02, 0x06, 0x04, 0x0C, 0x05, 0x07, 0x0B, 0x04,
        //	0x03, 0x04, 0x04, 0x03, 0x05, 0x0B, 0x07, 0x04, 0x05, 0x07, 0x0A, 0x04, 0x08, 0x0E, 0x0E, 0x03,
        //	0x01, 0x02, 0x02, 0x05, 0x03, 0x04, 0x05, 0x0B, 0x02, 0x06, 0x05, 0x07, 0x04, 0x0C, 0x0A, 0x04,
        //	0x03, 0x04, 0x05, 0x0A, 0x04, 0x03, 0x07, 0x04, 0x05, 0x07, 0x08, 0x0E, 0x0B, 0x04, 0x0E, 0x03,
        //	0x02, 0x06, 0x05, 0x07, 0x05, 0x07, 0x08, 0x0E, 0x06, 0x09, 0x07, 0x0F, 0x07, 0x0F, 0x0E, 0x0D,
        //	0x04, 0x0C, 0x0B, 0x04, 0x0A, 0x04, 0x0E, 0x03, 0x07, 0x0F, 0x0E, 0x0D, 0x0E, 0x0D, 0x02, 0x01,
        //	0x01, 0x02, 0x02, 0x05, 0x02, 0x05, 0x06, 0x07, 0x03, 0x05, 0x04, 0x0A, 0x04, 0x0B, 0x0C, 0x04,
        //	0x02, 0x05, 0x06, 0x07, 0x06, 0x07, 0x09, 0x0F, 0x05, 0x08, 0x07, 0x0E, 0x07, 0x0E, 0x0F, 0x0D,
        //	0x03, 0x05, 0x04, 0x0B, 0x05, 0x08, 0x07, 0x0E, 0x04, 0x07, 0x03, 0x04, 0x0A, 0x0E, 0x04, 0x03,
        //	0x04, 0x0A, 0x0C, 0x04, 0x07, 0x0E, 0x0F, 0x0D, 0x0B, 0x0E, 0x04, 0x03, 0x0E, 0x02, 0x0D, 0x01,
        //	0x03, 0x05, 0x05, 0x08, 0x04, 0x0A, 0x07, 0x0E, 0x04, 0x07, 0x0B, 0x0E, 0x03, 0x04, 0x04, 0x03,
        //	0x04, 0x0B, 0x07, 0x0E, 0x0C, 0x04, 0x0F, 0x0D, 0x0A, 0x0E, 0x0E, 0x02, 0x04, 0x03, 0x0D, 0x01,
        //	0x04, 0x07, 0x0A, 0x0E, 0x0B, 0x0E, 0x0E, 0x02, 0x0C, 0x0F, 0x04, 0x0D, 0x04, 0x0D, 0x03, 0x01,
        //	0x03, 0x04, 0x04, 0x03, 0x04, 0x03, 0x0D, 0x01, 0x04, 0x0D, 0x03, 0x01, 0x03, 0x01, 0x01, 0x00
        //};

        private static int[] edgeTable =
        {
            0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
            0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
            0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
            0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
            0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
            0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
            0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
            0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
            0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
            0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
            0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
            0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
            0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
            0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
            0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
            0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
            0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
            0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
            0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
            0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
            0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
            0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
            0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
            0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
            0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
            0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
            0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
            0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
            0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
            0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
            0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
            0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
        };

        private static int[,] triangleTable =
        {
            {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
            {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
            {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
            {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
            {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
            {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
            {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
            {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
            {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
            {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
            {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
            {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
            {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
            {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
            {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
            {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
            {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
            {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
            {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
            {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
            {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
            {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
            {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
            {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
            {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
            {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
            {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
            {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
            {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
            {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
            {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
            {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
            {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
            {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
            {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
            {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
            {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
            {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
            {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
            {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
            {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
            {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
            {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
            {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
            {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
            {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
            {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
            {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
            {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
            {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
            {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
            {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
            {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
            {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
            {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
            {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
            {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
            {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
            {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
            {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
            {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
            {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
            {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
            {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
            {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
            {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
            {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
            {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
            {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
            {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
            {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
            {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
            {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
            {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
            {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
            {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
            {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
            {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
            {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
            {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
            {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
            {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
            {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
            {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
            {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
            {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
            {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
            {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
            {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
            {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
            {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
            {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
            {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
            {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
            {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
            {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
            {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
            {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
            {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
            {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
            {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
            {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
            {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
            {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
            {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
            {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
            {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
            {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
            {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
            {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
            {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
            {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
            {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
            {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
            {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
            {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
            {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
            {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
            {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
            {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
            {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
            {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
            {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
            {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
            {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
            {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
            {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
            {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
            {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
            {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
            {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
            {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
            {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
            {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
            {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
            {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
            {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
            {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
            {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
            {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
            {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
            {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
            {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
            {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
            {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
            {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
            {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
            {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
            {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
            {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
            {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
            {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
            {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
            {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
            {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
            {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
            {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
            {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
            {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
            {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
            {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
            {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
            {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
            {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
            {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
            {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
            {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
            {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
            {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
            {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
            {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
            {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
            {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
            {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
            {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
            {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
            {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
            {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
            {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
            {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
        };
    }
}