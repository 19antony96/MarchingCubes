using System;
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
    class NaiveGPU : MarchingCubes
    {
        public static Action<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int> assign;
        public static Action<Index3D, ArrayView<Triangle>, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<Edge, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<byte, Stride1D.Dense>, Point, int, int, int> get_verts;


        public static MemoryBuffer3D<byte, Stride3D.DenseXY> cubeConfig;

        public static byte[,,] cubeBytes;

        public static TimeSpan ts = new TimeSpan();

        public static List<Point> vertices = new List<Point>();
        public static byte[,,] cubes;
        public static int count = 0;

        public NaiveGPU(int size)
        {
            ushort i = 0;
            FileInfo fi = CreateVolume(size);

            //var s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
            //var p = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Point));
            //var n = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal));

            assign = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView<Edge>, int, int>(Assign1D);
            get_verts = accelerator.LoadAutoGroupedStreamKernel<Index3D, ArrayView<Triangle>, ArrayView3D<byte, Stride3D.DenseXY>, ArrayView1D<Edge, Stride1D.Dense>, ArrayView3D<ushort, Stride3D.DenseXY>, ArrayView1D<byte, Stride1D.Dense>, Point, int, int, int>(getVertices);

            cubes = MarchingCubesGPU();

            using (StreamWriter fs = fi.CreateText())
            {
                MarchGPU(fs);

                int f = 0;
                for (f = 1; f < count - 1; f += 3)
                {
                    fs.WriteLine("f " + f + " " + (f + 1) + " " + (f + 2));
                }
                Console.WriteLine(count);
            }
        }


        public static void Assign1D(Index3D index, ArrayView3D<byte, Stride3D.DenseXY> edges, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView<Edge> triTable, int thresh, int width)
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


            cubeBytes = new byte[(index.X), (index.Y), (index.Z)];
            var cubePinned = GCHandle.Alloc(cubeBytes, GCHandleType.Pinned);
            PageLockedArray3D<byte> cubeLocked = accelerator.AllocatePageLocked3D<byte>(new Index3D(index.X, index.Y, index.Z));
            cubeConfig = accelerator.Allocate3DDenseXY<byte>(cubeLocked.Extent);
            var cubeScope = accelerator.CreatePageLockFromPinned<byte>(cubePinned.AddrOfPinnedObject(), cubeBytes.Length);
            cubeConfig.AsContiguous().CopyFromPageLockedAsync(accelerator.DefaultStream, cubeScope);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            assign(index, cubeConfig.View, sliced.View, triTable.View, threshold, width);

            accelerator.Synchronize();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            cubeConfig.AsContiguous().CopyToPageLockedAsync(cubeLocked);
            cubeBytes = cubeLocked.GetArray();
            cubePinned.Free();

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            return (cubeBytes);
        }

        public static void getVertices(Index3D index, ArrayView<Triangle> triangles, ArrayView3D<byte, Stride3D.DenseXY> edges, ArrayView1D<Edge, Stride1D.Dense> triTable, ArrayView3D<ushort, Stride3D.DenseXY> input, ArrayView1D<byte, Stride1D.Dense> flag, Point offset, int thresh, int batchSize, int width)
        {
            if (edges[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X)] != 0 && edges[(index.Z + (int)offset.Z), (index.Y + (int)offset.Y), (index.X + (int)offset.X)] != byte.MaxValue)
            {
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
                for (i = 0; i < 12; i += 3)
                {
                    if ((vertice[i].X > 0 || vertice[i].Y > 0 || vertice[i].Z > 0) ||
                        (vertice[i + 1].X > 0 || vertice[i + 1].Y > 0 || vertice[i + 1].Z > 0) ||
                        (vertice[i + 2].X > 0 || vertice[i + 2].Y > 0 || vertice[i + 2].Z > 0))
                    {
                        if (flag[0] == 0)
                            flag[0] = 1;
                        triangles[(batchSize * batchSize * batchSize * (i / 3)) + (index.Z * batchSize * batchSize) + (index.Y * batchSize) + index.X] = new Triangle(vertice[i], vertice[i + 1], vertice[i + 2]);
                    }
                }
            }
        }



        public static void MarchGPU(StreamWriter fs)
        {
            ushort[] sizes = { 16 };
            if (width < 200)
                sizes = new ushort[] { (ushort)width };
            foreach (ushort size in sizes)
            {
                batchSize = size;

                int i, j, k;
                int Z = slices.GetLength(0) - 1;
                int Y = width - 1;
                int X = length - 1;
                Index3D Nindex = new Index3D(X, Y, Z);
                int nX = (int)Math.Ceiling((double)(X / batchSize));
                int nY = (int)Math.Ceiling((double)(Y / batchSize));
                int nZ = (int)Math.Ceiling((double)(Z / batchSize));

                Triangle[] triangleList = new Triangle[Math.Max(Nindex.Size, (nX + 1) * (nY + 1) * (nZ + 1) * batchSize * batchSize * batchSize) * 5];
                int sum = 0;
                Triangle[] tri = new Triangle[Math.Min((Nindex.X) * (Nindex.Y) * (Nindex.Z) * 5 + 1, (batchSize) * (batchSize) * (batchSize) * 5 + 1)];
                PageLockedArray1D<Triangle> triLocked = accelerator.AllocatePageLocked1D<Triangle>(Math.Min((Nindex.X) * (Nindex.Y) * (Nindex.Z) * 5 + 1, (batchSize) * (batchSize) * (batchSize) * 5 + 1));
                MemoryBuffer1D<Triangle, Stride1D.Dense> triConfig = accelerator.Allocate1D<Triangle>(Math.Min((Nindex.X) * (Nindex.Y) * (Nindex.Z) * 5 + 1, (batchSize) * (batchSize) * (batchSize) * 5 + 1));
                MemoryBuffer1D<byte, Stride1D.Dense> flag = accelerator.Allocate1D<byte>(1);
                cubeConfig = accelerator.Allocate3DDenseXY(cubes);
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
                            Index3D index = (Math.Min(Nindex.X - i * batchSize, batchSize), Math.Min(Nindex.Y - j * batchSize, batchSize), Math.Min(Nindex.Z - k * batchSize, batchSize));
                            if (index.Size > 0)
                            {
                                Point offset = new Point() { X = i * batchSize, Y = j * batchSize, Z = k * batchSize };

                                get_verts(index, triConfig.View, cubeConfig.View, triTable, sliced.View, flag.View, offset, threshold, batchSize, width);

                                accelerator.Synchronize();
                                if (flag.GetAsArray1D()[0] > 0)
                                {
                                    triConfig.View.CopyToPageLockedAsync(triLocked);
                                    accelerator.Synchronize();
                                    tri = triLocked.GetArray();
                                    tri[tri.Length - 1] = new Triangle();
                                    Array.Copy(tri, 0, triangleList, iX * (tri.Length - 1), tri.Length - 1);
                                    sum += tri.Length;
                                    triConfig.View.MemSetToZero();
                                    triLocked.ArrayView.MemSetToZero();
                                    iX++;
                                    flag.MemSetToZero();
                                }
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

                triConfig.Dispose();
                cubeConfig.Dispose();
                triTable.Dispose();
                sliced.Dispose();
                var triC = triangleList.Where(x => (x.vertex1.X != 0 && x.vertex1.Y != 0 && x.vertex1.Z != 0) &&
                (x.vertex2.X != 0 && x.vertex2.Y != 0 && x.vertex2.Z != 0) &&
                (x.vertex3.X != 0 && x.vertex3.Y != 0 && x.vertex3.Z != 0)).ToList();
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
        }
    }
}