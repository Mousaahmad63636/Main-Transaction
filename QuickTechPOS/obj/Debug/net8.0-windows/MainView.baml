   M S B A M L   `   `   `  ����  QuickTechPOS!    QuickTechPOS.Views.MainView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .��5	      �   � RowDefinitions  5
   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   � ���#2196F3$��0��6$    � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $��* �6#   �� 5      $
��Auto �6#   �� 5      $
��Auto �6#   .+�5      ��    ApplicationTitle#���   6   $��0��5      $��20(�5      $
��Bold"�5      
�������$��15,10q�5      �� 5"      $��1��5#      $��16(�5$      
�������$��Center=�5&      $��Center=�5'      ��5"      �� *WelcomeMessage+5'      �� 5*      -   5         ��Name$ LanguageButton��5*      $��2��5+        LanguageMenu#���  5,      
������ 
�������$��14(�5/      $��0q�50      $
��15,0q�51      $
��10,5q�52       � Cursor$
 Hand��53      ��54      �� 55       �� 
TargetType" ��6   .�56      ��  �� Property � �� �� Value
 ���� 67    �� Triggers 57      P� 58       P� Property �@J�IsMouseOver	 P� Value$
	 True��6=   . �59   "   ��  � �� �#1976D26?   5:      5<      5>      �� 5C        LogoutButton#���  6   $��3��5D      
������ 
�������$��14(�5H      $��0q�5I      $
��15,0q�5J      $
��10,5q�5K      $
 Hand��5L      ��5E      �� *LogoutCommand+��5M      �� 5N      " ��6   .�5O      ��  � ��
 ���� 67    5P      P� 5Q       �@J�IsMouseOver$
	 True��6=   . �5R   "   ��  � �� �#1976D26?   5S      5U      5W      5X      �� 5[   
   -   5        
 ��Name$
 MainContent��5[      $��1��6.   5\      5]      