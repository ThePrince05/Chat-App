using Client__.Net_.MVVM.ViewModel;
using Client__.Net_.MVVM.Model;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Forms;
using System.Windows.Threading;
using Panel = System.Windows.Controls.Panel;
namespace Chat_App
{
    public partial class MainWindow : Window
    {
        private double lvGroupListOldMaxHeight;
        private double NewGroupControlMenusOldHeight;
        private double NewGroupControlMenusOldLvListFreindsMaxHeight;

        public MainWindow( MainViewModel mainViewModel)
        {
            InitializeComponent();

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            lvGroupListOldMaxHeight = lvGroupList.MaxHeight;
            NewGroupControlMenusOldHeight = NewGroupControlMenus.Height;
            NewGroupControlMenusOldLvListFreindsMaxHeight = NewGroupControlMenus.lvListFriends.MaxHeight;
            this.Closing += Window_Closing;

            DataContext = mainViewModel;

            if (DataContext is MainViewModel mainVM)
            {
                mainVM.ToggleNewGroupPanel += TogglePanel;

                // Create the ContextMenu
                var contextMenu = mainVM.CreateContextMenu();

                // Set the DataContext of the ContextMenu (optional, if you need further command binding)
                contextMenu.DataContext = mainVM;

                // Link the ContextMenu to the ListView (lvGroupList)
                lvGroupList.ContextMenu = contextMenu;
            }
            
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
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
                MainGrid.Margin = new Thickness(7);
                lvGroupList.MaxHeight = 840;
                NewGroupControlMenus.Height = 700;
                NewGroupControlMenus.lvListFriends.MaxHeight = 550;
            }
            else
            {
                WindowState = WindowState.Normal;
                MainGrid.Margin = new Thickness(0);
                lvGroupList.MaxHeight = lvGroupListOldMaxHeight;
                NewGroupControlMenus.Height = NewGroupControlMenusOldHeight;
                NewGroupControlMenus.lvListFriends.MaxHeight = NewGroupControlMenusOldLvListFreindsMaxHeight;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("MainWindow is closing.");
        }

        private bool isPanelVisible = false; // Track visibility state
        private void TogglePanel() 
        {
            Storyboard sb;
            Storyboard sbShade;
            ShadeControlMenu.MessageVisibility = "Collapsed";


            if (isPanelVisible)
            {
                sb = (Storyboard)NewGroupControlMenus.FindResource("SlideAndFadeOut");
                sbShade = (Storyboard)ShadeControlMenu.FindResource("ShadeOut");
                ShadeControlMenu.Visibility = Visibility.Collapsed;
                //Panel.SetZIndex(NewGroupControlMenus, 0);
            }
            else
            {
                sb = (Storyboard)NewGroupControlMenus.FindResource("SlideAndFadeIn");
                sbShade = (Storyboard)ShadeControlMenu.FindResource("ShadeIn");
                ShadeControlMenu.Visibility = Visibility.Visible;
                //Panel.SetZIndex(NewGroupControlMenus, 1);

            }

            sb.Begin();
            sbShade.Begin();
            isPanelVisible = !isPanelVisible; // Toggle state
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TogglePanel();
        }

        private void Border_OpenEditProfile(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            MainViewModel.OpenUserProfileEdit();
        }

        private async void lvGroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvGroupList.SelectedItem is Group selectedGroup)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    // Stop polling for the previous group
                    viewModel.StopMessagePolling();

                    // Clear existing messages
                    viewModel.Messages.Clear();

                    // Reset polling state for the selected group
                    viewModel.ResetPollingState(selectedGroup.Id);

                    // Set the new selected group
                    viewModel.SelectedGroup = selectedGroup;

                    // Load messages for the new group
                    viewModel.IsMessagesLoading = true;  // Show skeleton loader if applicable

                    // Initially load all messages (if applicable) or the latest messages based on polling logic
                    await viewModel.LoadMessagesAsync(selectedGroup.Id);

                    viewModel.IsMessagesLoading = false; // Hide skeleton loader

                    // Restart polling for the new group
                    viewModel.StartMessagePolling();
                }
            }
        }

        
        private void ToggleShade()
        {
            Storyboard st = new();

            if (ShadeControlMenu.MessageVisibility == "Visible")
            {
                ShadeControlMenu.MessageVisibility = "Collapsed";
                st = (Storyboard)ShadeControlMenu.FindResource("ShadeOut");
                ShadeControlMenu.Visibility = Visibility.Hidden;
            }
            else
            {
                ShadeControlMenu.MessageVisibility = "Visible";
                st = (Storyboard)ShadeControlMenu.FindResource("ShadeIn");
                ShadeControlMenu.Visibility = Visibility.Visible;
            }

            st.Begin();
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                textBox.MinHeight = 0; // Reset height
                textBox.Height = double.NaN; // Reset height
                textBox.Measure(new Size(textBox.ActualWidth, double.PositiveInfinity));
                textBox.Height = textBox.DesiredSize.Height; // Set new height
            }
        }

        private void MessageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // Insert a new line at the cursor position
                var textBox = sender as System.Windows.Controls.TextBox;
                if (textBox != null)
                {
                    int caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                    textBox.CaretIndex = caretIndex + 1; // Move cursor to new line
                }
                e.Handled = true; // Prevent default behavior
            }
        }
    }
}
