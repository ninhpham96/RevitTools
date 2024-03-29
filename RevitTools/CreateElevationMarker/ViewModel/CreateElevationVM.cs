﻿using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
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
        #region constructor
        public CreateElevationVM(UIApplication uiapp)
        {
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            Source = new List<Room>();

            view.lsvCreateElevation.ItemsSource = Source = CreateElevationModel.instance.GetAllRooms(doc);
            view.lsbview.ItemsSource = CreateElevationModel.instance.GetAllViewfamilytype(doc);
            view.lsbview.SelectedIndex = 0;
        }
        #endregion
        #region command
        [RelayCommand]
        void run()
        {
            if (doc.ActiveView.GetType().Name == "View3D")
            {
                MessageBox.Show("Bạn cần chuyển về view 2D");
                return;
            }
            foreach (var r in selectedRooms)
            {
                CreateElevationByRoom(r);
            }
        }

        [RelayCommand]
        void click(Room room)
        {
            if (selectedRooms.Contains(room))
            {
                selectedRooms.Remove(room);
            }
            else
            {
                selectedRooms.Add(room);
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
        #endregion
        #region methods
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
                for (int i = 0; i < 4; i++)
                {
                    var newEle = maker.CreateElevation(doc, doc.ActiveView.Id, i);
                    newEle.Name = room.Name + i.ToString();
                }
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
        #endregion
    }
}
