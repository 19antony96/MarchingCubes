using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using System.Windows;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime;
using ILGPU;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MarchingCubes
{
    class HistoPyramid3D : MarchingCubes
    {
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int, int> assign1D;
        public static Action<Index3D, ArrayView3D<uint, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> hpFirstLayer;
        public static Action<Index3D, ArrayView3D<uint, Stride3D.DenseXY>, ArrayView3D<uint, Stride3D.DenseXY>> hpCreation;
        public static Action<Index1D, ArrayView1D<FlatPoint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView3D<uint, Stride3D.DenseXY>> traversalKernel;
        public static Action<Index1D,
            ArrayView1D<FlatPoint, Stride1D.Dense>,
            ArrayView1D<uint, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Triangle, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Edge, Stride1D.Dense>,
            ArrayView3D<ushort, Stride3D.DenseXY>,
            int, int> hpFinalLayer;


        public static MemoryBuffer3D<byte, Stride3D.DenseXY> cubeConfig;
        private static MemoryBuffer3D<byte, Stride3D.DenseXY> HPBaseConfig;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer15;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer14;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer13;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer12;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer11;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer10;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer9;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer8;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer7;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer6;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer5;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer4;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer3;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer2;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> uintLayer1;

        public static byte[,,] cubeBytes;
        public static byte[,,] HPBaseLayer;
        public static uint[][,,] HP;
        public static int HPsize;

        public static TimeSpan ts = new TimeSpan();

        public static List<Point> vertices = new List<Point>();
        public static byte[,,] cubes;
        public static int count = 0;

        public HistoPyramid3D(int size)
        {
            ushort i = 0;
            FileInfo fi = CreateVolume(size);
            HPsize = width;
            if (Math.Log(width, 2) % 1 > 0)
            {
                HPsize = (int)Math.Pow(2, (int)Math.Log(width, 2) + 1);
            }
            //var s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
            //var p = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Point));
            //var n = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal));

            assign1D = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int, int>(Assign1D);
            hpFirstLayer = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<uint, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BuildHPFirst);
            hpCreation = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<uint, Stride3D.DenseXY>, ArrayView3D<uint, Stride3D.DenseXY>>(BuildHP);
            traversalKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView1D<FlatPoint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView3D<uint, Stride3D.DenseXY>>(HPTraverseKernel);
            hpFinalLayer = accelerator.LoadAutoGroupedStreamKernel<Index1D,
            ArrayView1D<FlatPoint, Stride1D.Dense>,
            ArrayView1D<uint, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Triangle, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Edge, Stride1D.Dense>,
            ArrayView3D<ushort, Stride3D.DenseXY>,
            int, int>(HPFinalLayer);

            cubes = MarchingCubesGPU();

            HPCreationGPU();

            using (StreamWriter fs = fi.CreateText())
            {
                HPTraversalGPU(fs);

                int f = 0;
                for (f = 1; f < count - 1; f += 3)
                {
                    fs.WriteLine("f " + f + " " + (f + 1) + " " + (f + 2));
                }
                Console.WriteLine(count);
            }

            for (i = 1; i < nLayers; i++)
            {
                if (getHPLayer(i) != null && !getHPLayer(i).IsDisposed)
                    getHPLayer(i).Dispose();
            }
        }

        public static void BuildHP(Index3D index, ArrayView3D<uint, Stride3D.DenseXY> HPLayer, ArrayView3D<uint, Stride3D.DenseXY> HPLayerPrev)
        {
            HPLayer[index] = HPLayerPrev[index.X * 2, index.Y * 2, index.Z * 2] + HPLayerPrev[index.X * 2 + 1, index.Y * 2, index.Z * 2] + HPLayerPrev[index.X * 2 + 1, index.Y * 2 + 1, index.Z * 2] + HPLayerPrev[index.X * 2, index.Y * 2 + 1, index.Z * 2] +
                HPLayerPrev[index.X * 2, index.Y * 2, index.Z * 2 + 1] + HPLayerPrev[index.X * 2 + 1, index.Y * 2, index.Z * 2 + 1] + HPLayerPrev[index.X * 2 + 1, index.Y * 2 + 1, index.Z * 2 + 1] + HPLayerPrev[index.X * 2, index.Y * 2 + 1, index.Z * 2 + 1];
        }


        public static void BuildHPFirst(Index3D index, ArrayView3D<uint, Stride3D.DenseXY> HPLayer, ArrayView3D<byte, Stride3D.DenseXY> HPLayerBase)
        {
            HPLayer[index] = (uint)(HPLayerBase[index.X * 2, index.Y * 2, index.Z * 2] + HPLayerBase[index.X * 2 + 1, index.Y * 2, index.Z * 2] + HPLayerBase[index.X * 2 + 1, index.Y * 2 + 1, index.Z * 2] + HPLayerBase[index.X * 2, index.Y * 2 + 1, index.Z * 2] +
                HPLayerBase[index.X * 2, index.Y * 2, index.Z * 2 + 1] + HPLayerBase[index.X * 2 + 1, index.Y * 2, index.Z * 2 + 1] + HPLayerBase[index.X * 2 + 1, index.Y * 2 + 1, index.Z * 2 + 1] + HPLayerBase[index.X * 2, index.Y * 2 + 1, index.Z * 2 + 1]);
        }

        private static void HPCreationGPU()
        {
            nLayers = (ushort)(Math.Ceiling(Math.Log(HPBaseLayer.GetLength(0), 2)) + 1);
            HP = new uint[10][,,];
            HP[0] = new uint[HPBaseLayer.GetLength(0), HPBaseLayer.GetLength(0), HPBaseLayer.GetLength(0)];
            Array.Copy(HPBaseLayer, HP[0], HPBaseLayer.Length);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            getHPLayer(1) = accelerator.Allocate3DDenseXY<uint>(new Index3D((int)HPBaseConfig.Extent.X / 2));
            hpFirstLayer(getHPLayer(1).IntExtent, getHPLayer(1).View, HPBaseConfig.View);
            accelerator.Synchronize();
            for (int i = 2; i < 16; i++)
            {
                int l = Math.Max(HP[0].GetLength(0) / (int)Math.Pow(2, i), 1);
                if (i < nLayers)
                {
                    Index3D index = new Index3D(l);
                    getHPLayer(i) = accelerator.Allocate3DDenseXY<uint>(index);

                    hpCreation(index, getHPLayer(i).View, getHPLayer(i - 1).View);
                    accelerator.Synchronize();
                }
            }


            stopWatch.Stop();
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            uint[,,] data = getHPLayer(nLayers - 1).GetAsArray3D();
            if (data.Length == 1)
                nTri = (int)data[0, 0, 0];
        }

        private static uint[][,,] HPCreation()
        {
            nLayers = (ushort)(Math.Ceiling(Math.Log(HPBaseLayer.GetLength(0), 2)) + 1);
            uint[][,,] HP = new uint[11][,,];
            HP[0] = new uint[HPBaseLayer.GetLength(0), HPBaseLayer.GetLength(0), HPBaseLayer.GetLength(0)];
            Array.Copy(HPBaseLayer, HP[0], HPBaseLayer.Length);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 1; i < 11; i++)
            {
                if (i < nLayers)
                {
                    int l = Math.Max(HP[i - 1].GetLength(0) / 2, 1);
                    HP[i] = new uint[l, l, l];
                    for (int iHP = 0; iHP < l; iHP++)
                    {
                        for (int jHP = 0; jHP < l; jHP++)
                        {
                            for (int kHP = 0; kHP < l; kHP++)
                                HP[i][iHP, jHP, kHP] = HP[i - 1][iHP * 2, jHP * 2, kHP * 2] + HP[i - 1][iHP * 2 + 1, jHP * 2, kHP * 2] + HP[i - 1][iHP * 2 + 1, jHP * 2 + 1, kHP * 2] + HP[i - 1][iHP * 2, jHP * 2 + 1, kHP * 2] +
                                    HP[i - 1][iHP * 2, jHP * 2, kHP * 2 + 1] + HP[i - 1][iHP * 2 + 1, jHP * 2, kHP * 2 + 1] + HP[i - 1][iHP * 2 + 1, jHP * 2 + 1, kHP * 2 + 1] + HP[i - 1][iHP * 2, jHP * 2 + 1, kHP * 2 + 1];
                        }
                    }
                }
                else
                    HP[i] = new uint[,,] { { { 0 } } };
            }
            nTri = (int)HP[nLayers - 1][0, 0, 0];


            stopWatch.Stop();
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            return HP;
        }

        public static void HPTraverseKernel(Index1D index, ArrayView1D<FlatPoint, Stride1D.Dense> p, ArrayView1D<uint, Stride1D.Dense> k, ArrayView3D<uint, Stride3D.DenseXY> HPLayer)
        {
            uint a = HPLayer[p[index].X, p[index].Y, p[index].Z];
            uint b = HPLayer[p[index].X + 1, p[index].Y, p[index].Z] + a;
            uint c = HPLayer[p[index].X, p[index].Y + 1, p[index].Z] + b;
            uint d = HPLayer[p[index].X + 1, p[index].Y + 1, p[index].Z] + c;
            uint e = HPLayer[p[index].X, p[index].Y, p[index].Z + 1] + d;
            uint f = HPLayer[p[index].X + 1, p[index].Y, p[index].Z + 1] + e;
            uint g = HPLayer[p[index].X, p[index].Y + 1, p[index].Z + 1] + f;
            uint h = HPLayer[p[index].X + 1, p[index].Y + 1, p[index].Z + 1] + g;
            if (d == 0)
                ;
            if (k[index] < a)
                ;
            else if (k[index] < b)
            {
                if (k[index] == a)
                    k[index] = 0;
                else
                    k[index] = k[index] - a;
                p[index].X++;
            }
            else if (k[index] < c)
            {
                if (k[index] == b)
                    k[index] = 0;
                else
                    k[index] = k[index] - b;
                p[index].Y++;
            }
            else if (k[index] < d)
            {
                if (k[index] == c)
                    k[index] = 0;
                else
                    k[index] = k[index] - c;
                p[index].X++;
                p[index].Y++;
            }
            else if (k[index] < e)
            {
                if (k[index] == d)
                    k[index] = 0;
                else
                    k[index] = k[index] - d;
                p[index].Z++;
            }
            else if (k[index] < f)
            {
                if (k[index] == e)
                    k[index] = 0;
                else
                    k[index] = k[index] - e;
                p[index].X++;
                p[index].Z++;
            }
            else if (k[index] < g)
            {
                if (k[index] == f)
                    k[index] = 0;
                else
                    k[index] = k[index] - f;
                p[index].Y++;
                p[index].Z++;
            }
            else if (k[index] < h)
            {
                if (k[index] == g)
                    k[index] = 0;
                else
                    k[index] = k[index] - g;
                p[index].X++;
                p[index].Y++;
                p[index].Z++;
            }
            p[index].X *= 2;
            p[index].Y *= 2;
            p[index].Z *= 2;
        }

        public static void HPFinalLayer(Index1D index,
            ArrayView1D<FlatPoint, Stride1D.Dense> p,
            ArrayView1D<uint, Stride1D.Dense> k,
            ArrayView3D<byte, Stride3D.DenseXY> HPLayer,
            ArrayView1D<Triangle, Stride1D.Dense> triangles,
            ArrayView3D<byte, Stride3D.DenseXY> edges,
            ArrayView1D<Edge, Stride1D.Dense> triTable,
            ArrayView3D<ushort, Stride3D.DenseXY> input,
            int thresh, int HPSqrt)
        {
            uint a = HPLayer[p[index].X, p[index].Y, p[index].Z];
            uint b = HPLayer[p[index].X + 1, p[index].Y, p[index].Z] + a;
            uint c = HPLayer[p[index].X, p[index].Y + 1, p[index].Z] + b;
            uint d = HPLayer[p[index].X + 1, p[index].Y + 1, p[index].Z] + c;
            uint e = HPLayer[p[index].X, p[index].Y, p[index].Z + 1] + d;
            uint f = HPLayer[p[index].X + 1, p[index].Y, p[index].Z + 1] + e;
            uint g = HPLayer[p[index].X, p[index].Y + 1, p[index].Z + 1] + f;
            uint h = HPLayer[p[index].X + 1, p[index].Y + 1, p[index].Z + 1] + g;
            if (k[index] < b)
            {
                if (k[index] == a)
                    k[index] = 0;
                else
                    k[index] = k[index] - a;
                p[index].X++;
            }
            else if (k[index] < c)
            {
                if (k[index] == b)
                    k[index] = 0;
                else
                    k[index] = k[index] - b;
                p[index].Y++;
            }
            else if (k[index] < d)
            {
                if (k[index] == c)
                    k[index] = 0;
                else
                    k[index] = k[index] - c;
                p[index].X++;
                p[index].Y++;
            }
            else if (k[index] < e)
            {
                if (k[index] == d)
                    k[index] = 0;
                else
                    k[index] = k[index] - d;
                p[index].Z++;
            }
            else if (k[index] < f)
            {
                if (k[index] == e)
                    k[index] = 0;
                else
                    k[index] = k[index] - e;
                p[index].X++;
                p[index].Z++;
            }
            else if (k[index] < g)
            {
                if (k[index] == f)
                    k[index] = 0;
                else
                    k[index] = k[index] - f;
                p[index].Y++;
                p[index].Z++;
            }
            else if (k[index] < h)
            {
                if (k[index] == g)
                    k[index] = 0;
                else
                    k[index] = k[index] - g;
                p[index].X++;
                p[index].Y++;
                p[index].Z++;
            }

            Index3D index3D = new Index3D(p[index].X, p[index].Y, p[index].Z);
            Cube tempCube = new Cube(
                           new Point(index3D.X, index3D.Y, index3D.Z, input[index3D.Z, index3D.Y, index3D.X],
                           new Normal(
                                input[index3D.Z, index3D.Y, Math.Min((input.Extent.Z) - 1, index3D.X + 1)] - input[index3D.Z, index3D.Y, Math.Max(index3D.X - 1, 0)],
                                input[index3D.Z, Math.Min((input.Extent.Y) - 1, index3D.Y + 1), index3D.X] - input[index3D.Z, Math.Max(index3D.Y - 1, 0), index3D.X],
                                input[Math.Min((int)input.Extent.X - 1, index3D.Z + 1), index3D.Y, index3D.X] - input[Math.Max(index3D.Z - 1, 0), index3D.Y, index3D.X]
                            )),
                           new Point((ushort)(index3D.X + 1), index3D.Y, index3D.Z, input[index3D.Z, index3D.Y, index3D.X + 1],
                           new Normal(
                                input[index3D.Z, index3D.Y, Math.Min((input.Extent.Z) - 1, (index3D.X + 1) + 1)] - input[index3D.Z, index3D.Y, Math.Max((index3D.X + 1) - 1, 0)],
                                input[index3D.Z, Math.Min((input.Extent.Y) - 1, index3D.Y + 1), (index3D.X + 1)] - input[index3D.Z, Math.Max(index3D.Y - 1, 0), (index3D.X + 1)],
                                input[Math.Min((int)input.Extent.X - 1, index3D.Z + 1), index3D.Y, (index3D.X + 1)] - input[Math.Max(index3D.Z - 1, 0), index3D.Y, (index3D.X + 1)]
                            )),
                           new Point((ushort)(index3D.X + 1), (ushort)(index3D.Y + 1), index3D.Z, input[index3D.Z, index3D.Y + 1, index3D.X + 1],
                           new Normal(
                                input[index3D.Z, (index3D.Y + 1), Math.Min((input.Extent.Z) - 1, (index3D.X + 1) + 1)] - input[index3D.Z, (index3D.Y + 1), Math.Max((index3D.X + 1) - 1, 0)],
                                input[index3D.Z, Math.Min((input.Extent.Y) - 1, (index3D.Y + 1) + 1), (index3D.X + 1)] - input[index3D.Z, Math.Max((index3D.Y + 1) - 1, 0), (index3D.X + 1)],
                                input[Math.Min((int)input.Extent.X - 1, index3D.Z + 1), (index3D.Y + 1), (index3D.X + 1)] - input[Math.Max(index3D.Z - 1, 0), (index3D.Y + 1), (index3D.X + 1)]
                            )),
                           new Point(index3D.X, (ushort)(index3D.Y + 1), index3D.Z, input[index3D.Z, index3D.Y + 1, index3D.X],
                           new Normal(
                                input[index3D.Z, (index3D.Y + 1), Math.Min((input.Extent.Z) - 1, index3D.X + 1)] - input[index3D.Z, (index3D.Y + 1), Math.Max(index3D.X - 1, 0)],
                                input[index3D.Z, Math.Min((input.Extent.Y) - 1, (index3D.Y + 1) + 1), index3D.X] - input[index3D.Z, Math.Max((index3D.Y + 1) - 1, 0), index3D.X],
                                input[Math.Min((int)input.Extent.X - 1, index3D.Z + 1), (index3D.Y + 1), index3D.X] - input[Math.Max(index3D.Z - 1, 0), (index3D.Y + 1), index3D.X]
                            )),
                           new Point(index3D.X, index3D.Y, (index3D.Z + 1), input[(index3D.Z + 1), index3D.Y, index3D.X],
                           new Normal(
                                input[(index3D.Z + 1), index3D.Y, Math.Min((input.Extent.Z) - 1, index3D.X + 1)] - input[(index3D.Z + 1), index3D.Y, Math.Max(index3D.X - 1, 0)],
                                input[(index3D.Z + 1), Math.Min((input.Extent.Y) - 1, index3D.Y + 1), index3D.X] - input[(index3D.Z + 1), Math.Max(index3D.Y - 1, 0), index3D.X],
                                input[Math.Min((int)input.Extent.X - 1, (index3D.Z + 1) + 1), index3D.Y, index3D.X] - input[Math.Max((index3D.Z + 1) - 1, 0), index3D.Y, index3D.X]
                            )),
                           new Point((ushort)(index3D.X + 1), index3D.Y, (index3D.Z + 1), input[(index3D.Z + 1), index3D.Y, index3D.X + 1],
                           new Normal(
                                input[(index3D.Z + 1), index3D.Y, Math.Min((input.Extent.Z) - 1, (index3D.X + 1) + 1)] - input[(index3D.Z + 1), index3D.Y, Math.Max((index3D.X + 1) - 1, 0)],
                                input[(index3D.Z + 1), Math.Min((input.Extent.Y) - 1, index3D.Y + 1), (index3D.X + 1)] - input[(index3D.Z + 1), Math.Max(index3D.Y - 1, 0), (index3D.X + 1)],
                                input[Math.Min((int)input.Extent.X - 1, (index3D.Z + 1) + 1), index3D.Y, (index3D.X + 1)] - input[Math.Max((index3D.Z + 1) - 1, 0), index3D.Y, (index3D.X + 1)]
                            )),
                           new Point((ushort)(index3D.X + 1), (ushort)(index3D.Y + 1), (index3D.Z + 1), input[(index3D.Z + 1), index3D.Y + 1, index3D.X + 1],
                           new Normal(
                                input[(index3D.Z + 1), (index3D.Y + 1), Math.Min((input.Extent.Z) - 1, (index3D.X + 1) + 1)] - input[(index3D.Z + 1), (index3D.Y + 1), Math.Max((index3D.X + 1) - 1, 0)],
                                input[(index3D.Z + 1), Math.Min((input.Extent.Y) - 1, (index3D.Y + 1) + 1), (index3D.X + 1)] - input[(index3D.Z + 1), Math.Max((index3D.Y + 1) - 1, 0), (index3D.X + 1)],
                                input[Math.Min((int)input.Extent.X - 1, (index3D.Z + 1) + 1), (index3D.Y + 1), (index3D.X + 1)] - input[Math.Max((index3D.Z + 1) - 1, 0), (index3D.Y + 1), (index3D.X + 1)]
                            )),
                           new Point(index3D.X, (ushort)(index3D.Y + 1), (index3D.Z + 1), input[(index3D.Z + 1), index3D.Y + 1, index3D.X],
                           new Normal(
                                input[(index3D.Z + 1), (index3D.Y + 1), Math.Min((input.Extent.Z) - 1, index3D.X + 1)] - input[(index3D.Z + 1), (index3D.Y + 1), Math.Max(index3D.X - 1, 0)],
                                input[(index3D.Z + 1), Math.Min((input.Extent.Y) - 1, (index3D.Y + 1) + 1), index3D.X] - input[(index3D.Z + 1), Math.Max((index3D.Y + 1) - 1, 0), index3D.X],
                                input[Math.Min((int)input.Extent.X - 1, (index3D.Z + 1) + 1), (index3D.Y + 1), index3D.X] - input[Math.Max((index3D.Z + 1) - 1, 0), (index3D.Y + 1), index3D.X]
                            ))
                           );
            triangles[index] = tempCube.MarchHP(threshold, triTable[edges[index3D.Z, index3D.Y, index3D.X]], (int)k[index]);
        }

        private static void HPTraversalGPU(StreamWriter fs)
        {
            int n;

            Index1D index = new Index1D(nTri);
            uint[] karray = Enumerable.Range(0, nTri).Select(x => (uint)x).ToArray();
            MemoryBuffer1D<uint, Stride1D.Dense> k = accelerator.Allocate1D<uint>(karray);
            MemoryBuffer1D<FlatPoint, Stride1D.Dense> p = accelerator.Allocate1D<FlatPoint>(nTri);
            int sum = 0;
            Triangle[] tri = new Triangle[nTri];
            PageLockedArray1D<Triangle> triLocked = accelerator.AllocatePageLocked1D<Triangle>(nTri);
            MemoryBuffer1D<Triangle, Stride1D.Dense> triConfig = accelerator.Allocate1D<Triangle>(nTri);
            p.MemSetToZero();
            accelerator.Synchronize();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (n = nLayers - 2; n > 0; n--)
            {
                traversalKernel(index, p.View, k.View, getHPLayer(n).View);
                accelerator.Synchronize();
            }

            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            count = 0;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime:" + elapsedTime + ", Batch Size:" + batchSize);
            stopWatch.Reset();
            stopWatch.Start();

            hpFinalLayer(index, p.View, k.View, HPBaseConfig.View, triConfig.View, cubeConfig.View, triTable.View, sliced.View, threshold, (int)Math.Sqrt(HPsize));

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            count = 0;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime:" + elapsedTime + ", Batch Size:" + batchSize);


            triConfig.View.CopyToPageLockedAsync(triLocked);
            accelerator.Synchronize();
            tri = triLocked.GetArray();
            triConfig.Dispose();
            cubeConfig.Dispose();
            sliced.Dispose();
            k.Dispose();
            p.Dispose();
            triTable.Dispose();
            HPBaseConfig.Dispose();

            foreach (var triangle in tri)
            {
                fs.WriteLine("v " + triangle.vertex1.X + " " + triangle.vertex1.Y + " " + triangle.vertex1.Z);
                fs.WriteLine("vn " + triangle.vertex1.normal.X + " " + triangle.vertex1.normal.Y + " " + triangle.vertex1.normal.Z);
                fs.WriteLine("v " + triangle.vertex2.X + " " + triangle.vertex2.Y + " " + triangle.vertex2.Z);
                fs.WriteLine("vn " + triangle.vertex2.normal.X + " " + triangle.vertex2.normal.Y + " " + triangle.vertex2.normal.Z);
                fs.WriteLine("v " + triangle.vertex3.X + " " + triangle.vertex3.Y + " " + triangle.vertex3.Z);
                fs.WriteLine("vn " + triangle.vertex3.normal.X + " " + triangle.vertex3.normal.Y + " " + triangle.vertex3.normal.Z);
                count += 3;
            }
        }

        public static void Assign1D(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> edges, ArrayView3D<byte, Stride3D.DenseXY> HPindices, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView<Edge> triTable, int thresh, int width, int HPsizeFator)
        {
            byte config = 0;
            config += (input[(index.Z), (index.Y), (index.X)] < thresh) ? (byte)0x01 : (byte)0;
            config += (input[(index.Z), (index.Y), (index.X) + 1] < thresh) ? (byte)0x02 : (byte)0;
            config += (input[(index.Z), (index.Y) + 1, (index.X) + 1] < thresh) ? (byte)0x04 : (byte)0;
            config += (input[(index.Z), (index.Y) + 1, (index.X)] < thresh) ? (byte)0x08 : (byte)0;
            config += (input[(index.Z) + 1, (index.Y), (index.X)] < thresh) ? (byte)0x10 : (byte)0;
            config += (input[(index.Z) + 1, (index.Y), (index.X) + 1] < thresh) ? (byte)0x20 : (byte)0;
            config += (input[(index.Z) + 1, (index.Y) + 1, (index.X) + 1] < thresh) ? (byte)0x40 : (byte)0;
            config += (input[(index.Z) + 1, (index.Y) + 1, (index.X)] < thresh) ? (byte)0x80 : (byte)0;
            edges[index.Z, index.Y, (index.X)] = config;
            if (config > 0 && config < 255)
                HPindices[index.X, index.Y, index.Z] = (byte)(triTable[(int)edges[index.Z, index.Y, (index.X)]].getn());
        }

        public static byte[,,] MarchingCubesGPU()
        {
            Index3D index = new Index3D(slices.GetLength(2) - 1, slices.GetLength(1) - 1, slices.GetLength(0) - 1);
            List<byte> edgeList = new List<byte>();

            //bit order 
            // i,j,k 
            // i+1,j,k
            // i+1,j+1,k
            // i,j+1,k
            // i,j,k+1
            // i+1,j,k+1
            // i+1,j+1,k+1
            // i,j+1,k+1

            HPBaseLayer = new byte[HPsize, HPsize, HPsize];

            cubeBytes = new byte[(index.Z), (index.Y), (index.X)];
            var cubePinned = GCHandle.Alloc(cubeBytes, GCHandleType.Pinned);
            var HPPinned = GCHandle.Alloc(HPBaseLayer, GCHandleType.Pinned);
            PageLockedArray3D<byte> cubeLocked = accelerator.AllocatePageLocked3D<byte>(new Index3D(index.Z, index.Y, index.X));
            PageLockedArray3D<byte> HPLocked = accelerator.AllocatePageLocked3D<byte>(new Index3D(HPBaseLayer.GetLength(0)));
            cubeConfig = accelerator.Allocate3DDenseXY<byte>(cubeLocked.Extent);
            HPBaseConfig = accelerator.Allocate3DDenseXY<byte>(HPLocked.Extent);
            var cubeScope = accelerator.CreatePageLockFromPinned<byte>(cubePinned.AddrOfPinnedObject(), cubeBytes.Length);
            var HPScope = accelerator.CreatePageLockFromPinned<byte>(HPPinned.AddrOfPinnedObject(), HPBaseLayer.Length);
            cubeConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);
            HPBaseConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, HPScope);
            HPBaseConfig.MemSetToZero();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            assign1D(index, cubeConfig.View, HPBaseConfig.View, sliced.View, triTable.View, threshold, width, (int)Math.Sqrt(HPsize));

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            cubeConfig.CopyToCPU(cubeBytes);
            HPBaseConfig.CopyToCPU(HPBaseLayer);
            cubePinned.Free();
            HPPinned.Free();

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            return (cubeBytes);
        }


        public static ref MemoryBuffer3D<uint, Stride3D.DenseXY> getHPLayer(int index)
        {
            switch (index)
            {
                case 1:
                    return ref uintLayer1;
                case 2:
                    return ref uintLayer2;
                case 3:
                    return ref uintLayer3;
                case 4:
                    return ref uintLayer4;
                case 5:
                    return ref uintLayer5;
                case 6:
                    return ref uintLayer6;
                case 7:
                    return ref uintLayer7;
                case 8:
                    return ref uintLayer8;
                case 9:
                    return ref uintLayer9;
                case 10:
                    return ref uintLayer10;
                case 11:
                    return ref uintLayer11;
                case 12:
                    return ref uintLayer12;
                case 13:
                    return ref uintLayer13;
                case 14:
                    return ref uintLayer14;
                case 15:
                    return ref uintLayer15;
                default:
                    return ref uintLayer15;
            }
        }
    }
}