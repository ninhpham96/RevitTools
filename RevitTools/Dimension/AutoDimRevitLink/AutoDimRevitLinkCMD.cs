using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
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

            List<RevitLinkInstance> linkInstances = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();
            var docLinked = linkInstances.First().GetLinkDocument();
            var refElemLinked = uidoc.Selection.PickObjects(ObjectType.LinkedElement, "Please pick an element in the linked model");
            foreach (var item in refElemLinked)
            {
                Element elem = docLinked.GetElement(item.LinkedElementId);
                MessageBox.Show(elem.Name);
            }



            return Result.Succeeded;

        }

    }
}
