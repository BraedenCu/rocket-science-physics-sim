using DongUtility;
namespace RocketScienceHomework
{
    internal class Simulate
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

        public Simulate()
        {
            dT = 1;
            simT = 20000;
            resetRocket();
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
            if(!angled)
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
        public void BruteForce()
        {
            double bestFuel = 0;
            double bestTime = 40000; //500000
            theta = Math.PI / 2;

            for (double i = 300000; i < 300001; i++)
            {
                fuelWeight = i;
                for (double t = 0; t < simT; t += dT)
                {
                    updateRocketPosition();
                    updateFuel();
                    bool endSimulation = checkThreshold(false); //checkThreshold, if true, stop looping.
                    if (endSimulation)
                    {
                        if (t < bestTime)
                        {
                            bestFuel = i;
                            bestTime = t;
                        }
                        t = simT;
                    }
                }
                resetRocket();
            }
            Console.WriteLine(bestFuel + "  " + bestTime);
        }

        public void Start()
        {
            //LEVEL ONE
            //BruteForce();
            //LEVEL TWO
            //AngledMotionBruteForce();
            AngledMotionOptimization();
        }



        /// <summary>
        /// ALL ABOVE THIS LINE IS SOLID
        /// </summary>

        public void resetRocket()
        {
            payloadWeight = 123048;
            fuelWeight = 0;
            theta = 0;
            totalDist = 0;
            nBoxes = 0;
            leftGround = false;
            fGrav = new Vector(0, 0, 0);
            fThrust = new Vector(0, 0, 0);
            fNet = new Vector(0, 0, 0);
            aRocket = new Vector(0, 0, 0);
            vRocket = new Vector(0, 0, 0);
            pRocket = new Vector(0, 0, earthR);
        }

        //record when a rocket passes a certain point (when it crosses dong x, calc y) //calc ki squarted at all 7 points then add together to get the total (FIXX)
        public void CheckRocketPosition()
        {
            for(int i = 0; i<points.Length; i++)
            {
                totalDist += Math.Abs(Math.Abs(pRocket.Y - points[i].Y) + Math.Abs(pRocket.Z - points[i].Z));
            }
        }

        public bool CalculateOneRocketCycle()
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

        public double[] RecursiveOptimization(double boxX, double boxY, double angleUpper, double angleLower, double fuelUpper, double fuelLower, double totalDist, double nBox)
        {
            double bestBoxX = boxX;
            double bestBoxY = boxY;
            double bestAngleUpper = angleUpper;
            double bestFuelUpper = fuelUpper;
            double bestAngleLower = angleLower; ;
            double bestFuelLower = fuelLower;
            double bestDist = totalDist;
            Console.WriteLine(boxX + "  " + boxY);

            //rows
            for (int i = 0; i < nBox; i++)
            {
                double boxNewX = (int) fuelLower + ((angleUpper) / (2 * nBoxes)) + (i * (angleUpper) / nBoxes);
                //columns
                for (int z = 0; z < nBox; z++)
                {
                    double boxNewY = (int) angleLower + ((fuelUpper) / (2 * nBoxes)) + (z * (fuelUpper) / nBoxes);

                    Console.WriteLine("     " + boxNewX + "  " + boxNewY);

                    //repopulate rocket values
                    /*
                    resetRocket();
                    fuelWeight = boxNewY;
                    theta = boxNewX;
                    nBoxes = nBox;
                    bool endSimulation = CalculateOneRocketCycle();
                    */


                    var Rocket = new Rocket(fuelWeight, theta, nBoxes, simT, dT);
                    bool endSimulation = Rocket.StartRocket();

                    if (endSimulation)
                    {
                        if (bestDist == 0)
                        {
                            bestDist = totalDist;
                            bestBoxX = boxNewX;
                            bestBoxY = boxNewY;
                            bestAngleUpper = bestBoxX + ((angleUpper) / (2 * nBoxes));
                            bestAngleLower = bestBoxX - ((angleUpper) / (2 * nBoxes));
                            //Console.WriteLine(bestAngleUpper);
                            bestFuelUpper = bestBoxY + ((fuelUpper) / (2 * nBoxes));
                            bestFuelLower = bestBoxY - ((fuelUpper) / (2 * nBoxes));
                        }
                        else if (totalDist < bestDist)
                        {
                            bestDist = totalDist;
                            bestBoxX = boxNewY;
                            bestBoxY = boxNewX;
                            //Console.WriteLine(bestAngleUpper);
                        }
                    }
                }
            }
            //Console.WriteLine(angleUpper);
            if(iter <= 0)
            {
                double[] outputCoords = { bestBoxX, bestBoxY, bestAngleUpper, bestFuelUpper, bestDist };
                return outputCoords;
            }
            else
            {
                iter -= 1;
                return RecursiveOptimization(bestBoxX, bestBoxY, bestAngleUpper, bestAngleLower, bestFuelUpper, bestFuelLower, bestDist, nBox);
            }
        }


        public void AngledMotionOptimization()
        {
            double bestFuel = 0;
            double bestDist = 0;
            double bestAngle = 0;
            double fuelLower = 0;
            double fuelUpper = 205010;
            double angleLower = 0;
            double angleUpper = 90;

            //optimization specific
            nBoxes = 3;
            iter = 10;
            double[] outputCoords = RecursiveOptimization(angleUpper, fuelUpper, angleUpper, angleLower, fuelUpper, fuelLower, bestDist, nBoxes);
            bestDist = outputCoords[4];
            bestFuel = outputCoords[3];
            bestAngle = outputCoords[2];

            Console.WriteLine(bestFuel + "  " + bestDist + "  " + bestAngle + "         fuel -> dist -> anlgle");
        }












        public void AngledMotionBruteForce()
        {
            double bestFuel = 0;
            double bestDist = 0;
            double bestAngle = 0;
            theta = Math.PI / 4;
            double fuelWeightLowerBound = 205009;
            double fuelWeightUpperBound = 205010;

            double angleLowerBound = 30;
            double angleUpperBound = 60;

            for(double angle = angleLowerBound; angle <= angleUpperBound; angle++)
            {
                for (fuelWeight = fuelWeightLowerBound; fuelWeight <= fuelWeightUpperBound; fuelWeight++)
                {
                    for (double t = 0; t < simT; t += dT)
                    {
                        updateRocketPosition();
                        CheckRocketPosition();
                        updateFuel();
                        bool endSimulation = checkThreshold(true);//checkThreshold, if true, stop looping.
                        if (endSimulation)
                        {
                            if (bestDist == 0)
                            {
                                bestDist = totalDist;
                                bestFuel = fuelWeight;
                                bestAngle = angle;
                            }
                            else if (totalDist < bestDist)
                            {
                                bestDist = totalDist;
                                bestFuel = fuelWeight;
                                bestAngle = angle;
                            }
                            t = simT;
                        }
                    }
                }
            }
            Console.WriteLine(bestFuel + "  " + bestDist + "  " + bestAngle + "         fuel -> dist -> anlgle");
        } 
    }
}
