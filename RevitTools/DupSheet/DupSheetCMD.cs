using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using DupSheet.View;
using DupSheet.ViewModel;

[Transaction(TransactionMode.Manual)]
public class DupSheetCMD : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiApp = commandData.Application;
        Document doc = uiApp.ActiveUIDocument.Document;
        DupSheetViewModel dupSheetViewModel = new DupSheetViewModel(uiApp);
        dupSheetViewModel.DupSheetView.ShowDialog();
        return Result.Succeeded;
    }
}
