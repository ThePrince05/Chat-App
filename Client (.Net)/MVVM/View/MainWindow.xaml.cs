using Chat_App.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chat_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimise_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void Maximise_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;

                // button behavior
                TitleBtnSize(30);

                // title label behavior
                lbl_title.FontSize = 15;

                // username label behavior
                lbl_username.FontSize = 14;
            }

            else
            { 
                Application.Current.MainWindow.WindowState = WindowState.Normal;

                // button behavior
                TitleBtnSize();

                // title label behavior
                lbl_title.FontSize = 12;

                // username label behavior
                lbl_username.FontSize = 12;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Access the ViewModel and execute the SendMessageCommand
                if (DataContext is MainViewModel viewModel && viewModel.SendMessageCommand.CanExecute(null))
                {
                    viewModel.SendMessageCommand.Execute(null);

                }
            }
        }

        private void TitleBtnSize(int btnSize = 20) {
            Maximise.Height = btnSize;
            Maximise.Width = btnSize;
            Minimise.Height = btnSize;
            Minimise.Width = btnSize;
            Exit.Height = btnSize;
            Exit.Width = btnSize;
        }
    }
}
