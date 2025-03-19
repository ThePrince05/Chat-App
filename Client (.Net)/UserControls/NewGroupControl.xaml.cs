using Client__.Net_.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client__.Net_.UserControls
{
    /// <summary>
    /// Interaction logic for NewGroupControl.xaml
    /// </summary>
    public partial class NewGroupControl : UserControl
    {
        public NewGroupControl()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is NewGroupViewModel viewModel)
            {
                foreach (var item in e.RemovedItems)
                {
                    viewModel.SelectedUsernames.Remove(item.ToString());
                }

                foreach (var item in e.AddedItems)
                {
                    viewModel.SelectedUsernames.Add(item.ToString());
                }

                // Notify that the CanCreateGroup property may have changed
                viewModel.OnPropertyChanged(nameof(viewModel.CanCreateGroup));
               
                // Output the selected usernames to the debug console
                foreach (var selectedItem in lvListFriends.SelectedItems)
                {
                    // Since it's a string, we can directly log the username
                    Debug.WriteLine($"Selected User: {selectedItem}");
                }
            }
        }
    }
}
