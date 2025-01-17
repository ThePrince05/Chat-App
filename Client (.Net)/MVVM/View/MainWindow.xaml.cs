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

                // username label behavior
                lbl_username.FontSize = 14;

                // left panel
                var grid = (Grid)FindName("MainGrid");
                var row1 = (UIElement)grid.Children[1];  // Find the element of Row 1 (based on the order of the grid children)
                row1.SetValue(MarginProperty, new Thickness(8, 0, 0, 0));  // Adjust as needed
            }
            else
            {
                this.WindowState = WindowState.Normal;

                // username label behavior
                lbl_username.FontSize = 12;

                // left panel
                var grid = (Grid)FindName("MainGrid");
                var row1 = (UIElement)grid.Children[1];
                row1.SetValue(MarginProperty, new Thickness(0));  // Default margin
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
            viewModel.OpenUserProfile();
        }
    }
}
