using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amporis.TasksChooser
{
    public class TaskRandom
    {
        int seed;
        Random random;

        public TaskRandom(string seed)
        {
            if (String.IsNullOrEmpty(seed))
                seed = Guid.NewGuid().ToString("N");
            this.seed = seed.GetIntHash(); 
            //Debug.WriteLine($"seed = '{seed}' = {this.seed}");
            random = new Random(this.seed);
        }

        public double NextDouble()
        {
            double rnd = random.NextDouble();
            if (rnd == 0)
            {
                seed++;
                random = new Random(seed);
                rnd = NextDouble();
            }
            return rnd;
        }

        public int NextInt(int max) => (int)Math.Truncate(NextDouble() * max);
        public int NextRange(int min, int max) => (int)Math.Truncate(NextDouble() * (max - min + 1)) + min; // 10-20, 20-10+1 = 11 => 0.0-10.9 => T = 0-10   + 10 = 10-20
    }
}
