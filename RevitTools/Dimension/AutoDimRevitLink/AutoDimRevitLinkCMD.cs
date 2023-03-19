using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Linq;

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
            .OfClass(typeof(RevitLinkInstance))
                            .Cast<RevitLinkInstance>().ToList();
            var docLinked = linkInstances.First().GetLinkDocument();
            var refElemLinked = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Please pick an element in the linked model");
                Element elem = docLinked.GetElement(refElemLinked.LinkedElementId);
                MessageBox.Show(elem.Name);
            //foreach (var item in refElemLinked)
            //{
            //}



            return Result.Succeeded;

        }

    }
}
