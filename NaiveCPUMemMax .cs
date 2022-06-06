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
    class NaiveCPUMemMax : MarchingCubes
    {

        public static TimeSpan ts = new TimeSpan();

        public static Normal[,,] normals;
        public static List<Point> vertices = new List<Point>();
        public static Edge[,,] cubes;
        public static Edge[] edges;
        public static int count = 0;

        public NaiveCPUMemMax(int size)
        {
            ushort i = 0;
            FileInfo fi = CreateVolume(size);

            //var s = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
            //var p = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Point));
            //var n = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Normal));

            var temp = MarchingCubesCPU();

            cubes = temp.configs;
            normals = temp.grads;

            using (StreamWriter fs = fi.CreateText())
            {
                MarchCPU(fs);

                int f = 0;
                for (f = 1; f < count - 1; f += 3)
                {
                    fs.WriteLine("f " + f + " " + (f + 1) + " " + (f + 2));
                }
                Console.WriteLine(count);
            }
        }

        private static (Normal[,,] grads, Edge[,,] configs) MarchingCubesCPU()
        {
            Edge[,,] edges = new Edge[(slices.GetLength(0) - 1), (width - 1), (length - 1)];
            byte cubeBytes;
            Normal[,,] grads = new Normal[(slices.GetLength(0)), (width), (length)];

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
                        grads[k, j, i] =
                        new Normal(
                             slices[k, j, Math.Min((width) - 1, i + 1)] - slices[k, j, Math.Max(i - 1, 0)],
                             slices[k, Math.Min((width) - 1, j + 1), i] - slices[k, Math.Max(j - 1, 0), i],
                             slices[Math.Min(slices.GetLength(0) - 1, k + 1), j, i] - slices[Math.Max(k - 1, 0), j, i]
                         );
                        if (k != slices.GetLength(0) - 1 && j != slices.GetLength(1) - 1 && i != slices.GetLength(2) - 1)
                        {
                            cubeBytes = 0;
                            cubeBytes += (slices[k, j, i] < threshold) ? (byte)0x01 : (byte)0;
                            cubeBytes += (slices[k, j, i + 1] < threshold) ? (byte)0x02 : (byte)0;
                            cubeBytes += (slices[k, j + 1, i + 1] < threshold) ? (byte)0x04 : (byte)0;
                            cubeBytes += (slices[k, j + 1, i] < threshold) ? (byte)0x08 : (byte)0;
                            cubeBytes += (slices[k + 1, j, i] < threshold) ? (byte)0x10 : (byte)0;
                            cubeBytes += (slices[k + 1, j, i + 1] < threshold) ? (byte)0x20 : (byte)0;
                            cubeBytes += (slices[k + 1, j + 1, i + 1] < threshold) ? (byte)0x40 : (byte)0;
                            cubeBytes += (slices[k + 1, j + 1, i] < threshold) ? (byte)0x80 : (byte)0;

                            edges[k, j, i] = triangleTable[cubeBytes];
                        }
                    }
                }
            }
            stopWatch.Stop();
            ts += stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            return (grads, edges);
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
                        Cube tempCube = new Cube(
                            new Point(i, j, k, slices[k, j, i], normals[k, j, i]),
                            new Point((ushort)(i + 1), j, k, slices[k, j, i + 1], normals[k, j, i + 1]),
                            new Point((ushort)(i + 1), (ushort)(j + 1), k, slices[k, j + 1, i + 1], normals[k, (j + 1), i + 1]),
                            new Point(i, (ushort)(j + 1), k, slices[k, j + 1, i], normals[k, (j + 1), i]),
                            new Point(i, j, (k + 1), slices[(k + 1), j, i], normals[(k + 1), j, i]),
                            new Point((ushort)(i + 1), j, (k + 1), slices[(k + 1), j, i + 1], normals[(k + 1), j, i + 1]),
                            new Point((ushort)(i + 1), (ushort)(j + 1), (k + 1), slices[(k + 1), j + 1, i + 1], normals[(k + 1), (j + 1), i + 1]),
                            new Point(i, (ushort)(j + 1), (k + 1), slices[(k + 1), j + 1, i], normals[(k + 1), (j + 1), i])
                            );
                        //var l = cubes[k, j, i];
                        vertice = tempCube.March(threshold, cubes[k, j, i]);

                        foreach (var vertex in vertice)
                        {
                            vertices.Add(vertex);
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
                fs.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                fs.WriteLine("vn " + vertex.normal.X + " " + vertex.normal.Y + " " + vertex.normal.Z);
                count++;

            }
        }
    }
}