using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace RevitTools.AutoDimmension
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var grids = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_Grids).Cast<Grid>().ToList();
            var columns = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilyInstance>().ToList();

            var obs = FindGridsNearColumn(grids, columns);
            using (TransactionGroup tranG = new TransactionGroup(doc, "Auto Dim"))
            {
                tranG.Start();
                foreach (var ob in obs)
                {
                    DimGridWitdColumns(uidoc, ob.Item1, ob.Item2);
                }
                tranG.Assimilate();
            }
            return Result.Succeeded;
        }
        List<Tuple<FamilyInstance, Grid>> FindGridsNearColumn(List<Grid> grids, List<FamilyInstance> columns)
        {
            var vertiGrids = new List<Grid>();
            var horizGrids = new List<Grid>();
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
        void DimGridWitdColumns(UIDocument uidoc, FamilyInstance column, Grid grid)
        {
            Document doc = uidoc.Document;
            ReferenceArray refArray = new ReferenceArray();
            try
            {
                if (column != null && grid != null)
                {
                    Line li = grid.Curve as Line;
                    if (li.Direction.X == -1)
                    {
                        var reference = column.GetReferenceByName("Center (Front/Back)");
                        refArray.Append(reference);
                    }
                    else if (li.Direction.Y == -1)
                    {
                        var reference = column.GetReferenceByName("Center (Left/Right)");
                        refArray.Append(reference);
                    }
                }
                XYZ dir = null;
                if (grid != null)
                {
                    dir = new XYZ(0, 0, 1).CrossProduct(grid.Curve.GetEndPoint(0) - grid.Curve.GetEndPoint(1));
                    Reference gridRef = null;
                    Options opt = new Options();
                    opt.ComputeReferences = true;
                    opt.IncludeNonVisibleObjects = true;
                    opt.View = doc.ActiveView;
                    foreach (GeometryObject obj in grid.get_Geometry(opt))
                    {
                        if (obj is Line)
                        {
                            Line l = obj as Line;
                            gridRef = l.Reference;
                            refArray.Append(gridRef);
                        }
                    }
                }
                LocationPoint loca = column.Location as LocationPoint;
                var point = new XYZ(loca.Point.X +1,loca.Point.Y+1,loca.Point.Z);
                
                Line line = Line.CreateUnbound(point, point + dir * 1000);
                using (Transaction t = new Transaction(doc, "Make Dim"))
                {
                    t.Start();
                    if (!doc.IsFamilyDocument)
                    {
                        doc.Create.NewDimension(
                          doc.ActiveView, line, refArray);
                    }
                    t.Commit();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
