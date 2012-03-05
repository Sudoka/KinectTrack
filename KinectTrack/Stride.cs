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
        private firstFrame;  //first frame of stride as determined by alg
        private lastFrame;  //last frame of "stride" as determiend by alg
        Stride(List<Skeleton> rawSkeletonList)
        {
            //TODO: transform this into DanSkeletons ....use other methods to make it into a "Stride" 
        }

        public DanSkeleton<List> convertToDanFromSkel(List<Skeleton> rawSkeletonList){
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

        //pelvic functions
          //side to side
          //up and down

        //limb lengths
          //right arm
          //left arm
          //right leg
          //left leg
          //etc  ...
       
         
        //foot angle
        
        //max elevation
        
        //"straightness"

        //...

        //print to file in format that would be used by machine learning svm/neural net/descriptive statistics, whatever
        public void printToFile(){
            return;
        }


 
    }
}
