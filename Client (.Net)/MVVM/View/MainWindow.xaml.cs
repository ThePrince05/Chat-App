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

                // left panel
                var grid = (Grid)FindName("MainGrid"); // Replace with the actual name of your grid if needed
                var row1 = (UIElement)grid.Children[1]; // Find the element of Row 1 (based on the order of the grid children)

                // Change the margin (left, top, right, bottom)
                row1.SetValue(MarginProperty, new Thickness(8, 0, 0, 0));  // Adjust as needed
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

                // left panel
                // Reset the margin when window is normal
                var grid = (Grid)FindName("MainGrid"); // Replace with actual name if needed
                var row1 = (UIElement)grid.Children[1];

                // Reset to default margin
                row1.SetValue(MarginProperty, new Thickness(0)); // Default margin
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Check for the Enter key with no Shift pressed (to send the message)
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                // Access the ViewModel and execute the SendMessageCommand
                if (DataContext is MainViewModel viewModel && viewModel.SendMessageCommand.CanExecute(null))
                {
                    e.Handled = true; // Prevents the default Enter behavior (new line)
                    viewModel.SendMessageCommand.Execute(null); // Execute the send message logic
                }
            }

            // Check for Shift + Enter (to insert a line break)
            if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                e.Handled = true; // Prevents the default behavior (new line)

                // Access the TextBox and insert a new line at the current caret position
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    // Insert a new line at the current caret position
                    int caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);

                    // Move the caret position to after the new line (so it can continue typing)
                    textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
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
