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
using System.Windows.Threading;

namespace LiMESH
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
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
        //int[] dataDetail = new int[300000];
        Color ColorTest;
        bool LightStatus;
        //double pointx = 0.0, pointy = 0.0, pointz = 0.0;

        
        //List<int> n = new List<int>();

        

        private BackgroundWorker _bgWorker = new BackgroundWorker();
        private double _workerState;

        public double WorkerState
        {
            get { return _workerState; }
            set
            {
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
            System.Drawing.KnownColor t = new System.Drawing.KnownColor();
            foreach (System.Drawing.KnownColor kc in System.Enum.GetValues(t.GetType()))
            {
                System.Drawing.ColorConverter cc = new System.Drawing.ColorConverter();
                System.Drawing.Color c = System.Drawing.Color.FromName(kc.ToString());

                if (!c.IsSystemColor)
                    cbColors.Items.Add(c);
            }

            pgBar.Visibility = Visibility.Hidden;
            //DataContext = this;

            //_bgWorker.DoWork += (s, e) =>
            //{
            //    while (progress < 100)
            //    {
            //        Thread.Sleep(10);
            //        WorkerState = progress;
            //    }
            //    MessageBox.Show("Working Done!");
            //};
            //_bgWorker.RunWorkerAsync();


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
        private void Grid_MouseMove(object sender, MouseWheelEventArgs e)
        {
            camMain.Position = new Point3D((camMain.Position.X - e.Delta / 360D), (camMain.Position.Y - e.Delta / 360D), (camMain.Position.Z - e.Delta / 360D));
        }
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

        //private void redata()
        //{
        //    int n = h.Count;
        //    int currentPoint = 0;
        //    int lastPoint = 0;
        //    int dataPerH = distanceData.Count / n;
        //    for (int i = distanceData.Count-dataPerH-1; i < distanceData.Count; i++)
        //    {
        //        if (distanceData.ElementAt(i) <= 10)
        //        {
        //            double nextVal;
        //            int nextPoint = (i);
        //            do
        //            {
        //                nextPoint++;
        //            } while ((nextVal = distanceData.ElementAt(nextPoint % dataPerH)+ distanceData.Count - dataPerH - 1) <= 10);
        //            if (i == distanceData.Count - dataPerH - 1)
        //            {
        //                double befVal;
        //                int befPoint = 0;
        //                do
        //                {
        //                    befPoint--;
        //                } while ((befVal = distanceData.ElementAt((dataPerH + befPoint)+ distanceData.Count - dataPerH - 1)) <= 10);
        //                distanceData[distanceData.Count - dataPerH - 1] = (befVal + nextVal) / 2;
        //            }
        //            else
        //            {
        //                distanceData[i] = (distanceData.ElementAt(i - 1) + nextVal) / 2;
        //            }
        //        }
        //        //currentPoint++;
        //    }
        //    currentPoint = 0;
        //    for (int i = 0; i < h.Count-1; i++)
        //    {
        //        lastPoint += dataDetail[i - 1];
        //        if (i == 0)
        //        {
        //            lastPoint = 0;
        //            for (int j = 0; j < dataPerH; j++)
        //            {
        //                if (distanceData.ElementAt(currentPoint) <= 10)
        //                {
        //                    double nextVal;
        //                    int nextPoint = (currentPoint);
        //                    do
        //                    {
        //                        nextPoint++;
        //                    } while ((nextVal = distanceData.ElementAt(nextPoint % dataPerH)) <= 10);
        //                    if (currentPoint == 0)
        //                    {
        //                        double befVal;
        //                        int befPoint = 0;
        //                        do
        //                        {
        //                            befPoint--;
        //                        } while ((befVal = distanceData.ElementAt((dataPerH + (befPoint)) + lastPoint)) <= 10);
        //                        distanceData[0] = (befVal + nextVal) / 2;
        //                    }
        //                    else
        //                    {
        //                        distanceData[currentPoint] = (distanceData.ElementAt(currentPoint - 1) + nextVal) / 2;
        //                    }
        //                }
        //                currentPoint++;
        //            }
        //        }
        //        else
        //        {
        //            for (int j = 0; j < dataPerH; j++)
        //            {
        //                if (distanceData.ElementAt(currentPoint) == 1)
        //                {
        //                    double nextVal;
        //                    int nextPoint = 0;
        //                    do
        //                    {
        //                        nextPoint++;
        //                    } while ((nextVal = distanceData.ElementAt((nextPoint * dataPerH)+currentPoint)) <= 10);
        //                    double befVal;
        //                    int befPoint = 0;
        //                    do
        //                    {
        //                        befPoint--;
        //                    } while ((befVal = distanceData.ElementAt((befPoint * dataPerH) + currentPoint)) <= 10);
        //                    distanceData[currentPoint] = (befVal + nextVal) / 2;
        //                }
        //                currentPoint++;
        //            }
        //        }

        //    }
        //}

        private void datarestruct()
        {
            int ang = angleList.Count;
            int n = angleData.Count / ang;
            //MessageBox.Show("Data Layer : "+n);
            //for(int i = 0; i < ang; i++)
            //{
            //    if (distanceData.ElementAt(i) <= 1)
            //    {
            //        double nextVal;
            //        int nextPoint = (i);
            //        do
            //        {
            //            nextPoint++;
            //        } while ((nextVal = distanceData.ElementAt(nextPoint % ang) ) <= 10);
            //        if (i == 0)
            //        {
            //            double befVal;
            //            int befPoint = 0;
            //            do
            //            {
            //                befPoint--;
            //            } while ((befVal = distanceData.ElementAt((ang + befPoint))) <= 10);
            //            distanceData[0] = (befVal + nextVal) / 2;
            //        }
            //        else
            //        {
            //            distanceData[i] = (distanceData.ElementAt(i - 1) + nextVal) / 2;
            //        }
            //    }
            //}
            //for (int i = (angleData.Count- ang - 1); i < angleData.Count; i++)
            //{
            //    if (distanceData.ElementAt(i) <= 1)
            //    {
            //        double nextVal;
            //        int nextPoint = (i);
            //        do
            //        {
            //            nextPoint++;
            //        } while ((nextVal = distanceData.ElementAt((nextPoint % ang) + distanceData.Count - ang - 1)) <= 10);
            //        if (i == distanceData.Count - ang - 1)
            //        {
            //            double befVal;
            //            int befPoint = 0;
            //            do
            //            {
            //                befPoint--;
            //            } while ((befVal = distanceData.ElementAt((ang + befPoint) + distanceData.Count - ang - 1)) <= 10);
            //            distanceData[distanceData.Count - ang - 1] = (befVal + nextVal) / 2;
            //        }
            //        else
            //        {
            //            distanceData[i] = (distanceData.ElementAt(i - 1) + nextVal) / 2;
            //        }
            //    }
            //}
            //for (int i= 1; i < n - 1; i++)
            //{
            //    for(int  p = 0; p < ang; p++)
            //    {
            //        int currentpoint = (i * ang) + p;
            //        if (distanceData.ElementAt(currentpoint) == 1)
            //        {
            //            int upperpoint = ((i + 1) * ang) + p;
            //            int tmp = i;
            //            while (distanceData[upperpoint] <= 10)
            //            {
            //                tmp++;
            //                upperpoint = ((tmp + 1) * ang) + p;
            //            }
            //            int lowerpoint = ((i - 1) * ang) + p;
            //            distanceData[currentpoint] = (distanceData[upperpoint] + distanceData[lowerpoint]) / 2;
            //        }

            //        int pos = p + (i * ang);
            //        if (distanceData.ElementAt(pos) == 0)
            //        {
            //            double nextVal;
            //            int nextPoint = (pos);
            //            do
            //            {
            //                nextPoint++;
            //            } while ((nextVal = distanceData.ElementAt((nextPoint % ang) + distanceData.Count - ang - 1)) <= 10);
            //            if (i == distanceData.Count - ang - 1)
            //            {
            //                double befVal;
            //                int befPoint = 0;
            //                do
            //                {
            //                    befPoint--;
            //                } while ((befVal = distanceData.ElementAt((ang + befPoint) + distanceData.Count - ang - 1)) <= 10);
            //                distanceData[pos] = (befVal + nextVal) / 2;
            //            }
            //            else
            //            {
            //                distanceData[pos] = (distanceData.ElementAt(pos - 1) + nextVal) / 2;
            //            }
            //        }
            //    }

            //}
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
                        } while ((nextVal = distanceData.ElementAt((nextPoint % ang)+lastPoint)) <= 10);
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

        BackgroundWorker worker = new BackgroundWorker();
        List<double> angleList = new List<double>(); 
        List<double> heightList = new List<double>();
        List<double> distanceData = new List<double>();
        List<double> angleData = new List<double>();
        List<double> heightData = new List<double>();

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
                    angles.Add(Double.Parse(token[1]));
                    heights.Add(Double.Parse(token[2]));
                }
                MessageBox.Show("Load data from file completed!\nPlease wait for a while...");
                collect();
                cal();
                triangleInDices();
                MessageBox.Show("Calculate data completed!"); 
                pgBar.Visibility = Visibility.Hidden;
                Light.SelectedIndex = 1;
                cbColors.SelectedIndex = 72;
                type.SelectedIndex = 1;
                type.IsEnabled = true;
                ColorBoxEnable();
                bgIMG.Visibility = Visibility.Hidden;
                sr.Close();
            }
        } 
        private void collect()
        {
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
                ShowareaList.Items.Add("No." + ((i % angleList.Count)+1) + ": Dis :" + distanceData.ElementAt(i) + ": Angle :" + angleData.ElementAt(i)+": Height :"+ heightData.ElementAt(i));
            }
        }
        private void fill()
        {
            int n = heightList.Count;
            int ang = angleList.Count;
            int total = n * ang;
            for (int i = 0; i < total; i++)
            {
                distanceData.Add(1);
                angleData.Add(angleList.ElementAt(i%ang));
                heightData.Add(heightList.ElementAt(i / ang));
            }
            int totals = distances.Count;
            int pos = 0;
            for (int i = 0; i < totals; i++)
            {
                while (angles.ElementAt(i) != angleList.ElementAt(pos % angleList.Count))
                {
                    pos++;
                }
                distanceData[pos] = distances.ElementAt(i);
            }
        }
        private void triangleInDices()
        {
            //Create Triangle in dice
            int n = heightList.Count;
            int angle = angleList.Count;
            for(int i = 0; i < n - 1; i++)
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
        private void sendata(double coX, double coY, double coZ)
        {
            coordinateX.Add(coX);
            coordinateY.Add(coY);
            coordinateZ.Add(coZ);
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
            if (ShowareaList.Items.Count!=0)
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
                myDirectionalLight.Color = Color.FromRgb(66,66,66);
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

        private void cbColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Drawing.Color color = (System.Drawing.Color)cbColors.SelectedItem;
            ColorTest = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
            if (mode) render();
            else CreatePointCloud(positionPoint);
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

        private void export(object sender, RoutedEventArgs e)
        {
            export();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //pgBar.Value = 0; //Initializing progress value to 0  
            //if (worker == null)
            //{
            //    worker = new BackgroundWorker();
            //    worker.DoWork += worker_DoWork;
            //    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            //    worker.ProgressChanged += worker_ProgressChanged;
            //    worker.WorkerReportsProgress = true;
            //    worker.WorkerSupportsCancellation = true;
            //}
            //if (worker.IsBusy != true)
            //{
            //    // Start the asynchronous operation.  
            //    worker.RunWorkerAsync();
            //}


            //richTextBox1.AppendText(Get_Data_From_FTP_Server_File());
            
            lb.Visibility = Visibility.Hidden;
            lb1.Visibility = Visibility.Hidden;
        }

        private String Get_Data_From_FTP_Server_File()
        {
            //used to display data into rich text.box
            String result = String.Empty;


            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://speedtest.tele2.net/100KB.zip");
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
            using (StreamWriter file = File.CreateText("states.txt"))
            {
                file.WriteLine(result);
                file.Close();
            }
            //close readers. 
            reader.Close();
            response.Close();
            //return data from file. 
            return result;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pgBar.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Done!");
            try
            {
                worker.CancelAsync();
                worker = null;
            }
            catch { }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 1000000; i++)
            {
                worker.ReportProgress((i / 1000000) * 100);
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


        public void Message(string data)
        {
            //listBox1.Items.Add(data);
            //richTextBox1.AppendText(data + "\n");
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamReader sr = new StreamReader(textBox1.Text);

                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(new IPEndPoint(IPAddress.Parse(ip.Text), 8085));

                byte[] buffer = new byte[1500];
                long bytesSent = 0;

                while (bytesSent < sr.BaseStream.Length)
                {
                    int bytesRead = sr.BaseStream.Read(buffer, 0, 1500);
                    tcpClient.GetStream().Write(buffer, 0, bytesRead);
                    Message(bytesRead + " bytes sent.");

                    bytesSent += bytesRead;
                }

                tcpClient.Close();

                Message("finished");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.ShowDialog();
            textBox1.Text = op.FileName;
        }

        private void Icon_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
