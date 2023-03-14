using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CreateElevationMarker
{
    public partial class CreateElevationVM : ObservableObject
    {
        private UIDocument uidoc { get; }
        private Document doc { get; }
        List<Room> source;
        List<Room> selectedRooms = new List<Room>();
        List<ViewFamilyType> src;
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
                if (_view == null)
                    _view = new CreateElevationView() { DataContext = this };
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
            src = new List<ViewFamilyType>();

            Source = CreateElevationModel.instance.GetAllRooms(doc);
            src = CreateElevationModel.instance.GetAllViewfamilytype(doc);

            view.lsvCreateElevation.ItemsSource = Source;
            view.lsbview.ItemsSource = src;
            view.lsbview.SelectedIndex = 0;
        }

        [RelayCommand]
        void run()
        {
            foreach (var r in selectedRooms)
            {
                CreateElevationByRoom(r);
            }
        }
        [RelayCommand]
        void checkAll(Room room)
        {
            selectedRooms.AddRange(Source);
            check(true);

        }
        [RelayCommand]
        void checkNone(Room room)
        {
            selectedRooms.Clear();
            check(false);
        }
        void check(bool b)
        {
            foreach (var item in view.lsvCreateElevation.Items)
            {
                var container = view.lsvCreateElevation.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(container);
                    if (checkbox != null)
                    {
                        checkbox.IsChecked = b;
                    }
                }
            }
        }
        void CreateElevationByRoom(Room room)
        {
            ViewFamilyType viewFamilyType = view.lsbview.SelectedItem as ViewFamilyType;
            XYZ loca = (room.Location as LocationPoint).Point;
            using (Transaction tran = new Transaction(doc, "create"))
            {
                tran.Start();
                var maker = ElevationMarker.CreateElevationMarker(doc, viewFamilyType.Id, loca, 100);
                tran.Commit();
            }
        }
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    var result = FindVisualChild<T>(child);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }
}
