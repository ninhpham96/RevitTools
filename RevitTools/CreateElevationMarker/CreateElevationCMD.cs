using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CreateElevationMarker;

namespace CreateElevationMarker
{
    [Transaction(TransactionMode.Manual)]
    public class CreateElevationCMD : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            using (TransactionGroup tranG = new TransactionGroup(doc, "run"))
            {
                tranG.Start();
                CreateElevationVM createElevationVM = new CreateElevationVM(uiapp);
                createElevationVM.view.ShowDialog();
                tranG.Assimilate();
            }            
            return Result.Succeeded;
        }        
    }
}
