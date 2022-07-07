using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                //dataset.F_Hip,
                //dataset.F_Knee,
                dataset.F_Pelvis,
                dataset.F_Shoulder,
                //dataset.M_Head,
                //dataset.M_Hip,
                //dataset.M_Pelvis,
                //dataset.M_Shoulder,
                dataset.MRbrain,
                dataset.ChestSmall,
                dataset.ChestCT,
                dataset.WristCT
            };
            foreach (dataset ds in dList)
            {
                //Console.WriteLine(i);
                //NaiveCPU run1 = new NaiveCPU(i);
                //NaiveGPU run2 = new NaiveGPU(i);
                //HistoPyramidGeneric run11 = new HistoPyramidGeneric(i);
                //OctreeEvenSubdiv run8 = new OctreeEvenSubdiv(i);


                MarchingCubes.SetValues(ds, "HP");
                HistoPyramid run5 = new HistoPyramid(0);
                MarchingCubes.SetValues(ds, "HP3D");
                HistoPyramid3D run6 = new HistoPyramid3D(0);
                MarchingCubes.SetValues(ds, "Octree");
                Octree run7 = new Octree(0);
                MarchingCubes.SetValues(ds, "OctreewPrior");
                OctreeWPrior run9 = new OctreeWPrior(0);
                MarchingCubes.SetValues(ds, "OctreeBONO");
                OctreeBONOwPrior run10 = new OctreeBONOwPrior(0);
                MarchingCubes.SetValues(ds, "AdaptiveHP");
                AdaptiveHistoPyramid run12 = new AdaptiveHistoPyramid(0);
                MarchingCubes.slices = null;
            }
            while (true) 
                ;
            Console.Out.WriteLine("DONE");
        }
    }
}
