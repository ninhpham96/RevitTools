using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RevitTools.Dimmension
{
    [Transaction(TransactionMode.Manual)]
    public class command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            DimGrids(uiapp);
            return Result.Succeeded;
        }


        public void DimGrids(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Pick all the grid lines you want to dimension to
            GridSelectionFilter filter = new GridSelectionFilter(doc);
            //var grids = uidoc.Selection.PickElementsByRectangle(filter, "Pick Grid Lines");
            var grids = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Grid)).ToList();
            
            ReferenceArray refArray = new ReferenceArray();
            XYZ dir = null;
            XYZ point = null;
            int scale = 0;
            foreach (Element el in grids)
            {
                Grid gr = el as Grid;

                if (gr == null) continue;
                if (dir == null)
                {
                    Curve crv = gr.Curve;

                    dir = new XYZ(0, 0, 1).CrossProduct((crv.GetEndPoint(0) - crv.GetEndPoint(1))); // Get the direction of the gridline
                    point = crv.GetEndPoint(0);
                    scale = gr.Document.ActiveView.Scale;
                }

                Reference gridRef = null;

                // Options to extract the reference geometry needed for the NewDimension method
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
                        refArray.Append(gridRef);   // Append to the list of all reference lines 
                    }
                }
            }

               // Pick a placement point for the dimension line
            Line line = Line.CreateBound(point*0.3, point + dir*1000);     // Creates the line to be used for the dimension line

            using (Transaction t = new Transaction(doc, "Make Dim"))
            {
                t.Start();
                if (!doc.IsFamilyDocument)
                {
                    var dim = doc.Create.NewDimension(doc.ActiveView, line, refArray);
                    var d = dim.Segments.GetEnumerator();
                    while(d.MoveNext())
                    {
                        DimensionSegment dd = d.Current as DimensionSegment;
                        dd.Prefix = "CH";
                    }
                    
                }
                t.Commit();
            }
        }
        /// 

        /// Grid Selection Filter (example for selection filters)
        /// 

        public class GridSelectionFilter : ISelectionFilter
        {
            Document doc = null;
            public GridSelectionFilter(Document document)
            {
                doc = document;
            }

            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Grids")
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return true;
            }
        }
    }
}
