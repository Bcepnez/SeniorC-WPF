   M S B A M L   `   `   `  ����A  =LiMESH, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:WpfApp1WpfApp1  5   	       LiMESH.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:WpfApp1     5   	   -   5          �Name$
  icon��5   	    � Title$ LiMESH��5   	   $��647.777��6   $��976.8��6)    � Icon$ logoMax.ico��5	   	    � WindowStartupLocation$ CenterScreen=�5
   	    � 
ResizeMode$ NoResize=�5   	   
�������.��5      ]� .+�5   
   o� $	��Top=�6   .@�5      m� $��_File Manager��6   .@�5      m� -   5        $��_Import Data��5       m� Icon 5      �� 5      $��
import.ico��6    5      m� 5      -   5        $��_Export Data��5       5      �� 5      $��
export.ico��6    5      �� 5      m� 5       -   5        $��_Exit Application��5        5!      �� 5"      $��	error.ico��6    5$      5%      m� 5&      -   5        $��_Server File Manager��5&      65   5'      �� 5(   
   -   5        $	��tab��5(      $��0,0.4,0,0.2q�6O   $
��auto��6d   .@�5)      �� $��Mesh renderer��6   $��-2,0,-1.8,-2q�6-   .��5*      ]� $��0,0,-0.4,-1.2q�6   .+�5+      � ���#123$
��Left=�6-   $��
0,0,0,-0.2q�6C   $	��159��6W   .+�5,      �� -   5        $��ShowareaList��5,   "   $
��Left=�66   $	��145��6Q   $��8,10,0,0q�6^   $	��Top=�6p   $	��141��6�   �� 5-      -   5        $
��type��5-   #   $��20��6/   $��65,167,10,0q�6;   $	��Top=�6P   .@�5.      �� .��6,   Mesh62   �� 5/      .��6,   Point cloud69   50      �� 51      -	   5        $��Light��51   #   $��65,198,10,0q�60   $��20��6E   $	��Top=�6Q   .@�52      �� .��6+   Ambient Light6:   �� 53      .��6+   Direction Light6<   54      �� 55      -
   5        $��cbColors��55   /   $��20��6#   $	��Top=�6?   $��65,228,10,0q�6�   �� 56      $��Show as:��6    $��	0,162,0,0q�63   $��36��6F   $	��Top=�6R   $
��Left=�6j   $��65��6�   
�������6�   �� 57      $��	Light as:��6    $��	0,192,0,0q�64   $��36��6G   $	��Top=�6S   $
��Left=�6k   $��65��6�   
�������6�   �� 58      $��Color :��6    $��	0,225,0,0q�62   $��36��6E   $	��Top=�6Q   $
��Left=�6i   $��65��6�   
�������6�   �� 59      -   5        $��fileList��59   "   $
��Left=�62   $	��188��6M   $��	7,281,0,0q�6Z   $	��Top=�6m   $	��144��6�   6�   �� 5;      -   5        $��	zoomValue��5;   �   $
��Left=�6"   $��19��6=   $��
72,478,0,0q�6I    �� TextWrapping$
 Wrap=�6]   $��0��6q   $	��Top=�6z   $��55��6�   6�   �� 5<      $��Zoom : ��6    
�������$
��Left=�6E   $��30��6`   $��
10,474,0,0q�6l   $	��Top=�6�   $��62��6�   �� 5>      -   5         ��Name$ rotateX��5>   �   $
��Left=�6"   $��19��6=   $��
72,508,0,0q�6I   $
 Wrap=�6]   $��0��6q   $	��Top=�6z   $��55��6�   6�   �� 5?      $��Rotate X : ��6    
�������$
��Left=�6I   $��24��6d   $��	7,503,0,0q�6p   $	��Top=�6�   $��65��6�   �� 5@      -   5        $ rotateZ��5@   �   $
��Left=�6"   $��20��6=   $��
72,535,0,0q�6I   $
 Wrap=�6]   $��0��6q   $	��Top=�6z   $��55��6�   6�   �� 5A      $��Rotate Z : ��6    
�������$
��Left=�6I   $��34��6d   $��	9,531,0,0q�6p   $	��Top=�6�   $��63��6�   �� 5B      $��List of file on server :��6    $��	0,252,0,0q�6C   $��29��6V   $	��Top=�6b   $
��Left=�6z   $	��131��6�   
�������6�   5G      � 5J       � RowDefinitions 5K      �� 5L      .+�5N      �� -   5        $��bgIMG��5N   6   $��HartLogo.png��6    $��0,0,5.6,78.4q�6C   {�. 	 J� Opacity$		 0.3Q�6j   '� 5O      -   5        $��viewport3D1��5O   %   $��0,0,5.6,-0.6q�68   
 '� Camera
 5P      =� 5Q   "   -   5        camMain�� � Position$ 10 5 41�5R   4    � LookDirection$ 	-10 -5 -45�5S   4   5T   #   .x�5W      g� -   5        MyModel�� g� Content 5X   "   � 5Y   &    � Material 5Z   *   u� 5[   .   -   5        matDiffuseMain�� u� Brush 5\   2   �� 5]   6    �� Color$ Violet��6F   5_   /   5a   '    g� 	Transform 5d   "   Y� 5e   &   �5g   .   �� -   5        myXTransform�� �� Rotation 5h   2   �� 5i   6   -   5        
rotate�� �� Axis$ 0 1 05�5i   \   5k   /   �� 5l   .   -   5        myYTransform�� 5m   2   �� 5n   6   -   5        rotate_1��$ 1 0 05�5n   ^   5p   /   �� 5q   .   -   5        
scaler�� �� CenterX$ 0Q�5q   O    �� CenterY$ 0Q�6[    �� CenterZ$ 0Q�6g   5s   '   5v      5w      �� 5{      -   5         ��Name$ slider1_Copy��5{   "    � Minimum$
 -360Q�5|   "    � Value$ 0Q�5}   !    � Maximum$	 360Q�5~       J� RenderTransformOrigin$ 0.5,0.5&�5      $��545,0,-209.4,319.4q�5�   "   $��32��5�   "   $��Bottom=�5�   "   $
��Left=�5�   "   $	��470��6=   ��5   b   Z� z�5�      V� 5�   "   .~�5�   &   �� �� 5�   &   �� 5�   &    �� Angle$	 270Q�66   T� 5�   &   5�   #   5�      �� 5�      $��Rotate Z��6    $��24��63   $��747,485,4.6,0q�6?   $	��Top=�6V   $ 0.5,0.5&�6n   z�5�      V� 5�   "   .~�5�   &   �� �� 5�   &   �� 5�   &   $ -90.566Q�66   T� 5�   &   5�   #   5�      �� 5�      -   5        $
��zoom��5�   A   $��-239,240,0,0q�5�   !   $	��Top=�5�      $��20��65   $
 1000Q�5�   !   $ 10000Q�5�      $ 0.493,0.508&�5�      ��. $
��Left=�6�   $	��500��6�   ��6n   Z� z�5�      V� 5�   "   .~�5�   &   �� �� 5�   &   �� 5�   &   $	 -90Q�66   T� 5�   &    T� X$ 8.062Q�69    T� Y$ 7.678Q�6C   5�   #   5�      �� 5�      $
��Zoom��6    $
��Left=�6/   $��34��6J   $��	0,505,0,0q�6V   $	��Top=�6i   $��42��6�   �� 5�      -   5        $ slider1��5�   !   $
 -360Q�5�   !   $ 0Q�5�   !   $	 360Q�5�      $��25��5�   A   $	��Top=�6M   $��
96,539,0,0q�6{   $ 0.5,0.5&�6�   $
��Left=�6�   $	��643��6�   ��6e   Z� 6�   �� 5�      $��Rotate X��6    $��24��63   $��
10,539,0,0q�6?   $	��Top=�6S   $
��Left=�6k   $��65��6�   �� 5�      -   5        $��0��5�   !   $��-30,534,814.6,0q�6-   $ 0.5,0.5&�6[   $��21��6{   $	��Top=�6�   $��Right=�6�   $��21��6�   z�5�      V� 5�   "   .~�5�   &   �� �� 5�   &   �� 5�   &   $
 -0.8Q�66   T� 5�   &   5�   #   5�      �� 5�      -   5        $��0��5�   !   $��21��6-   $��-30,507,814.6,0q�69   $	��Top=�6R   $��Right=�6�   $��21��6�   �� 5�      -   5        $��0��5�   !   $��21��6-   $��-30,476,814.6,0q�69   $	��Top=�6R   $��Right=�6�   $��21��6�   5�      5�      5�      5�      5�      5�      