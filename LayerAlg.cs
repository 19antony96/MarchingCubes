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
        //public static List<int> stdFactorOpt = new List<int> { 4, 8, 6, 7, 5, 3, 2 };
        //public static List<int> stdFactorOpt = new List<int> { 5, 6, 7, 8, 4 };
        public static List<int> xtdFactorOpt = new List<int> { 16, 15, 14, 13, 12, 11, 10, 9, 8 };
        public static List<int> stdFactorOpt = new List<int> { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5 };
        //public static List<int> xtdFactorOpt = new List<int> { 2, 3, 5, 6, 8, 7, 6, 5, 4, 3, 2 };
        //public static List<int> factorOpt = new List<int> { 23, 17, 13, 11, 8, 7, 6, 5, 4, 3, 2 };

        public static List<int> SimpleStochastic(long HPsize, bool extend)
        {
            stdFactorOpt.Reverse();
            List<int> factors = new List<int>();
            List<int> factorOpt = extend ? xtdFactorOpt : stdFactorOpt;
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

        public static List<int> ByDim(int height, int width, int depth, bool extend)
        {
            List<int> factorOpt = extend ? xtdFactorOpt : stdFactorOpt;
            List<int> hfactors = new List<int>();
            long hLayerSize = height;
            for (int nLayers = 0; hLayerSize > 1; nLayers++)
            {
                int temp = factorOpt.First();
                foreach (int factor in factorOpt)
                {
                    if (hLayerSize % factor < hLayerSize % temp)
                        temp = factor;
                }
                //Console.WriteLine(temp);
                hfactors.Add(temp);
                hLayerSize = (int)Math.Ceiling((double)hLayerSize / (double)temp);
            }
            List<int> wfactors = new List<int>();
            long wLayerSize = width;
            for (int nLayers = 0; wLayerSize > 1; nLayers++)
            {
                int temp = factorOpt.First();
                foreach (int factor in factorOpt)
                {
                    if (wLayerSize % factor < wLayerSize % temp)
                        temp = factor;
                }
                //Console.WriteLine(temp);
                wfactors.Add(temp);
                wLayerSize = (int)Math.Ceiling((double)wLayerSize / (double)temp);
            }
            List<int> dfactors = new List<int>();
            long dLayerSize = depth;
            for (int nLayers = 0; dLayerSize > 1; nLayers++)
            {
                int temp = factorOpt.First();
                foreach (int factor in factorOpt)
                {
                    if (dLayerSize % factor < dLayerSize % temp)
                        temp = factor;
                }
                //Console.WriteLine(temp);
                dfactors.Add(temp);
                dLayerSize = (int)Math.Ceiling((double)dLayerSize / (double)temp);
            }
            List<int> factors = new List<int>();
            factors.AddRange(hfactors);
            factors.AddRange(wfactors);
            factors.AddRange(dfactors);
            return factors;
        }
        public static List<int> RandomRefined(long HPsize, bool extend)
        {
            Random random = new Random();
            stdFactorOpt.Reverse();
            List<int> factors = new List<int>();
            List<int> factorOpt = extend ? xtdFactorOpt : stdFactorOpt;
            long LayerSize = HPsize;
            for (int nLayers = 0; LayerSize > 1; nLayers++)
            {
                int index = random.Next(0, factorOpt.Count());
                int temp = factorOpt[index];
                //Console.WriteLine(temp);
                factors.Add(temp);
                LayerSize = (int)Math.Ceiling((double)LayerSize / (double)temp);
            }

            int maxValue, maxIndex, minValue, minIndex;
            int padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
            int tol = (int)(HPsize / 100);
            int count = 0;
            while (padding < 0 || (padding > tol && count < 100))
            {
                if(padding > 0)
                {
                    minValue = factors.Where(x => x > factorOpt.Min()).Min();
                    minIndex = factors.ToList().IndexOf(minValue);
                    factors[minIndex]--;
                }
                else
                {
                    maxValue = factors.Where(x => x < factorOpt.Max()).Max();
                    maxIndex = factors.ToList().IndexOf(maxValue);
                    factors[maxIndex]++;
                }
                padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
                count++;
            }
            Console.WriteLine("Process required " + count + "Iterations");
            return factors;
        }

        public static List<int> TrueRandom(long HPsize, bool extend)
        {
            Random random = new Random();
            stdFactorOpt.Reverse();
            List<int> factors = new List<int>();
            List<int> factorOpt = extend ? xtdFactorOpt : stdFactorOpt;
            long LayerSize = HPsize;
            for (int nLayers = 0; LayerSize > 1; nLayers++)
            {
                int index = random.Next(0, factorOpt.Count());
                int temp = factorOpt[index];
                //Console.WriteLine(temp);
                factors.Add(temp);
                LayerSize = (int)Math.Ceiling((double)LayerSize / (double)temp);
            }

            int randIndex;
            int padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
            int tol = (int)(HPsize / 100);
            int count = 0;
            while (padding < 0 || (padding > tol && count < 100))
            {
                randIndex = random.Next(0, factors.Count());
                if (padding > 0)
                {
                    factors[randIndex]--;
                }
                else
                {
                    factors[randIndex]++;
                }
                padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
                count++;
            }
            Console.WriteLine("Process required " + count + "Iterations");
            return factors;
        }

        public static List<int> FixedRefined(long HPsize, int factor, bool extend)
        {
            Random random = new Random();
            stdFactorOpt.Reverse();
            List<int> factors = new List<int>();
            List<int> factorOpt = extend ? xtdFactorOpt : stdFactorOpt;
            long LayerSize = HPsize;
            for (int nLayers = 0; LayerSize > 1; nLayers++)
            {
                //Console.WriteLine(temp);
                factors.Add(factor);
                LayerSize = (int)Math.Ceiling((double)LayerSize / (double)factor);
            }

            int maxValue, maxIndex, minValue, minIndex;
            int padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
            int tol = (int)(HPsize / 100);
            int count = 0;
            while (padding < 0 || (padding > tol && count < 100))
            {
                if (padding > 0)
                {
                    minValue = factors.Where(x => x > factor - (factor / 2)).Min();
                    minIndex = factors.ToList().IndexOf(minValue);
                    factors[minIndex]--;
                }
                else
                {
                    maxValue = factors.Where(x => x < factor + (factor / 2)).Max();
                    maxIndex = factors.ToList().IndexOf(maxValue);
                    factors[maxIndex]++;
                }

                padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
                count++;
            }
            Console.WriteLine("Process required " + count + "Iterations");
            return factors;
        }

        public static List<int> FixedRandomSelection(long HPsize, int factor, bool extend)
        {
            Random random = new Random();
            stdFactorOpt.Reverse();
            List<int> factors = new List<int>();
            List<int> factorOpt = extend ? xtdFactorOpt : stdFactorOpt;
            long LayerSize = HPsize;
            for (int nLayers = 0; LayerSize > 1; nLayers++)
            {
                //Console.WriteLine(temp);
                factors.Add(factor);
                LayerSize = (int)Math.Ceiling((double)LayerSize / (double)factor);
            }

            int randIndex;
            int padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
            int tol = (int)(HPsize / 100);
            int count = 0;
            while (padding < 0 || (padding > tol && count < 100))
            {
                randIndex = random.Next(0, factors.Count());
                if (padding > 0)
                {
                    factors[randIndex]--;
                }
                else
                {
                    factors[randIndex]++;
                }
                padding = (int)(factors.Aggregate(1, (a, b) => a * b) - HPsize);
                count++;
            }
            Console.WriteLine("Process required " + count + "Iterations");
            return factors;
        }
    }
}
