using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnDimensions.Model
{
    public class RecColumn
    {
        #region Fields
        private Entity cadElement;
        private Point3d centerPoint;
        private List<Point3d> cornerPoints;
        private Point3d pointofGrid;
        private double a;
        private double b;
        #endregion

        #region Property
        public Entity CadElement
        {
            get { return cadElement; }
            set { cadElement = value; }
        }
        public Point3d CenterPoint
        {
            get { return centerPoint; }
            set { centerPoint = value; }
        }
        public List<Point3d> CornerPoints
        {
            get { return cornerPoints; }
            set { cornerPoints = value; }
        }
        public Point3d PointsofGrid
        {
            get { return pointofGrid; }
            set { pointofGrid = value; }
        }
        public double A
        {
            get { return a; }
            set { a = value; }
        }
        public double B
        {
            get { return b; }
            set { b = value; }
        }
        #endregion

        #region Constructor
        public RecColumn()
        {
            
        }
        #endregion

    }
}
