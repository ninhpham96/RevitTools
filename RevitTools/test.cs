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
        foreach (var item in select)
        {
            var ele = doc.GetElement(item) as Duct;
            if (ele != null)
            {
                foreach (Connector connector in Command.GetConnectors(ele))
                {
                    Debug.Write(connector.IsConnected);
                    if (connector.IsConnected == false)
                        connectors.Add(connector);
                }
            }
        }
        using(Transaction tran = new Transaction(doc, "ket noi"))
        {
            tran.Start();
            var duct = Duct.Create(doc,ducttype.Id,new ElementId(311), connectors[0], connectors[1]);
           
            tran.Commit();
        }
        return Result.Succeeded;
    }
}
