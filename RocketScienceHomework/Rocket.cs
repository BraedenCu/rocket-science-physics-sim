using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DongUtility;

namespace RocketScienceHomework
{
    internal class Rocket
    {
        public const double G = 6.67408E-11;
        public const double G2 = 6.67408E11;
        public const double earthR = 6378.1 * 1000; //km -> m
        public const double earthM = 5.97219E24; //kg
        public const double goalH = 3.5E7;  
        public const double initialH = earthR;
        public const double burnRate = 240; // kg/s
        public double payloadWeight;        //kg
        public double fuelWeight;           //kg
        public double dT;
        public double simT;
        public double nBoxes;
        public double iter = 0;

        //lvl two specific
        public double theta;
        public double totalDist;
        public bool leftGround;
        public Vector[] points = { new Vector(0, 0, 0), new Vector(0, 100000, 26000), new Vector(0, 200000, 30000), new Vector(0, 300000, 27000), new Vector(0, 400000, 18000), new Vector(0, 500000, 6000), new Vector(0, 544883, 0) };

        //forces
        public Vector fGrav;
        public Vector fThrust;
        public Vector fNet;
        public Vector aRocket, vRocket, pRocket;


        public Rocket(double fuelWeight, double theta, double nBoxes, double simT, double dT)
        {
            payloadWeight = 123048;
            this.fuelWeight = fuelWeight;
            this.theta = theta;
            totalDist = 0;
            this.simT = simT;
            this.dT = dT;
            this.nBoxes = nBoxes;
            leftGround = false;
            fGrav = new Vector(0, 0, 0);
            fThrust = new Vector(0, 0, 0);
            fNet = new Vector(0, 0, 0);
            aRocket = new Vector(0, 0, 0);
            vRocket = new Vector(0, 0, 0);
            pRocket = new Vector(0, 0, earthR);
        }

        public bool StartRocket()
        {
            for (double t = 0; t < simT; t += dT)
            {
                updateRocketPosition();
                CheckRocketPosition();
                updateFuel();
                bool threshold = checkThreshold(true);//checkThreshold, if true, stop looping.
                if (threshold)
                {
                    return true;
                }
            }
            return true;
        }


        public void calcForceGrav()
        {
            double zGrav = (earthM * (fuelWeight + payloadWeight) * G) / (pRocket.Z * pRocket.Z);
            fGrav = new Vector(0, 0, zGrav);
        }

        public void calculateThrust()
        {
            const double thrustConst = 1.2E7;
            if (fuelWeight > 0)
            {
                //fThrust = new Vector(0, 0, 1.2E7);
                fThrust = new Vector(0, Math.Cos(theta * (Math.PI / 180.0)) * thrustConst, Math.Sin(theta * (Math.PI / 180.0) * thrustConst));
            }
            else
            {
                fThrust = new Vector(0, 0, 0);
            }
        }

        public void updateRocketPosition()
        {
            calculateThrust();
            calcForceGrav();
            fNet = fThrust - fGrav;
            //Console.WriteLine(fuelWeight + "    " + fNet + "    " + fThrust + "   " + fGrav);
            //Console.WriteLine(pRocket.Z);
            aRocket = fNet / (fuelWeight + payloadWeight);
            vRocket = vRocket + (aRocket * dT);
            pRocket = pRocket + (vRocket * dT);
        }

        public bool checkThreshold(bool angled)
        {
            if (!angled)
            {
                //can't go below the Earth's surface
                if (pRocket.Z < earthR)
                {
                    pRocket.Z = earthR;
                }
                else if (pRocket.Z > goalH)
                {
                    return true; //exit for loop, found the correct value
                }
                else
                {
                    leftGround = true;
                }
            }
            else if (angled)
            {
                if (pRocket.Z <= earthR + 10 && leftGround) //if it hits the surface and the thrust is high enough      
                {
                    return true;
                }
                //can't go below the Earth's surface
                else if (pRocket.Z < earthR)
                {
                    pRocket.Z = earthR;
                }
                else
                {
                    leftGround = true;
                }
            }
            return false;
        }

        public void updateFuel()
        {
            fuelWeight -= burnRate * dT;
            if (fuelWeight < 0)
            {
                fuelWeight = 0;
            }
        }

        /// <summary>
        /// FIXXX
        /// </summary>
        //record when a rocket passes a certain point (when it crosses dong x, calc y) //calc ki squarted at all 7 points then add together to get the total (FIXX)
        public void CheckRocketPosition()
        {
            for (int i = 0; i < points.Length; i++)
            {
                totalDist += Math.Abs(Math.Abs(pRocket.Y - points[i].Y) + Math.Abs(pRocket.Z - points[i].Z));
            }
        }
    }
}
