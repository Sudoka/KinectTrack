using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace KinectTrack
{
    class Stride
    {
        private List<DanSkeleton> capturedFrames;
        private List<DanSkeleton> rotatedFrames;

        private int firstFrame;  //first frame of stride as determined by alg
        private int lastFrame;  //last frame of "stride" as determiend by alg
        private List<Skeleton> skelList;
        public Stride(List<Skeleton> rawSkeletonList)
        private double[] jointStandardDeviations = new double[20];  //20 joints same for below
        private double[] jointAvg = new double[20];
        private double[] jointDistStandardDeviations = new double[20];  //TODO: also not 20
        private double[] jointDistAvg = new double[20];  //TODO: also not 20
        private double[] anglesStandardDeviations = new double[20];  //TODO: it's not 20
        private double[] anglesAvg = new double[20];  //TODO: not 20, stupid
        Stride(List<Skeleton> rawSkeletonList)
        {

            capturedFrames = convertToDanFromSkel(rawSkeletonList);
            this.numFrames = capturedFrames.Count;

            //rotateSkelList capturedFrames so that it is aligned with x-axis direction of movement
            rotateSkelList(this.capturedFrames);
            //use foot crossing function to determine actual stride start and end (save as firstFrame and lastFrame)
            //now with a "Stride," calculate descriptive values and store as local vars
        }

  

        private List<DanSkeleton> convertToDanFromSkel(List<Skeleton> rawSkeletonList){
            //transform this into DanSkeletons
            //take rawSkeleton list and transform to DanSkeletons - save as CapturedFrames
            var outList = new List<DanSkeleton>(rawSkeletonList.Count);
            foreach (Skeleton s in rawSkeletonList)
            {
                DanSkeleton d = new DanSkeleton(s);
                outList.Add(d);
            }
            return outList;
        }

        public List<DanSkeleton> rotateSkelList(List<DanSkeleton> skelList)
        {
            //TODO: rotate the DanSkel list that we got from initial list
            SkeletonPoint startPos = skelList[0].Position;
            SkeletonPoint endPos = skelList[skelList.Count -1].Position;

            // Get a 2d Vector describing the start and end in terms of z and x
            Vector angleVector = new Vector(endPos.X - startPos.X, endPos.Z - startPos.Z);
            // Reference straight vector
            Vector idealVector = new Vector(1, 0);

            double angle = Vector.AngleBetween(angleVector, idealVector);

            foreach (DanSkeleton ds in skelList)
            {
                MessageBox.Show(ds.Joints[JointType.FootRight].Position.X);
                ds.rotateJointsXZ(startPos.X, startPos.Z, angle);
                MessageBox.Show(ds.Joints[JointType.FootRight].Position.X);
            }
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


        /// <summary>
        /// Given a framenumber and viewport, render the skeleton that corresponds to that frame in the viewport
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <param name="skelViewport"></param>
        public void drawFrameToViewport(int frameNumber, System.Windows.Controls.Viewport3D skelViewport)
        {
            Model3DGroup skel3dGroup = new Model3DGroup();
            DanSkeleton renderSkel = capturedFrames[frameNumber];
            foreach(Joint j in renderSkel.Joints) {
                // Create a cube for each joint
                ModelVisual3D curJointCube;
                if (renderSkel.isStepSkel)
                {
                    curJointCube = Utils3D.getCube(Colors.Blue);
                }
                else
                {
                    curJointCube = Utils3D.getCube(Colors.Red);
                }
                Transform3DGroup tGroup = new Transform3DGroup();
                // move the joints to the right positions
                tGroup.Children.Add(Utils3D.getJointPosTransform(j, 10));
                // Make the squares smaller
                tGroup.Children.Add(new ScaleTransform3D(.5, .5, .5));
                curJointCube.Transform = tGroup;
                skelViewport.Children.Add(curJointCube);
            }
            // Put the camera in the position of the kinect (more or less)
            skelViewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -3), new Vector3D(renderSkel.Position.X, renderSkel.Position.Y, renderSkel.Position.Z), new Vector3D(0, 1, 0), 75);
            // Add a light so that colors are visible
            skelViewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });
        }

        public int numFrames { get; set; }
    }
}
