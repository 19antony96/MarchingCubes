﻿using System;
using System.Data;
using System.IO;
using System.Linq;
using ILGPU.Runtime;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Algorithms;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MarchingCubes
{
    class OctreeBONOwPrior : MarchingCubes
    {
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, int> assign;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> branchAll;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> branchX;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> branchY;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> branchZ;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> branchXY;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> branchXZ;
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>> branchYZ;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelAll;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelX;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelY;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelZ;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelXY;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelXZ;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelYZ;
        public static Action<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<Triangle, Stride1D.Dense>, ArrayView1D<Edge, Stride1D.Dense>, ushort, int> octreeFinalLayer;

        public static MemoryBuffer3D<byte, Stride3D.DenseXY> layerConfig;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer15;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer14;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer13;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer12;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer11;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer10;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer9;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer8;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer7;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer6;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer5;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer4;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer3;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer2;
        public static MemoryBuffer3D<byte, Stride3D.DenseXY> byteLayer1;

        public static ushort xCode;
        public static ushort yCode;
        public static ushort zCode;

        public static TimeSpan ts = new TimeSpan();
        public static int count = 0;

        public OctreeBONOwPrior(int size)
        {
            Console.WriteLine("Octree BONO");
            ushort i = 0;
            FileInfo fi = CreateVolume(size);

            xCode = (ushort)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(2), 2)));
            yCode = (ushort)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(1), 2)));
            zCode = (ushort)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(0), 2)));

            //var s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
            //var p = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Point));
            //var n = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal));

            assign = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, int>(Assign);
            branchAll = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BranchAll);
            branchX = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BranchX);
            branchY = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BranchY);
            branchZ = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BranchZ);
            branchXY = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BranchXY);
            branchXZ = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BranchXZ);
            branchYZ = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<byte, Stride3D.DenseXY>>(BranchYZ);
            traversalKernelAll = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelAll);
            traversalKernelX = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelX);
            traversalKernelY = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelY);
            traversalKernelZ = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelZ);
            traversalKernelXY = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelXY);
            traversalKernelXZ = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelXZ);
            traversalKernelYZ = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelYZ);
            octreeFinalLayer = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<Triangle, Stride1D.Dense>, ArrayView1D<Edge, Stride1D.Dense>, ushort, int>(OctreeFinalLayer);

            MarchingCubesGPU();

            OctreeCreationGPU();


            using (StreamWriter fs = fi.CreateText())
            {
                OctreeTraversalGPU(fs);

                int f = 0;
                for (f = 1; f < count - 1; f += 3)
                {
                    fs.WriteLine("f " + f + " " + (f + 1) + " " + (f + 2));
                }
                Console.WriteLine(count);
            }

            for (i = 1; i < nLayers; i++)
            {
                if (getByteOctreeLayer(i) != null && !getByteOctreeLayer(i).IsDisposed)
                    getByteOctreeLayer(i).Dispose();
            }
            accelerator.Dispose();
            context.Dispose();
        }

        public static void BranchX(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> bytePrev, ArrayView3D<byte, Stride3D.DenseXY> layer)
        {
            if (bytePrev[(index.Z) * 2, (index.Y) * 2, (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2, (index.Y) * 2 + 1, (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y) * 2, (index.X) * 2] > 0)
                layer[(index.Z), (index.Y), (index.X)] = 1;
            else
                layer[(index.Z), (index.Y), (index.X)] = 0;
        }

        public static void BranchY(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> bytePrev, ArrayView3D<byte, Stride3D.DenseXY> layer)
        {
            if (bytePrev[(index.Z) * 2, (index.Y) * 2, (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2, (index.Y) * 2 + 1, (index.X) * 2] > 0)
                layer[(index.Z), (index.Y), (index.X)] = 1;
            else
                layer[(index.Z), (index.Y), (index.X)] = 0;
        }
        public static void BranchZ(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> bytePrev, ArrayView3D<byte, Stride3D.DenseXY> layer)
        {
            if (bytePrev[(index.Z), (index.Y), (index.X) * 2] > 0 ||
                bytePrev[(index.Z), (index.Y), (index.X) * 2 + 1] > 0)
                layer[(index.Z), (index.Y), (index.X)] = 1;
            else
                layer[(index.Z), (index.Y), (index.X)] = 0;
        }
        public static void BranchXY(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> bytePrev, ArrayView3D<byte, Stride3D.DenseXY> layer)
        {
            if (bytePrev[(index.Z) * 2, (index.Y) * 2, (index.X)] > 0 ||
                bytePrev[(index.Z) * 2, (index.Y) * 2 + 1, (index.X)] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y) * 2, (index.X)] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y) * 2 + 1, (index.X)] > 0)
                layer[(index.Z), (index.Y), (index.X)] = 1;
            else
                layer[(index.Z), (index.Y), (index.X)] = 0;
        }
        public static void BranchXZ(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> bytePrev, ArrayView3D<byte, Stride3D.DenseXY> layer)
        {

            if (bytePrev[(index.Z) * 2, (index.Y), (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2, (index.Y), (index.X) * 2 + 1] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y), (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y), (index.X) * 2 + 1] > 0)
                layer[(index.Z), (index.Y), (index.X)] = 1;
            else
                layer[(index.Z), (index.Y), (index.X)] = 0;
        }
        public static void BranchYZ(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> bytePrev, ArrayView3D<byte, Stride3D.DenseXY> layer)
        {
            if (bytePrev[(index.Z), (index.Y) * 2, (index.X) * 2] > 0 ||
                bytePrev[(index.Z), (index.Y) * 2, (index.X) * 2 + 1] > 0 ||
                bytePrev[(index.Z), (index.Y) * 2 + 1, (index.X) * 2 + 1] > 0 ||
                bytePrev[(index.Z), (index.Y) * 2 + 1, (index.X) * 2] > 0)
                layer[(index.Z), (index.Y), (index.X)] = 1;
            else
                layer[(index.Z), (index.Y), (index.X)] = 0;
        }
        public static void BranchAll(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> bytePrev, ArrayView3D<byte, Stride3D.DenseXY> layer)
        {
            if (bytePrev[(index.Z) * 2, (index.Y) * 2, (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2, (index.Y) * 2, (index.X) * 2 + 1] > 0 ||
                bytePrev[(index.Z) * 2, (index.Y) * 2 + 1, (index.X) * 2 + 1] > 0 ||
                bytePrev[(index.Z) * 2, (index.Y) * 2 + 1, (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y) * 2, (index.X) * 2] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y) * 2, (index.X) * 2 + 1] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y) * 2 + 1, (index.X) * 2 + 1] > 0 ||
                bytePrev[(index.Z) * 2 + 1, (index.Y) * 2 + 1, (index.X) * 2] > 0)
                layer[(index.Z), (index.Y), (index.X)] = 1;
            else
                layer[(index.Z), (index.Y), (index.X)] = 0;
        }

        private static void OctreeCreationGPU()
        {
            bool xSplit, ySplit, zSplit;
            int nX = (int)layerConfig.Extent.X, nY = (int)layerConfig.Extent.Y, nZ = (int)layerConfig.Extent.Z;
            int X = (int)layerConfig.Extent.X, Y = (int)layerConfig.Extent.Y, Z = (int)layerConfig.Extent.Z;
            Index3D index = new Index3D(nX, nY, nZ);
            nLayers = (ushort)(Math.Ceiling(Math.Log(OctreeSize, 2)) + 1);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            accelerator.Synchronize();
            for (int i = 1; i < nLayers; i++)
            {
                zSplit = ySplit = xSplit = true;
                //zSplit = ySplit = xSplit = false;
                //if ((xCode > 1 << (nLayers - i - 1)))
                //{
                //    zSplit = true;
                nZ = 1 << (nLayers - i - 1);
                Z = index.Z / 2;
                //}
                //if ((yCode > 1 << (nLayers - i - 1)))
                //{
                //    ySplit = true;
                nY = 1 << (nLayers - i - 1);
                Y = index.Y / 2;
                //}
                //if ((zCode > 1 << (nLayers - i - 1)))
                //{
                //    xSplit = true;
                nX = 1 << (nLayers - i - 1);
                X = index.X / 2;
                //}
                if (i < nLayers)
                {
                    index = new Index3D(X, Y, Z);
                    getByteOctreeLayer(i) = accelerator.Allocate3DDenseXY<byte>(index);
                    if (zSplit && ySplit && xSplit)
                        branchAll(index, getByteOctreeLayer(i - 1).View, getByteOctreeLayer(i).View);
                    else if (xSplit && ySplit)
                        branchXY(index, getByteOctreeLayer(i - 1).View, getByteOctreeLayer(i).View);
                    else if (xSplit && zSplit)
                        branchXZ(index, getByteOctreeLayer(i - 1).View, getByteOctreeLayer(i).View);
                    else if (ySplit && zSplit)
                        branchYZ(index, getByteOctreeLayer(i - 1).View, getByteOctreeLayer(i).View);
                    else if (xSplit)
                        branchX(index, getByteOctreeLayer(i - 1).View, getByteOctreeLayer(i).View);
                    else if (ySplit)
                        branchY(index, getByteOctreeLayer(i - 1).View, getByteOctreeLayer(i).View);
                    else if (zSplit)
                        branchZ(index, getByteOctreeLayer(i - 1).View, getByteOctreeLayer(i).View);
                    accelerator.Synchronize();
                }
            }

            stopWatch.Stop();
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime " + elapsedTime);
            byte[,,] data = getByteOctreeLayer(nLayers - 1).GetAsArray3D();
            if (data.Length == 1)
                nTri = data[0, 0, 0];
        }

        public static void OctreeTraverseKernelX(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(layer.Extent.X));
            if (layer[index3D] > 0)
            {
                newKeys[index * 8] = keys[index];
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = (uint)(keys[index] + (4 << (3 * n)));
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
                count[index] = 2;
            }
            else
            {
                newKeys[index * 8] = 0;
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
            }
        }

        public static void OctreeTraverseKernelY(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(layer.Extent.X));
            if (layer[index3D] > 0)
            {
                newKeys[index * 8] = keys[index];
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = (uint)(keys[index] + (2 << (3 * n)));
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
                count[index] = 2;
            }
            else
            {
                newKeys[index * 8] = 0;
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
            }
        }

        public static void OctreeTraverseKernelZ(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(layer.Extent.X));
            if (layer[index3D] > 0)
            {
                newKeys[index * 8] = keys[index];
                newKeys[index * 8 + 1] = (uint)(keys[index] + (1 << (3 * n)));
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
                count[index] = 2;
            }
            else
            {
                newKeys[index * 8] = 0;
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
            }
        }

        public static void OctreeTraverseKernelXY(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(layer.Extent.X));
            if (layer[index3D] > 0)
            {
                newKeys[index * 8] = keys[index];
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = (uint)(keys[index] + (2 << (3 * n)));
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = (uint)(keys[index] + (4 << (3 * n)));
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = (uint)(keys[index] + (6 << (3 * n)));
                newKeys[index * 8 + 7] = 0;
                count[index] = 4;
            }
            else
            {
                newKeys[index * 8] = 0;
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
            }
        }

        public static void OctreeTraverseKernelXZ(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(layer.Extent.X));
            if (layer[index3D] > 0)
            {
                newKeys[index * 8] = keys[index];
                newKeys[index * 8 + 1] = (uint)(keys[index] + (1 << (3 * n)));
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = (uint)(keys[index] + (4 << (3 * n)));
                newKeys[index * 8 + 5] = (uint)(keys[index] + (5 << (3 * n)));
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
                count[index] = 4;
            }
            else
            {
                newKeys[index * 8] = 0;
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
            }
        }

        public static void OctreeTraverseKernelYZ(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(layer.Extent.X));
            if (layer[index3D] > 0)
            {
                newKeys[index * 8] = keys[index];
                newKeys[index * 8 + 1] = (uint)(keys[index] + (1 << (3 * n)));
                newKeys[index * 8 + 2] = (uint)(keys[index] + (2 << (3 * n)));
                newKeys[index * 8 + 3] = (uint)(keys[index] + (3 << (3 * n)));
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
                count[index] = 4;
            }
            else
            {
                newKeys[index * 8] = 0;
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
            }
        }

        public static void OctreeTraverseKernelAll(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(layer.Extent.X));
            if (layer[index3D] > 0)
            {
                newKeys[index * 8] = keys[index];
                newKeys[index * 8 + 1] = (uint)(keys[index] + (1 << (3 * n)));
                newKeys[index * 8 + 2] = (uint)(keys[index] + (2 << (3 * n)));
                newKeys[index * 8 + 3] = (uint)(keys[index] + (3 << (3 * n)));
                newKeys[index * 8 + 4] = (uint)(keys[index] + (4 << (3 * n)));
                newKeys[index * 8 + 5] = (uint)(keys[index] + (5 << (3 * n)));
                newKeys[index * 8 + 6] = (uint)(keys[index] + (6 << (3 * n)));
                newKeys[index * 8 + 7] = (uint)(keys[index] + (7 << (3 * n)));
                count[index] = 8;
            }
            else
            {
                newKeys[index * 8] = 0;
                newKeys[index * 8 + 1] = 0;
                newKeys[index * 8 + 2] = 0;
                newKeys[index * 8 + 3] = 0;
                newKeys[index * 8 + 4] = 0;
                newKeys[index * 8 + 5] = 0;
                newKeys[index * 8 + 6] = 0;
                newKeys[index * 8 + 7] = 0;
            }
        }

        public static void OctreeFinalLayer(Index1D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView1D<Triangle, Stride1D.Dense> triangles, ArrayView1D<Edge, Stride1D.Dense> triTable, ushort threshold, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index], n);

            index3D = new Index3D(index3D.Z, index3D.Y, index3D.X);

            if (layer[index3D.Z, index3D.Y, index3D.X] > 0)
            {
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
                tempCube.Config(threshold);
                Point[] vertice = tempCube.MarchGPU(threshold, triTable[tempCube.config]);
                for (int i = 0; i < 12; i += 3)
                {
                    if ((vertice[i].X > 0 || vertice[i].Y > 0 || vertice[i].Z > 0) ||
                        (vertice[i + 1].X > 0 || vertice[i + 1].Y > 0 || vertice[i + 1].Z > 0) ||
                        (vertice[i + 2].X > 0 || vertice[i + 2].Y > 0 || vertice[i + 2].Z > 0))
                    {
                        triangles[index * 5 + (i / 3)] = new Triangle(vertice[i], vertice[i + 1], vertice[i + 2]);
                    }
                }
            }
        }

        private static void OctreeTraversalGPU(StreamWriter fs)
        {
            int n;
            var cnt = accelerator.Allocate1D<uint>(1);
            cnt.MemSetToZero();
            var newKeys = accelerator.Allocate1D<uint>(new uint[] { 0 });
            uint[] k = { 0 };
            k[0] <<= ((nLayers - 1) * 3);

            var keys = accelerator.Allocate1D<uint>(k);
            Index1D index = new Index1D(1);
            uint[] karray = Enumerable.Range(0, nTri).Select(x => (uint)x).ToArray();
            MemoryBuffer<ArrayView1D<uint, Stride1D.Dense>> sum = accelerator.Allocate1D<uint>(1);
            accelerator.Synchronize();
            var radixSort = accelerator.CreateRadixSort<uint, Stride1D.Dense, DescendingUInt32>();
            var prefixSum = accelerator.CreateReduction<uint, Stride1D.Dense, AddUInt32>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (n = nLayers - 1; n >= 0; n--)
            {
                bool xSplit = true, ySplit = true, zSplit = true;
                //if ((zCode <= 1 << (n - 1)))
                //{
                //    zSplit = false;
                //}
                //if ((yCode <= 1 << (n - 1)))
                //{
                //    ySplit = false;
                //}
                //if ((xCode <= 1 << (n - 1)))
                //{
                //    xSplit = false;
                //}
                newKeys = accelerator.Allocate1D<uint>(index.Size * 8);
                if (zSplit && ySplit && xSplit)
                    traversalKernelAll(index, getByteOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (xSplit && ySplit)
                    traversalKernelXY(index, getByteOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (xSplit && zSplit)
                    traversalKernelXZ(index, getByteOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (ySplit && zSplit)
                    traversalKernelYZ(index, getByteOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (xSplit)
                    traversalKernelX(index, getByteOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (ySplit)
                    traversalKernelY(index, getByteOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (zSplit)
                    traversalKernelZ(index, getByteOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                accelerator.Synchronize();

                var tempMemSize = accelerator.ComputeRadixSortTempStorageSize<uint, DescendingUInt32>((Index1D)newKeys.Length);
                using (var tempBuffer = accelerator.Allocate1D<int>(tempMemSize))
                {
                    radixSort(
                        accelerator.DefaultStream,
                        newKeys.View,
                        tempBuffer.View);
                }

                prefixSum(accelerator.DefaultStream, cnt, sum.View);

                accelerator.Synchronize();
                if (n > 0)
                {
                    getByteOctreeLayer(n).Dispose();
                }

                keys = newKeys;
                index = new Index1D((int)sum.GetAsArray1D()[0]);
                cnt = accelerator.Allocate1D<uint>(index.Size);
                cnt.MemSetToZero();
            }

            index = new Index1D(index.Size / 8);
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime:" + elapsedTime + ", Batch Size:" + batchSize);
            stopWatch.Reset();

            Triangle[] tri = new Triangle[index * 5];
            PageLockedArray1D<Triangle> triLocked = accelerator.AllocatePageLocked1D<Triangle>(index * 5);
            MemoryBuffer1D<Triangle, Stride1D.Dense> triConfig = accelerator.Allocate1D<Triangle>(index * 5);
            Console.WriteLine("Cubes: " + index.Size);

            //List<Index3D> templ = keys.GetAsArray1D().Select(x => getFromShuffleXYZ(x, nLayers - 1)).ToList();

            //templ.FindAll(x => x.X == templ[0].X && x.Y == templ[0].Y && x.Z == templ[0].Z);

            stopWatch.Start();

            octreeFinalLayer(index, getByteOctreeLayer(0).View, keys.View, sliced.View, triConfig, triTable.View, thresh, nLayers - 1);

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            count = 0;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime:" + elapsedTime + ", Batch Size:" + batchSize);


            triConfig.View.CopyToPageLockedAsync(triLocked);
            accelerator.Synchronize();
            tri = triLocked.GetArray();
            triConfig.Dispose();
            sliced.Dispose();
            triTable.Dispose();

            var triC = tri.Where(x => (x.vertex1.X != 0 && x.vertex1.Y != 0 && x.vertex1.Z != 0) &&
            (x.vertex2.X != 0 && x.vertex2.Y != 0 && x.vertex2.Z != 0) &&
            (x.vertex3.X != 0 && x.vertex3.Y != 0 && x.vertex3.Z != 0)).ToList();
            count = 0;
            foreach (var triangle in triC)
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

        public static void Assign(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> layer, ArrayView3D<ushort, Stride3D.DenseXY> input, int thresh)
        {
            byte cubeByte = 0;
            cubeByte += (input[(index.Z), (index.Y), (index.X)] < thresh) ? (byte)0x01 : (byte)0;
            cubeByte += (input[(index.Z), (index.Y), (index.X) + 1] < thresh) ? (byte)0x01 : (byte)0;
            cubeByte += (input[(index.Z), (index.Y) + 1, (index.X) + 1] < thresh) ? (byte)0x01 : (byte)0;
            cubeByte += (input[(index.Z), (index.Y) + 1, (index.X)] < thresh) ? (byte)0x01 : (byte)0;
            cubeByte += (input[(index.Z) + 1, (index.Y), (index.X)] < thresh) ? (byte)0x01 : (byte)0;
            cubeByte += (input[(index.Z) + 1, (index.Y), (index.X) + 1] < thresh) ? (byte)0x01 : (byte)0;
            cubeByte += (input[(index.Z) + 1, (index.Y) + 1, (index.X) + 1] < thresh) ? (byte)0x01 : (byte)0;
            cubeByte += (input[(index.Z) + 1, (index.Y) + 1, (index.X)] < thresh) ? (byte)0x01 : (byte)0;

            if (cubeByte == 0 || cubeByte == 8)
                layer[(index.Z), (index.Y), (index.X)] = 0;
            else
                layer[(index.Z), (index.Y), (index.X)] = 1;
        }

        public static void MarchingCubesGPU()
        {
            Index3D index = new Index3D(slices.GetLength(2) - 1, slices.GetLength(1) - 1, slices.GetLength(0) - 1); ;

            int Z = (int)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(0), 2)));
            int Y = (int)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(1), 2)));
            int X = (int)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(2), 2)));
            int max = Math.Max(Z, X);
            Z = Y = X = max;

            //bit order 
            // i,j,k 
            // i+1,j,k
            // i+1,j+1,k
            // i,j+1,k
            // i,j,k+1
            // i+1,j,k+1
            // i+1,j+1,k+1
            // i,j+1,k+1

            var octreeLayer = new byte[Z, Y, X];
            var layerPinned = GCHandle.Alloc(octreeLayer, GCHandleType.Pinned);
            PageLockedArray3D<byte> layerLocked = accelerator.AllocatePageLocked3D<byte>(new Index3D(Z, Y, X));
            layerConfig = accelerator.Allocate3DDenseXY<byte>(layerLocked.Extent);
            var layerScope = accelerator.CreatePageLockFromPinned<byte>(layerPinned.AddrOfPinnedObject(), octreeLayer.Length);
            layerConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, layerScope);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            assign(index, layerConfig.View, sliced.View, thresh);

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            layerConfig.CopyToCPU(octreeLayer);
            layerPinned.Free();

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime " + elapsedTime);
        }


        public static ref MemoryBuffer3D<byte, Stride3D.DenseXY> getByteOctreeLayer(int index)
        {
            switch (index)
            {
                case 1:
                    return ref byteLayer1;
                case 2:
                    return ref byteLayer2;
                case 3:
                    return ref byteLayer3;
                case 4:
                    return ref byteLayer4;
                case 5:
                    return ref byteLayer5;
                case 6:
                    return ref byteLayer6;
                case 7:
                    return ref byteLayer7;
                case 8:
                    return ref byteLayer8;
                case 9:
                    return ref byteLayer9;
                case 10:
                    return ref byteLayer10;
                case 11:
                    return ref byteLayer11;
                case 12:
                    return ref byteLayer12;
                case 13:
                    return ref byteLayer13;
                case 14:
                    return ref byteLayer14;
                case 15:
                    return ref byteLayer15;
                default:
                    return ref layerConfig;
            }
        }
    }
}