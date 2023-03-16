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
        private ObservableCollection<RoomTagType> srcroomConnectCeiling = new ObservableCollection<RoomTagType>();

        [RelayCommand]
        void Run()
        {
            MessageBox.Show("Test");
        }
        [RelayCommand]
        void Check()
        {
            if(view.rbtTruss.IsChecked == true)
            {
                MessageBox.Show("11");
            }
            else
            {
                MessageBox.Show("22");
            }
        }
        private UIDocument uidoc;
        public AutoTagRoomVM(UIDocument uidoc)
        {
            List<RoomTagType> source = new List<RoomTagType>(GetData.instance.GetAllTagRoom(uidoc.Document));
            if (true)
            {
                SetItemSource(source, true);
            }
            this.uidoc = uidoc;
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

                SrcroomConnectCeiling = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_見切縁").Cast<RoomTagType>().ToList());
            }
            else
            {
                SrcroomName = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_一般").Cast<RoomTagType>().ToList());

                SrcroomWall = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_壁_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomCeiling = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_天井_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomFloor = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_床_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomHabaki = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_幅木_Truss対応").Cast<RoomTagType>().ToList());

                SrcroomConnectCeiling = new ObservableCollection<RoomTagType>(source.Where(p => p.FamilyName == "dタグ_部屋_見切縁_Truss対応").Cast<RoomTagType>().ToList());
            }
        }
    }
}
