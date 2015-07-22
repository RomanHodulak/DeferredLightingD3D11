using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightingEngine_v2.LightingD3D11
{
    public class Interpolation
    {
        /*public static double Linear(double pointA, double pointB, double totaltime, double currenttime)
        {
            double r = currenttime / totaltime * (pointA - pointB);
            return r;
        }*/

        public static double Linear(double pointA, double pointB, double currenttime)
        {
            double B = pointB - pointA;
            double R = currenttime - pointA;
            return ((double)R / (double)B);
        }
    }

    public enum InterpolationType
    {
        Linear,
    }
}
