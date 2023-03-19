using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    public class AutoDimColumnsCMD : DimensionBase
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var obs = FindGridsNearColumn(doc);
            using (TransactionGroup tranG = new TransactionGroup(doc, "Auto Dim"))
            {
                tranG.Start();
                foreach (var ob in obs)
                {
                    AutoDim(uidoc, ob.Item1, ob.Item2);
                }
                tranG.Assimilate();
            }
            return Result.Succeeded;
        }
        List<Tuple<FamilyInstance, Grid>> FindGridsNearColumn(Document doc)
        {
            var grids = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_Grids).Cast<Grid>().ToList();
            var columns = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilyInstance>().ToList();
            
            var vertiGrids = FindVerticalGrids(grids);
            var horizGrids = FindHorizontalGrids(grids);
            List<Tuple<FamilyInstance, Grid>> result = new List<Tuple<FamilyInstance, Grid>>();


            foreach (var grid in grids)
            {
                var line = grid.Curve as Line;
                if (line != null && line.Direction.X == -1)
                {
                    vertiGrids.Add(grid);
                }
                else if (line != null && line.Direction.Y == -1)
                {
                    horizGrids.Add(grid);
                }
            }
            foreach (FamilyInstance column in columns)
            {
                var mindisV = double.MaxValue;
                var mindisH = double.MaxValue;
                Grid Vflag = null;
                Grid Hflag = null;
                foreach (var grid in vertiGrids)
                {
                    var dis = grid.Curve.Distance((column.Location as LocationPoint).Point);
                    if (dis < mindisV)
                    {
                        Vflag = grid;
                        mindisV = dis;
                    }
                }
                Tuple<FamilyInstance, Grid> tuple1 = new Tuple<FamilyInstance, Grid>(column, Vflag);
                result.Add(tuple1);
                foreach (var grid in horizGrids)
                {
                    var dis = grid.Curve.Distance((column.Location as LocationPoint).Point);
                    if (dis < mindisH)
                    {
                        Hflag = grid;
                        mindisH = dis;
                    }
                }
                Tuple<FamilyInstance, Grid> tuple2 = new Tuple<FamilyInstance, Grid>(column, Hflag);
                result.Add(tuple2);
            }
            return result;
        }        
    }
}
