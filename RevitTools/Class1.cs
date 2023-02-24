using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using application = Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using System.Windows;
using Autodesk.Revit.DB.Mechanical;
using System.Linq;
using System.Collections.Generic;
using System;
using Autodesk.Revit.Creation;
using System.Xml.Linq;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            application.Application app = uiapp.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            List<Connector> list0 = new List<Connector>();
            List<Connector> list1 = new List<Connector>();
            var ele = new FilteredElementCollector(doc,doc.ActiveView.Id).WhereElementIsNotElementType().OfType<Duct>().ToList();

            foreach( Duct duct in ele )
            {
                var connectorset = duct.ConnectorManager.Connectors;
                ConnectorSetIterator iterator = connectorset.ForwardIterator();
                while( iterator.MoveNext() )
                {
                    Connector connector = iterator.Current as Connector;
                    if (connector != null && connector.Id ==0)
                        list0.Add( connector );
                    else if (connector != null && connector.Id == 1)
                        list1.Add( connector );
                }
            }
            using (Transaction tran = new Transaction(doc, "tao ong"))
            {
                tran.Start();
                try
                {
                    var d = doc.Create.NewElbowFitting(list0[0], list1[1]);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Source);
                }
                tran.Commit();
            }

            return Result.Succeeded;
            #region
            //ElementId levelId = ElementId.InvalidElementId;
            //List<Connector> connectors = new List<Connector>();
            //ElementCategoryFilter filter_DuctAccessory = new ElementCategoryFilter(BuiltInCategory.OST_DuctAccessory);
            //ElementCategoryFilter filter_DuctTerminal = new ElementCategoryFilter(BuiltInCategory.OST_DuctTerminal);

            //LogicalOrFilter filters = new LogicalOrFilter(filter_DuctAccessory, filter_DuctTerminal);

            ////var pick = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element).ElementId;
            //var select1 = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType()
            //    .WherePasses(filter_DuctAccessory)
            //    .ToElements().ToList();
            //var select2 = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType()
            //    .WherePasses(filter_DuctTerminal)
            //    .ToElements().ToList();
            //FilteredElementCollector collector = new FilteredElementCollector(doc);
            //collector.OfClass(typeof(DuctType));
            //List<DuctType> ductType = collector.Cast<DuctType>().ToList();

            //#region
            //foreach (var element in select1)
            //{
            //    var ele = element as FamilyInstance;
            //    try
            //    {
            //        levelId = ele.LevelId;
            //        var connectorSet = ele.MEPModel.ConnectorManager.Connectors;
            //        ConnectorSetIterator iterator = connectorSet.ForwardIterator();
            //        while (iterator.MoveNext())
            //        {
            //            Connector connector = iterator.Current as Connector;
            //            if (connector != null && connector.Id == 8)
            //            {
            //                connectors.Add(connector);
            //            }
            //        }
            //    }
            //    catch (System.Exception e)
            //    {
            //        MessageBox.Show(e.Message);
            //    }
            //}
            //#endregion
            //foreach (var element in select2)
            //{
            //    var ele = element as FamilyInstance;
            //    try
            //    {
            //        var connectorSet = ele.MEPModel.ConnectorManager.Connectors;
            //        ConnectorSetIterator iterator = connectorSet.ForwardIterator();
            //        while (iterator.MoveNext())
            //        {
            //            Connector connector = iterator.Current as Connector;
            //            if (connector != null)
            //            {
            //                connectors.Add(connector);
            //            }
            //        }
            //    }
            //    catch (System.Exception e)
            //    {
            //        MessageBox.Show(e.Message);
            //    }
            //    break;
            //}
            //using (Transaction tran = new Transaction(doc, "tao ong"))
            //{
            //    tran.Start();
            //    //var newduct = doc.Create.NewFlexDuct(connectors[0], connectors[1], select3);
            //    //var d = Duct.Create(doc, ductType[8].Id, levelId, connectors[0], connectors[1]);
            //    try
            //    {
            //        var d = doc.Create.NewElbowFitting(connectors[0], connectors[1]);
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show(e.Source);
            //        throw;
            //    }
            //    tran.Commit();
            //}
            #endregion
        }
        public Duct CreateDuctBetweenConnectors(UIDocument uiDocument)
        {
            // prior to running this example
            // select some mechanical equipment with a supply air connector
            // and an elbow duct fitting with a connector in line with that connector
            ElementId levelId = ElementId.InvalidElementId;
            Connector connector1 = null, connector2 = null;
            ConnectorSetIterator csi = null;
            ICollection<ElementId> selectedIds = uiDocument.Selection.GetElementIds();
            Autodesk.Revit.DB.Document document = uiDocument.Document;
            // First find the selected equipment and get the correct connector
            foreach (ElementId id in selectedIds)
            {
                Element e = document.GetElement(id);
                if (e is FamilyInstance)
                {
                    FamilyInstance fi = e as FamilyInstance;
                    Family family = fi.Symbol.Family;
                    if (family.FamilyCategory.Name == "Duct Accessories")
                    {
                        csi = fi.MEPModel.ConnectorManager.Connectors.ForwardIterator();
                        while (csi.MoveNext())
                        {
                            Connector conn = csi.Current as Connector;
                            MessageBox.Show(conn.DuctSystemType.ToString());
                            if (conn.Direction == FlowDirectionType.Out &&
                                conn.DuctSystemType == DuctSystemType.SupplyAir)
                            {
                                connector1 = conn;
                                levelId = family.LevelId;
                                break;
                            }
                        }
                    }
                }
            }
            // next find the second selected item to connect to
            foreach (ElementId id in selectedIds)
            {
                Element e = document.GetElement(id);
                if (e is FamilyInstance)
                {
                    FamilyInstance fi = e as FamilyInstance;
                    Family family = fi.Symbol.Family;
                    if (family.FamilyCategory.Name != "Duct Accessories")
                    {
                        csi = fi.MEPModel.ConnectorManager.Connectors.ForwardIterator();
                        while (csi.MoveNext())
                        {
                            if (null == connector2)
                            {
                                Connector conn = csi.Current as Connector;

                                //make sure to choose the connector in line with the first connector
                                if (Math.Abs(conn.Origin.Y - connector1.Origin.Y) < 0.001)
                                {
                                    connector2 = conn;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Duct duct = null;
            if (null != connector1 && null != connector2 && levelId != ElementId.InvalidElementId)
            {
                // find a duct type
                FilteredElementCollector collector = new FilteredElementCollector(uiDocument.Document);
                collector.OfClass(typeof(DuctType));

                // Use Linq query to make sure it is one of the rectangular duct types
                var query = from element in collector
                            where element.Name.Contains("Mitered Elbows") == true
                            select element;

                // use extension methods to get first duct type
                DuctType ductType = collector.OfCategory(BuiltInCategory.OST_DuctFitting).Cast<DuctType>().First<DuctType>();

                if (null != ductType)
                {
                    duct = Duct.Create(document, ductType.Id, levelId, connector1, connector2);
                }
            }

            return duct;
        }
    }
}