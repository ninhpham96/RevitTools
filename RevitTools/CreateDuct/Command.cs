using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using rvPoint = Autodesk.Revit.DB.Point;
using System.Linq;
using Autodesk.Revit.Attributes;
using System.Windows;
using Autodesk.Revit.DB.Mechanical;
using System.Diagnostics;

namespace CreateDuct
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            List<Connector> connectorsetFrom = new List<Connector>();
            List<Connector> connectorsetTo = new List<Connector>();
            ElementId lvID = new ElementId(311);


            var DuctAccessories = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_DuctAccessory)
                .WhereElementIsNotElementType().ToList();
            var DuctTerminal = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_DuctTerminal)
                .WhereElementIsNotElementType().ToList();
            var ducttype = new FilteredElementCollector(doc).WhereElementIsElementType()
                .OfClass(typeof(DuctType)).ToList().Where(p => p.Name == "Taps / Short Radius").First();



            //get connector of DuctAccessories
            foreach (FamilyInstance duct in DuctAccessories)
            {
                var connectorSet = duct.MEPModel.ConnectorManager.UnusedConnectors;
                ConnectorSetIterator iterator = connectorSet.ForwardIterator();
                while (iterator.MoveNext())
                {
                    Connector connector = iterator.Current as Connector;
                    if (connector != null)
                    {
                        connectorsetFrom.Add(connector);
                    }
                }

            }
            //get connector of DuctTerminal
            foreach (FamilyInstance duct in DuctTerminal)
            {
                var connectorSet = duct.MEPModel.ConnectorManager.UnusedConnectors;
                ConnectorSetIterator iterator = connectorSet.ForwardIterator();
                while (iterator.MoveNext())
                {
                    Connector connector = iterator.Current as Connector;
                    if (connector != null)
                    {
                        connectorsetTo.Add(connector);
                    }
                }

            }

            //get location of connector

            foreach (Connector conn in connectorsetTo)
            {
                var loca = conn.Origin;
            }
            XYZ point = new XYZ(-11.2576769679356, 16.8624946926416, 10.59824783557);

            using (Transaction tran = new Transaction(doc, "tao duct"))
            {
                try
                {
                    tran.Start();
                    var newduct2 = Duct.Create(doc, ducttype.Id, lvID, connectorsetFrom[3], point);
                    var newduct1 = Duct.Create(doc, ducttype.Id, lvID, connectorsetTo[0], point);
                    tran.Commit();
                }
                catch (System.Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return Result.Succeeded;
        }
    }
}
