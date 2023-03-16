using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitTools
{
    public class GetData
    {
        private GetData() { }
        public static GetData instance { get; } = new GetData();
        public List<FamilySymbol> GetAllTagRoom(Document doc)
        {            
            var result = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>().Where(p=>(p.Family.FamilyCategory.Name == "Room Tags")).ToList();
            return result;
        }
    }
}
