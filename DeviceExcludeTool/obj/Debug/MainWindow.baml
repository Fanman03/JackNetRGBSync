   M S B A M L   `   `   `  ����L  HDeviceExcludeTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:DeviceExcludeToolDeviceExcludeTool  5   	   "    DeviceExcludeTool.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:DeviceExcludeTool     5   	   V RMaterialDesignThemes.Wpf, Version=3.1.3.1513, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5	   
   -   5          � Title$  Device Exclude Tool��5   	   $	��415��6%   $	��280��62      MaterialDesignBody#���   5
   	   $��Regular"�5   	   $��13(�5   	   &   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=�5   	      TextRenderingMode$
 Auto=�5   	     MaterialDesignPaper#���  5   	     MaterialDesignFont#���  5   	    � 
ResizeMode$ CanMinimize=�6;   .��5      �  � ColumnDefinitions 5   
   �� 5      $	��130 �6   6-   �� 5      $	��130 �6   6-    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5       $
��Auto �6   6,   �� 5!      $
��Auto �6   6,   �� 5"      $
��Auto �6   6,   �� 5#      $
��Auto �6   6,   �� 5$      $
��Auto �6   6,   .+�5&   
   �� $��0��6   $��0��6   $��4,0,0,0q�6-   $
��Left=�6>   $��32��6Y   $��code.png��6e    J� RenderTransformOrigin$ 0.449,0.543&�6w   6�   �� 5'   
   $��0��6   $��0��6!   $��2��61   $��40,4,4,4q�6E   $��22(�6W   Device Exclude Tool6z   �� 5(   
   $��1��6   $��0��6!   $��2��61   $��Center=�6E   $��4q�6b    �� TextAlignment$ Center=�6m   *(Check the boxes of only the device types�� 6�   6�   )'that you want JackNet RGB Sync to load.6�   �� 5)   
   -   5         ��Name$ keyboard��5)   0   $��2��6   $��0��6    $��4q�6B   .��6�   
Keyboard6�   �� 5*   
   -   5        $ mouse��5*   0   $��3��6   $��0��6    $��4q�6?   .��6�   Mouse6�   �� 5+   
   -   5        $ headset��5+   0   $��4��6   $��0��6    $��4q�6A   .��6�   	Headset6�   �� 5,   
   -   5        $ mousepad��5,   0   $��5��6   $��0��6    $��4q�6B   .��6�   
Mousepad6�   �� 5-   
   -   5        $ headsetstand��5-   0   $��6��6   $��0��6    $��4q�6F   .��6�   Headset Stand6�   �� 5.   
   -   5        $ keypad��5.   0   $��7��6   $��0��6    $��4q�6@   .��6�   Keypad6�   �� 5/   
   -   5        $ speaker��5/   0   $��8��6   $��0��6    $��4q�6A   .��6�   	Speaker6�   �� 50   
   -	   5        $ unknown��50   0   $��9��6   $��0��6    $��4q�6A   .��6�   	Unknown6�   �� 52   
   -
   5        $ 	mainboard��52   0   $��2��6   $��1��6    $��4q�6C   .��6�   	Mainboard6�   �� 53   
   -   5        $	 gpu��53   0   $��3��6   $��2��6    $��4q�6=   .��6~   Graphics Card6�   �� 54   
   -   5        $
 dram��54   0   $��4��6   $��3��6    $��4q�6>   .��6   DRAM6�   �� 55   
   -   5        $	 fan��55   0   $��5��6   $��4��6    $��4q�6=   .��6~   Fan6�   �� 56   
   -   5        $ cooler��56   0   $��6��6   $��5��6    $��4q�6@   .��6�   Cooler6�   �� 57   
   -   5        $ ledstrip��57   0   $��7��6   $��6��6    $��4q�6B   .��6�   	LED Strip6�   �� 58   
   -   5        $ 	ledmatrix��58   0   $��8��6   $��7��6    $��4q�6C   .��6�   
LED Matrix6�   �� 59   
   ���#2196F3$��9��6)   $��8��66   $��4q�6F   �� 5:      -   5        $	 all��5:   L     MaterialDesignLightCheckBox#��[ 6   .��6�   All6�   5;      �� 5<   
   -   5        	 ��Name$	 	resultBox��5<   O   $��4q�6   $��10��6   $��0��6+   $��2��6;   6d   �� 5=   
   -   5        
 ��Name$
 WriteBtn��5=   C   $��0��6   $��11��6!   $��2��6/   $��10q�6l   .��6x   Write Value6�   5>      5?      