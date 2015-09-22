// Access to standard .NET system
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

// Using the Robocode API that I have imported
using Robocode;
using Robocode.Util;

namespace JWH
{
    class Johnny5 : AdvancedRobot
    {
        Opponent target;
        double PI = Math.PI;
        int direction = 1;
        double firepower;

        int direction1 = 1;

        List<WaveBullet> waves = new List<WaveBullet>();

        // Note: this must be odd number so we can get
        // GuessFactor 0 at middle.

        static int[] stats = new int[31]; // 31 is the number of unique GuessFactors we're using
        //int[,] stats = new int[13,31];


        // The main method of my robot containing robot logics
        public override void Run()
        {
            // -- Initialization of the robot --

            //for (var i = 0; i < stats.Length; i++)
            //    stats[i] = new int[31];

            target = new Opponent();

            target.distance = 100000;

            //Setting my Colours
            SetColors(Color.Black, Color.Transparent, Color.Red, Color.Red, Color.PapayaWhip);

            // Allowing my Radar and gun to turn independently from the body
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForGunTurn = true;
                      
            SetTurnRadarRightRadians(2 * PI);

            // Main body of my loop
            while (true)
            {
                doMovement();
                doScanner();
                Execute(); 
            }
        }

        void doMovement()
        {
            if (Time % 20 == 0)
            { 		//every twenty 'ticks'
                direction *= -1;		//reverse direction
                SetAhead(direction * 700);	//move in that direction
            }
            SetTurnRightRadians(target.bearing + (PI / 2)); //every turn move to circle strafe the enemy
        }

        void doScanner()
        {
            double radarOffset;

            if ((Time - target.ctime) > 4)
            { 	//if we haven't seen anybody for a bit....
                radarOffset = 360;		//rotate the radar to find a target
            }
            else
            {
                //next is the amount we need to rotate the radar by to scan where the target is now
                radarOffset = RadarHeadingRadians - absbearing(X, Y, target.x, target.y);
                
                //this adds or subtracts small amounts from the bearing for the radar to produce the wobbling
                //and make sure we don't lose the target
                if (radarOffset < 0)
                    radarOffset -= PI / 8;
                else
                    radarOffset += PI / 8;
            }
            //turn the radar
            SetTurnRadarLeftRadians(NormaliseBearing(radarOffset));
        }
 
        //if a bearing is not within the -pi to pi range, alters it to provide the shortest angle
        double NormaliseBearing(double ang)
        {
            if (ang > PI)
                ang -= 2 * PI;
            if (ang < -PI)
                ang += 2 * PI;
            return ang;
        }

        //returns the distance between two x,y coordinates
        public double getrange(double x1, double y1, double x2, double y2)
        {
            double xo = x2 - x1;
            double yo = y2 - y1;
            double h = Math.Sqrt(xo * xo + yo * yo);
            return h;
        }

        //gets the absolute bearing between two x,y coordinates
        public double absbearing(double x1, double y1, double x2, double y2)
        {
            double xo = x2 - x1;
            double yo = y2 - y1;
            double h = getrange(x1, y1, x2, y2);
            if (xo > 0 && yo > 0)
            {
                return Math.Asin(xo / h);
            }
            if (xo > 0 && yo < 0)
            {
                return Math.PI - Math.Asin(xo / h);
            }
            if (xo < 0 && yo < 0)
            {
                return Math.PI + Math.Asin(-xo / h);
            }
            if (xo < 0 && yo > 0)
            {
                return 2.0 * Math.PI - Math.Asin(-xo / h);
            }
            return 0;
        }


        // Event handling system below

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            //if we have found a closer robot....

            if ((e.Distance < target.distance) || (target.name == e.Name))
            {
                //the next line gets the absolute bearing to the point where the bot is
                double absbearing_rad = (HeadingRadians + e.BearingRadians) % (2 * PI);
                //this section sets all the information about our target

                target.name = e.Name;
                target.x = X + Math.Sin(absbearing_rad) * e.Distance; //works out the x coordinate of where the target is
                target.y = Y + Math.Cos(absbearing_rad) * e.Distance; //works out the y coordinate of where the target is
                target.bearing = e.BearingRadians;
                target.head = e.HeadingRadians;
                //target.ctime = Time;
                target.ctime = e.Time;				//game time at which this scan was produced
                target.speed = e.Velocity;
                target.distance = e.Distance;


                // Enemy absolute bearing, you can use your one if you already declare it.

                firepower = (Math.Min(500 / target.getDistance(), 3));

                double absBearing = HeadingRadians + target.bearing;

                // find our enemy's location:
                double ex = target.x;
                double ey = target.y;

                Console.WriteLine("number of waves {0} ", waves.Count);

                // Let's process the waves now:
                for (int i = 0; i < waves.Count; i++)
                {
                    WaveBullet currentWave = (WaveBullet)waves.ElementAt(i);
                    if (currentWave.checkHit(ex, ey, Time))
                    {
                        waves.Remove(currentWave);
                        i--;
                    }
                }

                //double power = Math.Min(3, Math.Max(.1, firepower));

                // don't try to figure out the direction they're moving 
                // if they're not moving, just use the direction we had before
                if (target.speed != 0)
                {
                    if (Math.Sin(target.head - absBearing) * target.speed < 0)
                        direction1 = -1;
                    else
                        direction1 = 1;
                }

                //int[] currentStats = stats[target.distance / 100];

                int[] currentStats = stats;

                //foreach (var i in currentStats)
                //{
                //    Console.WriteLine("Current stats array element {0} = {1} ",i ,currentStats[i]);
                //}

                //Console.WriteLine("int array = {0}",((int)(target.distance / 100)));


                WaveBullet newWave = new WaveBullet(X, Y, absBearing, firepower,
                                direction1, Time, currentStats);

                int bestindex = 15; // initialize it to be in the middle, guessfactor 0.
                for (int i = 0; i < 31; i++)
                    if (currentStats[bestindex] < currentStats[i])
                        bestindex = i;

                // this should do the opposite of the math in the WaveBullet:
                double guessfactor = (double)(bestindex - (stats.Length - 1) / 2)
                                / ((stats.Length - 1) / 2);
                double angleOffset = direction1 * guessfactor * newWave.maxEscapeAngle();

                double gunAdjust = Utils.NormalRelativeAngle(
                        absBearing - GunHeadingRadians + angleOffset);
                SetTurnGunRightRadians(gunAdjust);

                //if (SetFireBullet(firepower) != null)
                //{
                //    Console.WriteLine("inside the waves.add");
                //    waves.Add(newWave);
                //}

                if (GunHeat == 0 && gunAdjust < Math.Atan2(9, target.distance) && SetFireBullet(firepower) != null)
                {
                        Console.WriteLine("inside the waves.add");
                        waves.Add(newWave);
                }
            }
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            if (e.Name == target.name)
            {
                target.distance = 10000; //this will effectively make us search for a new target
            }
        }

        public override void OnHitWall(HitWallEvent e)
        {
            Console.WriteLine("Fucking wall got in Johnny5's way");
        }

        public override void OnWin(WinEvent e)
        {
            Console.WriteLine("I am the best!");
        }
    }
}
