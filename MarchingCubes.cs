using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarchingCubes
{
    public enum dataset
    {
        bunny,
        CThead,
        F_Ankle,
        F_Head,
        F_Hip,
        F_Knee,
        F_Pelvis,
        F_Shoulder,
        M_Head,
        M_Hip,
        M_Pelvis,
        M_Shoulder,
        MRbrain,
        ChestSmall,
        ChestCT,
        WristCT,
        MRHead3,
        MRHead5,
        MRHead20,
        MRHead30,
        MRHead40,
        SkullLrg,

    }
    class MarchingCubes
    {
        public static Context context;
        public static CudaAccelerator accelerator;
        //public static CPUAccelerator accelerator;
        public static MemoryBuffer1D<Edge, Stride1D.Dense> triTable;
        public static MemoryBuffer3D<ushort, Stride3D.DenseXY> sliced;
        public static ushort thresh = 800;
        public static int length = 512;
        public static int width = 512;
        public static ushort[,,] slices;
        public static int batchSize ;
        public static int sliceSize = 127;
        public static int OctreeSize;
        public static ushort nLayers;
        public static int nTri;
        public static string outFilename;
        public static string filePath;
        public static string repStr;
        public static DirectoryInfo d;
        public static bool isDCM;
        public static int scaling = 1;

        public static void SetValues(dataset ds, string suffix)
        {
            switch (ds)
            {
                case dataset.bunny:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\bunny\\";
                    thresh = 512;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\bunny_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = false;
                    break;
                case dataset.CThead:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\CThead\\";
                    thresh = 512;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\CThead_{suffix}.obj";
                    repStr = "CThead.";
                    isDCM = false;
                    scaling = 2;
                    break;
                case dataset.F_Ankle:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Ankle\\Ankle\\";
                    thresh = 1230;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Ankle_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    break;
                case dataset.F_Head:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Head\\Head\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Head_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    break;
                case dataset.F_Hip:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Hip\\Hip\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Hip_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    break;
                case dataset.F_Knee:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Knee\\Knee\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Knee_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true; 
                    break;
                case dataset.F_Pelvis:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Pelvis\\Pelvis\\";
                    thresh = 1220;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Pelvis_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    break;
                case dataset.F_Shoulder:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Shoulder\\Shoulder\\";
                    thresh = 1180;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Shoulder_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    break;
                case dataset.M_Head:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Head\\Head\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Head_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true;
                    break; 
                case dataset.M_Hip:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Hip\\Hip\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Hip_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true;
                    break;
                case dataset.M_Pelvis:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Pelvis\\Pelvis\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Pelvis_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true; 
                    break;
                case dataset.M_Shoulder:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Shoulder\\Shoulder\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Shoulder_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true;
                    break;
                case dataset.MRbrain:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\MRbrain\\";
                    thresh = 1538;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRbrain_{suffix}.obj";
                    repStr = "MRbrain.";
                    isDCM = false;
                    scaling = 2;
                    break;
                case dataset.ChestSmall:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Resources\\";
                    thresh = 1220;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\ChestSmall_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true; 
                    break;
                case dataset.ChestCT:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Subject (1)\\98.12.2\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\ChestCT_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
                case dataset.WristCT:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\w3568970\\batch3\\";
                    thresh = 1280;
                    length = 440;
                    width = 440;
                    outFilename = $"D:\\College Work\\MP\\WristCT_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
                case dataset.MRHead3:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 005 [MR - SAG RF FAST VOL FLIP 3]\\";
                    thresh = 10;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead3_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
                case dataset.MRHead5:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 003 [MR - SAG RF FAST VOL FLIP 5]\\";
                    thresh = 30;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead5_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
                case dataset.MRHead20:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 004 [MR - SAG RF FAST VOL FLIP 20]\\";
                    thresh = 20;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead20_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
                case dataset.MRHead30:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 002 [MR - SAG RF FAST VOL FLIP 30]\\";
                    thresh = 20;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead30_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
                case dataset.MRHead40:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 006 [MR - SAG RF FAST VOL FLIP 40]\\";
                    thresh = 40;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead40_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
                case dataset.SkullLrg:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\DICOM\\";
                    thresh = 1350;
                    length = 400;
                    width = 400;
                    outFilename = $"D:\\College Work\\MP\\SkullLrg_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    break;
            }
        }

        public static FileInfo CreateVolume(int size)
        {
            context = Context.Create(builder => builder.Default().EnableAlgorithms());
            accelerator = context.CreateCudaAccelerator(0);
            //accelerator = context.CreateCPUAccelerator(0);
            triTable = accelerator.Allocate1D<Edge>(triangleTable);
            Console.WriteLine("Threshold:" + (thresh - 1024));

            string fileName = outFilename;
            FileInfo fi = new FileInfo(fileName);

            if(slices == null)
            {
                Console.WriteLine(outFilename);
                if (isDCM)
                {
                    ReadDCM();
                }
                else
                {
                    ReadFile();
                }
                //CreateSphere(width);
            }

            OctreeSize = Math.Max(width, slices.GetLength(0));
            if (Math.Log(Math.Max(width, slices.GetLength(0)), 2) % 1 > 0)
            {
                OctreeSize = (int)Math.Pow(2, (int)Math.Log(Math.Max(width, slices.GetLength(0)), 2) + 1);
            }

            sliced = accelerator.Allocate3DDenseXY<ushort>(slices);
            return fi;
        }

        public static void ReadDCM()
        {
            ushort k = 0;
            DicomFile dicoms;
            DirectoryInfo d = new DirectoryInfo(filePath);

            FileInfo[] files = d.GetFiles("*.dcm");
            if(repStr == "vhm." || repStr == "vhf.")
            {
                files = files.OrderBy(x => int.Parse(x.Name.Replace(repStr, "").Replace(".dcm", ""))).ToArray();
            }
            var header = DicomPixelData.Create(DicomFile.Open(files.First().FullName).Dataset);
            length = header.Height;
            width = header.Width;

            slices = new ushort[Math.Min(files.Length, 512), length, width];
            //if (length == 256 && width == 256)
            //    slices = new ushort[width, width, width];

            //if (length == 512 && width == 512)
            //    slices = new ushort[width, width, width];

            foreach (var file in files)
            {
                dicoms = DicomFile.Open(file.FullName);
                CreateBmp(dicoms, k);
                k++;
                if (k > 511)
                    break;
            }

        }

        public static void ReadFile()
        {
            ushort k = 0;
            DirectoryInfo d = new DirectoryInfo(filePath);

            FileInfo[] files = d.GetFiles();
            files = files.OrderBy(x => int.Parse(x.Name.Replace(repStr, ""))).ToArray();

            slices = new ushort[files.Length, length, width];
            //if (length == 256 && width == 256)
            //    slices = new ushort[width, width, width];

            //if (length == 512 && width == 512)
            //    slices = new ushort[width, width, width];

            foreach (var file in files)
            {
                Decode(file.FullName, k);
                k++;
                if (k > 1023)
                    break;
            }

        }


        public static void CreateSphere(int size)
        {
            length = size;
            width = size;

            double factor = Math.Sqrt(size / 2 * (size / 2) * 5);
            ushort[,,] sphere = new ushort[size / 2, size, size];
            for (int k = 0; k < size / 2; k++)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        sphere[k, j, i] = (ushort)Math.Round(Math.Sqrt((i - size / 2) * (i - size / 2) + (j - size / 2) * (j - size / 2) + (k - size / 2) * (k - size / 2)) * (2000 / factor));
                        //double h = (k + i) * 60;
                        //bool p = h > (double)threshold;
                        //sphere[k, j, i] = (ushort)(p ? threshold - 50 : threshold + 50);
                        //sphere[k, j, i] = (ushort)(((j - size / 2) + (i - size / 2) + (k - size / 2)) * 20);
                    }
                }
            }
            slices = sphere;
        }
        public static ushort[,,] CreateCayley(int size)
        {
            double factor = Math.Sqrt(size / 2 * (size / 2) * 5);
            ushort[,,] slice = new ushort[size, size, size];
            for (int k = 0; k < size; k++)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        float x = (float)i / size;
                        float y = (float)j / size;
                        float z = (float)k / size;
                        slice[k, j, i] = (ushort)(16 * x * y * z + 4 * (x + y + z) - 14);
                    }
                }
            }
            return slice;
        }
        public static uint getShuffleXYZ(Index3D index)
        {
            uint X, Y, Z, key = 0;
            X = (uint)index.Z;
            Y = (uint)index.Y;
            Z = (uint)index.X;
            for (int i = 0; i < 10; i++)
            {
                key += (((Z & (1 << i)) > 0) ? (uint)XMath.Pow(2, i * 3) : 0);
                key += (((Y & (1 << i)) > 0) ? (uint)XMath.Pow(2, i * 3 + 1) : 0);
                key += (((X & (1 << i)) > 0) ? (uint)XMath.Pow(2, i * 3 + 2) : 0);
            }
            return key;
        }

        public static Index3D getFromShuffleXYZ(uint xyz, int n)
        {
            uint X = 0, Y = 0, Z = 0;
            for (int i = 0; i < n; i++)
            {
                xyz >>= 3;
                X += (1 & xyz) << i;
                Y += (1 & xyz >> 1) << i;
                Z += (1 & xyz >> 2) << i;
            }
            return new Index3D((int)X, (int)Y, (int)Z);
        }


        public static Index3D getFromShuffleXYZ2(uint xyz, int n)
        {
            uint X = 0, Y = 0, Z = 0;
            for (int i = 0; i < n; i++)
            {
                X += (1 & xyz) << i;
                Y += (1 & xyz >> 1) << i;
                Z += (1 & xyz >> 2) << i;
                xyz >>= 3;
            }
            return new Index3D((int)X, (int)Y, (int)Z);
        }

        public static int get1D(Index3D index3D, Index3D size)
        {
            return index3D.Z * size.Y * size.X + index3D.Y * size.X + index3D.X;
        }

        public static Index3D get3D(Index1D index1D, Index3D size)
        {
            int index = index1D.X;
            int z = index1D / (size.Y * size.X);
            index -= z * size.Y * size.X;
            int y = index / size.X;
            int x = index % size.X;
            return new Index3D(x, y, z);
        }

        public static void CreateBmp(DicomFile dicom, int k)
        {
            var decomp = dicom.Clone();

            var header = DicomPixelData.Create(decomp.Dataset);
            header.PhotometricInterpretation = PhotometricInterpretation.Monochrome2;
            var h = header.GetFrame(0);

            GrayscalePixelDataU16 pixelData = new GrayscalePixelDataU16(header.Width, header.Width, header.BitDepth, h);

            ushort[,] pixArray = new ushort[pixelData.Width, pixelData.Width];

            List<byte> color = new List<byte>();
            //var pixelData = PixelDataFactory.Create(dicom.PixelData, 0); // returns IPixelData type


            if (pixelData is GrayscalePixelDataU16)
            {
                for (int i = 0; i < pixelData.Width; i++)
                {
                    for (int j = 0; j < pixelData.Height; j++)
                    {
                        int index = j * header.Width + i;
                        slices[k, j, i] = (ushort)pixelData.Data[index];
                    }
                }
            }
        }

        public static void DecodeTIFF(string path, int k)
        {
            MagickImage image = new MagickImage(path);
            var pixelValues = image.GetPixels().GetValues();

            if (true)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        int index = j * image.Width + i;
                        slices[k, j, i] = (ushort)pixelValues[index];
                    }
                }
            }
        }

        public static void Decode(string path, int k)
        {
            FileStream stream = File.OpenRead(path);
            byte[] byteTemp = new byte[stream.Length];
            ushort[] pixels = new ushort[stream.Length / 2];
            stream.Read(byteTemp, 0, (int)stream.Length);
            var reverse = byteTemp.Reverse().ToArray();

            Buffer.BlockCopy(reverse, 0, pixels, 0, (int)stream.Length);
            stream.Close();
            stream.Dispose();

            if (true)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int index = j * width + i;
                        slices[k, j, i] = (ushort)pixels[index];
                        if (slices[k, j, i] >= 4096)
                            slices[k, j, i] = 0;
                    }
                }
            }
        }

        public static Edge[] triangleTable =    
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
