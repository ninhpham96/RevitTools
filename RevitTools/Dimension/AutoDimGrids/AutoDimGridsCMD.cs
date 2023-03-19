using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    public class AutoDimGridsCMD : DimensionBase
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var grids = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_Grids).Cast<Grid>().ToList();

            using (TransactionGroup tranG = new TransactionGroup(doc))
            {
                tranG.Start("Auto Dim");
                AutoDim(uidoc, grids);
                tranG.Assimilate();
            }

            return Result.Succeeded;
        }
    }
}
