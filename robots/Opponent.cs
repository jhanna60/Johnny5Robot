using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWH
{
    public class Opponent
    {
        //Opponent class to store info on other combatants
        public String name;
        public double bearing;
        public double head;
        public long ctime;
        public double speed;
        public double x, y;
        public double distance;

        public double guessX(long when)
        {
            long diff = when - ctime;
            return x + Math.Sin(head) * speed * diff;
        }
        public double guessY(long when)
        {
            long diff = when - ctime;
            return y + Math.Cos(head) * speed * diff;
        }

        public double getDistance()
        {
            return distance;
        }
    }
}
