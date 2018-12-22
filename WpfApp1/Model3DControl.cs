using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WpfApp1
{
    public class Model3DControl : ReusableUIElement3D
    {
        Point3DCollection point3Ds1;
        Int32Collection triangleIndice1;
        protected override Model3D CreateElementModel()
        {
            Model3DGroup cubeModel = new Model3DGroup();
            cubeModel.Children.Add(CreateModel());
            return cubeModel;
        }
        public void CreateModel1(Point3DCollection point3Ds, Int32Collection triangleIndice)
        {
            point3Ds1 = point3Ds;
            triangleIndice1 = triangleIndice;
        }

        private GeometryModel3D CreateModel()
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush();
            solidColorBrush.Color = Colors.Bisque;
            return new GeometryModel3D
            {
                Geometry = new MeshGeometry3D
                {
                    Positions = new Point3DCollection
                    {
                        new Point3D(0, 0, 0),
                        new Point3D(2, 0, 0),
                        new Point3D(0, 2, 0),
                        new Point3D(2, 2, 0),
                        new Point3D(0, 0, 2),
                        new Point3D(2, 0, 2),
                        new Point3D(0, 2, 2),
                        new Point3D(2, 2, 2)
                    },

                    TextureCoordinates = new PointCollection
                    {
                        new Point(0, 1),
                        new Point(1, 1),
                        new Point(1, 0),
                        new Point(0, 0)
                    },

                    TriangleIndices = new Int32Collection
                    {
                        //6, 3, 7,
                        //5, 6, 7,
                        //7, 4, 5,
                        //1, 2, 6,
                        //6, 5, 1,
                        //2, 1, 3,
                        //1, 0, 3,
                        //1, 5, 4,
                        //4, 0, 1,
                        //7, 3, 0,
                        //0, 4, 7,


                        2, 3, 1,
                        2, 1, 0,
                        7, 1, 3,
                        7, 5, 1,
                        6, 5, 7,
                        6, 4, 5,
                        6, 2, 0,
                        2, 0, 4,
                        2, 7, 3,
                        2, 6, 7,
                        0, 1, 5,
                        0, 5, 4
                    }
                },
                //Material = new DiffuseMaterial(new VisualBrush(new MediaElement
                //{
                //    Source = new Uri("http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4", UriKind.RelativeOrAbsolute),
                //}))
                Material = new DiffuseMaterial((solidColorBrush))
                //Geometry = new MeshGeometry3D
                //{
                //    Positions = point3Ds1,

                //    TextureCoordinates = new PointCollection
                //    {
                //        new Point(0, 1),
                //        new Point(1, 1),
                //        new Point(1, 0),
                //        new Point(0, 0)
                //    },

                //    TriangleIndices = triangleIndice1
                //},
                //Material = new DiffuseMaterial((solidColorBrush))
            };
        }

        protected GeometryModel3D CreateModel(Point3DCollection point3Ds, Int32Collection triangleIndice)
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush();
            solidColorBrush.Color = Colors.Bisque;
            return new GeometryModel3D
            {
                Geometry = new MeshGeometry3D
                {
                    Positions = point3Ds,

                    TextureCoordinates = new PointCollection
                    {
                        new Point(0, 1),
                        new Point(1, 1),
                        new Point(1, 0),
                        new Point(0, 0)
                    },

                    TriangleIndices = triangleIndice
                },
                Material = new DiffuseMaterial((solidColorBrush))
            };
        }
    }
}
