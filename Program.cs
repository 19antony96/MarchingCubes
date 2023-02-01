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
            int n = 100;
            File.WriteAllText("Result.txt", "Run at: " + DateTime.Now.ToString() + ", N = " + n);
            File.AppendAllLines("Result.txt", new string[] { "" });
            TimeSpan FirstPassTime = TimeSpan.Zero;
            TimeSpan HPCreateTime = TimeSpan.Zero;
            TimeSpan HPTraverseTime = TimeSpan.Zero;
            TimeSpan HPExtractionTime = TimeSpan.Zero;
            TimeSpan TotalTime = TimeSpan.Zero;
            List<dataset> dList = new List<dataset>()
            {
                //dataset.bunny,
                dataset.CThead,
                //dataset.F_Ankle,
                //dataset.F_Head,
                //dataset.F_Pelvis,
                dataset.MRbrain,
                //dataset.WristCT,
                dataset.MRHead40,
                //dataset.SkullLrg

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
                //Console.WriteLine("--------------------------------------------------------------------------------");
                //for (int i = 1; i < 8; i++)
                //{
                //    int s = (int)Math.Pow(2, i);
                //    for (int j = 0; j < n; j++)
                //    {
                //        MarchingCubes.SetValues(ds, $"HP_{s}");
                //        if (Math.Log(Math.Pow(MarchingCubes.width, 3), s) % 1 == 0)
                //        {                
                //Console.WriteLine("Run: " + j);
                //            HistoPyramidGeneric run3 = new HistoPyramidGeneric(s);
                //            Thread.Sleep(1000);
                //            Console.WriteLine("--------------------------------------------------------------------------------");
                //        }
                //        FirstPassTime += HistoPyramidGeneric.FirstPassTime;
                //        HPCreateTime += HistoPyramidGeneric.HPCreateTime;
                //        HPTraverseTime += HistoPyramidGeneric.HPTraverseTime;
                //        HPExtractionTime += HistoPyramidGeneric.HPExtractionTime;
                //        TotalTime += HistoPyramidGeneric.TotalTime;
                //    }
                //    Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                //    Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                //    Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                //    Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                //    Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                //    File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                //    File.AppendAllLines("Result.txt", new string[] { $"HP_{s}"
                //        , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                //        , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                //        , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                //        , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                //        , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n) });
                //    FirstPassTime = TimeSpan.Zero;
                //    HPCreateTime = TimeSpan.Zero;
                //    HPTraverseTime = TimeSpan.Zero;
                //    HPExtractionTime = TimeSpan.Zero;
                //    TotalTime = TimeSpan.Zero;
                //}


                //MarchingCubes.SetValues(ds, "HP2D");
                //for (int i = 0; i < n; i++)
                //{                
                //Console.WriteLine("Run: " + i);
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
                //Thread.Sleep(1000);
                //Console.WriteLine("--------------------------------------------------------------------------------");

                //MarchingCubes.SetValues(ds, "HP3D");
                //for (int i = 0; i < n; i++)
                //{
                //Console.WriteLine("Run: " + i);
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
                //Thread.Sleep(1000);
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
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    Console.WriteLine("Run: " + i);
                    MarchingCubes.SetValues(ds, "AdaptiveHP");
                    AdaptiveHistoPyramid run9 = new AdaptiveHistoPyramid(0);
                    Thread.Sleep(1000);
                    Console.WriteLine("------------------------------------------------------------------------------");

                    FirstPassTime += AdaptiveHistoPyramid.FirstPassTime;
                    HPCreateTime += AdaptiveHistoPyramid.HPCreateTime;
                    HPTraverseTime += AdaptiveHistoPyramid.HPTraverseTime;
                    HPExtractionTime += AdaptiveHistoPyramid.HPExtractionTime;
                    TotalTime += AdaptiveHistoPyramid.TotalTime;
                }
                Console.WriteLine("First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n));
                Console.WriteLine("HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n));
                Console.WriteLine("HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n));
                Console.WriteLine("HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n));
                Console.WriteLine("Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n));
                File.AppendAllLines("Result.txt", new string[] { MarchingCubes.outFilename });
                File.AppendAllLines("Result.txt", new string[] { $"AHP"
                        , "First Pass Avg Time: " + TimeSpan.FromTicks(FirstPassTime.Ticks / n)
                        , "HP Creation Avg Time: " + TimeSpan.FromTicks(HPCreateTime.Ticks / n)
                        , "HP Traversal Avg Time: " + TimeSpan.FromTicks(HPTraverseTime.Ticks / n)
                        , "HP Extraction Avg Time: " + TimeSpan.FromTicks(HPExtractionTime.Ticks / n)
                        , "Total Avg Time: " + TimeSpan.FromTicks(TotalTime.Ticks / n) });
                FirstPassTime = TimeSpan.Zero;
                HPCreateTime = TimeSpan.Zero;
                HPTraverseTime = TimeSpan.Zero;
                HPExtractionTime = TimeSpan.Zero;
                TotalTime = TimeSpan.Zero;
                MarchingCubes.slices = null;
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Console.Out.WriteLine("DONE");
        }
    }
}
