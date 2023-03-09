using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
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

            Debug.WriteLine(grids.Count.ToString());
            Debug.WriteLine(columns.Count.ToString());

            return Result.Succeeded;
        }
        List<FamilyInstance[]> FindGridsNearColumn(Grid grids, FamilyInstance columns)
        {
            var result = new List<FamilyInstance[]>();
            FamilyInstance[] f = new FamilyInstance[3];

            

            return result;
        }

        void AutoDim(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            var ele = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element)) as FamilyInstance;
            var gird = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element)) as Grid;
            ReferenceArray refArray = new ReferenceArray();
            if (ele != null)
            {
                var reference = ele.GetReferenceByName("Center (Left/Right)");
                refArray.Append(reference);
            }
            XYZ dir = null;
            if (gird != null)
            {
                dir = new XYZ(0, 0, 1).CrossProduct((gird.Curve.GetEndPoint(0) - gird.Curve.GetEndPoint(1)));
                Reference gridRef = null;

                // Options to extract the reference geometry needed for the NewDimension method
                Options opt = new Options();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;
                opt.View = doc.ActiveView;
                foreach (GeometryObject obj in gird.get_Geometry(opt))
                {
                    if (obj is Line)
                    {
                        Line l = obj as Line;
                        gridRef = l.Reference;
                        refArray.Append(gridRef);   // Append to the list of all reference lines 
                    }
                }
            }
            LocationPoint loca = (ele.Location as LocationPoint);
            var point = loca.Point;
            Line line = Line.CreateUnbound(point, point + dir * 100);

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
