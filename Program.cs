using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispDICOMCMD;

namespace MarchingCubes		
{
    class Program
    {

        static void Main(string[] args)
        {
            for (int i = 10; i < 11; i += 25)
            {
                Console.WriteLine(i);
                DispDICOMCMD.DispDICOMCMD run1 = new DispDICOMCMD.DispDICOMCMD(i);
            }
            Console.Out.WriteLine("DONE");
        }
    }
}
