using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace KinectTrack
{
    class skelListStats
    {
        public float maxX, maxY, maxZ, minX, minY, minZ;
        private const int numKlusters = 128;  //try starting with 128 clusters?
        private SkeletonPoint[,] klusterz= new SkeletonPoint[numKlusters,20];  //ignoring skeleton position
        private List<List<int>> discreteFrames;  //a list of each "walking" sequence represented as a sequence of discrete klusters
        private List<List<Skeleton>> closet;
        private Random rando = new Random();

        public skelListStats(List<List<Skeleton>> closet)
        {
            this.closet = closet;  //should work since it won't be garbage-collected with a reference...  TODO: right?
            //assume that 0 will be within the range of values
            maxX = 0;
            minX = 0;
            maxY = 0;
            minY = 0;
            maxZ = 0;
            minZ = 0;
            for (int i = 0; i < closet.Count; i++)
            {
                List<Skeleton> currentList = closet[i];
                for (int k = 0; k < currentList.Count; k++)
                {
                    Skeleton currentFrame = currentList[k];
                    SkeletonPoint currentPos;
                    for (int j = 0; j < 20; j++)
                    {  //iterate through each joint
                        currentPos = currentFrame.Joints[(JointType)j].Position;
                        if (currentPos.X > maxX)
                        {
                            maxX = currentPos.X;
                        }
                        else if (currentPos.X < minX)
                        {
                            minX = currentPos.X;
                        }
                        if (currentPos.Y > maxY)
                        {
                            maxY = currentPos.Y;
                        }
                        else if (currentPos.Y < minY)
                        {
                            minY = currentPos.Y;
                        }
                        if (currentPos.Z > maxZ)
                        {
                            maxZ = currentPos.Z;
                        }
                        else if (currentPos.Z < minZ)
                        {
                            minZ = currentPos.Z;
                        }

                    }
                }
            }
        }

        /*
         * initialize Klusters - set the desired number of clusters to their randomized values
         */
        public void initKlusters()
        {
            for (int i = 0; i < numKlusters; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    klusterz[i, j].X = (float)getRand(minX, maxX);
                    klusterz[i, j].Y = (float)getRand(minY, maxY);
                    klusterz[i, j].Z = (float)getRand(minZ, maxZ);
                }
            }
            return;
        }

        /*
         * initKlsuters(String fileName) - get klusters from a file (in the case where we are classifying and are using klusters determined from training
         */
        public void initKlusters(String fileName)
        {
            //TODO: implement me
            return;
        }

        private double getRand(double min, double max)
        {
            return rando.NextDouble() * (max - min) + min;
        }

        public void printKlusters(String fileName)
        {

            String output = "";  //where all the goodies will go
            for (int i = 0; i < numKlusters; i++)
            {
                output += "Cluster " + i + ":\n";
                for (int j = 0; j < 20; j++)
                {
                    output += "X: " + klusterz[i, j].X + "\t";
                    output += "Y: " + klusterz[i, j].Y + "\t";
                    output += "Z: " + klusterz[i, j].Z + "\t";
                }
                output += "\n";
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))  //TDO: not sure what the @ does
            {
                file.WriteLine(output);
            }
            return;
        }

        /*
         * assignAllToKlusters() - assigns the given list of list of skeletons to a list of list of ints representing states
         *      - uses already initialized klusters
         */
        private void assignAllToKlusters()
        {
            discreteFrames = new List<List<int>>();
            for (int i = 0; i < closet.Count; i++)
            {  //for every frame sequence (each training sample)
                discreteFrames.Add(new List<int>());
                for (int j = 0; j < closet[i].Count; j++)
                {
                    discreteFrames[i][j] = frameToKluster(closet[i][j]);
                }
            }
            return;
        }


        /*
         * frameToKluster - takes a frame and returns the cluster it maps to
         */
        public int frameToKluster(Skeleton skelly)
        {
            int assignedKluster=-1;
            double minimum=0.0;
            //iterate over all klusters and return the one with the minimum error
            for (int i = 0; i < numKlusters; i++)
            {
                double temp = frameDistFromKluster(i, skelly);
                if (temp < minimum)
                {
                    minimum = temp;
                    assignedKluster = i;
                }
            }
            //TODO: assert that value is an actual kluster  (not -1) 
            return assignedKluster;
        }

        /*
         * frameDistFromKluster - calculates the difference between the given skeleton and a cluster represented as 
         *      (Xi - Ci)^2 where Xi is a value from the current frame and Ci is the corresponding value for the cluster
         */
        public double frameDistFromKluster(int klusterNum, Skeleton frame)
        {
            //TODO: assert klusterz has been initialized and frame != null
            double sum=0.0;
            for (int i = 0; i < 20; i++)
            {
                sum += Math.Pow((klusterz[klusterNum, i].X - frame.Joints[(JointType)i].Position.X), 2);
                sum += Math.Pow((klusterz[klusterNum, i].Y - frame.Joints[(JointType)i].Position.Y), 2);
                sum += Math.Pow((klusterz[klusterNum, i].Z - frame.Joints[(JointType)i].Position.Z), 2);
            }
            return sum;
        }
    }
}
