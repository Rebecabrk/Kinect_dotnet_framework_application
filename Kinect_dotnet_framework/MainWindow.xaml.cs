using System;
using System.Collections.Generic;
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
using Microsoft.Kinect;
using System.Diagnostics;

namespace Kinect_dotnet_framework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinectSensor;
        Skeleton[] skeletons;

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
                        SetEllipsePosition(HeadEllipse, skeleton.Joints[JointType.Head]);
                        SetEllipsePosition(LeftHandEllipse, skeleton.Joints[JointType.HandLeft]);
                        SetEllipsePosition(RightHandEllipse, skeleton.Joints[JointType.HandRight]);
                    }
                }
            }
        }

        private void SetEllipsePosition(Ellipse ellipse, Joint joint)
        {
            if(joint.TrackingState == JointTrackingState.Tracked)
            {
                DepthImagePoint point = kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, DepthImageFormat.Resolution640x480Fps30);

                Canvas.SetLeft(ellipse, point.X);
                Canvas.SetTop(ellipse, point.Y);
            }
        }
    }
}
