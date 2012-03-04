using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Reflection;

namespace KinectTrack
{
    /// <summary>
    /// Skeletons with Assignable Joint Collections!
    /// </summary>
    class DanSkeleton : Skeleton
    {
        // Copy a normal skeleton into a DanSkeleton
        public DanSkeleton(Skeleton s)
            : base()
        {
            this.Joints = s.Joints.DeepClone();
            this.Position = s.Position.DeepClone();
            this.TrackingId = s.TrackingId;
            this.TrackingState = s.TrackingState.DeepClone();
        }

        public void multiplyJoints(float val)
        {
            // NOTE: This function uses deep dark voodoo. 
            // 
            // I miss C++ sometimes

            Joint[] ja = new Joint[20];
            for (int i = 0; i < this.Joints.Count; i++)
            {
                // Clone the joint
                Joint j = this.Joints[(JointType)i].DeepClone();
                // Create a new position for that joint
                SkeletonPoint newPosition = new SkeletonPoint();
                // Set the position values
                newPosition.X = j.Position.X * val;
                newPosition.Y = j.Position.Y * val;
                newPosition.Z = j.Position.Z * val;
                // And set the position for the joint to be the right thing
                j.Position = newPosition;
                // Put it in the new joint array
                ja[i] = j;
            }
            // NOTE: Ideally, we could just do something like this.Joints[curJoint].Position = blah
            // But the JointCollection class doesn't want to let us. So we have to force it with Reflection
            // I've done some testing and this seems not to break anything, but I'm sure the performance isn't great
            typeof(JointCollection).GetField("_skeletonData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Joints, ja);
        }

        /*
         * normalizeSkel() - return this skeleton with normalized positions
         */

        public void normalize()
        {
            // NOTE: This function uses deep dark voodoo. 
            // 
            // Paul misses C++ sometimes
            double x, y, z, magnitude;
            SkeletonPoint newPosition;
            Joint[] ja = new Joint[20];
            for (int i = 0; i < this.Joints.Count; i++)
            {
                // Clone the joint
                Joint j = this.Joints[(JointType)i].DeepClone();
                // Create a new position for that joint
                newPosition = new SkeletonPoint();
                //find magnitude
                x = j.Position.X;
                y = j.Position.Y;
                z = j.Position.Z;
                magnitude = Math.Sqrt(x * x + y * y + z * z);
                // Set the position values
                newPosition.X = (float) (x /magnitude);
                newPosition.Y = (float) (y / magnitude);
                newPosition.Z = (float) (z/ magnitude);
                // And set the position for the joint to be the right thing
                j.Position = newPosition;
                // Put it in the new joint array
                ja[i] = j;
            }
            //and once more for the "Position"
            newPosition = new SkeletonPoint();
            x = this.Position.X;
            y = this.Position.Y;
            z = this.Position.Z;
            magnitude = Math.Sqrt(x * x + y * y + z * z);
            newPosition.X = (float)(x / magnitude);
            newPosition.Y = (float)(y / magnitude);
            newPosition.Z = (float)(z / magnitude);
            this.Position = newPosition;


            // NOTE: Ideally, we could just do something like this.Joints[curJoint].Position = blah
            // But the JointCollection class doesn't want to let us. So we have to force it with Reflection
            // I've done some testing and this seems not to break anything, but I'm sure the performance isn't great
            typeof(JointCollection).GetField("_skeletonData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Joints, ja);
            
        }


        /*
         * shiftSkel() - moves all of the points in this skeleton over by a certain amount
         * params:
         *   float xAmount - shift all x values by this amount
         *   float yAmount - shift all y values by this amount
         *   float zAmount - shift all z values by this amount
         */

        public void shift(float xAmount, float yAmount, float zAmount)
        {
            // NOTE: This function uses deep dark voodoo. 
            // 
            // Paul misses C++ sometimes
            Joint[] ja = new Joint[20];
            for (int i = 0; i < this.Joints.Count; i++)
            {
                // Clone the joint
                Joint j = this.Joints[(JointType)i].DeepClone();
                // Create a new position for that joint
                SkeletonPoint newPosition = new SkeletonPoint();
                // Set the position values
                newPosition.X = j.Position.X + xAmount;
                newPosition.Y = j.Position.Y + yAmount;
                newPosition.Z = j.Position.Z + zAmount;
                // And set the position for the joint to be the right thing
                j.Position = newPosition;
                // Put it in the new joint array
                ja[i] = j;
            }
            // NOTE: Ideally, we could just do something like this.Joints[curJoint].Position = blah
            // But the JointCollection class doesn't want to let us. So we have to force it with Reflection
            // I've done some testing and this seems not to break anything, but I'm sure the performance isn't great
            typeof(JointCollection).GetField("_skeletonData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Joints, ja);
        }
        //TODO: write other useful functions here (scaling, etc...)
    }
}
