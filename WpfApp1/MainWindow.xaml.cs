﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<double> coordinateX = new List<double>();
        List<double> coordinateY = new List<double>();
        List<double> coordinateZ = new List<double>();
        List<double> distance = new List<double>();
        List<double> angle = new List<double>();
        List<double> distances = new List<double>();
        List<double> angles = new List<double>();

        Point3DCollection positionPoint = new Point3DCollection();
        Int32Collection triangleIndice = new Int32Collection();

        AxisAngleRotation3D rotate1 = new AxisAngleRotation3D();
        AxisAngleRotation3D rotate2 = new AxisAngleRotation3D();
        AxisAngleRotation3D rotate3 = new AxisAngleRotation3D();
        ScaleTransform3D scale = new ScaleTransform3D();
        PerspectiveCamera myPCamera = new PerspectiveCamera();

        List<double> h = new List<double>();
        List<double> aPh = new List<double>();
        public MainWindow()
        {
            InitializeComponent();
            zoomValue.Text = zoom.Value.ToString();
            zoom.IsEnabled = false;
            slider1.IsEnabled = false;
            slider1_Copy.IsEnabled = false;
            zoomValue.IsReadOnly = true;
            rotateX.IsReadOnly = true;
            rotateZ.IsReadOnly = true;
        }
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            myPCamera.Position = new Point3D((myPCamera.Position.X - e.Delta / 360D), (myPCamera.Position.Y - e.Delta / 360D), (myPCamera.Position.Z - e.Delta / 360D));
        }
        private void cleardata()
        {
            showArea.Document.Blocks.Clear();
            showArea.Focus();
        }
        private bool checkLayer(double x)
        {
            if (h.Count == 0) return true;
            else
            {
                for (int i = 0; i < h.Count; i++)
                {
                    if (x == h.ElementAt(i))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private bool checkAngle(double x)
        {
            if (aPh.Count == 0) return true;
            else
            {
                for (int i = 0; i < aPh.Count; i++)
                {
                    if (x == aPh.ElementAt(i))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UAV Data Files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                cleardata();
                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog.FileName);
                //MessageBox.Show(sr.ReadToEnd());
                String data;
                int count = 0;
                List<double> heights = new List<double>();

                //Clearbox();
               // Clearbox2();
                while ((data = sr.ReadLine()) != null)
                {
                    String[] token = data.Split(',');
                    distances.Add(Double.Parse(token[0]));
                    angles.Add(Double.Parse(token[1]));
                    heights.Add(Double.Parse(token[2]));
                    if (checkLayer(Double.Parse(token[2])))
                    {
                        h.Add(Double.Parse(token[2]));
                    }
                    if (checkAngle(Double.Parse(token[1])))
                    {
                        aPh.Add(Double.Parse(token[1]));
                    }
                    showArea.AppendText(count +
                        "\ndistances : " + distances.ElementAt(count) +
                        "\nangles : " + angles.ElementAt(count) +
                        "\nheights : " + heights.ElementAt(count) +
                        "\nCOX : " + distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))) +
                        "\nCOY : " + distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))) +
                        "\n");

                    sendata(distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))),
                        distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))),
                        heights.ElementAt(count)*10,
                        count
                        );
                    point(distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))),
                        distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))),
                        heights.ElementAt(count)*10
                        );
                    count++;
                }
                triangleInDices();
                //MessageBox.Show(distances.Count().ToString() + " Data added to UAV tab!");
                //MessageBox.Show(h.Count + " Height Data from file!");
                //MessageBox.Show(aPh.Count + " Angle per height Data from file!");
                //MessageBox.Show(positionPoint.Count + " Position Data from file!");
                //MessageBox.Show(triangleIndice.Count + " Triangle In Dice Data from file!");
                MessageBox.Show("Load data from file completed!");
                render();
                sr.Close();
            }
        }
        private void triangleInDices()
        {
            int n = h.Count;
            int alpha = aPh.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int mul = i * alpha;
                for (int j = 0; j < alpha; j++)
                {
                    int f = (j % alpha) + mul;
                    int s = ((j + 1) % alpha) + mul;
                    int t = ((j + 1) % alpha) + ((i + 1) * alpha);
                    //System.out.println("Triangle in dice : " + f + " , " + s + " , " + t);
                    triangleIndice.Add(f);
                    triangleIndice.Add(s);
                    triangleIndice.Add(t);
                    triangleIndice.Add(t);
                    triangleIndice.Add(s);
                    triangleIndice.Add(f);

                    int f1 = (j % alpha) + mul;
                    int s1 = ((j + 1) % alpha) + ((i + 1) * alpha);
                    int t1 = (j % alpha) + ((i + 1) * alpha);
                    triangleIndice.Add(f1);
                    triangleIndice.Add(s1);
                    triangleIndice.Add(t1);
                    triangleIndice.Add(t1);
                    triangleIndice.Add(s1);
                    triangleIndice.Add(f1);
                }
            }
        }
        private void point(double x , double y, double z)
        {
            //return (new Point3D(x, z, y));
            positionPoint.Add(new Point3D(x, z, y));
        }
        private void sendata(double coX, double coY, double coZ, int counter)
        {
            coordinateX.Add(coX);
            coordinateY.Add(coY);
            coordinateZ.Add(coZ);
            showArea2.AppendText(counter +
                "\nX : " + coordinateX.ElementAt(counter) +
                "\nY : " + coordinateY.ElementAt(counter) +
                "\nZ : " + coordinateZ.ElementAt(counter) + "\n");

        }
        private double toRadians(double angleVal)
        {
            return (Math.PI / 180) * angleVal;
        }
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string richText = new TextRange(showArea.Document.ContentStart, showArea.Document.ContentEnd).Text;
            if (richText!= "")
            {
                SaveFileDialog saveFileDialogUAV = new SaveFileDialog();
                saveFileDialogUAV.InitialDirectory = @"C:\";
                saveFileDialogUAV.RestoreDirectory = true;
                saveFileDialogUAV.DefaultExt = "csv";
                saveFileDialogUAV.Title = "Browse CSV Files";
                saveFileDialogUAV.Filter = "Drone data files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialogUAV.FilterIndex = 1;
                if (saveFileDialogUAV.ShowDialog() == true )
                {
                    using (Stream s = File.Open(saveFileDialogUAV.FileName, FileMode.CreateNew))
                    using (StreamWriter sw = new StreamWriter(s))
                    {
                        int i = 0;
                        while (i < coordinateX.Count)
                        {
                            sw.Write(distances.ElementAt(i) + "," +
                                angles.ElementAt(i) + "," +
                                coordinateX.ElementAt(i) + "," +
                                coordinateY.ElementAt(i) + "," +
                                coordinateZ.ElementAt(i) + "\n");
                            i++;
                        }
                        MessageBox.Show("Save Complete!");
                    }
                }
            }
            else{
                MessageBox.Show("No data to Save!");
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ShowArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Test(object sender, RoutedEventArgs e)
        {

            //zoom.IsEnabled = true;
            //slider1.IsEnabled = true;
            //slider1_Copy.IsEnabled = true;
            ////// Declare scene objects.
            ////Viewport3D myViewport3D1 = new Viewport3D();
            //Model3DGroup myModel3DGroup = new Model3DGroup();
            //GeometryModel3D myGeometryModel = new GeometryModel3D();
            //ModelVisual3D myModelVisual3D = new ModelVisual3D();
            //// Defines the camera used to view the 3D object. In order to view the 3D object,
            //// the camera must be positioned and pointed such that the object is within view 
            //// of the camera.
            ////PerspectiveCamera myPCamera = new PerspectiveCamera();

            //// Specify where in the 3D scene the camera is.
            //myPCamera.Position = new Point3D(6, 5, 4);

            //// Specify the direction that the camera is pointing.
            //myPCamera.LookDirection = new Vector3D(-6, -5, -4);

            //// Define camera's horizontal field of view in degrees.
            //myPCamera.FieldOfView = 60;

            //// Asign the camera to the viewport
            //viewport3D1.Camera = myPCamera;

            //////< Viewport3D.Camera >
            //////     < PerspectiveCamera x: Name = "camMain" Position = "6 5 4" LookDirection = "-6 -5 -4" >
            //////     </ PerspectiveCamera >
            //////</ Viewport3D.Camera >


            //// Define the lights cast in the scene. Without light, the 3D object cannot 
            //// be seen. Note: to illuminate an object from additional directions, create 
            //// additional lights.
            //DirectionalLight myDirectionalLight = new DirectionalLight();
            //myDirectionalLight.Color = Colors.White;
            //myDirectionalLight.Direction = new Vector3D(-1, -1, -1);

            //myModel3DGroup.Children.Add(myDirectionalLight);

            //////< ModelVisual3D >
            //////                < ModelVisual3D.Content >
            //////                    < DirectionalLight x: Name = "dirLightMain" Direction = "-1,-1,-1" >

            //////                      </ DirectionalLight >

            //////                  </ ModelVisual3D.Content >

            //////              </ ModelVisual3D >

            ////  // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            ////  // is created.
            //MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            ////// Create a collection of normal vectors for the MeshGeometry3D.
            //Vector3DCollection myNormalCollection = new Vector3DCollection();
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myMeshGeometry3D.Normals = myNormalCollection;

            ////// Create a collection of vertex positions for the MeshGeometry3D. 
            ////Point3DCollection myPositionCollection = new Point3DCollection();
            ////myPositionCollection.Add(new Point3D(0, 0, 0));
            ////myPositionCollection.Add(new Point3D(1, 0, 0));
            ////myPositionCollection.Add(new Point3D(0, 1, 0));
            ////myPositionCollection.Add(new Point3D(1, 1, 0));
            ////myPositionCollection.Add(new Point3D(0, 0, 1));
            ////myPositionCollection.Add(new Point3D(1, 0, 1));
            ////myPositionCollection.Add(new Point3D(0, 1, 1));
            ////myPositionCollection.Add(new Point3D(1, 1, 1));
            ////myMeshGeometry3D.Positions = myPositionCollection;
            //myMeshGeometry3D.Positions = positionPoint;

            ////// Create a collection of texture coordinates for the MeshGeometry3D.
            //PointCollection myTextureCoordinatesCollection = new PointCollection();
            //myTextureCoordinatesCollection.Add(new Point(0, 1));
            //myTextureCoordinatesCollection.Add(new Point(1, 1));
            //myTextureCoordinatesCollection.Add(new Point(0, 0));
            //myTextureCoordinatesCollection.Add(new Point(1, 0));
            //myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            ////// Create a collection of triangle indices for the MeshGeometry3D.
            //Int32Collection myTriangleIndicesCollection = new Int32Collection();
            ////6 6 6  6 3 7  5 6 7  7 4 5  1 2 6  6 5 1  2 1 3  1 0 3  
            //myTriangleIndicesCollection.Add(6);
            //myTriangleIndicesCollection.Add(2);
            //myTriangleIndicesCollection.Add(3);

            //myTriangleIndicesCollection.Add(6);
            //myTriangleIndicesCollection.Add(3);
            //myTriangleIndicesCollection.Add(7);

            //myTriangleIndicesCollection.Add(5);
            //myTriangleIndicesCollection.Add(6);
            //myTriangleIndicesCollection.Add(7);

            //myTriangleIndicesCollection.Add(7);
            //myTriangleIndicesCollection.Add(4);
            //myTriangleIndicesCollection.Add(5);

            //myTriangleIndicesCollection.Add(1);
            //myTriangleIndicesCollection.Add(2);
            //myTriangleIndicesCollection.Add(6);

            //myTriangleIndicesCollection.Add(6);
            //myTriangleIndicesCollection.Add(5);
            //myTriangleIndicesCollection.Add(1);

            //myTriangleIndicesCollection.Add(2);
            //myTriangleIndicesCollection.Add(1);
            //myTriangleIndicesCollection.Add(3);

            //myTriangleIndicesCollection.Add(1);
            //myTriangleIndicesCollection.Add(0);
            //myTriangleIndicesCollection.Add(3);

            ////1 5 4  4 0 1  7 3 0  0 4 7
            //myTriangleIndicesCollection.Add(1);
            //myTriangleIndicesCollection.Add(5);
            //myTriangleIndicesCollection.Add(4);

            //myTriangleIndicesCollection.Add(4);
            //myTriangleIndicesCollection.Add(0);
            //myTriangleIndicesCollection.Add(1);

            //myTriangleIndicesCollection.Add(7);
            //myTriangleIndicesCollection.Add(3);
            //myTriangleIndicesCollection.Add(0);

            //myTriangleIndicesCollection.Add(0);
            //myTriangleIndicesCollection.Add(4);
            //myTriangleIndicesCollection.Add(7);

            //showArea.AppendText("Show!");
            ////myTriangleIndicesCollection.Add(3);
            ////myTriangleIndicesCollection.Add(4);
            ////myTriangleIndicesCollection.Add(5);
            ////myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            //myMeshGeometry3D.TriangleIndices = triangleIndice;

            ////// Apply the mesh to the geometry model.
            //myGeometryModel.Geometry = myMeshGeometry3D;

            ////// Create a horizontal linear gradient with four stops.   
            ////LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
            ////myHorizontalGradient.StartPoint = new Point(0, 0.5);
            ////myHorizontalGradient.EndPoint = new Point(1, 0.5);
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            ////// Define material and apply to the mesh geometries.
            ////DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            ////myGeometryModel.Material = myMaterial;


            //DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            //SolidColorBrush solidColor = new SolidColorBrush();
            //solidColor.Color = Colors.Bisque;
            //diffuseMaterial.Brush = solidColor;
            //myGeometryModel.Material = diffuseMaterial;

            ////// Apply a transform to the object. In this sample, a rotation transform is applied,  
            ////// rendering the 3D object rotated.
            //////RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            //////AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D();
            //////myAxisAngleRotation3d.Axis = new Vector3D(0, 3, 0);
            //////myAxisAngleRotation3d.Angle = 40;
            //////myRotateTransform3D.Rotation = myAxisAngleRotation3d;
            //////myGeometryModel.Transform = myRotateTransform3D;

            ////// Add the geometry model to the model group.
            //myModel3DGroup.Children.Add(myGeometryModel);

            ////// Add the group of models to the ModelVisual3d.
            //myModelVisual3D.Content = myModel3DGroup;
            ////myModelVisual3D.Content = myGeometryModel;

            //////RotateTranform
            ////AxisAngleRotation3D rotate1 = new AxisAngleRotation3D();
            ////AxisAngleRotation3D rotate2 = new AxisAngleRotation3D();
            ////AxisAngleRotation3D rotate3 = new AxisAngleRotation3D();
            //rotate1.Axis = new Vector3D(0, 0, 1);
            //rotate2.Axis = new Vector3D(0, 1, 0);
            //rotate2.Angle = slider1.Value;
            //rotate3.Axis = new Vector3D(1, 0, 0);
            //rotate3.Angle = slider1_Copy.Value;
            //RotateTransform3D rotatetransformx = new RotateTransform3D();
            //rotatetransformx.Rotation = rotate3;
            //RotateTransform3D rotatetransformy = new RotateTransform3D();
            //rotatetransformy.Rotation = rotate2;
            //RotateTransform3D rotatetransformz = new RotateTransform3D();
            //rotatetransformz.Rotation = rotate1;
            //Transform3DGroup transform3dgroup = new Transform3DGroup();

            //scale.CenterX = 0;
            //scale.CenterY = 0;
            //scale.CenterZ = 0;
            //scale.ScaleX = 1;
            //scale.ScaleY = 1;
            //scale.ScaleZ = 1;

            //double zoomVal = zoom.Value;
            ////ScaleTransform3D zoomer = new ScaleTransform3D(zoomVal,zoomVal,zoomVal);
            ////transform3dgroup.Children.Add(zoomer);
            //transform3dgroup.Children.Add(scale);
            //transform3dgroup.Children.Add(rotatetransformx);
            //transform3dgroup.Children.Add(rotatetransformy);
            //transform3dgroup.Children.Add(rotatetransformz);
            //myModelVisual3D.Transform = transform3dgroup;

            //viewport3D1.Children.Clear();
            //viewport3D1.Children.Add(myModelVisual3D);
            render();
        }

        private void render()
        {
            zoom.IsEnabled = true;
            slider1.IsEnabled = true;
            slider1_Copy.IsEnabled = true;
            //// Declare scene objects.
            //Viewport3D myViewport3D1 = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            // Defines the camera used to view the 3D object. In order to view the 3D object,
            // the camera must be positioned and pointed such that the object is within view 
            // of the camera.
            //PerspectiveCamera myPCamera = new PerspectiveCamera();

            // Specify where in the 3D scene the camera is.
            myPCamera.Position = new Point3D(6, 5, 4);

            // Specify the direction that the camera is pointing.
            myPCamera.LookDirection = new Vector3D(-6, -5, -4);

            // Define camera's horizontal field of view in degrees.
            myPCamera.FieldOfView = 60;

            // Asign the camera to the viewport
            viewport3D1.Camera = myPCamera;

            ////< Viewport3D.Camera >
            ////     < PerspectiveCamera x: Name = "camMain" Position = "6 5 4" LookDirection = "-6 -5 -4" >
            ////     </ PerspectiveCamera >
            ////</ Viewport3D.Camera >


            // Define the lights cast in the scene. Without light, the 3D object cannot 
            // be seen. Note: to illuminate an object from additional directions, create 
            // additional lights.
            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(-1, -1, -1);

            myModel3DGroup.Children.Add(myDirectionalLight);

            ////< ModelVisual3D >
            ////                < ModelVisual3D.Content >
            ////                    < DirectionalLight x: Name = "dirLightMain" Direction = "-1,-1,-1" >

            ////                      </ DirectionalLight >

            ////                  </ ModelVisual3D.Content >

            ////              </ ModelVisual3D >

            //  // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            //  // is created.
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            //// Create a collection of normal vectors for the MeshGeometry3D.
            Vector3DCollection myNormalCollection = new Vector3DCollection();
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myMeshGeometry3D.Normals = myNormalCollection;

            //// Create a collection of vertex positions for the MeshGeometry3D. 
            myMeshGeometry3D.Positions = positionPoint;

            //// Create a collection of texture coordinates for the MeshGeometry3D.
            PointCollection myTextureCoordinatesCollection = new PointCollection();
            myTextureCoordinatesCollection.Add(new Point(0, 1));
            myTextureCoordinatesCollection.Add(new Point(1, 1));
            myTextureCoordinatesCollection.Add(new Point(0, 0));
            myTextureCoordinatesCollection.Add(new Point(1, 0));
            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            //// Create a collection of triangle indices for the MeshGeometry3D.
            showArea.AppendText("Show!");
            myMeshGeometry3D.TriangleIndices = triangleIndice;

            //// Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;

            //// Create a horizontal linear gradient with four stops.   
            //LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
            //myHorizontalGradient.StartPoint = new Point(0, 0.5);
            //myHorizontalGradient.EndPoint = new Point(1, 0.5);
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            //myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            //// Define material and apply to the mesh geometries.
            //DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            //myGeometryModel.Material = myMaterial;


            DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            SolidColorBrush solidColor = new SolidColorBrush();
            solidColor.Color = Colors.Bisque;
            diffuseMaterial.Brush = solidColor;
            myGeometryModel.Material = diffuseMaterial;

            //// Add the geometry model to the model group.
            myModel3DGroup.Children.Add(myGeometryModel);

            //// Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            ////RotateTranform
            rotate1.Axis = new Vector3D(0, 0, 1);
            rotate2.Axis = new Vector3D(0, 1, 0);
            rotate2.Angle = slider1.Value;
            rotate3.Axis = new Vector3D(1, 0, 0);
            rotate3.Angle = slider1_Copy.Value;
            RotateTransform3D rotatetransformx = new RotateTransform3D();
            rotatetransformx.Rotation = rotate3;
            RotateTransform3D rotatetransformy = new RotateTransform3D();
            rotatetransformy.Rotation = rotate2;
            RotateTransform3D rotatetransformz = new RotateTransform3D();
            rotatetransformz.Rotation = rotate1;
            Transform3DGroup transform3dgroup = new Transform3DGroup();

            scale.CenterX = 0;
            scale.CenterY = 0;
            scale.CenterZ = 0;
            scale.ScaleX = 1;
            scale.ScaleY = 1;
            scale.ScaleZ = 1;

            double zoomVal = zoom.Value;
            //ScaleTransform3D zoomer = new ScaleTransform3D(zoomVal,zoomVal,zoomVal);
            //transform3dgroup.Children.Add(zoomer);
            transform3dgroup.Children.Add(scale);
            transform3dgroup.Children.Add(rotatetransformx);
            transform3dgroup.Children.Add(rotatetransformy);
            transform3dgroup.Children.Add(rotatetransformz);
            myModelVisual3D.Transform = transform3dgroup;

            viewport3D1.Children.Clear();
            viewport3D1.Children.Add(myModelVisual3D);
        }

        private void Zoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double zoomVal = zoom.Value;
            zoomValue.Text = zoomVal.ToString();
            scale.ScaleX = 1/zoomVal ;
            scale.ScaleY = 1/zoomVal ;
            scale.ScaleZ = 1/zoomVal ;
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rotate2.Angle = slider1.Value;
            rotateX.Text = slider1.Value.ToString();
        }

        private void Slider1_Copy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rotate3.Angle = slider1_Copy.Value;
            rotateZ.Text = slider1_Copy.Value.ToString();
        }
    }
}
