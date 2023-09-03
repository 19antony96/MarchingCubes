using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace MarchingCubes
{
    class Program
    {

        static void Main(string[] args)
        {

            int n = 20;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            File.WriteAllText("Result.txt", "Run started at: " + DateTime.Now.ToString() + ", N = " + n);
            File.AppendAllLines("Result.txt", new string[] { "" });
            TimeSpan FirstPassTime = TimeSpan.Zero;
            TimeSpan HPCreateTime = TimeSpan.Zero;
            TimeSpan HPTraverseTime = TimeSpan.Zero;
            TimeSpan HPExtractionTime = TimeSpan.Zero;
            TimeSpan TotalTime = TimeSpan.Zero;
            double RunningAvg = 0;
            double RunningVariance = 0;
            TimeSpan MaxTime = TimeSpan.Zero;
            TimeSpan MinTime = TimeSpan.Zero;
            List<int> HP_r = new List<int>() { 4, 5, 6, 7, 8, 10, 11, 14, 15 };
            long padding = 0, layerCount = 0, baseLayerSize = 0, SumSqrTime = 0, volume = 0, active_nh = 0, n_triangles = 0;
            int xMin = 0, yMin = 0, zMin = 0, xMax = 0, yMax = 0, zMax = 0;
            string fname = "Results_" + DateTime.Now.ToString("yyyy-MM-dd_hh_mm_ss") + ".xlsx";
            List<dataset> dList = new List<dataset>()
            {
                dataset.bunny,
                dataset.CThead,
                dataset.F_Ankle,
                dataset.F_Head,
                dataset.F_Pelvis,
                dataset.MRbrain,
                dataset.WristCT,
                dataset.MRHead40,
                dataset.SkullLrg,

                dataset.F_Hip,
                dataset.F_Knee,
                dataset.F_Shoulder,
                dataset.M_Head,
                dataset.M_Hip,
                dataset.M_Pelvis,
                dataset.M_Shoulder,
                //dataset.MRHead3,
                //dataset.MRHead5,
                //dataset.MRHead20,
                //dataset.MRHead30,
                dataset.ChestSmall,
                dataset.ChestCT,



                //dataset.PlaneScout,
                //dataset.WATERIDEAL_T2FSEASSET,
                //dataset.bval100260048008T22nexeDWI,
                ////dataset.multiPhase,
                //dataset.FATIDEAL_T2FSEASSET,
                //dataset.InPhaseIDEAL_T2FSEASSET,
                //dataset.uni_lateral_cropped_SER_111387,
                //dataset.uni_lateral_cropped_PE2_55883,
                //dataset.uni_lateral_cropped_PE6_69418,
                //dataset.uni_lateral_cropped_original_DCE_21414,

                //dataset.STIR_ASSET,
                //dataset.ACRIN_6698_Ax_DWI,
                //dataset.Ax_VIBRANT_MPh,
                //dataset.Ph1Ax_VIBRANT_MPh,
                //dataset.Ph2Ax_VIBRANT_MPh,
                //dataset.Ph3Ax_VIBRANT_MPh,
                //dataset.Ph4Ax_VIBRANT_MPh,
                //dataset.Ph5Ax_VIBRANT_MPh,
                //dataset.Ph6Ax_VIBRANT_MPh,
                //dataset.uni_lateral_cropped_SER_18037,
                //dataset.uni_lateral_cropped_PE2_94980,
                //dataset.uni_lateral_cropped_PE5_92945,
                //dataset.uni_lateral_cropped_original_DCE_59184,

                //dataset.T2_TIRM_AX_75890,


                //dataset.DBT_slices,
                //dataset.PARENCHYMAAL_PHASE,
                //dataset.PET_IR_NE_AC_WB,
                //dataset.PET_AC_80118,
                //dataset.NEPHRO,
                //dataset.PET_CT,
                //dataset.oblProstate,
                //dataset.PET_AC_66518,
                //dataset.LIVER_PELVISHASTESAGPOS,
                //dataset.LIVER_PELVISHASTEAXIALP,
                //dataset.LIVER_KIDNEYTIFL2DAXIAL,


                //dataset.HELICAL_MODE_61766,
                ////dataset.INSPIRATION_21722,
                ////dataset.INSPIRATION_67948,
                ////dataset.INSPIRATION_87999,
                ////dataset.INSPIRATION_90906,
                ////dataset.INSPIRATION_98909,
                ////dataset.EXPIRATION_08393,
                ////dataset.EXPIRATION_30420,
                ////dataset.EXPIRATION_70053,
                ////dataset.EXPIRATION_97449,
                ////dataset.EXPIRATION_78701,
                ////dataset.Recon_2_58420,
                ////dataset.Recon_3_13992,
                ////dataset.PRONE_INSPIRATION_07278,
                ////dataset.PRONE_INSPIRATION_18886,
                ////dataset.PRONE_INSPIRATION_50886,
                ////dataset.PRONE_INSPIRATION_81487,
                ////dataset.PRONE_INSPIRATION_89538,
                //dataset.Abd_CT_96816_89538,
                //dataset.PET_WB_60732,
                //dataset.PET_WB_44674,


                dataset.stagbeetle,
                dataset.present,
                dataset.bonsai,
                dataset.aneurism,
                //dataset.pawpawsaurus,
                //dataset.Spathorhynchus_fossorium
            };
            for (int n_bind = 0; n_bind < 1; n_bind++)
            {
                fname = "Results_" + DateTime.Now.ToString("yyyy-MM-dd_hh_mm_ss") + ".xlsx";
                bool bind = n_bind == 0;

                if (bind)
                {
                    fname += "_bind";
                }

                using (var package = new ExcelPackage(fname))
                {
                    try
                    {
                        foreach (dataset ds in dList)
                        {
                            string name = Enum.GetName(typeof(dataset), ds);
                            ExcelWorksheet sheet = package.Workbook.Worksheets.Add(name);
                            int rowIndex = 1, columnIndex = 2;

                            sheet.Cells["A2"].Value = "First Time Pass";
                            sheet.Cells["A3"].Value = "Construction Time";
                            sheet.Cells["A4"].Value = "Traversal Time";
                            sheet.Cells["A5"].Value = "Extraction Time";
                            sheet.Cells["A6"].Value = "Average Time";
                            sheet.Cells["A7"].Value = "Min Time";
                            sheet.Cells["A8"].Value = "Max Time";
                            sheet.Cells["A9"].Value = "Variance";
                            sheet.Cells["A10"].Value = "Padding";
                            sheet.Cells["A11"].Value = "Layer Count";
                            sheet.Cells["A12"].Value = "Base Layer Size";
                            sheet.Cells["A13"].Value = "Total # NH's";
                            sheet.Cells["A14"].Value = "Active NH's";
                            sheet.Cells["A15"].Value = "NH Density";
                            sheet.Cells["A16"].Value = "Total # Triangles";
                            sheet.Cells["A17"].Value = "Triangle Density";
                            sheet.Cells["A18"].Value = "Average Reduction Factor";
                            sheet.Cells["A19"].Value = "Range Reduction Factors";

                            MarchingCubes.SetValues(ds, "CPU");
                            try
                            {
                                for (int j = 0; j < 1; j++)
                                {
                                    sheet.Cells[ColumnIndexToColumnLetter(columnIndex) + "1"].Value = $"CPU";
                                    if (true)
                                    {
                                        Console.WriteLine("Run: " + j);
                                        NaiveCPU run1 = new NaiveCPU(25);
                                        Thread.Sleep(50);
                                        Console.WriteLine("--------------------------------------------------------------------------------");
                                    }
                                    FirstPassTime += NaiveCPU.FirstPassTime;
                                    HPExtractionTime += NaiveCPU.ExtractionTime;
                                    TotalTime += NaiveCPU.TotalTime;
                                    RunningAvg = TotalTime.Ticks / n;
                                    SumSqrTime += (NaiveCPU.TotalTime.Ticks * NaiveCPU.TotalTime.Ticks);
                                    RunningVariance = (long)((SumSqrTime / (j + 1)) - Math.Pow(RunningAvg, 2));
                                    if (!(MaxTime == TimeSpan.Zero))
                                    {
                                        MaxTime = MaxTime > NaiveCPU.TotalTime ? MaxTime : NaiveCPU.TotalTime;
                                    }
                                    else
                                    {
                                        MaxTime = NaiveCPU.TotalTime;
                                    }

                                    if (!(MinTime == TimeSpan.Zero))
                                    {
                                        MinTime = MinTime < NaiveCPU.TotalTime ? MinTime : NaiveCPU.TotalTime;
                                    }
                                    else
                                    {
                                        MinTime = NaiveCPU.TotalTime;
                                    }
                                    if (bind)
                                    {

                                        xMax = NaiveCPU.maxX;
                                        yMax = NaiveCPU.maxY;
                                        zMax = NaiveCPU.maxZ;
                                        xMin = NaiveCPU.minX;
                                        yMin = NaiveCPU.minY;
                                        zMin = NaiveCPU.minZ;

                                        MarchingCubes.SetBound(xMax, yMax, zMax, xMin, yMin, zMin);
                                    }
                                }
                                // write to console
                                //Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                                //Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                                Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                                //Console.WriteLine("Variance (clock cycles): " + RunningVariance);
                                //Console.WriteLine("Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)));
                                //Console.WriteLine("Max Time: " + MaxTime);
                                //Console.WriteLine("Min Time: " + MinTime);

                                // write to txt
                                File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                                File.AppendAllLines("Result.txt", new string[] { $"CPU"
                            , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                            , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                            , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                            , "Variance: " + "Variance (clock cycles): " + RunningVariance
                            , "Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance))
                            , "Max Time: " + MaxTime
                            , "Min Time: " + MinTime
                            , "X Max: " + xMax
                            , "Y Max: " + yMax
                            , "Z Max: " + zMax
                            , "X Min: " + xMin
                            , "Y Min: " + yMin
                            , "Z Min: " + zMin
                            });

                                // write to excel
                                string columnLetter = ColumnIndexToColumnLetter(columnIndex);
                                sheet.Cells[columnLetter + "2"].Value = TimeSpan.FromTicks(FirstPassTime.Ticks / n).TotalSeconds;
                                sheet.Cells[columnLetter + "5"].Value = TimeSpan.FromTicks(HPExtractionTime.Ticks / n).TotalSeconds;
                                sheet.Cells[columnLetter + "6"].Value = TimeSpan.FromTicks(TotalTime.Ticks / n).TotalSeconds;
                                sheet.Cells[columnLetter + "7"].Value = MinTime.TotalSeconds;
                                sheet.Cells[columnLetter + "8"].Value = MaxTime.TotalSeconds;
                                sheet.Cells[columnLetter + "9"].Value = TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)).TotalSeconds;
                                sheet.Cells[columnLetter + "10"].Value = "X Max: " + NaiveCPU.maxX;
                                sheet.Cells[columnLetter + "11"].Value = "Y Max: " + NaiveCPU.maxY;
                                sheet.Cells[columnLetter + "12"].Value = "Z Max: " + NaiveCPU.maxZ;
                                sheet.Cells[columnLetter + "13"].Value = "X Min: " + NaiveCPU.minX;
                                sheet.Cells[columnLetter + "14"].Value = "Y Min: " + NaiveCPU.minY;
                                sheet.Cells[columnLetter + "15"].Value = "Z Min: " + NaiveCPU.minZ;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception caught at " + DateTime.Now.ToString());
                                Console.WriteLine(ex.Message);
                                Console.Write(ex.StackTrace);
                                Console.WriteLine();
                            }

                            FirstPassTime = TimeSpan.Zero;
                            HPExtractionTime = TimeSpan.Zero;
                            TotalTime = TimeSpan.Zero;
                            SumSqrTime = 0;
                            RunningAvg = 0;
                            RunningVariance = 0;
                            MaxTime = TimeSpan.Zero;
                            MinTime = TimeSpan.Zero;
                            padding = 0;
                            layerCount = 0;
                            baseLayerSize = 0;
                            columnIndex++;

                            Console.WriteLine("--------------------------------------------------------------------------------");
                            //MarchingCubes.SetValues(ds, "GPU");
                            //NaiveGPU run2 = new NaiveGPU(25);
                            //Console.WriteLine("--------------------------------------------------------------------------------");
                            Thread.Sleep(1000);
                            foreach(int i in HP_r)
                            {
                                Thread.Sleep(1000);
                                try
                                {
                                    int s = i;
                                    for (int j = 0; j < n; j++)
                                    {
                                        MarchingCubes.SetValues(ds, $"HP_{s}");
                                        sheet.Cells[ColumnIndexToColumnLetter(columnIndex) + "1"].Value = $"HP_{s}";
                                        if (true)
                                        {
                                            Console.WriteLine("Run: " + j);
                                            HistoPyramidGeneric run3 = new HistoPyramidGeneric(s);
                                            Thread.Sleep(50);
                                            Console.WriteLine("--------------------------------------------------------------------------------");
                                        }
                                        FirstPassTime += HistoPyramidGeneric.FirstPassTime;
                                        HPCreateTime += HistoPyramidGeneric.HPCreateTime;
                                        HPTraverseTime += HistoPyramidGeneric.HPTraverseTime;
                                        HPExtractionTime += HistoPyramidGeneric.HPExtractionTime;
                                        TotalTime += HistoPyramidGeneric.TotalTime;
                                        RunningAvg = TotalTime.Ticks / n;
                                        SumSqrTime += (HistoPyramidGeneric.TotalTime.Ticks * HistoPyramidGeneric.TotalTime.Ticks);
                                        RunningVariance = (long)((SumSqrTime / (j + 1)) - Math.Pow(RunningAvg, 2));
                                        if (!(MaxTime == TimeSpan.Zero))
                                        {
                                            MaxTime = MaxTime > HistoPyramidGeneric.TotalTime ? MaxTime : HistoPyramidGeneric.TotalTime;
                                        }
                                        else
                                        {
                                            MaxTime = HistoPyramidGeneric.TotalTime;
                                        }

                                        if (!(MinTime == TimeSpan.Zero))
                                        {
                                            MinTime = MinTime < HistoPyramidGeneric.TotalTime ? MinTime : HistoPyramidGeneric.TotalTime;
                                        }
                                        else
                                        {
                                            MinTime = HistoPyramidGeneric.TotalTime;
                                        }
                                        padding += HistoPyramidGeneric.padding;
                                        layerCount += HistoPyramidGeneric.layerCount;
                                        baseLayerSize += HistoPyramidGeneric.baseLayerSize;
                                    }
                                    // write to console
                                    //Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                                    //Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                                    //Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                                    //Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                                    Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                                    //Console.WriteLine("Variance (clock cycles): " + RunningVariance);
                                    //Console.WriteLine("Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)));
                                    //Console.WriteLine("Max Time: " + MaxTime);
                                    //Console.WriteLine("Min Time: " + MinTime);
                                    //Console.WriteLine("Avg Padding: " + padding / n);
                                    //Console.WriteLine("Avg Layer Count: " + layerCount / n);
                                    //Console.WriteLine("Avg Base Layer Size: " + baseLayerSize);

                                    // write to txt
                                    File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                                    File.AppendAllLines("Result.txt", new string[] { $"HP_{s}"
                            , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                            , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                            , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                            , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                            , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                            , "Variance: " + "Variance (clock cycles): " + RunningVariance
                            , "Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance))
                            , "Max Time: " + MaxTime
                            , "Min Time: " + MinTime
                            , "Avg Padding: " + padding / n
                            , "Avg Layer Count: " + layerCount / n
                            , "Avg Base Layer Size: " + baseLayerSize / n});

                                    // write to excel
                                    string columnLetter = ColumnIndexToColumnLetter(columnIndex);
                                    sheet.Cells[columnLetter + "2"].Value = TimeSpan.FromTicks(FirstPassTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "3"].Value = TimeSpan.FromTicks(HPCreateTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "4"].Value = TimeSpan.FromTicks(HPTraverseTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "5"].Value = TimeSpan.FromTicks(HPExtractionTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "6"].Value = TimeSpan.FromTicks(TotalTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "7"].Value = MinTime.TotalSeconds;
                                    sheet.Cells[columnLetter + "8"].Value = MaxTime.TotalSeconds;
                                    sheet.Cells[columnLetter + "9"].Value = TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)).TotalSeconds;
                                    sheet.Cells[columnLetter + "10"].Value = padding / n;
                                    sheet.Cells[columnLetter + "11"].Value = layerCount / n;
                                    sheet.Cells[columnLetter + "12"].Value = baseLayerSize;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Exception caught at " + DateTime.Now.ToString());
                                    Console.WriteLine(ex.Message);
                                    Console.Write(ex.StackTrace);
                                    Console.WriteLine();
                                }

                                FirstPassTime = TimeSpan.Zero;
                                HPCreateTime = TimeSpan.Zero;
                                HPTraverseTime = TimeSpan.Zero;
                                HPExtractionTime = TimeSpan.Zero;
                                TotalTime = TimeSpan.Zero;
                                SumSqrTime = 0;
                                RunningAvg = 0;
                                RunningVariance = 0;
                                MaxTime = TimeSpan.Zero;
                                MinTime = TimeSpan.Zero;
                                padding = 0;
                                layerCount = 0;
                                baseLayerSize = 0;
                                columnIndex++;
                            }
                            Console.WriteLine("--------------------------------------------------------------------------------");


                            //MarchingCubes.SetValues(ds, "HP2D");
                            //for (int i = 0; i < n; i++)
                            //{
                            //    Console.WriteLine("Run: " + i);
                            //    HistoPyramid2D run4 = new HistoPyramid2D(0);
                            //    FirstPassTime += HistoPyramid2D.FirstPassTime;
                            //    HPCreateTime += HistoPyramid2D.HPCreateTime;
                            //    HPTraverseTime += HistoPyramid2D.HPTraverseTime;
                            //    HPExtractionTime += HistoPyramid2D.HPExtractionTime;
                            //    TotalTime += HistoPyramid2D.TotalTime;
                            //}
                            //Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                            //Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                            //Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                            //Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                            //Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                            //File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                            //File.AppendAllLines("Result.txt", new string[] { $"HP2D"
                            //        , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                            //        , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                            //        , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                            //        , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                            //        , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n) });
                            //FirstPassTime = TimeSpan.Zero;
                            //HPCreateTime = TimeSpan.Zero;
                            //HPTraverseTime = TimeSpan.Zero;
                            //HPExtractionTime = TimeSpan.Zero;
                            //TotalTime = TimeSpan.Zero;
                            //Thread.Sleep(50);
                            //Console.WriteLine("--------------------------------------------------------------------------------");

                            //MarchingCubes.SetValues(ds, "HP3D");
                            //for (int i = 0; i < n; i++)
                            //{
                            //    Console.WriteLine("Run: " + i);
                            //    HistoPyramid3D run5 = new HistoPyramid3D(0);
                            //    FirstPassTime += HistoPyramid3D.FirstPassTime;
                            //    HPCreateTime += HistoPyramid3D.HPCreateTime;
                            //    HPTraverseTime += HistoPyramid3D.HPTraverseTime;
                            //    HPExtractionTime += HistoPyramid3D.HPExtractionTime;
                            //    TotalTime += HistoPyramid3D.TotalTime;
                            //}
                            //Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                            //Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                            //Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                            //Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                            //Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                            //File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                            //File.AppendAllLines("Result.txt", new string[] { $"HP3D"
                            //        , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                            //        , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                            //        , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                            //        , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                            //        , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n) });
                            //FirstPassTime = TimeSpan.Zero;
                            //HPCreateTime = TimeSpan.Zero;
                            //HPTraverseTime = TimeSpan.Zero;
                            //HPExtractionTime = TimeSpan.Zero;
                            //TotalTime = TimeSpan.Zero;
                            //Thread.Sleep(50);
                            //Console.WriteLine("--------------------------------------------------------------------------------");
                            //MarchingCubes.SetValues(ds, "Octree");
                            //Octree run6 = new Octree(0);
                            //Thread.Sleep(1000);
                            //Console.WriteLine("--------------------------------------------------------------------------------");
                            //MarchingCubes.SetValues(ds, "OctreewPrior");
                            //OctreeWPrior run7 = new OctreeWPrior(0);
                            //Thread.Sleep(1000);
                            //Console.WriteLine("--------------------------------------------------------------------------------");
                            //MarchingCubes.SetValues(ds, "OctreeBONO");
                            //OctreeBONOwPrior run8 = new OctreeBONOwPrior(0);
                            //Thread.Sleep(1000);

                            Thread.Sleep(1000);
                            for (int j = 0; j < 1; j += 2)
                            {
                                Thread.Sleep(1000);
                                try
                                {
                                    //bool extend = j % 2 == 0;
                                    bool extend = false;
                                    bool reverse = false; // (j >> 1) % 2 == 0;
                                    bool byDim = false; // (j >> 2) % 2 == 0;
                                    for (int i = 0; i < n; i++)
                                    {
                                        Console.WriteLine("--------------------------------------------------------------------------------");
                                        Console.WriteLine("Run: " + i);
                                        sheet.Cells[ColumnIndexToColumnLetter(columnIndex) + "1"].Value = $"AHP" +
                                            (extend ? "_Ext" : "_NotExt")
                                            + (reverse ? "_Rev" : "_NotRev")
                                            + (byDim ? "_byDim" : "_Flat");
                                        MarchingCubes.SetValues(ds, "AdaptiveHP");
                                        AdaptiveHistoPyramid run9 = new AdaptiveHistoPyramid(0, extend, reverse, byDim);
                                        Thread.Sleep(50);
                                        Console.WriteLine("------------------------------------------------------------------------------");

                                        FirstPassTime += AdaptiveHistoPyramid.FirstPassTime;
                                        HPCreateTime += AdaptiveHistoPyramid.HPCreateTime;
                                        HPTraverseTime += AdaptiveHistoPyramid.HPTraverseTime;
                                        HPExtractionTime += AdaptiveHistoPyramid.HPExtractionTime;
                                        TotalTime += AdaptiveHistoPyramid.TotalTime;
                                        RunningAvg = TotalTime.Ticks / n;
                                        SumSqrTime += (AdaptiveHistoPyramid.TotalTime.Ticks * AdaptiveHistoPyramid.TotalTime.Ticks);
                                        RunningVariance = (long)((SumSqrTime / (j + 1)) - Math.Pow(RunningAvg, 2));
                                        if (!(MaxTime == TimeSpan.Zero))
                                        {
                                            MaxTime = MaxTime > AdaptiveHistoPyramid.TotalTime ? MaxTime : AdaptiveHistoPyramid.TotalTime;
                                        }
                                        else
                                        {
                                            MaxTime = AdaptiveHistoPyramid.TotalTime;
                                        }

                                        if (!(MinTime == TimeSpan.Zero))
                                        {
                                            MinTime = MinTime < AdaptiveHistoPyramid.TotalTime ? MinTime : AdaptiveHistoPyramid.TotalTime;
                                        }
                                        else
                                        {
                                            MinTime = AdaptiveHistoPyramid.TotalTime;
                                        }
                                        padding += AdaptiveHistoPyramid.padding;
                                        layerCount += AdaptiveHistoPyramid.layerCount;
                                        baseLayerSize += AdaptiveHistoPyramid.baseLayerSize;
                                    }
                                    //Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                                    //Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                                    //Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                                    //Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                                    Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                                    //Console.WriteLine("Variance (clock cycles): " + RunningVariance);
                                    //Console.WriteLine("Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)));
                                    //Console.WriteLine("Max Time: " + MaxTime);
                                    //Console.WriteLine("Min Time: " + MinTime);
                                    //Console.WriteLine("Avg Padding: " + padding / n);
                                    //Console.WriteLine("Avg Layer Count: " + layerCount / n);
                                    //Console.WriteLine("Avg Base Layer Size: " + baseLayerSize / n);
                                    File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                                    File.AppendAllLines("Result.txt", new string[]{ extend ? "Extended" : "Not Extended"
                                ,reverse ? "Reversed" : "Not Reversed"
                                ,byDim ? "Divided by Dimension" : "Flat Division"});
                                    File.AppendAllLines("Result.txt", new string[] { $"AHP"
                            , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                            , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                            , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                            , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                            , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                            , "Variance: " + "Variance (clock cycles): " + RunningVariance
                            , "Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance))
                            , "Max Time: " + MaxTime
                            , "Min Time: " + MinTime
                            , "Avg Padding: " + padding / n
                            , "Avg Layer Count: " + layerCount / n
                            , "Avg Base Layer Size: " + baseLayerSize / n});

                                    // write to excel
                                    string columnLetter = ColumnIndexToColumnLetter(columnIndex);
                                    sheet.Cells[columnLetter + "2"].Value = TimeSpan.FromTicks(FirstPassTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "3"].Value = TimeSpan.FromTicks(HPCreateTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "4"].Value = TimeSpan.FromTicks(HPTraverseTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "5"].Value = TimeSpan.FromTicks(HPExtractionTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "6"].Value = TimeSpan.FromTicks(TotalTime.Ticks / n).TotalSeconds;
                                    sheet.Cells[columnLetter + "7"].Value = MinTime.TotalSeconds;
                                    sheet.Cells[columnLetter + "8"].Value = MaxTime.TotalSeconds;
                                    sheet.Cells[columnLetter + "9"].Value = TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)).TotalSeconds;
                                    sheet.Cells[columnLetter + "10"].Value = padding / n;
                                    sheet.Cells[columnLetter + "11"].Value = layerCount / n;
                                    sheet.Cells[columnLetter + "12"].Value = baseLayerSize;

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Exception caught at " + DateTime.Now.ToString());
                                    Console.WriteLine(ex.Message);
                                    Console.Write(ex.StackTrace);
                                    Console.WriteLine();
                                }

                                FirstPassTime = TimeSpan.Zero;
                                HPCreateTime = TimeSpan.Zero;
                                HPTraverseTime = TimeSpan.Zero;
                                HPExtractionTime = TimeSpan.Zero;
                                TotalTime = TimeSpan.Zero;
                                SumSqrTime = 0;
                                RunningAvg = 0;
                                RunningVariance = 0;
                                MaxTime = TimeSpan.Zero;
                                MinTime = TimeSpan.Zero;
                                padding = 0;
                                layerCount = 0;
                                baseLayerSize = 0;
                                columnIndex++;
                            }

                            Thread.Sleep(1000);
                            for (int p = 0; p < 4; p += 2)
                            {
                                Thread.Sleep(1000);
                                //bool extend = p % 2 == 0;
                                bool extend = true;
                                bool reverse = (p >> 1) % 2 == 0;
                                bool randSel = (p >> 2) % 2 == 0;
                                {
                                    for (int i = 5; i < 21; i++)
                                    {
                                        Thread.Sleep(1000);
                                        int s = i;
                                        try
                                        {
                                            for (int j = 0; j < n; j++)
                                            {
                                                MarchingCubes.SetValues(ds, $"AHP_Improved_Fixed_Init_{s}");
                                                if (true)
                                                {
                                                    Console.WriteLine("Run: " + j);
                                                    sheet.Cells[ColumnIndexToColumnLetter(columnIndex) + "1"].Value = $"FixAHP_{s}" +
                                                        (extend ? "_Ext" : "_NotExt")
                                                        + (reverse ? "_Rev" : "_NotRev")
                                                        + (randSel ? "_Rand" : "_MinMax");
                                                    AdaptiveHistoPyramidImproved run10 = new AdaptiveHistoPyramidImproved(s, extend: extend, reverse: reverse, randomSel: randSel, randomInit: false, factorInit: i);
                                                    Thread.Sleep(50);
                                                    Console.WriteLine("--------------------------------------------------------------------------------");
                                                }
                                                FirstPassTime += AdaptiveHistoPyramidImproved.FirstPassTime;
                                                HPCreateTime += AdaptiveHistoPyramidImproved.HPCreateTime;
                                                HPTraverseTime += AdaptiveHistoPyramidImproved.HPTraverseTime;
                                                HPExtractionTime += AdaptiveHistoPyramidImproved.HPExtractionTime;
                                                TotalTime += AdaptiveHistoPyramidImproved.TotalTime;
                                                RunningAvg = TotalTime.Ticks / n;
                                                SumSqrTime += (AdaptiveHistoPyramidImproved.TotalTime.Ticks * AdaptiveHistoPyramidImproved.TotalTime.Ticks);
                                                RunningVariance = (long)((SumSqrTime / (j + 1)) - Math.Pow(RunningAvg, 2));
                                                if (!(MaxTime == TimeSpan.Zero))
                                                {
                                                    MaxTime = MaxTime > AdaptiveHistoPyramidImproved.TotalTime ? MaxTime : AdaptiveHistoPyramidImproved.TotalTime;
                                                }
                                                else
                                                {
                                                    MaxTime = AdaptiveHistoPyramidImproved.TotalTime;
                                                }

                                                if (!(MinTime == TimeSpan.Zero))
                                                {
                                                    MinTime = MinTime < AdaptiveHistoPyramidImproved.TotalTime ? MinTime : AdaptiveHistoPyramidImproved.TotalTime;
                                                }
                                                else
                                                {
                                                    MinTime = AdaptiveHistoPyramidImproved.TotalTime;
                                                }
                                                padding += AdaptiveHistoPyramidImproved.padding;
                                                layerCount += AdaptiveHistoPyramidImproved.layerCount;
                                                baseLayerSize += AdaptiveHistoPyramidImproved.baseLayerSize;
                                                volume = AdaptiveHistoPyramidImproved.volume;
                                                active_nh = AdaptiveHistoPyramidImproved.active_nh;
                                                n_triangles = AdaptiveHistoPyramidImproved.n_triangle;
                                            }
                                            //Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                                            //Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                                            //Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                                            //Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                                            Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                                            //Console.WriteLine("Variance (clock cycles): " + RunningVariance);
                                            //Console.WriteLine("Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)));
                                            //Console.WriteLine("Max Time: " + MaxTime);
                                            //Console.WriteLine("Min Time: " + MinTime);
                                            //Console.WriteLine("Avg Padding: " + padding / n);
                                            //Console.WriteLine("Avg Layer Count: " + layerCount / n);
                                            //Console.WriteLine("Avg Base Layer Size: " + baseLayerSize / n);
                                            File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                                            File.AppendAllLines("Result.txt", new string[] { $"AHP_Improved_Fixed_Init_{s}"
                                , extend ? "Extended" : "Not Extended"
                                , reverse ? "Reversed" : "Not Reversed"
                                , randSel ? "Randomly Selection" : "MinMax Selection"
                                , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                                , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                                , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                                , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                                , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                                , "Variance: " + "Variance (clock cycles): " + RunningVariance
                                , "Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance))
                                , "Max Time: " + MaxTime
                                , "Min Time: " + MinTime
                                , "Avg Padding: " + padding / n
                                , "Avg Layer Count: " + layerCount / n
                                , "Avg Base Layer Size: " + baseLayerSize / n
                                , "Total # NH's: " + volume
                                , "Active NH's: " + active_nh
                                , "NH Density: " + active_nh / (float)volume * 100 + "%"
                                , "Total # Triangles: " + n_triangles
                                , "Triangle Density: " + ((n_triangles / ((float)volume * 5)) * 100) + "%"
                                , "Average Reduction Factor: " + AdaptiveHistoPyramidImproved.avg_layer_size
                                , "Range Reduction Factor: " + AdaptiveHistoPyramidImproved.range_layer_size
                                    });

                                            // write to excel
                                            string columnLetter = ColumnIndexToColumnLetter(columnIndex);
                                            sheet.Cells[columnLetter + "2"].Value = TimeSpan.FromTicks(FirstPassTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "3"].Value = TimeSpan.FromTicks(HPCreateTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "4"].Value = TimeSpan.FromTicks(HPTraverseTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "5"].Value = TimeSpan.FromTicks(HPExtractionTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "6"].Value = TimeSpan.FromTicks(TotalTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "7"].Value = MinTime.TotalSeconds;
                                            sheet.Cells[columnLetter + "8"].Value = MaxTime.TotalSeconds;
                                            sheet.Cells[columnLetter + "9"].Value = TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)).TotalSeconds;
                                            sheet.Cells[columnLetter + "10"].Value = padding / n;
                                            sheet.Cells[columnLetter + "11"].Value = layerCount / n;
                                            sheet.Cells[columnLetter + "12"].Value = baseLayerSize;
                                            sheet.Cells[columnLetter + "13"].Value = volume;
                                            sheet.Cells[columnLetter + "14"].Value = active_nh;
                                            sheet.Cells[columnLetter + "15"].Value = active_nh / (float)volume * 100 + "%";
                                            sheet.Cells[columnLetter + "16"].Value = n_triangles;
                                            sheet.Cells[columnLetter + "17"].Value = ((n_triangles / ((float)volume * 5)) * 100) + "%";
                                            sheet.Cells[columnLetter + "17"].Value = AdaptiveHistoPyramidImproved.avg_layer_size;
                                            sheet.Cells[columnLetter + "17"].Value = AdaptiveHistoPyramidImproved.range_layer_size;

                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Exception caught at " + DateTime.Now.ToString());
                                            Console.WriteLine(ex.Message);
                                            Console.Write(ex.StackTrace);
                                            Console.WriteLine();
                                        }

                                        FirstPassTime = TimeSpan.Zero;
                                        HPCreateTime = TimeSpan.Zero;
                                        HPTraverseTime = TimeSpan.Zero;
                                        HPExtractionTime = TimeSpan.Zero;
                                        TotalTime = TimeSpan.Zero;
                                        SumSqrTime = 0;
                                        RunningAvg = 0;
                                        RunningVariance = 0;
                                        MaxTime = TimeSpan.Zero;
                                        MinTime = TimeSpan.Zero;
                                        padding = 0;
                                        layerCount = 0;
                                        baseLayerSize = 0;
                                        columnIndex++;
                                    }
                                }
                            }

                            Console.WriteLine("--------------------------------------------------------------------------------");

                            Thread.Sleep(1000);
                            for (int p = 0; p < 4; p += 2)
                            {
                                Thread.Sleep(1000);
                                //bool extend = p % 2 == 0;
                                bool extend = true;
                                bool reverse = (p >> 1) % 2 == 0;
                                bool randSel = (p >> 2) % 2 == 0;
                                {
                                    for (int i = 0; i < 1; i++)
                                    {
                                        Thread.Sleep(1000);
                                        try
                                        {
                                            int s = i;
                                            for (int j = 0; j < n; j++)
                                            {
                                                MarchingCubes.SetValues(ds, $"AHP_Improved_Random_Init");
                                                if (true)
                                                {
                                                    Console.WriteLine("Run: " + j);
                                                    sheet.Cells[ColumnIndexToColumnLetter(columnIndex) + "1"].Value = $"Rand_AHP_{s}" +
                                                        (extend ? "_Ext" : "_NotExt")
                                                        + (reverse ? "_Rev" : "_NotRev")
                                                        + (randSel ? "_Rand" : "_MinMax");
                                                    AdaptiveHistoPyramidImproved run11 = new AdaptiveHistoPyramidImproved(s, extend: extend, reverse: reverse, randomSel: randSel);
                                                    Thread.Sleep(50);
                                                    Console.WriteLine("--------------------------------------------------------------------------------");
                                                }
                                                FirstPassTime += AdaptiveHistoPyramidImproved.FirstPassTime;
                                                HPCreateTime += AdaptiveHistoPyramidImproved.HPCreateTime;
                                                HPTraverseTime += AdaptiveHistoPyramidImproved.HPTraverseTime;
                                                HPExtractionTime += AdaptiveHistoPyramidImproved.HPExtractionTime;
                                                TotalTime += AdaptiveHistoPyramidImproved.TotalTime;
                                                RunningAvg = TotalTime.Ticks / n;
                                                SumSqrTime += (AdaptiveHistoPyramidImproved.TotalTime.Ticks * AdaptiveHistoPyramidImproved.TotalTime.Ticks);
                                                RunningVariance = (long)((SumSqrTime / (j + 1)) - Math.Pow(RunningAvg, 2));
                                                if (!(MaxTime == TimeSpan.Zero))
                                                {
                                                    MaxTime = MaxTime > AdaptiveHistoPyramidImproved.TotalTime ? MaxTime : AdaptiveHistoPyramidImproved.TotalTime;
                                                }
                                                else
                                                {
                                                    MaxTime = AdaptiveHistoPyramidImproved.TotalTime;
                                                }

                                                if (!(MinTime == TimeSpan.Zero))
                                                {
                                                    MinTime = MinTime < AdaptiveHistoPyramidImproved.TotalTime ? MinTime : AdaptiveHistoPyramidImproved.TotalTime;
                                                }
                                                else
                                                {
                                                    MinTime = AdaptiveHistoPyramidImproved.TotalTime;
                                                }
                                                padding += AdaptiveHistoPyramidImproved.padding;
                                                layerCount += AdaptiveHistoPyramidImproved.layerCount;
                                                baseLayerSize += AdaptiveHistoPyramidImproved.baseLayerSize;

                                            }
                                            //Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                                            //Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                                            //Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                                            //Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                                            Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                                            //Console.WriteLine("Variance (clock cycles): " + RunningVariance);
                                            //Console.WriteLine("Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)));
                                            //Console.WriteLine("Max Time: " + MaxTime);
                                            //Console.WriteLine("Min Time: " + MinTime);
                                            //Console.WriteLine("Avg Padding: " + padding / n);
                                            //Console.WriteLine("Avg Layer Count: " + layerCount / n);
                                            //Console.WriteLine("Avg Base Layer Size: " + baseLayerSize / n);
                                            File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                                            File.AppendAllLines("Result.txt", new string[] { $"AHP_Improved_Random_Init"
                                    , extend ? "Extended" : "Not Extended"
                                    , reverse ? "Reversed" : "Not Reversed"
                                    , randSel ? "Randomly Selection" : "MinMax Selection"
                                    , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                                    , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                                    , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                                    , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                                    , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                                    , "Variance: " + "Variance (clock cycles): " + RunningVariance
                                    , "Std Dev: " + TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance))
                                    , "Max Time: " + MaxTime
                                    , "Min Time: " + MinTime
                                    , "Avg Padding: " + padding / n
                                    , "Avg Layer Count: " + layerCount / n
                                    , "Avg Base Layer Size: " + baseLayerSize / n
                                    , "Total # NH's: " + volume
                                    , "Active NH's: " + active_nh
                                    , "NH Density: " + active_nh / (float)volume * 100 + "%"
                                    , "Total # Triangles: " + n_triangles
                                    , "Triangle Density: " + ((n_triangles / ((float)volume * 5)) * 100) + "%"
                                    , "Average Reduction Factor: " + AdaptiveHistoPyramidImproved.avg_layer_size
                                    , "Range Reduction Factor: " + AdaptiveHistoPyramidImproved.range_layer_size
                                        });

                                            // write to excel
                                            string columnLetter = ColumnIndexToColumnLetter(columnIndex);
                                            sheet.Cells[columnLetter + "2"].Value = TimeSpan.FromTicks(FirstPassTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "3"].Value = TimeSpan.FromTicks(HPCreateTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "4"].Value = TimeSpan.FromTicks(HPTraverseTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "5"].Value = TimeSpan.FromTicks(HPExtractionTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "6"].Value = TimeSpan.FromTicks(TotalTime.Ticks / n).TotalSeconds;
                                            sheet.Cells[columnLetter + "7"].Value = MinTime.TotalSeconds;
                                            sheet.Cells[columnLetter + "8"].Value = MaxTime.TotalSeconds;
                                            sheet.Cells[columnLetter + "9"].Value = TimeSpan.FromTicks((long)Math.Sqrt(RunningVariance)).TotalSeconds;
                                            sheet.Cells[columnLetter + "10"].Value = padding / n;
                                            sheet.Cells[columnLetter + "11"].Value = layerCount / n;
                                            sheet.Cells[columnLetter + "12"].Value = baseLayerSize;
                                            sheet.Cells[columnLetter + "13"].Value = volume;
                                            sheet.Cells[columnLetter + "14"].Value = active_nh;
                                            sheet.Cells[columnLetter + "15"].Value = active_nh / (float)volume * 100 + "%";
                                            sheet.Cells[columnLetter + "16"].Value = n_triangles;
                                            sheet.Cells[columnLetter + "17"].Value = ((n_triangles / ((float)volume * 5)) * 100) + "%";
                                            sheet.Cells[columnLetter + "17"].Value = AdaptiveHistoPyramidImproved.avg_layer_size;
                                            sheet.Cells[columnLetter + "17"].Value = AdaptiveHistoPyramidImproved.range_layer_size;


                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Exception caught at " + DateTime.Now.ToString());
                                            Console.WriteLine(ex.Message);
                                            Console.Write(ex.StackTrace);
                                            Console.WriteLine();
                                        }

                                        FirstPassTime = TimeSpan.Zero;
                                        HPCreateTime = TimeSpan.Zero;
                                        HPTraverseTime = TimeSpan.Zero;
                                        HPExtractionTime = TimeSpan.Zero;
                                        TotalTime = TimeSpan.Zero;
                                        SumSqrTime = 0;
                                        RunningAvg = 0;
                                        RunningVariance = 0;
                                        MaxTime = TimeSpan.Zero;
                                        MinTime = TimeSpan.Zero;
                                        padding = 0;
                                        layerCount = 0;
                                        baseLayerSize = 0;
                                        columnIndex++;
                                    }
                                }
                            }
                            Console.WriteLine("--------------------------------------------------------------------------------");

                            MarchingCubes.slices = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception caught at " + DateTime.Now.ToString());
                        Console.WriteLine(ex.Message);
                        Console.Write(ex.StackTrace);
                        Console.WriteLine();
                    }
                    package.Save();
                    File.AppendAllLines("Result.txt", new string[] { "Run ended at: " + DateTime.Now.ToString() + ", N = " + n });
                    File.AppendAllLines("Result.txt", new string[] { "" });
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    Console.Out.WriteLine("DONE");
                }
            }
        }

        static string ColumnIndexToColumnLetter(int colIndex)
        {
            int div = colIndex;
            string colLetter = String.Empty;
            int mod = 0;

            while (div > 0)
            {
                mod = (div - 1) % 26;
                colLetter = (char)(65 + mod) + colLetter;
                div = (int)((div - mod) / 26);
            }
            return colLetter;
        }
    }

}
