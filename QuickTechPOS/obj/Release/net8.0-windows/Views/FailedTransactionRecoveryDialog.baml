   M S B A M L   `   `   `  ���� G  CQuickTechPOS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:QuickTechPOS.ViewsQuickTechPOS.Views  8    2QuickTechPOS.Views.FailedTransactionRecoveryDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     2local clr-namespace:QuickTechPOS.Views       � Title$!  Transaction Recovery Center��$	��600��$	��900�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ 	CanResize=�.��� $��10q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��$��0,0,0,10q�.+��� $!��Transaction Recovery Center��$��22(�$
��Bold"�$��0,0,0,5q��� $,��&Review and recover failed transactions��$��14(����#555555� $��1�� � ColumnDefinitions �� $��* ��� $
��Auto ��� $
��1.5* �.+��� $��0�����#DDDDDD$��1q� �� CornerRadius$ 4�����#F9F9F9 �� Padding$ 5q�._��  �� $
��Auto ��� $��* �.+��� $��0��$��Failed Transactions��$��SemiBold"�$��5,5,5,10q�&   System.Windows.Controls.DataGrid  -     Name$ FailedTransactionsGrid��$��1��   AutoGenerateColumns$ False��	   CanUserAddRows$	 False��
   CanUserDeleteRows$
 False��   
IsReadOnly$
 True��   GridLinesVisibility$ 
Horizontal=�   AlternatingRowBackground �#F5F5F5$��0q�   SelectionMode$ Single=����� *FailedTransactions+ �� SelectedItem �� *SelectedFailedTransaction+   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ ID��   Width5  /System.Windows.Controls.DataGridLengthConverter$ 50 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *FailedTransactionId+  $
 Date��$	 120  �� *FormattedAttemptDate+  $ Amount��$ 80  �� *TotalAmount+ �� StringFormat$ ${0:N2}��  $ Customer��$	 120  �� *CustomerName+  $ Cashier��$	 120  �� *CashierName+  $ Error��$ *  �� *UserFriendlyError+�� $��1��$��5��$��Center=�$��Stretch=����#DDDDDD�� $��2�����#DDDDDD$��1q�$ 4��
�������$ 10q�._��  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��$��Transaction Details��$��SemiBold"�$��0,0,0,10q�� $��1��$��0,0,0,10q� �� $
��Auto ��� $��* ��� $
��Auto ��� $��* � �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��Transaction ID:��$��0,0,10,5q�$��SemiBold"��� $��0��$��1��$��0,0,20,5q����� */-SelectedFailedTransaction.FailedTransactionId+�� $��0��$��2��$��Date:��$��0,0,10,5q�$��SemiBold"��� $��0��$��3��$��0,0,0,5q����� *0.SelectedFailedTransaction.FormattedAttemptDate+�� $��1��$��0��$��	Customer:��$��0,0,10,5q�$��SemiBold"��� $��1��$��1��$��0,0,20,5q����� *(&SelectedFailedTransaction.CustomerName+�� $��1��$��2��$��Cashier:��$��0,0,10,5q�$��SemiBold"��� $��1��$��3��$��0,0,0,5q����� *'%SelectedFailedTransaction.CashierName+�� $��2��$��0��$��Total Amount:��$��0,0,10,5q�$��SemiBold"��� $��2��$��1��$��0,0,20,5q����� *'%SelectedFailedTransaction.TotalAmount+$ ${0:N2}���� $��2��$��2��$��Payment:��$��0,0,10,5q�$��SemiBold"��� $��2��$��3��$��0,0,0,5q����� *)'SelectedFailedTransaction.PaymentMethod+� $��2��$��0,0,0,10q� �� $
��Auto ��� $
��Auto �.+��� $��0��$��Error Information��$��SemiBold"�$��0,0,0,5q��� $��1�����#FFEEEE$��1q����#FFEEEE$ 10q�$ 4��._��� .+��� $
��Wrap=����#CC0000���� *(&SelectedFailedTransaction.ErrorMessage+�� $��0,5,0,0q�$��Italic$�$��12(����� *,*SelectedFailedTransaction.FailureComponent+$ Component: {0}���� $��0,5,0,0q�$��Italic$�$��12(����� *&$SelectedFailedTransaction.RetryCount+$ Retry Count: {0}��� $��3�� �� $
��Auto ��� $��* �.+��� $��0��$��
Cart Items��$��SemiBold"�$��0,0,0,5q�  -   $ CartItemsGrid��$��1��$ False��$	 False��$
 False��$
 True��$ 
Horizontal=� �#F9F9F9$��1q����#DDDDDD���� *	CartItems+   $ Product��$ *  �� *Product.Name+  $	 Qty��$ 50  �� *
Quantity+  $ 
Unit Price��$ 80  �� *	UnitPrice+$ ${0:N2}��  $ Discount��$ 80  �� *
Discount+$ ${0:N2}��  $ Total��$ 80  �� *Total+$ ${0:N2}���� $��4�����#E8F4FF$��1q����#E8F4FF$ 10q�$ 4��$��	0,10,0,10q�._��� .+��� $ ��Suggestions for Resolution��$��SemiBold"�$��0,0,0,5q��� $
��Wrap=����� *ResolutionSuggestion+�� $��5�� �� Orientation$ 
Horizontal=�$��Right=�.+��� -    ��Name$ RetryButton��$��Retry Transaction��$	��150��$��35��$��0,0,10,0q����#2196F3
�������$��0q����� *RetryCommand+����  �� 
TargetType" ��.���  �� Property � {� �� Value �� *CanRetrySelected+��  � �� ��  �� 
TargetType" ��.R��� #��z��#��z��#��z��$ 4��._��� $��Center=�$��Center=� �� Triggers P�  P� Property �@J�IsMouseOver P� Value$
 True��. ���  � �� �#1976D2P�  � {�$ False��. ���  � �� �#CCCCCC�� -   $ CancelTransactionButton��$��Cancel Transaction��$	��150��$��35��$��0,0,10,0q����#FF5252
�������$��0q����� *CancelTransactionCommand+���� " ��.���  � {� �� *IsTransactionSelected+��  � �� �� " ��.R��� #��z��#��z��#��z��$ 4��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ���  � �� �#D32F2FP�  � {�$ False��. ���  � �� �#CCCCCC�� -   $ CloseButton��$��Close��$	��100��$��35�����#EEEEEE$��0q����� " ��.���  � �� �� " ��.R��� #��z��#��z��#��z��$ 4��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ���  � �� �#DDDDDD�� $��2�����#F5F5F5���#DDDDDD$��1q�$��0,10,0,0q�$
 10,5q�$ 4��._��� $ 
Horizontal=�.+��� $��Status: ��$��SemiBold"��� ���� *StatusMessage+�� $	�� | ���� $��Failed Transactions: ���� ���� *FailedTransactions.Count+�� $	�� | ���� $��Refresh��
������ $��0q� � Cursor$
 Hand������ *RefreshCommand+