   M S B A M L   `   `   `  ����   QuickTechPOS"    QuickTechPOS.Views.LoginView   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .��� ���#F1F5F9.+��� $	��400��$	��450��
�������  �� CornerRadius$  8�����#E2E8F0$��1q�   DropShadowEffect J� Effect# [  ._��  � RowDefinitions �� $
��Auto ��� $��* �.+��� $��0�����#1E293B$  8,8,0,0�� �� Padding$
 0,20q�._��� $��Center=�.+���   ApplicationTitle#���  $��28(�$
��Bold"�
�������$��Center=��� $��Point of Sale System��$��16(����#94A3B8$��Center=�$��0,4,0,0q��� $��1��$��40,30q�.+���   Username#���  $��0,0,0,6q�$��14(�$��SemiBold"����#334155�� $��1q����#CBD5E1$  4��$��0,0,0,16q�._��� -    ��Name$ UsernameTextBox��$��15(�$��12,10q�$��0q�
������ ���� *
Username+ �� UpdateSourceTrigger$ PropertyChanged=���   Password#���  $��0,0,0,6q�$��14(�$��SemiBold"����#334155�� $��1q����#CBD5E1$  4��$��0,0,0,16q�._�G� -    G�Name$ PasswordBox��$��15(�$��12,10q�$��0q�
������ �� ���#EF4444$
��Wrap=�$��0,0,0,16q�$��20��$��Medium"����� *ErrorMessage+��   SignInButton#���  ���#2563EB
�������$��16(�$��SemiBold"�$��16,12q�$��0,8,0,0q� � Cursor$
 Hand������ *LoginCommand+ � 	Resources 	 �� 
TargetType
 �� Property �� Value��%9   '��       �� "	 ��.��� 
 �@��CornerRadius$ 4������ "	 ��.��� 
 � �� �#2563EB�� 
 � ��$ 0q� �� Triggers P�  P� Property �@J�IsMouseOver P� Value$
 True��. ��� 
 � �� �#1D4ED8P�  � {�$ False��. ��� 
 � �� �#94A3B8�� $��QuickTech POS v2.0�����#64748B$��12(�$��Center=�$��0,30,0,0q���  �� Orientation$ 
Horizontal=�$��Center=�$��0,15,0,0q�.+��� -    ��Name$ LanguageSelector��$	��150��$��30��� $��0,16,0,0q�y��� *	IsLoading+   BooleanToVisibilityConverter �� 	Converter# [ .+��  � IsIndeterminate$
 True��$��4��