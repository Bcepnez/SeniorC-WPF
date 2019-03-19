using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace LiMESH
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int meshSelected = 0;
        static int pointCloudSelected = 1;
        static int notSelected = 2;
        static string FTPSERVER = "ftp://192.168.1.48/";

        List<double> coordinateX = new List<double>();
        List<double> coordinateY = new List<double>();
        List<double> coordinateZ = new List<double>();

        //Collect all data from file
        List<double> distances = new List<double>();
        List<double> angles = new List<double>();
        List<double> heights = new List<double>();

        //List of Distince data
        List<double> angleList = new List<double>();
        List<double> heightList = new List<double>();

        //filled in data ready to calculate
        List<double> distanceData = new List<double>();
        List<double> angleData = new List<double>();
        List<double> heightData = new List<double>();
        
        //3D Control
        Point3DCollection positionPoint = new Point3DCollection();
        Int32Collection triangleIndice = new Int32Collection();

        AxisAngleRotation3D rotate1 = new AxisAngleRotation3D();
        AxisAngleRotation3D rotate2 = new AxisAngleRotation3D();
        AxisAngleRotation3D rotate3 = new AxisAngleRotation3D();
        ScaleTransform3D scale = new ScaleTransform3D();
        PerspectiveCamera myPCamera = new PerspectiveCamera();

        //Color Contrrol
        Color ColorTest;
        

        //Display type control
        private bool mode;
        bool LightStatus;

        BackgroundWorker worker = new BackgroundWorker();

        //Setting method
        public MainWindow()
        {
            InitializeComponent();
            zoomValue.Text = (1 / (zoom.Value / 1000)).ToString();
            zoom.IsEnabled = false;
            slider1.IsEnabled = false;
            slider1_Copy.IsEnabled = false;
            zoomValue.IsReadOnly = true;
            rotateX.IsReadOnly = true;
            rotateZ.IsReadOnly = true;
            type.IsEnabled = false;
            Light.IsEnabled = false;
            cbColors.IsEnabled = false;
            System.Drawing.KnownColor t = new System.Drawing.KnownColor();
            foreach (System.Drawing.KnownColor kc in System.Enum.GetValues(t.GetType()))
            {
                System.Drawing.ColorConverter cc = new System.Drawing.ColorConverter();
                System.Drawing.Color c = System.Drawing.Color.FromName(kc.ToString());

                if (!c.IsSystemColor)
                    cbColors.Items.Add(c);
            }
        }
        private void ColorBoxEnable()
        {
            cbColors.IsEnabled = true;
        }
        private void ColorBoxDisable()
        {
            cbColors.IsEnabled = false;
        }
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            myPCamera.Position = new Point3D((myPCamera.Position.X - e.Delta / 360D), (myPCamera.Position.Y - e.Delta / 360D), (myPCamera.Position.Z - e.Delta / 360D));
        }
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Icon_MouseMove(object sender, MouseEventArgs e)
        {
            //myPCamera.LookDirection = new Vector3D(myPCamera.LookDirection.X - 150, myPCamera.LookDirection.Y - 150, myPCamera.LookDirection.Z - 50);
        }


                                    
        //Selection
        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int Item = type.SelectedIndex;
            if (Item == meshSelected)
            {
                render();
                Light.IsEnabled = true;
                mode = true;
            }
            else if (Item == pointCloudSelected)
            {
                CreatePointCloud(positionPoint);
                Light.IsEnabled = false;
                mode = false;
            }
            else if (Item == notSelected)
            {
                zoom.IsEnabled = false;
                slider1.IsEnabled = false;
                slider1_Copy.IsEnabled = false;
                zoomValue.IsReadOnly = true;
                rotateX.IsReadOnly = true;
                rotateZ.IsReadOnly = true;
                Light.IsEnabled = false;
                ColorBoxDisable();
            }
        }

        private void Light_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Light.SelectedIndex;
            if (select == 0)
            {
                LightStatus = true;
            }
            else if (select == 1)
            {
                LightStatus = false;
            }
            render();
        }

        private void cbColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Drawing.Color color = (System.Drawing.Color)cbColors.SelectedItem;
            ColorTest = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
            if (mode) render();
            else CreatePointCloud(positionPoint);
        }



        //Calculate method
        private void cal()
        {
            int max = distanceData.Count;
            for (int i = 0; i < max; i++)
            {
                point(distanceData.ElementAt(i) * (Math.Sin(toRadians(angleData.ElementAt(i)))),
                    distanceData.ElementAt(i) * (Math.Cos(toRadians(angleData.ElementAt(i)))),
                    heightData.ElementAt(i) * 100
                    );
            }
        }
        private void datarestruct()
        {
            int ang = angleList.Count;
            int n = angleData.Count / ang;

            int currentPoint = 0;
            int lastPoint = 0;
            for (int i = 0; i < n; i++)
            {
                if (i == 0) lastPoint = 0;
                else lastPoint += ang;
                for (int j = 0; j < ang; j++)
                {
                    if (distanceData.ElementAt(currentPoint) <= 10)
                    {
                        double nextVal;
                        int nextPoint = (currentPoint);
                        do
                        {
                            nextPoint++;
                        } while ((nextVal = distanceData.ElementAt((nextPoint % ang) + lastPoint)) <= 10);
                        if (currentPoint == 0)
                        {
                            double befVal;
                            int befPoint = 0;
                            do
                            {
                                befPoint++;
                            } while ((befVal = distanceData.ElementAt((ang - (befPoint)) + lastPoint)) <= 10);
                            distanceData[0] = (befVal + nextVal) / 2;
                        }
                        else
                        {
                            distanceData[currentPoint] = (distanceData.ElementAt(currentPoint - 1) + nextVal) / 2;
                        }
                    }
                    currentPoint++;
                }
            }
        }
        private void CreatePointCloud(Point3DCollection points)
        {
            zoom.IsEnabled = true;
            slider1.IsEnabled = true;
            slider1_Copy.IsEnabled = true;
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            myPCamera.Position = new Point3D(6, 5, 4);
            myPCamera.LookDirection = new Vector3D(-6, -5, -4);
            myPCamera.FieldOfView = 60;
            viewport3D1.Camera = myPCamera;

            DirectionalLight myDirectionalLight = new DirectionalLight();
            myDirectionalLight.Color = Colors.White;
            myDirectionalLight.Direction = new Vector3D(-1, -1, -1);

            myModel3DGroup.Children.Add(myDirectionalLight);

            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            //// Create a collection of vertex positions for the MeshGeometry3D. 
            for (int i = 0; i < points.Count; i++)
            {
                AddCubeToMesh(myMeshGeometry3D, points[i], 10);
            }

            //// Apply the mesh to the geometry model.
            myGeometryModel.Geometry = myMeshGeometry3D;


            DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            SolidColorBrush solidColor = new SolidColorBrush();
            //solidColor.Color = Colors.Red;
            solidColor.Color = ColorTest;
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
            getZoomValue();

            transform3dgroup.Children.Add(scale);
            transform3dgroup.Children.Add(rotatetransformx);
            transform3dgroup.Children.Add(rotatetransformy);
            transform3dgroup.Children.Add(rotatetransformz);
            myModelVisual3D.Transform = transform3dgroup;

            viewport3D1.Children.Clear();
            viewport3D1.Children.Add(myModelVisual3D);
        }
        private void render()
        {
            zoom.IsEnabled = true;
            slider1.IsEnabled = true;
            slider1_Copy.IsEnabled = true;
            bgIMG.Visibility = Visibility.Hidden;
            //// Declare scene objects.
            Model3DGroup myModel3DGroup = new Model3DGroup();
            GeometryModel3D myGeometryModel = new GeometryModel3D();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();
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
            //DirectionalLight myDirectionalLight = new DirectionalLight();
            //myDirectionalLight.Color = Colors.White;
            //myDirectionalLight.Direction = new Vector3D(-200, -2, -200);
            //dirLightMain = myDirectionalLight;

            if (LightStatus)
            {
                AmbientLight myDirectionalLight = new AmbientLight();
                myDirectionalLight.Color = Colors.White;
                myModel3DGroup.Children.Add(myDirectionalLight);
            }
            else
            {
                AmbientLight myDirectionalLight = new AmbientLight();
                myDirectionalLight.Color = Color.FromRgb(66, 66, 66);
                myModel3DGroup.Children.Add(myDirectionalLight);

                DirectionalLight myDirectionalLight1 = new DirectionalLight();
                myDirectionalLight1.Color = Color.FromRgb(44, 44, 44);
                myDirectionalLight1.Direction = new Vector3D(0, -1, -1);
                myModel3DGroup.Children.Add(myDirectionalLight1);

                SpotLight spot = new SpotLight();
                spot.Color = Color.FromRgb(66, 66, 66);
                spot.Direction = new Vector3D(0, 0, -1);
                spot.InnerConeAngle = 30;
                spot.OuterConeAngle = 60;
                spot.Position = new Point3D(0, 1, 30);
                myModel3DGroup.Children.Add(spot);
            }


            //  // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet 
            //  // is created.
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            //// Create a collection of normal vectors for the MeshGeometry3D.
            //Vector3DCollection myNormalCollection = new Vector3DCollection();
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myNormalCollection.Add(new Vector3D(0, 0, 1));
            //myMeshGeometry3D.Normals = myNormalCollection;

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


            //DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            //SolidColorBrush solidColor = new SolidColorBrush();
            //solidColor.Color = Colors.Bisque;
            //diffuseMaterial.Brush = solidColor;
            //myGeometryModel.Material = diffuseMaterial;

            DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
            SolidColorBrush solidColor = new SolidColorBrush();
            //solidColor.Color = Colors.Red;
            solidColor.Color = ColorTest;
            diffuseMaterial.Brush = solidColor;
            myGeometryModel.Material = diffuseMaterial;

            //DiffuseMaterial surface_material = new DiffuseMaterial(Brushes.Orange);
            //myGeometryModel.Material = surface_material;

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
            getZoomValue();

            transform3dgroup.Children.Add(scale);
            transform3dgroup.Children.Add(rotatetransformx);
            transform3dgroup.Children.Add(rotatetransformy);
            transform3dgroup.Children.Add(rotatetransformz);
            myModelVisual3D.Transform = transform3dgroup;

            viewport3D1.Children.Clear();
            viewport3D1.Children.Add(myModelVisual3D);
        }
        private void AddCubeToMesh(MeshGeometry3D mesh, Point3D center, double size)
        {
            if (mesh != null)
            {
                int offset = mesh.Positions.Count;

                mesh.Positions.Add(new Point3D(center.X - size, center.Y + size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y + size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y + size, center.Z + size));
                mesh.Positions.Add(new Point3D(center.X - size, center.Y + size, center.Z + size));
                mesh.Positions.Add(new Point3D(center.X - size, center.Y - size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y - size, center.Z - size));
                mesh.Positions.Add(new Point3D(center.X + size, center.Y - size, center.Z + size));
                mesh.Positions.Add(new Point3D(center.X - size, center.Y - size, center.Z + size));

                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 6);

                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 6);
                mesh.TriangleIndices.Add(offset + 7);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 5);
                mesh.TriangleIndices.Add(offset + 6);

                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 4);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 7);

                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 6);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 5);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 0);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 1);
            }
        }
        private void collect()
        {
            preprocess();
            angleList = angles.Distinct().ToList(); ;
            angleList.Sort();
            heightList = heights.Distinct().ToList();
            heightList.Sort();
            fill();
            datarestruct();
            showData();
        }
        private void showData()
        {
            for (int i = 0; i < (distanceData.Count); i++)
            {
                ShowareaList.Items.Add("No." + ((i % angleList.Count) + 1) + ": Dis :" + distanceData.ElementAt(i) + ": Angle :" + angleData.ElementAt(i) + ": Height :" + heightData.ElementAt(i));
            }
        }
        private void preprocess()
        {
            int totals = distances.Count;
            for (int i = 0; i < distances.Count; i++)
            {
                if (angles[i % distances.Count] == angles[(i + 1) % distances.Count])
                {
                    if (distances[i % distances.Count] != 0 && distances[(i + 1) % distances.Count] != 0)
                        distances[i % distances.Count] = (distances[i % distances.Count] + distances[(i + 1) % distances.Count]) / 2;
                    else
                    {
                        if (distances[i] == 0) distances[i] = distances[i + 1];
                        else distances[i] = distances[i];
                    }
                    distances.RemoveAt(i + 1);
                    heights.RemoveAt(i + 1);
                    angles.RemoveAt(i + 1);
                }
            }
        }
        private void fillInData()
        {
            int totals = distances.Count;
            int pos = 0;
            //for (int i = 0; i < totals; i++)
            //{
            //    while (angles.ElementAt(i) != angleList.ElementAt(pos % angleList.Count))
            //    {
            //        pos++;
            //    }
            //    distanceData[pos] = distances.ElementAt(i);
            //}
            for (int i = 0; i < totals; i++)
            {
                while (angles[i] != angleData[pos])
                {
                    pos++;
                }
                distanceData[pos] = distances[i];
            }
        }
        private void fill()
        {
            int n = heightList.Count;
            int ang = 360;/*angleList.Count;*/
            int total = n * ang;

            for (int i = 0; i < total; i++)
            {
                distanceData.Add(1);
                angleData.Add(angleList.ElementAt(i % ang));
                heightData.Add(heightList.ElementAt(i / ang));
            }
            fillInData();
        }
        private void triangleInDices()
        {
            //Create Triangle in dice
            int n = heightList.Count;
            int angle = angleList.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int mul = (i * angle);
                for (int j = 0; j < angle; j++)
                {
                    int f = (j % angle) + mul;
                    int s = ((j + 1) % angle) + mul;
                    int t = ((j + 1) % angle) + ((i + 1) * angle);
                    triangleIndice.Add(f);
                    triangleIndice.Add(s);
                    triangleIndice.Add(t);
                    triangleIndice.Add(t);
                    triangleIndice.Add(s);
                    triangleIndice.Add(f);

                    int f1 = (j % angle) + mul;
                    int s1 = ((j + 1) % angle) + ((i + 1) * angle);
                    int t1 = (j % angle) + ((i + 1) * angle);
                    triangleIndice.Add(f1);
                    triangleIndice.Add(s1);
                    triangleIndice.Add(t1);
                    triangleIndice.Add(t1);
                    triangleIndice.Add(s1);
                    triangleIndice.Add(f1);
                }
            }
        }
        private void point(double x, double y, double z)
        {
            positionPoint.Add(new Point3D(x, z, y));

            coordinateX.Add(x);
            coordinateY.Add(y);
            coordinateZ.Add(z);
        }
        private double toRadians(double angleVal)
        {
            return (Math.PI / 180) * angleVal;
        }

        

        //Save method
        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowareaList.Items.Count != 0)
            {
                SaveFileDialog saveFileDialogUAV = new SaveFileDialog();
                saveFileDialogUAV.InitialDirectory = @"C:\";
                saveFileDialogUAV.RestoreDirectory = true;
                saveFileDialogUAV.DefaultExt = "csv";
                saveFileDialogUAV.Title = "Save in CSV File format";
                saveFileDialogUAV.Filter = "Drone data files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialogUAV.FilterIndex = 1;
                if (saveFileDialogUAV.ShowDialog() == true)
                {
                    using (Stream s = File.Open(saveFileDialogUAV.FileName, FileMode.CreateNew))
                    using (StreamWriter sw = new StreamWriter(s))
                    {
                        int i = 0;
                        while (i < distanceData.Count)
                        {
                            sw.Write(distanceData.ElementAt(i) + "," +
                                angleData.ElementAt(i) + "," +
                                heightData.ElementAt(i) + "," +
                                coordinateX.ElementAt(i) + "," +
                                coordinateY.ElementAt(i) + "," +
                                coordinateZ.ElementAt(i) + "\n");
                            i++;
                        }
                        MessageBox.Show("Save Complete!");
                    }
                }
            }
            else
            {
                MessageBox.Show("No data to Save!");
            }
        }
        private void export(object sender, RoutedEventArgs e)
        {
            export();
        }
        private void export()
        {
            string header = "solid MySOLID\n";
            string tail = "endsolid MySOLID";
            string facet = "facet normal ";
            string endfacet = "endfacet\n";
            string loop = "outer loop\n";
            string endloop = "endloop\n";
            string vertex = "vertex ";

            SaveFileDialog saveFileDialogUAV = new SaveFileDialog();
            saveFileDialogUAV.InitialDirectory = @"C:\";
            saveFileDialogUAV.RestoreDirectory = true;
            saveFileDialogUAV.DefaultExt = "stl";
            saveFileDialogUAV.Title = "Save in STL File format";
            saveFileDialogUAV.Filter = "STL file (*.stl)|*.stl|All files (*.*)|*.*";
            saveFileDialogUAV.FilterIndex = 1;
            if (saveFileDialogUAV.ShowDialog() == true)
            {
                using (Stream s = File.Open(saveFileDialogUAV.FileName, FileMode.CreateNew))
                using (StreamWriter sw = new StreamWriter(s))
                {

                    sw.Write(header +
                        facet + " 0.0 -1.0 0.0\n" +
                        loop +
                        vertex + "0.0 0.0 0.0" + "\n" +
                        vertex + "1.0 0.0 0.0" + "\n" +
                        vertex + "0.0 0.0 1.0" + "\n" +
                        "\n" + endloop +
                        endfacet +

                        facet + " 0.0 0.0 -1.0\n" +
                        loop +
                        vertex + "0.0 0.0 0.0" + "\n" +
                        vertex + "0.0 1.0 0.0" + "\n" +
                        vertex + "1.0 0.0 0.0" + "\n" +
                        "\n" + endloop +
                        endfacet +

                        facet + " -1.0 0.0 0.0\n" +
                        loop +
                        vertex + "0.0 0.0 0.0" + "\n" +
                        vertex + "0.0 0.0 1.0" + "\n" +
                        vertex + "0.0 1.0 0.0" + "\n" +
                        "\n" + endloop +
                        endfacet +

                        facet + " 0.577 0.577 0.577\n" +
                        loop +
                        vertex + "1.0 0.0 0.0" + "\n" +
                        vertex + "0.0 1.0 0.0" + "\n" +
                        vertex + "0.0 0.0 1.0" + "\n" +
                        "\n" + endloop +
                        endfacet +
                        tail);
                    MessageBox.Show("Save Complete!");
                }
            }

        }
        


        //Load method
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UAV Data Files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == true)
            {

                bgIMG.Visibility = Visibility.Visible;
                cleardata();
                System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog.FileName);
                String data;
                while ((data = sr.ReadLine()) != null)
                {
                    String[] token = data.Split(',');
                    distances.Add(Double.Parse(token[0]));
                    //angles.Add((Double.Parse(token[1])));
                    angles.Add((int)(Double.Parse(token[1])));
                    heights.Add(Double.Parse(token[2]));
                }
                MessageBox.Show("Load data from file completed!\nPlease wait for a while...");
                collect();
                cal();
                triangleInDices();
                MessageBox.Show("Calculate data completed!");
                Light.SelectedIndex = 1;
                cbColors.SelectedIndex = 72;
                type.SelectedIndex = 0;
                type.IsEnabled = true;
                ColorBoxEnable();
                bgIMG.Visibility = Visibility.Hidden;
                sr.Close();
            }
        }



        //Network method
        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fileList.SelectedIndex != -1)
            {
                Get_Data_From_FTP_Server_File(fileList.SelectedItem.ToString());
            }
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            fileList.Items.Clear();
            fileList.IsSynchronizedWithCurrentItem = false;
            fileList.SelectedItem = null;
            ListFiles();
        }
        private void Get_Data_From_FTP_Server_File(string fi)
        {
            //used to display data into rich text.box
            String result = String.Empty;
            string ftpURI = FTPSERVER + fi;
            string fileExt = Path.GetExtension(fi);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpURI);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            //set up credentials. 
            request.Credentials = new NetworkCredential("", "");
            //initialize Ftp response.
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            //open readers to read data from ftp 
            Stream responsestream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responsestream);
            //read data from FTP
            result = reader.ReadToEnd();
            //save file locally on your pc
            string filename = "Recieved_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + fileExt;
            MessageBox.Show("You Select : " + fileList.SelectedItem.ToString() + "\nSave as : " + filename + "\nSave file completed!");
            using (StreamWriter file = File.CreateText(filename))
            {
                file.Write(result);
                file.Close();
            }
            reader.Close();
            response.Close();
        }
        private void ListFiles()
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FTPSERVER);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                List<string> directories = new List<string>();

                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    fileList.Items.Add(line);
                    line = streamReader.ReadLine();
                }

                streamReader.Close();
                response.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Server not found");
            }
        }



        //Reset Method
        private void cleardata()
        {
            ShowareaList.Items.Clear();
            triangleIndice.Clear();
            positionPoint.Clear();
            distances.Clear();
            angles.Clear();
            heights.Clear();

            distanceData.Clear();
            angleData.Clear();
            heightData.Clear();
            angleList.Clear();
            heightList.Clear();

            coordinateX.Clear();
            coordinateY.Clear();
            coordinateZ.Clear();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            slider1_Copy.Value = 0;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            slider1.Value = 0;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            zoom.Value = 1000;
        }



        //Control
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Zoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            getZoomValue();
        }
        private void getZoomValue()
        {
            double zoomVal = zoom.Value;
            zoomValue.Text = (1 / (zoomVal / 1000)).ToString();
            scale.ScaleX = 1 / zoomVal;
            scale.ScaleY = 1 / zoomVal;
            scale.ScaleZ = 1 / zoomVal;
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

    class LookBackConverter : IValueConverter
    {

        #region IValueConverter Members



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            return (new Point3D(0, 0, 0)-(Point3D)value);
        }



        public object ConvertBack(

            object value, Type targetType,

            object parameter,

            System.Globalization.CultureInfo culture)

        {

            return null;

        }



        #endregion
    }

}
