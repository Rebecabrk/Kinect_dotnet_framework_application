using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Kinect;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Python.Runtime;
using System.Net.Sockets;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Windows.Media;
using System.IO;

namespace Kinect_dotnet_framework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectSensor;
        private TcpClient client;
        private NetworkStream stream;

        private WriteableBitmap colorBitmap;
        private byte[] colorPixels;

        public MainWindow()
        {
            InitializeComponent();
            InitializeKinect();
            InitializeConnection();
        }

        private void InitializeKinect()
        {
            kinectSensor = KinectSensor.KinectSensors[0];
            if (kinectSensor != null && !kinectSensor.IsRunning)
            {
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectSensor.ColorFrameReady += KinectSensor_ColorFrameReady;

                kinectSensor.Start();
            }
        }

        private void InitializeConnection()
        {
            client = new TcpClient("127.0.0.1", 12345); // Adjust IP and port as necessary
            stream = client.GetStream();

            // Initialize WriteableBitmap
            ColorImageStream colorStream = kinectSensor.ColorStream;
            colorPixels = new byte[colorStream.FramePixelDataLength];
            colorBitmap = new WriteableBitmap(
                colorStream.FrameWidth,
                colorStream.FrameHeight,
                96.0,
                96.0,
                PixelFormats.Bgr32,
                null);

            VideoFeed.Source = colorBitmap;
        }

        private void KinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null) return;

                // Copy frame data to the colorPixels array
                colorFrame.CopyPixelDataTo(colorPixels);

                // Send frame to Python server
                try
                {
                    byte[] compressedFrame = CompressFrame(colorPixels);
                    stream.Write(compressedFrame, 0, compressedFrame.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending frame: " + ex.Message);
                    return;
                }

                // Receive landmarks from the Python server
                byte[] buffer = new byte[8192]; // Adjust buffer size if needed
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Parse landmarks
                var landmarks = JsonConvert.DeserializeObject<List<Dictionary<string, float>>>(jsonResponse);

                // Draw landmarks on the video feed
                DrawLandmarks(landmarks);
            }
        }

        private byte[] CompressFrame(byte[] frame)
        {
            // Compress the byte array before sending to the Python server
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapSource bitmapSource = BitmapSource.Create(
                    640, 480, 96, 96, PixelFormats.Bgr32, null, frame, 640 * 4);

                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(ms);

                return ms.ToArray();
            }
        }

        private void DrawLandmarks(List<Dictionary<string, float>> landmarks)
        {
            // Update WriteableBitmap with Kinect frame
            colorBitmap.WritePixels(
                new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
                colorPixels,
                colorBitmap.PixelWidth * sizeof(int),
                0);

            // Draw landmarks on top of the video feed
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                // Draw the video feed
                dc.DrawImage(colorBitmap, new Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));

                // Prepare to display coordinates
                StringBuilder coordinatesText = new StringBuilder();

                // Draw each landmark
                foreach (var landmark in landmarks)
                {
                    double x = landmark["x"] * colorBitmap.PixelWidth;
                    double y = landmark["y"] * colorBitmap.PixelHeight;
                    dc.DrawEllipse(Brushes.Red, null, new Point(x, y), 5, 5);

                    // Add coordinates to the text
                    coordinatesText.AppendLine($"Landmark: X={x:F2}, Y={y:F2}");
                }

                // Update the TextBox with coordinates
                LandmarkCoordinates.Text = coordinatesText.ToString();
            }

            // Render the drawing onto the WriteableBitmap
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                colorBitmap.PixelWidth, colorBitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(visual);

            // Bind the final render to the Image control in the UI
            VideoFeed.Source = renderTarget;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop Kinect
            if (kinectSensor != null && kinectSensor.IsRunning)
            {
                kinectSensor.Stop();
            }

            // Close TCP connection
            if (stream != null) stream.Close();
            if (client != null) client.Close();
        }
    }
}
