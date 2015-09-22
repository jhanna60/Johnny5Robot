using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robocode;
using Robocode.Util;
using System.Drawing;

namespace JWH
{
    public class WaveBullet
    {
        private double startX, startY, startBearing, power;
        private long fireTime;
        private int direction;
        private int[] returnSegment;

        public WaveBullet(double x, double y, double bearing, double power,
                int direction, long time, int[] segment)
        {
            startX = x;
            startY = y;
            startBearing = bearing;
            this.power = power;
            this.direction = direction;
            fireTime = time;
            returnSegment = segment;
        }

        public double getBulletSpeed()
        {
            return 20 - power * 3;
        }

        public double maxEscapeAngle()
        {
            return Math.Asin(8 / getBulletSpeed());
        }

        public Boolean checkHit(double enemyX, double enemyY, long currentTime)
        {
            // if the distance from the wave origin to our enemy has passed
            // the distance the bullet would have traveled...

            if (GetDistanceBetweenPoints(startX, startY, enemyX, enemyY) <=
                    (currentTime - fireTime) * getBulletSpeed())
            {
                double desiredDirection = Math.Atan2(enemyX - startX, enemyY - startY);
                double angleOffset = Utils.NormalRelativeAngle(desiredDirection - startBearing);
                double guessFactor =
                    Math.Max(-1, Math.Min(1, angleOffset / maxEscapeAngle())) * direction;
                int index = (int)Math.Round((returnSegment.Length - 1) / 2 * (guessFactor + 1));
                returnSegment[index]++;
                return true;
            }
            return false;
        }

        public static double GetDistanceBetweenPoints(double x, double y, double ex, double ey)
        {
            double distance = Math.Sqrt(Math.Pow(x - ex, 2) + Math.Pow(y - ey, 2));
            //return Math.Sqrt(distanceSq(x1, y1, x2, y2));
            //double a = x - ex;
            //double b = y - ey;
            //double distance = Math.Sqrt(a * a + b * b);

            //Console.WriteLine("Distance : {0} ", distance);

            return distance;
        }
    }
}
