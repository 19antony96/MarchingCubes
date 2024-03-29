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
    class OctreeEvenSubdiv : MarchingCubes
    {
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> assign;
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> branchAll;
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> branchX;
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> branchY;
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> branchZ;
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> branchXY;
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> branchXZ;
        public static Action<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>> branchYZ;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelAll;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelX;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelY;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelZ;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelXY;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelXZ;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int> traversalKernelYZ;
        public static Action<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<Triangle, Stride1D.Dense>, ArrayView1D<Edge, Stride1D.Dense>, ushort, int> octreeFinalLayer;


        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minConfig;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxConfig;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysConfig;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer15;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer14;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer13;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer12;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer11;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer10;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer9;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer8;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer7;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer6;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer5;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer4;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer3;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer2;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> maxLayer1;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer15;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer14;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer13;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer12;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer11;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer10;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer9;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer8;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer7;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer6;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer5;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer4;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer3;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer2;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> minLayer1;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer15;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer14;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer13;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer12;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer11;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer10;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer9;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer8;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer7;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer6;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer5;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer4;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer3;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer2;
        public static MemoryBuffer3D<uint, Stride3D.DenseXY> keysLayer1;

        public static ushort xCode;
        public static ushort yCode;
        public static ushort zCode;

        public static TimeSpan ts = new TimeSpan();
        public static int count = 0;

        public OctreeEvenSubdiv(int size)
        {
            Console.WriteLine("OctreeEvenSubdiv");
            ushort i = 0;
            FileInfo fi = CreateVolume(size);

            xCode = (ushort)(slices.GetLength(2) - 1);
            yCode = (ushort)(slices.GetLength(1) - 1);
            zCode = (ushort)(slices.GetLength(0) - 1);

            //var s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
            //var p = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Point));
            //var n = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal));

            assign = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(Assign);
            branchAll = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(BranchAll);
            branchX = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(BranchX);
            branchY = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(BranchY);
            branchZ = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(BranchZ);
            branchXY = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(BranchXY);
            branchXZ = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(BranchXZ);
            branchYZ = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>>(BranchYZ);
            traversalKernelAll = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelAll);
            traversalKernelX = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelX);
            traversalKernelY = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelY);
            traversalKernelZ = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelZ);
            traversalKernelXY = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelXY);
            traversalKernelXZ = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelXZ);
            traversalKernelYZ = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, ArrayView1D<uint, Stride1D.Dense>, int>(OctreeTraverseKernelYZ);
            octreeFinalLayer = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<uint, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<Triangle, Stride1D.Dense>, ArrayView1D<Edge, Stride1D.Dense>, ushort, int>(OctreeFinalLayer);

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
                if (getMinOctreeLayer(i) != null && !getMinOctreeLayer(i).IsDisposed)
                    getMinOctreeLayer(i).Dispose();
                if (getMaxOctreeLayer(i) != null && !getMaxOctreeLayer(i).IsDisposed)
                    getMaxOctreeLayer(i).Dispose();
            }
            accelerator.Dispose();
            context.Dispose();
        }

        public static void BranchX(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> minPrev, ArrayView3D<ushort, Stride3D.DenseXY> maxPrev, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max)
        {
            min[index] = XMath.Min(minPrev[(index.X) * 2, (index.Y), (index.Z)], minPrev[(index.X) * 2 + 1, (index.Y), (index.Z)]);
            max[index] = XMath.Max(maxPrev[(index.X) * 2, (index.Y), (index.Z)], maxPrev[(index.X) * 2 + 1, (index.Y), (index.Z)]);
        }

        public static void BranchY(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> minPrev, ArrayView3D<ushort, Stride3D.DenseXY> maxPrev, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max)
        {
            min[index] = XMath.Min(minPrev[(index.X), (index.Y) * 2, (index.Z)], minPrev[(index.X), (index.Y) * 2 + 1, (index.Z)]);
            max[index] = XMath.Max(maxPrev[(index.X), (index.Y) * 2, (index.Z)], maxPrev[(index.X), (index.Y) * 2 + 1, (index.Z)]);
        }
        public static void BranchZ(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> minPrev, ArrayView3D<ushort, Stride3D.DenseXY> maxPrev, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max)
        {
            min[index] = XMath.Min(minPrev[(index.X), (index.Y), (index.Z) * 2], minPrev[(index.X), (index.Y), (index.Z) * 2 + 1]);
            max[index] = XMath.Max(maxPrev[(index.X), (index.Y), (index.Z) * 2], maxPrev[(index.X), (index.Y), (index.Z) * 2 + 1]);
        }
        public static void BranchXY(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> minPrev, ArrayView3D<ushort, Stride3D.DenseXY> maxPrev, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max)
        {

            ushort[] tempMax = new[] {maxPrev[(index.X)*2, (index.Y)*2, (index.Z)],
                maxPrev[(index.X)*2, (index.Y)*2 + 1, (index.Z)],
                maxPrev[(index.X)*2 + 1, (index.Y)*2, (index.Z)],
                maxPrev[(index.X)*2 + 1, (index.Y)*2 + 1, (index.Z)] };

            ushort[] tempMin = new[] {minPrev[(index.X)*2, (index.Y)*2, (index.Z)],
                minPrev[(index.X)*2, (index.Y)*2 + 1, (index.Z)],
                minPrev[(index.X)*2 + 1, (index.Y)*2, (index.Z)],
                minPrev[(index.X)*2 + 1, (index.Y)*2 + 1, (index.Z)]};

            ushort tMax = tempMax[0];
            ushort tMin = tempMin[0];
            for (int i = 1; i < 4; i++)
            {
                if (tempMax[i] > tMax) tMax = tempMax[i];
                if (tempMin[i] < tMin) tMin = tempMin[i];
            }
            min[index] = tMin;
            max[index] = tMax;
        }
        public static void BranchXZ(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> minPrev, ArrayView3D<ushort, Stride3D.DenseXY> maxPrev, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max)
        {

            ushort[] tempMax = new[] {maxPrev[(index.X)*2, (index.Y), (index.Z)*2],
                maxPrev[(index.X)*2, (index.Y), (index.Z)*2 + 1],
                maxPrev[(index.X)*2 + 1, (index.Y), (index.Z)*2],
                maxPrev[(index.X)*2 + 1, (index.Y), (index.Z)*2 + 1] };

            ushort[] tempMin = new[] {minPrev[(index.X)*2, (index.Y), (index.Z)*2],
                minPrev[(index.X)*2, (index.Y), (index.Z)*2 + 1],
                minPrev[(index.X)*2 + 1, (index.Y), (index.Z)*2],
                minPrev[(index.X)*2 + 1, (index.Y), (index.Z)*2 + 1] };

            ushort tMax = tempMax[0];
            ushort tMin = tempMin[0];
            for (int i = 1; i < 4; i++)
            {
                if (tempMax[i] > tMax) tMax = tempMax[i];
                if (tempMin[i] < tMin) tMin = tempMin[i];
            }
            min[index] = tMin;
            max[index] = tMax;
        }
        public static void BranchYZ(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> minPrev, ArrayView3D<ushort, Stride3D.DenseXY> maxPrev, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max)
        {

            ushort[] tempMax = new[] {maxPrev[(index.X), (index.Y)*2, (index.Z)*2],
                maxPrev[(index.X), (index.Y)*2, (index.Z)*2 + 1],
                maxPrev[(index.X), (index.Y)*2 + 1, (index.Z)*2 + 1],
                maxPrev[(index.X), (index.Y)*2 + 1, (index.Z)*2] };

            ushort[] tempMin = new[] {minPrev[(index.X), (index.Y)*2, (index.Z)*2],
                minPrev[(index.X), (index.Y)*2, (index.Z)*2 + 1],
                minPrev[(index.X), (index.Y)*2 + 1, (index.Z)*2 + 1],
                minPrev[(index.X), (index.Y)*2 + 1, (index.Z)*2] };

            ushort tMax = tempMax[0];
            ushort tMin = tempMin[0];
            for (int i = 1; i < 4; i++)
            {
                if (tempMax[i] > tMax) tMax = tempMax[i];
                if (tempMin[i] < tMin) tMin = tempMin[i];
            }
            min[index] = tMin;
            max[index] = tMax;
        }
        public static void BranchAll(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> minPrev, ArrayView3D<ushort, Stride3D.DenseXY> maxPrev, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max)
        {

            ushort[] tempMax = new[] {maxPrev[(index.X)*2, (index.Y)*2, (index.Z)*2],
                maxPrev[(index.X)*2, (index.Y)*2, (index.Z)*2 + 1],
                maxPrev[(index.X)*2, (index.Y)*2 + 1, (index.Z)*2 + 1],
                maxPrev[(index.X)*2, (index.Y)*2 + 1, (index.Z)*2],
                maxPrev[(index.X)*2 + 1, (index.Y)*2, (index.Z)*2],
                maxPrev[(index.X)*2 + 1, (index.Y)*2, (index.Z)*2 + 1],
                maxPrev[(index.X)*2 + 1, (index.Y)*2 + 1, (index.Z)*2 + 1],
                maxPrev[(index.X)*2 + 1, (index.Y)*2 + 1, (index.Z)*2] };

            ushort[] tempMin = new[] {minPrev[(index.X)*2, (index.Y)*2, (index.Z)*2],
                minPrev[(index.X)*2, (index.Y)*2, (index.Z)*2 + 1],
                minPrev[(index.X)*2, (index.Y)*2 + 1, (index.Z)*2 + 1],
                minPrev[(index.X)*2, (index.Y)*2 + 1, (index.Z)*2],
                minPrev[(index.X)*2 + 1, (index.Y)*2, (index.Z)*2],
                minPrev[(index.X)*2 + 1, (index.Y)*2, (index.Z)*2 + 1],
                minPrev[(index.X)*2 + 1, (index.Y)*2 + 1, (index.Z)*2 + 1],
                minPrev[(index.X)*2 + 1, (index.Y)*2 + 1, (index.Z)*2]};

            ushort tMax = tempMax[0];
            ushort tMin = tempMin[0];
            for (int i = 1; i < 8; i++)
            {
                if (tempMax[i] > tMax) tMax = tempMax[i];
                if (tempMin[i] < tMin) tMin = tempMin[i];
            }
            min[index] = tMin;
            max[index] = tMax;
        }

        private static void OctreeCreationGPU()
        {
            bool xSplit, ySplit, zSplit;
            int nX = (int)minConfig.Extent.X, nY = (int)minConfig.Extent.Y, nZ = (int)minConfig.Extent.Z;
            int X = (int)minConfig.Extent.X, Y = (int)minConfig.Extent.Y, Z = (int)minConfig.Extent.Z;
            Index3D index = new Index3D(nX, nY, nZ);
            nLayers = (ushort)(Math.Ceiling(Math.Log(OctreeSize, 2)) + 1);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            accelerator.Synchronize();
            for (int i = 1; i < nLayers; i++)
            {
                zSplit = ySplit = xSplit = false;
                if ((xCode > 1 << (nLayers - i - 1)))
                {
                    zSplit = true;
                    nZ = 1 << (nLayers - i - 1);
                    Z = index.Z / 2;
                }
                if ((yCode > 1 << (nLayers - i - 1)))
                {
                    ySplit = true;
                    nY = 1 << (nLayers - i - 1);
                    Y = index.Y / 2;
                }
                if ((zCode > 1 << (nLayers - i - 1)))
                {
                    xSplit = true;
                    nX = 1 << (nLayers - i - 1);
                    X = index.X / 2;
                }
                if (i < nLayers)
                {
                    index = new Index3D(X, Y, Z);
                    getMinOctreeLayer(i) = accelerator.Allocate3DDenseXY<ushort>(index);
                    getMaxOctreeLayer(i) = accelerator.Allocate3DDenseXY<ushort>(index);
                    if(zSplit && ySplit && xSplit)
                        branchAll(index, getMinOctreeLayer(i - 1).View, getMaxOctreeLayer(i - 1).View, getMinOctreeLayer(i).View, getMaxOctreeLayer(i).View);
                    else if(xSplit && ySplit)
                        branchXY(index, getMinOctreeLayer(i - 1).View, getMaxOctreeLayer(i - 1).View, getMinOctreeLayer(i).View, getMaxOctreeLayer(i).View);
                    else if (xSplit && zSplit)
                        branchXZ(index, getMinOctreeLayer(i - 1).View, getMaxOctreeLayer(i - 1).View, getMinOctreeLayer(i).View, getMaxOctreeLayer(i).View);
                    else if (ySplit && zSplit)
                        branchYZ(index, getMinOctreeLayer(i - 1).View, getMaxOctreeLayer(i - 1).View, getMinOctreeLayer(i).View, getMaxOctreeLayer(i).View);
                    else if (xSplit)
                        branchX(index, getMinOctreeLayer(i - 1).View, getMaxOctreeLayer(i - 1).View, getMinOctreeLayer(i).View, getMaxOctreeLayer(i).View);
                    else if (ySplit)
                        branchY(index, getMinOctreeLayer(i - 1).View, getMaxOctreeLayer(i - 1).View, getMinOctreeLayer(i).View, getMaxOctreeLayer(i).View);
                    else if (zSplit)
                        branchZ(index, getMinOctreeLayer(i - 1).View, getMaxOctreeLayer(i - 1).View, getMinOctreeLayer(i).View, getMaxOctreeLayer(i).View);
                    accelerator.Synchronize();
                }
            }

            stopWatch.Stop();
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime " + elapsedTime);
            ushort[,,] data = getMinOctreeLayer(nLayers - 1).GetAsArray3D();
            if (data.Length == 1)
                nTri = data[0, 0, 0];
        }

        public static void OctreeTraverseKernelX(Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(max.Extent.X));
            if (max[index3D] > thresh && min[index3D] < thresh)
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
        
        public static void OctreeTraverseKernelY(Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(max.Extent.X));
            if (max[index3D] > thresh && min[index3D] < thresh)
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
        
        public static void OctreeTraverseKernelZ(Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(max.Extent.X));
            if (max[index3D] > thresh && min[index3D] < thresh)
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

        public static void OctreeTraverseKernelXY (Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(max.Extent.X));
            if (max[index3D] > thresh && min[index3D] < thresh)
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
        
        public static void OctreeTraverseKernelXZ(Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(max.Extent.X));
            if (max[index3D] > thresh && min[index3D] < thresh)
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
        
        public static void OctreeTraverseKernelYZ(Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(max.Extent.X));
            if (max[index3D] > thresh && min[index3D] < thresh)
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
        
        public static void OctreeTraverseKernelAll(Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView1D<uint, Stride1D.Dense> newKeys, ArrayView1D<uint, Stride1D.Dense> count, int n)
        {
            Index3D index3D = getFromShuffleXYZ(keys[index] >> ((n) * 3), (int)XMath.Log2(max.Extent.X));
            if (max[index3D] > thresh && min[index3D] < thresh)
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

        public static void OctreeFinalLayer(Index1D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView1D<uint, Stride1D.Dense> keys, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView1D<Triangle, Stride1D.Dense> triangles, ArrayView1D<Edge, Stride1D.Dense> triTable, ushort threshold, int n)
        {

            Index3D index3D = getFromShuffleXYZ(keys[index], n);

            index3D = new Index3D(index3D.Z, index3D.Y, index3D.X);

            if (max[index3D.Z, index3D.Y, index3D.X] > threshold && min[index3D.Z, index3D.Y, index3D.X] < threshold)
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
            for (n = nLayers - 1; n > 0; n--)
            {
                bool xSplit = true, ySplit = true, zSplit = true;
                if ((zCode < 1 << (n - 1)))
                {
                    zSplit = false;
                }
                if ((yCode < 1 << (n - 1)))
                {
                    ySplit = false;
                }
                if ((xCode < 1 << (n - 1)))
                {
                    xSplit = false;
                }
                newKeys = accelerator.Allocate1D<uint>(index.Size * 8);
                if (zSplit && ySplit && xSplit)
                    traversalKernelAll(index, getMinOctreeLayer(n).View, getMaxOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (xSplit && ySplit)
                    traversalKernelXY(index, getMinOctreeLayer(n).View, getMaxOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (xSplit && zSplit)
                    traversalKernelXZ(index, getMinOctreeLayer(n).View, getMaxOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (ySplit && zSplit)
                    traversalKernelYZ(index, getMinOctreeLayer(n).View, getMaxOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (xSplit)
                    traversalKernelX(index, getMinOctreeLayer(n).View, getMaxOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (ySplit)
                    traversalKernelY(index, getMinOctreeLayer(n).View, getMaxOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
                else if (zSplit)
                    traversalKernelZ(index, getMinOctreeLayer(n).View, getMaxOctreeLayer(n).View, keys.View.SubView(0, index.Size), newKeys.View, cnt.View, n);
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
                    getMinOctreeLayer(n).Dispose();
                    getMaxOctreeLayer(n).Dispose();
                }

                keys = newKeys;
                index = new Index1D((int)sum.GetAsArray1D()[0]);
                cnt = accelerator.Allocate1D<uint>(index.Size);
                cnt.MemSetToZero();
            }

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

            octreeFinalLayer(index, getMinOctreeLayer(0).View, getMaxOctreeLayer(0).View, keys.View, sliced.View, triConfig, triTable.View, thresh, nLayers - 1);

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

        public static void Assign(Index3D index, ArrayView3D<ushort, Stride3D.DenseXY> min, ArrayView3D<ushort, Stride3D.DenseXY> max, ArrayView3D<ushort, Stride3D.DenseXY> input)
        {
            var temp = new[] {input[(index.Z), (index.Y), (index.X)],
            input[(index.Z), (index.Y), (index.X) + 1],
            input[(index.Z), (index.Y) + 1, (index.X) + 1],
            input[(index.Z), (index.Y) + 1, (index.X)],
            input[(index.Z) + 1, (index.Y), (index.X)],
            input[(index.Z) + 1, (index.Y), (index.X) + 1],
            input[(index.Z) + 1, (index.Y) + 1, (index.X) + 1],
            input[(index.Z) + 1, (index.Y) + 1, (index.X)]};

            ushort tMax = temp[0];
            ushort tMin = temp[0];
            for (int i = 1; i < 8; i++)
            {
                if (temp[i] > tMax) tMax = temp[i];
                if (temp[i] < tMin) tMin = temp[i];
            }
            min[(index.Z), (index.Y), (index.X)] = tMin;
            max[(index.Z), (index.Y), (index.X)] = tMax;
        }

        public static void MarchingCubesGPU()
        {
            Index3D index = new Index3D(slices.GetLength(2) - 1, slices.GetLength(1) - 1, slices.GetLength(0) - 1);;

            int Z = (int)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(0), 2)));
            int Y = (int)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(1), 2)));
            int X = (int)Math.Pow(2, Math.Ceiling(Math.Log(slices.GetLength(2), 2)));

            //bit order 
            // i,j,k 
            // i+1,j,k
            // i+1,j+1,k
            // i,j+1,k
            // i,j,k+1
            // i+1,j,k+1
            // i+1,j+1,k+1
            // i,j+1,k+1

            var octreeMin = new ushort[Z, Y, X];
            var octreeMax = new ushort[Z, Y, X];
            var minPinned = GCHandle.Alloc(octreeMin, GCHandleType.Pinned);
            var maxPinned = GCHandle.Alloc(octreeMax, GCHandleType.Pinned);
            PageLockedArray3D<ushort> minLocked = accelerator.AllocatePageLocked3D<ushort>(new Index3D(Z, Y, X));
            PageLockedArray3D<ushort> maxLocked = accelerator.AllocatePageLocked3D<ushort>(new Index3D(Z, Y, X));
            minConfig = accelerator.Allocate3DDenseXY<ushort>(minLocked.Extent);
            maxConfig = accelerator.Allocate3DDenseXY<ushort>(maxLocked.Extent);
            var minScope = accelerator.CreatePageLockFromPinned<ushort>(minPinned.AddrOfPinnedObject(), octreeMin.Length);
            var maxScope = accelerator.CreatePageLockFromPinned<ushort>(maxPinned.AddrOfPinnedObject(), octreeMax.Length);
            minConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, minScope);
            maxConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, maxScope);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            assign(index, minConfig.View, maxConfig.View, sliced.View);

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            minConfig.CopyToCPU(octreeMin);
            maxConfig.CopyToCPU(octreeMax);
            minPinned.Free();
            maxPinned.Free();

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime " + elapsedTime);
        }


        public static ref MemoryBuffer3D<ushort, Stride3D.DenseXY> getMinOctreeLayer(int index)
        {
            switch (index)
            {
                case 1:
                    return ref minLayer1;
                case 2:
                    return ref minLayer2;
                case 3:
                    return ref minLayer3;
                case 4:
                    return ref minLayer4;
                case 5:
                    return ref minLayer5;
                case 6:
                    return ref minLayer6;
                case 7:
                    return ref minLayer7;
                case 8:
                    return ref minLayer8;
                case 9:
                    return ref minLayer9;
                case 10:
                    return ref minLayer10;
                case 11:
                    return ref minLayer11;
                case 12:
                    return ref minLayer12;
                case 13:
                    return ref minLayer13;
                case 14:
                    return ref minLayer14;
                case 15:
                    return ref minLayer15;
                default:
                    return ref minConfig;
            }
        }

        public static ref MemoryBuffer3D<ushort, Stride3D.DenseXY> getMaxOctreeLayer(int index)
        {
            switch (index)
            {
                case 1:
                    return ref maxLayer1;
                case 2:
                    return ref maxLayer2;
                case 3:
                    return ref maxLayer3;
                case 4:
                    return ref maxLayer4;
                case 5:
                    return ref maxLayer5;
                case 6:
                    return ref maxLayer6;
                case 7:
                    return ref maxLayer7;
                case 8:
                    return ref maxLayer8;
                case 9:
                    return ref maxLayer9;
                case 10:
                    return ref maxLayer10;
                case 11:
                    return ref maxLayer11;
                case 12:
                    return ref maxLayer12;
                case 13:
                    return ref maxLayer13;
                case 14:
                    return ref maxLayer14;
                case 15:
                    return ref maxLayer15;
                default:
                    return ref maxConfig;
            }
        }

        public static ref MemoryBuffer3D<uint, Stride3D.DenseXY> getKeysOctreeLayer(int index)
        {
            switch (index)
            {
                case 1:
                    return ref keysLayer1;
                case 2:
                    return ref keysLayer2;
                case 3:
                    return ref keysLayer3;
                case 4:
                    return ref keysLayer4;
                case 5:
                    return ref keysLayer5;
                case 6:
                    return ref keysLayer6;
                case 7:
                    return ref keysLayer7;
                case 8:
                    return ref keysLayer8;
                case 9:
                    return ref keysLayer9;
                case 10:
                    return ref keysLayer10;
                case 11:
                    return ref keysLayer11;
                case 12:
                    return ref keysLayer12;
                case 13:
                    return ref keysLayer13;
                case 14:
                    return ref keysLayer14;
                case 15:
                    return ref keysLayer15;
                default:
                    return ref keysConfig;
            }
        }
    }
}