   M S B A M L   `   `   `  ����C  ?Chat App, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:Client__.Net_.MVVM.ViewModelClient__.Net_.MVVM.ViewModel  5   	   G(clr-namespace:Client__.Net_.UserControlsClient__.Net_.UserControls  5	   	       Chat_App.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @	viewmodel*clr-namespace:Client__.Net_.MVVM.ViewModel     5   	   X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5   	   b ^MaterialDesignThemes.Wpf, Version=4.1.0.2556, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   ;userControls(clr-namespace:Client__.Net_.UserControls  5	   	   $	��500��5   	   $	��300��5   	   $	��900��5   	   $	��500��5   	   ���#36393F  � WindowStyle$
  None=�5   	    � WindowStartupLocation$ CenterScreen=�5   	    � 
ResizeMode$ 	CanResize=�5   	   '  !System.Windows.Shell.WindowChrome   WindowChrome 5        5   
      CaptionHeight$ 0Q�6      CornerRadius$ 15��5         GlassFrameThickness$ 5q�5      .��5       � -   5         �Name$ MainGrid��5        � RowDefinitions 5!   
   �� 5"      $
��auto �6   �� 5#      $��* �6   	 � ColumnDefinitions	 5&   
   �� 5'      $	��200 �6   �� 5(      $��8* �6   .+�5,   
   �� -   5        $��2��5,      ���#252525._�5/      � $��Stretch=�6   $��5 5 5 5q�50      .+�51      �� -   5        
 ��Name$
 	lbl_title��51      $��Chat App��52      $��0 0 0 0q�53      
�������$��SemiBold"�55      $��18(�56      $��Center=�57      �� 59      $��Right=�6    �� Orientation$ 
Horizontal=�5:      .+�5<      �� -   5        $��0,5,0,5q�5<      
������ $��50��5>      $��30��5?      $��Right=�5@         MaterialDesignFlatButton#��[  5B      .��5C      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$ WindowMinimize=�6'      
Foreground
 �����5D   '   5E      �� 5G      -   5        $��0,5,0,5q�5G      
������ $��50��5I      $��30��5J      $��Right=�5K      #��[  5M      .��5N        $ WindowMaximize=�6'   
 �����5O   '   5P      �� 5R      -   5        $��0,5,0,5q�5R      
������ $��50��5T      $��30��5U      $��Right=�5V      #��[  5X      .��5Y        $ Xmark=�6'   
 �����5Z   '   5[      5]      5^      5_      � 5b   
   $��1��6    K� ZIndex$ 2��5c      ���#2f3136 5e      �� 5f      $��40 �6    �� 5g      $��* �6    �� 5h      $��70 �6    .+�5l      � ���#292B2F	 5m      �� 5n      $
��100* �6'   �� 5o      $	��25* �6'   .+�5r      �� $��0��6   $��35��5s      )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Search��5t      $��18(�5u      $��10,7,0,0q�5v      
������ $��0q�5x      
���������5z      �� *SearchQuery+ �� UpdateSourceTrigger$ PropertyChanged=�.��6\    J� InputBindings 5{      �� 5|      
 �� Key$ Enter��6%    �� Command 5}   %   �� *SearchGroupsCommand+.��5~   -   5      �� 5�      $��2��6   $��22��5�      $��22��5�      $��0 0 0 0q�5�      $��0q�5�      $��Center=�5�      $��Center=�5�      #��[  5�      ��5�      �� *SearchGroupsCommand+.��5�        $ MagnifyingGlass=�6#   
 �����5�   #   5�      5�      �� 5�      $��1��6   .+�5�      � .+�5�      �� $��10,5,0,0q�6    $��18(�5�       
�������$
��Left=�5�       B� 5�      .�6$   .�5�      �� $�Groups��6"   5�      5�      �� 5�      $ 
Horizontal=�6!   $��Right=�5�   !   .+�5�      �� -   5        $��0q�5�   !   $��22��5�   !   $��22��5�   !   $��0 4 9 0q�5�   !   #��[  5�   !   .��5�        $
 Edit=�6+   
 �����$��Center=�5�   +   5�      5�      5�      �� 5�      -   5         ��Name$ lvGroupList��5�      $��5q�5�      $
��auto��5�      $
��auto��5�      
�������$ �� IsSynchronizedWithCurrentItem$ FalseY�5�      
������ $��0q�5�      $
��Auto=�5�      $��Disabled=�5�      ?  9System.Windows.Controls.Primitives.SelectiveScrollingGrid$   SelectiveScrollingOrientation$ Vertical=�5�        
GroupsCard#��[ 5�      ��5�      �� *Groups+��5�      �� 5�       �� 
TargetType" ��6    .�5�      ��  �� Property � y� �� Value$ Visible=�5�   %    �� Triggers 5�      �� 5�   "    �� Binding 6.   �� *IsGroupsLoading+ �� Value True5�   .   .a�5�   &   ��  � y�$ 	Collapsed=�5�   -   5�   #   5�      5�      �� 5�      $��5q�6   ��5�      �� 5�      " ��6    .�5�      ��  � y�$ 	Collapsed=�5�   %    5�      �� 5�   "    6.   �� *IsGroupsLoading+ True5�   .   .a�5�   &   ��  � y�$ Visible=�5�   -   5�   #   5�      .+�5�      �� $��50��6   ���#444444 �� CornerRadius$ 5��5�      $��5q�5�      �� 5�      $��50��6   ���#444444$ 5��5�      $��5q�5�      �� 5�      $��50��6   ���#444444$ 5��5�      $��5q�5�      5�      5�      � 5�      $��2��6   ���#292b2f	 5�      �� 5�      $��0.25* �6'   �� 5�      $
��0.5* �6'   �� 5�      $��0.25* �6'   .+�5�      � $��0��6   .+�5�      �� -	   5        $ 25��5�      $��30��5�      $��30��5�      $��Center=�5�       � Cursor$
 Hand��5�      $��0,3,0,2q�5�      ��5�      �� *User.SelectedColor+5�      5�      � 5�      $��1��6   $��Center=�5�      .+�5      �� .+�5     �� -
   5        $
 lbl_username��5      $��15(�5      
�������$��SemiBold"�5      $��0,9,0,-5q�5      ��5      �� *User.Username+ �� TargetNullValue$ Username��5      �� 5     $��Custom Status��6    
�������$��SemiBold"�5	      $��14(�5
      $��0,0,0,10q�5      5     5     � 5     $��2��6   .+�5     �� $��0,0,0,0q�6   
������ $��0q�5     $��30��5     $��30��5     $��Center=�5     #��[  5     ��5     �� *OpenSettingsCommand+.��5       $
 Gear=�6'   
 �����$��Center=�5  '   5     5     5      5"     � 5&  
   $��1��6   $��1��5'      5(     �� 5)     $
��365* �6    �� 5*     $��68 �6    	 5-     �� 5.     $
��630* �6#   �� 5/     $	��100 �6#   .+�53     -   'Client__.Net_.UserControls.ShadeControl  -   5           Name$  ShadeControlMenu��54  )   ! J� Opacity$	! 0.7Q�55  )   $��2��56  )   "   ShadeMessage$"  ��57  )   $y�	Collapsed=�58  )   $��4��59  )    53  )   �� *$"DataContext.ShadeControlMenuZIndex+# �� RelativeSource# �� $ �� AncestorType
$ Window59  )   0   *Client__.Net_.UserControls.NewGroupControl  5:     -   5        %  Name$% NewGroupControlMenus��5<  +   $ 1��5:  +   $��4��5;  +   $	��Top=�5=  +   $
��Left=�5>  +   $	��210��5?  +   $	��450��5@  +   & � DataContext& 5A  +   �� *
NewGroupVM+�� 5F     -   5        $ lvMessageList��5F     
�������
������ $��0q�5J     $��2��5K     $��8,2,0,33q�5L     $
��Auto=�5M     $��3��5N     ��5H     �� *
Messages+��5Q     �� 5R     " ��6   .�5S     ��  � ��$ Stretch=�5T  !    5U     �� 5W      6*   �� *IsCurrentUser+ True5X  *   .a�5Y  "   ��  � ��$ Right=�5Z  )   5[     �� 5]      6*   �� *IsCurrentUser+	 False5^  *   .a�5_  "   ��  � ��$
 Left=�5`  )   5a     5c     ��5f     �� 5g     .R�5h     �� ' �� Padding$' 10q�6!   $ 8��5i  !   $	��150��5j  !   $	��600��5k  !   $��	20 5 20 0q�5m  !   ��5l  !   �� *IsCurrentUser+  MessageBackgroundConverter( �� 	Converter#( [ ._�5n     �� .+�5p  "   �� $
��Bold"�5q  ,   $��14(�5r  ,   ���#84888E$��0,0,0,5q�5t  ,   ��5p  ,   �� *
username+5t  ,   �� 5v  "   $
��Wrap=�5w  ,   $��14(�5x  ,   
�������$��0,0,0,5q�5z  ,   ��5v  ,   �� *	message+5z  ,   �� 5|  "   $��10(�5}  ,   ���#aaa$��Right=�5  ,   $��Bottom=�5�  ,   ��5|  ,   �� *DisplaySentAt+5�  ,   5�     5�     5�     5�     �� 5�     $��8,2,0,33q�6   $��2��5�     $��2��5�     ��5�     �� 5�     " ��6   .�5�     ��  � y�$ 	Collapsed=�5�  !    5�     �� 5�      6*   �� *IsMessagesLoading+ True5�  *   .a�5�  "   ��  � y�$ Visible=�5�  )   5�     5�     .+�5�     �� $��60��6   ���#444444$ 5��5�     $��5q�5�     �� 5�     $��60��6   ���#444444$ 5��5�     $��5q�5�     �� 5�     $��60��6   ���#444444$ 5��5�     $��5q�5�     5�     �� 5�     $��35(�6   ���#AAAAAA$��80 0 0 0q�5�     $��Center=�5�     $
��Auto��5�     $��WrapWithOverflow=�5�     ) �� TextAlignment$) Center=�5�     y�5�     `� 5�      ! GroupStatusToMessageConverter* `� 	Converter#* [ 6#   .1�5�     �� + �� Path$+ Groups.Count�6"   �� 5�     $+ SelectedGroup�6"   �� 5�     $ + SuppressGroupStatusMessage�6"   5�     ��5�     `� 5�     #* [ 6#   .1�5�     �� $+ Groups.Count�6"   �� 5�     $+ SelectedGroup�6"   �� 5�     $ + SuppressGroupStatusMessage�6"   5�     5�     �� 5�     $��2��6   $��1��5�     $��68��5�     ���#36393F$��Bottom=�5�     �� 5�     -   5        , ��Name$, MessageTextBox��5�     $
��auto��5�     $��0��5�     $��2��5�     $��40��5�     $	��200��5�     $��14(�5�     $��0q�5�     $��Bottom=�5�       
MessageBox#��[ 5�     
�������- �� TextWrapping$
- Wrap=�5�     . �� AcceptsReturn$
. True��5�     $��8,7,8,11q�5�     ��5�     �� *	Message+$ PropertyChanged=�.��5�  7   {�5�     `� 5�      & "GroupSelectionToIsEnabledConverter#* [ 6#   .1�5�     �� $+ Groups.Count�6"   �� 5�     $+ SelectedGroup�6"   5�     .��5�  %   5�     �� 5�     $ 
Horizontal=�6   $
��Left=�5�     $��1��5�     $��1��5�     $��10,0,0,0q�5�     .+�5�     �� $��75��6   $
��Send��5�     $��0q�5�     $��50��5�     ��5�     �� *SendMessageCommand+��5�     �� *User.SelectedColor+5�     5�     5�     5�     5�     