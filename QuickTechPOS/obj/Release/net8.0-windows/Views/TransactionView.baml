   M S B A M L   `   `   `  ���� G  CQuickTechPOS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:QuickTechPOSQuickTechPOS  A%clr-namespace:QuickTechPOS.ConvertersQuickTechPOS.Converters  ;"clr-namespace:QuickTechPOS.HelpersQuickTechPOS.Helpers  (    "QuickTechPOS.Views.TransactionView   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     ,localclr-namespace:QuickTechPOS     <
converters%clr-namespace:QuickTechPOS.Converters     6helpers"clr-namespace:QuickTechPOS.Helpers     ���#F8FAFC  � UseLayoutRounding$
  True�� � 	Resources *   $QuickTechPOS.BooleanToIndexConverter   BooleanToIndexConverter2   ,QuickTechPOS.BooleanToWholesaleTextConverter # BooleanToWholesaleTextConverter+   %QuickTechPOS.BooleanToStringConverter  BooleanToStringConverter:   4QuickTechPOS.Converters.BooleanToVisibilityConverter   BooleanToVisibilityConverter6   0QuickTechPOS.InverseBooleanToVisibilityConverter ' #InverseBooleanToVisibilityConverter3   -QuickTechPOS.PositiveIntToVisibilityConverter $  PositiveIntToVisibilityConverter.   (QuickTechPOS.StringToVisibilityConverter  StringToVisibilityConverter1   +QuickTechPOS.ImagePathToVisibilityConverter " ImagePathToVisibilityConverter/	   )QuickTechPOS.NumericToVisibilityConverter   NumericToVisibilityConverter2
   ,QuickTechPOS.Converters.FirstLetterConverter 	 FirstLetterConverter-   'QuickTechPOS.Helpers.ImagePathConverter 
 ImagePathConverter3  -System.Windows.Media.Effects.DropShadowEffect  DropShadowEffect   ShadowDepth   	Direction   Color   Opacity   
BlurRadius  
HeaderText �� 
TargetType �� Property	 �� Value  SectionTitle  	TouchText  TouchSearchBox
 �� 
TargetType �� CornerRadius ��Name  TouchCategoryFilter  TouchButton �� Triggers P� Property P� Value  TouchSecondaryButton �� BasedOn  TouchSuccessButton  TouchDangerButton  TouchWarningButton  TouchIconButton  TouchCardPanel  TouchCardHeader  TouchProductCard  TouchCategoryCard �� ScaleX �� ScaleY  TouchCartItemTemplate �� Padding � ColumnDefinitions �� StringFormat �� Binding �� ElementName �� Path �� Value ��Name �� TextAlignment �� 	MaxLength � ToolTip �� UpdateSourceTrigger �� Mode  �� RelativeSource! � Cursor" �� AncestorType  TouchCategoryCardTemplate# J� InputBindings$ d� MouseAction% �� Command& �� CommandParameter' � RowDefinitions( �� TextAlignment) �� 
LineHeight  TouchProductCardTemplate��%  &	        &	      &	 
     &	      &	      &	      &	      &	 #     &	 (     &		 -     &	
 2     &	 7     &	 o     &	 �     &	 Q    &	 �    &	 �    &	 w    &	 M    7  &	 �    7  &	 �    7  &	 Q    7  &	 �    &	 �	    7  &	 A
    &	 �
    &	     &	 
    &	 :    7  &	 r    7                  	  
      $ 2Q�$	 270Q�$ #1E293B��$
 0.08Q�$ 8Q��� " ��.���  � ��$	 SemiBold"���  � ��$	 16(���  � ��	 �#1E293B��  � ��$	 0,0,0,6q��� " ��.���  � ��$
	 Bold"���  � ��$	 18(���  � ��	 �#334155��  � ��$	 0q��� " ��.���  � ��$	 14(���  � ��	 �#64748B��  � ��$	 0q��� " ��.���  � ��$	 45����  � ��$	 Center=���  � ��$
	 12,0q���  � ��$	 2q���  � ��	 �#CBD5E1��  � ��$	 16(���  � ��	 �� "
 ��.R��� #��z��#��z��#��z��$ 6��._��� $ PART_ContentHost��#��z���� " ��.���  � ��$	 48����  � ��$	 Center=���  � ��$
	 12,0q���  � ��$	 2q���  � ��	 �#CBD5E1��  � ��
	 �������  � ��$	 16(���  � ��$	 SemiBold"��� " ��.���  � ��	 �#2563EB��  � ��
	 �������  � ��$	 0q���  � ��$	 16,12q���  � ��$	 45����  � ��$	 SemiBold"���  � ��$	 16(���  � ��	 �� "
 ��.R��� #��z��#��z��#��z��$ 6��._��� $��Center=�$��Center=�#��z�� P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#1D4ED8P�  � ��$
 True��. ���  � ��	 �#1E40AFP�  � {�$ False��. ���  � ��	 �#94A3B8�� " ��8   .���  � ��	 �#64748B P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#475569P�  � ��$
 True��. ���  � ��	 �#334155�� " ��8   .���  � ��	 �#10B981 P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#059669P�  � ��$
 True��. ���  � ��	 �#047857�� " ��8   .���  � ��	 �#EF4444 P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#DC2626P�  � ��$
 True��. ���  � ��	 �#B91C1C�� " ��8   .���  � ��	 �#F59E0B P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#D97706P�  � ��$
 True��. ���  � ��	 �#B45309�� " ��.���  � ��$	 45����  � ��$	 45����  � ��
	 ���� ��  � ��$	 2q���  � ��	 �#CBD5E1��  � ��$	 18(���  � ��	 �� "
 ��.R��� #��z��#��z��#��z��$ 6��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#F1F5F9P�  � ��$
 True��. ���  � ��	 �#E2E8F0P�  � {�$ False��. ���  �@J�Opacity$		 0.5Q��� " ��.���  � ��
	 �������  � ��	 �#E2E8F0��  � ��$	 1q���  �@��CornerRadius$	 8����  �@J�Effect8	   �� " ��.���  � ��	 �#F8FAFC��  � ��	 �#E2E8F0��  � ��$	 0,0,0,1q���  �@��Padding$	 20,12q���  �@��CornerRadius$	 8,8,0,0���� " ��.���  � ��
	 �������  � ��	 �#E2E8F0��  � ��$	 2q���  �@��CornerRadius$	 8����  � ��$	 0,0,6,6q���  �@�Cursor$
	 Hand����  � ��$		 115����  � ��$		 145����  �@J�Effect	   $ 1Q�$	 270Q�$ #1E293B��$
 0.12Q�$ 6Q� P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#2563EB��  � ��$	 3q���  �@J�Effect	   $ 2Q�$	 270Q�$ #2563EB��$
 0.25Q�$ 8Q�P�  � {�$ False��. ���  �@J�Opacity$		 0.6Q��� " ��.���  � ��
	 �������  � ��	 �#E2E8F0��  � ��$	 2q���  �@��CornerRadius$	 8����  � ��$	 0,0,8,8q���  �@�Cursor$
	 Hand����  � ��$		 100����  � ��$		 140����  �@J�Effect	   $ 2Q�$	 270Q�$ #1E293B��$
 0.08Q�$ 8Q� P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#3B82F6��  � ��$	 3q���  �@J�Effect	   $ 4Q�$	 270Q�$ #3B82F6��$
 0.25Q�$ 12Q���  � z�	 �� $
 1.02Q�$
 1.02Q��� .R��� ���#F1F5F9$��0,0,0,1q�$ 12,10q�._��  �� $��* ��� $��55 ��� $��70 ��� $��30 �.+��� $��0��.+��� $��SemiBold"�$��13(����#1E293B$��CharacterEllipsis=�$	��150������ *Product.Name+�� $��11(����#64748B���� *	UnitPrice+$ ${0:F2}���� $��1��
����������#CBD5E1$��2q�$ 6��$��32������ " ��.���  � ��	 �#CBD5E1 ��  �� $ QuantityTextBox��$ 	IsFocused� True.a���  � ��	 �#2563EB��  � ��$	 3q�._��� $ QuantityTextBox��-   $��0q�
������ $��Center=�$��Center=�$��13(�$��SemiBold"�$ Center=�$ 8��$4 .Enter quantity: 1, 1.5, 1,4 (decimals allowed)������ *
Quantity+$ 	LostFocus=�$ TwoWay=�.������ " ��.���  � ��	 �#1E293B P�  �@J�	IsFocused$
 True��. ���  � ��	 �#F0F9FF��  ��   �� *Self+$	 Tag� Invalid.a���  � ��	 �#DC2626��  � ��	 �#FEF2F2��  ��   �� *Self+$	 Tag�	 Valid.a���  � ��	 �#059669.���� $��2��$
��Bold"�$��13(����#1E293B$��Center=�$��Right=����� *Total+$ ${0:F2}���� $��3��$��×��$��24��$��24�����#EF4444
�������$
��Bold"�$��14(�$��0q�$
! Hand������ *#!DataContext.RemoveFromCartCommand+  �� " UserControl���� ���� "
 ��.R��� #��z��$ 5��._��� $��Center=�$��Center=����� " �� P�  �@J�IsMouseOver$
 True��. ���  � ��	 �#DC2626P�  � ��$
 True��. ���  � ��	 �#B91C1C�� .R��� 8��  # d� $$ 	LeftClicke�% �� *'%DataContext.SelectCategoryCardCommand+  �� " UserControl& �� ._�� $��12q�' �� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��$��Center=�$��Center=�.+��� $
��Bold"�$��16(����#1E293B$( Center=�$
��Wrap=�$) 20��$	��120������ *Name+�� $��1��$��Center=�$��0,0,0,4q�.+��� $��SemiBold"�$��12(����#64748B$( Center=����� *ProductCount+$ {0} Products���� $��2��$��Tap to Browse��$��10(�$��Italic$����#94A3B8$��Center=��� .R��� 8��  # d� $$ 	LeftClicke�% �� *DataContext.AddToCartCommand+  �� " UserControl& �� ._�� $��8q�' �� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��$��Center=�$��Center=�$��0,4,0,8q�.+��� $��SemiBold"�$��12(����#1E293B$( Center=�$
��Wrap=�$��CharacterEllipsis=�$��48������ *Name+�� $��1��$��Center=�$��0,2,0,6q�.+��� $
��Bold"�$��16(����#2563EB$( Center=����� *FormattedSalePrice+�� $��2��$��
Tap to Add��$��10(�$��Italic$����#64748B$��Center=�$��0,0,0,1q�.��� ' �� $��* ��� $
��Auto ��� $
��Auto �.+�� $��0��$��16q� �� $��* ��� $	��300 �.+��� $��0��#��[ $��0,0,12,0q�._�� ' �� $
��Auto ��� $��* �.+��� $��0��#��[ ._��  �� $
��Auto ��� $��* �.+��� $��0��$��← Categories��#��[ $��14(�$
��12,8q�$��0,0,12,0q����� *ShowCategoriesCommand+y��� *IsShowingCategories+* �� 	Converter#* [ + �� ConverterParameter$+ Invert���� $��1��#��[ ���� *CurrentPageTitle+�� $��1��$
��Auto=�$��Disabled=�$��8q�.��� .+��� #��[ ���� *
Categories+y��� *IsShowingCategories+#* [ ���� .R�� , � Orientation$, 
Horizontal=�$
��Left=��� #��[ ���� *SearchedProducts+y��� *IsShowingCategories+#* [ $+ Invert������ .R�� $, 
Horizontal=�$
��Left=��� $��1��#��[ ._�� ' �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��#��[ ._��  �� $��* ��� $
��Auto �.+��� $��0��$
��Cart��#��[ �� $��1��$��Clear��#��[ $��11(�$
��10,5q����� *ClearCartCommand+�� $��1��$
��Auto=�$��Disabled=�.���� #��[ ���� *	CartItems+�� $��2�����#F8FAFC���#E2E8F0$��0,1,0,0q�$ 16,12q�._��� .+�� $��0,0,0,8q� �� $��* ��� $
��Auto �.+��� $��0��$��Total��$��18(�$
��Bold"����#1E293B�� $��1��$��22(�$
��Bold"����#1E293B���� *TotalAmount+$ ${0:F2}���  �� $��* ��� $
��Auto �.+��� $��0��$	��LBP��$��14(�$��SemiBold"����#2563EB�� $��1��$��14(�$��SemiBold"����#2563EB���� *ExchangeAmount+$ N0���� $��1��
����������#E2E8F0$��0,1,0,0q�$ 16,12q�- J� Effect#- [ ._��  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��. �� Orientation$. 
Horizontal=�$��0,0,20,0q�.+��� $
��💰��$��16(�$��0,0,6,0q��� $��Drawer:��$��SemiBold"�$��14(�$��0,0,4,0q��� $��14(�$
��Bold"����� *IsDrawerOpen+#* [ $+ OPEN|CLOSED������ " �� ��  �� *IsDrawerOpen+ True.a���  � ��	 �#10B981��  �� *IsDrawerOpen+	 False.a���  � ��	 �#EF4444�� $��1��$. 
Horizontal=�$��0,0,20,0q�y��� *SelectedTable+#* [ .+��� $
��📍��$��16(�$��0,0,6,0q��� $��Table:��$��SemiBold"�$��14(�$��0,0,4,0q��� $��14(�$
��Bold"����#2563EB���� " ��.���  � ��	 �� *SelectedTable.DisplayName+ ��  �� *SelectedTable.DisplayName+  .a���  � ��	 �� *SelectedTable.TableNumber+$ 	Table {0}����  �� *SelectedTable.TableNumber+ 0.a���  � ��	 �� *SelectedTable.Id+$ Table ID: {0}���� $��12(�$��SemiBold"�$��2,0,0,0q����� *SelectedTable.Status+$  ({0})������ " �� ��  �� *SelectedTable.Status+ 	Available.a���  � ��	 �#10B981��  �� *SelectedTable.Status+ Occupied.a���  � ��	 �#EF4444��  �� *SelectedTable.Status+ Reserved.a���  � ��	 �#F59E0B�� $��2��#��[ $��50��$��12(�$
��10,6q�$��0,0,12,0q����� *SelectTableCommand+���� $. 
Horizontal=�.+��� $��Center=����� " ��.���  � ��$	 📍 Select Table�� ��  �� *SelectedTable+#* [  Visible.a���  � ��$	 📍 Change Table���� $��4��$��🖨️ Print��#��[ $��50��$	��100��$��15(�$
��Bold"�$��0,0,10,0q����� *PrintReceiptCommand+�� $��5��$��Complete Sale��#��[ $��50��$	��160��$��16(�$
��Bold"����� *CheckoutCommand+{��� *IsDrawerOpen+�� $��2�����#F1F5F9���#E2E8F0$��0,1,0,0q�$
 16,6q�._��  �� $��* ��� $
��Auto �.+��� $��0�����#1E293B$��14(�$��Center=����� *StatusMessage+� $��1��/ � IsIndeterminate$
/ True��$	��120��$��4�����#2563EBy��� *IsProcessing+#* [ 