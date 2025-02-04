using Chat_App.MVVM.ViewModel;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Chat_App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void Minimise_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximise_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
                MainGrid.Margin = new Thickness(7);
            }
            else
            {
                WindowState = WindowState.Normal;
                MainGrid.Margin = new Thickness(0);
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

        private bool isPanelVisible = false; // Track visibility state

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb;


             if (isPanelVisible)
            {
                sb = (Storyboard)NewGroupControlMenus.FindResource("SlideAndFadeOut");
            }
            else
            {
                sb = (Storyboard)NewGroupControlMenus.FindResource("SlideAndFadeIn");
            }

            sb.Begin();
            isPanelVisible = !isPanelVisible; // Toggle state
        }
    }
}
