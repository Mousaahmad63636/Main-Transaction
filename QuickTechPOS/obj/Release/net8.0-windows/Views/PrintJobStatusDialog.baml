   M S B A M L   `   `   `  ����   QuickTechPOS-    'QuickTechPOS.Views.PrintJobStatusDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006        PrintJobsStatus  � Title#  �   $	��450��$	��700�� � WindowStartupLocation$ CenterOwner=�.��� $��10q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+���   PrintQueueManager#���  $��18(�$
��Bold"�$��0,0,0,10q�&   System.Windows.Controls.DataGrid  $��1��   AutoGenerateColumns$ False��   
IsReadOnly$
 True��   SelectionMode$ Single=�$��0,0,0,10q�   AlternatingRowBackground �#F5F5F5���� *	PrintJobs+ �� SelectedItem �� *SelectedJob+   Columns 0  *System.Windows.Controls.DataGridTextColumn    Created,  &System.Windows.Controls.DataGridColumn	   Header#	 �  
   Width5  /System.Windows.Controls.DataGridLengthConverter$	
 140 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *	CreatedAt+ �� StringFormat$ {0:MM/dd/yyyy HH:mm:ss}��   
 Status#	 �  $
 *  �� *StatusMessage+    Attempts#	 �  $
 70  �� *
RetryCount+    LastAttempt#	 �  $
 90  �� *LastAttempt+$ {0:HH:mm:ss}���� $��2��$��0,0,0,10q����#333333���� *StatusMessage+�� $��3�� �� Orientation$ 
Horizontal=�$��Right=�.+���  	 Retry#���  $��80��$��0,0,10,0q����� *RetryJobCommand+{��� *CanRetryJob+��   CancelButton#���  $��80��$��0,0,10,0q����� *CancelJobCommand+{��� *CanCancelJob+��  
 Remove#���  $��80��$��0,0,10,0q����� *RemoveJobCommand+{��� *CanRemoveJob+��  	 Refresh#��� 	 $��80��$��0,0,10,0q����� *RefreshCommand+��  
 CloseButton#��� 
 $��80������ *CloseCommand+