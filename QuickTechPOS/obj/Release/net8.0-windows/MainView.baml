   M S B A M L   `   `   `  ����   QuickTechPOS!    QuickTechPOS.Views.MainView   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .���   � RowDefinitions  �� $
��Auto ��� $��* �.+�� ���#2196F3$��0�� � ColumnDefinitions �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+���    ApplicationTitle#���   $��0��$��20(�$
��Bold"�
�������$��15,10q��� $��1��$��16(�
�������$��Center=�$��Center=����� *WelcomeMessage+�� -    ��Name$ LanguageButton��$��2��  LanguageMenu#���  
������ 
�������$��14(�$��0q�$
��15,0q�$
��10,5q� � Cursor$
 Hand������  �� 
TargetType" ��.���  �� Property � �� �� Value
 ����  �� Triggers P�  P� Property �@J�IsMouseOver	 P� Value$
	 True��. ���  � �� �#1976D2��   LogoutButton#���  $��3��
������ 
�������$��14(�$��0q�$
��15,0q�$
��10,5q�$
 Hand������ *LogoutCommand+���� " ��.���  � ��
 ����  P�  �@J�IsMouseOver$
	 True��. ���  � �� �#1976D2�� -   
 ��Name$
 MainContent��$��1��