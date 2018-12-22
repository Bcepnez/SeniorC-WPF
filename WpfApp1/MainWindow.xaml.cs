using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void cleardata()
        {
            showArea.Document.Blocks.Clear();
            showArea.Focus();
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
                    showArea.AppendText(count +
                        "\ndistances : " + distances.ElementAt(count) +
                        "\nangles : " + angles.ElementAt(count) +
                        "\nheights : " + heights.ElementAt(count) +
                        "\nCOX : " + distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))) +
                        "\nCOY : " + distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))) +
                        "\n");

                    sendata(distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))),
                        distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))),
                        heights.ElementAt(count),
                        count
                        );
                    count++;
                }
                //MessageBox.Show(distances.Count().ToString() + " Data added to UAV tab!");
                sr.Close();
            }
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

            //Model3DControl model = new Model3DControl();
            
            //// Declare scene objects.
            //Viewport3D myViewport3D = new Viewport3D();
            //Model3DGroup myModel3DGroup = new Model3DGroup();
            //GeometryModel3D myGeometryModel = new GeometryModel3D();
            //ModelVisual3D myModelVisual3D = new ModelVisual3D();
            //// Defines the camera used to view the 3D object. In order to view the 3D object,
            //// the camera must be positioned and pointed such that the object is within view 
            //// of the camera.
            //PerspectiveCamera myPCamera = new PerspectiveCamera();

            //// Specify where in the 3D scene the camera is.
            //myPCamera.Position = new Point3D(6, 5, 4);

            //// Specify the direction that the camera is pointing.
            //myPCamera.LookDirection = new Vector3D(-6, -5, -4);

            //// Define camera's horizontal field of view in degrees.
            ////myPCamera.FieldOfView = 60;

            //// Asign the camera to the viewport
            ////myViewport3D.Camera = myPCamera;

            ////< Viewport3D.Camera >
            ////     < PerspectiveCamera x: Name = "camMain" Position = "6 5 4" LookDirection = "-6 -5 -4" >
            ////     </ PerspectiveCamera >
            ////</ Viewport3D.Camera >


            //  // Define the lights cast in the scene. Without light, the 3D object cannot 
            //  // be seen. Note: to illuminate an object from additional directions, create 
            //  // additional lights.
            //  DirectionalLight myDirectionalLight = new DirectionalLight();
            //myDirectionalLight.Color = Colors.White;
            //myDirectionalLight.Direction = new Vector3D(-1, -1, -1);

            ////myModel3DGroup.Children.Add(myDirectionalLight);

            ////< ModelVisual3D >
            ////                < ModelVisual3D.Content >
            ////                    < DirectionalLight x: Name = "dirLightMain" Direction = "-1,-1,-1" >

            ////                      </ DirectionalLight >

            ////                  </ ModelVisual3D.Content >

            ////              </ ModelVisual3D >

            //  // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            //  // is created.
            //  MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            //// Create a collection of normal vectors for the MeshGeometry3D.
            //Vector3DCollection myNormalCollection = new Vector3DCollection();
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            ////myNormalCollection.Add(new Vector3D(0, 0, 1));
            ////myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myMeshGeometry3D.Normals = myNormalCollection;

            //// Create a collection of vertex positions for the MeshGeometry3D. 
            Point3DCollection myPositionCollection = new Point3DCollection();
            myPositionCollection.Add(new Point3D(0, 0, 0));
            myPositionCollection.Add(new Point3D(1, 0, 0));
            myPositionCollection.Add(new Point3D(0, 1, 0));
            myPositionCollection.Add(new Point3D(1, 1, 0));
            myPositionCollection.Add(new Point3D(0, 0, 1));
            myPositionCollection.Add(new Point3D(1, 0, 1));
            myPositionCollection.Add(new Point3D(0, 1, 1));
            myPositionCollection.Add(new Point3D(0, 1, 1));
            //myMeshGeometry3D.Positions = myPositionCollection;

            //// Create a collection of texture coordinates for the MeshGeometry3D.
            //PointCollection myTextureCoordinatesCollection = new PointCollection();
            //myTextureCoordinatesCollection.Add(new Point(0, 1));
            //myTextureCoordinatesCollection.Add(new Point(1, 1));
            //myTextureCoordinatesCollection.Add(new Point(0, 0));
            //myTextureCoordinatesCollection.Add(new Point(1, 0));
            ////myTextureCoordinatesCollection.Add(new Point(0, 1));
            ////myTextureCoordinatesCollection.Add(new Point(0, 0));
            //myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            //// Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            //6 3 7  5 6 7  7 4 5  1 2 6  6 5 1  2 1 3  1 0 3  
            myTriangleIndicesCollection.Add(6);
            myTriangleIndicesCollection.Add(6);
            myTriangleIndicesCollection.Add(6);

            myTriangleIndicesCollection.Add(5);
            myTriangleIndicesCollection.Add(6);
            myTriangleIndicesCollection.Add(7);

            myTriangleIndicesCollection.Add(7);
            myTriangleIndicesCollection.Add(4);
            myTriangleIndicesCollection.Add(5);

            myTriangleIndicesCollection.Add(1);
            myTriangleIndicesCollection.Add(2);
            myTriangleIndicesCollection.Add(6);

            myTriangleIndicesCollection.Add(6);
            myTriangleIndicesCollection.Add(5);
            myTriangleIndicesCollection.Add(1);

            myTriangleIndicesCollection.Add(2);
            myTriangleIndicesCollection.Add(1);
            myTriangleIndicesCollection.Add(3);

            myTriangleIndicesCollection.Add(1);
            myTriangleIndicesCollection.Add(0);
            myTriangleIndicesCollection.Add(3);

            //1 5 4  4 0 1  7 3 0  0 4 7
            myTriangleIndicesCollection.Add(1);
            myTriangleIndicesCollection.Add(5);
            myTriangleIndicesCollection.Add(4);

            myTriangleIndicesCollection.Add(4);
            myTriangleIndicesCollection.Add(0);
            myTriangleIndicesCollection.Add(1);

            myTriangleIndicesCollection.Add(7);
            myTriangleIndicesCollection.Add(3);
            myTriangleIndicesCollection.Add(0);

            myTriangleIndicesCollection.Add(0);
            myTriangleIndicesCollection.Add(4);
            myTriangleIndicesCollection.Add(7);

            showArea.AppendText("Show!");
            //myTriangleIndicesCollection.Add(3);
            //myTriangleIndicesCollection.Add(4);
            //myTriangleIndicesCollection.Add(5);
            //myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;
            ////Point3DCollection myTriangleIndicesCollection = new Point3DCollection();
            ////myTriangleIndicesCollection.Add(new Point3D(6, 6, 6));
            ////myTriangleIndicesCollection.Add(new Point3D(6, 3, 7));
            ////myTriangleIndicesCollection.Add(new Point3D(5, 6, 7));
            ////myTriangleIndicesCollection.Add(new Point3D(7, 4, 5));
            ////myTriangleIndicesCollection.Add(new Point3D(1, 2, 6));
            ////myTriangleIndicesCollection.Add(new Point3D(6, 5, 1));
            ////myTriangleIndicesCollection.Add(new Point3D(2, 1, 3));
            ////myTriangleIndicesCollection.Add(new Point3D(1, 0, 3));
            ////myTriangleIndicesCollection.Add(new Point3D(1, 5, 4));
            ////myTriangleIndicesCollection.Add(new Point3D(4, 0, 1));
            ////myTriangleIndicesCollection.Add(new Point3D(7, 3, 0));
            ////myTriangleIndicesCollection.Add(new Point3D(0, 4, 7));
            ////myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            //// Apply the mesh to the geometry model.
            //myGeometryModel.Geometry = myMeshGeometry3D;

            //// The material specifies the material applied to the 3D object. In this sample a  
            //// linear gradient covers the surface of the 3D object.

            //// Create a horizontal linear gradient with four stops.   
            ////LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
            ////myHorizontalGradient.StartPoint = new Point(0, 0.5);
            ////myHorizontalGradient.EndPoint = new Point(1, 0.5);
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            ////myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            //// Define material and apply to the mesh geometries.
            ////DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
            //DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            //SolidColorBrush solidColor = new SolidColorBrush();
            //solidColor.Color = Colors.Bisque;
            //diffuseMaterial.Brush = solidColor;
            //myGeometryModel.Material = diffuseMaterial;

            //// Apply a transform to the object. In this sample, a rotation transform is applied,  
            //// rendering the 3D object rotated.
            ////RotateTransform3D myRotateTransform3D = new RotateTransform3D();
            ////AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D();
            ////myAxisAngleRotation3d.Axis = new Vector3D(0, 3, 0);
            ////myAxisAngleRotation3d.Angle = 40;
            ////myRotateTransform3D.Rotation = myAxisAngleRotation3d;
            ////myGeometryModel.Transform = myRotateTransform3D;

            //// Add the geometry model to the model group.
            ////myModel3DGroup.Children.Add(myGeometryModel);

            //// Add the group of models to the ModelVisual3d.
            ////myModelVisual3D.Content = myModel3DGroup;
            //myModelVisual3D.Content = myGeometryModel;

            ////RotateTranform
            //AxisAngleRotation3D rotate1 = new AxisAngleRotation3D();
            //AxisAngleRotation3D rotate2 = new AxisAngleRotation3D();
            //AxisAngleRotation3D rotate3 = new AxisAngleRotation3D();
            //rotate1.Axis = new Vector3D(0, 0, 1);
            //rotate2.Axis = new Vector3D(0, 1, 0);
            //rotate3.Axis = new Vector3D(1, 0, 0);
            //RotateTransform3D rotateTransformX = new RotateTransform3D();
            //rotateTransformX.Rotation = rotate3;
            //RotateTransform3D rotateTransformY = new RotateTransform3D();
            //rotateTransformY.Rotation = rotate2;
            //RotateTransform3D rotateTransformZ = new RotateTransform3D();
            //rotateTransformZ.Rotation = rotate1;
            //Transform3DGroup transform3DGroup = new Transform3DGroup();
            //transform3DGroup.Children.Add(rotateTransformX);
            //transform3DGroup.Children.Add(rotateTransformY);
            //transform3DGroup.Children.Add(rotateTransformZ);
            //myModelVisual3D.Transform = transform3DGroup;

            //// 
            ////myViewport3D.Children.Add(myModelVisual3D);
            //myViewport3D.Children.Add(myModelVisual3D);
            ////mViewport3D = myViewport3D;
            ////this.mViewport3D = myViewport3D;
            ////this.Content = myViewport3D;

            //// Apply the viewport to the page so it will be rendered.
            ////this.myViewport = myVieาwport3D;
            ///
            //model.CreateModel(myPositionCollection,myTriangleIndicesCollection);
            
        }
    }
}
