   M S B A M L   `   `   `  ����  QuickTechPOS-    'QuickTechPOS.Views.PrintJobStatusDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	      PrintJobsStatus  � Title#  �   5   	   $	��450��63   $	��700��6@    � WindowStartupLocation$ CenterOwner=�5   	   .��5	      � $��10q�6    � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   ��   PrintQueueManager#���  6   $��18(�5      $
��Bold"�6"   $��0,0,0,10q�5      &   System.Windows.Controls.DataGrid  5   
   $��1��6      AutoGenerateColumns$ False��5         
IsReadOnly$
 True��5         SelectionMode$ Single=�5      $��0,0,0,10q�5         AlternatingRowBackground �#F5F5F5��5      �� *	PrintJobs+ �� SelectedItem 5      �� *SelectedJob+   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5        Created,  &System.Windows.Controls.DataGridColumn	   Header#	 �  6%   
   Width5  /System.Windows.Controls.DataGridLengthConverter$	
 140 5    %   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5   %   �� *	CreatedAt+ �� StringFormat$ {0:MM/dd/yyyy HH:mm:ss}��5    %     5!       
 Status#	 �  6%   $
 * 5#   %    5"   %   �� *StatusMessage+5#   %     5$        Attempts#	 �  6%   $
 70 5&   %    5%   %   �� *
RetryCount+5&   %     5'        LastAttempt#	 �  6%   $
 90 5)   %    5(   %   �� *LastAttempt+$ {0:HH:mm:ss}��5)   %   5+      �� 5-   
   $��2��6   $��0,0,0,10q�5/      ���#333333��5.      �� *StatusMessage+50      �� 52   
   $��3��6    �� Orientation$ 
Horizontal=�53      $��Right=�54      .+�55      ��  	 Retry#���  6   $��80��58      $��0,0,10,0q�59      ��56      �� *RetryJobCommand+{�57      �� *CanRetryJob+59      �� 5:        CancelButton#���  6   $��80��5=      $��0,0,10,0q�5>      ��5;      �� *CancelJobCommand+{�5<      �� *CanCancelJob+5>      �� 5?       
 Remove#���  6   $��80��5B      $��0,0,10,0q�5C      ��5@      �� *RemoveJobCommand+{�5A      �� *CanRemoveJob+5C      �� 5D       	 Refresh#��� 	 6   $��80��5F      $��0,0,10,0q�5G      ��5E      �� *RefreshCommand+5G      �� 5H       
 CloseButton#��� 
 6   $��80��5J      ��5I      �� *CloseCommand+5J      5K      5L      5M      