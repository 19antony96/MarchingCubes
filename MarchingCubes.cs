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
    public enum Format
    {
        dat,
        tiff,
        dcm,
        raw,
        file
    }

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

        PlaneScout,
        WATERIDEAL_T2FSEASSET,
        bval100260048008T22nexeDWI,
        multiPhase,
        FATIDEAL_T2FSEASSET,
        InPhaseIDEAL_T2FSEASSET,
        uni_lateral_cropped_SER_111387,
        uni_lateral_cropped_PE2_55883,
        uni_lateral_cropped_PE6_69418,
        uni_lateral_cropped_original_DCE_21414,

        STIR_ASSET,
        ACRIN_6698_Ax_DWI,
        Ax_VIBRANT_MPh,
        Ph1Ax_VIBRANT_MPh,
        Ph2Ax_VIBRANT_MPh,
        Ph3Ax_VIBRANT_MPh,
        Ph4Ax_VIBRANT_MPh,
        Ph5Ax_VIBRANT_MPh,
        Ph6Ax_VIBRANT_MPh,
        uni_lateral_cropped_SER_18037,
        uni_lateral_cropped_PE2_94980,
        uni_lateral_cropped_PE5_92945,
        uni_lateral_cropped_original_DCE_59184,

        T2_TIRM_AX_75890,

        DBT_slices,
        PARENCHYMAAL_PHASE,
        PET_IR_NE_AC_WB,
        PET_AC_80118,
        NEPHRO,
        PET_CT,
        oblProstate,
        PET_AC_66518,
        LIVER_PELVISHASTESAGPOS,
        LIVER_PELVISHASTEAXIALP,
        LIVER_KIDNEYTIFL2DAXIAL,

        HELICAL_MODE_61766,
        INSPIRATION_21722,
        INSPIRATION_67948,
        INSPIRATION_87999,
        INSPIRATION_90906,
        INSPIRATION_98909,
        EXPIRATION_08393,
        EXPIRATION_30420,
        EXPIRATION_70053,
        EXPIRATION_97449,
        EXPIRATION_78701,
        Recon_2_58420,
        Recon_3_13992,
        PRONE_INSPIRATION_07278,
        PRONE_INSPIRATION_18886,
        PRONE_INSPIRATION_50886,
        PRONE_INSPIRATION_81487,
        PRONE_INSPIRATION_89538,
        Abd_CT_96816_89538,
        PET_WB_60732,
        PET_WB_44674,

        stagbeetle,
        present,
        bonsai,
        aneurism,
        pawpawsaurus,
        Spathorhynchus_fossorium
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
        public static Format format;
        public static int scaling = 1;

        public static int maxX = 0, maxY = 0, maxZ = 0;
        public static int minX = 0, minY = 0, minZ = 0;
        public static bool bound = false;

        public static void SetBound(int xMax, int yMax, int zMax, int xMin, int yMin, int zMin)
        {
            maxX = xMax;
            maxY = yMax;
            maxZ = zMax;
            minX = xMin;
            minY = yMin;
            minZ = zMin;
            bound = true;
        }

        public static void UnsetBound()
        {
            bound = false;
        }

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
                    format = Format.file;
                    break;
                case dataset.CThead:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\CThead\\";
                    thresh = 512;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\CThead_{suffix}.obj";
                    repStr = "CThead.";
                    isDCM = false;
                    format = Format.file;
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
                    format = Format.dcm;
                    break;
                case dataset.F_Head:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Head\\Head\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Head_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.F_Hip:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Hip\\Hip\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Hip_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.F_Knee:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Knee\\Knee\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Knee_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.F_Pelvis:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Pelvis\\Pelvis\\";
                    thresh = 1220;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Pelvis_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.F_Shoulder:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\F_Shoulder\\Shoulder\\";
                    thresh = 1180;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\F_Shoulder_{suffix}.obj";
                    repStr = "vhf.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.M_Head:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Head\\Head\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Head_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true;
                    format = Format.dcm;
                    break; 
                case dataset.M_Hip:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Hip\\Hip\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Hip_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.M_Pelvis:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Pelvis\\Pelvis\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Pelvis_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.M_Shoulder:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\M_Shoulder\\Shoulder\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\M_Shoulder_{suffix}.obj";
                    repStr = "vhm.";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.MRbrain:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\MRbrain\\";
                    thresh = 1538;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRbrain_{suffix}.obj";
                    repStr = "MRbrain.";
                    isDCM = false;
                    format = Format.file;
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
                    format = Format.dcm;
                    break;
                case dataset.ChestCT:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Subject (1)\\98.12.2\\";
                    thresh = 1280;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\ChestCT_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.WristCT:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\w3568970\\batch3\\";
                    thresh = 1280;
                    length = 440;
                    width = 440;
                    outFilename = $"D:\\College Work\\MP\\WristCT_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.MRHead3:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 005 [MR - SAG RF FAST VOL FLIP 3]\\";
                    thresh = 10;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead3_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.MRHead5:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 003 [MR - SAG RF FAST VOL FLIP 5]\\";
                    thresh = 30;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead5_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.MRHead20:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 004 [MR - SAG RF FAST VOL FLIP 20]\\";
                    thresh = 20;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead20_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.MRHead30:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 002 [MR - SAG RF FAST VOL FLIP 30]\\";
                    thresh = 20;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead30_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.MRHead40:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Series 006 [MR - SAG RF FAST VOL FLIP 40]\\";
                    thresh = 40;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\MRHead40_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.SkullLrg:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\DICOM\\";
                    thresh = 1350;
                    length = 400;
                    width = 400;
                    outFilename = $"D:\\College Work\\MP\\SkullLrg_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PlaneScout:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\1.000000-ISPY2 3 Plane Scout-18415\\";
                    thresh = 40;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\PlaneScout_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.WATERIDEAL_T2FSEASSET:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\3.000000-ISPY2 WATERIDEAL T2FSEASSET no NP-35054\\";
                    thresh = 40;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\WATERIDEAL_T2FSEASSET_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.bval100260048008T22nexeDWI:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\5.000000-ACRIN-6698 4bval100260048008T22nexeDWI-63148\\";
                    thresh = 40;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\bval100260048008T22nexeDWI_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.multiPhase:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\6.000000-ISPY2 multiPhase38425680-100sec.Dyn.-27199\\";
                    thresh = 256;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\multiPhase_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.FATIDEAL_T2FSEASSET:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\300.000000-ISPY2 FATIDEAL T2FSEASSET no NP-34618\\";
                    thresh = 40;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\FATIDEAL_T2FSEASSET_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.InPhaseIDEAL_T2FSEASSET:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\301.000000-ISPY2 InPhaseIDEAL T2FSEASSET no NP-87919\\";
                    thresh = 40;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\InPhaseIDEAL_T2FSEASSET_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.uni_lateral_cropped_PE2_55883:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\61002.000000-ISPY2 VOLSER uni-lateral cropped PE2-55883\\";
                    thresh = 40;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\uni_lateral_cropped_PE2_55883_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.uni_lateral_cropped_PE6_69418:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\61006.000000-ISPY2 VOLSER uni-lateral cropped PE6-69418\\";
                    thresh = 40;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\uni_lateral_cropped_PE6_69418_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.uni_lateral_cropped_original_DCE_21414:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-21-2002-102212T3-ACRIN-6698ISPY2MRIT3-13902\\61800.000000-ISPY2 VOLSER uni-lateral cropped original DCE-21414\\";
                    thresh = 40;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\uni_lateral_cropped_original_DCE_21414_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;

                case dataset.STIR_ASSET:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\4.000000-ISPY2 Ax T2 STIR ASSET-31580\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\STIR_ASSET_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.ACRIN_6698_Ax_DWI:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\5.000000-ACRIN-6698 Ax DWI 100600800 DE-79030\\";
                    thresh = 100;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\ACRIN_6698_Ax_DWI_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.Ax_VIBRANT_MPh:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\600.000000-ISPY2 Ax VIBRANT MPh C-26652\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Ax_VIBRANT_MPh_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.Ph1Ax_VIBRANT_MPh:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\601.000000-ISPY2 Ph1Ax VIBRANT MPh C-02896\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Ph1Ax_VIBRANT_MPh_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.Ph2Ax_VIBRANT_MPh:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\602.000000-ISPY2 Ph2Ax VIBRANT MPh C-40608\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Ph2Ax_VIBRANT_MPh_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.Ph3Ax_VIBRANT_MPh:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\603.000000-ISPY2 Ph3Ax VIBRANT MPh C-55587\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Ph3Ax_VIBRANT_MPh_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.Ph4Ax_VIBRANT_MPh:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\604.000000-ISPY2 Ph4Ax VIBRANT MPh C-22587\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Ph4Ax_VIBRANT_MPh_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.Ph5Ax_VIBRANT_MPh:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\605.000000-ISPY2 Ph5Ax VIBRANT MPh C-87700\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Ph5Ax_VIBRANT_MPh_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.Ph6Ax_VIBRANT_MPh:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\606.000000-ISPY2 Ph6Ax VIBRANT MPh C-16436\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Ph6Ax_VIBRANT_MPh_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.uni_lateral_cropped_SER_18037:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\61000.000000-ISPY2 VOLSER uni-lateral cropped SER-18037\\";
                    thresh = 100;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\uni_lateral_cropped_SER_18037_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.uni_lateral_cropped_PE2_94980:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\61002.000000-ISPY2 VOLSER uni-lateral cropped PE2-94980\\";
                    thresh = 100;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\uni_lateral_cropped_PE2_94980_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.uni_lateral_cropped_PE5_92945:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\61005.000000-ISPY2 VOLSER uni-lateral cropped PE5-92945\\";
                    thresh = 100;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\uni_lateral_cropped_PE5_92945_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.uni_lateral_cropped_original_DCE_59184:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\09-20-2003-103939T3-ACRIN-6698ISPY2MRIT3-16224\\61800.000000-ISPY2 VOLSER uni-lateral cropped original DCE-59184\\";
                    thresh = 100;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\uni_lateral_cropped_original_DCE_59184_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.T2_TIRM_AX_75890:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\10.000000-ISPY2 T2 TIRM AX-75890\\";
                    thresh = 100;
                    length = 768;
                    width = 768;
                    outFilename = $"D:\\College Work\\MP\\T2_TIRM_AX_75890_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;

                case dataset.DBT_slices:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\DBT slices-78838\\";
                    thresh = 100;
                    length = 1024;
                    width = 1421;
                    outFilename = $"D:\\College Work\\MP\\DBT_slices_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.PARENCHYMAAL_PHASE:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\3.000000-PARENCHYMAL PHASE Sep1999-95798\\";
                    thresh = 1200;
                    length = 1024;
                    width = 1421;
                    outFilename = $"D:\\College Work\\MP\\PARENCHYMAAL_PHASE_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.PET_IR_NE_AC_WB:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\PET IR NO AC WB-89718\\";
                    thresh = 100;
                    length = 128;
                    width = 128;
                    outFilename = $"D:\\College Work\\MP\\PET_IR_NE_AC_WB_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.PET_AC_80118:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\1.000000-PET AC-80118\\";
                    thresh = 100;
                    length = 128;
                    width = 128;
                    outFilename = $"D:\\College Work\\MP\\PET_AC_80118_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.NEPHRO:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\5.000000-NEPHRO  4.0  B40f  M0.4-18678\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\NEPHRO_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.PET_CT:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\603.000000-PET-CT SERIES-72118\\";
                    thresh = 100;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\PET_CT_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.oblProstate:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\4.000000-t2spcrstaxial oblProstate-50358\\";
                    thresh = 100;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\oblProstate_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.PET_AC_66518:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\3.000000-PET AC-66518\\";
                    thresh = 100;
                    length = 128;
                    width = 128;
                    outFilename = $"D:\\College Work\\MP\\PET_AC_66518_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.LIVER_PELVISHASTESAGPOS:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\16.000000-LIVER-PELVISHASTESAGPOS-74838\\";
                    thresh = 1200;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\LIVER_PELVISHASTESAGPOS_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.LIVER_PELVISHASTEAXIALP:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\15.000000-LIVER-PELVISHASTEAXIALP-65398\\";
                    thresh = 1200;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\LIVER_PELVISHASTEAXIALP_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;
                case dataset.LIVER_KIDNEYTIFL2DAXIAL:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\6.000000-LIVER-KIDNEYTIFL2DAXIAL-30038\\";
                    thresh = 1200;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\LIVER_KIDNEYTIFL2DAXIAL_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    scaling = 2;
                    break;

                case dataset.HELICAL_MODE_61766:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\2.000000-HELICAL MODE-61766\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\HELICAL_MODE_61766_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.INSPIRATION_21722:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\2.000000-INSPIRATION-21722\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\INSPIRATION_21722_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.INSPIRATION_67948:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\2.000000-INSPIRATION-67948\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\INSPIRATION_67948_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.INSPIRATION_87999:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\2.000000-INSPIRATION-87999\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\INSPIRATION_87999_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.INSPIRATION_90906:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\2.000000-INSPIRATION-90906\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\INSPIRATION_90906_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.INSPIRATION_98909:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\2.000000-INSPIRATION-98909\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\INSPIRATION_98909_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.EXPIRATION_08393:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\3.000000-EXPIRATION-08393\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\EXPIRATION_08393_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.EXPIRATION_30420:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\3.000000-EXPIRATION-30420\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\EXPIRATION_30420_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.EXPIRATION_70053:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\3.000000-EXPIRATION-70053\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\EXPIRATION_70053_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.EXPIRATION_97449:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\3.000000-EXPIRATION-97449\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\EXPIRATION_97449_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.EXPIRATION_78701:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\3.000000-EXPIRATION-78701\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\EXPIRATION_78701_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.Recon_2_58420:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\3.000000-Recon 2-58420\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Recon_2_58420_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.Recon_3_13992:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\4.000000-Recon 3-13992\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Recon_3_13992_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PRONE_INSPIRATION_07278:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\5.000000-PRONE INSPRATION-07278\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\PRONE_INSPIRATION_07278_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PRONE_INSPIRATION_18886:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\5.000000-PRONE INSPRATION-18886\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\PRONE_INSPIRATION_18886_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PRONE_INSPIRATION_50886:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\5.000000-PRONE INSPRATION-50886\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\PRONE_INSPIRATION_50886_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PRONE_INSPIRATION_81487:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\5.000000-PRONE INSPRATION-81487\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\PRONE_INSPIRATION_81487_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PRONE_INSPIRATION_89538:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\5.000000-PRONE INSPRATION-89538\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\PRONE_INSPIRATION_89538_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.Abd_CT_96816_89538:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\2.000000-Abd.CT 5.0 B30s-96816\\";
                    thresh = 1200;
                    length = 512;
                    width = 512;
                    outFilename = $"D:\\College Work\\MP\\Abd_CT_96816_89538_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PET_WB_60732:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\605.000000-PET WB-uncorrected-60732\\";
                    thresh = 1200;
                    length = 128;
                    width = 128;
                    outFilename = $"D:\\College Work\\MP\\PET_WB_60732_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;
                case dataset.PET_WB_44674:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\tgca-luad\\606.000000-PET WB-44674\\";
                    thresh = 1200;
                    length = 128;
                    width = 128;
                    outFilename = $"D:\\College Work\\MP\\PET_WB_44674_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = true;
                    format = Format.dcm;
                    break;

                case dataset.stagbeetle:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\stagbeetle832x832x494.dat";
                    thresh = 800;
                    length = 832;
                    width = 832;
                    outFilename = $"D:\\College Work\\MP\\stagbeetle_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = false;
                    format = Format.dat;
                    break;
                case dataset.present:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\present492x492x442.dat";
                    thresh = 300;
                    length = 492;
                    width = 492;
                    outFilename = $"D:\\College Work\\MP\\present_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = false;
                    format = Format.dat;
                    break;
                case dataset.bonsai:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\bonsai_256x256x256_uint8.raw";
                    thresh = 50;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\bonsai_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = false;
                    format = Format.raw;
                    break;
                case dataset.aneurism:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\aneurism_256x256x256_uint8.raw";
                    thresh = 60;
                    length = 256;
                    width = 256;
                    outFilename = $"D:\\College Work\\MP\\aneurism_{suffix}.obj";
                    repStr = "repStr";
                    isDCM = false;
                    format = Format.raw;
                    break;
                case dataset.pawpawsaurus:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Pawpawsaurus\\";
                    thresh = 1200;
                    length = 646;
                    width = 958;
                    outFilename = $"D:\\College Work\\MP\\pawpawsaurus_{suffix}.obj";
                    repStr = "pawpa";
                    isDCM = false;
                    format = Format.tiff;
                    break;
                case dataset.Spathorhynchus_fossorium:
                    filePath = "C:\\Users\\antonyDev\\Downloads\\DICOMS\\Spathorhynchus_fossorium\\Spathorhynchus_fossorium\\8bitTIFF\\";
                    thresh = 3000;
                    length = 1024;
                    width = 1024;
                    outFilename = $"D:\\College Work\\MP\\Spathorhynchus_fossorium_{suffix}.obj";
                    repStr = "spath";
                    isDCM = false;
                    format = Format.tiff;
                    break;
            }
        }

        public static FileInfo CreateVolume(int size)
        {
            string fileName = outFilename;
            FileInfo
                fi = new FileInfo(fileName);
            try
            {
                context = Context.Create(builder => builder.Default().EnableAlgorithms());
                accelerator = context.CreateCudaAccelerator(0);
                //accelerator = context.CreateCPUAccelerator(0);
                triTable = accelerator.Allocate1D<Edge>(triangleTable);
                //Console.WriteLine("Threshold:" + (thresh - 1024));

                if (slices == null)
                {
                    Console.WriteLine(outFilename);
                    if (isDCM)
                    {
                        ReadDCM();
                    }
                    else
                    {
                        if (format == Format.dat || format == Format.raw)
                            ReadFile();
                        else
                            ReadFiles();
                    }
                    //CreateSphere(width);
                }

                if (bound)
                {

                    Slice();
                }

                Console.WriteLine("Threshold: " + thresh);
                //Console.WriteLine("MidPt Threshold: " + thresh);

                OctreeSize = Math.Max(width, slices.GetLength(0));
                if (Math.Log(Math.Max(width, slices.GetLength(0)), 2) % 1 > 0)
                {
                    OctreeSize = (int)Math.Pow(2, (int)Math.Log(Math.Max(width, slices.GetLength(0)), 2) + 1);
                }

                sliced = accelerator.Allocate3DDenseXY<ushort>(slices);
            }
            catch(Exception ex)
            {
                sliced.Dispose();
                triTable.Dispose();
                accelerator.Dispose();
                context.Dispose();
            }
            Console.WriteLine("X: " + slices.GetLength(0));
            Console.WriteLine("Y: " + slices.GetLength(1));
            Console.WriteLine("Z: " + slices.GetLength(2));
            return fi;
        }

        public static string ReadDCM()
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

            //slices = new ushort[Math.Min(files.Length, 512), length, width];
            slices = new ushort[files.Length, length, width];
            //if (length == 256 && width == 256)
            //    slices = new ushort[width, width, width];

            //if (length == 512 && width == 512)
            //    slices = new ushort[width, width, width];

            string modality;
            foreach (var file in files)
            {
                dicoms = DicomFile.Open(file.FullName);
                if (k == 0)
                {
                    Console.WriteLine(dicoms.Dataset.GetValue<string>(DicomTag.Modality, 0));
                    modality = dicoms.Dataset.GetValue<string>(DicomTag.Modality, 0);
                }

                CreateBmp(dicoms, k);
                k++;
                //if (k > 511)
                //    break;
            }

            return modality;

        }

        public static void ReadFiles()
        {
            ushort k = 0;
            DirectoryInfo d = new DirectoryInfo(filePath);

            FileInfo[] files = d.GetFiles().Where(file => !file.Attributes.HasFlag(FileAttributes.Hidden)).ToArray();
            //string[] s = files.Select(x => x.Name.Replace(repStr, "").Split('.').First()).ToArray();
            files = files.OrderBy(x => int.Parse(x.Name.Replace(repStr, "").Split('.').First())).ToArray();

            slices = new ushort[files.Length, length, width];

            //if (length == 256 && width == 256)
            //    slices = new ushort[width, width, width];

            //if (length == 512 && width == 512)
            //    slices = new ushort[width, width, width];

            foreach (var file in files)
            {
                if (format == Format.file)
                    Decode(file.FullName, k);
                else if (format == Format.tiff)
                    DecodeTIFF(file.FullName, k);
                k++;
                //if (k > 1023)
                //    break;
            }

        }

        public static void ReadFile()
        {
            //if (length == 256 && width == 256)
            //    slices = new ushort[width, width, width];

            //if (length == 512 && width == 512)
            //    slices = new ushort[width, width, width];

            if (format == Format.dat)
                DecodeDat(filePath);
            else if (format == Format.raw)
                DecodeRaw8(filePath);

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

            ushort max = 0;
            if (pixelData is GrayscalePixelDataU16)
            {
                for (int i = 0; i < pixelData.Width; i++)
                {
                    for (int j = 0; j < pixelData.Height; j++)
                    {
                        int index = j * header.Width + i;
                        slices[k, j, i] = (ushort)pixelData.Data[index];
                        if(slices[k, j, i] > max)
                        {
                            max = slices[k, j, i];
                        }
                    }
                }
            }
            //thresh = (ushort)(max / 4);
        }

        public static void DecodeTIFF(string path, int k)
        {
            MagickImage image = new MagickImage(path);
            var pixelValues = image.GetPixels().GetValues();

            ushort max = 0;
            if (true)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        int index = j * image.Width + i;
                        slices[k, j, i] = (ushort)pixelValues[index];
                        if (slices[k, j, i] > max)
                        {
                            max = slices[k, j, i];
                        }
                    }
                }
            }
            //thresh = (ushort)(5 * max / 8);
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

            ushort max = 0;
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
                        if (slices[k, j, i] > max)
                        {
                            max = slices[k, j, i];
                        }
                    }
                }
            }
            //thresh = (ushort)(max / 4);
        }

        public static void DecodeRaw8(string path)
        {
            FileStream stream = File.OpenRead(path);

            int sizeX = 256, sizeY = 256, sizeZ = 256; // declare variables for header information
            slices = new ushort[256, 256, 256];

            ushort max = 0;
            for (int k = 0; k < sizeZ; k++)
            {
                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        slices[k, j, i] = (ushort)stream.ReadByte();
                        if (slices[k, j, i] > max)
                        {
                            max = slices[k, j, i];
                        }
                    }
                }
            }

            //thresh = (ushort)(max / 4);

            stream.Close();
            stream.Dispose();
        }

        public static void DecodeDat(string path)
        {
            FileStream stream = File.OpenRead(path);

            int sizeX, sizeY, sizeZ; // declare variables for header information
            sizeX = stream.ReadByte();
            sizeX += stream.ReadByte() * 256;
            sizeY = stream.ReadByte();
            sizeY += stream.ReadByte() * 256;
            sizeZ = stream.ReadByte();
            sizeZ += stream.ReadByte() * 256;

            slices = new ushort[sizeZ, sizeY, sizeX];

            ushort max = 0;
            for (int k = 0; k < sizeZ; k++)
            {
                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        slices[k, j, i] = (ushort)stream.ReadByte();
                        slices[k, j, i] += (ushort)(stream.ReadByte() * 256);
                        if (slices[k, j, i] > max)
                        {
                            max = slices[k, j, i];
                        }
                    }
                }
            }

            //thresh = (ushort)(max / 4);

            stream.Close();
            stream.Dispose();
        }

        public static void Slice()
        {
            int xSize = maxX - minX;
            int ySize = maxY - minY;
            int zSize = maxZ - minZ;

            ushort[,,] tempSlice = new ushort[zSize + 1, ySize + 1, xSize + 1];
            
            for(int i = 0; i < zSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    for (int k = 0; k < xSize; k++)
                    {
                        tempSlice[i, j, k] = slices[i + minZ, j + minY, k + minX];
                    }
                }
            }
            slices = tempSlice;
            UnsetBound();
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
