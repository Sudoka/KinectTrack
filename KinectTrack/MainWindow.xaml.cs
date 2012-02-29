using System;
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
//NOTE: Color is aliased here to avoid conflicting with the class of the same name in System.Windows.Media
using DColor = System.Drawing.Color;

namespace KinectTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sensor;   //our sensor

        private bool drawDepthFrame = false; 

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
                sensor.SkeletonStream.Enable();  //add smoothing parameters?  TO DO
                //declare new event handler
                sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                sensor.Start();
            }
            catch
            {
                MessageBox.Show("Caught Something\n");
            }
        }

        WriteableBitmap colorBmp;
        WriteableBitmap depthBmp;
        //NOTE: this is a bit hacky, as I have hardcoded the sizes of the frames
        Int32Rect colorFrameRect = new Int32Rect(0, 0, 640, 480);
        Int32Rect depthFrameRect = new Int32Rect(0, 0, 320, 240);

        StringBuilder sb = new StringBuilder();

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Handle image frame
            using(ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
                if(colorFrame != null) {
                    DebugLabel.Content = "Woot, we got frames!";
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
                    SkelToBitmap(firstSkel, depthFrame);
                    // Print some basic stats
                    double ankleToKneeRight = jointDistance(firstSkel.Joints[JointType.AnkleRight], firstSkel.Joints[JointType.KneeRight]); 
                    double ankleToKneeLeft = jointDistance(firstSkel.Joints[JointType.AnkleLeft], firstSkel.Joints[JointType.KneeLeft]);
                    /*
                    sb.Append("Ankle to right knee dist = ");
                    sb.Append(ankleToKneeRight);
                    sb.Append("\n Ankle to left knee dist = ");
                    sb.Append(ankleToKneeLeft);
                    */
                    // Stupid .NET uses a different syntax for format strings for no discernable reason
                    String s = String.Format("Ankle to right knee dist = {0,5:f} \nAnkle to left knee dist = {1,5:f}", ankleToKneeRight, ankleToKneeLeft);
                    skelInfo.Text = s;
                    sb.Clear();
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
            Skeleton[] skelArray = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(skelArray);
            Skeleton first = (from s in skelArray where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
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
        
    }
}
