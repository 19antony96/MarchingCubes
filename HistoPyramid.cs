﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Collections;
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using System.ComponentModel;
using System.Threading.Tasks;
using Dicom.Imaging.Mathematics;
using System.Windows;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime;
using ILGPU;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace MarchingCubes
{
    class HistoPyramid
    {
        public static Context context;
        public static CudaAccelerator accelerator;
        //public static CPUAccelerator accelerator;
        //public static Action<Index1D, HistoPyramid> testHP;
        public static Action<Index3D, ArrayView3D<Edge, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int> assign_edges;
        public static Action<Index3D, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> assign_normal;
        public static Action<Index3D, ArrayView3D<Edge, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int> assign_edges1D;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView2D<byte, Stride2D.DenseX>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int, int> assign1D;
        public static Action<Index3D, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> assign_normal1D;
        public static Action<Index3D, ArrayView<Triangle>, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<Edge, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<byte, Stride1D.Dense>, Point, int, int, int> get_verts;
        public static Action<Index3D, ArrayView<Triangle>, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<Edge, Stride3D.DenseXY>, ArrayView1D<ushort, Stride1D.Dense>, Point, int, int, int> get_vertsX;
        public static Action<Index2D, ArrayView2D<uint, Stride2D.DenseX>, ArrayView2D<byte, Stride2D.DenseX>> hpFirstLayer;
        public static Action<Index2D, ArrayView2D<uint, Stride2D.DenseX>, ArrayView2D<uint, Stride2D.DenseX>> hpCreation;
        public static Action<Index1D, ArrayView1D<FlatPoint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView2D<uint, Stride2D.DenseX>> traversalKernel;
        public static Action<Index1D,
            ArrayView1D<FlatPoint, Stride1D.Dense>,
            ArrayView1D<uint, Stride1D.Dense>,
            ArrayView2D<byte, Stride2D.DenseX>,
            ArrayView1D<Triangle, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Edge, Stride1D.Dense>,
            ArrayView3D<ushort, Stride3D.DenseXY>,
            int, int> hpFinalLayer;


        public static MemoryBuffer3D<byte, Stride3D.DenseXY> cubeConfig;
        private static MemoryBuffer2D<byte, Stride2D.DenseX> HPBaseConfig;
        public static MemoryBuffer3D<Normal, Stride3D.DenseXY> gradConfig;
        public static MemoryBuffer1D<Edge, Stride1D.Dense> triTable;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> sliced;
        public static MemoryBuffer2D<byte, Stride2D.DenseX> byteLayer;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer15;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer14;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer13;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer12;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer11;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer10;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer9;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer8;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer7;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer6;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer5;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer4;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer3;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer2;
        public static MemoryBuffer2D<uint, Stride2D.DenseX> uintLayer1;
        public static MemoryBuffer2D<ulong, Stride2D.DenseX> ulongLayer;
        //public HistoPyramid HPGPU;

        public static readonly ushort threshold = 1280;
        public static int length = 512;
        public static int width = 512;
        public static ushort[,,] slices;
        public static ushort[] slices1D;
        public static byte[,,] cubeBytes;
        public static int batchSize;
        public static int sliceSize = 127;
        public static byte[,] HPBaseLayer;
        public static uint[][,] HP;
        public static int HPsize;
        public static ushort nLayers;
        public static int nTri;

        public static TimeSpan ts = new TimeSpan();
        public static int xCount = 0, yCount = 0, zCount = 0, lCount = 0;

        public static Normal[,,] normals;
        public static List<Point> vertices = new List<Point>();
        public static byte[,,] cubes;
        public static Edge[] edges;
        public static int count = 0;

        public HistoPyramid(int size)
        {
            //length = size;
            //width = size;
            //var sphere = CreateSphere(size);
            //slices = sphere;

            HPsize = width;
            if (Math.Log(width, 4) % 1 > 0)
            {
                HPsize = (int)Math.Pow(4, (int)Math.Log(width - 1, 4) + 1);
            }
            var s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
            var p = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Point));
            var n = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal));
            //width = size;
            //length = size;
            context = Context.CreateDefault();
            accelerator = context.CreateCudaAccelerator(0);
            //accelerator = context.CreateCPUAccelerator(0);
            triTable = accelerator.Allocate1D<Edge>(triangleTable);

            assign1D = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView2D<byte, Stride2D.DenseX>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int, int>(Assign1D);
            get_verts = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView<Triangle>, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<Edge, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<byte, Stride1D.Dense>, Point, int, int, int>(getVertices);
            hpFirstLayer = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<uint, Stride2D.DenseX>, ArrayView2D<byte, Stride2D.DenseX>>(BuildHPFirst);
            hpCreation = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<uint, Stride2D.DenseX>, ArrayView2D<uint, Stride2D.DenseX>>(BuildHP);
            traversalKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView1D<FlatPoint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView2D<uint, Stride2D.DenseX>>(HPTraverseKernel);
            hpFinalLayer = accelerator.LoadAutoGroupedStreamKernel<Index1D,
            ArrayView1D<FlatPoint, Stride1D.Dense>,
            ArrayView1D<uint, Stride1D.Dense>,
            ArrayView2D<byte, Stride2D.DenseX>,
            ArrayView1D<Triangle, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Edge, Stride1D.Dense>,
            ArrayView3D<ushort, Stride3D.DenseXY>,
            int, int>(HPFinalLayer);
            //testHP = accelerator.LoadAutoGroupedStreamKernel<Index1D, HistoPyramid>(TestHP);

            //get_vertsX = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView<Triangle>, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<Edge, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, Point, int, int, int>(getVerticesX);

            DicomFile dicoms;
            DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\Subject (1)\\98.12.2\\");
            //DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\w3568970\\batch3\\");
            //DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\DICOM\\DICOM\\ST000000\\SE000001\\");
            //DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\Resources\\");

            //DicomFile p = DicomFile.Open("C:\\Users\\antonyDev\\Downloads\\w3568970\\batch3\\view0296.dcm");
            //DicomPixelData pixelData = DicomPixelData.Create(p.Dataset);


            FileInfo[] files = d.GetFiles("*.dcm");



            //int[,,] edges;


            string fileName = @"C:\\Users\\antonyDev\\Desktop\\timetest3.obj";
            FileInfo fi = new FileInfo(fileName);

            ushort i, j, k = 0;


            slices = new ushort[files.Length, length, width];

            foreach (var file in files)
            //foreach (var file in sphere)
            {
                dicoms = DicomFile.Open(file.FullName);
                CreateBmp(dicoms, k);
                //slices[k] = file;
                k++;
                //if (k * length * width > Math.Pow(2, 32))
                //    break;
                //if (k > 13)
                //    break;
                //Console.WriteLine(k);
            }

            slices1D = new ushort[slices.Length];

            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            sliced = accelerator.Allocate3DDenseXY<ushort>(slices);
            sliced.View.To1DView().CopyToCPU(slices1D);
            //var temp = MarchingCubesCPU();
            cubes = MarchingCubesGPU();
            //int sum = 0;
            //foreach(Edge cube in cubes)
            //{
            //    foreach(ushort n in cube.getAsArray())
            //    {
            //        if (n > 0)
            //        {
            //            sum = cubes.ToList().IndexOf(cube);
            //            break;
            //        }
            //    }
            //    if (sum > 0)
            //        break;
            //}
            ////Console.WriteLine(sum);
            //cubes = temp.configs;
            //normals = temp.grads;
            //HP = HPCreation();
            //HPCreationGPU();
            //for (int r = 1; r < nLayers; r++)
            //{
            //    getHPLayer(r) = accelerator.Allocate2DDenseX(HP[r]);
            //}
            //for (int r = 1; r < nLayers; r++)
            //{
            //    HP[r] = getHPLayer(r).GetAsArray2D();
            //}
            //HPGPU = new HistoPyramid(HP, n, accelerator);

            //edges = temp.edges;
            //edges = march.edges;
            using (StreamWriter fs = fi.CreateText())
            {
                //HPTraversal(fs);
                MarchGPU(fs);
                //MarchGPUBatchRobust(fs);

                int f = 0;
                for (f = 1; f < count - 1; f += 3)
                {
                    fs.WriteLine("f " + f + " " + (f + 1) + " " + (f + 2));
                    //fs.WriteLine("f " + f + "//" + f + " " + (f + 1) + "//" + (f + 1) + " " + (f + 2) + "//" + (f + 2));
                }
                Console.WriteLine(count);
            }

            for (i = 1; i < nLayers; i++)
            {
                if (getHPLayer(i) != null && !getHPLayer(i).IsDisposed)
                    getHPLayer(i).Dispose();
            }
            //stopWatch.Stop();
            //ts = stopWatch.Elapsed;
            //string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            //    ts.Hours, ts.Minutes, ts.Seconds,
            //    ts.Milliseconds / 10);
            //Console.WriteLine("RunTime " + elapsedTime);
        }

        public ushort[,,] CreateSphere(int size)
        {
            double factor = Math.Sqrt((size / 2) * (size / 2) * 5);
            ushort[,,] slice = new ushort[size, size, size];
            for (int k = 0; k < size; k++)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        slice[k, j, i] = (ushort)(Math.Sqrt((i - size / 2) * (i - size / 2) + (j - size / 2) * (j - size / 2) + (k - size / 2) * (k - size / 2)) * (2000 / factor));
                        //double h = (k + i) * 60;
                        //bool p = h > (double)threshold;
                        //slice[k, j, i] = (ushort)(p ? threshold - 50 : threshold + 50);
                        //slice[k, j, i] = (ushort)(((j - size / 2) + (i - size / 2) + (k - size / 2)) * 20);
                    }
                }
            }
            return slice;
        }

        public static void BuildHP(Index2D index, ArrayView2D<uint, Stride2D.DenseX> HPLayer, ArrayView2D<uint, Stride2D.DenseX> HPLayerPrev)
        {
            HPLayer[index] = HPLayerPrev[index.X * 2, index.Y * 2] + HPLayerPrev[index.X * 2 + 1, index.Y * 2] + HPLayerPrev[index.X * 2 + 1, index.Y * 2 + 1] + HPLayerPrev[index.X * 2, index.Y * 2 + 1];
        }


        public static void BuildHPFirst(Index2D index, ArrayView2D<uint, Stride2D.DenseX> HPLayer, ArrayView2D<byte, Stride2D.DenseX> HPLayerBase)
        {
            HPLayer[index] = (uint)(HPLayerBase[index.X * 2, index.Y * 2] + HPLayerBase[index.X * 2 + 1, index.Y * 2] + HPLayerBase[index.X * 2 + 1, index.Y * 2 + 1] + HPLayerBase[index.X * 2, index.Y * 2 + 1]);
        }

        private static void HPCreationGPU()
        {
            nLayers = (ushort)(Math.Ceiling(Math.Log(HPBaseLayer.GetLength(0), 2)) + 1);
            HP = new uint[16][,];
            HP[0] = new uint[HPBaseLayer.GetLength(0), HPBaseLayer.GetLength(0)];
            Array.Copy(HPBaseLayer, HP[0], HPBaseLayer.Length);
            //for (int i = 0; i < HPBaseLayer.GetLength(0); i++)
            //{
            //    for (int j = 0; j < HPBaseLayer.GetLength(0); j++)
            //    {
            //        HP[0][j,i] = HPBaseLayer[j,i];
            //    }
            //}

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            getHPLayer(1) = accelerator.Allocate2DDenseX<uint>(new Index2D((int)HPBaseConfig.Extent.X / 2));
            hpFirstLayer(getHPLayer(1).IntExtent, getHPLayer(1).View, HPBaseConfig.View);
            accelerator.Synchronize();
            for (int i = 2; i < 16; i++)
            {
                int l = Math.Max(HP[0].GetLength(0) / (int)Math.Pow(2, i), 1);
                //HP[i] = new int[l, l];
                if (i < nLayers)
                {
                    Index2D index = new Index2D(l);
                    getHPLayer(i) = accelerator.Allocate2DDenseX<uint>(index);

                    hpCreation(index, getHPLayer(i).View, getHPLayer(i - 1).View);
                    accelerator.Synchronize();
                }
                //else
                //    HP[i] = new int[,] { { 0 } };
            }


            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            uint[,] data = data = getHPLayer(nLayers - 1).GetAsArray2D();
            if (data.Length == 1)
                nTri = (int)data[0, 0];
            //for (int n = 0; n < 1; n++)
            //{
            //    for (int i = 0; i < width; i++)
            //    {
            //        for (int j = 0; j < width; j++)
            //        {
            //            Console.Write(HP[n][j, i]);
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine(n);
            //}

            //return HP;
        }

        private static uint[][,] HPCreation()
        {
            nLayers = (ushort)(Math.Ceiling(Math.Log(HPBaseLayer.GetLength(0), 2)) + 1);
            uint[][,] HP = new uint[16][,];
            HP[0] = new uint[HPBaseLayer.GetLength(0), HPBaseLayer.GetLength(0)];
            Array.Copy(HPBaseLayer, HP[0], HPBaseLayer.Length);
            //for (int i = 0; i < HPBaseLayer.GetLength(0); i++)
            //{
            //    for (int j = 0; j < HPBaseLayer.GetLength(0); j++)
            //    {
            //        HP[0][j,i] = HPBaseLayer[j,i];
            //    }
            //}

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 1; i < 16; i++)
            {
                //int l = Math.Max(HP[i - 1].GetLength(0) / 2, 1);
                //HP[i] = new int[l, l];
                if (i < nLayers)
                {
                    int l = Math.Max(HP[i - 1].GetLength(0) / 2, 1);
                    HP[i] = new uint[l, l];
                    for (int iHP = 0; iHP < l; iHP++)
                    {
                        for (int jHP = 0; jHP < l; jHP++)
                        {
                            HP[i][iHP, jHP] = HP[i - 1][iHP * 2, jHP * 2] + HP[i - 1][iHP * 2 + 1, jHP * 2] + HP[i - 1][iHP * 2 + 1, jHP * 2 + 1] + HP[i - 1][iHP * 2, jHP * 2 + 1];
                        }
                    }
                }
                else
                    HP[i] = new uint[,] { { 0 } };
            }
            nTri = (int)HP[nLayers - 1][0, 0];


            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            //for (int n = 0; n < 1; n++)
            //{
            //    for (int i = 0; i < width; i++)
            //    {
            //        for (int j = 0; j < width; j++)
            //        {
            //            Console.Write(HP[n][j, i]);
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine(n);
            //}

            return HP;
        }

        public static void HPTraverseKernel(Index1D index, ArrayView1D<FlatPoint, Stride1D.Dense> p, ArrayView1D<uint, Stride1D.Dense> k, ArrayView2D<uint, Stride2D.DenseX> HPLayer)
        {
            uint a = HPLayer[p[index].X, p[index].Y];
            uint b = HPLayer[p[index].X + 1, p[index].Y] + a;
            uint c = HPLayer[p[index].X, p[index].Y + 1] + b;
            uint d = HPLayer[p[index].X + 1, p[index].Y + 1] + c;
            if (d == 0 || index == 16)
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
            k[index] = k[index];
            p[index].X *= 2;
            p[index].Y *= 2;
        }

        public static void HPFinalLayer(Index1D index,
            ArrayView1D<FlatPoint, Stride1D.Dense> p,
            ArrayView1D<uint, Stride1D.Dense> k,
            ArrayView2D<byte, Stride2D.DenseX> HPLayer,
            ArrayView1D<Triangle, Stride1D.Dense> triangles,
            ArrayView3D<byte, Stride3D.DenseXY> edges,
            ArrayView1D<Edge, Stride1D.Dense> triTable,
            ArrayView3D<ushort, Stride3D.DenseXY> input,
            int thresh, int HPSqrt)
        {
            uint a = HPLayer[p[index].X, p[index].Y];
            uint b = HPLayer[p[index].X + 1, p[index].Y] + a;
            uint c = HPLayer[p[index].X, p[index].Y + 1] + b;
            uint d = HPLayer[p[index].X + 1, p[index].Y + 1] + c;
            //int l = (int)k[index];
            //if (d == 0)
            //    ;
            if (k[index] < a)
                ;
            else if (k[index] < b)
            {
                k[index] = k[index] - a;
                p[index].X++;
            }
            else if (k[index] < c)
            {
                k[index] = k[index] - b;
                p[index].Y++;
            }
            else if (k[index] < d)
            {
                k[index] = k[index] - c;
                p[index].X++;
                p[index].Y++;
            }

            Index3D index3D = new Index3D(HPSqrt * (int)(p[index].X / (HPSqrt * HPSqrt)) + (int)(p[index].Y / (HPSqrt * HPSqrt)), p[index].X % (HPSqrt * HPSqrt), p[index].Y % (HPSqrt * HPSqrt));
            index3D = new Index3D(index3D.Z, index3D.Y, index3D.X);
            ushort l;
            //if (index3D.Z + 1 > input.Extent.Z - 1)
            //    l = input[-1,0,0];

            //new Point((ushort)(index3D.X + 1), (ushort)(index3D.Y + 1), (index3D.Z + 1), input[(index3D.Z + 1), index3D.Y + 1, index3D.X + 1],
            //new Normal(
            //     input[(index3D.Z + 1), (index3D.Y + 1), Math.Min((input.Extent.Z) - 1, (index3D.X + 1) + 1)] - input[(index3D.Z + 1), (index3D.Y + 1), Math.Max((index3D.X + 1) - 1, 0)],
            //     input[(index3D.Z + 1), Math.Min((input.Extent.Y) - 1, (index3D.Y + 1) + 1), (index3D.X + 1)] - input[(index3D.Z + 1), Math.Max((index3D.Y + 1) - 1, 0), (index3D.X + 1)],
            //     input[Math.Min((int)input.Extent.X - 1, (index3D.Z + 1) + 1), (index3D.Y + 1), (index3D.X + 1)] - input[Math.Max((index3D.Z + 1) - 1, 0), (index3D.Y + 1), (index3D.X + 1)]
            // ));

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
            //if (triTable[edges[index3D.Z, index3D.Y, index3D.X]].getn() / 3 != HPLayer[p[index].X, p[index].Y])
            //    ;
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
            //cubeConfig = accelerator.Allocate3DDenseXY(cubes);
            //byteLayer = accelerator.Allocate2DDenseX(HPBaseLayer);
            //k.MemSetToZero();
            p.MemSetToZero();
            accelerator.Synchronize();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (n = nLayers - 2; n > 0; n--)
            {
                traversalKernel(index, p.View, k.View, getHPLayer(n).View);
                accelerator.Synchronize();
                //getHPLayer(n).Dispose();
                //for (int o = 0; o < 1; o++)
                //{
                //    for (int i = 0; i < uintLayer.GetAsArray2D().GetLength(0); i++)
                //    {
                //        for (int j = 0; j < uintLayer.GetAsArray2D().GetLength(0); j++)
                //        {
                //            Console.Write(uintLayer.GetAsArray2D()[j, i]);
                //        }
                //        Console.WriteLine();
                //    }
                //    Console.WriteLine(n);
                //}
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


            //for (int o = 0; o < 1; o++)
            //{
            //    for (int i = 0; i < byteLayer.GetAsArray2D().GetLength(0); i++)
            //    {
            //        for (int j = 0; j < byteLayer.GetAsArray2D().GetLength(0); j++)
            //        {
            //            if ((byteLayer.GetAsArray2D()[j, i] != HP[0][j, i]))
            //                ;
            //            Console.Write(byteLayer.GetAsArray2D()[j, i]);
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine(n);
            //}

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
                //vertices.Add(vertex);
                fs.WriteLine("v " + triangle.vertex1.X + " " + triangle.vertex1.Y + " " + triangle.vertex1.Z);
                fs.WriteLine("vn " + triangle.vertex1.normal.X + " " + triangle.vertex1.normal.Y + " " + triangle.vertex1.normal.Z);
                fs.WriteLine("v " + triangle.vertex2.X + " " + triangle.vertex2.Y + " " + triangle.vertex2.Z);
                fs.WriteLine("vn " + triangle.vertex2.normal.X + " " + triangle.vertex2.normal.Y + " " + triangle.vertex2.normal.Z);
                fs.WriteLine("v " + triangle.vertex3.X + " " + triangle.vertex3.Y + " " + triangle.vertex3.Z);
                fs.WriteLine("vn " + triangle.vertex3.normal.X + " " + triangle.vertex3.normal.Y + " " + triangle.vertex3.normal.Z);
                //    //fs.WriteLine("vt " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                //    //Point normal = Normal(slices[0], slices[1], slices[2], slices[3], vertex, k);
                //    //fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
                //    //Point n = new Point(vertex.normal.X * 2 + normal.X, vertex.normal.Y * 2 + normal.Y, vertex.normal.Z * 2 + normal.Z, 0);
                //    //normals.Add(normal);
                count += 3;
            }

            //Edge[] r = new Edge[cubes.Length]; 
            //r = r.Where(x => x.E1 > 0).ToArray();
            //var pt = accelerator.Allocate1D<int>(n);
            int iX = 0;
        }

        private static void HPTraversal(StreamWriter fs)
        {

            //for (int o = 0; o < nLayers; o++)
            //{
            //    for (int i = 0; i < HP[o].GetLength(0); i++)
            //    {
            //        for (int j = 0; j < HP[o].GetLength(0); j++)
            //        {
            //            Console.Write(HP[o][j, i]);
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine(o);
            //}
            nTri = (int)HP[nLayers - 1][0, 0];
            count = nTri * 3;
            Triangle[] triangles = new Triangle[nTri];

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (uint i = 0; i < nTri; i++)
            {
                Index2D p = new Index2D(0, 0);
                uint k = i;
                for (int n = nLayers - 2; n >= 0; n--)
                {
                    uint a = HP[n][p.X, p.Y];
                    uint b = HP[n][p.X + 1, p.Y] + a;
                    uint c = HP[n][p.X, p.Y + 1] + b;
                    uint d = HP[n][p.X + 1, p.Y + 1] + c;
                    if (d == 0)
                        ;
                    if (k < a)
                        ;
                    else if (k < b)
                    {
                        k -= a;
                        p = p.Add(new Index2D(1, 0));
                    }
                    else if (k < c)
                    {
                        k -= b;
                        p = p.Add(new Index2D(0, 1));
                    }
                    else if (k < d)
                    {
                        k -= c;
                        p = p.Add(new Index2D(1, 1));
                    }
                    if (n > 0)
                        p = p.Add(p);
                }
                //if (HP[0][p.X, p.Y] == 0)
                //    ;
                uint pt = HP[0][p.X, p.Y];
                Index3D index3D = new Index3D(((int)Math.Sqrt(HPsize) * (int)(p.X / HPsize) + (int)(p.Y / HPsize)), p.X % HPsize, p.Y % HPsize);
                index3D = new Index3D(index3D.Z, index3D.Y, index3D.X);

                //Cube tempCube = new Cube(
                //    new Point(index3D.X, index3D.Y, index3D.Z, slices[index3D.Z, index3D.Y, index3D.X], normals[index3D.Z, index3D.Y, index3D.X]),
                //    new Point((ushort)(index3D.X + 1), index3D.Y, index3D.Z, slices[index3D.Z, index3D.Y, index3D.X + 1], normals[index3D.Z, index3D.Y, index3D.X + 1]),
                //    new Point((ushort)(index3D.X + 1), (ushort)(index3D.Y + 1), index3D.Z, slices[index3D.Z, index3D.Y + 1, index3D.X + 1], normals[index3D.Z, (index3D.Y + 1), index3D.X + 1]),
                //    new Point(index3D.X, (ushort)(index3D.Y + 1), index3D.Z, slices[index3D.Z, index3D.Y + 1, index3D.X], normals[index3D.Z, (index3D.Y + 1), index3D.X]),
                //    new Point(index3D.X, index3D.Y, (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y, index3D.X], normals[(index3D.Z + 1), index3D.Y, index3D.X]),
                //    new Point((ushort)(index3D.X + 1), index3D.Y, (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y, index3D.X + 1], normals[(index3D.Z + 1), index3D.Y, index3D.X + 1]),
                //    new Point((ushort)(index3D.X + 1), (ushort)(index3D.Y + 1), (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y + 1, index3D.X + 1], normals[(index3D.Z + 1), (index3D.Y + 1), index3D.X + 1]),
                //    new Point(index3D.X, (ushort)(index3D.Y + 1), (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y + 1, index3D.X], normals[(index3D.Z + 1), (index3D.Y + 1), index3D.X])
                //    );
                Cube tempCube = new Cube(
                               new Point(index3D.X, index3D.Y, index3D.Z, slices[index3D.Z, index3D.Y, index3D.X],
                               new Normal(
                                    slices[index3D.Z, index3D.Y, Math.Min((width) - 1, index3D.X + 1)] - slices[index3D.Z, index3D.Y, Math.Max(index3D.X - 1, 0)],
                                    slices[index3D.Z, Math.Min((width) - 1, index3D.Y + 1), index3D.X] - slices[index3D.Z, Math.Max(index3D.Y - 1, 0), index3D.X],
                                    slices[Math.Min(slices.GetLength(0) - 1, index3D.Z + 1), index3D.Y, index3D.X] - slices[Math.Max(index3D.Z - 1, 0), index3D.Y, index3D.X]
                                )),
                               new Point((ushort)(index3D.X + 1), index3D.Y, index3D.Z, slices[index3D.Z, index3D.Y, index3D.X + 1],
                               new Normal(
                                    slices[index3D.Z, index3D.Y, Math.Min((width) - 1, (index3D.X + 1) + 1)] - slices[index3D.Z, index3D.Y, Math.Max((index3D.X + 1) - 1, 0)],
                                    slices[index3D.Z, Math.Min((width) - 1, index3D.Y + 1), (index3D.X + 1)] - slices[index3D.Z, Math.Max(index3D.Y - 1, 0), (index3D.X + 1)],
                                    slices[Math.Min(slices.GetLength(0) - 1, index3D.Z + 1), index3D.Y, (index3D.X + 1)] - slices[Math.Max(index3D.Z - 1, 0), index3D.Y, (index3D.X + 1)]
                                )),
                               new Point((ushort)(index3D.X + 1), (ushort)(index3D.Y + 1), index3D.Z, slices[index3D.Z, index3D.Y + 1, index3D.X + 1],
                               new Normal(
                                    slices[index3D.Z, (index3D.Y + 1), Math.Min((width) - 1, (index3D.X + 1) + 1)] - slices[index3D.Z, (index3D.Y + 1), Math.Max((index3D.X + 1) - 1, 0)],
                                    slices[index3D.Z, Math.Min((width) - 1, (index3D.Y + 1) + 1), (index3D.X + 1)] - slices[index3D.Z, Math.Max((index3D.Y + 1) - 1, 0), (index3D.X + 1)],
                                    slices[Math.Min(slices.GetLength(0) - 1, index3D.Z + 1), (index3D.Y + 1), (index3D.X + 1)] - slices[Math.Max(index3D.Z - 1, 0), (index3D.Y + 1), (index3D.X + 1)]
                                )),
                               new Point(index3D.X, (ushort)(index3D.Y + 1), index3D.Z, slices[index3D.Z, index3D.Y + 1, index3D.X],
                               new Normal(
                                    slices[index3D.Z, (index3D.Y + 1), Math.Min((width) - 1, index3D.X + 1)] - slices[index3D.Z, (index3D.Y + 1), Math.Max(index3D.X - 1, 0)],
                                    slices[index3D.Z, Math.Min((width) - 1, (index3D.Y + 1) + 1), index3D.X] - slices[index3D.Z, Math.Max((index3D.Y + 1) - 1, 0), index3D.X],
                                    slices[Math.Min(slices.GetLength(0) - 1, index3D.Z + 1), (index3D.Y + 1), index3D.X] - slices[Math.Max(index3D.Z - 1, 0), (index3D.Y + 1), index3D.X]
                                )),
                               new Point(index3D.X, index3D.Y, (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y, index3D.X],
                               new Normal(
                                    slices[(index3D.Z + 1), index3D.Y, Math.Min((width) - 1, index3D.X + 1)] - slices[(index3D.Z + 1), index3D.Y, Math.Max(index3D.X - 1, 0)],
                                    slices[(index3D.Z + 1), Math.Min((width) - 1, index3D.Y + 1), index3D.X] - slices[(index3D.Z + 1), Math.Max(index3D.Y - 1, 0), index3D.X],
                                    slices[Math.Min(slices.GetLength(0) - 1, (index3D.Z + 1) + 1), index3D.Y, index3D.X] - slices[Math.Max((index3D.Z + 1) - 1, 0), index3D.Y, index3D.X]
                                )),
                               new Point((ushort)(index3D.X + 1), index3D.Y, (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y, index3D.X + 1],
                               new Normal(
                                    slices[(index3D.Z + 1), index3D.Y, Math.Min((width) - 1, (index3D.X + 1) + 1)] - slices[(index3D.Z + 1), index3D.Y, Math.Max((index3D.X + 1) - 1, 0)],
                                    slices[(index3D.Z + 1), Math.Min((width) - 1, index3D.Y + 1), (index3D.X + 1)] - slices[(index3D.Z + 1), Math.Max(index3D.Y - 1, 0), (index3D.X + 1)],
                                    slices[Math.Min(slices.GetLength(0) - 1, (index3D.Z + 1) + 1), index3D.Y, (index3D.X + 1)] - slices[Math.Max((index3D.Z + 1) - 1, 0), index3D.Y, (index3D.X + 1)]
                                )),
                               new Point((ushort)(index3D.X + 1), (ushort)(index3D.Y + 1), (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y + 1, index3D.X + 1],
                               new Normal(
                                    slices[(index3D.Z + 1), (index3D.Y + 1), Math.Min((width) - 1, (index3D.X + 1) + 1)] - slices[(index3D.Z + 1), (index3D.Y + 1), Math.Max((index3D.X + 1) - 1, 0)],
                                    slices[(index3D.Z + 1), Math.Min((width) - 1, (index3D.Y + 1) + 1), (index3D.X + 1)] - slices[(index3D.Z + 1), Math.Max((index3D.Y + 1) - 1, 0), (index3D.X + 1)],
                                    slices[Math.Min(slices.GetLength(0) - 1, (index3D.Z + 1) + 1), (index3D.Y + 1), (index3D.X + 1)] - slices[Math.Max((index3D.Z + 1) - 1, 0), (index3D.Y + 1), (index3D.X + 1)]
                                )),
                               new Point(index3D.X, (ushort)(index3D.Y + 1), (index3D.Z + 1), slices[(index3D.Z + 1), index3D.Y + 1, index3D.X],
                               new Normal(
                                    slices[(index3D.Z + 1), (index3D.Y + 1), Math.Min((width) - 1, index3D.X + 1)] - slices[(index3D.Z + 1), (index3D.Y + 1), Math.Max(index3D.X - 1, 0)],
                                    slices[(index3D.Z + 1), Math.Min((width) - 1, (index3D.Y + 1) + 1), index3D.X] - slices[(index3D.Z + 1), Math.Max((index3D.Y + 1) - 1, 0), index3D.X],
                                    slices[Math.Min(slices.GetLength(0) - 1, (index3D.Z + 1) + 1), (index3D.Y + 1), index3D.X] - slices[Math.Max((index3D.Z + 1) - 1, 0), (index3D.Y + 1), index3D.X]
                                ))
                               );
                //var l = cubes[k, j, i];
                tempCube.getConfig();
                triangles[i] = tempCube.MarchHP(threshold, triangleTable[cubes[index3D.Z, index3D.Y, index3D.X]], (int)k);
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            foreach (var triangle in triangles)
            {
                //vertices.Add(vertex);
                fs.WriteLine("v " + triangle.vertex1.X + " " + triangle.vertex1.Y + " " + triangle.vertex1.Z);
                fs.WriteLine("vn " + triangle.vertex1.normal.X + " " + triangle.vertex1.normal.Y + " " + triangle.vertex1.normal.Z);
                fs.WriteLine("v " + triangle.vertex2.X + " " + triangle.vertex2.Y + " " + triangle.vertex2.Z);
                fs.WriteLine("vn " + triangle.vertex2.normal.X + " " + triangle.vertex2.normal.Y + " " + triangle.vertex2.normal.Z);
                fs.WriteLine("v " + triangle.vertex3.X + " " + triangle.vertex3.Y + " " + triangle.vertex3.Z);
                fs.WriteLine("vn " + triangle.vertex3.normal.X + " " + triangle.vertex3.normal.Y + " " + triangle.vertex3.normal.Z);
                //    //fs.WriteLine("vt " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                //    //Point normal = Normal(slices[0], slices[1], slices[2], slices[3], vertex, k);
                //    //fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
                //    //Point n = new Point(vertex.normal.X * 2 + normal.X, vertex.normal.Y * 2 + normal.Y, vertex.normal.Z * 2 + normal.Z, 0);
                //    //normals.Add(normal);
                //count += 3;
            }
        }
        private static byte[,,] MarchingCubesCPU()
        {
            //Edge[,,] edges = new Edge[(slices.GetLength(0) - 1), (width - 1), (length - 1)];
            cubeBytes = new byte[(slices.GetLength(0) - 1), (width - 1), (length - 1)];
            byte cubeByte;
            HPBaseLayer = new byte[HPsize * (int)Math.Sqrt(HPsize), HPsize * (int)Math.Sqrt(HPsize)];

            //Normal[,,] grads = new Normal[(slices.GetLength(0)), (width), (length)];
            //byte[,] configBytes = new byte[511, 511];
            //int[,] edges = new int[length, width];

            //bit order 
            // i,j,k 
            // i+1,j,k
            // i+1,j+1,k
            // i,j+1,k
            // i,j,k+1
            // i+1,j,k+1
            // i+1,j+1,k+1
            // i,j+1,k+1
            int i, j, k;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (k = 0; k < slices.GetLength(0); k++)
            {
                for (i = 0; i < slices.GetLength(2); i++)
                {
                    for (j = 0; j < slices.GetLength(1); j++)
                    {
                        //grads[k, j, i] =
                        //new Normal(
                        //     slices[k, j, Math.Min((width) - 1, i + 1)] - slices[k, j, Math.Max(i - 1, 0)],
                        //     slices[k, Math.Min((width) - 1, j + 1), i] - slices[k, Math.Max(j - 1, 0), i],
                        //     slices[Math.Min(slices.GetLength(0) - 1, k + 1), j, i] - slices[Math.Max(k - 1, 0), j, i]
                        // );
                        if (k != slices.GetLength(0) - 1 && j != slices.GetLength(1) - 1 && i != slices.GetLength(2) - 1)
                        {
                            cubeByte = 0;
                            cubeByte += (slices[k, j, i] < threshold) ? (byte)0x01 : (byte)0;
                            cubeByte += (slices[k, j, i + 1] < threshold) ? (byte)0x02 : (byte)0;
                            cubeByte += (slices[k, j + 1, i + 1] < threshold) ? (byte)0x04 : (byte)0;
                            cubeByte += (slices[k, j + 1, i] < threshold) ? (byte)0x08 : (byte)0;
                            cubeByte += (slices[k + 1, j, i] < threshold) ? (byte)0x10 : (byte)0;
                            cubeByte += (slices[k + 1, j, i + 1] < threshold) ? (byte)0x20 : (byte)0;
                            cubeByte += (slices[k + 1, j + 1, i + 1] < threshold) ? (byte)0x40 : (byte)0;
                            cubeByte += (slices[k + 1, j + 1, i] < threshold) ? (byte)0x80 : (byte)0;

                            cubeBytes[k, j, i] = cubeByte;
                            HPBaseLayer[j + (int)(k / Math.Sqrt(HPsize)) * HPsize, i + HPsize * (int)(k % Math.Sqrt(HPsize))] = (byte)(triangleTable[cubeByte].getAsArray().Where(x => x >= 0).Count() / 3);
                        }
                    }
                }
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            return cubeBytes;
        }

        public static void Assign1D(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> edges, ArrayView2D<byte, Stride2D.DenseX> HPindices, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView<Edge> triTable, int thresh, int width, int HPsizeFator)
        {
            //return Enumerable.Range(0, matrix.GetLength(1))
            //    .Select(x => matrix[rowNumber, x])
            //    .ToArray();
            //byte config = 0;
            //edges[index] = triTable[(int)(
            //    ((input[index] < thresh) ? (byte)0x01 : (byte)0) +
            //    ((input[index.X, index.Y, index.Z + 1] < thresh) ? (byte)0x02 : (byte)0) +
            //    ((input[index.X, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x04 : (byte)0) +
            //    ((input[index.X, index.Y + 1, index.Z] < thresh) ? (byte)0x08 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z + 1] < thresh) ? (byte)0x20 : (byte)0) +
            //    ((input[index.X + 1, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x40 : (byte)0) +
            //    ((input[index.X + 1, index.Y + 1, index.Z] < thresh) ? (byte)0x80 : (byte)0))
            //    ];
            if (index.InBounds(new Index3D((int)(input.Extent.X - 1), width - 1, width - 1)))
            {
                //if (index.X > 24) 
                int q;
                //    ;
                byte config = 0;
                config += (input[(index.X), (index.Y), (index.Z)] < thresh) ? (byte)0x01 : (byte)0;
                config += (input[(index.X), (index.Y), (index.Z) + 1] < thresh) ? (byte)0x02 : (byte)0;
                config += (input[(index.X), (index.Y) + 1, (index.Z) + 1] < thresh) ? (byte)0x04 : (byte)0;
                config += (input[(index.X), (index.Y) + 1, (index.Z)] < thresh) ? (byte)0x08 : (byte)0;
                config += (input[(index.X) + 1, (index.Y), (index.Z)] < thresh) ? (byte)0x10 : (byte)0;
                config += (input[(index.X) + 1, (index.Y), (index.Z) + 1] < thresh) ? (byte)0x20 : (byte)0;
                config += (input[(index.X) + 1, (index.Y) + 1, (index.Z) + 1] < thresh) ? (byte)0x40 : (byte)0;
                config += (input[(index.X) + 1, (index.Y) + 1, (index.Z)] < thresh) ? (byte)0x80 : (byte)0;
                edges[index.X, index.Y, (index.Z)] = config;
                HPindices[index.Y + HPsizeFator * HPsizeFator * (int)(index.X / HPsizeFator), index.Z + HPsizeFator * HPsizeFator * (int)(index.X % HPsizeFator)] = (byte)(triTable[(int)config].getn() / 3);
                //if ((byte)(edges[index.X, index.Y, index.Z].getn() / 3) > 0)
                //    q = edges[index.X, index.Y, index.Z].getn();
                //Index2D l2;
                //if ((byte)(edges[index.X, index.Y, index.Z].getn() / 3) > 0)
                //    l2 = new Index2D(index.Y + HPsizeFator * HPsizeFator * (int)(index.X / HPsizeFator), index.Z + HPsizeFator * HPsizeFator * (int)(index.X % HPsizeFator));

                //Index3D index3D = new Index3D(HPsizeFator * (int)(l2.X / (HPsizeFator * HPsizeFator)) + (int)(l2.Y / (HPsizeFator * HPsizeFator)), l2.X % (HPsizeFator * HPsizeFator), l2.Y % (HPsizeFator * HPsizeFator));
                //if (!index3D.Equals(index))
                //    ;
            }

            //normals[index.X, index.Y, (index.Z)] =
            //new Normal(
            //    input[(index.X), (index.Y), Math.Min((width) - 1, (index.Z) + 1)] - input[(index.X), (index.Y), Math.Max((index.Z) - 1, 0)],
            //    input[(index.X), Math.Min((width) - 1, (index.Y) + 1), (index.Z)] - input[(index.X), Math.Max((index.Y) - 1, 0), (index.Z)],
            //    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.X) + 1), (index.Y), (index.Z)] - input[Math.Max((index.X) - 1, 0), (index.Y), (index.Z)]
            //);
        }

        public static byte[,,] MarchingCubesGPU()
        {
            //Edge[,,] cubeBytes = new Edge[slices.GetLength(0) - 1, width - 1, length - 1];
            //Normal[,,] grads = new Normal[slices.GetLength(0) - 1, width - 1, length - 1];
            Index3D index = new Index3D(slices.GetLength(0), slices.GetLength(1), slices.GetLength(2));
            List<byte> edgeList = new List<byte>();
            //List<Normal> normalList = new List<Normal>();
            //MemoryBuffer3D<Edge, Stride3D.DenseXY> cubeConfig = accelerator.Allocate3DDenseXY<Edge>(index);
            //MemoryBuffer3D<Normal, Stride3D.DenseXY> gradConfig = accelerator.Allocate3DDenseXY<Normal>(index);
            ////byte[,] configBytes = new byte[511, 511];

            //Normal[] grad = new Normal[grads.Length];

            //int[,,] edges = new int[slices.Length - 1, width - 1, length - 1];

            //bit order 
            // i,j,k 
            // i+1,j,k
            // i+1,j+1,k
            // i,j+1,k
            // i,j,k+1
            // i+1,j,k+1
            // i+1,j+1,k+1
            // i,j+1,k+1

            HPBaseLayer = new byte[HPsize * (int)Math.Sqrt(HPsize), HPsize * (int)Math.Sqrt(HPsize)];

            //Normal[,,] grads = new Normal[index.X, index.Y, index.Z];
            cubeBytes = new byte[(index.X - 1), (index.Y - 1), (index.Z - 1)];
            //var gradPinned = GCHandle.Alloc(grads, GCHandleType.Pinned);
            var cubePinned = GCHandle.Alloc(cubeBytes, GCHandleType.Pinned);
            var HPPinned = GCHandle.Alloc(HPBaseLayer, GCHandleType.Pinned);
            PageLockedArray3D<byte> cubeLocked = accelerator.AllocatePageLocked3D<byte>(new Index3D(index.X - 1, index.Y - 1, index.Z - 1));
            PageLockedArray2D<byte> HPLocked = accelerator.AllocatePageLocked2D<byte>(new Index2D(HPBaseLayer.GetLength(0)));
            //PageLockedArray3D<Normal> gradLocked = accelerator.AllocatePageLocked3D<Normal>(index);
            cubeConfig = accelerator.Allocate3DDenseXY<byte>(cubeLocked.Extent);
            HPBaseConfig = accelerator.Allocate2DDenseX<byte>(HPLocked.Extent);
            //gradConfig = accelerator.Allocate3DDenseXY<Normal>(index);
            //var gradScope = accelerator.CreatePageLockFromPinned<Normal>(gradPinned.AddrOfPinnedObject(), grads.Length);
            var cubeScope = accelerator.CreatePageLockFromPinned<byte>(cubePinned.AddrOfPinnedObject(), cubeBytes.Length);
            var HPScope = accelerator.CreatePageLockFromPinned<byte>(HPPinned.AddrOfPinnedObject(), HPBaseLayer.Length);
            //gradConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, gradScope);
            cubeConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);
            HPBaseConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, HPScope);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            assign1D(index, cubeConfig.View, HPBaseConfig.View, sliced.View, triTable.View, threshold, width, (int)Math.Sqrt(HPsize));

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            //cubeConfig.CopyToCPU(cubeBytes);
            //HPBaseConfig.CopyToCPU(HPBaseLayer);
            cubeConfig.AsContiguous().CopyToPageLockedAsync(cubeLocked);
            cubeBytes = cubeLocked.GetArray();
            HPBaseConfig.AsContiguous().CopyToPageLockedAsync(HPLocked);
            HPBaseLayer = HPLocked.GetArray();
            //stopWatch.Stop();
            //HPBaseConfig.Dispose();
            //cubeConfig.Dispose();
            cubePinned.Free();
            HPPinned.Free();
            //sliced.Dispose();
            //GCHandle.FromIntPtr(gradIntPtr).Free();
            //GCHandle.FromIntPtr(cubeIntPtr).Free();

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            //ToBuffer3D(grads, slices.GetLength(0) - 1, width - 1, length - 1)
            return (cubeBytes);
        }


        public static (byte[] configs, Normal[] grads) MarchingCubesGPUX()
        {
            int slicePre = 251, i;

            //byte[,,] cubeBytes = new byte[slices.GetLength(0) - 1, width - 1, length - 1];
            //Normal[,,] grads = new Normal[slices.GetLength(0) - 1, width - 1, length - 1];
            Index3D Nindex = new Index3D(slices.GetLength(0), width, length);
            int Z = slices.GetLength(0);
            int nZ = (int)Math.Ceiling((double)Z / (double)slicePre);
            Normal[] grads = new Normal[Z * (width) * (length)];
            byte[] cubeBytes = new byte[(Z - 1) * (width - 1) * (length - 1)];
            Normal[] gradsSlice = new Normal[Math.Min(slicePre, Z) * (width) * (length)];
            byte[] cubeBytesSlice = new byte[Math.Min(slicePre, Z - 1) * (width - 1) * (length - 1)];
            List<byte> byteList = new List<byte>(cubeBytes.Length);
            List<Normal> normalList = new List<Normal>(grads.Length);
            PageLockedArray3D<byte> cubeLocked = accelerator.AllocatePageLocked3D<byte>(new Index3D(Math.Min(slicePre, Z - 1), (width - 1), (length - 1)));
            PageLockedArray3D<Normal> gradLocked = accelerator.AllocatePageLocked3D<Normal>(new Index3D(Math.Min(slicePre, Z) * (width) * (length)));
            cubeConfig = accelerator.Allocate3DDenseXY<byte>(cubeLocked.Extent);
            gradConfig = accelerator.Allocate3DDenseXY<Normal>(gradLocked.Extent);
            //MemoryBuffer3D<byte, Stride3D.DenseXY> cubeConfig = accelerator.Allocate3DDenseXY<byte>(index);
            //MemoryBuffer3D<Normal, Stride3D.DenseXY> gradConfig = accelerator.Allocate3DDenseXY<Normal>(index);
            ////byte[,] configBytes = new byte[511, 511];

            //Normal[] grad = new Normal[grads.Length];

            //int[,,] bytes = new int[slices.Length - 1, width - 1, length - 1];

            //bit order 
            // i,j,k 
            // i+1,j,k
            // i+1,j+1,k
            // i,j+1,k
            // i,j,k+1
            // i+1,j,k+1
            // i+1,j+1,k+1
            // i,j+1,k+1

            var gradScope = accelerator.CreatePageLockFromPinned(gradsSlice);
            var cubeScope = accelerator.CreatePageLockFromPinned(cubeBytesSlice);
            gradConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, gradScope);
            cubeConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);

            //assign_normal = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(AssignNormal);
            //assign_bytes= accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<byte>, int>(Assignbytes);

            //assign_normal1D = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView1D<Normal, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>>(AssignNormal1D);
            //assign_bytes1D = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView1D<byte, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<byte>, int>(Assignbytes1D);

            //triTable = accelerator.Allocate1D<byte>(triangleTable);
            //Console.WriteLine(slices.ToString());
            //Index3D num = new Index3D(slices.GetLength(0) - 1, slices.GetLength(1) - 1, slices.GetLength(2) - 1);

            //Console.WriteLine(num.ToString());
            //Console.WriteLine(cubeBytes.GetLength(0) + "," + cubeBytes.GetLength(1) + "," + cubeBytes.GetLength(2));
            //Console.WriteLine(grads.GetLength(0) + "," + grads.GetLength(1) + "," + grads.GetLength(2));
            //Console.WriteLine(num.ToString());


            //assign_bytes(num, cubeConfig.View, sliced.View, triTable.View , threshold);
            //assign_normal(num, gradConfig.View, sliced.View);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (i = 0; i < nZ; i++)
            {
                Index3D index = new Index3D(Math.Min(Nindex.X - i * slicePre, slicePre), width, length);
                if (index.Size == 0)
                    break;
                Point offset = new Point(i * slicePre, 0, 0, 0, new Normal());
                assign1D(index, cubeConfig.View, HPBaseConfig.View, sliced.View, triTable.View, threshold, width, 0);
                //assign_bytes1D(index, cubeConfig.View, sliced.View, triTable.View, threshold);
                //assign_normal1D(index, gradConfig.View, sliced.View);
                accelerator.Synchronize();
                gradConfig.AsContiguous().CopyToPageLockedAsync(gradLocked);
                cubeConfig.AsContiguous().CopyToPageLockedAsync(cubeLocked);

                Array.Copy(gradLocked.GetArray(), 0, grads, (int)offset.Z * width * width, index.Size);
                Array.Copy(cubeLocked.GetArray(), 0, cubeBytes, (int)offset.Z * (width - 1) * (width - 1), (index.X - 1) * (index.Y - 1) * (index.Z - 1));
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            cubeConfig.Dispose();
            gradConfig.Dispose();
            triTable.Dispose();
            //sliced.Dispose();


            //byteList.RemoveRange((Z - 1) * (width - 1) * (length- 1),byteList.Count - (Z - 1) * (width - 1) * (length - 1));
            //normalList.RemoveRange(Z * width * length, normalList.Count - Z * width * length);



            //cubeBytes = byteList.ToArray();
            //grads = normalList.ToArray();

            ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            //ToBuffer3D(grads, slices.GetLength(0) - 1, width - 1, length - 1)
            return (cubeBytes, grads);
        }

        public static void MarchCPU(StreamWriter fs)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int i, j, k;
            Point[] vertice;
            for (k = 0; k < slices.GetLength(0) - 2; k++)
            {
                for (i = 0; i < slices.GetLength(2) - 2; i++)
                {
                    for (j = 0; j < slices.GetLength(1) - 2; j++)
                    {
                        if (cubes[k, j, i] != 0 && cubes[k, j, i] != byte.MaxValue)
                        {
                            Cube tempCube = new Cube(
                                new Point(i, j, k, slices[k, j, i],
                                new Normal(
                                     slices[k, j, Math.Min((width) - 1, i + 1)] - slices[k, j, Math.Max(i - 1, 0)],
                                     slices[k, Math.Min((width) - 1, j + 1), i] - slices[k, Math.Max(j - 1, 0), i],
                                     slices[Math.Min(slices.GetLength(0) - 1, k + 1), j, i] - slices[Math.Max(k - 1, 0), j, i]
                                 )),
                                new Point((ushort)(i + 1), j, k, slices[k, j, i + 1],
                                new Normal(
                                     slices[k, j, Math.Min((width) - 1, (i + 1) + 1)] - slices[k, j, Math.Max((i + 1) - 1, 0)],
                                     slices[k, Math.Min((width) - 1, j + 1), (i + 1)] - slices[k, Math.Max(j - 1, 0), (i + 1)],
                                     slices[Math.Min(slices.GetLength(0) - 1, k + 1), j, (i + 1)] - slices[Math.Max(k - 1, 0), j, (i + 1)]
                                 )),
                                new Point((ushort)(i + 1), (ushort)(j + 1), k, slices[k, j + 1, i + 1],
                                new Normal(
                                     slices[k, (j + 1), Math.Min((width) - 1, (i + 1) + 1)] - slices[k, (j + 1), Math.Max((i + 1) - 1, 0)],
                                     slices[k, Math.Min((width) - 1, (j + 1) + 1), (i + 1)] - slices[k, Math.Max((j + 1) - 1, 0), (i + 1)],
                                     slices[Math.Min(slices.GetLength(0) - 1, k + 1), (j + 1), (i + 1)] - slices[Math.Max(k - 1, 0), (j + 1), (i + 1)]
                                 )),
                                new Point(i, (ushort)(j + 1), k, slices[k, j + 1, i],
                                new Normal(
                                     slices[k, (j + 1), Math.Min((width) - 1, i + 1)] - slices[k, (j + 1), Math.Max(i - 1, 0)],
                                     slices[k, Math.Min((width) - 1, (j + 1) + 1), i] - slices[k, Math.Max((j + 1) - 1, 0), i],
                                     slices[Math.Min(slices.GetLength(0) - 1, k + 1), (j + 1), i] - slices[Math.Max(k - 1, 0), (j + 1), i]
                                 )),
                                new Point(i, j, (k + 1), slices[(k + 1), j, i],
                                new Normal(
                                     slices[(k + 1), j, Math.Min((width) - 1, i + 1)] - slices[(k + 1), j, Math.Max(i - 1, 0)],
                                     slices[(k + 1), Math.Min((width) - 1, j + 1), i] - slices[(k + 1), Math.Max(j - 1, 0), i],
                                     slices[Math.Min(slices.GetLength(0) - 1, (k + 1) + 1), j, i] - slices[Math.Max((k + 1) - 1, 0), j, i]
                                 )),
                                new Point((ushort)(i + 1), j, (k + 1), slices[(k + 1), j, i + 1],
                                new Normal(
                                     slices[(k + 1), j, Math.Min((width) - 1, (i + 1) + 1)] - slices[(k + 1), j, Math.Max((i + 1) - 1, 0)],
                                     slices[(k + 1), Math.Min((width) - 1, j + 1), (i + 1)] - slices[(k + 1), Math.Max(j - 1, 0), (i + 1)],
                                     slices[Math.Min(slices.GetLength(0) - 1, (k + 1) + 1), j, (i + 1)] - slices[Math.Max((k + 1) - 1, 0), j, (i + 1)]
                                 )),
                                new Point((ushort)(i + 1), (ushort)(j + 1), (k + 1), slices[(k + 1), j + 1, i + 1],
                                new Normal(
                                     slices[(k + 1), (j + 1), Math.Min((width) - 1, (i + 1) + 1)] - slices[(k + 1), (j + 1), Math.Max((i + 1) - 1, 0)],
                                     slices[(k + 1), Math.Min((width) - 1, (j + 1) + 1), (i + 1)] - slices[(k + 1), Math.Max((j + 1) - 1, 0), (i + 1)],
                                     slices[Math.Min(slices.GetLength(0) - 1, (k + 1) + 1), (j + 1), (i + 1)] - slices[Math.Max((k + 1) - 1, 0), (j + 1), (i + 1)]
                                 )),
                                new Point(i, (ushort)(j + 1), (k + 1), slices[(k + 1), j + 1, i],
                                new Normal(
                                     slices[(k + 1), (j + 1), Math.Min((width) - 1, i + 1)] - slices[(k + 1), (j + 1), Math.Max(i - 1, 0)],
                                     slices[(k + 1), Math.Min((width) - 1, (j + 1) + 1), i] - slices[(k + 1), Math.Max((j + 1) - 1, 0), i],
                                     slices[Math.Min(slices.GetLength(0) - 1, (k + 1) + 1), (j + 1), i] - slices[Math.Max((k + 1) - 1, 0), (j + 1), i]
                                 ))
                                );
                            //var l = cubes[k, j, i];
                            vertice = tempCube.March(threshold, triangleTable[cubes[k, j, i]]);

                            foreach (var vertex in vertice)
                            {
                                vertices.Add(vertex);
                            }
                        }
                    }
                }
            }
            ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            foreach (var vertex in vertices)
            {
                fs.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z * 10);
                fs.WriteLine("vn " + vertex.normal.X + " " + vertex.normal.Y + " " + vertex.normal.Z);
                //    //fs.WriteLine("vt " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                //    //Point normal = Normal(slices[0], slices[1], slices[2], slices[3], vertex, k);
                //    //fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
                //    //Point n = new Point(vertex.normal.X * 2 + normal.X, vertex.normal.Y * 2 + normal.Y, vertex.normal.Z * 2 + normal.Z, 0);
                //    //normals.Add(normal);
                count++;

            }

        }

        public static void getVertices(Index3D index, ArrayView<Triangle> triangles, ArrayView3D<byte, Stride3D.DenseXY> edges, ArrayView1D<Edge, Stride1D.Dense> triTable, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView1D<byte, Stride1D.Dense> flag, Point offset, int thresh, int batchSize, int width)
        {
            if (edges[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X)] != 0 && edges[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X)] != byte.MaxValue)
            {
                //Point p = new Point(
                //                    (index.X + (int)offset.X), (index.Y + (int)offset.Y), (index.Z + (int)offset.Z),
                //                    input[((index.Z + (int)offset.Z) + 1), (index.Y + (int)offset.Y) + 1, (index.X + (int)offset.X) + 1],
                //Normal n = normals[((index.Z + (int)offset.Z) + 1), ((index.Y + (int)offset.Y) + 1), (index.X + (int)offset.X) + 1];
                Cube tempCube = new Cube(
                            new Point(
                                (index.X + (int)offset.X), (index.Y + (int)offset.Y), (index.Z + (int)offset.Z),
                                input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X)],
                                new Normal(
                                    input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), Math.Min((width) - 1, (index.X + (int)offset.X) + 1)] - input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), Math.Max((index.X + (int)offset.X) - 1, 0)],
                                    input[(index.Z + (int)offset.Z), Math.Min((width) - 1, (index.Y + (int)offset.Y) + 1), (index.X + (int)offset.X)] - input[(index.Z + (int)offset.Z), Math.Max((index.Y + (int)offset.Y) - 1, 0), (index.X + (int)offset.X)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z) + 1), (index.Y + (int)offset.Y), (index.X + (int)offset.X)] - input[Math.Max((index.Z + (int)offset.Z) - 1, 0), (index.Y + (int)offset.Y), (index.X + (int)offset.X)]
                                )),
                            new Point(
                                (ushort)((index.X + (int)offset.X) + 1), (index.Y + (int)offset.Y), (index.Z + (int)offset.Z),
                                input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X) + 1],
                                new Normal(
                                    input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), Math.Min((width) - 1, (index.X + (int)offset.X) + 1 + 1)] - input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), Math.Max((index.X + (int)offset.X + 1 - 1), 0)],
                                    input[(index.Z + (int)offset.Z), Math.Min((width) - 1, (index.Y + (int)offset.Y) + 1), (index.X + (int)offset.X + 1)] - input[(index.Z + (int)offset.Z), Math.Max((index.Y + (int)offset.Y) - 1, 0), (index.X + (int)offset.X + 1)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z) + 1), (index.Y + (int)offset.Y), (index.X + (int)offset.X + 1)] - input[Math.Max((index.Z + (int)offset.Z) - 1, 0), (index.Y + (int)offset.Y), (index.X + (int)offset.X + 1)]
                                )),
                            new Point(
                                (ushort)((index.X + (int)offset.X) + 1), (ushort)((index.Y + (int)offset.Y) + 1), (index.Z + (int)offset.Z),
                                input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y) + 1, (index.X + (int)offset.X) + 1],
                                new Normal(
                                    input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y + 1), Math.Min((width) - 1, (index.X + (int)offset.X) + 1 + 1)] - input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y + 1), Math.Max((index.X + (int)offset.X + 1 - 1), 0)],
                                    input[(index.Z + (int)offset.Z), Math.Min((width) - 1, (index.Y + (int)offset.Y + 1) + 1), (index.X + (int)offset.X + 1)] - input[(index.Z + (int)offset.Z), Math.Max((index.Y + (int)offset.Y + 1) - 1, 0), (index.X + (int)offset.X + 1)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z) + 1), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X + 1)] - input[Math.Max((index.Z + (int)offset.Z) - 1, 0), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X + 1)]
                                )),
                            new Point(
                                (index.X + (int)offset.X), (ushort)((index.Y + (int)offset.Y) + 1), (index.Z + (int)offset.Z),
                                input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y) + 1, (index.X + (int)offset.X)],
                                new Normal(
                                    input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y + 1), Math.Min((width) - 1, (index.X + (int)offset.X + 1))] - input[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y + 1), Math.Max((index.X + (int)offset.X - 1), 0)],
                                    input[(index.Z + (int)offset.Z), Math.Min((width) - 1, (index.Y + (int)offset.Y + 1) + 1), (index.X + (int)offset.X)] - input[(index.Z + (int)offset.Z), Math.Max((index.Y + (int)offset.Y + 1) - 1, 0), (index.X + (int)offset.X)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z) + 1), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X)] - input[Math.Max((index.Z + (int)offset.Z) - 1, 0), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X)]
                                )),
                            new Point(
                                (index.X + (int)offset.X), (index.Y + (int)offset.Y), (index.Z + 1 + (int)offset.Z),
                                input[(index.Z + 1 + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X)],
                                new Normal(
                                    input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y), Math.Min((width) - 1, (index.X + (int)offset.X) + 1)] - input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y), Math.Max((index.X + (int)offset.X) - 1, 0)],
                                    input[(index.Z + (int)offset.Z + 1), Math.Min((width) - 1, (index.Y + (int)offset.Y) + 1), (index.X + (int)offset.X)] - input[(index.Z + (int)offset.Z + 1), Math.Max((index.Y + (int)offset.Y) - 1, 0), (index.X + (int)offset.X)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z + 1) + 1), (index.Y + (int)offset.Y), (index.X + (int)offset.X)] - input[Math.Max((index.Z + (int)offset.Z + 1) - 1, 0), (index.Y + (int)offset.Y), (index.X + (int)offset.X)]
                                )),
                            new Point(
                                (ushort)((index.X + (int)offset.X) + 1), (index.Y + (int)offset.Y), (index.Z + 1 + (int)offset.Z),
                                input[(index.Z + 1 + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X) + 1],
                                new Normal(
                                    input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y), Math.Min((width) - 1, (index.X + (int)offset.X) + 1 + 1)] - input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y), Math.Max((index.X + (int)offset.X + 1 - 1), 0)],
                                    input[(index.Z + (int)offset.Z + 1), Math.Min((width) - 1, (index.Y + (int)offset.Y) + 1), (index.X + (int)offset.X + 1)] - input[(index.Z + (int)offset.Z + 1), Math.Max((index.Y + (int)offset.Y) - 1, 0), (index.X + (int)offset.X + 1)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z + 1) + 1), (index.Y + (int)offset.Y), (index.X + (int)offset.X + 1)] - input[Math.Max((index.Z + (int)offset.Z + 1) - 1, 0), (index.Y + (int)offset.Y), (index.X + (int)offset.X + 1)]
                                )),
                            new Point(
                                (ushort)((index.X + (int)offset.X) + 1), (ushort)((index.Y + (int)offset.Y) + 1), (index.Z + 1 + (int)offset.Z),
                                input[(index.Z + 1 + (int)offset.Z), (index.Y + (int)offset.Y) + 1, (index.X + (int)offset.X) + 1],
                                new Normal(
                                    input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y + 1), Math.Min((width) - 1, (index.X + (int)offset.X) + 1 + 1)] - input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y + 1), Math.Max((index.X + (int)offset.X + 1 - 1), 0)],
                                    input[(index.Z + (int)offset.Z + 1), Math.Min((width) - 1, (index.Y + (int)offset.Y + 1) + 1), (index.X + (int)offset.X + 1)] - input[(index.Z + (int)offset.Z + 1), Math.Max((index.Y + (int)offset.Y + 1) - 1, 0), (index.X + (int)offset.X + 1)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z + 1) + 1), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X + 1)] - input[Math.Max((index.Z + (int)offset.Z + 1) - 1, 0), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X + 1)]
                                )),
                            new Point(
                                (index.X + (int)offset.X), (ushort)((index.Y + (int)offset.Y) + 1), (index.Z + 1 + (int)offset.Z),
                                input[(index.Z + 1 + (int)offset.Z), (index.Y + (int)offset.Y) + 1, (index.X + (int)offset.X)],
                                new Normal(
                                    input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y + 1), Math.Min((width) - 1, (index.X + (int)offset.X + 1))] - input[(index.Z + (int)offset.Z + 1), (index.Y + (int)offset.Y + 1), Math.Max((index.X + (int)offset.X - 1), 0)],
                                    input[(index.Z + (int)offset.Z + 1), Math.Min((width) - 1, (index.Y + (int)offset.Y + 1) + 1), (index.X + (int)offset.X)] - input[(index.Z + (int)offset.Z + 1), Math.Max((index.Y + (int)offset.Y + 1) - 1, 0), (index.X + (int)offset.X)],
                                    input[Math.Min((int)input.Length / ((width) * (width)) - 1, (index.Z + (int)offset.Z + 1) + 1), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X)] - input[Math.Max((index.Z + (int)offset.Z + 1) - 1, 0), (index.Y + (int)offset.Y + 1), (index.X + (int)offset.X)]
                                ))
                            );
                Point[] vertice = tempCube.MarchGPU(threshold, triTable[edges[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X)]]);
                int i;
                //count[0]++;
                for (i = 0; i < 12; i += 3)
                {
                    if ((vertice[i].X > 0 || vertice[i].Y > 0 || vertice[i].Z > 0) ||
                        (vertice[i + 1].X > 0 || vertice[i + 1].Y > 0 || vertice[i + 1].Z > 0) ||
                        (vertice[i + 2].X > 0 || vertice[i + 2].Y > 0 || vertice[i + 2].Z > 0))
                    {

                        //triangles[triangles.Length - 1].vertex1.value = 1;
                        if (flag[0] == 0)
                            flag[0] = 1;
                        //if (triangles[(batchSize * batchSize * batchSize * 0) + (index.Z * batchSize * batchSize) + (index.Y * batchSize) + index.X].vertex1.X > 0)
                        ////if (triangles.Length < 32 * 32 * 32 * 4 + 32 * 32 * 31 + 32 * 31 + 31 + 3)
                        //{
                        //    Triangle t = triangles[-1];
                        //}
                        triangles[(batchSize * batchSize * batchSize * (i / 3)) + (index.Z * batchSize * batchSize) + (index.Y * batchSize) + index.X] = new Triangle(vertice[i], vertice[i + 1], vertice[i + 2]);
                        //count[0]++;
                    }
                }
            }
        }



        public static void MarchGPU(StreamWriter fs)
        {
            //gradConfig.MemSetToZero();
            //cubeConfig.MemSetToZero();
            ushort[] sizes = { 12, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34 };
            if (width < 200)
                sizes = new ushort[] { (ushort)width };
            foreach (ushort size in sizes)
            {
                batchSize = size;

                //var gradScope = accelerator.CreatePageLockFromPinned(normals);
                //var cubeScope = accelerator.CreatePageLockFromPinned(cubes);
                //gradConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, gradScope);
                //cubeConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);

                int i, j, k;
                int Z = slices.GetLength(0) - 1;
                int Y = width - 1;
                int X = length - 1;
                Index3D Nindex = new Index3D(X, Y, Z);
                int nX = (int)Math.Ceiling((double)(X / batchSize));
                int nY = (int)Math.Ceiling((double)(Y / batchSize));
                int nZ = (int)Math.Ceiling((double)(Z / batchSize));

                //gradConfig = accelerator.Allocate3DDenseXY<Normal>(new Index3D(X + 1, Y + 1, Z + 1));
                //cubeConfig = accelerator.Allocate3DDenseXY<Edge>(Nindex);

                Triangle[] triangleList = new Triangle[Math.Max(Nindex.Size, (nX + 1) * (nY + 1) * (nZ + 1) * batchSize * batchSize * batchSize) * 5];
                int sum = 0;
                Triangle[] tri = new Triangle[Math.Min((Nindex.X) * (Nindex.Y) * (Nindex.Z) * 5 + 1, (batchSize) * (batchSize) * (batchSize) * 5 + 1)];
                PageLockedArray1D<Triangle> triLocked = accelerator.AllocatePageLocked1D<Triangle>(Math.Min((Nindex.X) * (Nindex.Y) * (Nindex.Z) * 5 + 1, (batchSize) * (batchSize) * (batchSize) * 5 + 1));
                MemoryBuffer1D<Triangle, Stride1D.Dense> triConfig = accelerator.Allocate1D<Triangle>(Math.Min((Nindex.X) * (Nindex.Y) * (Nindex.Z) * 5 + 1, (batchSize) * (batchSize) * (batchSize) * 5 + 1));
                MemoryBuffer1D<byte, Stride1D.Dense> flag = accelerator.Allocate1D<byte>(1);
                //count = 0;
                //int[] n = { 0 };

                //gradConfig = accelerator.Allocate3DDenseXY(normals);
                cubeConfig = accelerator.Allocate3DDenseXY(cubes);
                //Edge[] r = new Edge[cubes.Length]; 
                //r = r.Where(x => x.E1 > 0).ToArray();
                //var pt = accelerator.Allocate1D<int>(n);
                int iX = 0;
                triConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, triLocked);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                for (i = nX; i >= 0; i--)
                {
                    for (j = nY; j >= 0; j--)
                    {
                        for (k = nZ; k >= 0; k--)
                        {
                            //Console.WriteLine(i + "," + j + "," + k);
                            Index3D index = (Math.Min(Nindex.X - i * batchSize, batchSize), Math.Min(Nindex.Y - j * batchSize, batchSize), Math.Min(Nindex.Z - k * batchSize, batchSize));
                            if (index.Size > 0)
                            {
                                Point offset = new Point() { X = i * batchSize, Y = j * batchSize, Z = k * batchSize };

                                //assign_normal = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(AssignNormal);
                                //assign_edges= accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<Edge, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int>(AssignEdges);

                                get_verts(index, triConfig.View, cubeConfig.View, triTable, sliced.View, flag.View, offset, threshold, batchSize, width);

                                accelerator.Synchronize();
                                if (flag.GetAsArray1D()[0] > 0)
                                {
                                    triConfig.View.CopyToPageLockedAsync(triLocked);
                                    accelerator.Synchronize();
                                    tri = triLocked.GetArray();
                                    tri[tri.Length - 1] = new Triangle();
                                    Array.Copy(tri, 0, triangleList, iX * (tri.Length - 1), tri.Length - 1);
                                    //Console.WriteLine(triangleList.Count);
                                    sum += tri.Length;
                                    triConfig.View.MemSetToZero();
                                    triLocked.ArrayView.MemSetToZero();
                                    iX++;
                                    flag.MemSetToZero();
                                }
                                //foreach(Triangle triangle in tri)
                                //{
                                //    if (!triangle.Equals(new Triangle()))
                                //        triangleList.Add(triangle);
                                //}
                                //triangleList.AddRange(tri.Where(x => !x.Equals(new Triangle())).ToList());
                                //Console.WriteLine(triangleList.Count);
                            }
                        }
                    }
                }
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                count = 0;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime:" + elapsedTime + ", Batch Size:" + batchSize);

                // Get the elapsed time as a TimeSpan value.
                triConfig.Dispose();
                cubeConfig.Dispose();
                triTable.Dispose();
                sliced.Dispose();
                //}
                //Triangle[] temp = new Triangle[sum];
                //if (triangleList.Length > sum)
                //    Array.Copy(triangleList, temp, sum);
                var triC = triangleList.Where(x => (x.vertex1.X != 0 && x.vertex1.Y != 0 && x.vertex1.Z != 0) &&
                (x.vertex2.X != 0 && x.vertex2.Y != 0 && x.vertex2.Z != 0) &&
                (x.vertex3.X != 0 && x.vertex3.Y != 0 && x.vertex3.Z != 0)).ToList();
                foreach (var triangle in triC)
                {
                    //vertices.Add(vertex);
                    fs.WriteLine("v " + triangle.vertex1.X + " " + triangle.vertex1.Y + " " + triangle.vertex1.Z);
                    fs.WriteLine("vn " + triangle.vertex1.normal.X + " " + triangle.vertex1.normal.Y + " " + triangle.vertex1.normal.Z);
                    fs.WriteLine("v " + triangle.vertex2.X + " " + triangle.vertex2.Y + " " + triangle.vertex2.Z);
                    fs.WriteLine("vn " + triangle.vertex2.normal.X + " " + triangle.vertex2.normal.Y + " " + triangle.vertex2.normal.Z);
                    fs.WriteLine("v " + triangle.vertex3.X + " " + triangle.vertex3.Y + " " + triangle.vertex3.Z);
                    fs.WriteLine("vn " + triangle.vertex3.normal.X + " " + triangle.vertex3.normal.Y + " " + triangle.vertex3.normal.Z);
                    //    //fs.WriteLine("vt " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                    //    //Point normal = Normal(slices[0], slices[1], slices[2], slices[3], vertex, k);
                    //    //fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
                    //    //Point n = new Point(vertex.normal.X * 2 + normal.X, vertex.normal.Y * 2 + normal.Y, vertex.normal.Z * 2 + normal.Z, 0);
                    //    //normals.Add(normal);
                    count += 3;
                }
            }
            //List<Point> points = new List<Point>();
            //foreach (Triangle triangle in triangleList)
            //    points.AddRange(triangle.getV());

            //foreach(Point point in points)
            //{
            //    int dup = points.Where(x => x.X == point.X && x.Y == point.Y && x.Z == point.Z).Count();
            //    break;
            //}
            //pt.Dispose();
            //var triC = tri;

        }

        public static ref MemoryBuffer2D<uint, Stride2D.DenseX> getHPLayer(int index)
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
        //public static void getVerticesX(Index3D index, ArrayView<Triangle> triangles, ArrayView3D<Normal, Stride3D.DenseXY> normals, ArrayView3D<Edge, Stride3D.DenseXY> edges, ArrayView1D<ushort, Stride1D.Dense> input, Point offset, int thresh, int batchSize, int width)
        //{
        //    ushort s = input[(index.Z) * batchSize * batchSize + (index.Y) * batchSize + index.X + 1];
        //    Normal n = normals[(index.Z + 1), (index.Y + 1), index.X + 1];
        //    Cube tempCube = new Cube(
        //                    new Point(
        //                        (index.X + (int)offset.X), (index.Y + (int)offset.Y), (index.Z + (int)offset.Z),
        //                        input[index.Z * width * width + index.Y * width + index.X],
        //                        normals[index.Z, index.Y, index.X]),
        //                    new Point(
        //                        (ushort)((index.X + (int)offset.X) + 1), (index.Y + (int)offset.Y), (index.Z + (int)offset.Z),
        //                        input[index.Z * width * width + index.Y * width + index.X + 1],
        //                        normals[index.Z, index.Y, index.X + 1]),
        //                    new Point(
        //                        (ushort)((index.X + (int)offset.X) + 1), (ushort)((index.Y + (int)offset.Y) + 1), (index.Z + (int)offset.Z),
        //                        input[index.Z * width * width + (index.Y + 1) * width + index.X + 1],
        //                        normals[index.Z, (index.Y + 1), index.X + 1]),
        //                    new Point(
        //                        (index.X + (int)offset.X), (ushort)((index.Y + (int)offset.Y) + 1), (index.Z + (int)offset.Z),
        //                        input[index.Z * width * width + (index.Y + 1) * width + index.X],
        //                        normals[index.Z, (index.Y + 1), index.X]),
        //                    new Point(
        //                        (index.X + (int)offset.X), (index.Y + (int)offset.Y), ((index.Z + (int)offset.Z) + 1),
        //                        input[(index.Z + 1) * width * width + index.Y * width + index.X],
        //                        normals[(index.Z + 1), index.Y, index.X]),
        //                    new Point(
        //                        (ushort)((index.X + (int)offset.X) + 1), (index.Y + (int)offset.Y), ((index.Z + (int)offset.Z) + 1),
        //                        input[(index.Z + 1) * width * width + index.Y * width + index.X + 1],
        //                        normals[(index.Z + 1), index.Y, index.X + 1]),
        //                    new Point(
        //                        (ushort)((index.X + (int)offset.X) + 1), (ushort)((index.Y + (int)offset.Y) + 1), ((index.Z + (int)offset.Z) + 1),
        //                        input[(index.Z + 1) * width * width + (index.Y + 1) * width + index.X + 1],
        //                        normals[(index.Z + 1), (index.Y + 1), index.X + 1]),
        //                    new Point(
        //                        (index.X + (int)offset.X), (ushort)((index.Y + (int)offset.Y) + 1), ((index.Z + (int)offset.Z) + 1),
        //                        input[(index.Z + 1) * width * width + (index.Y + 1) * width + index.X],
        //                        normals[(index.Z + 1), (index.Y + 1), index.X])
        //                    );
        //    Point[] vertice = tempCube.MarchGPU(threshold, edges[index.Z, index.Y, index.X]);
        //    int i;
        //    //count[0]++;
        //    for (i = 0; i < 12; i += 3)
        //    {
        //        if ((vertice[i].X > 0 || vertice[i].Y > 0 || vertice[i].Z > 0) ||
        //            (vertice[i + 1].X > 0 || vertice[i + 1].Y > 0 || vertice[i + 1].Z > 0) ||
        //            (vertice[i + 2].X > 0 || vertice[i + 2].Y > 0 || vertice[i + 2].Z > 0))
        //        {

        //            //triangles[triangles.Length - 1].vertex1.value = 1;
        //            //triangles[triangles.Length - 1].vertex1.value = 1;
        //            if (triangles[triangles.Length - 1].vertex1.X != -1)
        //                triangles[triangles.Length - 1].vertex1.X = -1;
        //            //if (triangles[(batchSize * batchSize * batchSize * 0) + (index.Z * batchSize * batchSize) + (index.Y * batchSize) + index.X].vertex1.X > 0)
        //            ////if (triangles.Length < 32 * 32 * 32 * 4 + 32 * 32 * 31 + 32 * 31 + 31 + 3)
        //            //{
        //            //    Triangle t = triangles[-1];
        //            //}
        //            triangles[(batchSize * batchSize * batchSize * (i / 3)) + (index.Z * batchSize * batchSize) + (index.Y * batchSize) + index.X] = new Triangle(vertice[i], vertice[i + 1], vertice[i + 2]);
        //            //count[0]++;
        //        }
        //    }
        //}
        //public static void MarchGPUX(StreamWriter fs)
        //{
        //    ushort[] sizes = { 32 };
        //    foreach (ushort size in sizes)
        //    {

        //        int i, j, k;
        //        int Z = slices.GetLength(0) - 1;
        //        int Y = width - 1;
        //        int X = length - 1;
        //        Index3D Nindex = new Index3D(X, Y, Z);
        //        Index3D Gindex = new Index3D(X + 1, Y + 1, sliceSize + 1);
        //        int nZ = (int)Math.Ceiling((double)(Z / sliceSize));
        //        batchSize = size;
        //        List<Triangle> triangleList = new List<Triangle>();

        //        Triangle[] tri = new Triangle[Math.Min(Nindex.Size * 5 + 1, X * Y * sliceSize * 5 + 1)];
        //        PageLockedArray1D<Triangle> triLocked = accelerator.AllocatePageLocked1D<Triangle>(Math.Min(Nindex.Size * 5 + 1, width * width * sliceSize * 5 + 1));
        //        MemoryBuffer1D<Triangle, Stride1D.Dense> triConfig = accelerator.Allocate1D<Triangle>(Math.Min(Nindex.Size * 5 + 1, width * width * sliceSize * 5 + 1));

        //        gradConfig = accelerator.Allocate1D<Normal>((width) * (width) * (Math.Min(sliceSize, Z) + 1));
        //        cubeConfig = accelerator.Allocate1D<Edge>(Math.Min(Nindex.Size, X * Y * sliceSize));
        //        slicedConfig = accelerator.Allocate1D<ushort>((width) * (width) * (Math.Min(sliceSize, Z) + 1));


        //        //count = 0;
        //        //int[] n = { 0 };

        //        //var gradConfig = accelerator.Allocate1D(normals);
        //        //var cubeConfig = accelerator.Allocate1D(cubes);
        //        //Edge[] r = new Edge[cubes.Length]; 
        //        //r = r.Where(x => x.E1 > 0).ToArray();
        //        //var pt = accelerator.Allocate1D<int>(n);
        //        triConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, triLocked);
        //        Stopwatch stopWatch = new Stopwatch();
        //        stopWatch.Start();
        //        for (i = 0; i <= nZ; i++)
        //        {
        //            //Console.WriteLine(i + "," + j + "," + k);
        //            Index3D index = new Index3D(X, Y, Math.Min(Nindex.Z - i * sliceSize, sliceSize));
        //            if (index.Size < 1) break;
        //            Gindex = new Index3D(X + 1, Y + 1, Math.Min(sliceSize, Nindex.Z - i * sliceSize + 1));
        //            Normal[] normslices = new Normal[gradConfig.Length];
        //            Edge[] cubeslices = new Edge[cubeConfig.Length];
        //            ushort[] subslice = new ushort[slicedConfig.Length];
        //            Point offset = new Point(i * sliceSize, 0, 0, 0, new Normal());

        //            Array.Copy(normals, i * Gindex.Size, normslices, 0, Gindex.Size);
        //            Array.Copy(cubes, i * index.Size, cubeslices, 0, index.Size);

        //            Array.Copy(slices1D, i * (sliceSize) * (width) * (width), subslice, 0, Math.Min(slices1D.Length - i * (sliceSize) * width * width, subslice.Length));


        //            var gradScope = accelerator.CreatePageLockFromPinned(normslices);
        //            var cubeScope = accelerator.CreatePageLockFromPinned(cubeslices);

        //            for (int c = 1; c < cubes.Length; c++)
        //            {
        //                ushort[] c1 = cubes[c].getAsArray();
        //                ushort[] c2 = cubeslices[c].getAsArray();
        //                for (int e = 0; e < 12; e++)
        //                {
        //                    if (c1[e] != c2[e])
        //                        ;
        //                }
        //            }

        //            for (int c = 1; c < cubes.Length; c++)
        //            {
        //                if (!slices1D[c].Equals(subslice[c]) || !normslices[c].Equals(normals[c]))
        //                {
        //                    ;
        //                }
        //            }
        //            var sliceScope = accelerator.CreatePageLockFromPinned<ushort>(subslice);
        //            gradConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, gradScope);
        //            cubeConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);
        //            slicedConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, sliceScope);
        //            accelerator.Synchronize();
        //            //assign_normal = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(AssignNormal);
        //            //assign_edges= accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<Edge, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int>(AssignEdges);

        //            get_vertsX(index, triConfig.View, gradConfig.View, cubeConfig.View, slicedConfig.View, offset, threshold, sliceSize, batchSize);

        //            accelerator.Synchronize();
        //            //gradConfig.CopyToCPU(grads);
        //            //cubeConfig.CopyToCPU(cubeBytes);
        //            //cubeConfig.CopyToCPU(r);
        //            triConfig.View.CopyToPageLockedAsync(triLocked);
        //            accelerator.Synchronize();
        //            tri = triLocked.GetArray();
        //            //sum = 0;
        //            //foreach (Triangle cube in tri)
        //            //{
        //            //    foreach (Point n in cube.getV())
        //            //    {
        //            //        if (!n.Equals(new Point()))
        //            //        {
        //            //            sum = tri.ToList().IndexOf(cube);
        //            //            break;
        //            //        }
        //            //    }
        //            //    if (sum > 0)
        //            //        break;
        //            //}
        //            if (tri[tri.Length - 1].vertex1.value == 1)
        //            {
        //                tri[tri.Length - 1] = new Triangle();
        //                triangleList.AddRange(tri);

        //                triConfig.MemSetToZero();
        //                triLocked.ArrayView.MemSetToZero();
        //            }
        //            accelerator.Synchronize();
        //            //cubeScope.Dispose();
        //            //foreach(Triangle triangle in tri)
        //            //{
        //            //    if (!triangle.Equals(new Triangle()))
        //            //        triangleList.Add(triangle);
        //            //}
        //            //triangleList.AddRange(tri.Where(x => !x.Equals(new Triangle())).ToList());
        //            //Console.WriteLine(triangleList.Count);
        //        }
        //        stopWatch.Stop();
        //        ts = stopWatch.Elapsed;
        //        count = 0;
        //        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
        //            ts.Hours, ts.Minutes, ts.Seconds,
        //            ts.Milliseconds / 10);
        //        Console.WriteLine("RunTime:" + elapsedTime + ", Slice Size:" + sliceSize);

        //        // Get the elapsed time as a TimeSpan value.
        //        triConfig.Dispose();
        //        cubeConfig.Dispose();
        //        gradConfig.Dispose();
        //        sliced.Dispose();
        //        //}
        //        var triC = triangleList.Where(x => !x.Equals(new Triangle())).ToList();
        //        foreach (var triangle in triC)
        //        {
        //            //vertices.Add(vertex);
        //            fs.WriteLine("v " + triangle.vertex1.X + " " + triangle.vertex1.Y + " " + triangle.vertex1.Z);
        //            fs.WriteLine("vn " + triangle.vertex1.normal.X + " " + triangle.vertex1.normal.Y + " " + triangle.vertex1.normal.Z);
        //            fs.WriteLine("v " + triangle.vertex2.X + " " + triangle.vertex2.Y + " " + triangle.vertex2.Z);
        //            fs.WriteLine("vn " + triangle.vertex2.normal.X + " " + triangle.vertex2.normal.Y + " " + triangle.vertex2.normal.Z);
        //            fs.WriteLine("v " + triangle.vertex3.X + " " + triangle.vertex3.Y + " " + triangle.vertex3.Z);
        //            fs.WriteLine("vn " + triangle.vertex3.normal.X + " " + triangle.vertex3.normal.Y + " " + triangle.vertex3.normal.Z);
        //            //    //fs.WriteLine("vt " + vertex.X + " " + vertex.Y + " " + vertex.Z);
        //            //    //Point normal = Normal(slices[0], slices[1], slices[2], slices[3], vertex, k);
        //            //    //fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
        //            //    //Point n = new Point(vertex.normal.X * 2 + normal.X, vertex.normal.Y * 2 + normal.Y, vertex.normal.Z * 2 + normal.Z, 0);
        //            //    //normals.Add(normal);
        //            count += 3;
        //        }
        //    }
        //    //List<Point> points = new List<Point>();
        //    //foreach (Triangle triangle in triangleList)
        //    //    points.AddRange(triangle.getV());

        //    //foreach(Point point in points)
        //    //{
        //    //    int dup = points.Where(x => x.X == point.X && x.Y == point.Y && x.Z == point.Z).Count();
        //    //    break;
        //    //}
        //    //pt.Dispose();
        //    //var triC = tri;

        //}

        //public static void MarchGPUBatchRobust(StreamWriter fs)
        //{
        //    ushort[] sizes = { 127 };
        //    foreach (ushort size in sizes)
        //    {
        //        batchSize = size;

        //        //gradConfig = accelerator.Allocate1D(normals);
        //        //cubeConfig = accelerator.Allocate1D(cubes);
        //        //var gradScope = accelerator.CreatePageLockFromPinned(normals);
        //        //var cubeScope = accelerator.CreatePageLockFromPinned(cubes);
        //        //gradConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, gradScope);
        //        //cubeConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);

        //        int i, j, k;
        //        int Z = slices.GetLength(0) - 1;
        //        int Y = width - 1;
        //        int X = length - 1;
        //        Index3D Nindex = new Index3D(X, Y, Z);
        //        Index3D Gindex = new Index3D(batchSize + 1, batchSize + 1, batchSize + 1);
        //        int nX = (int)Math.Ceiling((double)(X / batchSize));
        //        int nY = (int)Math.Ceiling((double)(Y / batchSize));
        //        int nZ = (int)Math.Ceiling((double)(Z / batchSize));
        //        List<Triangle> triangleList = new List<Triangle>();
        //        gradConfig = accelerator.Allocate1D<Normal>(Math.Min(Gindex.Size, (batchSize + 1) * (batchSize + 1) * (batchSize + 1) + 1));
        //        cubeConfig = accelerator.Allocate1D<Edge>(Math.Min(Nindex.Size, batchSize * batchSize * batchSize));
        //        slicedConfig = accelerator.Allocate1D<ushort>(Math.Min(Gindex.Size, (batchSize + 1) * (batchSize + 1) * (batchSize + 1)));
        //        //Normal[] normslices = new Normal[Gindex.Size];
        //        //Edge[] cubeslices = new Edge[batchSize * batchSize * batchSize];
        //        //ushort[] subslice = new ushort[(Gindex.Size)];


        //        Triangle[] tri = new Triangle[Math.Min(Nindex.Size * 5 + 1, batchSize * batchSize * batchSize * 5 + 1)];
        //        PageLockedArray1D<Triangle> triLocked = accelerator.AllocatePageLocked1D<Triangle>(Math.Min(Nindex.Size * 5 + 1, batchSize * batchSize * batchSize * 5 + 1));
        //        MemoryBuffer1D<Triangle, Stride1D.Dense> triConfig = accelerator.Allocate1D<Triangle>(Math.Min(Nindex.Size * 5 + 1, batchSize * batchSize * batchSize * 5 + 1));
        //        //count = 0;
        //        //int[] n = { 0 };

        //        //var gradConfig = accelerator.Allocate1D(normals);
        //        //var cubeConfig = accelerator.Allocate1D(cubes);
        //        //Edge[] r = new Edge[cubes.Length]; 
        //        //r = r.Where(x => x.E1 > 0).ToArray();
        //        //var pt = accelerator.Allocate1D<int>(n);

        //        //triConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, triLocked);
        //        Stopwatch stopWatch = new Stopwatch();
        //        stopWatch.Start();
        //        for (i = 0; i <= nX; i++)
        //        {
        //            for (j = 0; j <= nY; j++)
        //            {
        //                for (k = 0; k <= nZ; k++)
        //                {
        //                    Normal[] normslices = new Normal[(batchSize + 1) * (batchSize + 1) * (batchSize + 1)];
        //                    Edge[] cubeslices = new Edge[batchSize * batchSize * batchSize];
        //                    ushort[] subslice = new ushort[(batchSize + 1) * (batchSize + 1) * (batchSize + 1)];
        //                    //Console.WriteLine(i + "," + j + "," + k);
        //                    Index3D index = (Math.Min(Nindex.X - i * batchSize, batchSize), Math.Min(Nindex.Y - j * batchSize, batchSize), Math.Min(Nindex.Z - k * batchSize, batchSize));
        //                    if (index.Size < 1) break;
        //                    Gindex = new Index3D(Math.Min(Nindex.X - i * batchSize, batchSize) + 1, Math.Min(Nindex.Y - j * batchSize, batchSize) + 1, Math.Min(Nindex.Z - k * batchSize, batchSize) + 1);
        //                    Point offset = new Point() { X = i * batchSize, Y = j * batchSize, Z = k * batchSize };
        //                    int Nk, Nj;
        //                    for(Nk = 0; Nk < Gindex.Z; Nk++)
        //                    {
        //                        for(Nj = 0; Nj < Gindex.Y; Nj++)
        //                        {
        //                            Array.Copy(slices1D, (int)(((offset.Z + Nk) * (Nindex.Y + 1) * (Nindex.X + 1)) + (offset.Y + Nj) * (Nindex.X + 1) + offset.X), subslice, Nk * (Gindex.Y) * (Gindex.X) + Nj * (Gindex.X), Gindex.X);
        //                            Array.Copy(normals, (int)(((offset.Z + Nk) * (Nindex.Y + 1) * (Nindex.X + 1)) + (offset.Y + Nj) * (Nindex.X + 1) + offset.X), normslices, Nk * (Gindex.Y) * (Gindex.X) + Nj * (Gindex.X), Gindex.X);
        //                            if (Nj < index.Y && Nk < index.Z)
        //                            {
        //                                Array.Copy(cubes, (int)(((offset.Z + Nk) * (Nindex.Y) * (Nindex.X)) + (offset.Y + Nj) * (Nindex.X) + offset.X), cubeslices, Nk * (index.Y) * (index.X) + Nj * (index.X), index.X);
        //                            }
        //                        }
        //                    }
        //                    //Array.Copy(normals, i * Gindex.Size, normslices, 0, Gindex.Size);
        //                    //Array.Copy(cubes, i * Gindex.Size, cubeslices, 0, Gindex.Size);

        //                    //Array.Copy(slices1D, i * (sliceSize) * (width) * (width), subslice, 0, Math.Min(slices1D.Length - i * (sliceSize) * width * width, subslice.Length));

        //                    //slicedConfig.View.CopyFromCPU(accelerator.DefaultStream, subslice);
        //                    //cubeConfig.View.CopyFromCPU(accelerator.DefaultStream, cubeslices);
        //                    //gradConfig.View.CopyFromCPU(accelerator.DefaultStream, normslices);
        //                    var gradScope = accelerator.CreatePageLockFromPinned(normslices);
        //                    var cubeScope = accelerator.CreatePageLockFromPinned(cubeslices);
        //                    var sliceScope = accelerator.CreatePageLockFromPinned<ushort>(subslice);
        //                    gradConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, gradScope);
        //                    cubeConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);
        //                    slicedConfig.View.CopyFromPageLockedAsync(accelerator.DefaultStream, sliceScope);
        //                    accelerator.Synchronize();

        //                    for (int c = 1; c < cubes.Length; c++)
        //                    {
        //                        ushort[] c1 = cubes[c].getAsArray();
        //                        ushort[] c2 = cubeslices[c].getAsArray();
        //                        for (int e = 0; e<12;e++)
        //                        {
        //                            if(c1[e] != c2[e])
        //                                ;
        //                        }
        //                    }

        //                    for (int c = 1; c < cubes.Length; c++)
        //                    {
        //                        if (!slices1D[c].Equals(subslice[c]) || !normslices[c].Equals(normals[c]))
        //                        {
        //                            ;
        //                        }
        //                    }
        //                    //assign_normal = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<Normal, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(AssignNormal);
        //                    //assign_edges= accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<Edge, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int>(AssignEdges);

        //                    get_vertsX(index, triConfig.View, gradConfig.View, cubeConfig.View, slicedConfig.View, offset, threshold, batchSize, batchSize + 1);

        //                    accelerator.Synchronize();
        //                    //gradConfig.CopyToCPU(grads);
        //                    //cubeConfig.CopyToCPU(cubeBytes);
        //                    //cubeConfig.CopyToCPU(r);
        //                    triConfig.View.CopyToPageLockedAsync(triLocked);
        //                    accelerator.Synchronize();
        //                    tri = triLocked.GetArray();
        //                    if (tri[tri.Length - 1].vertex1.value == 1)
        //                    {
        //                        triangleList.AddRange(tri);
        //                        triConfig.MemSetToZero();
        //                        triLocked.ArrayView.MemSetToZero();
        //                    }
        //                    sliceScope.Dispose();
        //                    cubeScope.Dispose();
        //                    gradScope.Dispose();

        //                    accelerator.Synchronize();
        //                    //foreach(Triangle triangle in tri)
        //                    //{
        //                    //    if (!triangle.Equals(new Triangle()))
        //                    //        triangleList.Add(triangle);
        //                    //}
        //                    //triangleList.AddRange(tri.Where(x => !x.Equals(new Triangle())).ToList());
        //                    //Console.WriteLine(triangleList.Count);
        //                }
        //            }
        //        }
        //        stopWatch.Stop();
        //        ts = stopWatch.Elapsed;
        //        count = 0;
        //        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
        //            ts.Hours, ts.Minutes, ts.Seconds,
        //            ts.Milliseconds / 10);
        //        Console.WriteLine("RunTime:" + elapsedTime + ", Batch Size:" + batchSize);

        //        // Get the elapsed time as a TimeSpan value.
        //        triConfig.Dispose();
        //        cubeConfig.Dispose();
        //        gradConfig.Dispose();
        //        sliced.Dispose();
        //        //}
        //        var triC = triangleList.Where(x => !x.Equals(new Triangle())).ToList();
        //        foreach (var triangle in triC)
        //        {
        //            //vertices.Add(vertex);
        //            fs.WriteLine("v " + triangle.vertex1.X + " " + triangle.vertex1.Y + " " + triangle.vertex1.Z);
        //            fs.WriteLine("vn " + triangle.vertex1.normal.X + " " + triangle.vertex1.normal.Y + " " + triangle.vertex1.normal.Z);
        //            fs.WriteLine("v " + triangle.vertex2.X + " " + triangle.vertex2.Y + " " + triangle.vertex2.Z);
        //            fs.WriteLine("vn " + triangle.vertex2.normal.X + " " + triangle.vertex2.normal.Y + " " + triangle.vertex2.normal.Z);
        //            fs.WriteLine("v " + triangle.vertex3.X + " " + triangle.vertex3.Y + " " + triangle.vertex3.Z);
        //            fs.WriteLine("vn " + triangle.vertex3.normal.X + " " + triangle.vertex3.normal.Y + " " + triangle.vertex3.normal.Z);
        //            //    //fs.WriteLine("vt " + vertex.X + " " + vertex.Y + " " + vertex.Z);
        //            //    //Point normal = Normal(slices[0], slices[1], slices[2], slices[3], vertex, k);
        //            //    //fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
        //            //    //Point n = new Point(vertex.normal.X * 2 + normal.X, vertex.normal.Y * 2 + normal.Y, vertex.normal.Z * 2 + normal.Z, 0);
        //            //    //normals.Add(normal);
        //            count += 3;
        //        }
        //    }
        //    //List<Point> points = new List<Point>();
        //    //foreach (Triangle triangle in triangleList)
        //    //    points.AddRange(triangle.getV());

        //    //foreach(Point point in points)
        //    //{
        //    //    int dup = points.Where(x => x.X == point.X && x.Y == point.Y && x.Z == point.Z).Count();
        //    //    break;
        //    //}
        //    //pt.Dispose();
        //    //var triC = tri;

        //}


        public static Normal[] ToBuffer(Normal[,,] buffer3D, int w, int h)
        {
            Normal[] buffer = new Normal[w * h * 1];
            Buffer.BlockCopy(buffer3D, 0, buffer, 0, w * h * System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal)));
            return buffer;
        }

        public static Edge[] ToBuffer(Edge[,,] buffer3D, int w, int h)
        {
            Edge[] buffer = new Edge[w * h * 1];
            Buffer.BlockCopy(buffer3D, 0, buffer, 0, w * h * System.Runtime.InteropServices.Marshal.SizeOf(typeof(Edge)));
            return buffer;
        }

        public static Edge[,,] ToBuffer3D(Edge[] buffer, int h, int w, int l)
        {
            Edge[,,] buff3D = new Edge[h, w, l];
            Buffer.BlockCopy(buffer, 0, buff3D, 0, h * w * l * System.Runtime.InteropServices.Marshal.SizeOf(typeof(Edge)));
            return buff3D;
        }


        public static Normal[,,] ToBuffer3D(Normal[] buffer, int h, int w, int l)
        {
            Normal[,,] buff3D = new Normal[h, w, l];
            Buffer.BlockCopy(buffer, 0, buff3D, 0, h * w * l * System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal)));
            return buff3D;
        }

        private static void CreateBmp(DicomFile dicom, int k)
        {
            var decomp = dicom.Clone();

            var header = DicomPixelData.Create(decomp.Dataset);
            var h = header.GetFrame(0);

            GrayscalePixelDataU16 pixelData = new GrayscalePixelDataU16(header.Width, header.Width, header.BitDepth, h);

            ushort[,] pixArray = new ushort[pixelData.Width, pixelData.Width];

            List<byte> color = new List<byte>();
            //var pixelData = PixelDataFactory.Create(dicom.PixelData, 0); // returns IPixelData type


            if (pixelData is GrayscalePixelDataU16)
            {

                //Context context = Context.CreateDefault();
                ////Accelerator accelerator;
                //Accelerator accelerator = context.CreateCPUAccelerator(0);
                //var loadedKernel = accelerator.LoadAutoGroupedStreamKernel(
                //(Index2D i, ArrayView<uushort> data, ArrayView2D<ushort, Stride2D.DenseX> output, int w) =>
                //{
                //    output[i] = (ushort)data[i.X * w + i.Y];
                //});

                //var tempOut = accelerator.Allocate2DDenseX<ushort>(new LongIndex2D(pixelData.Width, pixelData.Height));
                //var tempView = accelerator.Allocate1D(pixelData.Data);
                //loadedKernel(new Index2D(pixelData.Width, pixelData.Height), tempView.View, tempOut, width);

                for (int i = 0; i < pixelData.Width; i++)
                {
                    for (int j = 0; j < pixelData.Height; j++)
                    {
                        int index = j * header.Width + i;
                        slices[k, j, i] = (ushort)pixelData.Data[index];
                        //if (pixelData.GetPixel(j, i) > threshold) color.Add(255);
                        //else color.Add(0);
                        //color.Add(color.Last());
                        //color.Add(color.Last());
                    }
                }

                //accelerator.DefaultStream.Synchronize();
                //var l = tempOut.GetAsArray2D();

                //accelerator.Dispose();
                //context.Dispose();
                //pixArray = l;
            }
        }


        //public static Vertex Interpolate(Vertex v1, Vertex v2, double interpolant)
        //{

        //}

        //public static Point[] March(ushort threshold, byte config)
        //{
        //    int i, j, k;
        //    for (k = 0; k < cubes.GetLength(0) - 2; k++)
        //    {
        //        for (i = 0; i < cubes.GetLength(2) - 2; i++)
        //        {
        //            for (j = 0; j < cubes.GetLength(1) - 2; j++)
        //            {

        //                ushort[] ed = edges[k, j, i].getAsArray().Where(x => x >= 0).ToArray();
        //                Vertex[] points = new Vertex[ed.Length];
        //                for (i = 0; i < ed.Length; i++)
        //                {
        //                    switch (ed[i])
        //                    {
        //                        case (int)edgeMask.e1:
        //                            points[i] = V1.Interpolate(V2, threshold);
        //                            //points.Add(new Point3D(i + 0.5f, j, k));
        //                            break;
        //                        case (int)edgeMask.e2:
        //                            points[i] = V2.Interpolate(V3, threshold);
        //                            //points.Add(new Point3D(i + 1, j + 0.5f, k));
        //                            break;
        //                        case (int)edgeMask.e3:
        //                            points[i] = V4.Interpolate(V3, threshold);
        //                            //points.Add(new Point3D(i + 0.5f, j + 1, k));
        //                            break;
        //                        case (int)edgeMask.e4:
        //                            points[i] = V1.Interpolate(V4, threshold);
        //                            //points.Add(new Point3D(i, j + 0.5f, k));
        //                            break;
        //                        case (int)edgeMask.e5:
        //                            points[i] = V5.Interpolate(V6, threshold);
        //                            //points.Add(new Point3D(i + 0.5f, j, k + 1));
        //                            break;
        //                        case (int)edgeMask.e6:
        //                            points[i] = V6.Interpolate(V7, threshold);
        //                            //points.Add(new Point3D(i + 1, j + 0.5f, k + 1));
        //                            break;
        //                        case (int)edgeMask.e7:
        //                            points[i] = V8.Interpolate(V7, threshold);
        //                            //points.Add(new Point3D(i + 0.5f, j + 1, k + 1));
        //                            break;
        //                        case (int)edgeMask.e8:
        //                            points[i] = V5.Interpolate(V8, threshold);
        //                            //points.Add(new Point3D(i, j + 0.5f, k + 1));
        //                            break;
        //                        case (int)edgeMask.e9:
        //                            points[i] = V1.Interpolate(V5, threshold);
        //                            //points.Add(new Point3D(i, j, k+0.5f));
        //                            break;
        //                        case (int)edgeMask.e10:
        //                            points[i] = V2.Interpolate(V6, threshold);
        //                            //points.Add(new Point3D(i + 1, j, k+0.5f));
        //                            break;
        //                        case (int)edgeMask.e11:
        //                            points[i] = V3.Interpolate(V7, threshold);
        //                            //points.Add(new Point3D(i + 1, j + 1, k+0.5f));
        //                            break;
        //                        case (int)edgeMask.e12:
        //                            points[i] = V4.Interpolate(V8, threshold);
        //                            //points.Add(new Point3D(i, j + 1, k+0.5f));
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return points;
        //}

        //public static ushort[] GetRow(ushort[,] matrix, int rowNumber)
        //{
        //    return Enumerable.Range(0, matrix.GetLength(1))
        //            .Select(x => matrix[rowNumber, x])
        //            .ToArray();
        //}

        //private static Point Normal(ushort[,] slice1, ushort[,] slice2, ushort[,] slice3, ushort[,] slice4, Point point, ushort index)
        //{
        //    Point vertex = point;
        //    double factor, interpolant;
        //    vertex.Z -= (index - 1);
        //    //Point v1x1,v1x2,v1y1,v1y2,v1z1,v1z2, v2x1, v2x2, v2y1, v2y2, v2z1, v2z2;
        //    Point voxel1 = vertex, voxel2;
        //    Point normal1 = new Point(0,0,0,0), normal2;
        //    if (vertex.X % 1 != 0)
        //    {
        //        xCount++;
        //        voxel1 = new Point((int)vertex.X, vertex.Y, vertex.Z, slices[(int)vertex.Z, (int)vertex.Y, (int)vertex.X]);
        //        voxel2 = new Point((int)vertex.X + 1, vertex.Y, vertex.Z, slices[(int)vertex.Z, (int)vertex.Y, (int)vertex.X + 1]);
        //        interpolant = vertex.X - (int)vertex.X;
        //    }
        //    else if (vertex.Y % 1 != 0)
        //    {
        //        yCount++;
        //        voxel1 = new Point(vertex.X, (int)vertex.Y, vertex.Z, slices[(int)vertex.Z, (int)vertex.Y, (int)vertex.X]);
        //        voxel2 = new Point(vertex.X, (int)vertex.Y + 1, vertex.Z, slices[(int)vertex.Z, (int)vertex.Y + 1, (int)vertex.X]);
        //        interpolant = vertex.Y - (int)vertex.Y;
        //    }
        //    else if (vertex.Z % 1 != 0)
        //    {
        //        zCount++;
        //        voxel1 = new Point(vertex.X, vertex.Y, (int)vertex.Z, slices[(int)vertex.Z, (int)vertex.Y, (int)vertex.X]);
        //        voxel2 = new Point(vertex.X, vertex.Y, (int)vertex.Z + 1, slices[(int)vertex.Z + 1, (int)vertex.Y, (int)vertex.X]);
        //        interpolant = vertex.Z - (int)vertex.Z;
        //    }
        //    else
        //    {
        //        lCount++;
        //        if (voxel1.X == 0)
        //            normal1.X = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X])/2;
        //        else if (voxel1.X == length - 1)
        //            normal1.X = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X - 1])/2;
        //        else
        //            normal1.X = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X - 1]);

        //        if (voxel1.Y == 0)
        //            normal1.Y = (slices[(int)voxel1.Z, (int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X])/2;
        //        else if (voxel1.Y == length - 1)
        //            normal1.Y = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y - 1, (int)voxel1.X])/2;
        //        else
        //            normal1.Y = (slices[(int)voxel1.Z, (int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y - 1, (int)voxel1.X]);

        //        if (voxel1.Z == 0)
        //            normal1.Z = (slices[(int)voxel1.Z + 1, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X])/2;
        //        else if (voxel1.Z == length - 1)
        //            normal1.Z = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1, (int)voxel1.Y, (int)voxel1.X])/2;
        //        else
        //            normal1.Z = (slices[(int)voxel1.Z + 1, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1, (int)voxel1.Y, (int)voxel1.X]);
        //        factor = 1 / Math.Sqrt(Math.Pow(normal1.X, 2) + Math.Pow(normal1.Y, 2) + Math.Pow(normal1.Z, 2));
        //        return normal1 * factor;
        //    }
        //    normal1 = new Point(0, 0, 0, voxel1.value);
        //    normal2 = new Point(0, 0, 0, voxel2.value);

        //    if (voxel1.X == 0)
        //        normal1.X = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X])/2;
        //    else if (voxel1.X == length - 1)
        //        normal1.X = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X - 1])/2;
        //    else
        //        normal1.X = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X - 1]);

        //    if (voxel1.Y == 0)
        //        normal1.Y = (slices[(int)voxel1.Z, (int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X])/2;
        //    else if (voxel1.Y == length - 1)
        //        normal1.Y = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y - 1, (int)voxel1.X])/2;
        //    else
        //        normal1.Y = (slices[(int)voxel1.Z, (int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y - 1, (int)voxel1.X]);

        //    if (voxel1.Z == 0)
        //        normal1.Z = (slices[(int)voxel1.Z + 1, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z, (int)voxel1.Y, (int)voxel1.X])/2;
        //    else if (voxel1.Z == length - 1)
        //        normal1.Z = (slices[(int)voxel1.Z, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1, (int)voxel1.Y, (int)voxel1.X])/2;
        //    else
        //        normal1.Z = (slices[(int)voxel1.Z + 1, (int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1, (int)voxel1.Y, (int)voxel1.X]);

        //    if (voxel2.X == 0)
        //        normal2.X = (slices[(int)voxel2.Z, (int)voxel2.Y, (int)voxel2.X + 1] - slices[(int)vertex.Z, (int)voxel2.Y, (int)voxel2.X])/2;
        //    else if (voxel2.X == length - 1)
        //        normal2.X = (slices[(int)voxel2.Z, (int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z, (int)voxel2.Y, (int)voxel2.X - 1])/2;
        //    else
        //        normal2.X = (slices[(int)voxel2.Z, (int)voxel2.Y, (int)voxel2.X + 1] - slices[(int)vertex.Z, (int)voxel2.Y, (int)voxel2.X - 1]);

        //    if (voxel2.Y == 0)
        //        normal2.Y = (slices[(int)voxel2.Z, (int)voxel2.Y + 1, (int)voxel2.X] - slices[(int)vertex.Z, (int)voxel2.Y, (int)voxel2.X])/2;
        //    else if (voxel2.Y == length - 1)
        //        normal2.Y = (slices[(int)voxel2.Z, (int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z, (int)voxel2.Y - 1, (int)voxel2.X])/2;
        //    else
        //        normal2.Y = (slices[(int)voxel2.Z, (int)voxel2.Y + 1, (int)voxel2.X] - slices[(int)vertex.Z, (int)voxel2.Y - 1, (int)voxel2.X]);

        //    if (voxel2.Z == 0)
        //        normal2.Z = (slices[(int)voxel2.Z + 1, (int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z, (int)voxel2.Y, (int)voxel2.X])/2;
        //    else if (voxel2.Z == length - 1)
        //        normal2.Z = (slices[(int)voxel2.Z, (int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z - 1, (int)voxel2.Y, (int)voxel2.X])/2;
        //    else
        //        normal2.Z = (slices[(int)voxel2.Z + 1, (int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z - 1, (int)voxel2.Y, (int)voxel2.X]);
        //    var t = slices[(int)voxel2.Z + 1, (int)voxel2.Y, (int)voxel2.X];
        //    var l = slices[(int)vertex.Z - 1, (int)voxel2.Y, (int)voxel2.X];
        //    //v1x1 = new Point(voxel1.X -1, voxel1.Y, voxel1.Z, slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X - 1]);

        //    Point p = Interpolate(normal2, normal1, interpolant);
        //    if (double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsNaN(p.Z))
        //        ;
        //    var j = (vertex - new Point(64,64,64,0)) * (1 / Math.Sqrt(Math.Pow(vertex.X - 64, 2) + Math.Pow(vertex.Y - 64, 2) + Math.Pow(vertex.Z - 64, 2)));
        //    factor = 1/Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2));
        //    var m = p * factor;
        //    return p*factor;
        //}

        //public static Point Interpolate(Point pt2, Point pt1, double interpolant)
        //{
        //    double x = (pt1.X - pt2.X) * interpolant + pt2.X;
        //    double y = (pt1.Y - pt2.Y) * interpolant + pt2.Y;
        //    double z = (pt1.Z - pt2.Z) * interpolant + pt2.Z;

        //    return new Point(x, y, z, 0);
        //}


        //private static Bitmap array2Bitmap(ushort[,] pixArray)
        //{

        //    int res = 96;
        //    int stride = 3 * pixArray.GetLength(1);
        //    List<byte> color = new List<byte>();
        //    //var t =color.Max(); 
        //    int length = color.Count();

        //    for (int i = 0; i < pixArray.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < pixArray.GetLength(1); j++)
        //        {
        //            if (pixArray[j, i] > threshold) color.Add(255);
        //            else color.Add(0);
        //            color.Add(color.Last());
        //            color.Add(color.Last());
        //        }
        //    }

        //    byte[] array = color.ToArray();
        //    byte[] bytes = array;
        //    //Buffer.BlockCopy(array, 0, bytes, 0, array.Length * 2);
        //    // Initialize unmanaged memory to hold the array.
        //    int size = System.Runtime.InteropServices.Marshal.SizeOf(array[0]) * array.Length;
        //    IntPtr pnt = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
        //    //Copy the pixel data into the bitmap structure

        //    Bitmap source = new Bitmap(pixArray.GetLength(1),
        //                                pixArray.GetLength(1),
        //                                stride,
        //                                PixelFormat.Format24bppRgb,
        //                                pnt);




        //    var wb = new WriteableBitmap(source.Width, source.Height, res, res, System.Windows.Media.PixelFormats.Rgb24, null);
        //    wb.WritePixels(new Int32Rect(0, 0, source.Width, source.Height), array, source.Height * 3, 0);

        //    //byte[] outbytes = Convert16BitGrayScaleToRgb48(bytes, source.Width, source.Height);
        //    Rectangle dimension = new Rectangle(0, 0, source.Width, source.Height);
        //    BitmapData picData = source.LockBits(dimension, ImageLockMode.ReadWrite, source.PixelFormat);
        //    //IntPtr pixelStartAddress = picData.Scan0;

        //    //IntPtr pnt = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
        //    //Copy the pixel data into the bitmap structure
        //    System.Runtime.InteropServices.Marshal.Copy(bytes, 0, pnt, bytes.Length);
        //    source.UnlockBits(picData);
        //    return source;
        //}

        //private static (byte[,] cubeConfigs, int[,] edges) MarchingCubes(int[,] slice1, int[,] slice2, int index)
        //{
        //    byte[,] cubeBytes = new byte[length, width];
        //    //byte[,] configBytes = new byte[511, 511];
        //    int[,] edges = new int[length, width];

        //    //bit order 
        //    // i,j,k 
        //    // i+1,j,k
        //    // i+1,j+1,k
        //    // i,j+1,k
        //    // i,j,k+1
        //    // i+1,j,k+1
        //    // i+1,j+1,k+1
        //    // i,j+1,k+1

        //    for (int i = 0; i < slice1.GetLength(1) - 1; i++)
        //    {
        //        for (int j = 0; j < slice1.GetLength(1) - 1; j++)
        //        {

        //            cubeBytes[j, i] = 0;
        //            cubeBytes[j, i] += (slice1[j, i] < threshold) ? (byte)bitMask.v1 : (byte)0;
        //            cubeBytes[j, i] += (slice1[j, i + 1] < threshold) ? (byte)bitMask.v2 : (byte)0;
        //            cubeBytes[j, i] += (slice1[j + 1, i + 1] < threshold) ? (byte)bitMask.v3 : (byte)0;
        //            cubeBytes[j, i] += (slice1[j + 1, i] < threshold) ? (byte)bitMask.v4 : (byte)0;
        //            cubeBytes[j, i] += (slice2[j, i] < threshold) ? (byte)bitMask.v5 : (byte)0;
        //            cubeBytes[j, i] += (slice2[j, i + 1] < threshold) ? (byte)bitMask.v6 : (byte)0;
        //            cubeBytes[j, i] += (slice2[j + 1, i + 1] < threshold) ? (byte)bitMask.v7 : (byte)0;
        //            cubeBytes[j, i] += (slice2[j + 1, i] < threshold) ? (byte)bitMask.v8 : (byte)0;
        //            //configBytes[j, i] = regularCellClass[cubeBytes[j, i]];
        //            edges[j, i] = edgeTable[cubeBytes[j, i]];
        //        }
        //    }
        //    return (cubeBytes, edges);
        //}

        //public static List<Point3D> getEdges(byte cube, int edges, int[,] slice1, int[,] slice2, int i, int j, int k)
        //{
        //    //var l = edgeTable[cube];
        //    //var t = GetRow(triangleTable, cube);
        //    //int v1 = slice1[j, i];
        //    //int v2 = slice1[j, i + 1];
        //    //int v3 = slice1[j + 1, i +1];
        //    //int v4 = slice1[j + 1, i];
        //    //int v5 = slice2[j, i];
        //    //int v6 = slice2[j, i + 1];
        //    //int v7 = slice2[j + 1, i + 1];
        //    //int v8 = slice2[j + 1, i];

        //    List<Point3D> points = new List<Point3D>();
        //    int[] ed = GetRow(triangleTable, cube).Where(x => x >= 0).ToArray();
        //    if (ed.Length > 0)
        //    {

        //    }
        //    foreach (var edge in ed)
        //    {
        //        switch (edge)
        //        {
        //            case (int)edgeMask.e1:
        //                points.Add(new Point3D((float)(i + Interpolate(slice1[j, i], slice1[j, i + 1], i, j, k, slice1, slice2)), j, k));
        //                //points.Add(new Point3D(i + 0.5f, j, k));
        //                break;
        //            case (int)edgeMask.e2:
        //                points.Add(new Point3D(i + 1, (float)(j + Interpolate(slice1[j, i + 1], slice1[j + 1, i + 1], i, j, k, slice1, slice2)), k));
        //                //points.Add(new Point3D(i + 1, j + 0.5f, k));
        //                break;
        //            case (int)edgeMask.e3:
        //                points.Add(new Point3D((float)(i + Interpolate(slice1[j + 1, i], slice1[j + 1, i + 1], i, j, k, slice1, slice2)), j + 1, k));
        //                //points.Add(new Point3D(i + 0.5f, j + 1, k));
        //                break;
        //            case (int)edgeMask.e4:
        //                points.Add(new Point3D(i, (float)(j + Interpolate(slice1[j, i], slice1[j + 1, i], i, j, k, slice1, slice2)), k));
        //                //points.Add(new Point3D(i, j + 0.5f, k));
        //                break;
        //            case (int)edgeMask.e5:
        //                points.Add(new Point3D((float)(i + Interpolate(slice2[j, i], slice2[j, i + 1], i, j, k, slice1, slice2)), j, k + 1));
        //                //points.Add(new Point3D(i + 0.5f, j, k + 1));
        //                break;
        //            case (int)edgeMask.e6:
        //                points.Add(new Point3D(i + 1, (float)(j + Interpolate(slice2[j, i + 1], slice2[j + 1, i + 1], i, j, k, slice1, slice2)), k + 1));
        //                //points.Add(new Point3D(i + 1, j + 0.5f, k + 1));
        //                break;
        //            case (int)edgeMask.e7:
        //                points.Add(new Point3D((float)(i + Interpolate(slice2[j + 1, i], slice2[j + 1, i + 1], i, j, k, slice1, slice2)), j + 1, k + 1));
        //                //points.Add(new Point3D(i + 0.5f, j + 1, k + 1));
        //                break;
        //            case (int)edgeMask.e8:
        //                points.Add(new Point3D(i, (float)(j + Interpolate(slice2[j, i], slice2[j + 1, i], i, j, k, slice1, slice2)), k + 1));
        //                //points.Add(new Point3D(i, j + 0.5f, k + 1));
        //                break;
        //            case (int)edgeMask.e9:
        //                points.Add(new Point3D(i, j, (float)(k + Interpolate(slice1[j, i], slice2[j, i], i, j, k, slice1, slice2))));
        //                //points.Add(new Point3D(i, j, k+0.5f));
        //                break;
        //            case (int)edgeMask.e10:
        //                points.Add(new Point3D(i + 1, j, (float)(k + Interpolate(slice1[j, i + 1], slice2[j, i + 1], i, j, k, slice1, slice2))));
        //                //points.Add(new Point3D(i + 1, j, k+0.5f));
        //                break;
        //            case (int)edgeMask.e11:
        //                points.Add(new Point3D(i + 1, j + 1, (float)(k + Interpolate(slice1[j + 1, i + 1], slice2[j + 1, i + 1], i, j, k, slice1, slice2))));
        //                //points.Add(new Point3D(i + 1, j + 1, k+0.5f));
        //                break;
        //            case (int)edgeMask.e12:
        //                points.Add(new Point3D(i, j + 1, (float)(k + Interpolate(slice1[j + 1, i], slice2[j + 1, i], i, j, k, slice1, slice2))));
        //                //points.Add(new Point3D(i, j + 1, k+0.5f));
        //                break;
        //        }
        //    }
        //    return points;
        //}


        //public static double Interpolate(int top, int bottom, int i, int j, int k, int[,] slice1, int[,] slice2)
        //{
        //    float epsilon = 50;
        //    double p1 = Math.Max(top, bottom);
        //    double p2 = Math.Min(top, bottom);
        //    if (p2 == 0)
        //        p2 = 4 * epsilon;

        //    double x = ((double)threshold - p2) / (double)(p1 - p2);
        //    //if (bottom > top)
        //    //    x = 1 - x;
        //    if (x < 0 || x > 1)
        //        ;
        //    return x;
        //    //return 0.5;
        //}

        //public static int[] GetColumn(int[,] matrix, int columnNumber)
        //{
        //	return Enumerable.Range(0, matrix.GetLength(0))
        //			.Select(x => matrix[x, columnNumber])
        //			.ToArray();
        //}

        //public static int[] GetRow(int[,] matrix, int rowNumber)
        //{
        //    return Enumerable.Range(0, matrix.GetLength(1))
        //            .Select(x => matrix[rowNumber, x])
        //            .ToArray();
        //}

        ////public static void ListToExcel(List<string> list)
        ////{

        ////	using (StreamWriter sw = File.CreateText("list.csv"))
        ////	{
        ////		for (int i = 0; i < list.Count; i++)
        ////		{
        ////			sw.WriteLine(list[i]);
        ////		}
        ////	}
        ////}

        //private enum bitMask
        //{
        //    v1 = 0x01,
        //    v2 = 0x02,
        //    v3 = 0x04,
        //    v4 = 0x08,
        //    v5 = 0x10,
        //    v6 = 0x20,
        //    v7 = 0x40,
        //    v8 = 0x80,
        //}

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

        //private static int[] edgeTable =
        //{
        //    0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
        //    0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
        //    0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
        //    0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
        //    0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
        //    0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
        //    0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
        //    0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
        //    0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
        //    0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
        //    0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
        //    0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
        //    0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
        //    0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
        //    0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
        //    0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
        //    0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
        //    0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
        //    0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
        //    0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
        //    0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
        //    0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
        //    0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
        //    0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
        //    0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
        //    0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
        //    0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
        //    0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
        //    0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
        //    0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
        //    0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
        //    0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
        //};

        private static Edge[] triangleTable =
        {
                new Edge(-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1),
                new Edge(8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1),
                new Edge(3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1),
                new Edge(4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1),
                new Edge(4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1),
                new Edge(9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1),
                new Edge(10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1),
                new Edge(5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1),
                new Edge(5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1),
                new Edge(8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1),
                new Edge(2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1),
                new Edge(2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1),
                new Edge(11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1),
                new Edge(5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1),
                new Edge(11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1),
                new Edge(11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1),
                new Edge(2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1),
                new Edge(6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1),
                new Edge(3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1),
                new Edge(6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1),
                new Edge(6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1),
                new Edge(8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1),
                new Edge(7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1),
                new Edge(3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1),
                new Edge(0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1),
                new Edge(9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1),
                new Edge(8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1),
                new Edge(5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1),
                new Edge(0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1),
                new Edge(6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1),
                new Edge(10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1),
                new Edge(1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1),
                new Edge(0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1),
                new Edge(3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1),
                new Edge(6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1),
                new Edge(9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1),
                new Edge(8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1),
                new Edge(3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1),
                new Edge(6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1),
                new Edge(10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1),
                new Edge(10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1),
                new Edge(2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1),
                new Edge(7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1),
                new Edge(7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1),
                new Edge(2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1),
                new Edge(1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1),
                new Edge(11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1),
                new Edge(8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1),
                new Edge(0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1),
                new Edge(7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1),
                new Edge(7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1),
                new Edge(10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1),
                new Edge(0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1),
                new Edge(7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1),
                new Edge(6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1),
                new Edge(6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1),
                new Edge(4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1),
                new Edge(10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1),
                new Edge(8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1),
                new Edge(1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1),
                new Edge(10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1),
                new Edge(10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1),
                new Edge(9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1),
                new Edge(7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1),
                new Edge(3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1),
                new Edge(7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1),
                new Edge(3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1),
                new Edge(6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1),
                new Edge(9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1),
                new Edge(1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1),
                new Edge(4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1),
                new Edge(7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1),
                new Edge(6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1),
                new Edge(0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1),
                new Edge(6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1),
                new Edge(0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1),
                new Edge(11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1),
                new Edge(6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1),
                new Edge(5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1),
                new Edge(9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1),
                new Edge(1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1),
                new Edge(10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1),
                new Edge(0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1),
                new Edge(10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1),
                new Edge(11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1),
                new Edge(9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1),
                new Edge(7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1),
                new Edge(2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1),
                new Edge(9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1),
                new Edge(9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1),
                new Edge(1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1),
                new Edge(5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1),
                new Edge(0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1),
                new Edge(10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1),
                new Edge(2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1),
                new Edge(0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1),
                new Edge(0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1),
                new Edge(9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1),
                new Edge(5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1),
                new Edge(5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1),
                new Edge(8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1),
                new Edge(9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1),
                new Edge(1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1),
                new Edge(3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1),
                new Edge(4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1),
                new Edge(9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1),
                new Edge(11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1),
                new Edge(11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1),
                new Edge(2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1),
                new Edge(9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1),
                new Edge(3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1),
                new Edge(1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1),
                new Edge(4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1),
                new Edge(0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1),
                new Edge(9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1),
                new Edge(1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
                new Edge(-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1)
        };
    }
}
//    public class HostHistoPyramid : IDisposable
//    {
//        public MemoryBuffer2D<byte, Stride2D.DenseX> layer0;
//        public MemoryBuffer2D<byte, Stride2D.DenseX> layer1;
//        public MemoryBuffer2D<byte, Stride2D.DenseX> layer2;
//        public MemoryBuffer2D<uushort, Stride2D.DenseX> layer3;
//        public MemoryBuffer2D<uushort, Stride2D.DenseX> layer4;
//        public MemoryBuffer2D<uushort, Stride2D.DenseX> layer5;
//        public MemoryBuffer2D<uushort, Stride2D.DenseX> layer6;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer7;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer8;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer9;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer10;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer11;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer12;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer13;
//        public MemoryBuffer2D<uint, Stride2D.DenseX> layer14;
//        public MemoryBuffer2D<ulong, Stride2D.DenseX> layer15;
//        public byte size;
//        public HistoPyramid Histo;


//        public HostHistoPyramid(int[][,] HP, int n, Accelerator acc)
//        {
//            size = (byte)n;
//            byte[] tempBytes = HP[0].Cast<int>().Select(x => (byte)x).ToArray();
//            byte[,] temp2Dbytes = new byte[HP[0].GetLength(0), HP[0].GetLength(0)];
//            Buffer.BlockCopy(tempBytes, 0, temp2Dbytes, 0, tempBytes.Length);
//            layer0 = acc.Allocate2DDenseX<byte>(temp2Dbytes);

//            tempBytes = HP[1].Cast<int>().Select(x => (byte)x).ToArray();
//            temp2Dbytes = new byte[HP[1].GetLength(0), HP[1].GetLength(1)];
//            Buffer.BlockCopy(tempBytes, 0, temp2Dbytes, 0, tempBytes.Length);
//            layer1 = acc.Allocate2DDenseX<byte>(temp2Dbytes);

//            tempBytes = HP[2].Cast<int>().Select(x => (byte)x).ToArray();
//            temp2Dbytes = new byte[HP[2].GetLength(0), HP[2].GetLength(0)];
//            Buffer.BlockCopy(tempBytes, 0, temp2Dbytes, 0, tempBytes.Length);
//            layer2 = acc.Allocate2DDenseX<byte>(temp2Dbytes);

//            uushort[] tempUushorts = HP[3].Cast<int>().Select(x => (uushort)x).ToArray();
//            uushort[,] temp2DUushorts = new uushort[HP[3].GetLength(0), HP[3].GetLength(0)];
//            Buffer.BlockCopy(tempUushorts, 0, temp2DUushorts, 0, tempUushorts.Length);
//            layer3 = acc.Allocate2DDenseX<uushort>(temp2DUushorts);

//            tempUushorts = HP[4].Cast<int>().Select(x => (uushort)x).ToArray();
//            temp2DUushorts = new uushort[HP[4].GetLength(0), HP[4].GetLength(0)];
//            Buffer.BlockCopy(tempUushorts, 0, temp2DUushorts, 0, tempUushorts.Length);
//            layer4 = acc.Allocate2DDenseX<uushort>(temp2DUushorts);

//            tempUushorts = HP[5].Cast<int>().Select(x => (uushort)x).ToArray();
//            temp2DUushorts = new uushort[HP[5].GetLength(0), HP[5].GetLength(0)];
//            Buffer.BlockCopy(tempUushorts, 0, temp2DUushorts, 0, tempUushorts.Length);
//            layer5 = acc.Allocate2DDenseX<uushort>(temp2DUushorts);

//            tempUushorts = HP[6].Cast<int>().Select(x => (uushort)x).ToArray();
//            temp2DUushorts = new uushort[HP[6].GetLength(0), HP[6].GetLength(0)];
//            Buffer.BlockCopy(tempUushorts, 0, temp2DUushorts, 0, tempUushorts.Length);
//            layer6 = acc.Allocate2DDenseX<uushort>(temp2DUushorts);

//            uint[] tempUints = HP[7].Cast<int>().Select(x => (uint)x).ToArray();
//            uint[,] temp2DUints = new uint[HP[7].GetLength(0), HP[7].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer7 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            tempUints = HP[8].Cast<int>().Select(x => (uint)x).ToArray();
//            temp2DUints = new uint[HP[8].GetLength(0), HP[8].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer8 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            tempUints = HP[9].Cast<int>().Select(x => (uint)x).ToArray();
//            temp2DUints = new uint[HP[9].GetLength(0), HP[9].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer9 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            tempUints = HP[10].Cast<int>().Select(x => (uint)x).ToArray();
//            temp2DUints = new uint[HP[10].GetLength(0), HP[10].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer10 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            tempUints = HP[11].Cast<int>().Select(x => (uint)x).ToArray();
//            temp2DUints = new uint[HP[11].GetLength(0), HP[11].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer11 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            tempUints = HP[12].Cast<int>().Select(x => (uint)x).ToArray();
//            temp2DUints = new uint[HP[12].GetLength(0), HP[12].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer12 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            tempUints = HP[13].Cast<int>().Select(x => (uint)x).ToArray();
//            temp2DUints = new uint[HP[13].GetLength(0), HP[13].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer13 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            tempUints = HP[14].Cast<int>().Select(x => (uint)x).ToArray();
//            temp2DUints = new uint[HP[14].GetLength(0), HP[14].GetLength(0)];
//            Buffer.BlockCopy(tempUints, 0, temp2DUints, 0, tempUints.Length);
//            layer14 = acc.Allocate2DDenseX<uint>(temp2DUints);

//            ulong[] tempUlong = HP[15].Cast<int>().Select(x => (ulong)x).ToArray();
//            ulong[,] temp2DUlong = new ulong[HP[15].GetLength(0), HP[15].GetLength(0)];
//            Buffer.BlockCopy(tempUlong, 0, temp2DUlong, 0, tempUlong.Length);
//            layer15 = acc.Allocate2DDenseX<ulong>(temp2DUlong);

//            Histo = new HistoPyramid(layer0.View,
//                layer1.View,
//                layer2.View,
//                layer3.View,
//                layer4.View,
//                layer5.View,
//                layer6.View,
//                layer7.View,
//                layer8.View,
//                layer9.View,
//                layer10.View,
//                layer11.View,
//                layer12.View,
//                layer13.View,
//                layer14.View,
//                layer15.View,
//                size
//                );
//        }

//        public void Dispose()
//        {
//            layer0.Dispose();
//            layer1.Dispose();
//            layer2.Dispose();
//            layer3.Dispose();
//            layer4.Dispose();
//            layer5.Dispose();
//            layer6.Dispose();
//            layer7.Dispose();
//            layer8.Dispose();
//            layer9.Dispose();
//            layer10.Dispose();
//            layer11.Dispose();
//            layer12.Dispose();
//            layer13.Dispose();
//            layer14.Dispose();
//            layer15.Dispose();
//        }
//    }

//    public struct HistoPyramid
//    {
//        public ArrayView2D<byte, Stride2D.DenseX> layer0;
//        public ArrayView2D<byte, Stride2D.DenseX> layer1;
//        public ArrayView2D<byte, Stride2D.DenseX> layer2;
//        public ArrayView2D<uushort, Stride2D.DenseX> layer3;
//        public ArrayView2D<uushort, Stride2D.DenseX> layer4;
//        public ArrayView2D<uushort, Stride2D.DenseX> layer5;
//        public ArrayView2D<uushort, Stride2D.DenseX> layer6;
//        public ArrayView2D<uint, Stride2D.DenseX> layer7;
//        public ArrayView2D<uint, Stride2D.DenseX> layer8;
//        public ArrayView2D<uint, Stride2D.DenseX> layer9;
//        public ArrayView2D<uint, Stride2D.DenseX> layer10;
//        public ArrayView2D<uint, Stride2D.DenseX> layer11;
//        public ArrayView2D<uint, Stride2D.DenseX> layer12;
//        public ArrayView2D<uint, Stride2D.DenseX> layer13;
//        public ArrayView2D<uint, Stride2D.DenseX> layer14;
//        public ArrayView2D<ulong, Stride2D.DenseX> layer15;
//        public byte size;

//        public HistoPyramid(ArrayView2D<byte, Stride2D.DenseX> l0,
//            ArrayView2D<byte, Stride2D.DenseX> l1,
//            ArrayView2D<byte, Stride2D.DenseX> l2,
//            ArrayView2D<uushort, Stride2D.DenseX> l3,
//            ArrayView2D<uushort, Stride2D.DenseX> l4,
//            ArrayView2D<uushort, Stride2D.DenseX> l5,
//            ArrayView2D<uushort, Stride2D.DenseX> l6,
//            ArrayView2D<uint, Stride2D.DenseX> l7,
//            ArrayView2D<uint, Stride2D.DenseX> l8,
//            ArrayView2D<uint, Stride2D.DenseX> l9,
//            ArrayView2D<uint, Stride2D.DenseX> l10,
//            ArrayView2D<uint, Stride2D.DenseX> l11,
//            ArrayView2D<uint, Stride2D.DenseX> l12,
//            ArrayView2D<uint, Stride2D.DenseX> l13,
//            ArrayView2D<uint, Stride2D.DenseX> l14,
//            ArrayView2D<ulong, Stride2D.DenseX> l15,
//            int n)
//        {
//            layer0 = l0;
//            layer1 = l1;
//            layer2 = l2;
//            layer3 = l3;
//            layer4 = l4;
//            layer5 = l5;
//            layer6 = l6;
//            layer7 = l7;
//            layer8 = l8;
//            layer9 = l9;
//            layer10 = l10;
//            layer11 = l11;
//            layer12 = l12;
//            layer13 = l13;
//            layer14 = l14;
//            layer15 = l15;
//            size = (byte)n;
//        }

//        public static void TestHP(Index1D index, HistoPyramid HP0)
//        {
//            uint l = HP0[index, 0, 0];
//            l++;
//        }

//        public uint this[int index, int i, int j]
//        {
//            get
//            {
//                switch (index)
//                {
//                    case 0:
//                        return layer0[i, j];
//                    case 1:
//                        return layer1[i, j];
//                    case 2:
//                        return layer2[i, j];
//                    case 3:
//                        return layer3[i, j];
//                    case 4:
//                        return layer4[i, j];
//                    case 5:
//                        return layer5[i, j];
//                    case 6:
//                        return layer6[i, j];
//                    case 7:
//                        return layer7[i, j];
//                    case 8:
//                        return layer8[i, j];
//                    case 9:
//                        return layer9[i, j];
//                    case 10:
//                        return layer10[i, j];
//                    case 11:
//                        return layer11[i, j];
//                    case 12:
//                        return layer12[i, j];
//                    case 13:
//                        return layer13[i, j];
//                    case 14:
//                        return layer14[i, j];
//                    case 15:
//                        return (uint)layer15[i, j];
//                    default:
//                        return 0;
//                }
//            }
//        }
//    }

//}
/*        public static void AssignNormal(Index3D index, ArrayView3D<Normal, Stride3D.DenseXY> normals, ArrayView3D<ushort, Stride3D.DenseXY> input)
        {
            normals[index] =
                new Normal(
                    input[index.X, index.Y, Math.Min((width) - 1, index.Z + 1)] - input[index.X, index.Y, Math.Max(index.Z - 1, 0)],
                    input[index.X, Math.Min((width) - 1, index.Y + 1), index.Z] - input[index.X, Math.Max(index.Y - 1, 0), index.Z],
                    input[Math.Min(250 - 1, index.X + 1), index.Y, index.Z] - input[Math.Max(index.X - 1, 0), index.Y, index.Z]
                );
        }

        public static void AssignEdges(Index3D index, ArrayView3D<Edge, Stride3D.DenseXY> edges, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView<Edge> triTable, int thresh)
        { 
            //return Enumerable.Range(0, matrix.GetLength(1))
            //    .Select(x => matrix[rowNumber, x])
            //    .ToArray();
            //byte config = 0;
            //edges[index] = triTable[(int)(
            //    ((input[index] < thresh) ? (byte)0x01 : (byte)0) +
            //    ((input[index.X, index.Y, index.Z + 1] < thresh) ? (byte)0x02 : (byte)0) +
            //    ((input[index.X, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x04 : (byte)0) +
            //    ((input[index.X, index.Y + 1, index.Z] < thresh) ? (byte)0x08 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z + 1] < thresh) ? (byte)0x20 : (byte)0) +
            //    ((input[index.X + 1, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x40 : (byte)0) +
            //    ((input[index.X + 1, index.Y + 1, index.Z] < thresh) ? (byte)0x80 : (byte)0))
            //    ];
            byte config = 0;
            config += (input[index] < thresh) ? (byte)0x01 : (byte)0;
            config += (input[index.X, index.Y, index.Z + 1] < thresh) ? (byte)0x02 : (byte)0;
            config += (input[index.X, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x04 : (byte)0;
            config += (input[index.X, index.Y + 1, index.Z] < thresh) ? (byte)0x08 : (byte)0;
            config += (input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0;
            config += (input[index.X + 1, index.Y, index.Z + 1] < thresh) ? (byte)0x20 : (byte)0;
            config += (input[index.X + 1, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x40 : (byte)0;
            config += (input[index.X + 1, index.Y + 1, index.Z] < thresh) ? (byte)0x80 : (byte)0;
            edges[index] = triTable[(int)config];
        }        
        
        public static void AssignNormal1D(Index3D index, ArrayView1D<Normal, Stride1D.Dense> normals, ArrayView3D<ushort, Stride3D.DenseXY> input)
        {
            normals[index.X * 127 * 127 + index.Y * 127 + index.Z] =
                new Normal(
                    input[index.X, index.Y, Math.Min(128 - 1, index.Z + 1)] - input[index.X, index.Y, Math.Max(index.Z - 1, 0)],
                    input[index.X, Math.Min(128 - 1, index.Y + 1), index.Z] - input[index.X, Math.Max(index.Y - 1, 0), index.Z],
                    input[Math.Min(128 - 1, index.X + 1), index.Y, index.Z] - input[Math.Max(index.X - 1, 0), index.Y, index.Z]
                );
        }

        public static void AssignEdges1D(Index3D index, ArrayView1D<Edge, Stride1D.Dense> edges, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView<Edge> triTable, int thresh)
        { 
            //return Enumerable.Range(0, matrix.GetLength(1))
            //    .Select(x => matrix[rowNumber, x])
            //    .ToArray();
            //byte config = 0;
            //edges[index] = triTable[(int)(
            //    ((input[index] < thresh) ? (byte)0x01 : (byte)0) +
            //    ((input[index.X, index.Y, index.Z + 1] < thresh) ? (byte)0x02 : (byte)0) +
            //    ((input[index.X, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x04 : (byte)0) +
            //    ((input[index.X, index.Y + 1, index.Z] < thresh) ? (byte)0x08 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0) +
            //    ((input[index.X + 1, index.Y, index.Z + 1] < thresh) ? (byte)0x20 : (byte)0) +
            //    ((input[index.X + 1, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x40 : (byte)0) +
            //    ((input[index.X + 1, index.Y + 1, index.Z] < thresh) ? (byte)0x80 : (byte)0))
            //    ];
            byte config = 0;
            config += (input[index] < thresh) ? (byte)0x01 : (byte)0;
            config += (input[index.X, index.Y, index.Z + 1] < thresh) ? (byte)0x02 : (byte)0;
            config += (input[index.X, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x04 : (byte)0;
            config += (input[index.X, index.Y + 1, index.Z] < thresh) ? (byte)0x08 : (byte)0;
            config += (input[index.X + 1, index.Y, index.Z] < thresh) ? (byte)0x10 : (byte)0;
            config += (input[index.X + 1, index.Y, index.Z + 1] < thresh) ? (byte)0x20 : (byte)0;
            config += (input[index.X + 1, index.Y + 1, index.Z + 1] < thresh) ? (byte)0x40 : (byte)0;
            config += (input[index.X + 1, index.Y + 1, index.Z] < thresh) ? (byte)0x80 : (byte)0;
            edges[index.X * 127 * 127 + index.Y * 127 + index.Z] = triTable[(int)config];
        }
*/

//private static Cube[,] MarchingCubes(ushort index)
//{
//    Cube[,] cubeBytes = new Cube[length, width];
//    //byte[,] configBytes = new byte[511, 511];
//    //int[,] edges = new int[length, width];

//    //bit order 
//    // i,j,k 
//    // i+1,j,k
//    // i+1,j+1,k
//    // i,j+1,k
//    // i,j,k+1
//    // i+1,j,k+1
//    // i+1,j+1,k+1
//    // i,j+1,k+1


//    for (ushort i = 0; i < slices.GetLength(2) - 1; i++)
//    {
//        for (ushort j = 0; j < slices.GetLength(1) - 1; j++)
//        {
//            Stopwatch stopWatch = new Stopwatch();
//            stopWatch.Start();
//            cubeBytes[j, i] = new Cube(
//                new Point(i, j, index, slices[index, j, i], new Normal(
//                    slices[index, j, Math.Min(length - 1, i + 1)] - slices[index, j, Math.Max(i - 1, 0)],
//                    slices[index, Math.Min(length - 1, j + 1), i] - slices[index, Math.Max(j - 1, 0), i],
//                    slices[Math.Min(slices.Length - 1, index + 1), j, i] - slices[Math.Max(index - 1, 0), j, i]
//                    )),
//                new Point((ushort)(i + 1), j, index, slices[index, j, i + 1], new Normal(
//                    slices[index, j, Math.Min(length - 1, (i + 1) + 1)] - slices[index, j, Math.Max((i + 1) - 1, 0)],
//                    slices[index, Math.Min(length - 1, j + 1), (i + 1)] - slices[index, Math.Max(j - 1, 0), (i + 1)],
//                    slices[Math.Min(slices.Length - 1, index + 1), j, (i + 1)] - slices[Math.Max(index - 1, 0), j, (i + 1)]
//                    )),
//                new Point((ushort)(i + 1), (ushort)(j + 1), index, slices[index, j + 1, i + 1], new Normal(
//                    slices[index, (j + 1), Math.Min(length - 1, (i + 1) + 1)] - slices[index, (j + 1), Math.Max((i + 1) - 1, 0)],
//                    slices[index, Math.Min(length - 1, (j + 1) + 1), (i + 1)] - slices[index, Math.Max((j + 1) - 1, 0), (i + 1)],
//                    slices[Math.Min(slices.Length - 1, index + 1), (j + 1), (i + 1)] - slices[Math.Max(index - 1, 0), (j + 1), (i + 1)]
//                    )),
//                new Point(i, (ushort)(j + 1), index, slices[index, j + 1, i], new Normal(
//                    slices[index, (j + 1), Math.Min(length - 1, i + 1)] - slices[index, (j + 1), Math.Max(i - 1, 0)],
//                    slices[index, Math.Min(length - 1, (j + 1) + 1), i] - slices[index, Math.Max((j + 1) - 1, 0), i],
//                    slices[Math.Min(slices.Length - 1, index + 1), (j + 1), i] - slices[Math.Max(index - 1, 0), (j + 1), i]
//                    )),
//                new Point(i, j, (index + 1), slices[(index + 1), j, i], new Normal(
//                    slices[(index + 1), j, Math.Min(length - 1, i + 1)] - slices[(index + 1), j, Math.Max(i - 1, 0)],
//                    slices[(index + 1), Math.Min(length - 1, j + 1), i] - slices[(index + 1), Math.Max(j - 1, 0), i],
//                    slices[Math.Min(slices.Length - 1, (index + 1) + 1), j, i] - slices[Math.Max((index + 1) - 1, 0), j, i]
//                    )),
//                new Point((ushort)(i + 1), j, (index + 1), slices[(index + 1), j, i + 1], new Normal(
//                    slices[(index + 1), j, Math.Min(length - 1, (i + 1) + 1)] - slices[(index + 1), j, Math.Max((i + 1) - 1, 0)],
//                    slices[(index + 1), Math.Min(length - 1, j + 1), (i + 1)] - slices[(index + 1), Math.Max(j - 1, 0), (i + 1)],
//                    slices[Math.Min(slices.Length - 1, (index + 1) + 1), j, (i + 1)] - slices[Math.Max((index + 1) - 1, 0), j, (i + 1)]
//                    )),
//                new Point((ushort)(i + 1), (ushort)(j + 1), (index + 1), slices[(index + 1), j + 1, i + 1], new Normal(
//                    slices[(index + 1), (j + 1), Math.Min(length - 1, (i + 1) + 1)] - slices[(index + 1), (j + 1), Math.Max((i + 1) - 1, 0)],
//                    slices[(index + 1), Math.Min(length - 1, (j + 1) + 1), (i + 1)] - slices[(index + 1), Math.Max((j + 1) - 1, 0), (i + 1)],
//                    slices[Math.Min(slices.Length - 1, (index + 1) + 1), (j + 1), (i + 1)] - slices[Math.Max((index + 1) - 1, 0), (j + 1), (i + 1)]
//                    )),
//                new Point(i, (ushort)(j + 1), (index + 1), slices[(index + 1), j + 1, i], new Normal(
//                    slices[(index + 1), (j + 1), Math.Min(length - 1, i + 1)] - slices[(index + 1), (j + 1), Math.Max(i - 1, 0)],
//                    slices[(index + 1), Math.Min(length - 1, (j + 1) + 1), i] - slices[(index + 1), Math.Max((j + 1) - 1, 0), i],
//                    slices[Math.Min(slices.Length - 1, (index + 1) + 1), (j + 1), i] - slices[Math.Max((index + 1) - 1, 0), (j + 1), i]
//                    ))
//                );
//            //foreach (Point point in cubeBytes[j, i].voxels)
//            //{
//            //    point.normal = new Normal(
//            //        slices[(int)point.Z][(int)point.Y, (int)Math.Min(length - 1, point.X + 1)] - slices[(int)point.Z][(int)point.Y, (int)Math.Max(point.X - 1, 0)],
//            //        slices[(int)point.Z][(int)Math.Min(length - 1, point.Y + 1), (int)point.X] - slices[(int)point.Z][(int)Math.Max(point.Y - 1, 0), (int)point.X],
//            //        slices[(int)Math.Min(slices.Length - 1, point.Z + 1)][(int)point.Y, (int)point.X] - slices[(int)Math.Max(point.Z - 1, 0)][(int)point.Y, (int)point.X]
//            //        );
//            //}
//            //cubeBytes[j, i].Config(threshold);
//            //configBytes[j, i] = regularCellClass[cubeBytes[j, i]];
//            //edges[j, i] = edgeTable[cubeBytes[j, i].getConfig()];
//            stopWatch.Stop();
//            // Get the elapsed time as a TimeSpan value.
//            ts += stopWatch.Elapsed;
//        }
//    }
//    return cubeBytes;
//}