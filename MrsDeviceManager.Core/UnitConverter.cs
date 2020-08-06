using System;

namespace MrsDeviceManager.Core
{
    /// <summary>
    /// Convert angle types
    /// </summary>
    public static class UnitConverter
    {
        /// <summary>
        /// Convert radian to degree
        /// </summary>
        /// <param name="rad">radian unit</param>
        /// <returns>degree unit</returns>
        public static double RadianToDegree(double rad)
        {
            return rad / Math.PI * 180;
        }

        /// <summary>
        /// Convert degree to radian
        /// </summary>
        /// <param name="degree">degree unit</param>
        /// <returns>radian unit</returns>
        public static double DegreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        /// <summary>
        /// Convert degree to mils
        /// </summary>
        /// <param name="degree">degree unit</param>
        /// <returns>mils unit</returns>
        public static double DegreeToMils(double degree)
        {
            return degree * 17.777778;
        }

        /// <summary>
        /// Convert mils to degree
        /// </summary>
        /// <param name="mils">mils unit</param>
        /// <returns>degree unit</returns>
        public static double MilsToDegree(double mils)
        {
            return mils * 0.05625;
        }
    }
}
