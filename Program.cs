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
            for (int i = 25; i < 26; i += 25)
            {
                Console.WriteLine(i);
                //NaiveCPU run1 = new NaiveCPU(i);
                //NaiveGPU run2 = new NaiveGPU(i);
                //HistoPyramid run5 = new HistoPyramid(i);
                //HistoPyramid3D run6 = new HistoPyramid3D(i);
                //Octree run7 = new Octree(i);
                //OctreeEvenSubdiv run8 = new OctreeEvenSubdiv(i);
                OctreeWPrior run9 = new OctreeWPrior(i);
                //OctreeBONOwPrior run10 = new OctreeBONOwPrior(i);
            }
            while (true) 
                ;
            Console.Out.WriteLine("DONE");
        }
    }
}
