   M S B A M L   `   `   `  ����J  FSmartUSKit_View, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:SmartUSKit_CSSmartUSKit_CS  5   B   A%clr-namespace:SmartUSKit_CS.USViewersSmartUSKit_CS.USViewers  5          SmartUSKit_CS.USBMViewer   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    6(   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     6j   @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:SmartUSKit_CS     6B   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6l   ;	usviewers%clr-namespace:SmartUSKit_CS.USViewers     5      &    SmartUSKit_CS.USViewers.USViewer    Loaded$  USViewer_Loaded��5         SizeChanged$ USViewer_SizeChanged��62    N� Title$ USGeneralView��5   =   .��5      �  � ColumnDefinitions 5   
   �� 5	      $��1* �6   �� 5
      $��7* �6   .+�5   
   � 
���   �.+�5      �� -   5         ��Name$ InfoStackpanel��5      $��0��6)   $��0��66      ImageThickness#���   5      .+�5      �� 
�������&   System.Windows.Media.TextOptions   TextHintingMode$ Animated=�6+   .��5      �� -   5         ��Name$ 	timeLabel��5       $
��Wrap=�63   2018-02-27 15:235      5      �� 5      -   5         ��Name$ UserIdLabel��5      $
��Left=�6*   $��ID:--��6E   
�������5      �� 5      -   5        $ UserNameLabel��5      $
��Left=�6,   $��Name:--��6G   
�������5      �� 5      -   5        $ GenderLabel��5      $
��Left=�6*   $��	Gender:M ��6E   
�������5      �� 5       -   5        $ AgeLabel��5!      $
��Left=�6'   $��Age:--��6B   
�������5"      � 5#       � Orientation$ 
Horizontal=�6   $y�	Collapsed=�65   .+�5$      �� 
�������.��6/   MI:64   �� 5%      -   5        $ MILabel��5%      $��0,0,15,0q�6-   
�������6?   �� 5&      	 �� Orientation$	 
Horizontal=�6!   .+�5'      �� 
�������.��63   TIS:69   �� 5(      -   5        $ TISLabel��5(       
�������62   5)      5*      �� 5+      -	   5        $ 	gainLabel��5+      
�������.��6>   GN:--6E   �� 5,      -
   5        $ 
depthLabel��5,      
�������.��6?   D:--6E   �� 5-      -   5        $ ENHLabel��5-      
�������$y�Hidden=�6=   .��6Q   ENH:--5/      �� 50      -   5        $ DRLabel��50      
�������$y�Hidden=�6<   .��6P   DR:--52      �� 53      -   5        $ FLabel��53      
�������$y�Hidden=�6;   .��6O   F:--55      58      �� 59      -   5        $ PatientStackpanel��5:      $��0��6,   $��0��69   $��Right=�5;      $	 Vertical=�6-   5>      5?      � 5@   
   $��1��6   
 � RowDefinitions
 5A      �� 5B      $��3* �6    �� 5C      $��2* �6    .+�5E      � -   5         �Name$ imageScreen��5E      $��0��6(   
���   � 5F      �� 5G      �� 5H      $��8 �6'   .+�5J      �� -   5         ��Name$ 	imageView��5K      $��Center=�6(   $��Center=�6E   $��/Resources/none.png��5L      $
��Fill=�62   �� 5M      -   5         ��Name$ GreenPointCanvas��5M      .+�5N      A� -   5         A�Name$ 
GreenPoint��5O      $��12��6-   $��12��68   
��� � �5P      5Q      �� 5R      -   5        $ 	GrayImage��5R      $��1��6+   $
��Fill=�6;   �� 5S      -   5        $ ImageCountLabel��5T      $��Right=�6.   $��Bottom=�6J   
������ $	��0/0��5U   .   $��16(�6<   
�������5V      �� 5X      -   5        $ CanvasRuler��5X      �� 5Z      -   5        $ CanvasImage��5[      $��Center=�6*   $��Center=�6G   
������ 5\      �� 5]      -   5        $ 
StateLabel��5^      $��24,0,0,0q�6)   $��Bottom=�6;   $
��Live��5_      $��16(�6$   
�������62   5`      � 5a      $��1��6   
���   �.+�5b      �� -   5        $ 	ImageLine��5b      $
��Fill=�6+   �� 5c      -   5        $ CanvasTimescale��5c      �� 5d      -   5        $ CanvasLineImage��5e      
������ $y�Hidden=�5f   7   �� 5g      -   5        $ LabelHeartbeat��5h      $
��Left=�6-   $	��Top=�6H   $��心跳:��5i      $��14(�6#   
��� � �$y�Hidden=�5j      5k      5m      5n      5q      