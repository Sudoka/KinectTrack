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
        //TODO: write other useful functions here (scaling, etc...)
    }
}
