   M S B A M L   `   `   `  ����d  `PresentationFramework.Classic, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35jMclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.ClassicMicrosoft.Windows.Themes  5   	    Launcher   Launcher.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   `ThemesMclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Classic     5   	     � Title$  
MainWindow��5   	   $	��515��6   $	��352��6)    � WindowStyle$
 None=�65    � AllowsTransparency . ���	#FFE0E5EB$��1q�6y    � WindowStartupLocation$ CenterScreen=�6�    � Icon$% Resources/GroovifyLogoSmall.ico��6�    � 	Resources 5         ButtonStyle1 �� 
TargetType �� Property �� Value	 �� CornerRadius
 J� SnapsToDevicePixels �� StrokeDashArray �� Color �� Opacity �� 
TargetType5   /Microsoft.Windows.Themes.ClassicBorderDecorator  Name   BorderBrush   BorderThickness   BorderStyle   
Background �� ContentStringFormat �� ContentStringFormat �� Triggers P� Property P� Value �� 
TargetName��%b  &	        �� 5   
   " ��6%   .�5      ��  �@�FocusVisualStyle 5	      �� 5
      .�5      ��  � �� 5      �� 5   "   .R�5   &   �� $	 5��6-   ��6>   Z� ��6T   Z� ._�5   *   �� $��4q�64   
 . #��� ��6Z   $��1��6�   $	 1 2R�6�   ��6�   Z� 5   '   5   #   5      5      5      �� 5       � �� �	#FF0000006+   �� 5       � ��# � ��6+   �� 5       � �� 5      �� 5      $ 	#FFB93F3F��6&   $ 0Q�68   5      �� 5       � ��$ 3q�60   �� 5       � ��$ Center=�6;   �� 5        � ��$ Center=�69   �� 5!       � ��$ 0,0,1,1q�6(   �� 5"       � �� 5#      �� 5$      " ��6&   .R�5%        $ ContentContainer��68   # z��6R   $ 3q�6~   $ Raised=�6�   # z��6�   
 . ._�5&      �� #��z��6/   #��z��6c   # z 6�   #��z��6�   #��z��6  ��. #
 z
 6D  #��z��6�  5'       5(      P� 5)       �@J�IsKeyboardFocused . . �5*   "   �� $ ContentContainer��6@    �@ BorderStyle$ RaisedFocused=�6^   5+      P� 5,       �@��IsDefaulted . . �5-   "   �� $ ContentContainer��6@    �@ BorderStyle$ RaisedFocused=�6^   5.      P� 5/       � �� . . �50   "   �� $ ContentContainer��6@    �@ BorderStyle$ RaisedPressed=�6^   51      P� 52       �@d�	IsChecked$
 TrueY�6H   . �53   "   �� $ ContentContainer��6@    �@ BorderStyle$ RaisedPressed=�6^   54      P� 55       � {� .  . �56   "   �� # � ��6?    � ��57      59      5;      5<      5=      .��5>      � ���	#FF2E2E2E.+�5?   
   � ���	#FF2E2E2E$��42��6&   $	��Top=�62    J� Opacity$	 0.9Q�6J   .+�5@      A� -   5         A�Name$ Close��5@      ���	#FFA3A8B2$
��Left=�66   $��10��6Q   $��
330,16,0,0q�6]   $	��Top=�6q   $��10��6�   6�   A� 5A      -   5        $	 Min��5A      ���	#FFA3A8B2$
��Left=�64   $��10��6O   $��
315,16,0,0q�6[   $	��Top=�6o   $��10��6�   6�   �� 5C      $��1q�6   $	 5��6)   $��Right=�6:   $��26��6V   $��0,264,90,-248q�6b   $	��Top=�6y   $	��172��6�   ���	#FF444444._�5D      �� -   5         ��Name$ Username��5D       �� TextWrapping$
 Wrap=�6,   $��Username��6@   $��0q�6f   $��	3,3,-1,-3q�6z   ���	#FFE6E6E6��6P   Z� 6�   5E      �� 5F      $��1q�6   $	 5��6)   $��Right=�6:   $��26��6V   $��0,295,90,-279q�6b   $	��Top=�6y   $	��172��6�   ���	#FF4444446�   5G      �� 5H   
   $J��DThis software is in no way affiliated or approved by Grooveshark.com��6   $
��Left=�6_   $��34,497,0,-4q�6z   $	��Top=�6�   $��9(�6�   ���	#FFA0A0A0��6�   Z� 6�   G� 5I   
   -   5         G�Name$ Password��5I      $
��Left=�6(   $��
94,299,0,0q�6C   $	��Top=�6W    J� RenderTransformOrigin$ -6.5,3.7&�6o   $	��167��6�   $��0q�6�     G� Password$  Password��6�   ���	#FFE6E6E6��6�   Z� 6P  �� 5J   
   -   5        ! ��Name$! Current��5J      $��Center=�6!   $��Logging In
��6E   $
��Left=�6_   $��	0,242,0,0q�6z   $	��Top=�6�   $ 0.656,0.027&�6�   $	��350��6�   $y�Hidden=�6�   �� 5K   
   $	��178��6   $��
86,50,86,0q�6   $	��Top=�61   $ 
0.484,0.47&�6I   $=��7pack://siteoforigin:,,,/Resources/GroovifyLogoSmall.png��6l   �� 5L   
   $	 5��6   $��1q�6"   $
��Left=�66   $��22��6Q   $��
86,344,0,0q�6]   $	��Top=�6q   $��86��6�   ���	#FF00A8EC._�5M      �� -   5        $��Register��5M      ���	#FF00A8EC$��	0,-3,0,-1q�6V   ��6?   Z� 6i   5N      �� 5O   
   $	 5��6   $��1q�6"   $
��Left=�66   $��22��6Q   $��178,344,0,0q�6]   $	��Top=�6r   $��83��6�   ���	#FF00A8EC._�5P      �� -   5        $��Login��5P      ���	#FF00A8EC$��	0,-2,0,-1q�6S   ��6<   Z� 6f   5Q      �� 5R   
   -   5        " ��Name$" 
RememberMe��5R      $��Remember Me��6'   $
��Left=�6=   $��
90,323,0,0q�6X   $	��Top=�6l   $	��171��6�   # d� 	IsChecked$
# TrueY�6�   ���	#FF7E7E7E6�   5S      5T      