   M S B A M L   `   `   `  ����   QuickTechPOS%    QuickTechPOS.Views.CashInDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml        CashInTitle  � Title#  �   $	��500��$	��450�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=� � 	Resources   
HeaderText �� 
TargetType �� Property �� Value  
InputField  PrimaryButton �� 
TargetType �� CornerRadius	 �� Triggers
 P� Property P� Value  SecondaryButton �� BasedOn  SuccessButton  	CardStyle��%�  &	       &	 v     &	     &	 �    7  &	 W    7  &	 �    �� " ��.���  � ��$ SemiBold"���  � ��$ 14(���  � �� �#1E293B��  � ��$ 0,0,0,6q��� " ��.���  � ��$ 38����  � ��$
 12,0q���  � ��$ Center=���  � �� �#CBD5E1��  � ��$ 1q���  � ��
 ������� " ��.���  � �� �#2563EB��  � ��
 �������  � ��$ SemiBold"���  � ��$ 0q���  � ��$ 38����  � ��$
 16,0q���  � �� �� " ��.R��� #��z��$ 4��#��z��#��z��._��� $��Center=�$��Center=�	 P� 
 �@J�IsMouseOver$
 True��. ���  � �� �#1D4ED8P� 
 � {�$ False��. ���  � �� �#94A3B8�� " ��8   .���  � �� �#F1F5F9��  � �� �#334155��  � ��$ 1q���  � �� �#CBD5E1	 P� 
 �@J�IsMouseOver$
 True��. ���  � �� �#E2E8F0�� " ��8   .���  � �� �#10B981	 P� 
 �@J�IsMouseOver$
 True��. ���  � �� �#059669�� " ��.���  � ��
 �������  � �� �#E2E8F0��  � ��$ 1q���  �@��CornerRadius$ 6����  �@��Padding$ 16q���  � ��$ 0,0,0,16q�.���� ���#F8FAFC$��0q�._�� $��24q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��#���   $��22(�$
��Bold"����#0F172A$��0,0,0,20q��� $��1��.+��� #��[ ._��  � ColumnDefinitions �� $	��130 ��� $��* � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��  DrawerId#���  $��SemiBold"����#64748B$��0,0,0,8q��� $��0��$��1��$��SemiBold"����#0F172A$��0,0,0,8q����� *Drawer.DrawerId+�� $��1��$��0��  CurrentBalance#���  $��SemiBold"����#64748B�� $��1��$��1��$
��Bold"����#10B981$��16(����� * Drawer.FormattedCurrentBalance+��  	 CashInAmount#��� 	 #��[ � $��0,0,0,16q� �� $
��Auto ��� $��* �.+��� $��0�����#E2E8F0$��38��$��38��$��1,1,0,1q����#CBD5E1$ 4,0,0,4��._��� $��$��$��SemiBold"�$��Center=�$��Center=����#334155�� $��1��#��[ $��0,1,1,1q����#CBD5E1���� *CashInAmount+ �� StringFormat$ N2�� �� UpdateSourceTrigger$ PropertyChanged=���  
 RequiredNotes#��� 
 #��[ �� $	��100�� �� TextWrapping$
 Wrap=� �� AcceptsReturn$
 True��" �� VerticalScrollBarVisibility$
 Auto=�$��12q����#CBD5E1
����������� *Notes+$ PropertyChanged=��� ���#EF4444$
��Wrap=�$��Medium"�$��0,16,0,0q����� *ErrorMessage+y��� *ErrorMessage+  StringToVisibilityConverter �� 	Converter# [ �  � IsIndeterminate$
 True��$��4��$��0,16,0,0q�y��� *IsProcessing+   BooleanToVisibilityConverter# [ �� $��2�� �� Orientation$ 
Horizontal=�$��Right=�$��0,24,0,0q�.+���   CancelButton#���  #��[ $	��100��$��0,0,12,0q����� *CancelCommand+��   ExecuteCashIn#���  #��[ $	��140������ *ExecuteCashInCommand+