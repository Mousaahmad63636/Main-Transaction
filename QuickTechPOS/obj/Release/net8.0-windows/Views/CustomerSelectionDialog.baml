   M S B A M L   `   `   `  ����   QuickTechPOS0    *QuickTechPOS.Views.CustomerSelectionDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml        SelectCustomerTitle  � Title#  �   $	��500��$	��600�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=� � 	Resources   
HeaderText �� 
TargetType �� Property �� Value  
InputField  PrimaryButton �� 
TargetType �� CornerRadius	 �� Triggers
 P� Property P� Value  SecondaryButton �� BasedOn  SuccessButton��%  &	       &	 v     &	     &	 �    7  &	 W    7  �� " ��.���  � ��$ SemiBold"���  � ��$ 14(���  � �� �#1E293B��  � ��$ 0,0,0,8q��� " ��.���  � ��$ 38����  � ��$
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
 True��. ���  � �� �#059669.���� ���#F8FAFC$��0q�._�� $��24q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��#���   $��22(�$
��Bold"����#0F172A$��0,0,0,20q�� $��1��$��0,0,0,16q� � ColumnDefinitions �� $��* ��� $
��Auto �.+��� -    ��Name$ SearchTextBox��$��0��#��[ $��0,0,8,0q��� -   $��1��  SearchButton#���  #��[ $	��100���� $��2��
����������#E2E8F0$��1q�$ 6��$��0,0,0,20q�._��� -    ��Name$ CustomerListView��$��0q�
������  �� SelectionMode$ Single=� �� View �� .P���   CustomerName#���   �� Width$	 200�� �� CellTemplate �� .R��� $��SemiBold"����#0F172A���� *Name+��   CustomerPhone#���  $	 140�� �� .R��� ���#334155���� *Phone+��  	 CustomerBalance#��� 	 $	 120�� �� .R��� $��SemiBold"����#2563EB���� *FormattedBalance+��  
 CustomerEmail#��� 
 $	 180�� �� .R��� ���#334155$
��Wrap=����� *Email+���� " ��.���  � ��$ Stretch=���  � ��$	 8,6q�	 P� 
 �@��
IsSelected$
 True��. ���  � �� �#EFF6FF��  � �� �#BFDBFEP� 
 �@J�IsMouseOver$
 True��. ���  � �� �#F1F5F9�� $��3�� �� Orientation$ 
Horizontal=�$��Right=�.+��� -     CancelButton#���  $	��100��#��[ $��0,0,12,0q��� -     SelectButton#���  $	��100��#��[ 