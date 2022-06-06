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
                Octree run1 = new Octree(i);
            }
            Console.Out.WriteLine("DONE");
        }
    }
}
