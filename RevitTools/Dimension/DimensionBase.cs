using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace RevitTools.Dimmension
{
    public abstract class DimensionBase : IExternalCommand
    {
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Result.Succeeded;
        }
        protected List<Grid> FindVerticalGrids(List<Grid> grids)
        {
            if ((grids.First().Curve as Line).Origin.Y == -1)
            {
                grids.Sort((gr1, gr2) =>
                {
                    int compareX = (gr1.Curve as Line).Origin.X.CompareTo((gr2.Curve as Line).Origin.X);
                    return compareX;
                });
            }
            else if ((grids.First().Curve as Line).Origin.X == -1)
            {
                grids.Sort((gr1, gr2) =>
                {
                    int compareX = (gr1.Curve as Line).Origin.Y.CompareTo((gr2.Curve as Line).Origin.Y);
                    return compareX;
                });
            }
            if (grids.Count == 0) return null;
            var vertiGrids = new List<Grid>();
            foreach (var grid in grids)
            {
                var line = grid.Curve as Line;
                if (line != null && line.Direction.X == -1)
                {
                    vertiGrids.Add(grid);
                }
            }
            return vertiGrids;
        }
        protected List<Grid> FindHorizontalGrids(List<Grid> grids)
        {
            if ((grids.First().Curve as Line).Origin.Y == -1)
            {
                grids.Sort((gr1, gr2) =>
                {
                    int compareX = (gr1.Curve as Line).Origin.X.CompareTo((gr2.Curve as Line).Origin.X);
                    return compareX;
                });
            }
            else if ((grids.First().Curve as Line).Origin.X == -1)
            {
                grids.Sort((gr1, gr2) =>
                {
                    int compareX = (gr1.Curve as Line).Origin.Y.CompareTo((gr2.Curve as Line).Origin.Y);
                    return compareX;
                });
            }
            if (grids.Count == 0) return null;
            var horiGrids = new List<Grid>();
            foreach (var grid in grids)
            {
                var line = grid.Curve as Line;
                if (line != null && line.Direction.Y == -1)
                {
                    horiGrids.Add(grid);
                }
            }
            return horiGrids;
        }
        protected void AutoDim(UIDocument uidoc, FamilyInstance column, Grid grid)
        {
            Document doc = uidoc.Document;
            var viewScale = doc.ActiveView.Scale;
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
                var point = new XYZ(loca.Point.X + viewScale * 10 / 304.8, loca.Point.Y + viewScale * 10 / 304.8, loca.Point.Z);

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
        protected void AutoDim(UIDocument uidoc, List<Grid> grids)
        {
            Document doc = uidoc.Document;
            var viewScale = doc.ActiveView.Scale;
            try
            {
                DimGrid(uidoc, FindVerticalGrids(grids),20);
                DimGrid(uidoc, new List<Grid>() { FindVerticalGrids(grids).First(), FindVerticalGrids(grids).Last() },10);
                DimGrid(uidoc, FindHorizontalGrids(grids),20);
                DimGrid(uidoc, new List<Grid>() { FindHorizontalGrids(grids).First(), FindHorizontalGrids(grids).Last() }, 10);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void DimGrid(UIDocument uidoc, List<Grid> grids, double dis)
        {
            Document doc = uidoc.Document;
            var viewscale = doc.ActiveView.Scale;
            ReferenceArray refArray = new ReferenceArray();
            XYZ dir = null;
            foreach (Element el in grids)
            {
                Grid gr = el as Grid;

                if (gr == null) continue;
                if (dir == null)
                {
                    Curve crv = gr.Curve;
                    dir = new XYZ(0, 0, 1).CrossProduct(crv.GetEndPoint(0) - crv.GetEndPoint(1));
                }
                Reference gridRef = null;
                Options opt = new Options();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;
                opt.View = doc.ActiveView;
                foreach (GeometryObject obj in gr.get_Geometry(opt))
                {
                    if (obj is Line)
                    {
                        Line l = obj as Line;
                        gridRef = l.Reference;
                        refArray.Append(gridRef);
                    }
                }
            }
            XYZ point = new XYZ(grids.First().Curve.GetEndPoint(0).X - viewscale * dis / 304.8, grids.First().Curve.GetEndPoint(0).Y - viewscale * dis / 304.8, 0);
            Line line = Line.CreateBound(point, point + dir * 100);
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
    }

}
