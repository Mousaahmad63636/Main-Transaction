   M S B A M L   `   `   `  ����   QuickTechPOS*    $QuickTechPOS.Views.AddCustomerDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml        AddCustomerTitle  � Title#  �   $	��460��$	��450�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=� � 	Resources   
HeaderText �� 
TargetType �� Property �� Value  
InputField  TextAreaField �� BasedOn  PrimaryButton �� 
TargetType	 �� CornerRadius
 �� Triggers P� Property P� Value  SecondaryButton  SuccessButton��%  &	       &	 v     &	 0    7  &	     &	 �    7  &	 G    7  �� " ��.���  � ��$ SemiBold"���  � ��$ 14(���  � �� �#1E293B��  � ��$ 0,0,0,6q��� " ��.���  � ��$ 38����  � ��$
 12,0q���  � ��$ Center=���  � �� �#CBD5E1��  � ��$ 1q���  � ��
 �������  � ��$ 0,0,0,16q��� " ��8   .���  � ��$ 80����  �@��TextWrapping$
 Wrap=���  �@��AcceptsReturn$
 True����  � ��$	 Top=��� # �@��VerticalScrollBarVisibility$
 Auto=���  � ��$ 12q��� " ��.���  � �� �#2563EB��  � ��
 �������  � ��$ SemiBold"���  � ��$ 0q���  � ��$ 38����  � ��$
 16,0q���  � �� �� " ��.R��� #��z��$	 4��#��z��#��z��._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ���  � �� �#1D4ED8P�  � {�$ False��. ���  � �� �#94A3B8�� " ��8   .���  � �� �#F1F5F9��  � �� �#334155��  � ��$ 1q���  � �� �#CBD5E1
 P�  �@J�IsMouseOver$
 True��. ���  � �� �#E2E8F0�� " ��8   .���  � �� �#10B981
 P�  �@J�IsMouseOver$
 True��. ���  � �� �#059669.���� ���#F8FAFC$��0q�._�� $��24q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��#���   $��22(�$
��Bold"����#0F172A$��0,0,0,20q��� $��1��$
��Auto=�.���� .+���   CustomerName#���  #��[ �� -    ��Name$ NameTextBox��#��[ ��   CustomerPhone#���  #��[ �� -   $ PhoneTextBox��#��[ ��  	 CustomerEmail#��� 	 #��[ �� -   $ EmailTextBox��#��[ ��  
 CustomerAddress#��� 
 #��[ �� -   $ AddressTextBox��#��[ �� $��2�� �� Orientation$ 
Horizontal=�$��Right=�$��0,16,0,0q�.+��� -     CancelButton#���  $	��100��#��[ $��0,0,12,0q��� -     
SaveButton#���  $	��100��#��[ 