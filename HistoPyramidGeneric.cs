﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ILGPU.Runtime;
using ILGPU;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MarchingCubes
{
    class HistoPyramidGeneric : MarchingCubes
    {
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<byte, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int, int, int> assign;
        public static Action<Index1D, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<byte, Stride1D.Dense>, int> hpFirstLayer;
        public static Action<Index1D, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> hpCreation;
        public static Action<Index1D, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernel;
        public static Action<Index1D,
            ArrayView1D<long, Stride1D.Dense>,
            ArrayView1D<uint, Stride1D.Dense>,
            ArrayView1D<byte, Stride1D.Dense>,
            ArrayView1D<Triangle, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Edge, Stride1D.Dense>,
            ArrayView3D<ushort, Stride3D.DenseXY>,
            int, int, int, int, int> hpFinalLayer;


        public static MemoryBuffer3D<byte, Stride3D.DenseXY> cubeConfig;
        private static MemoryBuffer1D<byte, Stride1D.Dense> HPBaseConfig;
        public static MemoryBuffer3D<Normal, Stride3D.DenseXY> gradConfig;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer15;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer14;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer13;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer12;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer11;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer10;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer9;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer8;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer7;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer6;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer5;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer4;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer3;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer2;
        public static MemoryBuffer1D<uint, Stride1D.Dense> uintLayer1;

        public static byte[,,] cubeBytes;
        public static byte[,] HPBaseLayer;
        public static int HPsize;

        public static TimeSpan ts = new TimeSpan();

        public static List<Point> vertices = new List<Point>();
        public static byte[,,] cubes;
        public static int factor = 5;
        public static int X, Y, Z;

        public static int count = 0;

        public HistoPyramidGeneric(int size)
        {
            Console.WriteLine("HistoPyramid Generic");
            ushort i = 0;
            FileInfo fi = CreateVolume(size);

            X = slices.GetLength(2) - 1;
            Y = slices.GetLength(1) - 1;
            Z = slices.GetLength(0) - 1;

            HPsize = width;
            if (Math.Log(width, 5) % 1 > 0)
            {
                HPsize = (int)Math.Pow(5, (int)Math.Log(width - 1, 5) + 1);
            }
            //var s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
            //var p = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Point));
            //var n = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal));

            assign = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D< byte, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int, int, int>(Assign);
            hpFirstLayer = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<byte, Stride1D.Dense>, int>(BuildHPFirst);
            hpCreation = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(BuildHP);
            traversalKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView1D<long, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(HPTraverseKernel);
            hpFinalLayer = accelerator.LoadAutoGroupedStreamKernel<Index1D,
            ArrayView1D<long, Stride1D.Dense>,
            ArrayView1D<uint, Stride1D.Dense>,
            ArrayView1D<byte, Stride1D.Dense>,
            ArrayView1D<Triangle, Stride1D.Dense>,
            ArrayView3D<byte, Stride3D.DenseXY>,
            ArrayView1D<Edge, Stride1D.Dense>,
            ArrayView3D<ushort, Stride3D.DenseXY>,
            int, int, int, int, int>(HPFinalLayer);

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

        public static void BuildHP(Index1D index, ArrayView1D<uint, Stride1D.Dense> HPLayer, ArrayView1D<uint, Stride1D.Dense> HPLayerPrev, int factor)
        {
            uint tempSum = 0;
            for(int i = 0; i < factor; i++)
                tempSum += HPLayerPrev[index * factor + i];
            HPLayer[index] = tempSum;
        }


        public static void BuildHPFirst(Index1D index, ArrayView1D<uint, Stride1D.Dense> HPLayer, ArrayView1D<byte, Stride1D.Dense> HPLayerBase, int factor)
        {
            uint tempSum = 0;
            for (int i = 0; i < factor; i++)
                tempSum += HPLayerBase[index * factor + i];
            HPLayer[index] = tempSum;
        }

        private static void HPCreationGPU()
        {
            nLayers = (ushort)(Math.Ceiling(Math.Log(HPBaseConfig.Extent, 5)));

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            getHPLayer(1) = accelerator.Allocate1D<uint>(new Index1D((int)HPBaseConfig.Extent / factor));
            hpFirstLayer(getHPLayer(1).IntExtent, getHPLayer(1).View, HPBaseConfig.View, factor);
            accelerator.Synchronize();
            int l = getHPLayer(1).IntExtent;
            for (int i = 2; i < 16; i++)
            {
                l /= factor; 
                if (i < nLayers)
                {
                    Index1D index = new Index1D(l);
                    getHPLayer(i) = accelerator.Allocate1D<uint>(index);

                    hpCreation(index, getHPLayer(i).View, getHPLayer(i - 1).View, factor);
                    accelerator.Synchronize();
                }
            }

            stopWatch.Stop();
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            uint[] data = getHPLayer(nLayers - 1).GetAsArray1D();
            if (data.Length == 1)
                nTri = (int)data[0];
        }

        public static void HPTraverseKernel(Index1D index, ArrayView1D<long, Stride1D.Dense> p, ArrayView1D<uint, Stride1D.Dense> k, ArrayView1D<uint, Stride1D.Dense> HPLayer, int factor)
        {
            uint comp = 0;
            for(int i = 0; i < factor; i++)
            {
                if(k[index] < comp + HPLayer[p[index] + i])
                {
                    k[index] -= comp;
                    p[index] += i;
                    break;
                }
                comp += HPLayer[p[index] + i];
            }
            p[index] *= factor;
        }

        public static void HPFinalLayer(Index1D index,
            ArrayView1D<long, Stride1D.Dense> p,
            ArrayView1D<uint, Stride1D.Dense> k,
            ArrayView1D<byte, Stride1D.Dense> HPLayer,
            ArrayView1D<Triangle, Stride1D.Dense> triangles,
            ArrayView3D<byte, Stride3D.DenseXY> edges,
            ArrayView1D<Edge, Stride1D.Dense> triTable,
            ArrayView3D<ushort, Stride3D.DenseXY> input,
            int HPSqrt, int factor, int x, int y, int z)
        {
            uint comp = 0;
            if (index > 109)
                ;
            for (int i = 0; i < factor; i++)
            {
                if (k[index] < comp + HPLayer[p[index] + i])
                {
                    k[index] -= comp;
                    p[index] += i;
                    break;
                }
                comp += HPLayer[p[index] + i];
            }

            Index3D index3D = get3D(new Index1D((int)p[index]), new Index3D(z, y, x));
            index3D = new Index3D(index3D.Z, index3D.Y, index3D.X);

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
            MemoryBuffer1D<long, Stride1D.Dense> p = accelerator.Allocate1D<long>(nTri);
            Triangle[] tri = new Triangle[nTri];
            PageLockedArray1D<Triangle> triLocked = accelerator.AllocatePageLocked1D<Triangle>(nTri);
            MemoryBuffer1D<Triangle, Stride1D.Dense> triConfig = accelerator.Allocate1D<Triangle>(nTri);
            p.MemSetToZero();
            accelerator.Synchronize();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (n = nLayers - 2; n > 0; n--)
            {
                traversalKernel(index, p.View, k.View, getHPLayer(n).View, factor);
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

            hpFinalLayer(index, p.View, k.View, HPBaseConfig.View, triConfig.View, cubeConfig.View, triTable.View, sliced.View, threshold, factor, X, Y, Z);

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

        public static void Assign(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> edges, ArrayView1D<byte, Stride1D.Dense> HPindices, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView<Edge> triTable, int thresh, int x, int y, int z)
        {
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
            HPindices[get1D(index, new Index3D(x, y, z))] = (byte)(triTable[(int)config].getn() / 3);
            if((byte)(triTable[(int)config].getn() / 3) > 0)
            {
                var l = get1D(index, new Index3D(x, y, z));
            }
        }

        public static byte[,,] MarchingCubesGPU()
        {
            Index3D index = new Index3D(slices.GetLength(0) - 1, slices.GetLength(1) - 1, slices.GetLength(2) - 1);
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

            HPBaseLayer = new byte[HPsize * (int)Math.Sqrt(HPsize), HPsize * (int)Math.Sqrt(HPsize)];

            cubeBytes = new byte[(index.X), (index.Y), (index.Z)];
            var cubePinned = GCHandle.Alloc(cubeBytes, GCHandleType.Pinned);
            var HPPinned = GCHandle.Alloc(HPBaseLayer, GCHandleType.Pinned);
            PageLockedArray3D<byte> cubeLocked = accelerator.AllocatePageLocked3D<byte>(new Index3D(index.X, index.Y, index.Z));
            PageLockedArray2D<byte> HPLocked = accelerator.AllocatePageLocked2D<byte>(new Index2D(HPBaseLayer.GetLength(0)));
            cubeConfig = accelerator.Allocate3DDenseXY<byte>(cubeLocked.Extent);
            HPBaseConfig = accelerator.Allocate1D<byte>(HPLocked.Extent.Size);
            var cubeScope = accelerator.CreatePageLockFromPinned<byte>(cubePinned.AddrOfPinnedObject(), cubeBytes.Length);
            var HPScope = accelerator.CreatePageLockFromPinned<byte>(HPPinned.AddrOfPinnedObject(), HPBaseLayer.Length);
            cubeConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);
            HPBaseConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, HPScope);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            assign(index, cubeConfig.View, HPBaseConfig.View, sliced.View, triTable.View, threshold, index.X, index.Y, index.Z);

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            cubeConfig.AsContiguous().CopyToPageLockedAsync(cubeLocked);
            cubeBytes = cubeLocked.GetArray();
            //cubeConfig.Dispose();
            HPBaseConfig.AsContiguous().CopyToPageLockedAsync(HPLocked);
            HPBaseLayer = HPLocked.GetArray();
            cubePinned.Free();
            HPPinned.Free();

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            return (cubeBytes);
        }

        public static ref MemoryBuffer1D<uint, Stride1D.Dense> getHPLayer(int index)
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