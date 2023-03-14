using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.DB.Architecture;
using DupSheet.Revit;

namespace CreateElevationMarker
{
    public class CreateElevationModel
    {
        private CreateElevationModel() { }
        public static CreateElevationModel instance { get; } = new CreateElevationModel();

        public List<Room> GetAllRooms(Document doc)
        {
            return new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).OfType<Room>().ToList();
        }
        public List<ViewFamilyType> GetAllViewfamilytype(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .Where(p => (p as ViewFamilyType).FamilyName == "Elevation").ToList();
        }
    }

}
