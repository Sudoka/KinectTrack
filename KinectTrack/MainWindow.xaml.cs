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

namespace KinectTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sensor;   //our sensor

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
                        colorBmp.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height), pixels, stride, 0);
                    }
                    visBox.Source = colorBmp;
                }
            }

            DepthImageFrame depthFrame = e.OpenDepthImageFrame();
            SkeletonFrame skelFrame = e.OpenSkeletonFrame();

            if (depthFrame != null)
            {
                byte[] pixels = generateColorPixels(depthFrame);

                int stride = depthFrame.Width * 4; // Bytes per row in the image

                if (depthBmp == null)
                {
                    depthBmp = new WriteableBitmap(BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride));
                }
                else
                {
                    depthBmp.WritePixels(new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height), pixels, stride, 0);
                }
                depthBox.Source = depthBmp;
                
            }
            if (skelFrame != null)
            {
                // Stuff goes here...
                Skeleton firstSkel = getFirstSkeleton(skelFrame);
                if (firstSkel != null)
                {
                    SkelToBitmap(firstSkel, depthFrame);
                }

            }
            //declare and initialize color, depth and skeleton objects
            //ColorImageFrame colorFrame = (ColorImageFrame) e.OpenColorImageFrame();
            //DepthImageFrame depthFrame = (DepthImageFrame)e.OpenDepthImageFrame();
            //SkeletonFrame skelFrame = (SkeletonFrame)e.OpenSkeletonFrame();
/*
            if (colorFrame != null)
            {
                Bitmap visBoxBMap = ImageToBitmap(colorFrame);
                visBox.= visBoxBMap;
            }
            if (depthFrame != null)
            {
                Bitmap depBoxBMap = DepthToBitmap(depthFrame);
                if (depBoxBMap !=null)
                {
                    depBox.Image = depBoxBMap;
                }
            }
            if (skelFrame != null)
            {
                //Bitmap skelBoxBMap = SkelToBitmap(skelFrame);
                //skelBox.Image = skelBoxBMap;
                SkelToBitmap(skelFrame, e);
            }
            */
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
                int depth = rawData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth <= 900)
                {
                    pixels.setPixelColor(colorIndex, System.Drawing.Color.Blue);
                }
                if (depth > 900 && depth <= 2000)
                {
                    pixels.setPixelColor(colorIndex, System.Drawing.Color.LawnGreen);
                }
                if (depth > 2000)
                {
                    pixels.setPixelColor(colorIndex, System.Drawing.Color.Red);
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
            // TODO: This is hacked up
            faceImage.Margin = new Thickness(visLeft + (headColorPoint.X/2) - (faceImage.Width/2), visTop + (headColorPoint.Y/2) - (faceImage.Height /2), 0, 0);

        }

        private void moveElementToPoint(FrameworkElement element, ColorImagePoint point)
        {
//            Canvas.SetLeft(element, point.X - element.Width / 2);
 //           Canvas.SetTop(element, point.Y - element.Height / 2);
        }

        void addPointToBitMap(ColorImagePoint colorPoint)
        {
            /*
            Bitmap bmp = (Bitmap)visBox.Image;
            // Sometimes the colorPoint values are out of the legal range of the bmp
            int maxX = bmp.Width;
            int maxY = bmp.Height;
            bmp.SetPixel(Utils.constrainInt(colorPoint.X, 0, maxX), Utils.constrainInt(colorPoint.Y, 0, maxY), Color.Lime);  //update TO DO
            return;
            */
        }

        Bitmap ImageToBitmap(ColorImageFrame Image)
        {
            /*
            byte[] pixeldata = new byte[Image.PixelDataLength];
            Image.CopyPixelDataTo(pixeldata);
            Bitmap bmap = new Bitmap(Image.Width, Image.Height, PixelFormat.Format32bppRgb);
            BitmapData bmapdata = bmap.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly, bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixeldata, 0, ptr, Image.PixelDataLength);
            bmap.UnlockBits(bmapdata);
            return bmap;
            */
            return null;
        }
        /*
         *depthImageToBitmap - 
         */
        /* Bitmap ImageToBitmap(DepthImageFrame depthFrame)
         {
              //get the raw data from the kinect with depth for every pixel
             short[] rawDepthData = new short[depthFrame.PixelDataLength];
             depthFrame.CopyPixelDataTo(rawDepthData);

             //use the depthFrame to create the image to display on screen
             //depthFrame contains color information for all pixels in image
             //height x width x 4 (R, G, B, empty byte)
             Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

             //initialize rgb constants
             //hard coded locations for b, g, r
             const int BlueIndex = 0;
             const int GreenIndex = 1;
             const int RedIndex = 2;

             //loop through all distances
             //pick a rgb value based on distance
             for(int depthIndex=0, colorIndex=0; (depthIndex<rawDepthData.Length
                 && colorIndex < pixels.Length); depthIndex++, colorIndex+=4){
                     //get the player
                     int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                     //get depth
                     int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    
                 if (depth <= 900)
                     {   //super close
                         pixels[colorIndex + BlueIndex] = 255;
                         pixels[colorIndex + GreenIndex] = 0;
                         pixels[colorIndex + RedIndex] = 0;
                     }


                     if (depth > 900 && depth < 2000)
                     {   //super close
                         pixels[colorIndex + BlueIndex] = 0;
                         pixels[colorIndex + GreenIndex] = 255;
                         pixels[colorIndex + RedIndex] = 0;
                     }

                     if (depth > 2000)
                     {   //super close
                         pixels[colorIndex + BlueIndex] = 0;
                         pixels[colorIndex + GreenIndex] = 0;
                         pixels[colorIndex + RedIndex] = 255;
                     }
                 }

             MemoryStream stream = new MemoryStream(pixels);
   
                  //   BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels2, stride2);
             ImageConverter convert = new ImageConverter();
            
             Bitmap bmp = new Bitmap();
             bmp.
             return bmp;
         } */

        Bitmap DepthToBitmap(DepthImageFrame imageFrame)
        {
            /*
            short[] pixelData = new short[imageFrame.PixelDataLength];
            imageFrame.CopyPixelDataTo(pixelData);

            Bitmap bmap = new Bitmap(
            imageFrame.Width,
            imageFrame.Height,
            PixelFormat.Format16bppRgb555);

            BitmapData bmapdata = bmap.LockBits(
             new Rectangle(0, 0, imageFrame.Width,
                                    imageFrame.Height),
             ImageLockMode.WriteOnly,
             bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(pixelData,
             0,
             ptr,
             imageFrame.Width *
               imageFrame.Height);
            bmap.UnlockBits(bmapdata);
            return bmap;
            */
            return null;
        }

        private void tiltButton_Click(object sender, RoutedEventArgs e)
        {
            sensor.ElevationAngle = (int)tiltSlider.Value.Clamp(sensor.MinElevationAngle, sensor.MaxElevationAngle);
        }

        private void visBox_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}
