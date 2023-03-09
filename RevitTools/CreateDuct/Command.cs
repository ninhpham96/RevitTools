using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using System.Diagnostics;
using System;
using System.Windows;
using Autodesk.Revit.UI.Selection;

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
            Document doc = uidoc.Document;
            ElementId lvID;
            var filter = new DuctAccessoryFilter();
            if (doc.ActiveView.GetType().Name == "View3D")
            {
                MessageBox.Show("Go to view 2D to run.", "Warning", MessageBoxButton.OK);
                return Result.Failed;
            }
            else
            {
                lvID = doc.ActiveView.GenLevel.Id;
                try
                {
                    FamilyInstance Fasu = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, filter, "Please, pick FASU!")) as FamilyInstance;
                    List<FamilyInstance> listAnemostat = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfCategory(BuiltInCategory.OST_DuctTerminal).Cast<FamilyInstance>().ToList();
                    ElementId ducttypeId = new FilteredElementCollector(doc).WhereElementIsElementType()
                        .OfClass(typeof(DuctType)).ToList().Where(p => p.Name == "Taps / Short Radius").First().Id;

                    List<Tuple<Connector, Connector>> listPairConnector = FindConnectorNearConnector(Fasu, listAnemostat);
                    using (TransactionGroup tranG = new TransactionGroup(doc, "AutoRoute"))
                    {
                        tranG.Start();
                        foreach (Tuple<Connector, Connector> pairConnector in listPairConnector)
                        {
                            if (CheckDistance(pairConnector))
                                CreateMultiDuct(pairConnector, doc, ducttypeId, lvID);
                            else
                            {
                                CreateTwoDuct(pairConnector, doc, ducttypeId, lvID);
                            }
                        }
                        tranG.Assimilate();
                    }
                }
                catch (Exception)
                {
                }
                return Result.Succeeded;
            }

        }
        void CreateMultiDuct(Tuple<Connector, Connector> pairConnector, Document doc, ElementId ducttypeId, ElementId lvID)
        {
            using (Transaction t1 = new Transaction(doc))
            {
                t1.Start("Create Multi Duct");
                List<XYZ> listLocation = GetLocaToCreateDuct(pairConnector.Item1, pairConnector.Item2);
                Duct newduct1 = Duct.Create(doc, ducttypeId, lvID, pairConnector.Item1, listLocation[0]);
                Duct newduct2 = Duct.Create(doc, ducttypeId, lvID, pairConnector.Item2, listLocation[1]);
                Duct newduct3 = Duct.Create(doc, ducttypeId, lvID, GetUnConnectors(newduct1).First(), GetUnConnectors(newduct2).First());
                foreach (var conn in GetConnectors(newduct3))
                {
                    foreach (var con in GetConnectors(new List<Duct>() { newduct1, newduct2 }))
                    {
                        Debug.WriteLine(GetConnectors(new List<Duct>() { newduct1, newduct2 }).Count);
                        if (conn.Origin.DistanceTo(con.Origin) == 0)
                        {
                            var newduct = doc.Create.NewElbowFitting(conn, con);
                        }
                    }
                }
                t1.Commit();
            }
        }
        void CreateTwoDuct(Tuple<Connector, Connector> pairConnector, Document doc, ElementId ducttypeId, ElementId lvID)
        {
            using (Transaction t1 = new Transaction(doc))
            {
                t1.Start("Create Two Duct");
                List<XYZ> listLocation = GetLocaToCreateDuct(pairConnector.Item1, pairConnector.Item2);
                Duct newduct1 = Duct.Create(doc, ducttypeId, lvID, pairConnector.Item1, listLocation[0]);
                Duct newduct2 = Duct.Create(doc, ducttypeId, lvID, pairConnector.Item2, GetUnConnectors(newduct1).First());
                foreach (var conn in GetConnectors(newduct1))
                {
                    foreach (var con in GetConnectors(newduct2))
                    {
                        if (conn.Origin.DistanceTo(con.Origin) == 0)
                        {
                            var newduct = doc.Create.NewElbowFitting(conn, con);
                        }
                    }
                }
                t1.Commit();
            }
        }
        List<Tuple<Connector, Connector>> FindConnectorNearConnector(FamilyInstance Fasu, List<FamilyInstance> listAnemo)
        {
            List<Connector> connofFasu1 = new List<Connector>();
            List<Connector> connofFasu2 = new List<Connector>();
            List<Connector> connofAnemo1 = new List<Connector>();
            List<Connector> connofAnemo2 = new List<Connector>();
            List<Connector> connofAnemo3 = new List<Connector>();
            List<Connector> connofAnemo4 = new List<Connector>();
            var locaOrigin = (Fasu.Location as LocationPoint).Point;
            List<Tuple<Connector, Connector>> result = new List<Tuple<Connector, Connector>>();
            foreach (Connector conn in GetUnConnectors(Fasu))
            {
                if (conn.Origin.Y >= locaOrigin.Y)
                    connofFasu1.Add(conn);
                else if (conn.Origin.Y <= locaOrigin.Y)
                    connofFasu2.Add(conn);
            }
            foreach (Connector conn in GetUnConnectors(listAnemo))
            {
                if (conn.Origin.Y >= locaOrigin.Y && conn.Origin.X <= locaOrigin.X)
                {
                    connofAnemo1.Add(conn);
                }
                else if (conn.Origin.Y >= locaOrigin.Y && conn.Origin.X > locaOrigin.X)
                {
                    connofAnemo2.Add(conn);
                }
                else if (conn.Origin.Y <= locaOrigin.Y && conn.Origin.X <= locaOrigin.X)
                {
                    connofAnemo3.Add(conn);
                }
                else if (conn.Origin.Y < locaOrigin.Y && conn.Origin.X > locaOrigin.X)
                {
                    connofAnemo4.Add(conn);
                }
            }
            connofFasu1.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.X.CompareTo(conn2.Origin.X);

                if (compareX == 0)
                {
                    return conn1.Origin.Y.CompareTo(conn2.Origin.Y);
                }
                return compareX;
            });
            connofFasu2.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.X.CompareTo(conn2.Origin.X);

                if (compareX == 0)
                {
                    return conn1.Origin.Y.CompareTo(conn2.Origin.Y);
                }
                return compareX;
            });
            connofAnemo1.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.Y.CompareTo(conn2.Origin.Y);

                if (compareX == 0)
                {
                    return conn1.Origin.X.CompareTo(conn2.Origin.X);
                }
                return compareX;
            });
            connofAnemo2.Sort((conn1, conn2) =>
            {
                int compareX = conn2.Origin.Y.CompareTo(conn1.Origin.Y);

                if (compareX == 0)
                {
                    return conn2.Origin.X.CompareTo(conn1.Origin.X);
                }
                return compareX;
            });
            connofAnemo3.Sort((conn1, conn2) =>
            {
                int compareX = conn2.Origin.Y.CompareTo(conn1.Origin.Y);

                if (compareX == 0)
                {
                    return conn2.Origin.X.CompareTo(conn1.Origin.X);
                }
                return compareX;
            });
            connofAnemo4.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.Y.CompareTo(conn2.Origin.Y);

                if (compareX == 0)
                {
                    return conn1.Origin.X.CompareTo(conn2.Origin.X);
                }
                return compareX;
            });
            for (int i = 0; i < connofFasu1.Count; i++)
            {
                if (connofAnemo1.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connofFasu1[i], connofAnemo1[0]);
                    result.Add(tuple);
                    connofAnemo1.RemoveAt(0);
                    continue;
                }
                if (connofAnemo2.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connofFasu1[i], connofAnemo2[0]);
                    result.Add(tuple);
                    connofAnemo2.RemoveAt(0);
                    continue;
                }
            }
            for (int i = 0; i < connofFasu2.Count; i++)
            {
                if (connofAnemo3.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connofFasu2[i], connofAnemo3[0]);
                    result.Add(tuple);
                    connofAnemo3.RemoveAt(0);
                    continue;
                }
                if (connofAnemo4.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connofFasu2[i], connofAnemo4[0]);
                    result.Add(tuple);
                    connofAnemo4.RemoveAt(0);
                    continue;
                }
            }
            return result;
        }
        List<XYZ> GetLocaToCreateDuct(Connector connFasu, Connector connAnemo)
        {
            if (connFasu.CoordinateSystem.BasisZ.X == -1 || connFasu.CoordinateSystem.BasisZ.X == 1)
            {
                return new List<XYZ>()
                {
                    new XYZ(connAnemo.Origin.X,connFasu.Origin.Y,connFasu.Origin.Z),
                    new XYZ(connAnemo.Origin.X,connAnemo.Origin.Y,connFasu.Origin.Z)
                };
            }
            else if (connFasu.CoordinateSystem.BasisZ.Y == -1 || connFasu.CoordinateSystem.BasisZ.Y == 1)
            {
                return new List<XYZ>()
                {
                    new XYZ(connFasu.Origin.X,connAnemo.Origin.Y,connFasu.Origin.Z),
                    new XYZ(connAnemo.Origin.X,connAnemo.Origin.Y,connFasu.Origin.Z)
                };
            }
            else
                return null;
        }
        List<Connector> GetUnConnectors(FamilyInstance fam)
        {
            List<Connector> connectors = new List<Connector>();
            ConnectorSet connectorset = fam.MEPModel.ConnectorManager.UnusedConnectors;
            ConnectorSetIterator iterator = connectorset.ForwardIterator();
            while (iterator.MoveNext())
            {
                Connector connector = iterator.Current as Connector;
                if (connector != null)
                    connectors.Add(connector);
            }
            return connectors;
        }
        List<Connector> GetUnConnectors(Duct duct)
        {
            List<Connector> connectors = new List<Connector>();
            ConnectorSet connectorset = duct.ConnectorManager.UnusedConnectors;
            ConnectorSetIterator iterator = connectorset.ForwardIterator();
            while (iterator.MoveNext())
            {
                Connector connector = iterator.Current as Connector;
                if (connector != null)
                    connectors.Add(connector);
            }
            return connectors;
        }
        List<Connector> GetConnectors(Duct duct)
        {
            List<Connector> connectors = new List<Connector>();
            ConnectorSet connectorset = duct.ConnectorManager.Connectors;
            ConnectorSetIterator iterator = connectorset.ForwardIterator();
            while (iterator.MoveNext())
            {
                Connector connector = iterator.Current as Connector;
                if (connector != null)
                    connectors.Add(connector);
            }
            return connectors;
        }
        List<Connector> GetUnConnectors(List<Duct> ducts)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var duct in ducts)
            {
                connectors.AddRange(GetUnConnectors(duct));
            }
            return connectors;
        }
        List<Connector> GetConnectors(List<Duct> ducts)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var duct in ducts)
            {
                connectors.AddRange(GetConnectors(duct));
            }
            return connectors;
        }
        List<Connector> GetUnConnectors(List<FamilyInstance> ducts)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var duct in ducts)
            {
                connectors.AddRange(GetUnConnectors(duct));
            }
            return connectors;
        }
        bool CheckDistance(Tuple<Connector, Connector> pairConnector)
        {
            double distanceX = Math.Abs(pairConnector.Item1.Origin.X - pairConnector.Item2.Origin.X);
            double distanceY = Math.Abs(pairConnector.Item1.Origin.Y - pairConnector.Item2.Origin.Y);
            if (distanceX < 1 || distanceY < 1)
                return false;
            return true;
        }
    }
    class DuctAccessoryFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue == -2008016;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
