using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect;
using Emgu.CV;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

namespace Kinect_dotnet_framework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinectSensor;
        Skeleton[] skeletons;
        const float movementThreshold_X = 0.05f;         //minimum distance (left-right) to be considered a shake
        float previousRightHandPosition_X = 0;               
        int shakeCount = 0;                 
        const int shakeCountThreshold = 2;              //minimum of shakes to be considered waving

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if(kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor.SkeletonFrameReady -= KinectSensor_SkeletonFrameReady;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if(kinectSensor != null)
            {
                kinectSensor.SkeletonStream.Enable();
                kinectSensor.SkeletonFrameReady += KinectSensor_SkeletonFrameReady;
                kinectSensor.Start();
            }
        }

        private void KinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using(SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    Skeleton skeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);

                    if(skeleton != null)
                    {
                        SetImagePosition(HeadImage, skeleton.Joints[JointType.Head]);
                        SetImagePosition(LeftHandImage, skeleton.Joints[JointType.HandLeft]);
                        SetImagePosition(RightHandImage, skeleton.Joints[JointType.HandRight]);
                        TrackHandShake(skeleton);
                        TrackHandRaise(skeleton);
                    }
                }
            }
        }

        private void SetImagePosition(System.Windows.Controls.Image image, Joint joint)
        {
            if (joint.TrackingState == JointTrackingState.Tracked)
            {
                DepthImagePoint point = kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, DepthImageFormat.Resolution640x480Fps30);

                Canvas.SetLeft(image, point.X);
                Canvas.SetTop(image, point.Y);
            }
        }

        private void TrackHandShake(Skeleton skeleton)
        {
            Joint rightHand = skeleton.Joints[JointType.HandRight];

            if (rightHand.TrackingState == JointTrackingState.Tracked) {
                var rightHandPosition = skeleton.Joints[JointType.HandRight].Position;

                float distanceMoved_X = Math.Abs(rightHandPosition.X - previousRightHandPosition_X);

                if(distanceMoved_X >= movementThreshold_X)
                {
                    shakeCount++;
                }

                if (shakeCount >= shakeCountThreshold)
                {
                    DisplayHello("Hello!");
                    shakeCount = 0;
                }

                previousRightHandPosition_X = rightHandPosition.X;
            }
        }

        private void TrackHandRaise(Skeleton skeleton)
        {
            Joint rightHand = skeleton.Joints[JointType.HandRight];
            Joint leftHand = skeleton.Joints[JointType.HandLeft];
            Joint chest = skeleton.Joints[JointType.ShoulderCenter];

            if (rightHand.TrackingState == JointTrackingState.Tracked && rightHand.Position.Y > chest.Position.Y)
            {
                Task.Run(() =>
                {
                    int rightHandFingers = CountFingers(rightHand);
                    DisplayFingerCount("Right Hand: " + rightHandFingers);
                });
            }

            if (leftHand.TrackingState == JointTrackingState.Tracked && leftHand.Position.Y > chest.Position.Y)
            {
                Task.Run(() =>
                {
                    int leftHandFingers = CountFingers(leftHand);
                    DisplayFingerCount("Left Hand: " + leftHandFingers);
                });
            }
        }

        private int CountFingers(Joint handJoint)
        {
            //to do: implement finger counting algorithm
            Random random = new Random();
            return random.Next(0, 6);
        }

        private void DisplayFingerCount(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (message.StartsWith("Left Hand:"))
                {
                    LeftFingerCountTextBlock.Text = message;
                }
                else if (message.StartsWith("Right Hand:"))
                {
                    RightFingerCountTextBlock.Text = message;
                }
            });

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(7);
            timer.Tick += (sender, e) =>
            {
                if (message.StartsWith("Left Hand:"))
                {
                    LeftFingerCountTextBlock.Text = "";
                }
                else if (message.StartsWith("Right Hand:"))
                {
                    RightFingerCountTextBlock.Text = "";
                }
                timer.Stop();
            };
            timer.Start();
        }

        private void DisplayHello(string message)
        {
            // Update the UI on the main thread
            Dispatcher.Invoke(() =>
            {
                WaveTextBlock.Text = message;
            });

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (sender, e) =>
            {
                WaveTextBlock.Text = "";
                timer.Stop();
            };
            timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            // the base class will perform any necessary cleaning
            base.OnClosed(e);

            if(kinectSensor != null)
            {
                kinectSensor.Stop();
            }
        }
    }
}
