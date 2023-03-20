using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    public class AutoDimDoubouchiCMD:DimensionBase
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //UIApplication uiapp = commandData.Application;
            //UIDocument uidoc = uiapp.ActiveUIDocument;
            //Document doc = uidoc.Document;

            //SeclectFamilyInstance seclectFamilyInstance = new SeclectFamilyInstance();
            //var familys = uidoc.Selection.PickElementsByRectangle(seclectFamilyInstance).Cast<FamilyInstance>().ToList();

            ////var grids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids)
            ////    .WhereElementIsNotElementType().Cast<Grid>().ToList();
            ////grids.Sort((gr1, gr2) =>
            ////{
            ////    int compareX = (gr1.Curve as Line).Origin.X.CompareTo((gr2.Curve as Line).Origin.X);
            ////    return compareX;
            ////});
            ////List<Grid> gr = new List<Grid>() { grids.First(), grids.Last() };
            //AutoDim(uidoc, familys, null);

            return Result.Succeeded;
        }
    }
    
}
