using System.Windows;

namespace OptionsWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// None of this is called for current functionality purpose
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}