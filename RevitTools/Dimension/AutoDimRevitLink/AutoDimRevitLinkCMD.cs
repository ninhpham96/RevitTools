using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    public class AutoDimRevitLinkCMD : DimensionBase
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            DimGrids(uidoc);
            return Result.Succeeded;

        }
        public void DimGrids(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View currentView = doc.ActiveView;
            ReferenceArray referenceArray = new ReferenceArray();

            // Lấy tất cả các đối tượng RevitLinkInstance trong view đó.
            FilteredElementCollector collector = new FilteredElementCollector(doc, currentView.Id);
            RevitLinkInstance linkInstance = collector.OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().First();

            Document linkedDoc = linkInstance.GetLinkDocument();

            // Lấy tất cả các đối tượng mà RevitLinkInstance chứa và nằm trong phạm vi view hiện tại.
            BoundingBoxXYZ bbox = currentView.CropBox;
            Outline outline = new Outline(bbox.Min, bbox.Max);
            BoundingBoxIntersectsFilter bboxFilter = new BoundingBoxIntersectsFilter(outline);
            FilteredElementCollector linkedCollector = new FilteredElementCollector(linkedDoc);
            linkedCollector.WherePasses(bboxFilter);
            List<FamilyInstance> linkedElements = linkedCollector
                .OfCategory(BuiltInCategory.OST_GenericModel).Cast<FamilyInstance>().ToList();
            foreach (FamilyInstance linkedElement in linkedElements)
            {

                if (linkedElement.Document.Equals(linkedDoc))
                {
                    var r = linkedElement.GetReferenceByName("中心(左/右)");
                    referenceArray.Append(r);
                }
            }
            //var ele = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
            //    .WhereElementIsNotElementType().Cast<FamilyInstance>()
            //    .ToList();
            //foreach (var item in ele)
            //{
            //    referenceArray.Append(item.GetReferenceByName("中心(左/右)"));
            //}

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
                    dir = new XYZ(0, 0, 1).CrossProduct(new XYZ(0, 10, 0));
                }

                //Reference gridRef = null;
                //Options opt = new Options();
                //opt.ComputeReferences = true;
                //opt.IncludeNonVisibleObjects = true;
                //opt.View = doc.ActiveView;
                //foreach (GeometryObject obj in gr.get_Geometry(opt))
                //{
                //    if (obj is Line)
                //    {
                //        Line l = obj as Line;
                //        gridRef = l.Reference;
                //        referenceArray.Append(gridRef);
                //        refArray.Append(gridRef);
                //    }
                //}
            }
            XYZ pickPoint = uidoc.Selection.PickPoint();    // Pick a placement point for the dimension line
            Line line = Line.CreateBound(pickPoint, pickPoint + dir * 100);     // Creates the line to be used for the dimension line

            using (Transaction t = new Transaction(doc, "Make Dim"))
            {
                t.Start();
                if (!doc.IsFamilyDocument)
                {
                    doc.Create.NewDimension(doc.ActiveView, line, referenceArray);
                }
                t.Commit();
            }
        }
        public Reference ConvertReferenceLink(Reference r, Document doc)
        {
            if (r.LinkedElementId == ElementId.InvalidElementId) return null;
            string[] ss = r.ConvertToStableRepresentation(doc).Split(':');
            string res = string.Empty;
            bool first = true;
            foreach (string s in ss)
            {
                string t = s;
                if (s.Contains("RVTLINK")) t = "RVTLINK";
                if (!first)
                {
                    res = string.Concat(res, ":", t);
                }
                else
                {
                    res = t;
                    first = false;
                }
            }
            return Reference.ParseFromStableRepresentation(doc, res);
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
}
