﻿using System;
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
        public bool isStepSkel = false;

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
            SkeletonPoint newPosition;
            for (int i = 0; i < this.Joints.Count; i++)
            {
                // Clone the joint
                Joint j = this.Joints[(JointType)i].DeepClone();
                // Create a new position for that joint
                newPosition = new SkeletonPoint();
                // Set the position values
                newPosition.X = j.Position.X - xAmount;
                newPosition.Y = j.Position.Y - yAmount;
                newPosition.Z = j.Position.Z - zAmount;
                // And set the position for the joint to be the right thing
                j.Position = newPosition;
                // Put it in the new joint array
                ja[i] = j;
            }
            newPosition = new SkeletonPoint();
            newPosition.X = this.Position.X - xAmount;
            newPosition.Y = this.Position.Y - yAmount;
            newPosition.Z = this.Position.Z - zAmount;
            // NOTE: Ideally, we could just do something like this.Joints[curJoint].Position = blah
            // But the JointCollection class doesn't want to let us. So we have to force it with Reflection
            // I've done some testing and this seems not to break anything, but I'm sure the performance isn't great
            typeof(JointCollection).GetField("_skeletonData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Joints, ja);
            this.Position = newPosition;
        }

        private void setJointCollection(Joint[] jointData)
        {
            // NOTE: Ideally, we could just do something like this.Joints[curJoint].Position = blah
            // But the JointCollection class doesn't want to let us. So we have to force it with Reflection
            // I've done some testing and this seems not to break anything, but I'm sure the performance isn't great
            typeof(JointCollection).GetField("_skeletonData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Joints, jointData);
        }

        private Joint[] getJointsCopy()
        {
            Joint[] jc = new Joint[20];
            for (int i = 0; i < this.Joints.Count; i++)
            {
                // Clone the joint
                Joint j = this.Joints[(JointType)i].DeepClone();
                // Put it in the new joint array
                jc[i] = j;
            }
            return jc;
        }

        //TODO: rotate the position of the skelton as well as the joints
        public void rotateJointsXZ(double xCenter, double zCenter, double angle)
        {
            Joint[] rotatedJoints = new Joint[20];//getJointsCopy();
            for(int i = 0; i < rotatedJoints.Length; i++)
            {
                Joint curJoint = this.Joints[(JointType)i].DeepClone();
                //Joint curJoint = rotatedJoints[i];
                // Translate to origin
                double cX = curJoint.Position.X - xCenter;
                // Z = Y!
                double cZ = curJoint.Position.Z - zCenter;
                // Calculate sines and cosines
                double sinAngle = Math.Sin(angle);
                double cosAngle = Math.Cos(angle);

                // do rotation
                double xRot = cX * cosAngle - cZ * sinAngle;
                double zRot = cX * sinAngle + cZ * cosAngle;

                double newX = xRot + xCenter;
                double newZ = zRot + zCenter;


                /*
                // The radius of the polar coordinate for this point
                double r = Math.Sqrt((cX * cX) + (cZ * cZ));
                // The angle of the current point (have to convert to degrees since
                // the Vector class expresses differences in degrees and the trig functions
                // return radian results
                double a = Utils.radiansToDegrees(Math.Atan(cZ / cX));

                // Add teh new angle 
                double newA = a + angle;

                // translate back to the original points and convert to cartesian coordinates
                double newX = r*Math.Cos(newA) + xCenter;
                double newZ = r*Math.Sin(newA) + zCenter;
                */
                SkeletonPoint rotPos = new SkeletonPoint();
                rotPos.Y = curJoint.Position.Y;
                rotPos.X = (float)newX;
                rotPos.Z = (float)newZ;
                curJoint.Position = rotPos;
                rotatedJoints[i] = curJoint;
            }
            typeof(JointCollection).GetField("_skeletonData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Joints, rotatedJoints);
            //setJointCollection(rotatedJoints);
        }
        //TODO: write other useful functions here (scaling, etc...)
    }
}
