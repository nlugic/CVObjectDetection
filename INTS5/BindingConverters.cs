using Emgu.CV;
using System;
using System.Globalization;
using System.Windows.Data;

namespace INTS5
{
    [ValueConversion(typeof(AppUIState), typeof(bool))]
    public class PlayButtonStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AppUIState state = (AppUIState)value;
            return (state == AppUIState.VideoLoaded || state == AppUIState.StreamLoaded
                || state == AppUIState.VideoPaused || state == AppUIState.VideoFastForwarding
                || state == AppUIState.StreamPaused);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(AppUIState), typeof(bool))]
    public class FastForwardButtonStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AppUIState state = (AppUIState)value;
            return (state == AppUIState.VideoLoaded
                || state == AppUIState.VideoPaused || state == AppUIState.VideoPlaying);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(AppUIState), typeof(bool))]
    public class PauseButtonStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AppUIState state = (AppUIState)value;
            return (state == AppUIState.VideoPlaying || state == AppUIState.VideoFastForwarding
                || state == AppUIState.StreamPlaying);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(AppUIState), typeof(bool))]
    public class StopButtonStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AppUIState state = (AppUIState)value;
            return (state == AppUIState.VideoPaused || state == AppUIState.VideoPlaying
                || state == AppUIState.VideoFastForwarding || state == AppUIState.StreamPaused
                || state == AppUIState.StreamPlaying);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(AppUIState), typeof(bool))]
    public class AdjustmentsInterfaceStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AppUIState)value != AppUIState.NoMediaLoaded;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Mat), typeof(bool))]
    public class RemoveImageToTrackButtonStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Mat) != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(Mat), typeof(bool))]
    public class EdgeDetectionCheckBoxStateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (AppUIState)values[0] != AppUIState.NoMediaLoaded && (values[1] as Mat) == null;
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
