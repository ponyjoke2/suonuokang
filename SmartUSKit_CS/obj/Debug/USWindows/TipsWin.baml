   M S B A M L   `   `   `  ����J  FSmartUSKit_View, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:SmartUSKit_CS.USWindowsSmartUSKit_CS.USWindows  5   	   %    SmartUSKit_CS.USWindows.TipsWin   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   7local%clr-namespace:SmartUSKit_CS.USWindows     5   	     � 
ResizeMode$  NoResize=�5       � WindowStartupLocation$ CenterScreen=�60    � Topmost .  � Title$
 Tips��5   	   $	��200��6   $	��300��6#    � 	Resources 5	         ExaminationBorderBrush �� Color  ButtonBorder �� EndPoint �� 
StartPoint � Color	 � Offset
 �� 
TargetType  MyButton �� Property �� Value �� 
TargetType �� CornerRadius �� Triggers P� Property P� Value �� 
TargetName  ExaminationFontBrush��%�  &	        &	      &	 �     7  &	 b    �� 5
   
   $
 Gray��69   �� 5   
   $ 0,0.5&�63   $ 1,0.5&�6D   .��5      � $ #278af7��6   $	 0Q�6+   � 5      $ #5dd3aa��6   $	 1Q�6+   5      �� 5   
   "
 ��6   .�5      ��  � ��$ 15(�6)   �� 5       � ��$ 4q�6'   �� 5       � ��
 �����6+   �� 5       � ��$ 0q�60   �� 5       � ��
 �  ��6+   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� $��border��6^   ���	#FF45C5D7$��0q�69   $ 5��6M   ���	#7282B7EA._�5      �� #��z��6/   $��Center=�6b   $��Center=�6   5       5      P� 5       �@��IsDefaulted . . �5   "   �� $ border��6@   # � ��6T    � ���� 5    "   $ border��6?    � ��
 �  ��6S   5!      P� 5"       �@J�IsMouseOver . . �5#   "   �� $ border��6D    � ��$	 2.5q�6X   5$      P� 5%       � �� . . �5&   "   �� $ border��6?    � �� �	#FF49CECA6S   5'      P� 5(       � {� .  . �5)   "   �� $ border��6?   8    � ��6S   5,      5.      50      51      �� 52   
   $ Black��67   53      .��54      �  � RowDefinitions 55   
   �� 56      $��2* �6   �� 57      $��1* �6   .+�59   
   �� -   5         ��Name$ 	TipsLabel��59      $�� ��6#   $��20(�6/   $
��auto��6=   $��Center=�6K   $��Center=�6h   �� 5;   
   -   5         ��Name$ 
SaveButton��5;      $��保存��6%   $	��100��62   $��30��6>   $��1��6J   #��[ 6W   |�.  $��20(�6�   $��0��6�   
���   �6�   5<      5=      