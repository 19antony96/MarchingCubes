using System;
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
    class NaiveCPU : MarchingCubes
    {
        public static byte[,,] cubeBytes;

        public static TimeSpan ts = new TimeSpan();
        public static List<Point> vertices = new List<Point>();
        public static TimeSpan FirstPassTime;
        public static TimeSpan ExtractionTime;
        public static TimeSpan TotalTime;
        public static byte[,,] cubes;
        public static int count = 0;
        public static int maxX = 0, maxY = 0, maxZ = 0;
        public static int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;

        public NaiveCPU(int size)
        {
            vertices = new List<Point>();
            maxX = 0;
            maxY = 0;
            maxZ = 0;
            minX = int.MaxValue;
            minY = int.MaxValue;
            minZ = int.MaxValue;
            Console.WriteLine("CPU");
            ushort i = 0;
            FileInfo fi = CreateVolume(size);

            cubes = MarchingCubesCPU();
            using (StreamWriter fs = fi.CreateText())
            {
                MarchCPU(fs);

                int f = 0;
                for (f = 1; f < count - 1; f += 3)
                {
                    //fs.WriteLine("f " + f + " " + (f + 1) + " " + (f + 2));
                }
                Console.WriteLine(count);
            }
        }

        private static byte[,,] MarchingCubesCPU()
        {
            cubeBytes = new byte[(slices.GetLength(0) - 1), (length - 1), (width - 1)];
            byte cubeByte;

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

            int cub = 0;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (k = 0; k < slices.GetLength(0); k++)
            {
                for (i = 0; i < slices.GetLength(2); i++)
                {
                    for (j = 0; j < slices.GetLength(1); j++)
                    {
                        if (k != slices.GetLength(0) - 1 && j != slices.GetLength(1) - 1 && i != slices.GetLength(2) - 1)
                        {
                            cubeByte = 0;
                            cubeByte += (slices[k, j, i] < thresh) ? (byte)0x01 : (byte)0;
                            cubeByte += (slices[k, j, i + 1] < thresh) ? (byte)0x02 : (byte)0;
                            cubeByte += (slices[k, j + 1, i + 1] < thresh) ? (byte)0x04 : (byte)0;
                            cubeByte += (slices[k, j + 1, i] < thresh) ? (byte)0x08 : (byte)0;
                            cubeByte += (slices[k + 1, j, i] < thresh) ? (byte)0x10 : (byte)0;
                            cubeByte += (slices[k + 1, j, i + 1] < thresh) ? (byte)0x20 : (byte)0;
                            cubeByte += (slices[k + 1, j + 1, i + 1] < thresh) ? (byte)0x40 : (byte)0;
                            cubeByte += (slices[k + 1, j + 1, i] < thresh) ? (byte)0x80 : (byte)0;

                            cubeBytes[k, j, i] = cubeByte;
                            if (cubeByte > 0 && cubeByte < 255)
                                cub++;
                        }
                    }
                }
            }
            stopWatch.Stop();
            ts += stopWatch.Elapsed;
            FirstPassTime = ts;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime " + elapsedTime);
            Console.WriteLine("Cubes: " + cub);
            return cubeBytes;
        }

        public static void MarchCPU(StreamWriter fs)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int i, j, k;
            Point[] vertice;
            for (k = 0; k < slices.GetLength(0) - 1; k++)
            {
                for (i = 0; i < slices.GetLength(2) - 1; i++)
                {
                    for (j = 0; j < slices.GetLength(1) - 1; j++)
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

                            vertice = tempCube.March(thresh, triangleTable[cubes[k, j, i]]);

                            foreach (var vertex in vertice)
                            {
                                vertices.Add(vertex);
                            }
                        }
                    }
                }
            }
            ts = stopWatch.Elapsed;

            ExtractionTime = ts;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.TotalMilliseconds * 10000);
            Console.WriteLine("RunTime " + elapsedTime);
            foreach (var vertex in vertices)
            {
                //    fs.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                //    fs.WriteLine("vn " + -vertex.normal.X + " " + -vertex.normal.Y + " " + -vertex.normal.Z);

                maxX = (int)Math.Max(maxX, vertex.X);
                maxY = (int)Math.Max(maxY, vertex.Y);
                maxZ = (int)Math.Max(maxZ, vertex.Z);
                minX = (int)Math.Min(minX, vertex.X);
                minY = (int)Math.Min(minY, vertex.Y);
                minZ = (int)Math.Min(minZ, vertex.Z);
                
                count++;

            }

        }

        public static implicit operator NaiveCPU(OctreeWPrior v)
        {
            throw new NotImplementedException();
        }
    }
}