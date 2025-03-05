using Chat_App.MVVM.ViewModel;
using Client__.Net_.MVVM.Model;
using Client__.Net_.MVVM.ViewModel;
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
using System.Windows.Shapes;

namespace Client__.Net_.MVVM.View
{
    /// <summary>
    /// Interaction logic for UserProfileEdit.xaml
    /// </summary>
    public partial class UserProfileEdit : Window
    {
        public UserProfileEdit()
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
    }
}
