using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTools.Dimmension;
using System;
using System.Linq;
using System.Windows;

namespace AutoDimGrids
{
    [Transaction(TransactionMode.Manual)]
    public class Command : DimensionBase
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
