using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    public class DimChCMD :DimensionBase
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            ReferenceArray refs = new ReferenceArray();

            var pickRef = uidoc.Selection.PickObjects(ObjectType.Face);

            foreach ( var obj in pickRef )
            {
                refs.Append(obj);
            }
            var point = uidoc.Selection.PickPoint(ObjectSnapTypes.None);
            Line line = Line.CreateUnbound(point, new XYZ(0, 0, 1).CrossProduct(new XYZ(0, 10, 0)));
            using (Transaction tran = new Transaction(doc, "dim"))
            {
                tran.Start();
                if (!doc.IsFamilyDocument)
                {
                    doc.Create.NewDimension(doc.ActiveView, line, refs);
                }
                tran.Commit();
            }
            return Result.Succeeded;
        }
    }
}
