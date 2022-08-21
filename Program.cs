using System;
using System.Collections.Generic;
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
                MarchingCubes.SetValues(ds, "CPU");
                NaiveCPU run1 = new NaiveCPU(25);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "GPU");
                NaiveGPU run2 = new NaiveGPU(25);
                Console.WriteLine("--------------------------------------------------------------------------------");
                for (int i = 2; i < 9; i++)
                {
                    if (i != 6)
                    {
                        MarchingCubes.SetValues(ds, $"HP_{i}");
                        HistoPyramidGeneric run3 = new HistoPyramidGeneric(i);
                        Thread.Sleep(1000);
                        Console.WriteLine("--------------------------------------------------------------------------------");
                    }
                }

                MarchingCubes.SetValues(ds, "HP");
                HistoPyramid run4 = new HistoPyramid(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "HP3D");
                HistoPyramid3D run5 = new HistoPyramid3D(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "Octree");
                Octree run6 = new Octree(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "OctreewPrior");
                OctreeWPrior run7 = new OctreeWPrior(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "OctreeBONO");
                OctreeBONOwPrior run8 = new OctreeBONOwPrior(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "AdaptiveHP");
                AdaptiveHistoPyramid run9 = new AdaptiveHistoPyramid(0);
                Thread.Sleep(1000);
                Console.WriteLine("------------------------------------------------------------------------------");
                MarchingCubes.slices = null;
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Console.Out.WriteLine("DONE");
        }
    }
}
