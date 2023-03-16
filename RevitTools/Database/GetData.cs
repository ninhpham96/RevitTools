using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitTools
{
    public class GetData
    {
        private GetData() { }
        public static GetData instance { get; } = new GetData();
        public List<RoomTagType> GetAllTagRoom(Document doc)
        {
            try
            {
                
                List<RoomTagType> result = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                        .Where(p => (p as FamilySymbol).Category.Name == "Room Tags").Cast<RoomTagType>().ToList();
                return result;
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }
    }
}
