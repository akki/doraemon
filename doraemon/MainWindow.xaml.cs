//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using AForge.Imaging;
    using AForge.Imaging.Filters;
    using AForge.Math.Geometry;
    using AForge;
    //using Microsoft.Kinect.Toolkit;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "In a full-fledged application, the SpeechRecognitionEngine object should be properly disposed. For the sake of simplicity, we're omitting that code in this sample.")]
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private KinectSensor sensor;
        string command = null;
        //string dir = null;
        WriteableBitmap depthBitmap;
        WriteableBitmap colorBitmap;
        DepthImagePixel[] depthPixels;
        IntRange bottleH = new IntRange(164, 250);
        Range bottleS = new Range(0.6f, 1);
        Range bottleL = new Range(0.6f, 1);
        IntRange boxH = new IntRange(89, 130);
        Range boxS = new Range(0.6f, 1);
        Range boxL = new Range(0.6f, 1);
        IntRange botH = new IntRange(0, 20);
        Range botS = new Range(0.6f, 1);
        Range botL = new Range(0.6f, 1);
        int xf = 0;
        int[] objec = new int[3];
        int[] bot = new int[3];
        byte[] colorPixels;
        int path = 0;
        int[] humanPosition;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Execute initialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            objec[0] = -1;
            objec[1] = -1;
            objec[2] = -1;
            bot[0] = 0;
            bot[1] = 0;
            bot[2] = 0;

            if (null != this.sensor)
            {
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.sensor.SkeletonStream.Enable();
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.depthBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                this.img.Source = this.colorBitmap;
                this.sensor.AllFramesReady += sensor_AllFramesReady;

                try
                {
                    // Start the sensor!
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
                return;
            }

            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {
                //recognitionSpans = new List<Span> { forwardSpan, backSpan, rightSpan, leftSpan };

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);


                // Create a grammar from grammar definition XML file.
                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    speechEngine.LoadGrammar(g);
                }

                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += SpeechRejected;


                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                this.statusBarText.Text = Properties.Resources.NoSpeechRecognizer;
            }
        }

        void sensor_AllFramesReady(object sender, Microsoft.Kinect.AllFramesReadyEventArgs e)
        {
            if(this.command == "Stop")
            {
                Bot.stop();
            }
            if(this.command == "Forward")
            {
                Bot.traverse();
            }
            if (this.command == "Right")
            {
                Bot.turnRight();
            }
            if (this.command == "Left")
            {
                Bot.turnRight();
            }
            xf++;
            if (xf % 5 == 0)
            {
                xf = 0;
                if (this.command != null)
                {
                    using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                    {
                        using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                        {
                            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                            {
                                humanPosition = frameToHuman(skeletonFrame);

                                if (colorFrame != null)
                                {
                                    // Copy the pixel data from the image to a temporary array
                                    colorFrame.CopyPixelDataTo(this.colorPixels);

                                    // Write the pixel data into our bitmap
                                    this.colorBitmap.WritePixels(
                                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                                        this.colorPixels,
                                        this.colorBitmap.PixelWidth * sizeof(int),
                                        0);

                                    // Error here due to OpenCV_core290.dll
                                    //int[] objPos = new int[2];
                                    //objPos = tmp.matchColor(ImageProc.colorFrameToImage(colorFrame));
                                    //if (objPos != null)
                                    //{
                                    //    short blobDepth = getDepthAtPoint(objPos, depthFrame);
                                    //    this.lblObject.Content = objPos[0] + ", " + objPos[1] + ", " + blobDepth;
                                    //}
                                    //else
                                    //{
                                    //    this.lblObject.Content = "Null";
                                    //}
                                    System.Drawing.Bitmap bmp = ImageProc.colorFrameToAforge(colorFrame);
                                    HSLFiltering filter = new HSLFiltering();
                                    // set color ranges to keep
                                    if (objec[0] == -1)
                                    {
                                        if (command == "Fetching Bottle")
                                        {
                                            filter.Hue = bottleH;
                                            filter.Saturation = bottleS;
                                            filter.Luminance = bottleL;
                                        }
                                        else if (command == "Fetching Box")
                                        {
                                            filter.Hue = boxH;
                                            filter.Saturation = boxS;
                                            filter.Luminance = boxL;
                                        }

                                        //// apply the filter
                                        filter.ApplyInPlace(bmp);

                                        BlobCounter blobCounter = new BlobCounter(bmp);
                                        int i = blobCounter.ObjectsCount;
                                        ExtractBiggestBlob fil = new ExtractBiggestBlob();

                                        int[] pp = new int[2];
                                        pp[0] = 0;
                                        pp[1] = 0;
                                        int h = 0;
                                        if (i > 0)
                                        {
                                            fil.Apply(bmp);
                                            pp[0] = fil.BlobPosition.X;
                                            pp[1] = fil.BlobPosition.Y;

                                            h = fil.Apply(bmp).Height;
                                        }

                                        short blobDepth = getDepthAtPoint(pp, depthFrame);
                                        this.lblObject.Content = pp[0] + ", " + pp[1] + ", " + blobDepth;
                                        this.objec[0] = pp[0];
                                        this.objec[1] = pp[1];
                                        this.objec[2] = blobDepth;
                                    }
                                    else
                                    {
                                        filter.Hue = botH;
                                        filter.Saturation = botS;
                                        filter.Luminance = botL;
                                        filter.ApplyInPlace(bmp);

                                        BlobCounter blobCounter = new BlobCounter(bmp);
                                        int i = blobCounter.ObjectsCount;
                                        ExtractBiggestBlob fil = new ExtractBiggestBlob();

                                        int[] pp = new int[2];
                                        pp[0] = 0;
                                        pp[1] = 0;
                                        int h = 0;
                                        if (i > 0)
                                        {
                                            fil.Apply(bmp);
                                            pp[0] = fil.BlobPosition.X;
                                            pp[1] = fil.BlobPosition.Y;

                                            h = fil.Apply(bmp).Height;
                                        }

                                        short blobDepth = getDepthAtPoint(pp, depthFrame);
                                        this.lblBot.Content = pp[0] + ", " + pp[1] + ", " + blobDepth;
                                        this.bot[0] = pp[0];
                                        this.bot[1] = pp[1];
                                        this.bot[2] = blobDepth;
                                    }

                                    //Assign Manual Position to bot and object
                                }
                                if (humanPosition != null)
                                {
                                    this.lblHuman.Content = humanPosition[0] + ", " + humanPosition[1] + ", " + humanPosition[2];
                                }
                                else
                                {
                                    this.lblHuman.Content = "No Human detected";
                                }

                                if (this.path == 0)
                                {
                                    if (humanPosition != null)
                                    {
                                        if (Bot.moveDoraemon(this.bot[0], this.humanPosition[0], this.bot[2], this.humanPosition[2]) == 0)
                                        {
                                            this.path = 1;
                                        }  
                                    }
                                }
                                else
                                {
                                    if (Bot.moveDoraemon(this.bot[0], this.objec[0], this.bot[2], this.objec[2]) == 0)
                                    {
                                        Bot.stop();
                                    } 
                                }
                            }
                        }
                    }
                    this.lbl.Content = command;
                } 
            }
        }

        /// <summary>
        /// Execute uninitialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "BOTTLE":
                        this.command = "Fetching Bottle";
                        break;

                    case "BOX":
                        this.command = "Fetching Box";
                        break;
                    case "STOP":
                        this.command = "Stop";
                        break;
                    case "LEFT":
                        this.command = "Left";
                        break;
                    case "RIGHT":
                        this.command = "Right";
                        break;
                    case "MOVE":
                        this.command = "Forward";
                        break;
                }
            }
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {

        }


        int[] frameToHuman(SkeletonFrame frame)
        {
            int[] pos = new int[3];
            Skeleton[] skeletons = null;
            if (frame != null)
            {
                if (skeletons == null)
                {
                    // Allocate array of skeletons
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                }

                // Copy skeletons from this frame
                frame.CopySkeletonDataTo(skeletons);

                // Find first tracked skeleton, if any
                Skeleton skeleton = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();

                if (skeleton != null)
                {
                    // Obtain the left knee joint; if tracked, print its position
                    Joint j = skeleton.Joints[JointType.KneeLeft];

                    if (j.TrackingState == JointTrackingState.Tracked)
                    {
                        pos[2] = (int)(j.Position.Z * 1000);
                        pos[0] = (int)(640 * (j.Position.X + 2.0) / 4.0) + (int)(640 / 2);
                        pos[1] = (int)(480 * (j.Position.Y + 2.0) / 4.0) + (int)(480.0 / 2.0);
                        return pos;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        // Returns depth at a pixel position from a depth frame
        short getDepthAtPoint(int[] position, DepthImageFrame frame)
        {
            if (frame != null)
            {
                frame.CopyDepthImagePixelDataTo(this.depthPixels);
                int x =  depthPixels.Length;
                int pix = 640 * (position[1]) + position[0];
                short depth = depthPixels[pix].Depth;
                return depth;
            }
            else
            {
                return 0;
            }
        }
    }
}