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
    [Transaction(TransactionMode.Manual)]
    public class command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //ReferenceArray refsArray = new ReferenceArray();
            //List<Reference> refs = uidoc.Selection.PickObjects(ObjectType.Face).ToList();
            //MessageBox.Show(refs.Count.ToString());
            //foreach (Reference r in refs)
            //{

            //    MessageBox.Show(r.GetType().Name);
            //}
            DimGrids(uidoc);

            return Result.Succeeded;
        }

        public void DimGrids(UIDocument uidoc)
        {
            Document doc = uidoc.Document;

            // Pick all the grid lines you want to dimension to
            GridSelectionFilter filter = new GridSelectionFilter(doc);
            var grids = uidoc.Selection.PickElementsByRectangle(filter, "Pick Grid Lines");

            ReferenceArray refArray = new ReferenceArray();
            XYZ dir = null;

            foreach (Element el in grids)
            {
                Grid gr = el as Grid;

                if (gr == null) continue;
                if (dir == null)
                {
                    Curve crv = gr.Curve;
                    dir = new XYZ(0, 0, 1).CrossProduct((crv.GetEndPoint(0) - crv.GetEndPoint(1))); // Get the direction of the gridline
                }
                Debug.WriteLine(dir.X);
                Debug.WriteLine(dir.Y);
                Debug.WriteLine(dir.Z);
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

            XYZ pickPoint = uidoc.Selection.PickPoint();    // Pick a placement point for the dimension line
            Line line = Line.CreateBound(pickPoint, pickPoint + dir * 100);     // Creates the line to be used for the dimension line

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
