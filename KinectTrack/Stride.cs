using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using MoreLinq;

namespace KinectTrack
{
    class Stride
    {
        public List<DanSkeleton> capturedFrames; //TODO: make private again
        

        private int firstFrame;  //first frame of stride as determined by alg
        private int lastFrame;  //last frame of "stride" as determiend by alg
        
       
        private double[] jointStandardDeviations = new double[20];  //20 joints same for below
        private double[] jointAvg = new double[20];
        private double[] jointDistStandardDeviations = new double[20];  //TODO: also not 20
        private double[] jointDistAvg = new double[20];  //TODO: also not 20
        private double[] anglesStandardDeviations = new double[20];  //TODO: it's not 20
        private double[] anglesAvg = new double[20];  //TODO: not 20, stupid
        public Stride(List<Skeleton> rawSkeletonList)
        {

            capturedFrames = convertToDanFromSkel(rawSkeletonList);
            this.numFrames = capturedFrames.Count;

            //rotateSkelList capturedFrames so that it is aligned with x-axis direction of movement
            rotateSkelList(this.capturedFrames);
            //use foot crossing function to determine actual stride start and end (save as firstFrame and lastFrame)
            this.getStridePositions();
            //TODO: sanitize/add checks
            capturedFrames[firstFrame].isStepSkel = true;
            capturedFrames[lastFrame].isStepSkel = true;
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

            //foreach (DanSkeleton ds in skelList)
            for(int i = 0; i < skelList.Count; i++)
            {
                DanSkeleton ds = skelList[i];
                ds.rotateJointsXZ(startPos.X, startPos.Z, angle);
            }
            //use first and last frames to determine movement direction
            //rotate entire skeleton to act as though movign along x-axis
            //return
            return null;
        }

        // Returns the difference in the position between the left foot and the right foot for a given skeleton
        private double footDifference(DanSkeleton d)
        {
            return d.Joints[JointType.FootLeft].Position.X - d.Joints[JointType.FootRight].Position.X;

        }

        private RelDir firstFootDown;
        private RelDir midFootDown;
        private RelDir lastFootDown;

        public void getStridePositions(){
            //use x-axis basis to determine foot-overlap period
            //set firstFrame and lastFrame variables (indexes to determien stride from private capturedFrames
            //iterate through skeleton frames to find first, second and third "crossing points"
            int crossingPointNum=0;
            //base it on left - right (x-axis based after rotation)
            bool truthTest;
            DanSkeleton skeletonZero = capturedFrames[0];
            truthTest=(footDifference(skeletonZero) < 0); // if lfoot behind rfoot then truthTest = true
            //TODO: Do we need to correct for jitter in this function?
            for(int i=0;i<capturedFrames.Count;i++){
                //if truthTest differs, then the relative positions of the points have changed
                if(truthTest!=(footDifference(capturedFrames[i]) < 0)){
                    truthTest=(!truthTest);
                    if(crossingPointNum==0){
                        this.firstFrame=i;
                        crossingPointNum++;
                        firstFootDown = getLowerFoot(capturedFrames[i]);
                    }else if(crossingPointNum==2){  //we are interested in the 3rd point to end a full-stride
                        this.lastFrame=i;
                        crossingPointNum++;
                        lastFootDown = getLowerFoot(capturedFrames[i]);
                        break;  //TODO: use a "break" statment? gasp! // WRONG: break's are awesome. you should take one now!
                    }else{
                        crossingPointNum++;
                        midFootDown = getLowerFoot(capturedFrames[i]);
                    }
                }
            }
            return;
        }

        private RelDir getLowerFoot(DanSkeleton skel)
        {
            if (skel.Joints[JointType.FootLeft].Position.Y < skel.Joints[JointType.FootRight].Position.Y)
                return RelDir.L;
            else
                return RelDir.R;
        }


        private DanSkeleton firstSkelInCycle
        {
            get
            {
                return capturedFrames[firstFrame];
            }
        }

        private DanSkeleton lastSkelInCycle
        {
            get
            {
                return capturedFrames[lastFrame];
            }
        }
        /*
         * TODO: DESCRIPTIVE ATTRIBTUES - write functions to calculate these and record them as class variables
         *       List is below
         */

        //percent of cycle in swing v. stance mode //TODO: need to find the location of all footfalls in cycle to know this

        //strideLength in meters 
        public double strideLengthMeters
        {
            get
            {
                return Utils3D.skelPointDist(firstSkelInCycle.Joints[JointType.FootRight].Position,
                    lastSkelInCycle.Joints[JointType.FootRight].Position);
            }
        }
        //strideLenght in time (number of frames)
        public int strideLengthFrames
        {
            get
            {
                return lastFrame - firstFrame;
            }
        }
        //velocity (combination of distance + frames)  note: learning alg should 
        //          be able to determine this implicitly, but the more info we give it the better  (can use to guess age!)
        public double strideMetersPerSecond
        {
            get
            {
                return (strideLengthMeters / (strideLengthFrames / 30.0)); //TODO: This assumes we never drop frames! 
                //IDEA: Could we record the wall clock time for all captured skeletons and use that? Or do we do well enough
                // that assuming 30 fps is ok?
            }
        }
        //step length (should be strideLength/2 but could calculate for right and left to see if there is a difference
        //TODO: need to know location of all footfalls

        //step length vs leg length (pg 43)
        //step width pg 41 (distance between feet between touchdown points) //TODO: Average?
        //foot separation (max and min)
        
        //arm motion/arc

        //averages and standard deviations of all point positions
        //averages and standard deviations of all distances between points
        //averages and standard deviations of all angles between lines between points


        //max height off of ground (probably just max head height)
        public double maxHeadHeightMeters
        {
            get
            {
                return capturedFrames.Max(x => x.Position.Y);
            }
        }
        //min height from ground (prob just min head height)
        public double minHeadHeightMeters
        {
            get
            {
                return capturedFrames.Min(x => x.Position.Y);
            }
        }

        //pelvic functions
          //pelvic rotation pg 4
          //pelvic list  pg 4p
       
        //knee flexion?? pg 4 (might not be able to measure this)

        //other rotations
          //rotations of thorax and shoulders
          //rotations of thigh and leg
          //rotations in the ankle and foot
         
        //foot angle

        //TODO: When should we get the angle? min angle, max angle? average? 

        private double angleBetweenJointPairs(Joint[] pair1, Joint[] pair2)
        {
            if (!pair1[1].Position.Equals(pair2[0].Position))
            {
                throw new ArgumentException("Joint Pairs must share a common base!");
            }
            Vector3D v1 = jointPairToVector3D(pair1);
            Vector3D v2 = jointPairToVector3D(pair2);
            return Vector3D.AngleBetween(v1, v2);
        }

        private Vector3D jointPairToVector3D(Joint[] pair)
        {
            return new Vector3D(
                pair[1].Position.X - pair[0].Position.X,
                pair[1].Position.Y - pair[0].Position.Y, 
                pair[1].Position.Z - pair[0].Position.Z);
        }
        
        //max foot elevation
        public double maxRFootHeight
        {
            get
            {
                return capturedFrames.Max(x => x.Joints[JointType.FootRight].Position.Y);
            }
        }
        public double maxLFootHeight
        {
            get
            {
                return capturedFrames.Max(x => x.Joints[JointType.FootLeft].Position.Y);
            }
        }
        public double maxFootHeightOverall
        {
            get
            {
                return Math.Max(this.maxLFootHeight, this.maxRFootHeight);
            }
        }
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
                // Color the skeletons based on if they are step skels
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
            // Put the camera in a place where it can see the whole stride
            skelViewport.Camera = new PerspectiveCamera(new Point3D(this.getStrideMidX(), 0, -3), new Vector3D(0,0,1), new Vector3D(0, 1, 0), 75);
            // Add a light so that colors are visible
            skelViewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });
        }

        // Return the middle coordinate of a recorded stride set
        private double getStrideMidX()
        {
            return (this.capturedFrames[0].Position.X + this.capturedFrames[this.capturedFrames.Count - 1].Position.X) / 2;
        }


        //private void renderA
        public int numFrames { get; set; }
    }
}
