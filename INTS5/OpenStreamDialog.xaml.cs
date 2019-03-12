using System.Windows;
using System.Windows.Controls;

namespace INTS5
{
    public partial class OpenStreamDialog : Window
    {
        #region Properties

        public string StreamUrl => tbxStreamUrl.Text;

        #endregion

        #region Constructors

        public OpenStreamDialog()
        {
            InitializeComponent();

            DataContext = this;
        }

        #endregion

        #region Event Handlers

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

        private void tbxStreamUrl_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            btnOpen.IsEnabled = !string.IsNullOrWhiteSpace((sender as TextBox).Text);
        }
    }
}
