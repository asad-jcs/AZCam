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

            // Set the capture resolution
            capture.Set(VideoCaptureProperties.FrameWidth, 1080);
            capture.Set(VideoCaptureProperties.FrameHeight, 1920);

            while (isCameraRunning)
            {
                using (var frame = new Mat())
                {
                    // Capture the frame
                    capture.Read(frame);

                    if (frame.Empty())
                        continue;

                    // Rotate the frame 90 degrees clockwise
                    Cv2.Rotate(frame, frame, RotateFlags.Rotate90Clockwise);

                    // Display the frame in PictureBox
                    Bitmap image = frame.ToBitmap();
                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = image;

                    // Write to the video file if recording
                    if (isRecording)
                    {
                        writer.Write(frame);
                    }
                }
            }
        }


        private void StartRecording()
        {
            if (isRecording) return;

            // Define the video file path
            string filePath = "C:\\Video\\recorded_video.mp4";

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Initialize the VideoWriter with resolution 1080x1920, 30 fps
            writer = new VideoWriter(filePath, FourCC.MJPG, 30, new OpenCvSharp.Size(1080, 1920));

            if (!writer.IsOpened())
            {
                MessageBox.Show("Could not open the video file for writing.");
                return;
            }

            isRecording = true;
        }

        private void buttonStopRecording_Click(object sender, EventArgs e)
        {
            // Stop recording
            isRecording = false;

            writer?.Release();
            writer = null;
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
