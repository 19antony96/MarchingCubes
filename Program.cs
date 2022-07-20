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
                dataset.F_Shoulder,
                dataset.MRbrain,
                dataset.ChestSmall,
                dataset.ChestCT,
                dataset.WristCT,
                dataset.MRHead40,
                dataset.SkullLrg

                //dataset.F_Hip,
                //dataset.F_Knee,
                //dataset.M_Head,
                //dataset.M_Hip,
                //dataset.M_Pelvis,
                //dataset.M_Shoulder,
                //dataset.VolFlip3,
                //dataset.VolFlip5,
                //dataset.VolFlip20,
                //dataset.VolFlip30,
            };
            foreach (dataset ds in dList)
            {
                //Console.WriteLine(i);
                //NaiveCPU run1 = new NaiveCPU(i);
                //NaiveGPU run2 = new NaiveGPU(i);
                //HistoPyramidGeneric run11 = new HistoPyramidGeneric(i);
                //OctreeEvenSubdiv run8 = new OctreeEvenSubdiv(i);
                for (int i = 2; i < 9; i++)
                {
                    if (i != 6)
                    {
                        MarchingCubes.SetValues(ds, $"HP_{i}");
                        HistoPyramidGeneric run11 = new HistoPyramidGeneric(i);
                        Thread.Sleep(1000);
                        Console.WriteLine("--------------------------------------------------------------------------------");
                    }
                }

                MarchingCubes.SetValues(ds, "HP");
                HistoPyramid run5 = new HistoPyramid(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "HP3D");
                HistoPyramid3D run6 = new HistoPyramid3D(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "Octree");
                Octree run7 = new Octree(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "OctreewPrior");
                OctreeWPrior run9 = new OctreeWPrior(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "OctreeBONO");
                OctreeBONOwPrior run10 = new OctreeBONOwPrior(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.SetValues(ds, "AdaptiveHP");
                AdaptiveHistoPyramid run12 = new AdaptiveHistoPyramid(0);
                Thread.Sleep(1000);
                Console.WriteLine("--------------------------------------------------------------------------------");
                MarchingCubes.slices = null;
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Console.Out.WriteLine("DONE");
        }
    }
}
