using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ColumnDimensions.Command;
using ColumnDimensions.Model;
using ColumnDimensions.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace ColumnDimensions.ViewMode
{
    public class ColDim
    {
        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Fields
        private List<string> cadLayers = Cad.CadLayer;
        private List<string> cadDimensionStyles = Cad.CadDimStyle;
        private UserInterface1 viewObejct;
        #endregion

        #region Property
        public List<string> CadLAyers
        {
            get { return cadLayers; }
            set { cadLayers = value; OnPropertyChanged(); }
        }
        public List<string> CadDimensionStyles
        {
            get { return cadDimensionStyles; }
            set { cadDimensionStyles = value; OnPropertyChanged(); }
        }
        public string SelectedLayerColumn { get; set; }
        public string SelectedLayerAxis { get; set; }
        public string SelectedDimStyle { get; set; }
        public string SelectedDimLayer { get; set; }
        public double DistanceTo { set; get; }
        public List<RecColumn> ElementList { get; set; }
        public Point3dCollection AxisPoints { get; set; }
        public List<Point3d> PointsList { get; set; }
        public List<Point3d> AxisPointsIntersect { get; set; }
        public MyCommand Close { get; set; }
        public MyCommand Run { get; set; }
        #endregion


        #region Constructor
        public ColDim(UserInterface1 windowObject)
        {
            viewObejct = windowObject;
            Close = new MyCommand(ExcuteClose, CanExcuteClose);
            Run = new MyCommand(ExcuteRun, CanExcuteRun);
        }
        #endregion

        public void ColumnDimensions()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;

            #region Variables
            double area;
            double prem;
            double a;
            double b;
            Point3d pmax;
            Point3d pmin;
            Point3d pc;
            double angleH;
            double angleV;
            ElementList = new List<RecColumn>();
            AxisPoints = new Point3dCollection();
            List<Entity> listofAxis = new List<Entity>();
            PointsList = new List<Point3d>();
            AxisPointsIntersect = new List<Point3d>();
            List<double> length;
            int signX = 1;
            int signY = 1;
            #endregion

            using (Transaction tran = db.TransactionManager.StartTransaction())
            {
                try
                {
                    #region Get The Elements From The Selection
                    TypedValue[] tv = new TypedValue[2];
                    tv.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE,LINE"), 0);
                    tv.SetValue(new TypedValue((int)DxfCode.LayerName, $"{SelectedLayerColumn},{SelectedLayerAxis}"), 1);
                    SelectionFilter filter = new SelectionFilter(tv);
                    PromptSelectionResult prompt = editor.GetSelection(filter);
                    SelectionSet selectionSet = prompt.Value;
                    if (prompt.Status != PromptStatus.OK)
                    {
                        return;
                    }
                    #endregion

                    #region Supply Element with Data
                    foreach (SelectedObject item in selectionSet)
                    {
                        Entity ent = tran.GetObject(item.ObjectId, OpenMode.ForRead) as Entity;
                        if ((ent.Drawable.GetType().Name == "Polyline" || ent.Drawable.GetType().Name == "Line") && ent.Layer == SelectedLayerAxis)
                        {
                            listofAxis.Add(ent);
                        }
                    }
                    foreach (SelectedObject item in selectionSet)
                    {
                        Entity ent = tran.GetObject(item.ObjectId, OpenMode.ForRead) as Entity;
                        if ((ent.Drawable.GetType().Name == "Polyline" || ent.Drawable.GetType().Name == "Line") && ent.Layer == SelectedLayerAxis)
                        {
                            for (int i = 1; i < listofAxis.Count; i++)
                            {
                                ent.IntersectWith(listofAxis[i], Intersect.OnBothOperands, AxisPoints, IntPtr.Zero, IntPtr.Zero);
                            }
                        }
                    }
                    foreach (Point3d item in AxisPoints)
                    {
                        PointsList.Add(item);
                    }

                    AxisPointsIntersect = PointsList.Distinct<Point3d>().ToList();

                    foreach (SelectedObject item in selectionSet)
                    {
                        Entity ent = tran.GetObject(item.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent.Drawable.GetType().Name == "Polyline" && ent.Layer == SelectedLayerColumn)
                        {
                            RecColumn element = new RecColumn();
                            element.CornerPoints = new List<Point3d>();
                            area = (ent as Polyline).Area;
                            prem = (ent as Polyline).Length;
                            a = 0.5 * (0.5 * prem - Math.Sqrt(Math.Pow((0.5 * prem), 2) - 4 * area));
                            b = 0.5 * (0.5 * prem + Math.Sqrt(Math.Pow((0.5 * prem), 2) - 4 * area));
                            pmax = (ent as Polyline).Drawable.Bounds.Value.MaxPoint;
                            pmin = (ent as Polyline).Drawable.Bounds.Value.MinPoint;
                            pc = new Point3d((pmax.X + pmin.X) / 2, (pmax.Y + pmin.Y) / 2, 0);
                            length = new List<double>();
                            foreach (var point in AxisPointsIntersect)
                            {
                                length.Add(Math.Sqrt(Math.Pow(pc.X - point.X, 2) + Math.Pow(pc.Y - point.Y, 2)));
                            }
                            double minLength = length.Min();
                            int index = length.IndexOf(minLength);
                            element.CadElement = ent;
                            element.CenterPoint = pc;
                            element.CornerPoints.Add((ent as Polyline).GetPoint3dAt(0));
                            element.CornerPoints.Add((ent as Polyline).GetPoint3dAt(1));
                            element.CornerPoints.Add((ent as Polyline).GetPoint3dAt(2));
                            element.CornerPoints.Add((ent as Polyline).GetPoint3dAt(3));
                            element.PointsofGrid = AxisPointsIntersect[index];
                            element.A = a;
                            element.B = b;
                            ElementList.Add(element);
                        }
                    }
                    #endregion

                    #region Get The Dimension Style From Name
                    DimStyleTableRecord dimStyleRecord = null;
                    DimStyleTable dmt = tran.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
                    foreach (var item in dmt)
                    {
                        DimStyleTableRecord dmtr = tran.GetObject(item, OpenMode.ForRead) as DimStyleTableRecord;
                        if (dmtr.Name == SelectedDimStyle)
                        {
                            dimStyleRecord = dmtr;
                        }

                    }
                    #endregion

                    #region Draw Dimensions
                    ObjectId bt = db.CurrentSpaceId;
                    BlockTableRecord btr = tran.GetObject(bt, OpenMode.ForWrite) as BlockTableRecord;
                    using (Transaction tran2 = db.TransactionManager.StartTransaction())
                    {
                        foreach (var item in ElementList)
                        {
                            #region Define The Dimensions
                            RotatedDimension dimH = new RotatedDimension();
                            RotatedDimension dimV = new RotatedDimension();
                            RotatedDimension dimHAll = new RotatedDimension();
                            RotatedDimension dimVAll = new RotatedDimension();
                            #endregion

                            #region Get The Index of Point Of Corner to Dimension From it
                            length = new List<double>();
                            foreach (var point in item.CornerPoints)
                            {
                                length.Add(Math.Sqrt(Math.Pow(item.PointsofGrid.X - point.X, 2) + Math.Pow(item.PointsofGrid.Y - point.Y, 2)));
                            }
                            double minLength = length.Min();
                            int index = length.IndexOf(minLength);
                            #endregion

                            #region Handel the position of Dim
                            if (item.PointsofGrid.X > item.CornerPoints[index].X && item.PointsofGrid.Y < item.CornerPoints[index].Y)
                            {
                                signX = 1; signY = -1;
                            }
                            else if (item.PointsofGrid.X < item.CornerPoints[index].X && item.PointsofGrid.Y < item.CornerPoints[index].Y)
                            {
                                signX = 1; signY = 1;
                            }
                            else if (item.PointsofGrid.X > item.CornerPoints[index].X && item.PointsofGrid.Y > item.CornerPoints[index].Y)
                            {
                                signX = -1; signY = -1;
                            }
                            else
                            {
                                signX = -1; signY = 1;
                            }
                            #endregion

                            #region Create H Dimension
                            dimH.XLine1Point = item.PointsofGrid;
                            dimH.XLine2Point = item.CornerPoints[index];
                            dimH.Rotation = 0;
                            dimH.DimLinePoint = new Point3d(item.CornerPoints[index].X, item.CornerPoints[index].Y + (signX * DistanceTo), 0);
                            dimH.DimensionStyle = dimStyleRecord.ObjectId;
                            dimH.Annotative = dimStyleRecord.Annotative;
                            dimH.Layer = SelectedDimLayer;
                            if (dimH.Measurement == 0)
                            {
                                dimH.Visible = false;
                            }
                            #endregion

                            #region Create V Dimension
                            dimV.XLine1Point = item.PointsofGrid;
                            dimV.XLine2Point = item.CornerPoints[index];
                            dimV.Rotation = 1.5707963268;
                            dimV.DimLinePoint = new Point3d(item.CornerPoints[index].X + (DistanceTo * signY), item.CornerPoints[index].Y, 0);
                            dimV.DimensionStyle = dimStyleRecord.ObjectId;
                            dimV.Annotative = dimStyleRecord.Annotative;
                            dimH.Annotative = dimStyleRecord.Annotative;
                            dimV.Layer = SelectedDimLayer;
                            if (dimV.Measurement == 0)
                            {
                                dimV.Visible = false;
                            }
                            #endregion

                            #region Handel the position of Big Dimension
                            int indexAll = index + 2;
                            int indexAllp1 = index + 3;
                            int indexAllm1 = index + 1;
                            if (indexAll > 3)
                            {
                                indexAll = indexAll - 3;
                            }
                            if (indexAllp1 > 3)
                            {
                                indexAllp1 = indexAllp1 - 3;
                            }
                            if (indexAllm1 > 3)
                            {
                                indexAllm1 = indexAllm1 - 3;
                            }
                            #endregion

                            angleH = 0;
                            angleV = 1.5707963268;

                            #region Get The Angle for Aligned Elements
                            if ((item.CornerPoints[0].X - item.CornerPoints[1].X) * (item.CornerPoints[0].Y - item.CornerPoints[1].Y) != 0)
                            {
                                double deltax = Math.Abs(item.CornerPoints[0].X - item.CornerPoints[1].X);
                                double deltay = Math.Abs(item.CornerPoints[0].Y - item.CornerPoints[1].Y);
                                angleH = Math.Atan(deltay / deltax);
                                angleV = angleH + 1.5707963268;
                            }
                            #endregion

                            #region Create H Big Dimension
                            dimHAll.XLine1Point = item.CornerPoints[indexAll];
                            dimHAll.XLine2Point = item.CornerPoints[indexAllp1];
                            dimHAll.Rotation = angleH;
                            dimHAll.DimLinePoint = new Point3d(item.CornerPoints[indexAll].X, item.CornerPoints[indexAll].Y + (-1 * signX * DistanceTo), 0);
                            dimHAll.DimensionStyle = dimStyleRecord.ObjectId;
                            dimHAll.Annotative = dimStyleRecord.Annotative;
                            dimHAll.Layer = SelectedDimLayer;
                            if (dimHAll.Measurement == 0)
                            {
                                dimHAll.Visible = false;
                            }
                            #endregion

                            #region Create V Big Dimension
                            dimVAll.XLine1Point = item.CornerPoints[indexAll];
                            dimVAll.XLine2Point = item.CornerPoints[indexAllm1];
                            dimVAll.Rotation = angleV;
                            dimVAll.DimLinePoint = new Point3d(item.CornerPoints[indexAll].X + (DistanceTo * -1 * signY), item.CornerPoints[indexAll].Y, 0);
                            dimVAll.DimensionStyle = dimStyleRecord.ObjectId;
                            dimVAll.Annotative = dimStyleRecord.Annotative;
                            dimHAll.Annotative = dimStyleRecord.Annotative;
                            dimVAll.Layer = SelectedDimLayer;
                            if (dimVAll.Measurement == 0)
                            {
                                dimVAll.Visible = false;
                            }
                            #endregion

                            #region Apply the Dimensions to Model View
                            btr.AppendEntity(dimH);
                            btr.AppendEntity(dimV);
                            btr.AppendEntity(dimHAll);
                            btr.AppendEntity(dimVAll);
                            tran.AddNewlyCreatedDBObject(dimH, true);
                            tran.AddNewlyCreatedDBObject(dimV, true);
                            tran.AddNewlyCreatedDBObject(dimHAll, true);
                            tran.AddNewlyCreatedDBObject(dimVAll, true);
                            #endregion

                        }
                        tran2.Commit();
                    }
                    #endregion

                    tran.Commit();
                }
                catch (System.Exception ex)
                {
                    editor.WriteMessage(ex.Message);
                    tran.Abort();
                }
            }
        }

        public void ExcuteRun(object par)
        {
            ColumnDimensions();
            viewObejct.Close();
        }
        public bool CanExcuteRun(object par)
        {
            return true;
        }

        public void ExcuteClose(object par)
        {
            viewObejct.Close();
        }
        public bool CanExcuteClose(object par)
        {
            return true;
        }
    }
}
