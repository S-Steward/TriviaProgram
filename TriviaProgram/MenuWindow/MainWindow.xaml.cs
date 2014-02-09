using System.Windows;
using TriviaLibrary;

namespace MenuWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            GameplayWindow.MainWindow game = new GameplayWindow.MainWindow();
            game.Show();
            this.Close();
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Currently disabled

        //private void DisplayPopup(object sender, RoutedEventArgs e)
        //{
        //    OptionsWindow.MainWindow options = new OptionsWindow.MainWindow();
        //    options.Show();

        //}
    }
}