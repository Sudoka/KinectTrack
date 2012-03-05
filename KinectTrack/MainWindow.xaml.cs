﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Windows.Media.Media3D;
using Petzold.Media3D;
//NOTE: Color is aliased here to avoid conflicting with the class of the same name in System.Windows.Media
using DColor = System.Drawing.Color;
using WMColor = System.Windows.Media.Color;


namespace KinectTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sensor;   //our sensor

        // Should the depth frame be drawn
        private bool drawDepthFrame = false;
        // Should the 3d skeleton frame be drawn
        private bool draw3dFrame = false;
        //Should we want to pring debug statements
        private bool debugging = false;

        //TODO: probably delete this?
        private FixedCapacityList<double> rkneelist;  //???  WHAT IS this for?

        // Bitmaps for the color and depth frames 
        WriteableBitmap colorBmp;
        WriteableBitmap depthBmp;
        //NOTE: this is a bit hacky, as I have hardcoded the sizes of the frames in order to lessen GC pauses
        Int32Rect colorFrameRect = new Int32Rect(0, 0, 640, 480);
        Int32Rect depthFrameRect = new Int32Rect(0, 0, 640, 480);

        //The skeleton frame that gets written to whenever the skeleton is in the frame
        List<Skeleton> skelList = new List<Skeleton>();

        // The skelList for use by the 3d display function
        List<DanSkeleton> copySkelList;

        // The skelList for testing normalized skeletons
        List<Skeleton> normSkelList = new List<Skeleton>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {

            if (KinectSensor.KinectSensors.Count < 1)
            {
                MessageBox.Show("You Should probably make sure the kinect is plugged in!\n");
                return;
            }
            try
            {
                sensor = KinectSensor.KinectSensors[0];  //initialize sensor 
                if (sensor == null)
                {
                    MessageBox.Show("Kinect Sensor Returned Null");
                    return;
                }
                //enable sensor streams
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                sensor.SkeletonStream.Enable();  //TODO: Should we add smoothing params?
                //declare new event handler
                sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                sensor.Start();

                rkneelist = new FixedCapacityList<double>(90);  //TODO: ??? 
            }
            catch
            {
                MessageBox.Show("Caught Something\n");
            }
        }

        /*
         * sensor_AllFramesReady() - event handler for when the kinect is ready to send us frames
         */

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Handle image frame
            using(ColorImageFrame colorFrame = e.OpenColorImageFrame()) 
            {
                if(colorFrame != null) 
                {
                    //DebugLabel.Content = "Woot, we got frames!";
                    byte[] pixels = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(pixels);

                    int stride = colorFrame.Width * 4; // Bytes per row in the image

                    if(colorBmp == null) {
                       colorBmp = new WriteableBitmap(BitmapSource.Create(colorFrame.Width,colorFrame.Height,96,96, PixelFormats.Bgr32, null, pixels, stride));
                    } else {
                        colorBmp.WritePixels(colorFrameRect, pixels, stride, 0);
                    }
                    visBox.Source = colorBmp;
                }
            }

            DepthImageFrame depthFrame = e.OpenDepthImageFrame();
            SkeletonFrame skelFrame = e.OpenSkeletonFrame();

            //NOTE: Ideally, these would be in using blocks, but that fact that skelToBitmap needs the depthframe complicates things
            //TODO: (to NOTE above - what about nested Using statements?) http://stackoverflow.com/questions/1329739/nested-using-statements-in-c-sharp
            if (depthFrame != null && drawDepthFrame)
            {
                byte[] pixels = generateColorPixels(depthFrame);

                int stride = depthFrame.Width * 4; // Bytes per row in the image

                if (depthBmp == null)
                {
                    depthBmp = new WriteableBitmap(BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride));
                }
                else
                {
                    depthBmp.WritePixels(depthFrameRect, pixels, stride, 0);
                }
                depthBox.Source = depthBmp;

                
            }
            if (skelFrame != null)
            {
                // Stuff goes here...
                Skeleton firstSkel = getFirstSkeleton(skelFrame);
                if (firstSkel != null)
                {
                    // Print the fun face on the image frame
                    Skeleton normSkel = normalizeSkel(firstSkel);
                    //testing for point indices  TODO:  not working on display, delete this
                    SkeletonPoint p = new SkeletonPoint();
                    p.X = .25F;
                    p.Y = .25F;
                    p.Z = .25F;
                    //Skeleton sSkel = shiftSkel(normSkel, p);
                    SkelToBitmap(firstSkel, depthFrame);
                    //SkelToBitmap(sSkel, depthFrame);   //seeing if we get the normalized skeleton
                    skelList.Add(firstSkel);
                    normSkelList.Add(normSkel);  //parallel list of normalized skeletons (smaller)

                    // Print some basic stats
                    double ankleToKneeRight = jointDistance(firstSkel.Joints[JointType.AnkleRight], firstSkel.Joints[JointType.KneeRight]); 
                    double ankleToKneeLeft = jointDistance(firstSkel.Joints[JointType.AnkleLeft], firstSkel.Joints[JointType.KneeLeft]);
                    // Stupid .NET uses a different syntax for format strings for no discernable reason
                    String s = String.Format("Ankle to right knee dist = {0,5:f} Ankle to left knee dist = {1,5:f} \n", ankleToKneeRight, ankleToKneeLeft);

                    double kneeToHipRight = jointDistance(firstSkel.Joints[JointType.KneeRight], firstSkel.Joints[JointType.HipRight]); 
                    double kneeToHipLeft = jointDistance(firstSkel.Joints[JointType.KneeLeft], firstSkel.Joints[JointType.HipLeft]);
                    String s2 = String.Format("Knee to Hip right = {0,5:f} Knee to Hip left = {1,5:f} \n", kneeToHipRight, ankleToKneeLeft);


                    rkneelist.addElement(ankleToKneeRight);
                    string s3 = String.Format("Average of ankle to knee is {0,5:f}", rkneelist.average());
                    //NOTE: Stringbuilder needed?
                    skelInfo.Text = s + s2 + s3;
                }
            }

            // Dispose of skelFrame and depthFrame
            if (skelFrame != null) skelFrame.Dispose();
            if (depthFrame != null) depthFrame.Dispose();
        }

        /// <summary>
        /// Returns the Euclidean distance between joints. Only makes sense when called with adjacent points
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="joint2"></param>
        /// <returns></returns>
        private double jointDistance(Joint j, Joint j2)
        {
            double x = j.Position.X - j2.Position.X;
            double y = j.Position.Y - j2.Position.Y;
            double z = j.Position.Z - j2.Position.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        Skeleton getFirstSkeleton(SkeletonFrame frame)
        {
            Skeleton[] closet = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(closet);
            Skeleton first = (from s in closet where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();  //nifty
            return first;
        }

        /// <summary>
        /// This function generates some pixel data from a given DepthImageFrame
        /// </summary>
        /// <param name="depthFrame"></param>
        /// <returns></returns>
        private byte[] generateColorPixels(DepthImageFrame depthFrame)
        {
            short[] rawData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawData);

            byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            for (int depthIndex = 0, colorIndex = 0;
                depthIndex < rawData.Length && colorIndex < pixels.Length;
                depthIndex++, colorIndex += 4)
            {
                // Get player (0 if no player, 1-6 otherwise)
                int player = rawData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                // Get depth value
                // This is in mm
                // range for depth is 800-4000mm (2.5 feet - 13.1 feet) in normal mode
                int depth = rawData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                //Note that we only need to write to the non-zero indices (probably)
                // NOTE: setPixelColor is likely to be slow when called with non-static colors
                if (depth <= 900)
                {
                    pixels.setPixelColor(colorIndex, System.Drawing.Color.Blue);
                    //pixels[colorIndex + Utils.BlueIndex] = 255;
                    //pixels[colorIndex + Utils.GreenIndex] = 0;
                    //pixels[colorIndex + Utils.RedIndex] = 0;
                }
                if (depth > 900 && depth <= 2000)
                {
                    pixels.setPixelColor(colorIndex, System.Drawing.Color.Red);
                   // pixels[colorIndex + Utils.BlueIndex] = 0;
                    //pixels[colorIndex + Utils.GreenIndex] = 255;
                    //pixels[colorIndex + Utils.RedIndex] = 0;
                }
                if (depth > 2000)
                {
                    pixels.setPixelColor(colorIndex, System.Drawing.Color.CadetBlue);
                    //pixels[colorIndex + Utils.BlueIndex] = 0;
                    //pixels[colorIndex + Utils.GreenIndex] = 0;
                    //pixels[colorIndex + Utils.RedIndex] = 255;
                }
            }
            return pixels;
        }

        private void SkelToBitmap(Skeleton skel, DepthImageFrame dFrame)
        {

            if (dFrame != null)
            {
                DepthImagePoint headDepthPoint = dFrame.MapFromSkeletonPoint(skel.Joints[JointType.Head].Position);
                ColorImagePoint headColorPoint = dFrame.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);

                moveFaceToPoint(headColorPoint);
            }
        }

        private void moveFaceToPoint(ColorImagePoint headColorPoint)
        {
            var marg = visBox.Margin;
            var visLeft = marg.Left;
            var visTop = marg.Top;
            // NOTE: This is hacked up
            // NOTE: note that we have to divide the values in headColorPoint by 2 since we are displaying the images at 1/2 resolution
            // TODO: determine if there is a way to do this without creating a new object on every call!
            faceImage.Margin = new Thickness(visLeft + (headColorPoint.X/2) - (faceImage.Width/2), visTop + (headColorPoint.Y/2) - (faceImage.Height /2), 0, 0);

        }

        /*
         * normalizeSkel - normalizes a skeleton such that all points are treated as vectors and normalized as such: each point has its x, y, and z
         *                  coordinate divided by the length of the vector for that point.  Returns a new skeleton with normalized components.
         */
        private Skeleton normalizeSkel(Skeleton skelly){
            if (debugging == true)
            {
                System.Console.Write("Value of head.x before normalization\t" + skelly.Joints[JointType.Head].Position.X + "\n");
                System.Console.Write("Value of Skel.Position before normalization\t" + skelly.Position.X + "\n");
            }
            DanSkeleton newSkelly = new DanSkeleton(skelly);
            newSkelly.normalize();
            if (debugging == true)
            {
                System.Console.Write("Value of head.x after normalization\t" + newSkelly.Joints[JointType.Head].Position.X + "\n");
                System.Console.Write("Value of Skel.Position after normalization\t" + newSkelly.Position.X + "\n");
            }
            return newSkelly;   //polymorphism boosh!
        }


        /*
         * shiftSkel-  shifts skeleton from its position to this new position
         * param:
         *   SkeletonPoint newCenter - the new point around which to center the skeleton 
         *   Skeleton skelly - the skeleton to shift
         */
        private Skeleton shiftSkel(Skeleton skelly, SkeletonPoint newCenter)
        {

            if (debugging == true)
            {
                System.Console.Write("Value of head.x before shift\t" + skelly.Joints[JointType.Head].Position.X + "\n");
                System.Console.Write("Value of Skel.Position before shift\t" + skelly.Position.X + "\n");
            }
            //figure out the values by which to shift
            //based on ORIGINAL SKELETON POS - NEW SKELETON POS 
            float xAmount, yAmount, zAmount;
            xAmount = skelly.Position.X - newCenter.X;
            yAmount = skelly.Position.Y - newCenter.Y;
            zAmount = skelly.Position.Z - newCenter.Z;
            //shift it
            DanSkeleton newSkelly = new DanSkeleton(skelly);
            newSkelly.shift(xAmount, yAmount, zAmount);
            if (debugging == true)
            {
                System.Console.Write("Value of head.x after shift\t" + newSkelly.Joints[JointType.Head].Position.X + "\n");
                System.Console.Write("Value of Skel.Position after shift\t" + newSkelly.Position.X + "\n");
            }
            return newSkelly;   //polymorphism boosh!
        }
       


        private void tiltButton_Click(object sender, RoutedEventArgs e)
        {
            sensor.ElevationAngle = (int)tiltSlider.Value.Clamp(sensor.MinElevationAngle, sensor.MaxElevationAngle);
        }

        //What?
        private void visBox_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void depthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            drawDepthFrame = !drawDepthFrame;
        }

        private void depthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            drawDepthFrame = !drawDepthFrame;
        }

        private void grabSkelList_Click(object sender, RoutedEventArgs e)
        {
            Stride test = new Stride(skelList);
            //TODO: add null checks and stuff here
            copySkelList = new List<DanSkeleton>();

            foreach(Skeleton s in skelList) 
            {
                DanSkeleton d = new DanSkeleton(s);
                copySkelList.Add(d);
            }
            // Set up the slider
            skelSlider.Minimum = 0;
            skelSlider.Maximum = copySkelList.Count;
            skelSlider.IsSnapToTickEnabled = true;

            // Find step locations
            List<double> leftFootPosDiffs = new List<double>();
            // Get distance between all left foot positions
            for(int skelIndex = 0; skelIndex < copySkelList.Count - 1; skelIndex++) {
                Skeleton curSkel = copySkelList[skelIndex];
                Skeleton nextSkel = copySkelList[skelIndex + 1];
                double curDist = Utils3D.skelPointDist(curSkel.Joints[JointType.FootLeft].Position
                    ,nextSkel.Joints[JointType.FootLeft].Position);
                leftFootPosDiffs.Add(curDist);
            }
            //NOTE: this is only a guess at a good value for epsilon
            double epsilon = .01;

            // Find all step diffs smaller than epsilon (i.e. those likely to be points where the foot is down)
            List<int> stepFrames = new List<int>();
            for (int i = 0; i < leftFootPosDiffs.Count; i++)
            {
                double curDiff = leftFootPosDiffs[i];
                if (curDiff < epsilon)
                {
                    stepFrames.Add(i);
                }
            }

            //Coalesce the stepframes into single frames and ignore outliers (i.e. there must be more than one in a row
            // for it to be a valid foot down position)
            List<int> realStepFrames = new List<int>();
            for (int i = 0; i < stepFrames.Count; i++)
            {
                int iStart = i;
                while (i < stepFrames.Count-1 && stepFrames[i + 1] == (stepFrames[i] + 1))
                {
                    i++;
                }
                if (Math.Abs(i - iStart) > 1) realStepFrames.Add(iStart);
            }
            
            // Set those skeletons as step skeletons
            foreach (int frameNumber in realStepFrames)
            {
                copySkelList[frameNumber].isStepSkel = true;
            }
        }

        private void skelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Get the slider frame number
            int frameNumber = (int)e.NewValue;
            //Clear the viewport
            skelViewport.Children.Clear();
            renderSkeleton(copySkelList[frameNumber.Clamp(0,copySkelList.Count-1)]);
        }

        private void renderSkeleton(DanSkeleton renderSkel)
        {
            Model3DGroup skel3dGroup = new Model3DGroup();
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
                this.skelViewport.Children.Add(curJointCube);
            }
            // Put the camera in the position of the kinect (more or less)
            this.skelViewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -3), new Vector3D(renderSkel.Position.X, renderSkel.Position.Y, renderSkel.Position.Z), new Vector3D(0, 1, 0), 75);
            // Add a light so that colors are visible
            this.skelViewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

        }

        private void DEBUG_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
