using Client__.Net_.MVVM.Model;
using Client__.Net_.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Client__.Net_.MVVM.View
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        public UserProfile()
        {
            InitializeComponent();
          
            // Create a list of ColorItem objects for ComboBox
            List<ColorItem> colors = new List<ColorItem>
            {
                new ColorItem("Red", new SolidColorBrush(Color.FromArgb(255, 158, 8, 8))),
                new ColorItem("Green", new SolidColorBrush(Colors.Green)),
                new ColorItem("Blue", new SolidColorBrush(Colors.DodgerBlue)),
                new ColorItem("Yellow", new SolidColorBrush(Color.FromArgb(255, 229, 175, 9))),
                new ColorItem("Purple", new SolidColorBrush(Color.FromArgb(255, 86, 25, 116)))

            };

            // Bind the list of colors to the ComboBox
            ColorComboBox.ItemsSource = colors;


        }

        // Close the Window
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Handle ComboBox selection changes
        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ColorComboBox.SelectedItem as ColorItem;
            if (selectedItem != null)
            {
                var viewModel = this.DataContext as LoginViewModel;
                if (viewModel != null)
                {
                    viewModel.SelectedColor = selectedItem.Color; // Updates Hex automatically
                }
            }
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
