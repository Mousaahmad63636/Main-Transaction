   M S B A M L   `   `   `  ����   QuickTechPOS*    $QuickTechPOS.Views.CloseDrawerDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml        CloseDrawerTitle  � Title#  �   $	��660��$	��500�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=�.��� $��15q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��#���   $��20(�$
��Bold"����#2C3E50$��0,0,0,15q�� $��1�� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0�����#F8F9FA���#DADADA$��1q� �� CornerRadius$ 4�� �� Padding$ 15q�$��0,0,0,15q�._��  � ColumnDefinitions �� $��* ��� $��* � �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��  Cashier#���  $
��Bold"�$��0,0,0,5q��� $��0��$��1��$��0,0,0,5q����� *Drawer.CashierName+�� $��1��$��0��  OpenedAt#���  $
��Bold"�$��0,0,0,5q��� $��1��$��1��$��0,0,0,5q����� *Drawer.FormattedOpenedAt+�� $��2��$��0��  OpeningBalance#���  $
��Bold"�$��0,0,0,5q��� $��2��$��1��$��0,0,0,5q����� * Drawer.FormattedOpeningBalance+�� $��3��$��0��  SystemBalance#���  $
��Bold"�$��0,0,0,5q��� $��3��$��1��$��0,0,0,5q����� * Drawer.FormattedCurrentBalance+�� $��1�����#F8F9FA���#DADADA$��1q�$ 4��$ 15q�$��0,0,0,15q�._��  �� $��* ��� $��* ��� $��* ��� $��* ��� $��* � �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��  
DailySales#���  $
��Bold"�$��Center=�$��0,0,0,5q��� $��0��$��1�� 
 CashIn#���  $
��Bold"�$��Center=�$��0,0,0,5q��� $��0��$��2��  CashOut#���  $
��Bold"�$��Center=�$��0,0,0,5q��� $��0��$��3��  
TotalSales#���  $
��Bold"�$��Center=�$��0,0,0,5q��� $��0��$��4�� 	 NetSales#��� 	 $
��Bold"�$��Center=�$��0,0,0,5q��� $��1��$��0��$��Center=����#2ECC71$
��Bold"�$��0,0,0,5q����� *Drawer.FormattedDailySales+�� $��1��$��1��$��Center=����#3498DB$
��Bold"�$��0,0,0,5q����� *Drawer.FormattedCashIn+�� $��1��$��2��$��Center=����#E74C3C$
��Bold"�$��0,0,0,5q����� *Drawer.FormattedCashOut+�� $��1��$��3��$��Center=����#2980B9$
��Bold"�$��0,0,0,5q����� *Drawer.FormattedTotalSales+�� $��1��$��4��$��Center=����#8E44AD$
��Bold"�$��0,0,0,5q����� *Drawer.FormattedNetSales+�� $��2�� 
 CountCashMessage#��� 
 $
��Wrap=�$��0,0,0,10q�� $��3�� �� $
��Auto ��� $��* �.+��� $��0��$��$��$
��Bold"�$��Center=�$��5,0,5,0q��� $��1��$��32��$	��5,0q�$��Center=����� *ClosingBalance+ �� StringFormat$ N2�� �� UpdateSourceTrigger$ PropertyChanged=��� $��4�����#DADADA$��1q�$ 4��$ 15q�$��	0,15,0,15q����� *CalculatedDifference+  DifferenceToColorConverter	 �� 	Converter#	 [ ._��  �� $��* ��� $
��Auto �.+��� $��0��  DifferenceText#���  $
��Bold"�$��Center=��� $��1��$
��Bold"�$��Center=����� *CalculatedDifference+$ ${0:F2}������ *CalculatedDifference+ " DifferenceToTextColorConverter#	 [ �� $��5��  OptionalNotes#���  $
��Bold"�$��0,0,0,5q��� $��6��$��80��
 �� TextWrapping$

 Wrap=� �� AcceptsReturn$
 True��" �� VerticalScrollBarVisibility$
 Auto=�$��5q�$��0,0,0,10q����� *ClosingNotes+$ PropertyChanged=��� $��7��
���  ��$
��Wrap=�$��0,5,0,0q����� *ErrorMessage+y��� *ErrorMessage+  StringToVisibilityConverter#	 [ � $��8�� � IsIndeterminate$
 True��$��4��$��0,5,0,0q�y��� *IsProcessing+   BooleanToVisibilityConverter#	 [ �� $��2�� �� Orientation$ 
Horizontal=�$��Center=�$��0,15,0,0q�.+���   PrintAfterClosing#���  $��Medium"�$	��159�� d� 	IsChecked �� *PrintReportAfterClosing+��   CancelButton#���  $��80��$��32��$��0,0,10,0q����� *CancelCommand+�� -     CloseDrawerBtn#���  $	��110��$��40�����#E74C3C
�������$
��Bold"�$��14(����� *CloseDrawerCommand+��   DrawerReport#���  $	��100��$��32��$��0,0,10,0q����� *PrintReportCommand+