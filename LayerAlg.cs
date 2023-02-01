using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarchingCubes
{
    public static class LayerAlg
    {
        //public static List<int> factorOpt = new List<int> { 5, 6, 7, 8, 4, 2, 3 };
        public static List<int> factorOpt = new List<int> { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        //public static List<int> factorOpt = new List<int> { 23, 17, 13, 11, 8, 7, 6, 5, 4, 3, 2 };

        public static List<int> SimpleStochastic(long HPsize)
        {
            List<int> factors = new List<int>();
            long LayerSize = HPsize;
            for (int nLayers = 0; LayerSize > 1; nLayers++)
            {
                int temp = factorOpt.First();
                foreach (int factor in factorOpt)
                {
                    if (LayerSize % factor < LayerSize % temp)
                        temp = factor;
                }
                //Console.WriteLine(temp);
                factors.Add(temp);
                LayerSize = (int)Math.Ceiling((double)LayerSize / (double)temp);
            }
            return factors;
        }

        public static List<int> Stochastic(long HPsize)
        {
            List<int> factors = new List<int>();
            long LayerSize = HPsize;
            for (int nLayers = 0; LayerSize > 1; nLayers++)
            {
                int temp = 8;
                foreach (int factor in factorOpt)
                {
                    if (LayerSize % factor < LayerSize % temp)
                        temp = factor;
                }
                //Console.WriteLine(temp);
                factors.Add(temp);
                LayerSize = (int)Math.Ceiling((double)LayerSize / (double)temp);
            }
            return factors;
        }
    }
}
