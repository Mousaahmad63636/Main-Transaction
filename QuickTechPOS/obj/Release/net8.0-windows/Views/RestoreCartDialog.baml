   M S B A M L   `   `   `  ����   QuickTechPOS*    $QuickTechPOS.Views.RestoreCartDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml        RestoreCartTitle  � Title#  �   $	��500��$	��600�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=� � 	Resources   
HeaderText �� 
TargetType �� Property �� Value  PrimaryButton �� 
TargetType �� CornerRadius	 �� Triggers
 P� Property P� Value  SecondaryButton �� BasedOn  SuccessButton��%^  &	       &	 v     &	 �    7  &	 �    7  �� " ��.���  � ��$ SemiBold"���  � ��$ 14(���  � �� �#1E293B��  � ��$ 0,0,0,8q��� " ��.���  � �� �#2563EB��  � ��
 �������  � ��$ SemiBold"���  � ��$ 0q���  � ��$ 38����  � ��$
 16,0q���  � �� �� " ��.R��� #��z��$ 4��#��z��#��z��._��� $��Center=�$��Center=�	 P� 
 �@J�IsMouseOver$
 True��. ���  � �� �#1D4ED8P� 
 � {�$ False��. ���  � �� �#94A3B8�� " ��8   .���  � �� �#F1F5F9��  � �� �#334155��  � ��$ 1q���  � �� �#CBD5E1	 P� 
 �@J�IsMouseOver$
 True��. ���  � �� �#E2E8F0�� " ��8   .���  � �� �#10B981	 P� 
 �@J�IsMouseOver$
 True��. ���  � �� �#059669.���� ���#F8FAFC$��0q�._�� $��24q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��#���   $��22(�$
��Bold"����#0F172A$��0,0,0,20q��� $��1��
����������#E2E8F0$��1q�$ 6��$��0,0,0,20q�._��� -    ��Name$ HeldCartsListView��$��0q�
������  �� SelectionMode$ Single=� �� View �� .P���  
 CartId#���   �� Width$ 50�� �� CellTemplate �� .R��� $��SemiBold"����#0F172A���� *Id+��   CustomerName#���  $	 150�� �� .R��� $��SemiBold"����#0F172A���� *CustomerName+��   	ItemCount#���  $ 80�� �� .R��� ���#334155���� *Items.Count+��  	 Total#���  $	 100�� �� .R��� $��SemiBold"����#2563EB���� *TotalAmount+ �� StringFormat$ ${0:F2}����  	 	CreatedAt#��� 	 $	 150�� �� .R��� ���#334155���� *	CreatedAt+$ {0:g}������ " ��.���  � ��$ Stretch=���  � ��$	 8,6q�	 P� 
 �@��
IsSelected$
 True��. ���  � �� �#EFF6FF��  � �� �#BFDBFEP� 
 �@J�IsMouseOver$
 True��. ���  � �� �#F1F5F9�� $��2�� �� Orientation$ 
Horizontal=�$��Right=�.+��� -    
 CancelButton#��� 
 $	��100��#��[ $��0,0,12,0q��� -     RestoreButton#���  $	��120��#��[ 