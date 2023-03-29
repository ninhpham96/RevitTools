using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AutoDimDoubouchiCMD : DimensionBase
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            ReferenceArray referenceArray = new ReferenceArray();

            //var reference = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList().First();
            //var linkdoc = reference.GetLinkDocument();
            //var ele = new FilteredElementCollector(linkdoc).WhereElementIsNotElementType()
            //    .OfCategory(BuiltInCategory.OST_GenericModel)
            //    .Cast<FamilyInstance>().ToList();
            //foreach (var item in ele)
            //{
            //    referenceArray.Append(item.GetReferenceByName("Center (Front/Back)"));
            //}
            //MessageBox.Show(referenceArray.Size.ToString());
            // Lấy đối tượng View hiện tại.
            // Lấy đối tượng View hiện tại.
            //View currentView = doc.ActiveView;

            //// Lấy tất cả các đối tượng RevitLinkInstance trong view đó.
            //FilteredElementCollector collector = new FilteredElementCollector(doc, currentView.Id);
            //RevitLinkInstance linkInstance = collector.OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().First();

            //Document linkedDoc = linkInstance.GetLinkDocument();

            //// Lấy tất cả các đối tượng mà RevitLinkInstance chứa và nằm trong phạm vi view hiện tại.
            //BoundingBoxXYZ bbox = currentView.CropBox;
            //Outline outline = new Outline(bbox.Min, bbox.Max);
            //BoundingBoxIntersectsFilter bboxFilter = new BoundingBoxIntersectsFilter(outline);
            //FilteredElementCollector linkedCollector = new FilteredElementCollector(linkedDoc);
            //linkedCollector.WherePasses(bboxFilter);
            //List<FamilyInstance> linkedElements = linkedCollector
            //    .OfCategory(BuiltInCategory.OST_GenericModel).Cast<FamilyInstance>().ToList();
            //foreach (FamilyInstance linkedElement in linkedElements)
            //{
            //    if (linkedElement.Document.Equals(linkedDoc))
            //    {
            //        referenceArray.Append(linkedElement.GetReferenceByName("Center (Left/Right)"));
            //    }
            //}
            //using (TransactionGroup tranG = new TransactionGroup(doc, "Auto Dim"))
            //{
            //    tranG.Start();
            //    AutoDim(uidoc, referenceArray);
            //    tranG.Assimilate();
            //}

            var ele = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType().Cast<FamilyInstance>()
                .ToList();
            foreach (var item in ele)
            {
                referenceArray.Append(item.GetReferenceByName("Center (Front/Back)"));
            }
            using (TransactionGroup tranG = new TransactionGroup(doc, "Auto Dim"))
            {
                tranG.Start();
                AutoDim(uidoc, referenceArray);
                tranG.Assimilate();
            }
            //MessageBox.Show(referenceArray.Size.ToString());

            return Result.Succeeded;
        }
    }

}
