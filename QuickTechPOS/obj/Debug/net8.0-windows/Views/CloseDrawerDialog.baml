   M S B A M L   `   `   `  ����  QuickTechPOS*    $QuickTechPOS.Views.CloseDrawerDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	      CloseDrawerTitle  � Title#  �   5   	   $	��660��5   	   $	��500��5   	    � WindowStartupLocation$ CenterOwner=�5   	    � 
ResizeMode$ NoResize=�5   	   .��5
      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   #���   5      $��20(�5      $
��Bold"�5      ���#2C3E50$��0,0,0,15q�5      � 5   
   $��1��6    5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5       $
��Auto �6    �� 5!      $
��Auto �6    �� 5"      $
��Auto �6    �� 5#      $
��Auto �6    �� 5$      $
��Auto �6    �� 5%      $��* �6    .+�5)      �� $��0��6   ���#F8F9FA���#DADADA$��1q�5,       �� CornerRadius$ 4��5-       �� Padding$ 15q�5.      $��0,0,0,15q�5/      ._�50      �  � ColumnDefinitions 51      �� 52      $��* �6+   �� 53      $��* �6+    55      �� 56      $
��Auto �6(   �� 57      $
��Auto �6(   �� 58      $
��Auto �6(   �� 59      $
��Auto �6(   .+�5=      �� $��0��6    $��0��6-     Cashier#���  5>       $
��Bold"�5?       $��0,0,0,5q�5@       �� 5A      $��0��6    $��1��6-   $��0,0,0,5q�5C       ��5B       �� *Drawer.CashierName+5C       �� 5E      $��1��6    $��0��6-     OpenedAt#���  5F       $
��Bold"�5G       $��0,0,0,5q�5H       �� 5I      $��1��6    $��1��6-   $��0,0,0,5q�5K       ��5J       �� *Drawer.FormattedOpenedAt+5K       �� 5M      $��2��6    $��0��6-     OpeningBalance#���  5N       $
��Bold"�5O       $��0,0,0,5q�5P       �� 5Q      $��2��6    $��1��6-   $��0,0,0,5q�5S       ��5R       �� * Drawer.FormattedOpeningBalance+5S       �� 5U      $��3��6    $��0��6-     SystemBalance#���  5V       $
��Bold"�5W       $��0,0,0,5q�5X       �� 5Y      $��3��6    $��1��6-   $��0,0,0,5q�5[       ��5Z       �� * Drawer.FormattedCurrentBalance+5[       5\      5]      �� 5`      $��1��6   ���#F8F9FA���#DADADA$��1q�5c   	   $ 4��5d   	   $ 15q�5e   	   $��0,0,0,15q�5f   	   ._�5g      �  5h      �� 5i      $��* �6+   �� 5j      $��* �6+   �� 5k      $��* �6+   �� 5l      $��* �6+   �� 5m      $��* �6+    5o      �� 5p      $
��Auto �6(   �� 5q      $
��Auto �6(   .+�5u      �� $��0��6    $��0��6-     
DailySales#���  5v      $
��Bold"�5w      $��Center=�5x      $��0,0,0,5q�5y      �� 5z      $��0��6    $��1��6-    
 CashIn#���  5{      $
��Bold"�5|      $��Center=�5}      $��0,0,0,5q�5~      �� 5      $��0��6    $��2��6-     CashOut#���  5�      $
��Bold"�5�      $��Center=�5�      $��0,0,0,5q�5�      �� 5�      $��0��6    $��3��6-     
TotalSales#���  5�      $
��Bold"�5�      $��Center=�5�      $��0,0,0,5q�5�      �� 5�      $��0��6    $��4��6-    	 NetSales#��� 	 5�      $
��Bold"�5�      $��Center=�5�      $��0,0,0,5q�5�      �� 5�      $��1��6    $��0��6-   $��Center=�5�      ���#2ECC71$
��Bold"�5�      $��0,0,0,5q�5�      ��5�      �� *Drawer.FormattedDailySales+5�      �� 5�      $��1��6    $��1��6-   $��Center=�5�      ���#3498DB$
��Bold"�5�      $��0,0,0,5q�5�      ��5�      �� *Drawer.FormattedCashIn+5�      �� 5�      $��1��6    $��2��6-   $��Center=�5�      ���#E74C3C$
��Bold"�5�      $��0,0,0,5q�5�      ��5�      �� *Drawer.FormattedCashOut+5�      �� 5�      $��1��6    $��3��6-   $��Center=�5�      ���#2980B9$
��Bold"�5�      $��0,0,0,5q�5�      ��5�      �� *Drawer.FormattedTotalSales+5�      �� 5�      $��1��6    $��4��6-   $��Center=�5�      ���#8E44AD$
��Bold"�5�      $��0,0,0,5q�5�      ��5�      �� *Drawer.FormattedNetSales+5�      5�      5�      �� 5�      $��2��6    
 CountCashMessage#��� 
 5�      $
��Wrap=�5�      $��0,0,0,10q�5�      � 5�      $��3��6    5�      �� 5�      $
��Auto �6'   �� 5�      $��* �6'   .+�5�      �� $��0��6   $��$��5�      $
��Bold"�5�      $��Center=�5�      $��5,0,5,0q�5�      �� 5�      $��1��6   $��32��5�      $	��5,0q�5�      $��Center=�5�      ��5�      �� *ClosingBalance+ �� StringFormat$ N2�� �� UpdateSourceTrigger$ PropertyChanged=�5�      5�      �� 5�      $��4��6   ���#DADADA$��1q�5�      $ 4��5�      $ 15q�5�      $��	0,15,0,15q�5�      ��5�      �� *CalculatedDifference+  DifferenceToColorConverter	 �� 	Converter#	 [ ._�5�      �  5�      �� 5�      $��* �6+   �� 5�      $
��Auto �6+   .+�5�      �� $��0��6      DifferenceText#���  5�       $
��Bold"�5�       $��Center=�5�       �� 5�      $��1��6    $
��Bold"�5�       $��Center=�5�       ��5�       �� *CalculatedDifference+$ ${0:F2}����5�       �� *CalculatedDifference+ " DifferenceToTextColorConverter#	 [ 5�       5�      5�      �� 5�      $��5��6     OptionalNotes#���  5�      $
��Bold"�5�      $��0,0,0,5q�5�      �� 5�      $��6��6   $��80��5�      
 �� TextWrapping$

 Wrap=�5�       �� AcceptsReturn$
 True��5�      " �� VerticalScrollBarVisibility$
 Auto=�5�      $��5q�5�      $��0,0,0,10q�5�      ��5�      �� *ClosingNotes+$ PropertyChanged=�5�      �� 5�      $��7��6   
���  ��$
��Wrap=�5�      $��0,5,0,0q�5�      ��5�      �� *ErrorMessage+y�5�      �� *ErrorMessage+  StringToVisibilityConverter#	 [ � 5�      $��8��6    � IsIndeterminate$
 True��5�      $��4��5�      $��0,5,0,0q�5�      y�5�      �� *IsProcessing+   BooleanToVisibilityConverter#	 [ 5�      �� 5  
   $��2��6    �� Orientation$ 
Horizontal=�5     $��Center=�5     $��0,15,0,0q�5     .+�5     ��   PrintAfterClosing#���  6   $��Medium"�5     $	��159��6    d� 	IsChecked 5     �� *PrintReportAfterClosing+5     �� 5       CancelButton#���  6   $��80��5
     $��32��5     $��0,0,10,0q�5     ��5	     �� *CancelCommand+5     �� 5     -   5          CloseDrawerBtn#���  5     $	��110��5     $��40��5     ���#E74C3C
�������$
��Bold"�5     $��14(�5     ��5     �� *CloseDrawerCommand+5  #   �� 5       DrawerReport#���  6   $	��100��5  	   $��32��5  	   $��0,0,10,0q�5  	   ��5  	   �� *PrintReportCommand+5  	   5     5     5     