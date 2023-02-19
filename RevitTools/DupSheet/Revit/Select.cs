using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace DupSheet.Revit
{
    public class Select
    {
        private Select() { }
        public static Select instance { get; } = new Select();

        public List<ViewSheet> GetAllViewSheet(Document doc)
        {
            return new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).OfType<ViewSheet>().ToList();             
        }
        public List<ScheduleSheetInstance> GetAllViewSchedule(Document doc)
        {
            return new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_ScheduleGraphics).OfType<ScheduleSheetInstance>().ToList();
        }
    }
}
