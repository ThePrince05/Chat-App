   M S B A M L   `   `   `  ����C  ?Chat App, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:Client__.Net_.MVVM.ViewModelClient__.Net_.MVVM.ViewModel  5   	   )    #Client__.Net_.MVVM.View.UserProfile   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   <local*clr-namespace:Client__.Net_.MVVM.ViewModel     5   	   b ^MaterialDesignThemes.Wpf, Version=4.1.0.2556, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5   	   $	��800��5
   	   $	��460��5   	      MaterialDesignBody#���   5   	     � AllowsTransparency$
  True��5   	    � WindowStyle$
 None=�5   	    � WindowStartupLocation$ CenterScreen=�5   	   ��5   	   Z�  � DataContext 5      1   +Client__.Net_.MVVM.ViewModel.LoginViewModel  5   
   .��5      #  MaterialDesignThemes.Wpf.Card     UniformCornerRadius$ 15Q�6   ���#333333$��25q�5      .��5   
   )  #MaterialDesignThemes.Wpf.DialogHost  -   5          Name$ 
DialogHost��5   $      CloseOnClickAway$
 True��5   $   .��5      �� .+�5      �� -   5        $��0,5,0,0q�5      
������ $��0q�5      $��60��5      $��50��5       $��Right=�5!        MaterialDesignFlatButton#��[ 5#      .��5$      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$ CircleXmark=�6#      
Foreground
 �����5%   #   5&        5(      $��0 10 0 5q�6   $ UserPlus=�5)      
 �����$	��120��5+      $	��180��5,      �� 5.      $��0 25 0 5q�6   $��Center=�5/      $��40(�50      $
��Bold"�51      $��
Welcome!!!��52      
�������53      �� 54      $��17(�6   $��SemiBold"�55      
�������$��Center=�57      $��Login with a new username��58      �� 5:      $��0 30 0 0q�6   $	��300��5;      $��15(�5<      
�������)  #MaterialDesignThemes.Wpf.HintAssist	   Hint$	 Enter Username��5>      $��2q�5?        MaterialDesignDivider#���  5A       ! MaterialDesignOutlinedTextBox#��[ 5B      ��5@      �� *
Username+5B      �� 5D      $��0 20 0 0q�6   $	��300��5E      $��15(�5F      
�������$	 Enter Password��5H      $��2q�5I      #���  5K      #��[ 5L      ��5J      �� *
Password+5L      �� 5P      -   5        
 ��Name$
 ColorComboBox��5Q      $��0 30 0 0q�5P      +  %MaterialDesignThemes.Wpf.ShadowAssist   ShadowDepth$ Depth1=�5R      $��50��5S      $	��300��5T      $��10q�5U      
����������#333333$��15(�5X      $��1q�5Y      
������� �� SelectedValuePath$ Color��5[       �� DisplayMemberPath$
 Name��5]      5`      �� 5c      $	��300��6   $��40��5d      $��Bottom=�5e      $��Center=�5f      
�������$��	0 30 0 20q�5h      ��5i      �� *SelectedColor+�� 5k      -   5         ��Name$ loginBtn��5l      $��0 30 0 0q�5k       ! MaterialDesignFlatMidBgButton#��[ 5m      $ Depth0=�5n      $��53��5o      $	��300��5p      +  %MaterialDesignThemes.Wpf.ButtonAssist   CornerRadius$ 10��5q      $��18(�5r      $
��SAVE��5s      ��5t      �� *SaveUserCommand+��5u      ��  �� RelativeSource ��  �� AncestorType
 Window5v      5w      5x      5z      