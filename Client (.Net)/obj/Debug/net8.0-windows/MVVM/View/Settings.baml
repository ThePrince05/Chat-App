   M S B A M L   `   `   `  ����H  DClient (.Net), Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:Client__.Net_.MVVM.ViewModelClient__.Net_.MVVM.ViewModel  5   	   &     Client__.Net_.MVVM.View.Settings   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   <local*clr-namespace:Client__.Net_.MVVM.ViewModel     5   	   b ^MaterialDesignThemes.Wpf, Version=4.1.0.2556, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5   	     � Title$  Settings��5
   	   $	��680��5   	   $	��600��5   	      MaterialDesignBody#���   5   	    � AllowsTransparency$
 True��5   	    � WindowStyle$
 None=�5   	    � WindowStartupLocation$ CenterScreen=�5   	   ��5   	   Z�  � DataContext 5      1   +Client__.Net_.MVVM.ViewModel.LoginViewModel  5   
   .��5      #  MaterialDesignThemes.Wpf.Card     UniformCornerRadius$ 15Q�6   ���#333333$��25q�5      .��5   
   )  #MaterialDesignThemes.Wpf.DialogHost  -   5          Name$ 
DialogHost��5   $      CloseOnClickAway$
 True��5   $   .��5       �� .+�5"      �� -   5        $��0,5,0,0q�5"      
������ $��0q�5$      $��60��5%      $��50��5&      $��Right=�5'        MaterialDesignFlatButton#��[ 5)      .��5*      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$ CircleXmark=�6#   	   
Foreground
	 �����5+   #   5,      �� 5.      $��
20 0 20 20q�6   .+�5/        $��0 0 0 30q�6#   $ ScrewdriverWrench=�50   #   
	 �����$	��110��52   #   $	��200��53   #   �� 56      $��Settings Configuration��6    $��40(�57       $
��Bold"�58       $��Center=�59       $��0,0,0,50q�5:       
�������5;       �� 5>      $��
20 0 20 20q�6!   .+�5?      �� -   5        
 ��Name$
 SupabaseUrlTextBox��5?   "    ! MaterialDesignOutlinedTextBox#��[ 5@   "   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Supabase URL��5A   "   $��20(�5B   "   $��0,0,0,20q�5D   "   
�������
���������5C   "   �� *SupabaseSettings.SupabaseUrl+5F   "   �� 5H      -   5        $
 SupabaseApiKeyBox��5H   "   #��[ 5I   "   $ Supabase API Key��5J   "   $��20(�5L   "   $��0,0,0,70q�5M   "   
�������
���������5K   "   �� *!SupabaseSettings.SupabaseApiKey+5O   "   �� 5Q      $
��SAVE��6!    ! MaterialDesignFlatMidBgButton#��[ 5R   !   +  %MaterialDesignThemes.Wpf.ShadowAssist   ShadowDepth$ Depth0=�5S   !   $��53��5T   !   $	��300��5U   !   +  %MaterialDesignThemes.Wpf.ButtonAssist   CornerRadius$ 10��5V   !   $��25(�5W   !   ��5X   !   �� *User.SelectedColor+��5Y   !   �� *SaveSettingsCommand+5Z      5\      5]      5^      5_      5`      