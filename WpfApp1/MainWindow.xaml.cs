using Microsoft.Win32;
using SimpleTCP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace LiMESH
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        static int meshSelected = 0;
        static int pointCloudSelected = 1;
        static int notSelected = 2;

        List<double> coordinateX = new List<double>();
        List<double> coordinateY = new List<double>();
        List<double> coordinateZ = new List<double>();
        List<double> distances = new List<double>();
        List<double> angles = new List<double>();
        List<double> heights = new List<double>();

        Point3DCollection positionPoint = new Point3DCollection();
        Int32Collection triangleIndice = new Int32Collection();

        AxisAngleRotation3D rotate1 = new AxisAngleRotation3D();
        AxisAngleRotation3D rotate2 = new AxisAngleRotation3D();
        AxisAngleRotation3D rotate3 = new AxisAngleRotation3D();
        ScaleTransform3D scale = new ScaleTransform3D();
        PerspectiveCamera myPCamera = new PerspectiveCamera();
        int[] dataDetail = new int[30000];
        Color ColorTest;
        bool LightStatus;
        double pointx = 0.0, pointy = 0.0, pointz = 0.0;

        List<double> h = new List<double>();
        List<double> aPh = new List<double>();

        private BackgroundWorker _bgWorker = new BackgroundWorker();
        private double _workerState;

        public double WorkerState
        {
            get { return _workerState; }
            set {
                _workerState = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("workerState"));
                }
            }
        }

        #region INotifyPropertyChanged Member
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

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
            type.IsEnabled = false;
            Light.IsEnabled = false;
            cbColors.IsEnabled = false;
            showArea.AppendText("Welcome to Program");
            showArea.IsReadOnly = true;
            System.Drawing.KnownColor t = new System.Drawing.KnownColor();
            foreach (System.Drawing.KnownColor kc in System.Enum.GetValues(t.GetType()))
            {
                System.Drawing.ColorConverter cc = new System.Drawing.ColorConverter();
                System.Drawing.Color c = System.Drawing.Color.FromName(kc.ToString());

                if (!c.IsSystemColor)
                    cbColors.Items.Add(c);
            }

            DataContext = this;

            _bgWorker.DoWork += (s, e) =>
            {
                for (double i = 0; i <= 100; i+=0.01)
                {
                    Thread.Sleep(10);
                    WorkerState = i;
                }
                MessageBox.Show("Working Done!");
            };
            _bgWorker.RunWorkerAsync();

            //dirLightMain.Direction = new Vector3D(pointx,pointy,pointz);
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
        private void cleardata()
        {
            showArea.Document.Blocks.Clear();
            showArea.Focus();
        }
        private bool checkLayer(double x)
        {

            if (h.Count == 0)
            {
                return true;
            }
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

        private void cal()
        {
            //BackgroundWorker worker = new BackgroundWorker();
            //worker.WorkerReportsProgress = true;
            //worker.DoWork += worker_DoWork;
            //worker.ProgressChanged += worker_ProgressChanged;

            //worker.RunWorkerAsync();
            
            int max = distances.Count;
            for (int i = 0; i < max; i++)
            {
                //Thread.Sleep(100);
                //pgBar.Value = ((i + 1) / max) * 100;
                showArea.AppendText(i +
                        "\ndistances : " + distances.ElementAt(i) +
                        "\nangles : " + angles.ElementAt(i) +
                        "\nheights : " + heights.ElementAt(i) +
                        "\nCOX : " + distances.ElementAt(i) * (Math.Sin(toRadians(angles.ElementAt(i)))) +
                        "\nCOY : " + distances.ElementAt(i) * (Math.Cos(toRadians(angles.ElementAt(i)))) +
                        "\n");

                sendata(distances.ElementAt(i) * (Math.Sin(toRadians(angles.ElementAt(i)))),
                    distances.ElementAt(i) * (Math.Cos(toRadians(angles.ElementAt(i)))),
                    heights.ElementAt(i) * 10,
                    i
                    );
                point(distances.ElementAt(i) * (Math.Sin(toRadians(angles.ElementAt(i)))),
                    distances.ElementAt(i) * (Math.Cos(toRadians(angles.ElementAt(i)))),
                    heights.ElementAt(i) * 10
                    );
            }
            //pgBar.IsIndeterminate = false;
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
                String data;
                int count = 0;
                //pgBar.IsIndeterminate = true;
                while ((data = sr.ReadLine()) != null)
                {
                    String[] token = data.Split(',');
                    distances.Add(Double.Parse(token[0]));
                    angles.Add(Double.Parse(token[1]));
                    heights.Add(Double.Parse(token[2]));
                    if (checkLayer(Double.Parse(token[2])))
                    {
                        dataDetail[h.Count] = 0;
                        h.Add(Double.Parse(token[2]));
                        aPh.Clear();
                    }
                    if (checkAngle(Double.Parse(token[1])))
                    {
                        dataDetail[h.Count - 1]++;
                        aPh.Add(Double.Parse(token[1]));
                    }
                    //showArea.AppendText(count +
                    //    "\ndistances : " + distances.ElementAt(count) +
                    //    "\nangles : " + angles.ElementAt(count) +
                    //    "\nheights : " + heights.ElementAt(count) +
                    //    "\nCOX : " + distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))) +
                    //    "\nCOY : " + distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))) +
                    //    "\n");

                    //sendata(distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))),
                    //    distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))),
                    //    heights.ElementAt(count) * 10,
                    //    count
                    //    );
                    //point(distances.ElementAt(count) * (Math.Sin(toRadians(angles.ElementAt(count)))),
                    //    distances.ElementAt(count) * (Math.Cos(toRadians(angles.ElementAt(count)))),
                    //    heights.ElementAt(count) * 10
                    //    );
                    count++;
                }
                //pgBar.IsIndeterminate = false;
                MessageBox.Show("Load data from file completed!");
                cal();
                //pgBar.IsIndeterminate = false;
                triangleInDices();
                MessageBox.Show("Calculate data completed!");
                Light.SelectedIndex = 0;
                cbColors.SelectedIndex = 114;
                type.SelectedIndex = 0;
                type.IsEnabled = true;
                ColorBoxEnable();
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
        private void point(double x, double y, double z)
        {
            //return (new Point3D(x, z, y));
            positionPoint.Add(new Point3D(x, z, y));
        }
        private void sendata(double coX, double coY, double coZ, int counter)
        {
            coordinateX.Add(coX);
            coordinateY.Add(coY);
            coordinateZ.Add(coZ);
            //showArea2.AppendText(counter +
            //    "\nX : " + coordinateX.ElementAt(counter) +
            //    "\nY : " + coordinateY.ElementAt(counter) +
            //    "\nZ : " + coordinateZ.ElementAt(counter) + "\n");

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
            if (richText != "")
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
            else
            {
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
            MessageBox.Show("No action perform Now!");
        }

        private void Zoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            getZoomValue();
        }

        private void getZoomValue()
        {
            double zoomVal = zoom.Value;
            zoomValue.Text = zoomVal.ToString();
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

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //NewTabCommand = new ActionCommand(p => NewTab());
            
        }

        public ICommand NewTabCommand { get; }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            //for (int i = 0; i < IPHost.AddressList.Length; i++)
            //{
            //    ipList.Items.Add(IPHost.AddressList[i].ToString());
            //}
            //ipList.SelectedIndex = 0;

        }

        //private void CreateAltitudeMap()
        //{
        //    // Calculate the function's value over the area.
        //    const int xwidth = 512;
        //    const int zwidth = 512;
        //    const double dx = (xmax - xmin) / xwidth;
        //    const double dz = (zmax - zmin) / zwidth;
        //    double[,] values = new double[xwidth, zwidth];
        //    for (int ix = 0; ix < xwidth; ix++)
        //    {
        //        double x = xmin + ix * dx;
        //        for (int iz = 0; iz < zwidth; iz++)
        //        {
        //            double z = zmin + iz * dz;
        //            values[ix, iz] = F(x, z);
        //        }
        //    }

        //    // Get the upper and lower bounds on the values.
        //    var get_values =
        //        from double value in values
        //        select value;
        //    double ymin = get_values.Min();
        //    double ymax = get_values.Max();

        //    // Make the BitmapPixelMaker.
        //    BitmapPixelMaker bm_maker =
        //        new BitmapPixelMaker(xwidth, zwidth);

        //    // Set the pixel colors.
        //    for (int ix = 0; ix < xwidth; ix++)
        //    {
        //        for (int iz = 0; iz < zwidth; iz++)
        //        {
        //            byte red, green, blue;
        //            MapRainbowColor(values[ix, iz], ymin, ymax,
        //                out red, out green, out blue);
        //            bm_maker.SetPixel(ix, iz, red, green, blue, 255);
        //        }
        //    }

        //    // Convert the BitmapPixelMaker into a WriteableBitmap.
        //    WriteableBitmap wbitmap = bm_maker.MakeBitmap(96, 96);

        //    // Save the bitmap into a file.
        //    wbitmap.Save("Texture.png");
        //}

        private void IpList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //showArea2.Document.Blocks.Clear();
            //showArea2.AppendText("My IP address is " + ipList.SelectedItem);
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

        private bool mode;
        private void render()
        {
            zoom.IsEnabled = true;
            slider1.IsEnabled = true;
            slider1_Copy.IsEnabled = true;
            bgIMG.Visibility= Visibility.Hidden;
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
                DirectionalLight myDirectionalLight = new DirectionalLight();
                myDirectionalLight.Color = Colors.White;
                myDirectionalLight.Direction = new Vector3D(-1, -1, -1);
                myModel3DGroup.Children.Add(myDirectionalLight);
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
                AddCubeToMesh(myMeshGeometry3D, points[i], 0.5);
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void XSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //pointx = xSlide.Value * -1;
            //XValue.Text = xSlide.Value.ToString();
            //render();
        }

        private void cbColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Drawing.Color color = (System.Drawing.Color)cbColors.SelectedItem;
            ColorTest = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
            if (mode) render();
            else CreatePointCloud(positionPoint);
        }

        private void YSlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //pointy = ySlide.Value * -1;
            //YValue.Text = ySlide.Value.ToString();
            //render();
        }





        public void listenerThread()
        {
            TcpListener tcpListener = new TcpListener(8080);
            tcpListener.Start();
            while (true)
            {
                Socket handlerSocket = tcpListener.AcceptSocket();
                if (handlerSocket.Connected)
                {
                    //Control.CheckForIllegalCrossThreadCalls = false;
                    
                    lbConnections.Items.Add(handlerSocket.RemoteEndPoint.ToString() + " connected.");
                    lock (this)
                    {
                        nSockets.Add(handlerSocket);
                    }
                    ThreadStart thdstHandler = new
                    ThreadStart(handlerThread);
                    Thread thdHandler = new Thread(thdstHandler);
                    thdHandler.Start();
                }
            }
        }

        private void handlerThread()
        {
            Socket handlerSocket = (Socket)nSockets[nSockets.Count - 1];
            NetworkStream networkStream = new NetworkStream(handlerSocket);
            int thisRead = 0;
            int blockSize = 1024;
            Byte[] dataByte = new Byte[blockSize];
            lock (this)
            {
                // Only one process can access
                // the same file at any given time
                Stream fileStream = File.OpenWrite("c:\\my documents\\SubmittedFile.txt");
                while (true)
                {
                    thisRead = networkStream.Read(dataByte, 0, blockSize);
                    fileStream.Write(dataByte, 0, thisRead);
                    if (thisRead == 0) break;
                }
                fileStream.Close();
            }
            lbConnections.Items.Add("File Written");
            handlerSocket = null;

        }

        private ArrayList nSockets;


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //textbox.AppendText(GetIP());

            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            myIP.Text = "My IP: " + IPHost.AddressList[1].ToString();
            nSockets = new ArrayList();
            Thread thdListener = new Thread(new ThreadStart(listenerThread));
            thdListener.Start();


            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "UAV Data Files (*.csv)|*.csv|All files (*.*)|*.*";
            //openFileDialog.FilterIndex = 1;
            //string path = "";
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    path = openFileDialog.FileName;
            //    MessageBox.Show("Name : "+path);

            //    Stream fileStream = File.OpenRead(path);
            //    // Alocate memory space for the file
            //    byte[] fileBuffer = new byte[fileStream.Length];
            //    fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
            //    // Open a TCP/IP Connection and send the data
            //    TcpClient clientSocket = new TcpClient(ip.Text, 8080);
            //    NetworkStream networkStream = clientSocket.GetStream();
            //    networkStream.Write(fileBuffer, 0, fileBuffer.GetLength(0));
            //    networkStream.Close();

            //}
            ipGet();           
        }

        private void Server_DataReceive(object sender, Message e)
        {

        }

        private void ConnectToServer()
        {
            //try
            //{
            //    //hostAddr = Dns.GetHostEntry(AddressBox.Text);
            //    IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            //    for (int i = 0; i < IPHost.AddressList.Length; i++)
            //    {
            //        ipList.Items.Add(IPHost.AddressList[i].ToString());
            //    }
            //    ipList.SelectedIndex = 0;
            //    //Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //    //Server.Connect(hostAddr.AddressList[0].ToString(), 5005);

            //    //StateObject state = new StateObject();
            //    //state.client = Server;

            //    //Server.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            //    //StatusLabel.Text = "Connected To Server @ " + AddressBox.Text;

            //}
            //catch (Exception)
            //{
            //    //StatusLabel.Text = "Could Not Connect To " + AddressBox.Text;
            //}
        }

        private void DisconnectFromServer()
        {
            //Server.Close();
        }


        //public void Ping_all()
        //{

        //    string gate_ip = NetworkGateway();

        //    //Extracting and pinging all other ip's.
        //    string[] array = gate_ip.Split('.');

        //    for (int i = 2; i <= 255; i++)
        //    {

        //        string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

        //        //time in milliseconds           
        //        Ping(ping_var, 4, 4000);

        //    }

        //}
        //public void Ping(string host, int attempts, int timeout)
        //{
        //    for (int i = 0; i < attempts; i++)
        //    {
        //        new Thread(delegate ()
        //        {
        //            try
        //            {
        //                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        //                ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
        //                ping.SendAsync(host, timeout, host);
        //            }
        //            catch
        //            {
        //                // Do nothing and let it try again until the attempts are exausted.
        //                // Exceptions are thrown for normal ping failurs like address lookup
        //                // failed.  For this reason we are supressing errors.
        //            }
        //        }).Start();
        //    }
        //}

        //private void PingCompleted(object sender, PingCompletedEventArgs e)
        //{
        //    string ip = (string)e.UserState;
        //    if (e.Reply != null && e.Reply.Status == IPStatus.Success)
        //    {
        //        string hostname = GetHostName(ip);
        //        string macaddres = GetMacAddress(ip);
        //        string[] arr = new string[3];

        //        //store all three parameters to be shown on ListView
        //        arr[0] = ip;
        //        arr[1] = hostname;
        //        arr[2] = macaddres;

        //        // Logic for Ping Reply Success
        //        ListViewItem item;
        //        if (this.InvokeRequired)
        //        {

        //            this.Invoke(new Action(() =>
        //            {

        //                item = new ListViewItem(arr);

        //                lstLocal.Items.Add(item);


        //            }));
        //        }


        //    }
        //    else
        //    {
        //        // MessageBox.Show(e.Reply.Status.ToString());
        //    }
        //}

        //public string GetHostName(string ipAddress)
        //{
        //    try
        //    {
        //        IPHostEntry entry = Dns.GetHostEntry(ipAddress);
        //        if (entry != null)
        //        {
        //            return entry.HostName;
        //        }
        //    }
        //    catch (SocketException)
        //    {
        //        // MessageBox.Show(e.Message.ToString());
        //    }

        //    return null;
        //}

            

        //Get MAC address
        //public string GetMacAddress(string ipAddress)
        //{
        //    string macAddress = string.Empty;
        //    System.Diagnostics.Process Process = new System.Diagnostics.Process();
        //    Process.StartInfo.FileName = "arp";
        //    Process.StartInfo.Arguments = "-a " + ipAddress;
        //    Process.StartInfo.UseShellExecute = false;
        //    Process.StartInfo.RedirectStandardOutput = true;
        //    Process.StartInfo.CreateNoWindow = true;
        //    Process.Start();
        //    string strOutput = Process.StandardOutput.ReadToEnd();
        //    string pattern = @"(?<ip>([0-9]{1,3}\.?){4})\s*(?<mac>([a-f0-9]{2}-?){6})";

        //    foreach(Match m in Regex.Matches(strOutput, pattern, RegexOptions.IgnoreCase))
        //    {
        //        ipList.Items.Add(new MacIpPair() { });
        //    }
        //    //string[] substrings = strOutput.Split('-');
        //    //if (substrings.Length >= 8)
        //    //{
        //    //    macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
        //    //             + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
        //    //             + "-" + substrings[7] + "-"
        //    //             + substrings[8].Substring(0, 2);
        //    //    ipList.Items.Add(IPHost.AddressList[i].ToString());
        //    //    return macAddress;
        //    //}

        //    //else
        //    //{
        //    //    return "OWN Machine";
        //    //}
        //}

        private string GetIP()
        {
            string host = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(host);
            foreach (IPAddress ipadd in ipEntry.AddressList)
            {
                //textblox.Text += ipadd.AddressFamily.ToString()+"\n";
                if (ipadd.AddressFamily.ToString() == "InterNetworkV6")
                {
                    return ipadd.ToString();
                }
            }
            return ".";
        }

        private void ipGet()
        {
            //string strHostName = string.Empty;

            //textblox.Text = "";

            //// Getting Ip address of local machine...
            //// First get the host name of local machine.
            //IPGlobalProperties network = IPGlobalProperties.GetIPGlobalProperties();
            //TcpConnectionInformation[] connections = network.GetActiveTcpConnections();
            //for(int i = 0;i<connections.Length;i++)
            //{
            //    textblox.Text += connections[i].ToString() + "\n";
            //}
            ////strHostName = Dns.GetHostName();

            ////// Then using host name, get the IP address list..

            ////IPHostEntry ipEntry = Dns.GetHostByName(strHostName);
            ////IPAddress[] iparrAddr = ipEntry.AddressList;

            ////if (iparrAddr.Length > 0)
            ////{
            ////    for (int intLoop = 0; intLoop < iparrAddr.Length; intLoop++)
            ////        textblox.Text += iparrAddr[intLoop].ToString()+"\n";
            ////}
            ///
            //string ipAddr = ip.Text;
            //System.Diagnostics.Process process = new System.Diagnostics.Process();
            //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = "ping "+ipAddr;
            //process.StartInfo = startInfo;
            //process.Start();

            //string ipAddr = ip.Text;
            //if(ipAddr.Equals(""))
            //{
            //    ipAddr = "localhost";
            //}
            //Ping ping = new Ping();
            //PingReply pingresult = ping.Send(ipAddr);
            ////if (pingresult.Status.ToString() == "Success")
            ////{
            ////    MessageBox.Show("Ping to "+ipAddr+":"+pingresult.Status.ToString());
            ////}
            //MessageBox.Show("Ping to " + ipAddr + ":" + pingresult.Status.ToString());
            ////textbox.Text = ipAddr;
        }

        private void ZSlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //pointz = zSlide.Value * -1;
            //ZValue.Text = zSlide.Value.ToString();
            //render();
        }

        private void export(object sender, RoutedEventArgs e)
        {
            export();
        }

        private void PgBar_ValueChanged(object sender, ProgressChangedEventArgs e)
        {
            pgBar.Value = e.ProgressPercentage;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void redata()
        {
            for (int i = 0; i < distances.Count; i++)
            {

            }
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

        //public void ReceiveFile(string fileName)
        //{
        //    // Get the object used to communicate with the server.
        //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://www.contoso.com/test.htm");
        //    request.Method = WebRequestMethods.Ftp.DownloadFile;

        //    // This example assumes the FTP site uses anonymous logon.
        //    request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");

        //    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

        //    Stream responseStream = response.GetResponseStream();
        //    StreamReader reader = new StreamReader(responseStream);
        //    Console.WriteLine(reader.ReadToEnd());

        //    Console.WriteLine($"Download Complete, status {response.StatusDescription}");

        //    reader.Close();
        //    response.Close();
        //}

        //public void SendFile(string fileName)
        //{
        //    // Get the object used to communicate with the server.
        //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://www.contoso.com/test.htm");
        //    request.Method = WebRequestMethods.Ftp.UploadFile;

        //    // This example assumes the FTP site uses anonymous logon.
        //    request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");

        //    // Copy the contents of the file to the request stream.
        //    byte[] fileContents;
        //    using (StreamReader sourceStream = new StreamReader("testfile.txt"))
        //    {
        //        fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
        //    }

        //    request.ContentLength = fileContents.Length;

        //    using (Stream requestStream = request.GetRequestStream())
        //    {
        //        requestStream.Write(fileContents, 0, fileContents.Length);
        //    }

        //    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        //    {
        //        Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
        //    }
        //}


        //public void SendoFile(string fileName)
        //{
        //    try
        //    {
        //        string IpAddressString = ip.Text;
        //        int portnumber = 21;
        //        IPEndPoint ipEnd_client = new IPEndPoint(IPAddress.Parse(IpAddressString), portnumber);
        //        Socket clientSock_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        //        string filePath = "";

        //        fileName = fileName.Replace("\\", "/");
        //        Console.WriteLine(fileName);

        //        while (fileName.IndexOf("/") > -1)
        //        {
        //            filePath += fileName.Substring(0, fileName.IndexOf("/") + 1);
        //            fileName = fileName.Substring(fileName.IndexOf("/") + 1);
        //        }

        //        byte[] fileNameByte = Encoding.UTF8.GetBytes(fileName);
        //        if (fileNameByte.Length > 5000 * 1024)
        //        {
        //            Console.WriteLine("File size is more than 5Mb, please try with small file.");
        //            return;
        //        }

        //        Console.WriteLine("Buffering ...");
        //        string fullPath = filePath + fileName;

        //        byte[] fileData = File.ReadAllBytes(fullPath);
        //        byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
        //        byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);


        //        fileNameLen.CopyTo(clientData, 0);
        //        fileNameByte.CopyTo(clientData, 4);
        //        fileData.CopyTo(clientData, 4 + fileNameByte.Length);

        //        Console.WriteLine("Connection to server...");
        //        clientSock_client.Connect(ipEnd_client);

        //        Console.WriteLine("File sending...");
        //        clientSock_client.Send(clientData, 0, clientData.Length, 0);

        //        Console.WriteLine("Disconnecting...");
        //        clientSock_client.Close();
        //        Console.WriteLine("File [" + fullPath + "] transferred.");
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message == "No connection could be made because the target machine actively refused it")
        //            Console.WriteLine("File Sending fail. Because server not running.");
        //        else
        //            Console.WriteLine("File Sending fail. " + ex.Message);
        //        return;
        //    }
        //    //connected = true;
        //    return;
        //}
    }
}
