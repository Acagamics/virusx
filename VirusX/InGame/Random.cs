﻿using System;
using Microsoft.Xna.Framework;

namespace VirusX
{
    /// <summary>
    /// a better, global random number generator
    /// </summary>
    class Random
    {
        private const UInt32 PHI = 0x9e3779b9;
        private static UInt32[] Q = new UInt32[4096];
        private static UInt32 c = 362436;


        public static void InitRandom(UInt32 seed)
        {
            UInt32 i;
			Q[0] = seed;
			Q[1] = seed + PHI;
			Q[2] = seed + PHI + PHI;
 
			for (i = 3; i < 4096; i++)
					Q[i] = Q[i - 3] ^ Q[i - 2] ^ PHI ^ i;
        }

        public static UInt32 Next()
        {
            UInt64 t, a = 18782L;
			UInt32 i = 4095;
			UInt32 x, r = 0xfffffffe;
			i = (i + 1) & 4095;
			t = a * Q[i] + c;
            c = (UInt32)(t >> 32);
            x = (UInt32)(t + c);
			if (x < c)
            {
                x++;
                c++;
			}
			return (Q[i] = r - x);
        }

        /// <summary>
        /// between 0 and max-1
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static uint Next(uint max)
        {
            return Next() % max;
        }

        /// <summary>
        /// between 0 and max-1
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(int max)
        {
            return Math.Abs((int)Next()) % max;
        }

        /// <summary>
        /// between min and max-1
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(int min, int max)
        {
            return Math.Abs((int)Next()) % (max - min) + min;
        }

        public static double NextDouble()
        {
            return ((double)Next() / UInt32.MaxValue);
        }

        public static double NextDouble(double max)
        {
            return ((double)Next() / UInt32.MaxValue) * max;
        }

        public static double NextDouble(double min, double max)
        {
            return ((double)Next() / UInt32.MaxValue) * (max - min) + min;
        }

        public static Vector2 NextDirection()
        {
            double angle = NextDouble(MathHelper.TwoPi);
            return new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
        }
    }
}
