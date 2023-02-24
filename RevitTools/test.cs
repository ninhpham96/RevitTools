//// Decompiled with JetBrains decompiler
//// Type: ricaun.EasyConduit.ConduiteNow
//// Assembly: ricaun.EasyConduit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
//// MVID: BC26673B-5188-4622-9AD6-21AE883165DC
//// Assembly location: C:\Users\Admin\Downloads\RevitEasyConduit-master\RevitEasyConduit-master\ricaun.EasyConduit.bundle\Contents\ricaun.EasyConduit.dll

//using Autodesk.Revit.DB;
//using Autodesk.Revit.DB.Electrical;
//using Autodesk.Revit.UI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ricaun.EasyConduit
//{
//    public class ConduiteNow
//    {
//        public UIDocument uidoc = (UIDocument)null;
//        public Document doc = (Document)null;
//        public bool show = false;
//        public string service = "";
//        public double c_size = 0.065;
//        public ElementId c_id = ElementId.InvalidElementId;
//        public StringBuilder message = new StringBuilder();
//        private static Dictionary<ElementId, XYZ> DicOriginId = new Dictionary<ElementId, XYZ>();

//        private bool Check => App.Check;

//        private bool Licence => App.Licence;

//        public void Show()
//        {
//            TaskDialog taskDialog = new TaskDialog("Error");
//            taskDialog.MainContent = this.message.ToString();
//            if (!(taskDialog.MainContent != ""))
//                return;
//            taskDialog.Show();
//        }

//        public void ActiveUIDocument(UIDocument ui)
//        {
//            this.uidoc = ui;
//            this.doc = this.uidoc.Document;
//        }

//        private ElementArray SelectionObjects()
//        {
//            Document document = this.uidoc.Document;
//            Autodesk.Revit.UI.Selection.Selection selection = this.uidoc.Selection;
//            ElementArray elementArray = new ElementArray();
//            if (selection.GetElementIds().Count > 0)
//            {
//                foreach (ElementId elementId in (IEnumerable<ElementId>)selection.GetElementIds())
//                    elementArray.Append(document.GetElement(elementId));
//            }
//            else
//            {
//                try
//                {
//                    foreach (Element element in (IEnumerable<Element>)selection.PickElementsByRectangle())
//                        elementArray.Append(element);
//                }
//                catch (Exception ex)
//                {
//                }
//            }
//            return elementArray;
//        }

//        private static ConnectorSet GetConnectors(Element element)
//        {
//            if (element == null)
//                return (ConnectorSet)null;
//            try
//            {
//                if (element is FamilyInstance familyInstance && familyInstance.MEPModel != null)
//                    return familyInstance.MEPModel.ConnectorManager.Connectors;
//                switch (element)
//                {
//                    case MEPSystem mepSystem:
//                        return mepSystem.ConnectorManager.Connectors;
//                    case MEPCurve mepCurve:
//                        return mepCurve.ConnectorManager.Connectors;
//                }
//            }
//            catch (Exception ex)
//            {
//                App.Error(ex.ToString());
//            }
//            return (ConnectorSet)null;
//        }

//        private static ConnectorSet GetConnectorsNotConnected(Element element)
//        {
//            ConnectorSet connectorSet = new ConnectorSet();
//            ConnectorSet connectors = ConduiteNow.GetConnectors(element);
//            if (connectors != null)
//            {
//                foreach (Connector connector in connectors)
//                {
//                    if (connector.Domain == 4 && connector.Shape == null && !connector.IsConnected)
//                        connectorSet.Insert(connector);
//                }
//            }
//            return connectorSet.IsEmpty ? (ConnectorSet)null : connectorSet;
//        }

//        private void ShowConnectors()
//        {
//            Document document = this.uidoc.Document;
//            StringBuilder stringBuilder1 = new StringBuilder();
//            ElementArray elementArray = new ElementArray();
//            foreach (Element selectionObject in this.SelectionObjects())
//            {
//                if (selectionObject is FamilyInstance familyInstance)
//                    elementArray.Append((Element)familyInstance);
//            }
//            if (elementArray.Size > 0)
//            {
//                foreach (FamilyInstance familyInstance in elementArray)
//                {
//                    BoundingBoxXYZ boundingBoxXyz = ((Element)familyInstance)[document.ActiveView];
//                    ConnectorSetIterator connectorSetIterator = familyInstance.MEPModel.ConnectorManager.Connectors.ForwardIterator();
//                    while (connectorSetIterator.MoveNext())
//                    {
//                        Connector current1 = connectorSetIterator.Current as Connector;
//                        ConnectorElement current2 = connectorSetIterator.Current as ConnectorElement;
//                        ConnectorProfileType shape;
//                        if (current1.Domain == 4 && current1.ConnectorType == 1)
//                        {
//                            StringBuilder stringBuilder2 = stringBuilder1;
//                            object[] objArray = new object[6];
//                            objArray[0] = (object)current1.Domain.ToString();
//                            shape = current1.Shape;
//                            objArray[1] = (object)shape.ToString();
//                            objArray[2] = (object)current1.Description.ToString();
//                            objArray[3] = (object)((object)current1.Origin).ToString();
//                            objArray[4] = (object)((object)current1.CoordinateSystem.BasisZ).ToString();
//                            objArray[5] = (object)current1.IsConnected.ToString();
//                            stringBuilder2.AppendFormat("{0} {1} {2}\n {3}\n {4} {5}\n", objArray);
//                        }
//                        if (current1.Domain == 4 && current1.ConnectorType == 32)
//                        {
//                            StringBuilder stringBuilder3 = stringBuilder1;
//                            object[] objArray = new object[6];
//                            objArray[0] = (object)current1.Domain.ToString();
//                            shape = current1.Shape;
//                            objArray[1] = (object)shape.ToString();
//                            objArray[2] = (object)current1.Description.ToString();
//                            objArray[3] = (object)0;
//                            objArray[4] = (object)0;
//                            objArray[5] = (object)0;
//                            stringBuilder3.AppendFormat("{0} {1} {2}\n {3}\n {4} {5}\n", objArray);
//                            foreach (Element element in (IEnumerable<Element>)this.uidoc.Selection.PickElementsByRectangle())
//                            {
//                                if (element is Conduit conduit)
//                                {
//                                    foreach (Connector connector in ((MEPCurve)conduit).ConnectorManager.Connectors)
//                                    {
//                                        try
//                                        {
//                                            connector.ConnectTo(current1);
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            TaskDialog.Show("Show", stringBuilder1.ToString());
//        }

//        private void DisconnectAll()
//        {
//            ElementArray elementArray = this.SelectionObjects();
//            Transaction transaction = new Transaction(this.uidoc.Document);
//            transaction.Start("connect");
//            foreach (Element element1 in elementArray)
//            {
//                foreach (Element element2 in elementArray)
//                {
//                    if (!((object)element1).Equals((object)element2))
//                    {
//                        foreach (Connector connector1 in ConduiteNow.GetConnectors(element1))
//                        {
//                            foreach (Connector connector2 in ConduiteNow.GetConnectors(element2))
//                            {
//                                try
//                                {
//                                    if (connector1.IsConnected)
//                                        connector1.DisconnectFrom(connector2);
//                                }
//                                catch (Exception ex)
//                                {
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            transaction.Commit();
//        }

//        private void ConnectAll()
//        {
//            Document document = this.uidoc.Document;
//            StringBuilder stringBuilder = new StringBuilder();
//            ElementArray elementArray = this.SelectionObjects();
//            Transaction transaction = new Transaction(this.uidoc.Document);
//            transaction.Start("connect");
//            foreach (Element element1 in elementArray)
//            {
//                foreach (Element element2 in elementArray)
//                {
//                    if (!((object)element1).Equals((object)element2))
//                    {
//                        foreach (Connector connector1 in ConduiteNow.GetConnectors(element1))
//                        {
//                            foreach (Connector connector2 in ConduiteNow.GetConnectors(element2))
//                            {
//                                try
//                                {
//                                    if (connector1.ConnectorType == 1 && !connector1.IsConnected)
//                                        document.Create.NewTransitionFitting(connector1, connector2);
//                                    if (connector1.ConnectorType == 16 && !connector1.IsConnected)
//                                    {
//                                        (element1 as FamilyInstance).MEPModel.ConnectorManager.UnusedConnectors.Clear();
//                                        if (((APIObject)(element1 as FamilyInstance).MEPModel.ConnectorManager.UnusedConnectors).IsReadOnly)
//                                            stringBuilder.AppendLine("IsReadOnly " + element1.Name + " " + connector1.ConnectorType.ToString());
//                                    }
//                                    if (connector1.ConnectorType != 32)
//                                        ;
//                                }
//                                catch (Exception ex)
//                                {
//                                    stringBuilder.AppendLine("Er: " + ex.ToString());
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            transaction.Commit();
//            new TaskDialog("Error")
//            {
//                MainContent = stringBuilder.ToString()
//            }.Show();
//        }

//        private void ElementMEPModel()
//        {
//            Document document = this.uidoc.Document;
//            StringBuilder stringBuilder = new StringBuilder();
//            foreach (Element selectionObject in this.SelectionObjects())
//            {
//                if (selectionObject is FamilyInstance familyInstance && familyInstance.MEPModel != null)
//                {
//                    ConnectorSetIterator connectorSetIterator = familyInstance.MEPModel.ConnectorManager.Connectors.ForwardIterator();
//                    while (connectorSetIterator.MoveNext())
//                    {
//                        Connector current = connectorSetIterator.Current as Connector;
//                        stringBuilder.AppendLine("ConnectorType: " + current.ConnectorType.ToString());
//                    }
//                }
//            }
//            new TaskDialog(string.Format("Traverse {0} MEP System{1}", (object)1, (object)2))
//            {
//                MainContent = stringBuilder.ToString()
//            }.Show();
//        }

//        private void ElementMEPSystem()
//        {
//            FilteredElementCollector elementCollector = new FilteredElementCollector(this.uidoc.Document).OfClass(typeof(MEPSystem));
//            StringBuilder stringBuilder = new StringBuilder();
//            foreach (MEPSystem mepSystem in elementCollector)
//            {
//                stringBuilder.AppendLine("System Name: " + ((Element)mepSystem).Name);
//                stringBuilder.AppendLine("Id: " + ((object)((Element)mepSystem).Id).ToString());
//            }
//            new TaskDialog(string.Format("Traverse {0} MEP System{1}", (object)1, (object)2))
//            {
//                MainContent = stringBuilder.ToString()
//            }.Show();
//        }

//        private void AutoConnect_original_plano(XYZ xyz1) => this.AutoConnect_original_plano(xyz1, xyz1);

//        private void AutoConnect_original_plano(XYZ xyz1, XYZ xyz2)
//        {
//            ConnectorSet connectorSet1 = (ConnectorSet)null;
//            ConnectorSet connectorSet2 = (ConnectorSet)null;
//            foreach (Element selectionObject in this.SelectionObjects())
//            {
//                ConnectorSet connectorsNotConnected = ConduiteNow.GetConnectorsNotConnected(selectionObject);
//                if (connectorsNotConnected != null)
//                {
//                    if (connectorSet1 == null)
//                        connectorSet1 = connectorsNotConnected;
//                    else if (connectorSet2 == null)
//                        connectorSet2 = connectorsNotConnected;
//                    else
//                        break;
//                }
//            }
//            if (connectorSet1 == null || connectorSet2 == null)
//                return;
//            Connector c1 = (Connector)null;
//            Connector c2 = (Connector)null;
//            foreach (Connector connector in connectorSet1)
//            {
//                if (connector.CoordinateSystem.BasisZ.DotProduct(xyz1) == 1.0)
//                {
//                    c1 = connector;
//                    break;
//                }
//            }
//            foreach (Connector connector in connectorSet2)
//            {
//                if (connector.CoordinateSystem.BasisZ.DotProduct(xyz2) == 1.0)
//                {
//                    c2 = connector;
//                    break;
//                }
//            }
//            if (c1 == null || c2 == null)
//                return;
//            this.AutoConnect(c1, c2);
//        }

//        private bool AutoConnectOpenDialog()
//        {
//            Units units = this.uidoc.Document.GetUnits();
//            double cSize = this.c_size;
//            string str = UnitFormatUtils.Format(units, (UnitType)83, cSize, false, false);
//            ConduitDialog conduitDialog = new ConduitDialog();
//            conduitDialog.FormCaption = "EasyConduit";
//            conduitDialog.DefaultValue = str;
//            conduitDialog.ComboItems = this.GetConduitNames();
//            conduitDialog.ComboIndex = this.GetConduitByNameInt(this.c_id);
//            conduitDialog.ChangeSize(this.GetStringMax(this.GetConduitNames()) * 7);
//            int num1 = (int)conduitDialog.ShowDialog();
//            string inputResponse = conduitDialog.InputResponse;
//            string itemResponse = conduitDialog.ItemResponse;
//            conduitDialog.Close();
//            if (inputResponse == "")
//                return false;
//            double num2 = 0.0;
//            if (UnitFormatUtils.TryParse(units, (UnitType)83, inputResponse, ref num2))
//                this.c_size = num2;
//            this.c_id = this.GetConduitByName(itemResponse);
//            return true;
//        }

//        public void AutoConnect_original()
//        {
//            if (this.Check || !this.Licence)
//                return;
//            Document document = this.uidoc.Document;
//            Autodesk.Revit.UI.Selection.Selection selection = this.uidoc.Selection;
//            if (document.IsFamilyDocument)
//                return;
//            if (selection.GetElementIds().Count == 0)
//            {
//                if (!this.AutoConnectOpenDialog())
//                    ;
//            }
//            else
//            {
//                Transaction transaction = new Transaction(document);
//                transaction.Start(App.Local.name);
//                this.AutoConnect();
//                transaction.Commit();
//            }
//        }

//        private bool AutoConnect_oldoldold()
//        {
//            StringBuilder stringBuilder = new StringBuilder();
//            List<FamilyInstance> familyInstanceList1 = new List<FamilyInstance>();
//            foreach (Element selectionObject in this.SelectionObjects())
//            {
//                if (selectionObject is FamilyInstance && ConduiteNow.GetConnectorsNotConnected(selectionObject) != null)
//                    familyInstanceList1.Add(selectionObject as FamilyInstance);
//            }
//            if (familyInstanceList1.Count < 2)
//                return false;
//            familyInstanceList1.Sort((Comparison<FamilyInstance>)((ff1, ff2) =>
//            {
//                XYZ connectorsNotConnectedXyz1 = ConduiteNow.GetConnectorsNotConnectedXYZ((Element)ff1);
//                XYZ connectorsNotConnectedXyz2 = ConduiteNow.GetConnectorsNotConnectedXYZ((Element)ff2);
//                return (int)(1000000.0 * (connectorsNotConnectedXyz2.X - connectorsNotConnectedXyz1.X + (connectorsNotConnectedXyz2.Y - connectorsNotConnectedXyz1.Y) + (connectorsNotConnectedXyz2.Z - connectorsNotConnectedXyz1.Z) * 10.0));
//            }));
//            List<FamilyInstance> familyInstanceList2 = new List<FamilyInstance>();
//            familyInstanceList2.Add(familyInstanceList1[0]);
//            familyInstanceList1.Remove(familyInstanceList1[0]);
//            for (int index = 0; index < 64; ++index)
//            {
//                double num = -1.0;
//                FamilyInstance e1 = (FamilyInstance)null;
//                FamilyInstance e2 = (FamilyInstance)null;
//                foreach (FamilyInstance f2 in familyInstanceList1)
//                {
//                    foreach (FamilyInstance f1 in familyInstanceList2)
//                    {
//                        double distanceFamilyInstance = this.getDistanceFamilyInstance(f1, f2);
//                        if (num == -1.0 || distanceFamilyInstance <= num)
//                        {
//                            e1 = f1;
//                            e2 = f2;
//                            num = distanceFamilyInstance;
//                        }
//                    }
//                }
//                if (e1 != null && e2 != null)
//                {
//                    familyInstanceList1.Remove(e2);
//                    familyInstanceList2.Add(e2);
//                    this.AutoConnect_original((Element)e1, (Element)e2);
//                    stringBuilder.AppendLine("> Id: " + ((object)((Element)e1).Id).ToString());
//                    stringBuilder.AppendLine("> To Id: " + ((object)((Element)e2).Id).ToString());
//                    stringBuilder.AppendLine("> Dist: " + num.ToString());
//                }
//            }
//            new TaskDialog("Balbal").MainContent = stringBuilder.ToString();
//            return true;
//        }

//        private bool AutoConnect()
//        {
//            DateTime dateTime1 = new DateTime(DateTime.Now.Ticks);
//            StringBuilder stringBuilder = new StringBuilder();
//            List<Element> elementList1 = new List<Element>();
//            foreach (Element selectionObject in this.SelectionObjects())
//            {
//                if (ConduiteNow.GetConnectorsNotConnected(selectionObject) != null)
//                    elementList1.Add(selectionObject);
//            }
//            if (elementList1.Count < 2)
//                return false;
//            elementList1.Sort((Comparison<Element>)((ee1, ee2) => ee1.Id.IntegerValue - ee2.Id.IntegerValue));
//            ConduiteNow.StartDicOrigin(elementList1);
//            elementList1.Sort((Comparison<Element>)((ee1, ee2) => (int)(1000000.0 * this.getDistanceElementToElement(ee1, ee2))));
//            while (((IEnumerable<Element>)elementList1).Count<Element>() > 32)
//                elementList1.RemoveAt(((IEnumerable<Element>)elementList1).Count<Element>() - 1);
//            List<Element> elementList2 = new List<Element>();
//            elementList2.Add(elementList1[0]);
//            elementList1.Remove(elementList1[0]);
//            for (int index = 0; index < 32; ++index)
//            {
//                double num = -1.0;
//                Element e1_1 = (Element)null;
//                Element e2_1 = (Element)null;
//                foreach (Element e2_2 in elementList1)
//                {
//                    foreach (Element e1_2 in elementList2)
//                    {
//                        double elementToElement = this.getDistanceElementToElement(e1_2, e2_2);
//                        if (num == -1.0 || elementToElement <= num)
//                        {
//                            e1_1 = e1_2;
//                            e2_1 = e2_2;
//                            num = elementToElement;
//                        }
//                    }
//                }
//                if (e1_1 != null && e2_1 != null)
//                {
//                    elementList1.Remove(e2_1);
//                    elementList2.Add(e2_1);
//                    this.AutoConnect_original(e1_1, e2_1);
//                    stringBuilder.AppendLine(">>>> " + ((object)e1_1.Id).ToString() + " " + ((object)e2_1.Id).ToString() + " " + num.ToString());
//                }
//            }
//            new TaskDialog("Balbal").MainContent = stringBuilder.ToString();
//            DateTime dateTime2 = new DateTime(DateTime.Now.Ticks);
//            return true;
//        }

//        private void AutoConnect_original(Element e1, Element e2)
//        {
//            ConnectorSet connectorsNotConnected1 = ConduiteNow.GetConnectorsNotConnected(e1);
//            ConnectorSet connectorsNotConnected2 = ConduiteNow.GetConnectorsNotConnected(e2);
//            if (connectorsNotConnected1 == null || connectorsNotConnected2 == null)
//                return;
//            List<ConduiteNow.Connectores> connectoresList = new List<ConduiteNow.Connectores>();
//            foreach (Connector c1 in connectorsNotConnected1)
//            {
//                foreach (Connector c2 in connectorsNotConnected2)
//                    connectoresList.Add(new ConduiteNow.Connectores(c1, c2, e1, e2));
//            }
//            connectoresList.Sort((Comparison<ConduiteNow.Connectores>)((con1, con2) => (int)(1000000.0 * (con1.distance - con2.distance))));
//            foreach (ConduiteNow.Connectores connectores in connectoresList)
//            {
//                Connector connector1 = connectores.connector1;
//                Connector connector2 = connectores.connector2;
//                double distance = connectores.distance;
//                for (int index = 1; index < 100; ++index)
//                {
//                    List<XYZ> list = this.ConnectToList(connector1, connector2, distance * (double)index / 100.0);
//                    if (list != null)
//                    {
//                        this.ListToConduit(list, connector1, connector2);
//                        return;
//                    }
//                }
//            }
//        }

//        private void AutoConnect(Connector c1, Connector c2)
//        {
//            List<XYZ> listXyz = this.getListXYZ(c1, c2);
//            if (listXyz == null)
//                return;
//            this.ListToConduit(listXyz, c1, c2);
//        }

//        private List<XYZ> getListXYZ(Connector c1, Connector c2)
//        {
//            for (int index = 1; index < 100; ++index)
//            {
//                List<XYZ> list = this.ConnectToList(c1, c2, (double)index / 0.3048 / 10.0);
//                if (list != null)
//                    return list;
//            }
//            return this.ConnectToListOther(c1, c2);
//        }

//        private List<XYZ> ConnectToListOther(Connector c1, Connector c2, double d = 0.328083989501312)
//        {
//            List<XYZ> listOther = new List<XYZ>();
//            XYZ xyz1 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(d));
//            XYZ xyz2 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(d));
//            if (this.PointToAngle(c1.Origin, xyz1, xyz2) > 1.5707963277949 || this.PointToAngle(xyz1, xyz2, c2.Origin) > 1.5707963277949 || xyz1.DistanceTo(xyz2) < d && !xyz1.IsAlmostEqualTo(xyz2))
//                return (List<XYZ>)null;
//            listOther.Add(c1.Origin);
//            listOther.Add(xyz1);
//            listOther.Add(xyz2);
//            listOther.Add(c2.Origin);
//            return listOther;
//        }

//        private List<XYZ> ConnectToList(Connector c1, Connector c2, double d = 0.1)
//        {
//            List<XYZ> list = new List<XYZ>();
//            double num1 = this.LineToLine(c1, c2);
//            double num2 = this.LineToLine(c2, c1);
//            if (num1 > 10000000000000.0 || num2 > 10000000000000.0)
//            {
//                num1 = 0.0;
//                num2 = 0.0;
//            }
//            if (num1 == 0.0)
//            {
//                double num3 = this.LineToPlane(c1, c2) / 2.0;
//                double num4 = this.LineToPlane(c2, c1) / 2.0;
//                if (num3 == 0.0 || num4 == 0.0)
//                {
//                    num3 = d;
//                    num4 = d;
//                }
//                if (num3 < 0.0)
//                {
//                    num4 += -num3 + 1.0 * d;
//                    num3 = 1.0 * d;
//                }
//                else if (num4 < 0.0)
//                {
//                    num3 += -num4 + 1.0 * d;
//                    num4 = 1.0 * d;
//                }
//                XYZ p2 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(num3));
//                XYZ p3 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(num4));
//                if (p2.DistanceTo(p3) < d && !p2.IsAlmostEqualTo(p3))
//                {
//                    double num5 = num3 - d / 2.0;
//                    double num6 = num4 - d / 2.0;
//                    p2 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(num5));
//                    p3 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(num6));
//                }
//                else
//                {
//                    if (this.PointToAngle(c1.Origin, p2, p3) > 1.5707963277949)
//                        return (List<XYZ>)null;
//                    if (num3 > d && num4 > d || p2.DistanceTo(p3) > d && num4 == num3)
//                    {
//                        double num7 = d;
//                        double num8 = d;
//                        p2 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(num7));
//                        p3 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(num8));
//                    }
//                }
//                list.Add(c1.Origin);
//                list.Add(p2);
//                list.Add(p3);
//                list.Add(c2.Origin);
//            }
//            else
//            {
//                if (num1 < 0.0 && num2 < 0.0 || num1 < 0.0 || num2 < 0.0)
//                    return (List<XYZ>)null;
//                XYZ xyz1 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(num1));
//                XYZ xyz2 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(num2));
//                if (xyz1.IsAlmostEqualTo(xyz2))
//                {
//                    list.Add(c1.Origin);
//                    list.Add(xyz1);
//                    list.Add(c2.Origin);
//                }
//                else
//                {
//                    XYZ xyz3 = xyz1;
//                    XYZ xyz4 = xyz2;
//                    double num9 = c1.CoordinateSystem.BasisZ.DotProduct(new XYZ(0.0, 0.0, 1.0));
//                    double num10 = c2.CoordinateSystem.BasisZ.DotProduct(new XYZ(0.0, 0.0, 1.0));
//                    if (num9 == 0.0 && num10 == 0.0)
//                    {
//                        if (c1.Origin.Z > c2.Origin.Z)
//                            xyz2 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(d));
//                        else
//                            xyz1 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(d));
//                    }
//                    else if (num9 == 0.0)
//                        xyz1 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(d));
//                    else if (num10 == 0.0)
//                        xyz2 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(d));
//                    if (xyz1.DistanceTo(xyz2) < 0.05)
//                        this.message.AppendLine("<><><><>D " + (object)(d * 3.048) + ": " + xyz1.DistanceTo(xyz2).ToString());
//                    this.message.AppendLine("<<<<<<<<<D " + (object)(d * 3.048) + ": " + (object)xyz1.DistanceTo(c1.Origin) + " | " + (object)xyz3.DistanceTo(c1.Origin));
//                    this.message.AppendLine("<<<<<<<<<D " + (object)(d * 3.048) + ": " + (object)xyz2.DistanceTo(c2.Origin) + " | " + (object)xyz4.DistanceTo(c2.Origin));
//                    if (xyz1.GetLength() <= xyz3.GetLength())
//                        ;
//                    if (xyz2.GetLength() <= xyz4.GetLength())
//                        ;
//                    list.Add(c1.Origin);
//                    list.Add(xyz1);
//                    list.Add(xyz2);
//                    list.Add(c2.Origin);
//                }
//            }
//            if (list.Count == 0)
//                return (List<XYZ>)null;
//            for (int index = 0; index < list.Count - 1; ++index)
//            {
//                XYZ xyz5 = list[index];
//                XYZ xyz6 = list[index + 1];
//                if (xyz5.IsAlmostEqualTo(xyz6))
//                {
//                    list.Remove(xyz5);
//                    --index;
//                }
//                else if (xyz5.DistanceTo(xyz6) < 0.05)
//                    return (List<XYZ>)null;
//            }
//            for (int index = 0; index < list.Count - 2; ++index)
//            {
//                XYZ p1 = list[index];
//                XYZ p2 = list[index + 1];
//                XYZ p3 = list[index + 2];
//                XYZ xyz7 = p2.Subtract(p1).Normalize();
//                XYZ xyz8 = p3.Subtract(p2).Normalize();
//                double angle = this.PointToAngle(p1, p2, p3);
//                if (xyz8.DotProduct(xyz7) == 1.0)
//                {
//                    list.Remove(p2);
//                    --index;
//                }
//                else
//                {
//                    if (angle < 0.0872664635997165)
//                    {
//                        this.message.AppendLine("D " + (object)(d * 3.048) + ": 3deg " + (object)(angle / Math.PI));
//                        return (List<XYZ>)null;
//                    }
//                    if (0.0005 > Math.Tan(angle / 2.0) * this.c_size * 3.048 / 2.0 + 1E-09)
//                        return (List<XYZ>)null;
//                    if (angle > 1.5707963277949)
//                    {
//                        this.message.AppendLine("D " + (object)(d * 3.048) + ": 90deg" + (object)(angle / Math.PI));
//                        return (List<XYZ>)null;
//                    }
//                }
//            }
//            return list;
//        }

//        private double PointToAngle(XYZ p1, XYZ p2, XYZ p3) => p2.Subtract(p1).Normalize().AngleTo(p3.Subtract(p2).Normalize());

//        private List<XYZ> ListTo(List<XYZ> points)
//        {
//            double num1 = 1E-09;
//            for (int index = 0; index < points.Count - 1; ++index)
//            {
//                XYZ point1 = points[index];
//                XYZ point2 = points[index + 1];
//                if (point1.IsAlmostEqualTo(point2))
//                {
//                    points.Remove(point1);
//                    --index;
//                }
//            }
//            for (int index = 0; index < points.Count - 2; ++index)
//            {
//                XYZ point3 = points[index];
//                XYZ point4 = points[index + 1];
//                XYZ point5 = points[index + 2];
//                XYZ xyz1 = point4.Subtract(point3).Normalize();
//                XYZ xyz2 = point5.Subtract(point4).Normalize();
//                double angle = this.PointToAngle(point3, point4, point5);
//                if (xyz2.DotProduct(xyz1) == 1.0)
//                {
//                    points.Remove(point4);
//                    --index;
//                }
//                else
//                {
//                    if (angle > Math.PI - num1)
//                        return (List<XYZ>)null;
//                    if (angle > Math.PI / 2.0 + num1)
//                    {
//                        double num2 = 125.0 / 381.0;
//                        double val1 = point3.DistanceTo(point4);
//                        double val2 = point4.DistanceTo(point5);
//                        if (val1 > num2 && val2 > num2)
//                        {
//                            double num3 = Math.Min(val1, val2) - num2;
//                            double num4 = val1 - num3;
//                            double num5 = num3;
//                            points.Remove(point4);
//                            XYZ xyz3 = point3.Add(xyz1.Multiply(num4));
//                            XYZ xyz4 = point4.Add(xyz2.Multiply(num5));
//                            points.Insert(index + 1, xyz4);
//                            points.Insert(index + 1, xyz3);
//                            ++index;
//                        }
//                    }
//                }
//            }
//            return points;
//        }

//        private void ListToConduit(List<XYZ> points, Connector con1, Connector con2)
//        {
//            Document document = this.uidoc.Document;
//            ElementId invalidElementId = ElementId.InvalidElementId;
//            List<Conduit> conduitList = new List<Conduit>();
//            if (points.Count <= 1)
//                return;
//            for (int index = 0; index < points.Count - 1; ++index)
//            {
//                XYZ point1 = points[index];
//                XYZ point2 = points[index + 1];
//                if (!point1.IsAlmostEqualTo(point2))
//                {
//                    Conduit conduit = Conduit.Create(document, this.c_id, point1, point2, invalidElementId);
//                    if (this.c_size > 0.0)
//                        this.ChangeConduitSize(conduit, this.c_size);
//                    if (this.service != "")
//                        this.ChangeConduitService((Element)conduit, this.service);
//                    conduitList.Add(conduit);
//                }
//            }
//            if (conduitList.Count > 1)
//            {
//                for (int index = 0; index < conduitList.Count - 1; ++index)
//                {
//                    Conduit c1 = conduitList[index];
//                    Conduit conduit = conduitList[index + 1];
//                    this.ConduitConnector(c1, conduit);
//                    if (index == 0)
//                        this.ConduitConnector(c1, con1);
//                    this.ConduitConnector(conduit, con2);
//                }
//            }
//            else if (conduitList.Count == 1)
//            {
//                Conduit c1 = conduitList[0];
//                this.ConduitConnector(c1, con1);
//                this.ConduitConnector(c1, con2);
//            }
//        }

//        private double ListGetLength(List<XYZ> points)
//        {
//            double length = 0.0;
//            for (int index = 0; index < points.Count - 1; ++index)
//            {
//                XYZ point1 = points[index];
//                XYZ point2 = points[index + 1];
//                length += point1.DistanceTo(point2);
//            }
//            return length;
//        }

//        private double LineToPlane(XYZ lp, XYZ ld, XYZ pp, XYZ pd)
//        {
//            double plane = 0.0;
//            double num1 = ld.DotProduct(pd);
//            double num2 = lp.Subtract(pp).DotProduct(pd);
//            if (num1 != 0.0)
//                plane = -num2 / num1;
//            return plane;
//        }

//        private double LineToPlane(Connector c1, Connector c2) => this.LineToPlane(c1.CoordinateSystem.Origin, c1.CoordinateSystem.BasisZ, c2.CoordinateSystem.Origin, c2.CoordinateSystem.BasisZ);

//        private double LineToLine(XYZ lp, XYZ ld, XYZ pp, XYZ pd)
//        {
//            XYZ pd1 = pd.CrossProduct(ld).CrossProduct(pd);
//            return this.LineToPlane(lp, ld, pp, pd1);
//        }

//        private double LineToLine(Connector c1, Connector c2) => this.LineToLine(c1.CoordinateSystem.Origin, c1.CoordinateSystem.BasisZ, c2.CoordinateSystem.Origin, c2.CoordinateSystem.BasisZ);

//        private double ConnectorAngleBasis(Connector c1, Connector c2) => (double)(int)(c1.CoordinateSystem.BasisZ.AngleTo(c2.CoordinateSystem.BasisZ) * 10000.0 / 3.14159265 * 180.0) / 10000.0;

//        private double ConnectorAngleOrigin(Connector c1, Connector c2) => (double)(int)(c1.Origin.Subtract(c2.Origin).Normalize().AngleTo(c2.CoordinateSystem.BasisZ) * 10000.0 / 3.14159265 * 180.0) / 10000.0;

//        private double ConnectorRS(Connector c1, Connector c2) => c1.CoordinateSystem.BasisZ.CrossProduct(c2.CoordinateSystem.BasisZ).GetLength();

//        private double ConnectorQPR(Connector c1, Connector c2) => c2.CoordinateSystem.BasisZ.CrossProduct(c1.Origin.Subtract(c2.Origin)).GetLength();

//        private XYZ ConnectorIntersection(Connector c1, Connector c2)
//        {
//            if (this.ConnectorRS(c1, c2) != 0.0)
//            {
//                XYZ xyz1 = c1.Origin.Add(c1.CoordinateSystem.BasisZ.Multiply(this.ConnectorQPR(c1, c2) / this.ConnectorRS(c1, c2)));
//                XYZ xyz2 = c2.Origin.Add(c2.CoordinateSystem.BasisZ.Multiply(this.ConnectorQPR(c2, c1) / this.ConnectorRS(c2, c1)));
//                if (xyz1.IsAlmostEqualTo(xyz2))
//                    return xyz1;
//            }
//            return (XYZ)null;
//        }

//        private string GetConduitName(ElementId id) => this.GetConduitName(this.uidoc.Document.GetElement(id));

//        private string GetConduitName(Element e)
//        {
//            Parameter parameter = e[(BuiltInParameter) - 1002002];
//            return e.Name + " | " + parameter.AsString();
//        }

//        private object[] GetConduitNames()
//        {
//            FilteredElementCollector elementCollector = new FilteredElementCollector(this.uidoc.Document);
//            elementCollector.OfClass(typeof(ConduitType));
//            List<object> objectList = new List<object>();
//            foreach (Element element in (IEnumerable<Element>)elementCollector.ToElements())
//                objectList.Add((object)this.GetConduitName(element));
//            return objectList.ToArray();
//        }

//        private int GetStringMax(object[] o)
//        {
//            int val1 = 0;
//            foreach (object obj in o)
//                val1 = Math.Max(val1, obj.ToString().Length);
//            return val1;
//        }

//        private ElementId GetConduitByName(string str)
//        {
//            FilteredElementCollector elementCollector = new FilteredElementCollector(this.uidoc.Document);
//            elementCollector.OfClass(typeof(ConduitType));
//            foreach (Element element in (IEnumerable<Element>)elementCollector.ToElements())
//            {
//                if (this.GetConduitName(element) == str)
//                    return element.Id;
//            }
//            return ElementId.InvalidElementId;
//        }

//        private int GetConduitByNameInt(ElementId id)
//        {
//            FilteredElementCollector elementCollector = new FilteredElementCollector(this.uidoc.Document);
//            elementCollector.OfClass(typeof(ConduitType));
//            int conduitByNameInt = 0;
//            foreach (Element element in (IEnumerable<Element>)elementCollector.ToElements())
//            {
//                if (ElementId.op_Equality(element.Id, id))
//                    return conduitByNameInt;
//                ++conduitByNameInt;
//            }
//            return 0;
//        }

//        private void ChangeConduitSize(Conduit cond, double dim = 0.16) => ((Element)cond)[(BuiltInParameter) - 1140123].Set(dim);

//        private void ChangeConduitService(Element ele, string str) => ele[(BuiltInParameter) - 1140128]?.Set(str);

//        private bool ConduitConnector(Conduit c1, Conduit c2)
//        {
//            ConnectorSet connectors1 = ((MEPCurve)c1).ConnectorManager.Connectors;
//            ConnectorSet connectors2 = ((MEPCurve)c2).ConnectorManager.Connectors;
//            foreach (Connector connector1 in connectors1)
//            {
//                foreach (Connector connector2 in connectors2)
//                {
//                    if (connector1.Origin.IsAlmostEqualTo(connector2.Origin))
//                    {
//                        try
//                        {
//                            ((Element)c1).Document.Create.NewElbowFitting(connector1, connector2);
//                            return true;
//                        }
//                        catch (Exception ex)
//                        {
//                            TaskDialog.Show("Error", "Some fitting was too small to create.!");
//                            return true;
//                        }
//                    }
//                }
//            }
//            return false;
//        }

//        private bool ConduitConnector(Conduit c1, Connector c2)
//        {
//            foreach (Connector connector in ((MEPCurve)c1).ConnectorManager.Connectors)
//            {
//                if (connector.Origin.IsAlmostEqualTo(c2.Origin))
//                {
//                    try
//                    {
//                        connector.ConnectTo(c2);
//                        try
//                        {
//                            ((Element)c1).Document.Create.NewTransitionFitting(connector, c2);
//                        }
//                        catch (Exception ex)
//                        {
//                        }
//                        return true;
//                    }
//                    catch (Exception ex)
//                    {
//                    }
//                }
//            }
//            return false;
//        }

//        private static void StartDicOrigin(List<Element> eles)
//        {
//            ConduiteNow.DicOriginId.Clear();
//            foreach (Element ele in eles)
//                ConduiteNow.DicOriginId.Add(ele.Id, ConduiteNow.DicGetConnectorsNotConnectedXYZ(ele));
//            int num = 0;
//            XYZ xyz1 = new XYZ(0.0, 0.0, 0.0);
//            foreach (XYZ xyz2 in ConduiteNow.DicOriginId.Values)
//            {
//                ++num;
//                xyz1 = xyz1.Add(xyz2);
//            }
//            ConduiteNow.DicOriginId.Add(ElementId.InvalidElementId, xyz1.Divide((double)num));
//        }

//        private static XYZ GetConnectorsNotConnectedXYZ(Element ele) => ConduiteNow.DicOriginId[ele.Id];

//        private static XYZ GetConnectorsNotConnectedXYZ(ElementId ele) => ConduiteNow.DicOriginId[ele];

//        private static XYZ DicGetConnectorsNotConnectedXYZ(Element ele)
//        {
//            XYZ xyz = new XYZ(0.0, 0.0, 0.0);
//            int num = 0;
//            foreach (Connector connector in ConduiteNow.GetConnectorsNotConnected(ele))
//            {
//                ++num;
//                xyz = xyz.Add(connector.Origin);
//            }
//            return xyz.Divide((double)num);
//        }

//        private double getDistanceFamilyInstance(FamilyInstance f1, FamilyInstance f2)
//        {
//            XYZ connectorsNotConnectedXyz1 = ConduiteNow.GetConnectorsNotConnectedXYZ((Element)f1);
//            XYZ connectorsNotConnectedXyz2 = ConduiteNow.GetConnectorsNotConnectedXYZ((Element)f2);
//            double num1 = Math.Abs(connectorsNotConnectedXyz2.X - connectorsNotConnectedXyz1.X);
//            double num2 = Math.Abs(connectorsNotConnectedXyz2.Y - connectorsNotConnectedXyz1.Y);
//            double num3 = Math.Abs(connectorsNotConnectedXyz2.Z - connectorsNotConnectedXyz1.Z);
//            return num3 != 0.0 ? num3 + Math.Sqrt(num2 * num2 + num1 * num1) : num1 + num2;
//        }

//        private double getDistanceElementToElement(Element e1, Element e2)
//        {
//            XYZ connectorsNotConnectedXyz1 = ConduiteNow.GetConnectorsNotConnectedXYZ(ElementId.InvalidElementId);
//            XYZ connectorsNotConnectedXyz2 = ConduiteNow.GetConnectorsNotConnectedXYZ(e1);
//            XYZ connectorsNotConnectedXyz3 = ConduiteNow.GetConnectorsNotConnectedXYZ(e2);
//            double num1 = Math.Abs(connectorsNotConnectedXyz3.X - connectorsNotConnectedXyz2.X);
//            double num2 = Math.Abs(connectorsNotConnectedXyz3.Y - connectorsNotConnectedXyz2.Y);
//            double num3 = Math.Round(Math.Abs(connectorsNotConnectedXyz3.Z - connectorsNotConnectedXyz2.Z), 12);
//            double num4 = 0.0 + (connectorsNotConnectedXyz2.X + connectorsNotConnectedXyz3.X - connectorsNotConnectedXyz1.X) / 100000.0 + (connectorsNotConnectedXyz2.Y + connectorsNotConnectedXyz3.Y - connectorsNotConnectedXyz1.Y) / 10000.0 + (connectorsNotConnectedXyz2.Z + connectorsNotConnectedXyz3.Z - connectorsNotConnectedXyz1.Z) / 1000.0;
//            return Math.Round((num3 != 0.0 ? num3 + Math.Sqrt(num2 * num2 + num1 * num1) : num1 + num2) + num4, 12);
//        }

//        private class Connectores
//        {
//            public Connector connector1;
//            public Connector connector2;
//            public Element element1;
//            public Element element2;
//            public double distance;

//            public Connectores(Connector c1, Connector c2, Element e1, Element e2)
//            {
//                this.connector1 = c1;
//                this.connector2 = c2;
//                this.element1 = e1;
//                this.element2 = e2;
//                this.distance = this.getDistance();
//            }

//            private double getDistance_normal() => this.connector1.Origin.DistanceTo(this.connector2.Origin) + Math.Abs(this.connector1.Origin.Z - this.connector2.Origin.Z) / 0.1;

//            private double getDistance()
//            {
//                XYZ connectorsNotConnectedXyz1 = ConduiteNow.GetConnectorsNotConnectedXYZ(this.element1);
//                XYZ connectorsNotConnectedXyz2 = ConduiteNow.GetConnectorsNotConnectedXYZ(this.element2);
//                return this.getDistance_normal() + (this.connector1.Origin.X - connectorsNotConnectedXyz1.X + this.connector2.Origin.X - connectorsNotConnectedXyz2.X) / 100000.0 + (this.connector1.Origin.Y - connectorsNotConnectedXyz1.Y + this.connector2.Origin.Y - connectorsNotConnectedXyz2.Y) / 50000.0 + (this.connector1.Origin.Z - connectorsNotConnectedXyz1.Z + this.connector2.Origin.Z - connectorsNotConnectedXyz2.Z) / 10000.0;
//            }
//        }
//    }
//}
