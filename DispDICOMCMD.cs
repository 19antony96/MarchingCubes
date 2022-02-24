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

namespace DispDICOMCMD
{
    class DispDICOMCMD
    {
        public static readonly short threshold = 1350;
        public static readonly int length = 440;
        public static readonly int width = 440;
        public static List<short[,]> slices = new List<short[,]>();

        public static int xCount = 0, yCount = 0, zCount = 0, lCount = 0;

        public DispDICOMCMD()
        {
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    var image = new DicomImage(openFileDialog1.FileName);
            //    var bmpsrc = CreateBmp(image);

            //    //var p = image.PixelData;
            //    //var bmpsrc = image.RenderImage().As<Bitmap>();
            //    //List<float> color = new List<float>();
            //    //var pixeldatatest = image.PixelData; // returns DicomPixelData type
            //    //var pixelData = PixelDataFactory.Create(image.PixelData, 0); // returns IPixelData type
            //    //if (pixelData is GrayscalePixelDataU16)
            //    //{
            //    //    for (int i = 0; i < pixelData.Width; i++)
            //    //    {
            //    //        for (int j = 0; j < pixelData.Height; j++)
            //    //        {
            //    //            color.Add(Convert.ToSingle(pixelData.GetPixel(i, j)));
            //    //        }
            //    //    }
            //    //}
            //    //var b = color.Select(x => x).Distinct().OrderBy(x => x).ToList();
            //    //List<float> colors = new List<float>();
            //    //for (int y = 0; y < bitmap.Height; y++)
            //    //{
            //    //    for (int x = 0; x < bitmap.Width; x++)
            //    //    {
            //    //        colors.Add(bitmap.GetPixel(x, y).GetBrightness());
            //    //        // Get the brightness of this pixel
            //    //    }
            //    //}
            //    //var t = colors.Select(x => x).Distinct().OrderBy(x => x).ToList();



            //    pictureBox1.Image = bmpsrc;
            //}

            DicomFile dicoms;
            //DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\Subject (1)\\98.12.2\\");
            DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\w3568970\\batch3\\");
            //DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\DICOM\\DICOM\\ST000000\\SE000001\\");
            //DirectoryInfo d = new DirectoryInfo("C:\\Users\\antonyDev\\Downloads\\Resources\\");

            //DicomFile p = DicomFile.Open("C:\\Users\\antonyDev\\Downloads\\w3568970\\batch3\\view0296.dcm");
            //DicomPixelData pixelData = DicomPixelData.Create(p.Dataset);

            List<Point> normals = new List<Point>();

            FileInfo[] files = d.GetFiles("*.dcm");


            //List<short[,]> slices = new List<short[,]>();
            List<Point> vertices = new List<Point>();
            Cube[,] cubes;

            int[,] edges;
            int count = 0;

            var sphere = CreateSphere();

            string fileName = @"C:\\Users\\antonyDev\\Desktop\\normal1.obj";
            FileInfo fi = new FileInfo(fileName);
            using (StreamWriter fs = fi.CreateText())
            {
                short i, j, k = 0;
                foreach (var file in files)
                    //foreach (var file in sphere)
                {
                    dicoms = DicomFile.Open(file.FullName);
                    slices.Add(CreateBmp(dicoms));
                    //slices.Add(file);
                    if (slices.Count > 3)
                    {
                        //var temp = MarchingCubes(slices[0], slices[1], k);
                        //cubes = temp.cubeConfigs;
                        //edges = temp.edges;
                        //short[,] slice1 = slices[0];
                        //short[,] slice2 = slices[1];
                        //for (i = 0; i < cubes.GetLength(0) - 1; i++)
                        //{
                        //    for (j = 0; j < cubes.GetLength(1) - 1; j++)
                        //    {
                        //        foreach (var vertex in getEdges(cubes[j, i], edges[j, i], slice1, slice2, i, j, k))
                        //        {
                        //            if (vertex.X > 512 || vertex.Y > 512)
                        //                ;
                        //            count++;
                        //            fs.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                        //        }
                        //    }
                        //}

                        var temp = MarchingCubes(slices[1], slices[2], k);
                        cubes = temp.cubeConfigs;
                        edges = temp.edges;
                        for (i = 0; i < cubes.GetLength(0) - 1; i++)
                        {
                            for (j = 0; j < cubes.GetLength(1) - 1; j++)
                            {
                                foreach (var vertex in cubes[j, i].March(threshold))
                                {
                                    vertices.Add(vertex);
                                    fs.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                                    //fs.WriteLine("vt " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                                    Point normal = Normal(slices[0], slices[1], slices[2], slices[3], vertex, k);
                                    //fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
                                    Point n = new Point(vertex.normal.X*2 + normal.X, vertex.normal.Y*2 + normal.Y, vertex.normal.Z*2 + normal.Z, 0);
                                    normals.Add(normal);
                                    count++;
                                }
                            }
                        }
                        k++;
                        //if (k == 16)
                        //    break;
                        //Console.WriteLine(k);
                    }
                    if (slices.Count > 4)
                    {
                        slices.RemoveAt(0);
                    }
                }

                //vertices = new List<Point>() { vertices[0], vertices[1], vertices[2] };
                //normals = new List<Point>() { normals[0], normals[1], normals[2] };
                //count = 3;

                //foreach (Point vertex in vertices)
                //{
                //    fs.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                //}

                var second = vertices.Distinct(new PointComparer());

                Normal norm = new Normal(0, 0, 0);
                //foreach (Point point in second)
                //{
                //    List<Point> vert = vertices.Where(x => x == point).ToList();
                //    norm.X = 0;
                //    norm.Y = 0;
                //    norm.Z = 0;
                //    foreach (Point normal in vert)
                //    {
                //        norm.X += normal.normal.X / vert.Count;
                //        norm.Y += normal.normal.Y / vert.Count;
                //        norm.Z += normal.normal.Z / vert.Count;
                //    }
                //    if (norm.X == 0)
                //        ;
                //    normals.Add(norm);
                //}

                //Normal norm = new Normal(0, 0, 0);
                //var v2 = vertices.ToArray();
                //foreach (Point vertex in v2)
                ////foreach (Point vertex in second)
                //{
                //    norm.X = 0;
                //    norm.Y = 0;
                //    norm.Z = 0;
                //    Console.WriteLine(vertex.Z);
                //    //fs.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                //    vertices.Where(x => x == vertex).ToList().ForEach(x =>
                //    {
                //        norm.X += x.normal.X;
                //        norm.Y += x.normal.Y;
                //        norm.Z += x.normal.Z;
                //    });
                //    vertices.Where(x => x == vertex).ToList().ForEach(x =>
                //    {
                //        x.normal = new Normal(norm.X * (1 / Math.Sqrt(Math.Pow(norm.X, 2) + Math.Pow(norm.Y, 2) + Math.Pow(norm.Z, 2))), norm.Y * (1 / Math.Sqrt(Math.Pow(norm.X, 2) + Math.Pow(norm.Y, 2) + Math.Pow(norm.Z, 2))), norm.Z * (1 / Math.Sqrt(Math.Pow(norm.X, 2) + Math.Pow(norm.Y, 2) + Math.Pow(norm.Z, 2))));
                //    });
                //    //norm.X = 0;
                //    //norm.Y = 0;
                //    //norm.Z = 0;
                //    //foreach(Point normal in vert)
                //    //{
                //    //    norm.X += normal.normal.X / vert.Count;
                //    //    norm.Y += normal.normal.Y / vert.Count;
                //    //    norm.Z += normal.normal.Z / vert.Count;
                //    //}
                //    //if (norm.X == 0)
                //    //    ;
                //    //normals.Add(norm);
                //    //fs.WriteLine("vn " + norm.X + " " + norm.Y + " " + norm.Z);
                //}

                foreach (Point normal in normals)
                {
                    fs.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
                }

                int f = 0;
                for (f = 1; f < count - 1; f += 3)
                {
                    fs.WriteLine("f " + f + " " + (f + 1) + " " + (f + 2));
                    //fs.WriteLine("f " + f + "//" + f + " " + (f + 1) + "//" + (f + 1) + " " + (f + 2) + "//" + (f + 2));
                }
                Console.WriteLine(xCount + yCount + zCount + lCount);
            }

        }

        //private static void getDistinct(byte[,] array)
        //{
        //	List<byte> dist = new List<byte>();
        //	for (int i = 0; i < array.GetLength(0) - 1; i++)
        //	{
        //		for (int j = 0; j < array.GetLength(1) - 1; j++)
        //		{
        //			dist.Add(array[j, i]);

        //		}
        //	}
        //	List<string> total = new List<string>();
        //	List<byte> configs = dist.Select(x => x).Distinct().OrderBy(x => x).ToList();
        //	foreach (byte config in configs)
        //	{
        //		total.Add(dist.Where(x => x == config).Count().ToString());
        //	}
        //	ListToExcel(total);
        //}

        public List<short[,]> CreateSphere()
        {
            List<short[,]> volume = new List<short[,]>();
            for(int k = 0; k < 128; k++)
            {
                short[,] slice = new short[128, 128];
                for (int i = 0; i < 128; i++)
                {
                    for (int j = 0; j < 128; j++)
                    {
                        slice[j, i] = (short)(Math.Sqrt((i - 64) * (i - 64) + (j - 64) * (j - 64) + (k - 64) * (k - 64)) * 20);
                        //bool p = h > (double)threshold;
                        //slice[j, i] = (short)(p ? threshold - 50 : threshold +50);
                        //slice[j, i] = (short)(((j - 64) + (i - 64) + (k - 64)) * 20);
                    }
                }
                volume.Add(slice);
            }
            return volume;
        }

        private static Point Normal(short[,] slice1, short[,] slice2, short[,] slice3, short[,] slice4, Point point, short index)
        {
            Point vertex = point;
            double factor, interpolant;
            vertex.Z -= (index - 1);
            //Point v1x1,v1x2,v1y1,v1y2,v1z1,v1z2, v2x1, v2x2, v2y1, v2y2, v2z1, v2z2;
            Point voxel1 = vertex, voxel2;
            Point normal1 = new Point(0,0,0,0), normal2;
            if (vertex.X % 1 != 0)
            {
                xCount++;
                voxel1 = new Point((int)vertex.X, vertex.Y, vertex.Z, slices[(int)vertex.Z][(int)vertex.Y, (int)vertex.X]);
                voxel2 = new Point((int)vertex.X + 1, vertex.Y, vertex.Z, slices[(int)vertex.Z][(int)vertex.Y, (int)vertex.X + 1]);
                interpolant = vertex.X - (int)vertex.X;
            }
            else if (vertex.Y % 1 != 0)
            {
                yCount++;
                voxel1 = new Point(vertex.X, (int)vertex.Y, vertex.Z, slices[(int)vertex.Z][(int)vertex.Y, (int)vertex.X]);
                voxel2 = new Point(vertex.X, (int)vertex.Y + 1, vertex.Z, slices[(int)vertex.Z][(int)vertex.Y + 1, (int)vertex.X]);
                interpolant = vertex.Y - (int)vertex.Y;
            }
            else if (vertex.Z % 1 != 0)
            {
                zCount++;
                voxel1 = new Point(vertex.X, vertex.Y, (int)vertex.Z, slices[(int)vertex.Z][(int)vertex.Y, (int)vertex.X]);
                voxel2 = new Point(vertex.X, vertex.Y, (int)vertex.Z + 1, slices[(int)vertex.Z + 1][(int)vertex.Y, (int)vertex.X]);
                interpolant = vertex.Z - (int)vertex.Z;
            }
            else
            {
                lCount++;
                if (voxel1.X == 0)
                    normal1.X = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X])/2;
                else if (voxel1.X == length - 1)
                    normal1.X = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X - 1])/2;
                else
                    normal1.X = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X - 1]);

                if (voxel1.Y == 0)
                    normal1.Y = (slices[(int)voxel1.Z][(int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X])/2;
                else if (voxel1.Y == length - 1)
                    normal1.Y = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y - 1, (int)voxel1.X])/2;
                else
                    normal1.Y = (slices[(int)voxel1.Z][(int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y - 1, (int)voxel1.X]);

                if (voxel1.Z == 0)
                    normal1.Z = (slices[(int)voxel1.Z + 1][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X])/2;
                else if (voxel1.Z == length - 1)
                    normal1.Z = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1][(int)voxel1.Y, (int)voxel1.X])/2;
                else
                    normal1.Z = (slices[(int)voxel1.Z + 1][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1][(int)voxel1.Y, (int)voxel1.X]);
                factor = 1 / Math.Sqrt(Math.Pow(normal1.X, 2) + Math.Pow(normal1.Y, 2) + Math.Pow(normal1.Z, 2));
                return normal1 * factor;
            }
            normal1 = new Point(0, 0, 0, voxel1.value);
            normal2 = new Point(0, 0, 0, voxel2.value);

            if (voxel1.X == 0)
                normal1.X = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X])/2;
            else if (voxel1.X == length - 1)
                normal1.X = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X - 1])/2;
            else
                normal1.X = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X + 1] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X - 1]);

            if (voxel1.Y == 0)
                normal1.Y = (slices[(int)voxel1.Z][(int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X])/2;
            else if (voxel1.Y == length - 1)
                normal1.Y = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y - 1, (int)voxel1.X])/2;
            else
                normal1.Y = (slices[(int)voxel1.Z][(int)voxel1.Y + 1, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y - 1, (int)voxel1.X]);

            if (voxel1.Z == 0)
                normal1.Z = (slices[(int)voxel1.Z + 1][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z][(int)voxel1.Y, (int)voxel1.X])/2;
            else if (voxel1.Z == length - 1)
                normal1.Z = (slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1][(int)voxel1.Y, (int)voxel1.X])/2;
            else
                normal1.Z = (slices[(int)voxel1.Z + 1][(int)voxel1.Y, (int)voxel1.X] - slices[(int)vertex.Z - 1][(int)voxel1.Y, (int)voxel1.X]);

            if (voxel2.X == 0)
                normal2.X = (slices[(int)voxel2.Z][(int)voxel2.Y, (int)voxel2.X + 1] - slices[(int)vertex.Z][(int)voxel2.Y, (int)voxel2.X])/2;
            else if (voxel2.X == length - 1)
                normal2.X = (slices[(int)voxel2.Z][(int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z][(int)voxel2.Y, (int)voxel2.X - 1])/2;
            else
                normal2.X = (slices[(int)voxel2.Z][(int)voxel2.Y, (int)voxel2.X + 1] - slices[(int)vertex.Z][(int)voxel2.Y, (int)voxel2.X - 1]);

            if (voxel2.Y == 0)
                normal2.Y = (slices[(int)voxel2.Z][(int)voxel2.Y + 1, (int)voxel2.X] - slices[(int)vertex.Z][(int)voxel2.Y, (int)voxel2.X])/2;
            else if (voxel2.Y == length - 1)
                normal2.Y = (slices[(int)voxel2.Z][(int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z][(int)voxel2.Y - 1, (int)voxel2.X])/2;
            else
                normal2.Y = (slices[(int)voxel2.Z][(int)voxel2.Y + 1, (int)voxel2.X] - slices[(int)vertex.Z][(int)voxel2.Y - 1, (int)voxel2.X]);

            if (voxel2.Z == 0)
                normal2.Z = (slices[(int)voxel2.Z + 1][(int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z][(int)voxel2.Y, (int)voxel2.X])/2;
            else if (voxel2.Z == length - 1)
                normal2.Z = (slices[(int)voxel2.Z][(int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z - 1][(int)voxel2.Y, (int)voxel2.X])/2;
            else
                normal2.Z = (slices[(int)voxel2.Z + 1][(int)voxel2.Y, (int)voxel2.X] - slices[(int)vertex.Z - 1][(int)voxel2.Y, (int)voxel2.X]);
            var t = slices[(int)voxel2.Z + 1][(int)voxel2.Y, (int)voxel2.X];
            var l = slices[(int)vertex.Z - 1][(int)voxel2.Y, (int)voxel2.X];
            //v1x1 = new Point(voxel1.X -1, voxel1.Y, voxel1.Z, slices[(int)voxel1.Z][(int)voxel1.Y, (int)voxel1.X - 1]);

            Point p = Interpolate(normal2, normal1, interpolant);
            if (double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsNaN(p.Z))
                ;
            var j = (vertex - new Point(64,64,64,0)) * (1 / Math.Sqrt(Math.Pow(vertex.X - 64, 2) + Math.Pow(vertex.Y - 64, 2) + Math.Pow(vertex.Z - 64, 2)));
            factor = 1/Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2));
            var m = p * factor;
            return p*factor;
        }

        public static Point Interpolate(Point pt2, Point pt1, double interpolant)
        {
            double x = (pt1.X - pt2.X) * interpolant + pt2.X;
            double y = (pt1.Y - pt2.Y) * interpolant + pt2.Y;
            double z = (pt1.Z - pt2.Z) * interpolant + pt2.Z;

            return new Point(x, y, z, 0);
        }

        private static short[,] CreateBmp(DicomFile dicom)
        {
            var decomp = dicom.Clone();

            var header = DicomPixelData.Create(decomp.Dataset);
            var h = header.GetFrame(0);

            GrayscalePixelDataU16 pixelData = new GrayscalePixelDataU16(header.Width, header.Width, header.BitDepth, h);

            short[,] pixArray = new short[pixelData.Width, pixelData.Width];

            List<byte> color = new List<byte>();
            //var pixelData = PixelDataFactory.Create(dicom.PixelData, 0); // returns IPixelData type


            if (pixelData is GrayscalePixelDataU16)
            {

                Context context = Context.CreateDefault();
                //Accelerator accelerator;
                Accelerator accelerator = context.CreateCPUAccelerator(0);
                var loadedKernel = accelerator.LoadAutoGroupedStreamKernel(
                (Index2D i, ArrayView<ushort> data, ArrayView2D<short, Stride2D.DenseX> output, int w) =>
                {
                    output[i] = (short)data[i.X * w + i.Y];
                });

                var tempOut = accelerator.Allocate2DDenseX<short>(new LongIndex2D(pixelData.Width, pixelData.Height));
                var tempView = accelerator.Allocate1D(pixelData.Data);
                loadedKernel(new Index2D(pixelData.Width, pixelData.Height), tempView.View, tempOut, width);

                //for (int i = 0; i < pixelData.Width; i++)
                //{
                //    for (int j = 0; j < pixelData.Height; j++)
                //    {
                //        int index = j * header.Width + i;
                //        pixArray[j, i] = (short)pixelData.Data[index];
                //        //if (pixelData.GetPixel(j, i) > threshold) color.Add(255);
                //        //else color.Add(0);
                //        //color.Add(color.Last());
                //        //color.Add(color.Last());
                //    }
                //}
                
                accelerator.DefaultStream.Synchronize();
                var l = tempOut.GetAsArray2D();

                accelerator.Dispose();
                context.Dispose();
                pixArray = l;
            }
            return pixArray;
        }

        private static Bitmap array2Bitmap(short[,] pixArray)
        {

            int res = 96;
            int stride = 3 * pixArray.GetLength(1);
            List<byte> color = new List<byte>();
            //var t =color.Max(); 
            int length = color.Count();

            for (int i = 0; i < pixArray.GetLength(0); i++)
            {
                for (int j = 0; j < pixArray.GetLength(1); j++)
                {
                    if (pixArray[j, i] > threshold) color.Add(255);
                    else color.Add(0);
                    color.Add(color.Last());
                    color.Add(color.Last());
                }
            }

            byte[] array = color.ToArray();
            byte[] bytes = array;
            //Buffer.BlockCopy(array, 0, bytes, 0, array.Length * 2);
            // Initialize unmanaged memory to hold the array.
            int size = System.Runtime.InteropServices.Marshal.SizeOf(array[0]) * array.Length;
            IntPtr pnt = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            //Copy the pixel data into the bitmap structure

            Bitmap source = new Bitmap(pixArray.GetLength(1),
                                        pixArray.GetLength(1),
                                        stride,
                                        PixelFormat.Format24bppRgb,
                                        pnt);




            var wb = new WriteableBitmap(source.Width, source.Height, res, res, System.Windows.Media.PixelFormats.Rgb24, null);
            wb.WritePixels(new Int32Rect(0, 0, source.Width, source.Height), array, source.Height * 3, 0);

            //byte[] outbytes = Convert16BitGrayScaleToRgb48(bytes, source.Width, source.Height);
            Rectangle dimension = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData picData = source.LockBits(dimension, ImageLockMode.ReadWrite, source.PixelFormat);
            //IntPtr pixelStartAddress = picData.Scan0;

            //IntPtr pnt = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            //Copy the pixel data into the bitmap structure
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, pnt, bytes.Length);
            source.UnlockBits(picData);
            return source;
        }

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
        private static (Cube[,] cubeConfigs, int[,] edges) MarchingCubes(short[,] slice1, short[,] slice2, short index)
        {
            Cube[,] cubeBytes = new Cube[length, width];
            //byte[,] configBytes = new byte[511, 511];
            int[,] edges = new int[length, width];

            //bit order 
            // i,j,k 
            // i+1,j,k
            // i+1,j+1,k
            // i,j+1,k
            // i,j,k+1
            // i+1,j,k+1
            // i+1,j+1,k+1
            // i,j+1,k+1


            for (short i = 0; i < slice1.GetLength(1) - 1; i++)
            {
                for (short j = 0; j < slice1.GetLength(1) - 1; j++)
                {

                    cubeBytes[j, i] = new Cube(
                        new Point(i, j, index, slice1[j, i], threshold),
                        new Point((short)(i + 1), j, index, slice1[j, i + 1], threshold),
                        new Point((short)(i + 1), (short)(j + 1), index, slice1[j + 1, i + 1], threshold),
                        new Point(i, (short)(j + 1), index, slice1[j + 1, i], threshold),
                        new Point(i, j, (short)(index + 1), slice2[j, i], threshold),
                        new Point((short)(i + 1), j, (short)(index + 1), slice2[j, i + 1], threshold),
                        new Point((short)(i + 1), (short)(j + 1), (short)(index + 1), slice2[j + 1, i + 1], threshold),
                        new Point(i, (short)(j + 1), (short)(index + 1), slice2[j + 1, i], threshold)
                        );
                    cubeBytes[j, i].Config(threshold);
                    //configBytes[j, i] = regularCellClass[cubeBytes[j, i]];
                    edges[j, i] = edgeTable[cubeBytes[j, i].getConfig()];
                }
            }
            return (cubeBytes, edges);
        }

        public static List<Point3D> getEdges(byte cube, int edges, int[,] slice1, int[,] slice2, int i, int j, int k)
        {
            //var l = edgeTable[cube];
            //var t = GetRow(triangleTable, cube);
            //int v1 = slice1[j, i];
            //int v2 = slice1[j, i + 1];
            //int v3 = slice1[j + 1, i +1];
            //int v4 = slice1[j + 1, i];
            //int v5 = slice2[j, i];
            //int v6 = slice2[j, i + 1];
            //int v7 = slice2[j + 1, i + 1];
            //int v8 = slice2[j + 1, i];

            List<Point3D> points = new List<Point3D>();
            int[] ed = GetRow(triangleTable, cube).Where(x => x >= 0).ToArray();
            if (ed.Length > 0)
            {

            }
            foreach (var edge in ed)
            {
                switch (edge)
                {
                    case (int)edgeMask.e1:
                        points.Add(new Point3D((float)(i + Interpolate(slice1[j, i], slice1[j, i + 1], i, j, k, slice1, slice2)), j, k));
                        //points.Add(new Point3D(i + 0.5f, j, k));
                        break;
                    case (int)edgeMask.e2:
                        points.Add(new Point3D(i + 1, (float)(j + Interpolate(slice1[j, i + 1], slice1[j + 1, i + 1], i, j, k, slice1, slice2)), k));
                        //points.Add(new Point3D(i + 1, j + 0.5f, k));
                        break;
                    case (int)edgeMask.e3:
                        points.Add(new Point3D((float)(i + Interpolate(slice1[j + 1, i], slice1[j + 1, i + 1], i, j, k, slice1, slice2)), j + 1, k));
                        //points.Add(new Point3D(i + 0.5f, j + 1, k));
                        break;
                    case (int)edgeMask.e4:
                        points.Add(new Point3D(i, (float)(j + Interpolate(slice1[j, i], slice1[j + 1, i], i, j, k, slice1, slice2)), k));
                        //points.Add(new Point3D(i, j + 0.5f, k));
                        break;
                    case (int)edgeMask.e5:
                        points.Add(new Point3D((float)(i + Interpolate(slice2[j, i], slice2[j, i + 1], i, j, k, slice1, slice2)), j, k + 1));
                        //points.Add(new Point3D(i + 0.5f, j, k + 1));
                        break;
                    case (int)edgeMask.e6:
                        points.Add(new Point3D(i + 1, (float)(j + Interpolate(slice2[j, i + 1], slice2[j + 1, i + 1], i, j, k, slice1, slice2)), k + 1));
                        //points.Add(new Point3D(i + 1, j + 0.5f, k + 1));
                        break;
                    case (int)edgeMask.e7:
                        points.Add(new Point3D((float)(i + Interpolate(slice2[j + 1, i], slice2[j + 1, i + 1], i, j, k, slice1, slice2)), j + 1, k + 1));
                        //points.Add(new Point3D(i + 0.5f, j + 1, k + 1));
                        break;
                    case (int)edgeMask.e8:
                        points.Add(new Point3D(i, (float)(j + Interpolate(slice2[j, i], slice2[j + 1, i], i, j, k, slice1, slice2)), k + 1));
                        //points.Add(new Point3D(i, j + 0.5f, k + 1));
                        break;
                    case (int)edgeMask.e9:
                        points.Add(new Point3D(i, j, (float)(k + Interpolate(slice1[j, i], slice2[j, i], i, j, k, slice1, slice2))));
                        //points.Add(new Point3D(i, j, k+0.5f));
                        break;
                    case (int)edgeMask.e10:
                        points.Add(new Point3D(i + 1, j, (float)(k + Interpolate(slice1[j, i + 1], slice2[j, i + 1], i, j, k, slice1, slice2))));
                        //points.Add(new Point3D(i + 1, j, k+0.5f));
                        break;
                    case (int)edgeMask.e11:
                        points.Add(new Point3D(i + 1, j + 1, (float)(k + Interpolate(slice1[j + 1, i + 1], slice2[j + 1, i + 1], i, j, k, slice1, slice2))));
                        //points.Add(new Point3D(i + 1, j + 1, k+0.5f));
                        break;
                    case (int)edgeMask.e12:
                        points.Add(new Point3D(i, j + 1, (float)(k + Interpolate(slice1[j + 1, i], slice2[j + 1, i], i, j, k, slice1, slice2))));
                        //points.Add(new Point3D(i, j + 1, k+0.5f));
                        break;
                }
            }
            return points;
        }


        public static double Interpolate(int top, int bottom, int i, int j, int k, int[,] slice1, int[,] slice2)
        {
            float epsilon = 50;
            double p1 = Math.Max(top, bottom);
            double p2 = Math.Min(top, bottom);
            if (p2 == 0)
                p2 = 4 * epsilon;

            double x = ((double)threshold - p2) / (double)(p1 - p2);
            //if (bottom > top)
            //    x = 1 - x;
            if (x < 0 || x > 1)
                ;
            return x;
            //return 0.5;
        }

        //public static int[] GetColumn(int[,] matrix, int columnNumber)
        //{
        //	return Enumerable.Range(0, matrix.GetLength(0))
        //			.Select(x => matrix[x, columnNumber])
        //			.ToArray();
        //}

        public static int[] GetRow(int[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        //public static void ListToExcel(List<string> list)
        //{

        //	using (StreamWriter sw = File.CreateText("list.csv"))
        //	{
        //		for (int i = 0; i < list.Count; i++)
        //		{
        //			sw.WriteLine(list[i]);
        //		}
        //	}
        //}

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
