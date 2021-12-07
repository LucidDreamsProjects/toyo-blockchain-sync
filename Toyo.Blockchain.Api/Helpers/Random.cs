using System;

namespace Toyo.Blockchain.Api.Helpers
{
    public class ToyoRandom
    {
        private int rSeed = 0;

        public static ulong Raffle(int min, int max)
        {
            var result = new ToyoRandom().Rnd(min, max);
            var typeId = Convert.ToUInt64(result);
            return typeId;
        }

        private int PreRnd(int n1, int n2, int but = -1)
        {
            var random = new Random(rSeed + System.Environment.TickCount);

            rSeed += random.Next(1, 5);

            int saida = random.Next(n1, n2 + 1);

            if (saida == but)
                saida++;
            if (saida > n2 + 1)
                saida = n1;

            return saida;
        }

        private int Rnd(int _min, int _max, int but = -999)
        {
            long seed = PreRnd(0, 1000);

            long a = PreRnd(1100515245, 1103515245);
            //increment
            long c = PreRnd(10345, 12345);
            //modulus m (which is also the maximum possible random value)
            long m = (long)Math.Pow(2f, 31f);

            //How many random numbers do we want to generate?
            int amountOfNumbers = 733;

            //Array that will store the random numbers so we can display them
            float[] randomNumbers = new float[amountOfNumbers];

            for (int i = 0; i < amountOfNumbers; i++)
            {
                //Basic idea: seed = (a * seed + c) mod m
                seed = (a * seed + c) % m;

                //To get a value between 0 and 1
                float randomValue = seed / (float)m;

                //Remove this line if you want to speed up the testing of the algorithm
                float minMax = _max + 0.99999f - _min;
                int valFin = (int)(minMax * randomValue) + _min;

                randomNumbers[i] = valFin;
            }

            int qual = PreRnd(0, 732);
            int saida = (int)randomNumbers[qual];

            if (saida == but)
                saida = (int)randomNumbers[qual + 1];
            if (qual >= amountOfNumbers)
                saida = (int)randomNumbers[1];

            return saida;
        }
    }
}
