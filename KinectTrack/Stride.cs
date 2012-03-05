using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace KinectTrack
{
    class Stride
    {
        private List<DanSkeleton> capturedFrames;
        private int firstFrame;  //first frame of stride as determined by alg
        private int lastFrame;  //last frame of "stride" as determiend by alg
        private double[] jointStandardDeviations = new double[20];  //20 joints same for below
        private double[] jointAvg = new double[20];
        private double[] jointDistStandardDeviations = new double[20];  //TODO: also not 20
        private double[] jointDistAvg = new double[20];  //TODO: also not 20
        private double[] anglesStandardDeviations = new double[20];  //TODO: it's not 20
        private double[] anglesAvg = new double[20];  //TODO: not 20, stupid
        Stride(List<Skeleton> rawSkeletonList)
        {
            //TODO: transform this into DanSkeletons ....use other methods to make it into a "Stride" 
            //take rawSkeleton list and transform to DanSkeletons - save as CapturedFrames
            //rotateSkelList capturedFrames so that it is aligned with x-axis direction of movement
            //use foot crossing function to determine actual stride start and end (save as firstFrame and lastFrame)
            //now with a "Stride," calculate descriptive values and store as local vars
        }

        public List<DanSkeleton> convertToDanFromSkel(List<Skeleton> rawSkeletonList){
            //TODO:  take input list of Skeletons and convert to DanSkelton List
            return null;
        }

        public List<DanSkeleton> rotateSkelList(List<DanSkeleton> skelList)
        {
            //TODO: rotate the DanSkel list that we got from initial list
            //use first and last frames to determine movement direction
            //rotate entire skeleton to act as though movign along x-axis
            //return
            return null;
        }

        public void getStridePositions(){
            //use x-axis basis to determine foot-overlap period
            //set firstFrame and lastFrame variables (indexes to determien stride from private capturedFrames
            return;
        }

        /*
         * TODO: DESCRIPTIVE ATTRIBTUES - write functions to calculate these and record them as class variables
         *       List is below
         */

        //percent of cycle in swing v. stance mode

        //strideLength in distance
        //strideLenght in time (number of frames)
        //velocity (combination of distance + frames)  note: learning alg should 
        //          be able to determine this implicitly, but the more info we give it the better  (can use to guess age!)
        //step length (should be strideLength/2 but could calculate for right and left to see if there is a difference
        //step length vs leg length (pg 43)
        //step width pg 41 (distance between feet between touchdown points)
        //foot separation (max and min)
        
        //arm motion/arc

        //averages and standard deviations of all point positions
        //averages and standard deviations of all distances between points
        //averages and standard deviations of all angles between lines between points


        //max height off of ground (probably just max head height)
        //min height from ground (prob just min head height)


        //pelvic functions
          //pelvic rotation pg 4
          //pelvic list  pg 4p
       
        //knee flexion?? pg 4 (might not be able to measure this)

        //other rotations
          //rotations of thorax and shoulders
          //rotations of thigh and leg
          //rotations in the ankle and foot
         
        //foot angle
        
        //max foot elevation
        //lateral displacement of body pg 9
            //increased with feet farther apart, decreased closer together


        //center of mass measurements? see pg 3 
        
        //"straightness"

        //...

        //print to file in format that would be used by machine learning svm/neural net/descriptive statistics, whatever
        public void printToFile(){
            return;
        }


 
    }
}
