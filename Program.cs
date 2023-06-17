using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarchingCubes
{
    class Program
    {

        static void Main(string[] args)
        {
            int n = 1;
            File.WriteAllText("Result.txt", "Run started at: " + DateTime.Now.ToString() + ", N = " + n);
            File.AppendAllLines("Result.txt", new string[] { "" });
            TimeSpan FirstPassTime = TimeSpan.Zero;
            TimeSpan HPCreateTime = TimeSpan.Zero;
            TimeSpan HPTraverseTime = TimeSpan.Zero;
            TimeSpan HPExtractionTime = TimeSpan.Zero;
            TimeSpan TotalTime = TimeSpan.Zero;
            TimeSpan RunningAvg = TimeSpan.Zero;
            TimeSpan RunningVariance = TimeSpan.Zero;
            TimeSpan MaxTime = TimeSpan.Zero;
            TimeSpan MinTime = TimeSpan.Zero;
            long padding = 0, layerCount = 0, baseLayerSize = 0, OldAvg;
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
                dataset.SkullLrg

                //dataset.F_Hip,
                //dataset.F_Knee,
                //dataset.F_Shoulder,
                //dataset.M_Head,
                //dataset.M_Hip,
                //dataset.M_Pelvis,
                //dataset.M_Shoulder,
                //dataset.VolFlip3,
                //dataset.VolFlip5,
                //dataset.VolFlip20,
                //dataset.VolFlip30,
                //dataset.ChestSmall,
                //dataset.ChestCT,
            };
            foreach (dataset ds in dList)
            {
                //MarchingCubes.SetValues(ds, "CPU");
                //NaiveCPU run1 = new NaiveCPU(25);
                //Console.WriteLine("--------------------------------------------------------------------------------");
                //MarchingCubes.SetValues(ds, "GPU");
                //NaiveGPU run2 = new NaiveGPU(25);
                Console.WriteLine("--------------------------------------------------------------------------------");
                for (int i = 4; i < 12; i++)
                {
                    int s = i;
                    for (int j = 0; j < n; j++)
                    {
                        MarchingCubes.SetValues(ds, $"HP_{s}");
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
                        OldAvg = (RunningAvg.Ticks * j / n);
                        RunningAvg += TimeSpan.FromTicks(HistoPyramidGeneric.TotalTime.Ticks / n);
                        RunningVariance = TimeSpan.FromTicks((long)((n / (n + 1)) * (RunningVariance.Ticks + ((Math.Pow(OldAvg - HistoPyramidGeneric.TotalTime.Ticks, 2) / n) + 1))));
                        if(!(MaxTime == TimeSpan.Zero))
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
                    Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                    Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                    Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                    Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                    Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                    Console.WriteLine("Variance: " + RunningVariance);
                    Console.WriteLine("Max Time: " + MaxTime);
                    Console.WriteLine("Min Time: " + MinTime);
                    Console.WriteLine("Avg Padding: " + padding / n);
                    Console.WriteLine("Avg Layer Count: " + layerCount / n);
                    Console.WriteLine("Avg Base Layer Size: " + baseLayerSize);
                    File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                    File.AppendAllLines("Result.txt", new string[] { $"HP_{s}"
                        , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                        , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                        , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                        , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                        , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n) 
                        , "Variance: " + RunningVariance
                        , "Max Time: " + MaxTime
                        , "Min Time: " + MinTime
                        , "Avg Padding: " + padding / n
                        , "Avg Layer Count: " + layerCount / n
                        , "Avg Base Layer Size: " + baseLayerSize / n});
                    FirstPassTime = TimeSpan.Zero;
                    HPCreateTime = TimeSpan.Zero;
                    HPTraverseTime = TimeSpan.Zero;
                    HPExtractionTime = TimeSpan.Zero;
                    TotalTime = TimeSpan.Zero;
                    RunningAvg = TimeSpan.Zero;
                    RunningVariance = TimeSpan.Zero;
                    MaxTime = TimeSpan.Zero;
                    MinTime = TimeSpan.Zero;
                    padding = 0;
                    layerCount = 0;
                    baseLayerSize = 0;
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
                for (int j = 0; j < 8; j++)
                {
                    bool extend = j % 2 == 0;
                    bool reverse = (j >> 1) % 2 == 0;
                    bool byDim = (j >> 2) % 2 == 0;
                    for (int i = 0; i < n; i++)
                    {
                        Console.WriteLine("--------------------------------------------------------------------------------");
                        Console.WriteLine("Run: " + i);
                        MarchingCubes.SetValues(ds, "AdaptiveHP");
                        AdaptiveHistoPyramid run9 = new AdaptiveHistoPyramid(0, extend, reverse, byDim);
                        Thread.Sleep(50);
                        Console.WriteLine("------------------------------------------------------------------------------");

                        FirstPassTime += AdaptiveHistoPyramid.FirstPassTime;
                        HPCreateTime += AdaptiveHistoPyramid.HPCreateTime;
                        HPTraverseTime += AdaptiveHistoPyramid.HPTraverseTime;
                        HPExtractionTime += AdaptiveHistoPyramid.HPExtractionTime;
                        TotalTime += AdaptiveHistoPyramid.TotalTime;
                        OldAvg = (RunningAvg.Ticks * j / n);
                        RunningAvg += TimeSpan.FromTicks(HistoPyramidGeneric.TotalTime.Ticks / n);
                        RunningVariance = TimeSpan.FromTicks((long)((n / (n + 1)) * (RunningVariance.Ticks + ((Math.Pow(OldAvg - HistoPyramidGeneric.TotalTime.Ticks, 2) / n) + 1))));
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
                    Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                    Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                    Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                    Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                    Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                    Console.WriteLine("Variance: " + RunningVariance);
                    Console.WriteLine("Max Time: " + MaxTime);
                    Console.WriteLine("Min Time: " + MinTime);
                    Console.WriteLine("Avg Padding: " + padding / n);
                    Console.WriteLine("Avg Layer Count: " + layerCount / n);
                    Console.WriteLine("Avg Base Layer Size: " + baseLayerSize / n);
                    File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });

                    Console.WriteLine("Adaptive HistoPyramid");
                    File.AppendAllLines("Result.txt", new string[]{ extend ? "Extended" : "Not Extended"
                            ,reverse ? "Reversed" : "Not Reversed"
                            ,byDim ? "Divided by Dimension" : "Flat Division"});
                    File.AppendAllLines("Result.txt", new string[] { $"AHP"
                        , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                        , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                        , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                        , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                        , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                        , "Variance: " + RunningVariance
                        , "Max Time: " + MaxTime
                        , "Min Time: " + MinTime
                        , "Avg Padding: " + padding / n
                        , "Avg Layer Count: " + layerCount / n
                        , "Avg Base Layer Size: " + baseLayerSize / n});
                    FirstPassTime = TimeSpan.Zero;
                    HPCreateTime = TimeSpan.Zero;
                    HPTraverseTime = TimeSpan.Zero;
                    HPExtractionTime = TimeSpan.Zero;
                    TotalTime = TimeSpan.Zero;
                    RunningAvg = TimeSpan.Zero;
                    RunningVariance = TimeSpan.Zero;
                    MaxTime = TimeSpan.Zero;
                    MinTime = TimeSpan.Zero;
                    padding = 0;
                    layerCount = 0;
                    baseLayerSize = 0;
                }

                for (int p = 0; p < 4; p++)
                {
                    bool extend = p % 2 == 0;
                    bool reverse = (p >> 1) % 2 == 0;
                    {
                        for (int i = 4; i < 12; i++)
                        {
                            int s = i;
                            for (int j = 0; j < n; j++)
                            {
                                MarchingCubes.SetValues(ds, $"AHP_Improved_Fixed_Init_{s}");
                                if (true)
                                {
                                    Console.WriteLine("Run: " + j);
                                    AdaptiveHistoPyramidImproved run10 = new AdaptiveHistoPyramidImproved(s, extend: extend, reverse: reverse, randomInit:false, factorInit:i);
                                    Thread.Sleep(50);
                                    Console.WriteLine("--------------------------------------------------------------------------------");
                                }
                                FirstPassTime += AdaptiveHistoPyramidImproved.FirstPassTime;
                                HPCreateTime += AdaptiveHistoPyramidImproved.HPCreateTime;
                                HPTraverseTime += AdaptiveHistoPyramidImproved.HPTraverseTime;
                                HPExtractionTime += AdaptiveHistoPyramidImproved.HPExtractionTime;
                                TotalTime += AdaptiveHistoPyramidImproved.TotalTime;
                                OldAvg = (RunningAvg.Ticks * j / n);
                                RunningAvg += TimeSpan.FromTicks(AdaptiveHistoPyramidImproved.TotalTime.Ticks / n);
                                RunningVariance = TimeSpan.FromTicks((long)((n / (n + 1)) * (RunningVariance.Ticks + ((Math.Pow(OldAvg - AdaptiveHistoPyramidImproved.TotalTime.Ticks, 2) / n) + 1))));
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
                            Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                            Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                            Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                            Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                            Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                            Console.WriteLine("Variance: " + RunningVariance);
                            Console.WriteLine("Max Time: " + MaxTime);
                            Console.WriteLine("Min Time: " + MinTime);
                            Console.WriteLine("Avg Padding: " + padding / n);
                            Console.WriteLine("Avg Layer Count: " + layerCount / n);
                            Console.WriteLine("Avg Base Layer Size: " + baseLayerSize / n);
                            File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                            File.AppendAllLines("Result.txt", new string[] { $"AHP_Improved_Fixed_Init_{s}" 
                                , extend ? "Extended" : "Not Extended"
                                , reverse ? "Reversed" : "Not Reversed"
                                , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                                , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                                , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                                , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                                , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                                , "Variance: " + RunningVariance
                                , "Max Time: " + MaxTime
                                , "Min Time: " + MinTime
                                , "Avg Padding: " + padding / n
                                , "Avg Layer Count: " + layerCount / n
                                , "Avg Base Layer Size: " + baseLayerSize / n});
                            FirstPassTime = TimeSpan.Zero;
                            HPCreateTime = TimeSpan.Zero;
                            HPTraverseTime = TimeSpan.Zero;
                            HPExtractionTime = TimeSpan.Zero;
                            TotalTime = TimeSpan.Zero;
                            RunningAvg = TimeSpan.Zero;
                            RunningVariance = TimeSpan.Zero;
                            MaxTime = TimeSpan.Zero;
                            MinTime = TimeSpan.Zero;
                            padding = 0;
                            layerCount = 0;
                            baseLayerSize = 0;
                        }
                    }
                }

                Console.WriteLine("--------------------------------------------------------------------------------");

                for (int p = 0; p < 4; p++)
                {
                    bool extend = p % 2 == 0;
                    bool reverse = (p >> 1) % 2 == 0;
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            int s = i;
                            for (int j = 0; j < n; j++)
                            {
                                MarchingCubes.SetValues(ds, $"AHP_Improved_Random_Init");
                                if (true)
                                {
                                    Console.WriteLine("Run: " + j);
                                    AdaptiveHistoPyramidImproved run11 = new AdaptiveHistoPyramidImproved(s, extend:extend, reverse:reverse);
                                    Thread.Sleep(50);
                                    Console.WriteLine("--------------------------------------------------------------------------------");
                                }
                                FirstPassTime += AdaptiveHistoPyramidImproved.FirstPassTime;
                                HPCreateTime += AdaptiveHistoPyramidImproved.HPCreateTime;
                                HPTraverseTime += AdaptiveHistoPyramidImproved.HPTraverseTime;
                                HPExtractionTime += AdaptiveHistoPyramidImproved.HPExtractionTime;
                                TotalTime += AdaptiveHistoPyramidImproved.TotalTime;
                                OldAvg = (RunningAvg.Ticks * j / n);
                                RunningAvg += TimeSpan.FromTicks(AdaptiveHistoPyramidImproved.TotalTime.Ticks / n);
                                RunningVariance = TimeSpan.FromTicks((long)((n / (n + 1)) * (RunningVariance.Ticks + ((Math.Pow(OldAvg - AdaptiveHistoPyramidImproved.TotalTime.Ticks, 2) / n) + 1))));
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
                            Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                            Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                            Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                            Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                            Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                            Console.WriteLine("Variance: " + RunningVariance);
                            Console.WriteLine("Max Time: " + MaxTime);
                            Console.WriteLine("Min Time: " + MinTime);
                            Console.WriteLine("Avg Padding: " + padding / n);
                            Console.WriteLine("Avg Layer Count: " + layerCount / n);
                            Console.WriteLine("Avg Base Layer Size: " + baseLayerSize / n);
                            File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                            File.AppendAllLines("Result.txt", new string[] { $"AHP_Improved_Random_Init"
                                , extend ? "Extended" : "Not Extended"
                                , reverse ? "Reversed" : "Not Reversed"
                                , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                                , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                                , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                                , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                                , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n)
                                , "Variance: " + RunningVariance
                                , "Max Time: " + MaxTime
                                , "Min Time: " + MinTime
                                , "Avg Padding: " + padding / n
                                , "Avg Layer Count: " + layerCount / n
                                , "Avg Base Layer Size: " + baseLayerSize / n});
                            FirstPassTime = TimeSpan.Zero;
                            HPCreateTime = TimeSpan.Zero;
                            HPTraverseTime = TimeSpan.Zero;
                            HPExtractionTime = TimeSpan.Zero;
                            TotalTime = TimeSpan.Zero;
                            RunningAvg = TimeSpan.Zero;
                            RunningVariance = TimeSpan.Zero;
                            MaxTime = TimeSpan.Zero;
                            MinTime = TimeSpan.Zero;
                            padding = 0;
                            layerCount = 0;
                            baseLayerSize = 0;
                        }
                    }
                }
                Console.WriteLine("--------------------------------------------------------------------------------");


                MarchingCubes.slices = null;
            }
            File.AppendAllLines("Result.txt", new string[]{ "Run ended at: " + DateTime.Now.ToString() + ", N = " + n});
            File.AppendAllLines("Result.txt", new string[] { "" });
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Console.Out.WriteLine("DONE");
        }
    }
}
