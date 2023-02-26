using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using System.Collections.Generic;
using CreateDuct;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;

[Transaction(TransactionMode.Manual)]
public class Command1 : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiapp = commandData.Application;
        UIDocument uidoc = uiapp.ActiveUIDocument;
        Application app = uiapp.Application;
        Document doc = uidoc.Document;
        List<Connector> connectors = new List<Connector>();

        var ducttype = new FilteredElementCollector(doc).WhereElementIsElementType()
                .OfClass(typeof(DuctType)).ToList().Where(p => p.Name == "Taps / Short Radius").First();

        var select = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element).ToList();
              
        return Result.Succeeded;
    }
}
