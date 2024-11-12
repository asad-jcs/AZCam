using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzCam
{
    public partial class CameraForm : Form
    {
        private VideoCapture capture;
        private Thread cameraThread;
        private VideoWriter writer;
        private bool isCameraRunning;
        private bool isRecording;
        private int elapsedSeconds = 0;
        public CameraForm()
        {
            InitializeComponent();
        }

        private void buttonStartRecording_Click(object sender, EventArgs e)
        {
            if (!isCameraRunning)
            {
                cameraThread = new Thread(StartCamera);
                cameraThread.Start();
                isCameraRunning = true;
            }

            // Start recording
            StartRecording();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isCameraRunning)
            {
                // Start the camera capture in a separate thread
                cameraThread = new Thread(StartCamera);
                cameraThread.Start();
                isCameraRunning = true;
            }
        }

        private void StartCamera()
        {
            capture = new VideoCapture(0); // 0 is the default camera
            capture.Open(0);

            if (!capture.IsOpened())
            {
                MessageBox.Show("Camera could not be opened.");
                return;
            }

            // Attempt to set the capture resolution to 1080x1920
            capture.Set(VideoCaptureProperties.FrameWidth, 1080);
            capture.Set(VideoCaptureProperties.FrameHeight, 1920);

            int actualWidth = (int)capture.Get(VideoCaptureProperties.FrameWidth);
            int actualHeight = (int)capture.Get(VideoCaptureProperties.FrameHeight);

            MessageBox.Show($"Camera resolution set to {actualWidth}x{actualHeight}");

            // Initialize VideoWriter with the rotated resolution (1920x1080 after rotation)
            writer = new VideoWriter("C:\\Video\\recorded_video.avi", FourCC.MJPG, 30, new OpenCvSharp.Size(actualHeight, actualWidth), true);

            if (!writer.IsOpened())
            {
                MessageBox.Show("Could not open the video file for writing.");
                return;
            }

            isCameraRunning = true;
            elapsedSeconds = 0;
            /*timerRecording.Start();*/ // Start timer for recording

            // Main loop to capture and display frames
            Task.Run(() =>
            {
                while (isCameraRunning)
                {
                    using (var frame = new Mat())
                    {
                        // Capture the frame
                        capture.Read(frame);

                        if (frame.Empty())
                            continue;

                        // Rotate the frame 90 degrees
                        Cv2.Rotate(frame, frame, RotateFlags.Rotate90Clockwise);

                        // Update the PictureBox with the rotated frame
                        Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                        pictureBoxCamera.Invoke((MethodInvoker)(() =>
                        {
                            pictureBoxCamera.Image?.Dispose();
                            pictureBoxCamera.Image = image;
                            pictureBoxCamera.SizeMode = PictureBoxSizeMode.StretchImage;

                            // Adjust the PictureBox size based on the rotated frame size (1920x1080)
                            pictureBoxCamera.Width = frame.Height;  // 1920
                            pictureBoxCamera.Height = frame.Width;  // 1080
                        }));

                        // Write frame to video file if recording
                        if (isRecording)
                        {
                            writer.Write(frame);
                        }
                    }
                }
            });
        }

        private void StartRecording()
        {
            if (isRecording) return;

            isRecording = true;
            elapsedSeconds = 0;
            timerRecording.Start(); // Start the timer
            MessageBox.Show("Recording started successfully!");
        }

        private void buttonStopRecording_Click(object sender, EventArgs e)
        {
            isRecording = false;
            isCameraRunning = false;

            writer?.Release();
            writer = null;
            capture?.Release();

            timerRecording.Stop(); // Stop the timer
            MessageBox.Show("Recording stopped. Video saved at C:\\Video\\recorded_video.avi");
        }

        // Timer tick event to update the elapsed time
        private void timerRecording_Tick(object sender, EventArgs e)
        {
            elapsedSeconds++;
            TimeSpan time = TimeSpan.FromSeconds(elapsedSeconds);
            labelTimer.Text = $"Recording Time: {time.ToString(@"hh\:mm\:ss")}";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop the camera capture when closing the form
            isCameraRunning = false;
            cameraThread?.Join();

            capture?.Release();
            base.OnFormClosing(e);
        }
    }
}
