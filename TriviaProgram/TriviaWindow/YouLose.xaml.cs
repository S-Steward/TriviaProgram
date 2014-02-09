using System.Windows;

namespace GameplayWindow
{
    /// <summary>
    /// Interaction logic for YouLose.xaml
    /// </summary>
    public partial class YouLose : Window
    {
        public YouLose()
        {
            InitializeComponent();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}