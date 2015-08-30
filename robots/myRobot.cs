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


        // The main method of my robot containing robot logics
        public override void Run()
        {
            // -- Initialization of the robot --

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
                doFirepower();
                doGun();
                Fire(firepower);
                Execute(); 
            }

        }

        void doFirepower()
        {
            //Equation to work out power of bullet
            firepower = (Math.Min(400 / target.getDistance(), 3));
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

        void doGun()
        {
            //works out how long it would take a bullet to travel to where the enemy is *now*
            //this is the best estimation we have
            long time = Time + (int)(target.distance / (20 - (3 * firepower)));

            //Going to implement a different way to shoot based on how successful our hits have been
            //offsets the gun by the angle to the next shot based on linear targeting provided by the opponent class
            double gunOffset = GunHeadingRadians - absbearing(X, Y, target.guessX(time), target.guessY(time));
            //double gunOffset = GunHeadingRadians - absbearing(X, Y, target.x, target.y);
            SetTurnGunLeftRadians(NormaliseBearing(gunOffset));

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

        //if a heading is not within the 0 to 2pi range, alters it to provide the shortest angle
        double NormaliseHeading(double ang)
        {
            if (ang > 2 * PI)
                ang -= 2 * PI;
            if (ang < 0)
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
                target.ctime = Time;				//game time at which this scan was produced
                target.speed = e.Velocity;
                target.distance = e.Distance;
            }
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            if (e.Name == target.name)
            {
                target.distance = 10000; //this will effectively make it search for a new target
            }
        }


        public override void OnHitWall(HitWallEvent e)
        {
            Console.WriteLine("Fucking wall got in Johnny5's way");
        }

        public override void OnWin(WinEvent e)
        {
            //On win we wish to have a victory dance
            //SetTurnRadarLeft(360);
            Console.WriteLine("I am the best!");
        }
    }
}
