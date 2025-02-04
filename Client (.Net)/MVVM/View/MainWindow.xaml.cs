using Chat_App.MVVM.ViewModel;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chat_App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Minimise_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximise_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;

            }
            else
            {
                this.WindowState = WindowState.Normal;

            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle key events here if needed
        }

        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            MainViewModel.OpenUserProfileEdit();
        }
    }
}
