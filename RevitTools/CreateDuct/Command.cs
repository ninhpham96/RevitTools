using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using System.Diagnostics;
using System;
using System.Windows;

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

            #region get data
            FamilyInstance DuctAccessories = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_DuctAccessory).Cast<FamilyInstance>()
                .ToList().FirstOrDefault();
            List< FamilyInstance> DuctTerminal = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_DuctTerminal).Cast<FamilyInstance>()
                .ToList();
            var ducttype = new FilteredElementCollector(doc).WhereElementIsElementType()
                .OfClass(typeof(DuctType)).ToList().Where(p => p.Name == "Taps / Short Radius").First();
            #endregion
            List<Tuple<Connector,Connector>> target = FindPoint(DuctAccessories, DuctTerminal);
            using (TransactionGroup tran = new TransactionGroup(doc, "tao duct"))
            {
                try
                {
                    List<Connector> listconn;
                    List<Connector> listconns;
                    tran.Start();
                    foreach (var item in target)
                    {
                        var listPoint = GetPointtoConnect(item.Item1, item.Item2);
                        using (Transaction t1 = new Transaction(doc))
                        {
                            t1.Start("tao duct");
                            Duct newduct1 = Duct.Create(doc, ducttype.Id, lvID, item.Item1, listPoint[0]);
                            Duct newduct2 = Duct.Create(doc, ducttype.Id, lvID, item.Item2, listPoint[1]);
                            listconn = GetUnConnectors(new List<Duct>() { newduct1, newduct2 });
                            Duct newduct3 = Duct.Create(doc, ducttype.Id, new ElementId(311), listconn[0], listconn[1]);
                            listconns = GetConnectors(newduct3);
                            var temp = new List<Connector>();
                            foreach (var conn in listconns)
                            {
                                foreach (var con in listconn)
                                {
                                    if (conn.Origin.DistanceTo(con.Origin) <= 0.0001)
                                    {
                                        var newduct = doc.Create.NewElbowFitting(conn, con);
                                    }
                                }
                            }                            
                            t1.Commit();
                        }
                    }
                    tran.Assimilate();
                }
                catch (System.Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            return Result.Succeeded;
        }
        public List<Tuple<Connector, Connector>> FindPoint(FamilyInstance DuctAccessories, List<FamilyInstance> listDuctTerminal)
        {
            //list connector.Y lon hon locOrigin.Y
            List<Connector> connsFrom1 = new List<Connector>();
            List<Connector> connsFrom2 = new List<Connector>();
            //list connector.Y nho hon locOrigin.Y
            List<Connector> connsTo1 = new List<Connector>();
            List<Connector> connsTo2 = new List<Connector>();
            List<Connector> connsTo3 = new List<Connector>();
            List<Connector> connsTo4 = new List<Connector>();
            var locOrigin = (DuctAccessories.Location as LocationPoint).Point;
            List<Tuple<Connector, Connector>> listTarget = new List<Tuple<Connector, Connector>>();

            //get connector of DuctAccessories
            foreach (Connector item in GetUnConnectors(DuctAccessories))
            {
                if (item.Origin.Y >= locOrigin.Y)
                    connsFrom1.Add(item);
                else if (item.Origin.Y <= locOrigin.Y)
                    connsFrom2.Add(item);
            }
            //get connector of DuctTerminal
            foreach (Connector connector in GetUnConnectors(listDuctTerminal))
            {
                if (connector.Origin.Y >= locOrigin.Y && connector.Origin.X <= locOrigin.X)
                {
                    connsTo1.Add(connector);
                }
                else if (connector.Origin.Y >= locOrigin.Y && connector.Origin.X > locOrigin.X)
                {
                    connsTo2.Add(connector);
                }
                else if (connector.Origin.Y <= locOrigin.Y && connector.Origin.X <= locOrigin.X)
                {
                    connsTo3.Add(connector);
                }
                else if (connector.Origin.Y < locOrigin.Y && connector.Origin.X > locOrigin.X)
                {
                    connsTo4.Add(connector);
                }
            }
            #region sort
            //SORT THEO X MIN, Y MIN
            connsFrom1.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.X.CompareTo(conn2.Origin.X);

                if (compareX == 0)
                {
                    return conn1.Origin.Y.CompareTo(conn2.Origin.Y);
                }
                return compareX;
            });
            connsFrom2.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.X.CompareTo(conn2.Origin.X);

                if (compareX == 0)
                {
                    return conn1.Origin.Y.CompareTo(conn2.Origin.Y);
                }
                return compareX;
            });
            //SORT THEO Y MIN, X MIN
            connsTo1.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.Y.CompareTo(conn2.Origin.Y);

                if (compareX == 0)
                {
                    return conn1.Origin.X.CompareTo(conn2.Origin.X);
                }
                return compareX;
            });
            //sort theo y max, x max
            connsTo2.Sort((conn1, conn2) =>
            {
                int compareX = conn2.Origin.Y.CompareTo(conn1.Origin.Y);

                if (compareX == 0)
                {
                    return conn2.Origin.X.CompareTo(conn1.Origin.X);
                }
                return compareX;
            });
            // sort theo y max
            connsTo3.Sort((conn1, conn2) =>
            {
                int compareX = conn2.Origin.Y.CompareTo(conn1.Origin.Y);

                if (compareX == 0)
                {
                    return conn2.Origin.X.CompareTo(conn1.Origin.X);
                }
                return compareX;
            });
            //sort theo y min
            connsTo4.Sort((conn1, conn2) =>
            {
                int compareX = conn1.Origin.Y.CompareTo(conn2.Origin.Y);

                if (compareX == 0)
                {
                    return conn1.Origin.X.CompareTo(conn2.Origin.X);
                }
                return compareX;
            });
            #endregion
            for (int i = 0; i < connsFrom1.Count; i++)
            {
                if (connsTo1.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connsFrom1[i], connsTo1[0]);
                    listTarget.Add(tuple);
                    connsTo1.RemoveAt(0);
                    continue;
                }
                if (connsTo2.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connsFrom1[i], connsTo2[0]);
                    listTarget.Add(tuple);
                    connsTo2.RemoveAt(0);
                    continue;
                }
            }
            for (int i = 0; i < connsFrom2.Count; i++)
            {
                if (connsTo3.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connsFrom2[i], connsTo3[0]);
                    listTarget.Add(tuple);
                    connsTo3.RemoveAt(0);
                    continue;
                }
                if (connsTo4.Count > 0)
                {
                    Tuple<Connector, Connector> tuple = new Tuple<Connector, Connector>(connsFrom2[i], connsTo4[0]);
                    listTarget.Add(tuple);
                    connsTo4.RemoveAt(0);
                    continue;
                }
            }
            return listTarget;
        }
        public List<XYZ> GetPointtoConnect(Connector fromconn, Connector toconn)
        {
            if (fromconn.CoordinateSystem.BasisZ.X == -1 || fromconn.CoordinateSystem.BasisZ.X == 1)
            {
                return new List<XYZ>()
                {
                    new XYZ(toconn.Origin.X,fromconn.Origin.Y,fromconn.Origin.Z),
                    new XYZ(toconn.Origin.X,toconn.Origin.Y,fromconn.Origin.Z)
                };
            }
            else if (fromconn.CoordinateSystem.BasisZ.Y == -1 || fromconn.CoordinateSystem.BasisZ.Y == 1)
            {
                return new List<XYZ>()
                {
                    new XYZ(fromconn.Origin.X,toconn.Origin.Y,fromconn.Origin.Z),
                    new XYZ(toconn.Origin.X,toconn.Origin.Y,fromconn.Origin.Z)
                };
            }
            else
                return null;
        }
        public List<Connector> GetUnConnectors(FamilyInstance fam)
        {
            List<Connector> connectors = new List<Connector>();
            var connectorSetofDuctAccessories = fam.MEPModel.ConnectorManager.UnusedConnectors;
            ConnectorSetIterator iteratorofDuctAccessories = connectorSetofDuctAccessories.ForwardIterator();
            while (iteratorofDuctAccessories.MoveNext())
            {
                Connector connector = iteratorofDuctAccessories.Current as Connector;
                if (connector != null)
                    connectors.Add(connector);
            }
            return connectors;
        }
        public List<Connector> GetUnConnectors(Duct duct)
        {
            List<Connector> connectors = new List<Connector>();
            var connectorSetofDuctAccessories = duct.ConnectorManager.UnusedConnectors;
            ConnectorSetIterator iteratorofDuctAccessories = connectorSetofDuctAccessories.ForwardIterator();
            while (iteratorofDuctAccessories.MoveNext())
            {
                Connector connector = iteratorofDuctAccessories.Current as Connector;
                if (connector != null)
                    connectors.Add(connector);
            }
            return connectors;
        }
        public List<Connector> GetConnectors(Duct duct)
        {
            List<Connector> connectors = new List<Connector>();
            var connectorSetofDuctAccessories = duct.ConnectorManager.Connectors;
            ConnectorSetIterator iteratorofDuctAccessories = connectorSetofDuctAccessories.ForwardIterator();
            while (iteratorofDuctAccessories.MoveNext())
            {
                Connector connector = iteratorofDuctAccessories.Current as Connector;
                if (connector != null)
                    connectors.Add(connector);
            }
            return connectors;
        }
        public List<Connector> GetUnConnectors(List<Duct> ducts)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var duct in ducts)
            {
                connectors.AddRange(GetUnConnectors(duct));
            }
            return connectors;
        }
        public List<Connector> GetConnectors(List<Duct> ducts)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var duct in ducts)
            {
                connectors.AddRange(GetConnectors(duct));
            }
            return connectors;
        }
        public List<Connector> GetUnConnectors(List<FamilyInstance> ducts)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var duct in ducts)
            {
                connectors.AddRange(GetUnConnectors(duct));
            }
            return connectors;
        }
    }
}
