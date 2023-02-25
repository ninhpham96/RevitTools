using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using System.Diagnostics;
using System;

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
            var DuctAccessories = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_DuctAccessory).Cast<FamilyInstance>()
                .ToList().FirstOrDefault();
            var DuctTerminal = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_DuctTerminal).Cast<FamilyInstance>()
                .ToList();
            var ducttype = new FilteredElementCollector(doc).WhereElementIsElementType()
                .OfClass(typeof(DuctType)).ToList().Where(p => p.Name == "Taps / Short Radius").First();
            #endregion


            var target = FindPoint(DuctAccessories, DuctTerminal);

            using (Transaction tran = new Transaction(doc, "tao duct"))
            {
                try
                {
                    tran.Start();
                    var listpoint = GetPointtoConnect(target[0].Item1, target[0].Item2);

                    //MessageBox.Show(listpoint[0].ToString() + "\n" + listpoint[1].ToString());

                    Duct newduct1 = Duct.Create(doc, ducttype.Id, lvID, target[0].Item1, listpoint[0]);
                    Duct newduct2 = Duct.Create(doc, ducttype.Id, lvID, target[0].Item2, listpoint[1]);
                    //    //var newduct2 = Duct.Create(doc, ducttype.Id, lvID, connectorsetFrom[4], point);
                    //    //var newduct1 = Duct.Create(doc, ducttype.Id, lvID, target[0].Item1, target[0].Item2);
                    //    MessageBox.Show(target[0].Item2.Origin.ToString());
                    //    var d = doc.Create.NewElbowFitting(target[0].Item1, target[0].Item2);
                    tran.Commit();
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
            foreach (Connector item in GetConnectors(DuctAccessories))
            {
                if(item.Origin.Y >= locOrigin.Y)
                    connsFrom1.Add(item);
                else if(item.Origin.Y <= locOrigin.Y)
                    connsFrom2.Add(item);
            }
            //get connector of DuctTerminal
            foreach (FamilyInstance duct in listDuctTerminal)
            {
                foreach (Connector connector in GetConnectors(duct))
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
        public static List<Connector> GetConnectors(FamilyInstance fam)
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
        public static List<Connector> GetConnectors(Duct duct)
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
    }
}
