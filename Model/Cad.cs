using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Internal.Windows;
using ColumnDimensions.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ColumnDimensions.Model
{
    public class Cad
    {
        public static List<string> CadLayer { set; get; }
        public static List<string> CadDimStyle { set; get; }

        [CommandMethod("coldim")]
        public void ColumnDim()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;


            #region license

            DateTime dateNow = DateTime.Now;
            DateTime dateExpire = new DateTime(2023, 2, 2, 1, 0, 0);
            if (DateTime.Compare(dateNow,dateExpire)<0)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("The license is Still Working and will expire in 2/1/2023 at 1:00AM");
            }
            else
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("The license is Expired ... Please Content ENG/ Moustafa Safwat for help\nThank you...");
                return;
            }
            #endregion


            using(Transaction tran = db.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable lt = tran.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    DimStyleTable dmt = tran.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
                    CadLayer = new List<string>();
                    CadDimStyle = new List<string>();
                    foreach (var item in lt)
                    {
                        LayerTableRecord ltr = tran.GetObject(item, OpenMode.ForRead) as LayerTableRecord;
                        CadLayer.Add(ltr.Name);
                    }
                    foreach (var item in dmt)
                    {
                        DimStyleTableRecord dmr = tran.GetObject(item, OpenMode.ForRead) as DimStyleTableRecord;
                        CadDimStyle.Add(dmr.Name);
                    }
                    UserInterface1 window = new UserInterface1();
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(window);


                    tran.Commit();
                }
                catch (System.Exception ex)
                {
                    editor.WriteMessage(ex.Message);
                    tran.Abort();
                }
                
            }
        }
    }
}
