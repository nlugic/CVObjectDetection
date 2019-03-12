using Emgu.CV;
using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace INTS5
{
    public enum AppUIState : byte
    {
        NoMediaLoaded,
        ImageLoaded,
        VideoLoaded,
        StreamLoaded,
        VideoPlaying,
        VideoFastForwarding,
        StreamPlaying,
        VideoPaused,
        StreamPaused
    }

    public class CVObjectRecognitionViewModel : INotifyPropertyChanged
    {
        #region Properties

        private string windowTitle;
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                NotifyPropertyChanged("WindowTitle");
            }
        }

        private AppUIState currentUIState;
        public AppUIState CurrentUIState
        {
            get { return currentUIState; }
            set
            {
                currentUIState = value;
                NotifyPropertyChanged("CurrentUIState");
            }
        }

        private short brightness;
        public short Brightness
        {
            get { return brightness; }
            set
            {
                if (value < -255 || value > 255)
                    throw new ArgumentException("-255 <= Brightness <= 255");

                brightness = value;
                NotifyPropertyChanged("Brightness");
            }
        }

        private double contrast;
        public double Contrast
        {
            get { return contrast; }
            set
            {
                if (value < 0.01 || value > 7.99)
                    throw new ArgumentException("0.01 <= Contrast <= 7.99");

                contrast = value;
                NotifyPropertyChanged("Contrast");
            }
        }

        private double gamma;
        public double Gamma
        {
            get { return gamma; }
            set
            {
                if (value < 0.01 || value > 7.99)
                    throw new ArgumentException("0.01 <= Gamma <= 7.99");

                gamma = value;
                NotifyPropertyChanged("Gamma");
            }
        }
        
        public Mat LoadedImage { get; set; }
        
        private Mat trackingImage;
        public Mat TrackingImage
        {
            get { return trackingImage; }
            set
            {
                trackingImage = value;
                NotifyPropertyChanged("TrackingImage");
            }
        }

        private bool detectEdges;
        public bool DetectEdges
        {
            get { return detectEdges; }
            set
            {
                detectEdges = value;
                NotifyPropertyChanged("DetectEdges");
            }
        }

        public VideoCapture VideoSource { get; set; }
        public DispatcherTimer FrameTimer { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Constructors

        public CVObjectRecognitionViewModel() { }

        #endregion
    }
}
