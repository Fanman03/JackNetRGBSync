   M S B A M L   `   `   `  ���� L  HDeviceExcludeTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:DeviceExcludeToolDeviceExcludeTool  "    DeviceExcludeTool.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     1localclr-namespace:DeviceExcludeTool     V RMaterialDesignThemes.Wpf, Version=3.1.3.1513, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  -     � Title$  Device Exclude Tool��$	��410��$	��280��   MaterialDesignBody#���   $��Regular"�$��13(�&   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=�   TextRenderingMode$
 Auto=�  MaterialDesignPaper#���    MaterialDesignFont#���   � 
ResizeMode$ CanMinimize=�.���  � ColumnDefinitions �� $	��130 ��� $	��130 � � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��4,0,0,0q�$
��Left=�$��32��$��code.png�� J� RenderTransformOrigin$ 0.449,0.543&��� $��0��$��0��$��2��$��40,4,4,4q�$��22(�Device Exclude Tool�� $��1��$��0��$��2��$��Center=�$��4q� �� TextAlignment$ Center=�*(Check the boxes of only the device types�� )'that you want JackNet RGB Sync to load.�� -    ��Name$ keyboard��$��2��$��0��$��4q�.��
Keyboard�� -   $ mouse��$��3��$��0��$��4q�.��Mouse�� -   $ headset��$��4��$��0��$��4q�.��	Headset�� -   $ mousepad��$��5��$��0��$��4q�.��
Mousepad�� -   $ headsetstand��$��6��$��0��$��4q�.��Headset Stand�� -   $ keypad��$��7��$��0��$��4q�.��Keypad�� -   $ speaker��$��8��$��0��$��4q�.��	Speaker�� -	   $ unknown��$��9��$��0��$��4q�.��	Unknown�� -
   $ 	mainboard��$��2��$��1��$��4q�.��	Mainboard�� -   $	 gpu��$��3��$��2��$��4q�.��Graphics Card�� -   $
 dram��$��4��$��3��$��4q�.��DRAM�� -   $	 fan��$��5��$��4��$��4q�.��Fan�� -   $ cooler��$��6��$��5��$��4q�.��Cooler�� -   $ ledstrip��$��7��$��6��$��4q�.��	LED Strip�� -   $ 	ledmatrix��$��8��$��7��$��4q�.��
LED Matrix�� ���#2196F3$��9��$��8��$��4q��� -   $	 all��  MaterialDesignLightCheckBox#��[ .��All�� -   	 ��Name$	 	resultBox��$��4q�$��10��$��0��$��2���� -   
 ��Name$
 WriteBtn��$��0��$��11��$��2��$��10q�.��Write Value