using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Threading;

namespace INTS5
{
    public partial class MainWindow : Window
    {
        #region Properties

        private CVObjectDetector detector = new CVObjectDetector();
        private CVObjectRecognitionViewModel viewModel = new CVObjectRecognitionViewModel
        {
            WindowTitle = "OpenCV Object Recognition",
            CurrentUIState = AppUIState.NoMediaLoaded,
            Brightness = 0,
            Contrast = 1.0f,
            Gamma = 1.0f,
            LoadedImage = null,
            TrackingImage = null,
            VideoSource = null,
            FrameTimer = new DispatcherTimer()
        };

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            
            viewModel.FrameTimer.Tick += VideoPlayback;
            DataContext = viewModel;
        }

        #endregion

        #region OpenCV Frame Processing
        
        private void VideoPlayback(object sender, EventArgs e)
        {
            using (Mat newFrame = viewModel.VideoSource.QueryFrame())
            {
                if (newFrame == null)
                    viewModel.VideoSource.SetCaptureProperty(CapProp.PosFrames, 0.0);
                else
                    DisplayFrame(newFrame);
            }
        }

        private void DisplayFrame(Mat frame)
        {
            using (Image<Bgr, byte> tempImage = new Image<Bgr, byte>(frame.Width, frame.Height))
            {
                frame.ConvertTo(tempImage, DepthType.Default, viewModel.Contrast, viewModel.Brightness);
                tempImage._GammaCorrect(1.0 / viewModel.Gamma);

                if (viewModel.TrackingImage != null)
                    imgDisplay.Image = detector.MarkMatch(tempImage.Mat);
                else if (viewModel.DetectEdges)
                    imgDisplay.Image = detector.MarkEdges(tempImage.Mat);
                else
                    imgDisplay.Image = tempImage.Mat;
            }
        }

        #endregion
        
        #region Methods

        private void ResetMediaState(AppUIState newState, VideoCapture newVideoSource = null, Mat newImage = null)
        {
            AppUIState oldState = viewModel.CurrentUIState;
            viewModel.CurrentUIState = newState;

            bool restartPlayback = viewModel.FrameTimer.IsEnabled && newVideoSource != null;
            viewModel.FrameTimer.Stop();
            
            viewModel.VideoSource?.Dispose();
            viewModel.VideoSource = newVideoSource;
            viewModel.LoadedImage?.Dispose();
            viewModel.LoadedImage = newImage;
            imgDisplay.Image?.Dispose();
            imgDisplay.Image = newImage;

            if (restartPlayback)
            {
                if (newState == AppUIState.StreamLoaded)
                    viewModel.CurrentUIState = AppUIState.StreamPlaying;
                else if (oldState == AppUIState.VideoFastForwarding)
                    viewModel.CurrentUIState = AppUIState.VideoFastForwarding;
                else
                    viewModel.CurrentUIState = AppUIState.VideoPlaying;

                if (newState == AppUIState.StreamLoaded)
                    viewModel.FrameTimer.Interval = TimeSpan.FromMilliseconds(30.0);
                else
                    viewModel.FrameTimer.Interval = TimeSpan.FromMilliseconds(((viewModel.CurrentUIState == AppUIState.VideoFastForwarding)
                        ? 500.0 : 1000.0) / newVideoSource.GetCaptureProperty(CapProp.Fps));

                viewModel.FrameTimer.Start();
            }
        }

        #endregion

        #region Event Handlers

        private void btnImportImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openImage = new OpenFileDialog
            {
                Title = "Open Image...",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*"
            };

            bool? dialogResult = openImage.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value
                && !string.IsNullOrWhiteSpace(openImage.FileName))
            {
                viewModel.WindowTitle = "OpenCV Object Recognition - " + openImage.FileName;

                ResetMediaState(AppUIState.ImageLoaded, null,
                    CvInvoke.Imread(openImage.FileName, ImreadModes.AnyColor));
            }
        }

        private void btnImportVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openVideo = new OpenFileDialog
            {
                Title = "Open Video...",
                Filter = "Video Files|*.avi;*.mp4;*.mkv;*.wmv;*.mpg|All Files|*.*"
            };
            
            bool? dialogResult = openVideo.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value
                && !string.IsNullOrWhiteSpace(openVideo.FileName))
            {
                viewModel.WindowTitle = "OpenCV Object Recognition - " + openVideo.FileName;

                ResetMediaState(AppUIState.VideoLoaded, new VideoCapture(openVideo.FileName));
            }
        }

        private void btnStartCapture_Click(object sender, RoutedEventArgs e)
        {
            viewModel.WindowTitle = "OpenCV Object Recognition - Video Capture #0";
            ResetMediaState(AppUIState.StreamLoaded, new VideoCapture(0));
        }
        
        private void btnImportStream_Click(object sender, RoutedEventArgs e)
        {
            OpenStreamDialog openStream = new OpenStreamDialog();

            bool? dialogResult = openStream.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value
                && !string.IsNullOrWhiteSpace(openStream.StreamUrl))
            {
                viewModel.WindowTitle = "OpenCV Object Recognition - " + openStream.StreamUrl;
                ResetMediaState(AppUIState.StreamLoaded, new VideoCapture(openStream.StreamUrl));
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentUIState == AppUIState.StreamLoaded || viewModel.CurrentUIState == AppUIState.StreamPaused)
                viewModel.CurrentUIState = AppUIState.StreamPlaying;
            else
                viewModel.CurrentUIState = AppUIState.VideoPlaying;

            viewModel.FrameTimer.Interval = TimeSpan.FromMilliseconds((viewModel.CurrentUIState == AppUIState.VideoPlaying)
                ? 1000.0 / viewModel.VideoSource.GetCaptureProperty(CapProp.Fps) : 30.0);
            
            viewModel.FrameTimer.Start();
        }

        private void btnFastForward_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentUIState = AppUIState.VideoFastForwarding;

            viewModel.FrameTimer.Interval = TimeSpan.FromMilliseconds(500.0
                / viewModel.VideoSource.GetCaptureProperty(CapProp.Fps));

            viewModel.FrameTimer.Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentUIState = (viewModel.CurrentUIState == AppUIState.StreamPlaying)
                ? AppUIState.StreamPaused : AppUIState.VideoPaused;

            viewModel.FrameTimer.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            viewModel.WindowTitle = "OpenCV Object Recognition";

            ResetMediaState(AppUIState.NoMediaLoaded);
        }
        
        private void ImageAdjusted(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (viewModel.CurrentUIState == AppUIState.ImageLoaded)
                DisplayFrame(viewModel.LoadedImage);
        }

        private void btnResetBrightness_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Brightness = 0;
        }

        private void btnResetContrast_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Contrast = 1.0;
        }

        private void btnResetGamma_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Gamma = 1.0;
        }

        private void btnLoadImageToTrack_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog loadImage = new OpenFileDialog
            {
                Title = "Open Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*"
            };

            bool? dialogResult = loadImage.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value
                && !string.IsNullOrWhiteSpace(loadImage.FileName))
            {
                bool restartPlayback = viewModel.FrameTimer.IsEnabled;
                viewModel.FrameTimer.Stop();

                viewModel.DetectEdges = false;
                viewModel.TrackingImage?.Dispose();
                viewModel.TrackingImage = CvInvoke.Imread(loadImage.FileName, ImreadModes.AnyColor);
                imgTracking.Image?.Dispose();
                imgTracking.Image = viewModel.TrackingImage;
                detector.AddModelImage(viewModel.TrackingImage);

                if (restartPlayback)
                    viewModel.FrameTimer.Start();
                else if (viewModel.CurrentUIState == AppUIState.ImageLoaded)
                    DisplayFrame(viewModel.LoadedImage);
            }
        }

        private void btnRemoveImageToTrack_Click(object sender, RoutedEventArgs e)
        {
            imgTracking.Image?.Dispose();
            imgTracking.Image = null;
            viewModel.TrackingImage?.Dispose();
            viewModel.TrackingImage = null;
        }

        #endregion
    }
}
