using System;
using System.Linq;

namespace ColourSearchEngine
{
    public class Histogram
    {
        public int[][] RgbHistogram { get; set; }
        public int[][] HsvHistogram { get; set; }

        public static double RgbDistance(int[][] l, int[][] r, Func<int[], int[], int, double> compareMethod)
        {
            double ldiff = compareMethod(l[0], r[0], 4);
            double rdiff = compareMethod(l[1], r[1], 4);
            double gdiff = compareMethod(l[2], r[2], 4);
            double bdiff = compareMethod(l[3], r[3], 4);

            return ldiff + rdiff + gdiff + bdiff;
        }

        public static double HsvChiSquareDistance(int[][] l, int[][] r, Func<int[], int[], int, double> compareMethod)
        {
            double hdiff = compareMethod(l[0], r[0], 3) * 100 / 256 / 4;
            double sdiff = compareMethod(l[1], r[1], 3);
            double vdiff = compareMethod(l[2], r[2], 3);

            return hdiff + sdiff + vdiff;
        }

        public static double ChiSquare(int[] h1, int[] h2, int dimensions)
        {
            double diff = 0;
            for (int i = 0; i < h2.Length; i++)
            {
                double x = Math.Pow((h1[i] - h2[i]), 2);

                long y = h1[i] + h2[i];

                diff += y > 0 ? x/y : x;
            }

            return diff/2;
        }

        public static double ChiSquare2(int[] h1, int[] h2, int dimensions)
        {
            double diff = 0;

            for (int j = 0; j < h1.Length; j++)
            {
                long a = h1[j] - h2[j];
                long b = h1[j];
                
                if (b > 0)
                    diff += a * a / (double)b;
            }

            return diff;
        }

        public static double Correlation(int[] h1, int[] h2, int dimensions)
        {
            double result = 0;
            double s1 = 0, s2 = 0, s11 = 0, s12 = 0, s22 = 0;

            for (int i = 0; i < h1.Length; i++)
            {
                double a = h1[i];
                double b = h2[i];

                s12 += a * b;
                s1 += a;
                s11 += a * a;
                s2 += b;
                s22 += b * b;
            }

            double total = h1.Sum();
            double scale = 1d/total;
            double num = s12 - s1*s2*scale;
            double denom2 = (s11 - s1*s1*scale)*(s22 - s2*s2*scale);
            result = Math.Abs(denom2) > Double.Epsilon ? num/Math.Sqrt(denom2) : 1d;
            
            return result / dimensions;
        }

        public static double Intersection(int[] h1, int[] h2, int dimensions)
        {
            double diff = 0;
            for (int i = 0; i < h1.Length; i++)
            {
                diff += Math.Min(h1[i], h2[i]);
            }

            return diff;
        }
    }
}