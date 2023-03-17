using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RevitTools
{
    public partial class AutoTagRoomVM : ObservableObject
    {
        #region field
        private AutoTagRoomView _view;
        public AutoTagRoomView view
        {
            get
            {
                if (_view == null)
                    _view = new AutoTagRoomView() { DataContext = this };
                return _view;
            }
            set
            {
                _view = value;
                OnPropertyChanged();
            }
        }
        private UIDocument uidoc;
        private Document doc;
        List<Room> rooms;
        List<RoomTagType> source;
        [ObservableProperty]
        public ObservableCollection<RoomTagType> srcroomName = new ObservableCollection<RoomTagType>();
        [ObservableProperty]
        private ObservableCollection<RoomTagType> srcroomWall = new ObservableCollection<RoomTagType>();
        [ObservableProperty]
        private ObservableCollection<RoomTagType> srcroomFloor = new ObservableCollection<RoomTagType>();
        [ObservableProperty]
        private ObservableCollection<RoomTagType> srcroomCeiling = new ObservableCollection<RoomTagType>();
        [ObservableProperty]
        private ObservableCollection<RoomTagType> srcroomHabaki = new ObservableCollection<RoomTagType>();
        [ObservableProperty]
        private ObservableCollection<RoomTagType> srcroomCeilingConnect = new ObservableCollection<RoomTagType>();
        #endregion
        #region command
        [RelayCommand]
        void Run()
        {
            try
            {
                using (Transaction tran = new Transaction(doc, "tao tag"))
                {
                    tran.Start();
                    var typeid = view.cbtagRoom.SelectedItem as RoomTagType;
                    MessageBox.Show(typeid.Id.ToString());
                    XYZ cen = GetRoomCenter(rooms.First());
                    UV center = new UV(cen.X, cen.Y);
                    RoomTag roomTag = doc.Create.NewRoomTag(new LinkElementId(rooms.First().Id), center, doc.ActiveView.Id);
                    roomTag.ChangeTypeId(typeid.Id);
                    roomTag.HasLeader = true;
                    roomTag.TagHeadPosition = rooms.First().get_BoundingBox(doc.ActiveView).Max-1;
                    //roomTag.HasLeader = false;
                    tran.Commit();
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        [RelayCommand]
        void Check()
        {
            ClearSource();
            if (view.rbtNotTruss.IsChecked == true)
            {
                SetItemSource(source, true);
            }
            if (view.rbtTruss.IsChecked == true)
            {
                SetItemSource(source, false);
            }
            SetSelectedIndex();
        }
        #endregion
        public AutoTagRoomVM(UIDocument uidoc)
        {
            this.uidoc = uidoc;
            doc = uidoc.Document;
            rooms = GetData.instance.GetRooms(doc);
            source = GetData.instance.GetAllTagRoom(uidoc.Document);
            if (true)
            {
                SetItemSource(source, true);
                SetSelectedIndex();
            }
        }
        #region methods
        public XYZ GetRoomCenter(Room room)
        {
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);
            return roomCenter;
        }
        public XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ bounding = elem.get_BoundingBox(doc.ActiveView);
            XYZ center = (bounding.Max + bounding.Min) * 0.5;
            return center;
        }
        void SetItemSource(List<RoomTagType> source, bool b)
        {
            if (b)
            {
                SrcroomName = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_一般").Cast<RoomTagType>().ToList());

                SrcroomWall = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_壁").Cast<RoomTagType>().ToList());

                SrcroomCeiling = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_天井").Cast<RoomTagType>().ToList());

                SrcroomFloor = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_床").Cast<RoomTagType>().ToList());

                SrcroomHabaki = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_幅木").Cast<RoomTagType>().ToList());

                SrcroomCeilingConnect = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_見切縁").Cast<RoomTagType>().ToList());
            }
            else
            {
                SrcroomName = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_一般").Cast<RoomTagType>().ToList());

                SrcroomWall = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_壁_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomCeiling = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_天井_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomFloor = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_床_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomHabaki = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_幅木_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomCeilingConnect = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_見切縁_Truss対応").Cast<RoomTagType>().ToList());
            }
        }
        void SetSelectedIndex()
        {
            view.cbtagCeil.SelectedIndex = 0;
            view.cbtagFloor.SelectedIndex = 0;
            view.cbtagRoom.SelectedIndex = 0;
            view.cbtagMawari.SelectedIndex = 0;
            view.cbtagWall.SelectedIndex = 0;
            view.cbtagHabaki.SelectedIndex = 0;

        }
        void ClearSource()
        {
            SrcroomName.Clear();
            SrcroomWall.Clear();
            SrcroomCeiling.Clear();
            SrcroomFloor.Clear();
            SrcroomCeilingConnect.Clear();
        }
        #endregion
    }
}
