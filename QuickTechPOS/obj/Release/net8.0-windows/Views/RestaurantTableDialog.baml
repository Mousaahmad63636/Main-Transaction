   M S B A M L   `   `   `  ���� G  CQuickTechPOS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:QuickTechPOSQuickTechPOS  A%clr-namespace:QuickTechPOS.ConvertersQuickTechPOS.Converters  .    (QuickTechPOS.Views.RestaurantTableDialog   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     ,localclr-namespace:QuickTechPOS     <
converters%clr-namespace:QuickTechPOS.Converters       � Title$  Select Restaurant Table��$
��1000��$	��700�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ 	CanResize=� � ShowInTaskbar$ False�����#F8FAFC � UseLayoutRounding$
 True�� � 	Resources :   4QuickTechPOS.Converters.BooleanToVisibilityConverter    BooleanToVisibilityConverter6   0QuickTechPOS.InverseBooleanToVisibilityConverter ' #InverseBooleanToVisibilityConverter.   (QuickTechPOS.StringToVisibilityConverter  StringToVisibilityConverter  
HeaderText �� 
TargetType �� Property �� Value  SectionTitle  	SearchBox	 �� 
TargetType
 �� CornerRadius ��Name  PrimaryButton �� Triggers P� Property P� Value  SecondaryButton �� BasedOn  SuccessButton 	 CardPanelStyle3  -System.Windows.Media.Effects.DropShadowEffect   ShadowDepth   	Direction   Color   Opacity   
BlurRadius 
 	TableCard �� Binding �� Value  StatusBadge  
StatusText  TableCardTemplate J� InputBindings d� MouseAction �� Command �� RelativeSource �� AncestorType �� CommandParameter � RowDefinitions �� Orientation��%/  &	        &	      &	 
     &	      &	 �     &	 �     &	 �    &	 �    7  &	     7  &		 �    &	
 J    &	 	    &	 �
    &	     7 
 7  7  7        �� " ��.���  � ��$ SemiBold"���  � ��$ 14(���  � �� �#1E293B��  � ��$ 0,0,0,5q��� " ��.���  � ��$
 Bold"���  � ��$ 18(���  � �� �#334155��  � ��$ 0,0,0,10q��� " ��.���  � ��$ 36����  � ��$ Center=���  � ��$
 12,0q���  � ��$ 1q���  � �� �#CBD5E1��  � ��$ 14(���  � �� �� "	 ��.R��� #��z��#��z��#��z��$
 6��._��� $ PART_ContentHost��#��z���� " ��.���  � �� �#2563EB��  � ��
 �������  � ��$ 0q���  � ��$
 16,8q���  � ��$ 36����  � ��$ SemiBold"���  � ��$ 14(���  � �� �� "	 ��.R��� #��z��#��z��#��z��$
 6��._��� $��Center=�$��Center=�#��z�� P�  �@J�IsMouseOver$
 True��. ���  � �� �#1D4ED8P�  � {�$ False��. ���  � �� �#94A3B8�� " ��8   .���  � �� �#64748B P�  �@J�IsMouseOver$
 True��. ���  � �� �#475569�� " ��8   .���  � �� �#10B981 P�  �@J�IsMouseOver$
 True��. ���  � �� �#059669�� " ��.���  � ��
 �������  � �� �#E2E8F0��  � ��$ 1q���  �@��CornerRadius$ 8����  �@J�Effect   $ 1Q�$	 270Q�$ #1E293B��$
 0.05Q�$ 8Q��� " ��.���  � ��
 �������  � ��$ 2q���  �@��CornerRadius$ 6����  � ��$ 4q���  �@��Padding$ 8q���  �@�Cursor$
 Hand����  � ��$	 140����  � ��$	 120����  �@J�Effect   $ 1Q�$	 270Q�$ #1E293B��$	 0.1Q�$ 4Q���  � �� �#E2E8F0 ��  �� *Status+ Occupied.a���  � �� �#DC2626��  � �� �#FEF2F2��  �@J�Effect   $ 2Q�$	 270Q�$ #DC2626��$	 0.3Q�$ 8Q���  �� *Status+ 	Available.a���  � �� �#10B981��  � ��
 �������  �� *Status+ Reserved.a���  � �� �#F59E0B��  � �� �#FFFBEB��  �� *Status+ Out of Service.a���  � �� �#64748B��  � �� �#F8FAFC��  �@J�Opacity$	 0.7Q�P�  �@J�IsMouseOver$
 True��. ���  �@J�Effect   $ 3Q�$	 270Q�$ #2563EB��$	 0.4Q�$ 12Q���  � ��$ 3q��� " ��.���  �@��CornerRadius$ 8����  �@��Padding$	 4,2q���  � ��$ Center=� ��  �� *Status+ 	Available.a���  � �� �#DCFCE7��  �� *Status+ Occupied.a���  � �� �#FEE2E2��  �� *Status+ Reserved.a���  � �� �#FEF3C7��  �� *Status+ Out of Service.a���  � �� �#F1F5F9�� " ��.���  � ��$ SemiBold"���  � ��$ 10(���  � ��$ Center=� ��  �� *Status+ 	Available.a���  � �� �#166534��  �� *Status+ Occupied.a���  � �� �#991B1B��  �� *Status+ Reserved.a���  � �� �#92400E��  �� *Status+ Out of Service.a���  � �� �#475569�� .R��� 8��   d� $ LeftDoubleClicke� �� * DataContext.SelectTableCommand+ �� 
 Window �� ._��  �� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��$
��Bold"�$��24(�$��Center=����#1E293B$��0,0,0,4q����� *DisplayName+�� $��1��8�� $��0,0,0,4q�._��� $ 
Horizontal=�$��Center=�.+��� $��10(�$��Center=�$��0,0,2,0q����� " �� ��  �� *Status+ 	Available.a���  � ��$	 ✓����  � �� �#166534��  �� *Status+ Occupied.a���  � ��$	 ●����  � �� �#991B1B��  �� *Status+ Reserved.a���  � ��$
 📋����  � �� �#92400E��  �� *Status+ Out of Service.a���  � ��$	 ⚠����  � �� �#475569�� 8�� ���� *Status+� $��2���� $��3��$��10(�$	��8,4q�$��Stretch=����� * DataContext.SelectTableCommand+ �� 
 Window���� ���� " ��8   ��  �� *Status+ Occupied.a���  � ��$ Continue����  � �� �#EF4444��  �� *Status+ 	Available.a���  � ��$ Select����  � �� �#10B981��  �� *Status+ Reserved.a���  � ��$	 Use����  � �� �#F59E0B��  �� *Status+ Out of Service.a���  � ��$	 N/A����  � �� �#94A3B8��  � {�$ False��.���  �� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0�����#F8FAFC���#E2E8F0$��0,0,0,1q� �� Padding$ 24,16q�._��   � ColumnDefinitions  �� $��* ��� $
��Auto �.+��� $��0��.+��� $��Select Restaurant Table��#��[ �� $l��fChoose a table for this transaction. Status is automatic: Available (no items) / Occupied (has items).��#��[ ���#64748B�� $��1��$ 
Horizontal=�.+��� 
����������#E2E8F0$��1q�$
 6��$
 12,8q�$��0,0,8,0q�._��� $ 
Horizontal=�.+��� $��Available: ��$��SemiBold"����#10B981�� $
��Bold"����#10B981���� *AvailableTableCount+�� $�� | Occupied: ��$��SemiBold"����#EF4444$��8,0,0,0q��� $
��Bold"����#EF4444���� *OccupiedTableCount+�� $��🔄 Refresh��#��[ $��12(����� *RefreshTablesCommand+�� $��1��
����������#E2E8F0$��0,0,0,1q�$ 24,16q�._��   �� $��2* ��� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0,0,16,0q�.+��� $��Search Tables��#��[ �� -   ! ��Name$! SearchTextBox������ *SearchQuery+" �� UpdateSourceTrigger$" PropertyChanged=�.��   	HintBrush# j� TileMode$ �� Opacity% j� Stretch& j� 
AlignmentX' !� Visual��%�   &	       !� $
# None=�$	$ 0.5Q�$
% None=�$
& Left=�' �� $.��(Search by table number or description...��$��Italic$�$��12,0,0,0q�.������ " ��# [  P�  � �� Z� . ���  � ��# [ P�  � ��$  ��. ���  � ��# [ .���� $��1��$��0,0,16,0q�.+��� $��Filter by Status��#��[ �� $��36��$��Center=�$
��12,0q�$��1q����#CBD5E1$��14(����� *StatusFilterOptions+( �� SelectedItem( �� *SelectedStatusFilter+�� $��2��$��Bottom=�$��0,0,16,0q�.+��� $��Show Only Available��$��SemiBold"����#1E293B) d� 	IsChecked) �� *ShowOnlyAvailable+�� $��3��$��Bottom=�.+��� $��Clear Filters��#��[ $��12(����� *ClearFiltersCommand+�� $��2��$
��Auto=�$��Disabled=�$��24q����#F8FAFC.��� .+�� y��� *	IsLoading+* �� 	Converter#* [  .+��� $��Center=�$��Center=�.+�� + � IsIndeterminate$
+ True��$	��200��$��4�����#2563EB$��0,0,0,16q��� $"��Loading restaurant tables...��$��Center=�$��16(����#64748B� y��� *	IsLoading+#* [ .+��� $��Center=�$��Center=�y��� *FilteredTableCount+#* [ , �� ConverterParameter$, 0��.+��� $
��📍��$��48(�$��Center=�$��0,0,0,16q�- J� Opacity$	- 0.5Q��� $��No tables found��$��18(�$��SemiBold"�$��Center=����#64748B$��0,0,0,8q��� $B��<Try adjusting your search criteria or refresh the table list��$��14(�$��Center=����#94A3B8�� #��[ ���� *FilteredTables+���� .R�A� . A� Columns$. 4��$
��Left=��� $��3��
����������#E2E8F0$��0,1,0,0q�$ 24,16q�._��   �� $��* ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��Center=�.+��� ���#64748B$��Center=����� *StatusMessage+�� $	�� | �����#CBD5E1$	��8,0q�$��Center=��� $��Legend: �����#64748B$��Center=��� ���#DCFCE7$
 3��$	 4,2q�$	��4,0q�._��� $��	Available��$��10(����#166534�� ���#FEE2E2$
 3��$	 4,2q�$	��4,0q�._��� $!��Occupied (Auto - Has Items)��$��10(����#991B1B�� ���#FEF3C7$
 3��$	 4,2q�$	��4,0q�._��� $��Reserved��$��10(����#92400E�� $��1��$ 
Horizontal=�.+��� $��Cancel��#��[ $��0,0,8,0q�$	��100������ *CancelCommand+�� $��
Statistics��#��[ $	��100������ *ShowStatisticsCommand+