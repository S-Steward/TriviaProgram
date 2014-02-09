using System.Windows;

namespace GameplayWindow
{
    /// <summary>
    /// Interaction logic for YouWin.xaml
    /// </summary>
    public partial class YouWin : Window
    {
        public YouWin()
        {
            InitializeComponent();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}