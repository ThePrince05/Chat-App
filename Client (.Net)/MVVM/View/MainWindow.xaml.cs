using Client__.Net_.MVVM.ViewModel;
using Client__.Net_.MVVM.Model;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
namespace Chat_App
{
    public partial class MainWindow : Window
    {
        private double lvGroupListOldMaxHeight;
        private double NewGroupControlMenusOldHeight;
        private double NewGroupControlMenusOldLvListFreindsMaxHeight;

        private NewGroupViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            lvGroupListOldMaxHeight = lvGroupList.MaxHeight;
            NewGroupControlMenusOldHeight = NewGroupControlMenus.Height;
            NewGroupControlMenusOldLvListFreindsMaxHeight = NewGroupControlMenus.lvListFriends.MaxHeight;
            this.Closing += Window_Closing;

            if (DataContext is MainViewModel mainVM)
            {
                mainVM.ToggleNewGroupPanel += TogglePanel;
                
                if (mainVM != null)
                {
                    lvGroupList.ContextMenu = mainVM.CreateContextMenu();
                }
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
            Application.Current.Shutdown();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle key events here if needed
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


            if (isPanelVisible)
            {
                sb = (Storyboard)NewGroupControlMenus.FindResource("SlideAndFadeOut");
                sbShade = (Storyboard)ShadeControlMenu.FindResource("ShadeOut");
                ShadeControlMenu.Visibility = Visibility.Hidden;
            }
            else
            {
                sb = (Storyboard)NewGroupControlMenus.FindResource("SlideAndFadeIn");
                sbShade = (Storyboard)ShadeControlMenu.FindResource("ShadeIn");
                ShadeControlMenu.Visibility = Visibility.Visible;
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
                    viewModel.SelectedGroup = selectedGroup;  // <-- Set SelectedGroup!
                    await viewModel.LoadMessagesAsync(selectedGroup.Id);
                }
            }
        }
       

    }
}
