using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace KinectTrack
{
    static class  Utils
    {
        public const int BlueIndex = 0;
        public const int GreenIndex = 1;
        public const int RedIndex = 2;

        /// <summary>
        /// Clamp val to be between min and max
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        /// <summary>
        /// Set the pixel at pixelIndex to be a given color
        /// </summary>
        /// <param name="pixelArray"></param>
        /// <param name="pixelIndex"></param>
        /// <param name="c"></param>
        public static void setPixelColor(this byte[] pixelArray, int pixelIndex, Color c)
        {
            pixelArray[pixelIndex + BlueIndex] = c.B;
            pixelArray[pixelIndex + GreenIndex] = c.G;
            pixelArray[pixelIndex + RedIndex] = c.R;
        }
    }
}
