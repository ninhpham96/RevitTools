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
    public partial class CreateElevationVM: ObservableObject
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
        [ObservableProperty]
        private bool isChecked;
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

        [RelayCommand]
        void run(Room room)
        {

        }
        [RelayCommand]
        void checkAll(Room room)
        {
            foreach (var item in view.lsvCreateElevation.Items)
            {
                var container = view.lsvCreateElevation.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(container);
                    if (checkbox != null)
                    {
                        checkbox.IsChecked = true;
                    }
                }
            }
        }
        [RelayCommand]
        void checkNone(Room room)
        {
            foreach (var item in view.lsvCreateElevation.Items)
            {
                var container = view.lsvCreateElevation.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(container);
                    if (checkbox != null)
                    {
                        checkbox.IsChecked = false;
                    }
                }
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
