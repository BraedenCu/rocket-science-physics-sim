using DongUtility;
namespace RocketScienceHomework
{
    internal class Simulate
    {

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

        public void Start()
        {
            //LEVEL ONE
            //BruteForce();
            //LEVEL TWO
            AngledMotionBruteForce();
            //AngledMotionOptimization();
        }
        
        public void BruteForce()
        {
            double bestFuel = 0;
            double bestTime = 0;

            for (double i = 0; i < 300000; i++)
            {
                fuelWeight = i;
                var rocket = new Rocket(fuelWeight, 90, 3, 3000, 10, false);
                double[] output = rocket.StartRocket(); //t, fuelWeight, theta, totalDist
                if (bestTime == 0)
                {
                    bestTime = output[0];
                    bestFuel = output[1];
                }
                else if (output[0] < bestTime)
                {
                    bestFuel = output[1];
                    bestTime = output[0];
                }
            }
            Console.WriteLine(bestFuel + "  " + bestTime);
        }

        public void AngledMotionBruteForce()
        {
            double bestFuel = 0;
            double chiSquared = 0;
            double bestAngle = 0;
            bool angled = true;

            double angleLowerBound = 0;
            double angleUpperBound = 20;
            double fuelWeightLowerBound = 10000;
            double fuelWeightUpperBound = 15000;

            for (double angle = angleLowerBound; angle <= angleUpperBound; angle++)
            {
                for (fuelWeight = fuelWeightLowerBound; fuelWeight <= fuelWeightUpperBound; fuelWeight++)
                {
                    var rocket = new Rocket(fuelWeight, angle, 3, 3000, 10, true);
                    double[] output = rocket.StartRocket(); //t, fuelWeight, theta, dist, chi squared
                    if (chiSquared == 0)
                    {
                        chiSquared = output[4];
                        bestFuel = output[1];
                        bestAngle = output[2];
                    }
                    else if (output[4] < chiSquared)
                    {
                        chiSquared = output[4];
                        bestFuel = output[1];
                        bestAngle = output[2];
                    }
                }
            }
            Console.WriteLine(bestFuel + "  " + chiSquared + "  " + bestAngle + "         fuel -> chi squared -> anlgle2");
        }


        public double[] RecursiveOptimization(double boxX, double boxY, double angleUpper, double angleLower, double fuelUpper, double fuelLower, double totalDist, double nBox)
        {
            double bestBoxX = 0;
            double bestBoxY = 0;

            double bestAngleUpper = 0;
            double bestFuelUpper = 0;
            double bestAngleLower = 0;
            double bestFuelLower = 0;

            Console.WriteLine(boxX + "  " + boxY + ": New Recursion" + "lower -> higher angle" + angleLower + "\t" + angleUpper + "    lower -> higher weight" + fuelLower + "\t" + fuelUpper);
            //reset best dist and angles
            double bestDist = 0;

            //angleUpper = boxX 

            //rows
            for (int i = 0; i < nBox; i++)
            {
                Console.WriteLine(angleLower + "    " + angleUpper);
                double boxNewX = (int)angleLower + ((angleUpper) / (2 * nBoxes)) + (i * (angleUpper) / nBoxes);
                //columns
                for (int z = 0; z < nBox; z++)
                {
                    double boxNewY = (int)fuelLower + ((fuelUpper) / (2 * nBoxes)) + (z * (fuelLower) / nBoxes);
                    //Console.WriteLine("     " + boxNewX + "  " + boxNewY);
                    var rocket = new Rocket(boxNewY, boxNewX, 3, 3000, 10, true);
                    double[] output = rocket.StartRocket(); //t, fuelWeight, theta, totalDist
                    //Console.WriteLine(output[2]);

                    if (bestDist == 0)
                    {
                        bestDist = output[3];
                        bestBoxX = output[2];
                        bestBoxY = output[1];
                        bestAngleUpper = bestBoxX + ((angleUpper) / (2 * nBoxes));
                        bestAngleLower = bestBoxX - ((angleUpper) / (2 * nBoxes));

                        bestFuelUpper = bestBoxY + ((fuelUpper) / (2 * nBoxes));
                        bestFuelLower = bestBoxY - ((fuelUpper) / (2 * nBoxes));
                    }
                    else if (totalDist > bestDist)
                    {
                        bestDist = output[3];
                        bestBoxX = output[2];
                        bestBoxY = output[1];
                    }
                }
            }
            if (iter <= 0)
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
            double fuelLower = 199990;
            double fuelUpper = 200000;
            double angleLower = 0;
            double angleUpper = 90;


            //optimization specific
            nBoxes = 3;
            iter = 10;
            double[] outputCoords = RecursiveOptimization(angleUpper, fuelUpper, angleUpper, angleLower, fuelUpper, fuelLower, bestDist, nBoxes);
            bestDist = outputCoords[3];
            bestFuel = outputCoords[1];
            bestAngle = outputCoords[0];

            Console.WriteLine(bestFuel + "  " + bestAngle + "  " + bestDist + "         fuel -> angle -> dist2");
        }
    }
}
