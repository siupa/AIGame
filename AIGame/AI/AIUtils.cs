using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGame.AI
{
    class AIUtils
    {
        private static Random _random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Returns double values between -1 and 1.
        /// </summary>
        public static double RandomClamped()
        {
            return (_random.NextDouble() - 0.5) * 2.0;
        }

        /// <summary>
        /// Returns double values between 0 and max.
        /// </summary>
        public static double Random(double max)
        {
            return _random.NextDouble() * max;
        }

        /// <summary>
        /// Returns int values between min and max.
        /// </summary>
        public static int RandomInt(int min, int max)
        {
            int range = max - min;
            return (int)Math.Floor(_random.NextDouble() * range) + min;
        }

        public static double Clamp(double value)
        {
            if (value > 1)
                return 1;
            else if (value < 0)
                return 0;
            else
                return value;
        }

        public static double ClampTo(int clampTo, double value)
        {
            if (value > clampTo)
                return clampTo;
            else if (value < 0)
                return 0;
            else
                return value;
        }

        public static double ClampAsOneAroundZero(double value)
        {
            if (value > 0.5)
                return 0.5;
            else if (value < -0.5)
                return -0.5;
            else
                return value;
        }
    }
}
