//------------------------------------------------------------------------------
// <copyright file="speechRecognizer.cs" organisation="NebulaX">
//     Copyright (c) NebulaX.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// add reference : 
// Assembly Microsoft.Speech
// C:\Windows\assembly\GAC_MSIL\Microsoft.Speech\11.0.0.0__31bf3856ad364e35\Microsoft.Speech.dll

// IMPORTANT: This sample requires the Speech Platform SDK (v11) to be installed on the developer workstation

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doraemon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;

    public class SpeechRecognizer : IDisposable
    {
        private readonly Dictionary<string, WhatSaid> thingNames = new Dictionary<string, WhatSaid>
            {
                { "Bottle", new WhatSaid { Verb = Verbs.Bottle } },
                { "Hard Drive", new WhatSaid { Verb = Verbs.Drive } },
                { "Drive", new WhatSaid { Verb = Verbs.Drive } },
                { "Pen", new WhatSaid { Verb = Verbs.Pen } },
            };

        private readonly Dictionary<string, WhatSaid> commandNames = new Dictionary<string, WhatSaid>
            {
                { "Stop", new WhatSaid { Verb = Verbs.Stop } },
                { "Pause", new WhatSaid { Verb = Verbs.Stop } },
                { "Freeze", new WhatSaid { Verb = Verbs.Stop } },
                { "Wait", new WhatSaid { Verb = Verbs.Stop } },
                { "Unfreeze", new WhatSaid { Verb = Verbs.Resume } },
                { "Resume", new WhatSaid { Verb = Verbs.Resume } },
                { "Continue", new WhatSaid { Verb = Verbs.Resume } },
                { "Start", new WhatSaid { Verb = Verbs.Resume } },
                { "Go", new WhatSaid { Verb = Verbs.Resume } },
            };

        private SpeechRecognitionEngine sre;
        private KinectAudioSource kinectAudioSource;
        private bool stopped;
        private bool isDisposed;

        private SpeechRecognizer()
        {
            RecognizerInfo ri = GetKinectRecognizer();
            this.sre = new SpeechRecognitionEngine(ri);
            this.LoadGrammar(this.sre);
        }

        public event EventHandler<SaidSomethingEventArgs> SaidSomething;

        public enum Verbs
        {
            Resume,
            Stop,
            Drive,
            Bottle,
            Pen,
            None
        }

        public EchoCancellationMode EchoCancellationMode
        {
            get
            {
                this.CheckDisposed();

                if (this.kinectAudioSource == null)
                {
                    return EchoCancellationMode.None;
                }

                return this.kinectAudioSource.EchoCancellationMode;
            }

            set
            {
                this.CheckDisposed();

                if (this.kinectAudioSource != null)
                {
                    this.kinectAudioSource.EchoCancellationMode = value;
                }
            }
        }

        // This method exists so that it can be easily called and return safely if the speech prereqs aren't installed.
        // We isolate the try/catch inside this class, and don't impose the need on the caller.
        public static SpeechRecognizer Create()
        {
            SpeechRecognizer recognizer = null;

            try
            {
                recognizer = new SpeechRecognizer();
            }
            catch (Exception)
            {
                // speech prereq isn't installed. a null recognizer will be handled properly by the app.
            }

            return recognizer;
        }

        public void Start(KinectAudioSource kinectSource)
        {
            this.CheckDisposed();

            if (kinectSource != null)
            {
                this.kinectAudioSource = kinectSource;
                this.kinectAudioSource.AutomaticGainControlEnabled = false;
                this.kinectAudioSource.BeamAngleMode = BeamAngleMode.Adaptive;
                var kinectStream = this.kinectAudioSource.Start();
                this.sre.SetInputToAudioStream(
                    kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.sre.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        public void Stop()
        {
            this.CheckDisposed();

            if (this.sre != null)
            {
                if (this.kinectAudioSource != null)
                {
                    this.kinectAudioSource.Stop();
                }

                this.sre.RecognizeAsyncCancel();
                this.sre.RecognizeAsyncStop();

                this.sre.SpeechRecognized -= this.SreSpeechRecognized;
                this.sre.SpeechHypothesized -= this.SreSpeechHypothesized;
                this.sre.SpeechRecognitionRejected -= this.SreSpeechRecognitionRejected;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "sre",
            Justification = "This is suppressed because FXCop does not see our threaded dispose.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();

                if (this.sre != null)
                {
                    // NOTE: The SpeechRecognitionEngine can take a long time to dispose
                    // so we will dispose it on a background thread
                    ThreadPool.QueueUserWorkItem(
                        delegate(object state)
                        {
                            IDisposable toDispose = state as IDisposable;
                            if (toDispose != null)
                            {
                                toDispose.Dispose();
                            }
                        },
                            this.sre);
                    this.sre = null;
                }

                this.isDisposed = true;
            }
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("SpeechRecognizer");
            }
        }

        private void LoadGrammar(SpeechRecognitionEngine speechRecognitionEngine)
        {
            // Build a simple grammar of shapes, colors, and some simple program control
            var thing = new Choices();
            foreach (var name in this.thingNames)
            {
                thing.Add(name.Key);
            }

            var command = new Choices();
            foreach (var name in this.commandNames)
            {
                command.Add(name.Key);
            }

            var objectChoices = new Choices();
            objectChoices.Add(thing);
            objectChoices.Add(command);

            var actionGrammar = new GrammarBuilder();
            actionGrammar.AppendWildcard();
            actionGrammar.Append(objectChoices);

            // This is needed to ensure that it will work on machines with any culture, not just en-us.
            var gb = new GrammarBuilder { Culture = speechRecognitionEngine.RecognizerInfo.Culture };
            gb.Append(actionGrammar);

            var g = new Grammar(gb);
            speechRecognitionEngine.LoadGrammar(g);
            speechRecognitionEngine.SpeechRecognized += this.SreSpeechRecognized;
            speechRecognitionEngine.SpeechHypothesized += this.SreSpeechHypothesized;
            speechRecognitionEngine.SpeechRecognitionRejected += this.SreSpeechRecognitionRejected;
        }

        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            var said = new SaidSomethingEventArgs { Verb = Verbs.None, Matched = "?" };

            if (this.SaidSomething != null)
            {
                this.SaidSomething(new object(), said);
            }

            Console.WriteLine("\nSpeech Rejected");
        }

        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.Write("\rSpeech Hypothesized: \t{0}", e.Result.Text);
        }

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.Write("\rSpeech Recognized: \t{0}", e.Result.Text);

            if ((this.SaidSomething == null) || (e.Result.Confidence < 0.3))
            {
                return;
            }

            var said = new SaidSomethingEventArgs { Verb = 0, Phrase = e.Result.Text };

            // Look for a match in the order of the lists below, first match wins.
            List<Dictionary<string, WhatSaid>> allDicts = new List<Dictionary<string, WhatSaid>> { this.commandNames, this.thingNames };

            bool found = false;
            for (int i = 0; i < allDicts.Count && !found; ++i)
            {
                foreach (var phrase in allDicts[i])
                {
                    if (e.Result.Text.Contains(phrase.Key))
                    {
                        said.Verb = phrase.Value.Verb;
                        said.Matched = phrase.Key;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                return;
            }

            if (this.stopped)
            {
                // Only accept restart or reset
                if (said.Verb != Verbs.Resume)
                {
                    return;
                }

                this.stopped = false;
            }
            else
            {
                if (said.Verb == Verbs.Resume)
                {
                    return;
                }
            }

            if (said.Verb == Verbs.Stop)
            {
                this.stopped = true;
            }

            if (this.SaidSomething != null)
            {
                this.SaidSomething(new object(), said);
            }
        }

        private struct WhatSaid
        {
            public Verbs Verb;
        }

        public class SaidSomethingEventArgs : EventArgs
        {
            public Verbs Verb { get; set; }

            public string Phrase { get; set; }

            public string Matched { get; set; }

        }
    }
}



























/*
 * 
// used for angle detection
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using Microsoft.Research.Kinect.Nui;
//using Microsoft.Speech.Recognition;
//using Microsoft.Speech.AudioFormat;
using Microsoft.Kinect;
//using Microsoft.Speech;
using System.Threading;
using System.IO;

namespace kinectStart
{
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private Stream audioStream;
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

            if (null != this.sensor)
            {
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
        }

    }

}



*/




























/*
namespace KinectSpeechRecognise
{
    public partial class MainWindow : Window
    {
        private const string RecID = "SR_MS_en-US_Kinect_10.0"; //check this variable as per the given software
        private int imgCount;
        private KinectAudioSource audioSource;
        private SpeechRecognitionEngine sre;
        private Thread audioThread;
        
        Runtime runtime = Runtime.Kinects[0];
        public MainWindow()
        {
            InitializeComponent();
            initializeSpeech();
            
            // Runtime initializationis handled when the window is opened.
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
    
            runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady);
        }

        private void initializeSpeech()
        {
            RecognizerInfo ri = SpeechRecognitionEngine.InstallRecognizers().Where(r => r.Id == RecID).FirstOrDefault();
            sre = new SpeechRecognitionEngine(ri.Id);
            var commands = new Choices();
            commands.Add("wallet");
            commands.Add("bottle");

            var gb = new GrammarBuilder();
            gb.Culture = ri.Culture;
            gb.Append(commands);

            var g = new Grammar(gb);
            sre.LoadGrammar(g);

            sre.SpeechRecognized += new EventHandler<speechRecognizedEventArgs>(sre_SpeechRecognised);

            audioThread = new Thread(startAudioListening);

            audioThread.Start();
        }

        private void startAudioListening()
        {
            // this function is not to be changed. It is a requisite if you want you use kinect

            audioSource = new KinectAudioSource();
            audioSource.FeatureMode = true;
            audioSource.AutomaticGainControl = false;
            audioSource.SystemMode = SystemMode.OptibeamArrayOnly;

            Stream aStream = audioSource.Start();
            sre.SetInputToAudioStream(aStream,
                                        new SpeechAudioFormatInfo(
                                            EncodingFormat.Pcm, 16000, 16, 1,
                                            32000, 2, null));

            sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void sre_SpeechRecognised(object sender, SpeechRecognisedEventArgs e)
        {
            
            // this function checks what you are saying and acts accordingly.
            if (e.Result.Text.ToLower() == "wallet" && e.Result.Confidence >= 0.50)
            {
                MessageBox.Show("you said wallet");
                // do your things here when person says "wallet"..;
            }
            if (e.Result.Text.ToLower() == "bottle" && e.Result.Confidence >= 0.50)
            {
                MessageBox.Show("you said bottle");
                // do your things here when person says "bottle"..;
            }
        }

        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonSet = e.SkeletonFrame;

            SkeletonFrame data = (from s in skeletonSet.Skeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();
            if (data != null)
            {
                SetEllipsePosition(Head, data.Joints[JointID.Head]);
                SetEllipsePosition(leftHand, data.Joints[JointID.HandLeft]);
                SetEllipsePosition(rightHand, data.Joints[JointID.HandRight]);
                ProcessGesture(data.Joints[JointID.Head], data.Joints[JointID.HandLeft], data.Joints[JointID.HandRight])
            }

        }

        private void ProcessGesture(Joint head, Joint handleft, Joint handright)
        {
            Microsoft.Research.Kinect.Nui.Vector vector = new Microsoft.Research.Kinect.Nui.Vector();
            vector.X = ScaleVector(800, handright.Position.X);
            vector.X = ScaleVector(600, -handright.Position.Y);
            vector.Z = handright.Position.Z;

            handright.Position = vector;

            if (handleft.Position.Y > head.Position.Y)
            {
                Canvas.SetLeft(image1, handright.Position.X);
                Canvas.SetTop(image1, handright.Position.Y);
            }
        }

        private void SetEllipsePosition(Ellipse ellipse, Joint joint)
        {
            Microsoft.Research.Kinect.Nui.Vector vector = new Microsoft.Research.Kinect.Nui.Vector();
            vector.X = ScaleVector(800, joint.Position.X);
            vector.Y = ScaleVector(600, -joint.Position.Y);
            vector.Z = joint.Position.Z;

            Joint updatedJoint = new joint();
            updatedJoint.ID = joint.ID;
            updatedJoint.TrackingState = JointTrackingState.Tracked;
            updatedJoint.Position = vector;

            Canvas.SetLeft(ellipse, updatedJoint.Position.X);
            Canvas.SetTop(ellipse, updatedJoint.Position.Y);
        }
    }

}
*/

































/*
namespace kinectStart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}*/
