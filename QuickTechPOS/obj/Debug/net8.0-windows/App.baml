   M S B A M L   `   `   `  ����G  CQuickTechPOS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:QuickTechPOSQuickTechPOS  5      A%clr-namespace:QuickTechPOS.ConvertersQuickTechPOS.Converters  5          QuickTechPOS.App   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      ,localclr-namespace:QuickTechPOS     5      <
converters%clr-namespace:QuickTechPOS.Converters     5        �� 	Resources  5       �� MergedDictionaries �� Source3  -System.Windows.Media.Effects.DropShadowEffect   DropShadowEffect   ShadowDepth   	Direction   Color   Opacity   
BlurRadius  
HeaderText �� 
TargetType	 �� Property
 �� Value  SectionTitle  	TouchText  TouchButton �� 
TargetType �� CornerRadius �� Padding �� Triggers P� Property P� Value  TouchCardPanel.   (QuickTechPOS.StringToVisibilityConverter  StringToVisibilityConverter3   -QuickTechPOS.PositiveIntToVisibilityConverter $  PositiveIntToVisibilityConverter+   %QuickTechPOS.BooleanToStringConverter  BooleanToStringConverter*   $QuickTechPOS.BooleanToBrushConverter 	 BooleanToBrushConverter6   0QuickTechPOS.InverseBooleanToVisibilityConverter '
 #InverseBooleanToVisibilityConverter-   'QuickTechPOS.DifferenceToColorConverter  DifferenceToColorConverter1   +QuickTechPOS.DifferenceToTextColorConverter " DifferenceToTextColorConverter9	   3QuickTechPOS.Converters.TableStatusToColorConverter  TableStatusToColorConverter=
   7QuickTechPOS.Converters.TableStatusToTextColorConverter # TableStatusToTextColorConverter?   9QuickTechPOS.Converters.TableStatusToBorderColorConverter % !TableStatusToBorderColorConverter:   4QuickTechPOS.Converters.BooleanToVisibilityConverter   BooleanToVisibilityConverter�� 5   
    5	      �� 5
      $e _pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml?�6%   �� 5      $h bpack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml?�6%   �� 5      $t npack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.Teal.xaml?�6%   �� 5      $s mpack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Pink.xaml?�6%   %1  &	        &	 ^     &	 #    &	 �    &	 s    &	 �    7   &	 �    &	 �    &	 �    &		 �    &	
 �    &	     &	 $    &	 7    &	 J    &	 ]    &	 p      5      $ 2Q�68   $	 270Q�6H   $ #1E293B��5      $
 0.08Q�6/   $ 8Q�6>   �� 5      " ��6'   .�5      �� 	 � ��$
 SemiBold"�6/   �� 5      	 � ��$
 16(�6-   �� 5      	 � ��
 �#1E293B6/   �� 5      	 � ��$
 0,0,0,6q�6+   5      �� 5      " ��6)   .�5      �� 	 � ��$

 Bold"�6/   �� 5      	 � ��$
 18(�6-   �� 5      	 � ��
 �#3341556/   �� 5      	 � ��$
 0q�6+   5      �� 5!      " ��6&   .�5"      �� 	 � ��$
 14(�6-   �� 5#      	 � ��
 �#64748B6/   �� 5$      	 � ��$
 0q�6+   5%      �� 5'      " ��6(   .�5(      �� 	 � ��$
 45��6+   �� 5)      	 � ��
 �#2563EB6/   �� 5*      	 � ��

 �����6/   �� 5+      	 � ��$
 0q�64   �� 5,      	 � ��$
 SemiBold"�6/   �� 5-      	 � ��$
 14(�6-   �� 5.      	 �@�Cursor$

 Hand��6+   �� 5/      	 � ��
 50      �� 51      " ��6*   .R�52      �� #��z��6%   #��z��53   %   #��z��54   %   $ 6��55   %   # z��56   %   ._�57   "   �� $��Center=�63   $��Center=�58   3   59      5:      5<       5=      P� 5>       �@J�IsMouseOver$
 True��65   . �5?      �� 	 � ��
 �#1D4ED867   5@      P� 5A       � ��$
 True��63   . �5B      �� 	 � ��
 �#1E40AF67   5C      P� 5D       � {�$ False��63   . �5E      �� 	 � ��
 �#94A3B867   5F      5H      �� 5J      " ��6+   .�5K      �� 	 � ��

 �����6/   �� 5L      	 � ��
 �#E2E8F060   �� 5M      	 � ��$
 1q�64   �� 5N      	 �@��CornerRadius$
 8��61   �� 5O      	 �@J�Effect8
   6+   5P        5R      60     5S      65     5T      6-     5U      6,     5V      68     5W      6/     5X      63   	  5Z      65   
  5[      69     5\      6;     5]      66   5_      5a      