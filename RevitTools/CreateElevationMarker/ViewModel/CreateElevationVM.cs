using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace CreateElevationMarker
{
    public class CreateElevationVM: ObservableObject
    {
        private UIDocument uidoc { get; }
        private Document doc { get; }
        List<Room> source;
        public List<Room> Source
        {
            get { return source; }
            set { source = value; }
        }
        private CreateElevationView _view;
        public CreateElevationView view
        {
            get
            {
                if(_view == null)
                    _view = new CreateElevationView() { DataContext = this};
                return _view;
            }
            set
            {
                _view = value;
                OnPropertyChanged();
            }
        }
        public CreateElevationVM(UIApplication uiapp) 
        {
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;
            Source = new List<Room>();
            var rooms = CreateElevationModel.instance.GetAllRooms(doc);
            view.lsvCreateElevation.ItemsSource = rooms;
        }
    }
}
