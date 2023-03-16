using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RevitTools
{
    public class AutoTagRoomVM : ObservableObject
    {
        private AutoTagRoomView _view;
        public AutoTagRoomView view {
            get
            {
                if(_view == null)
                    _view = new AutoTagRoomView() { DataContext=this};
                return _view;
            } 
            set
            {
                _view = value;
                OnPropertyChanged();
            }
        }
        private UIDocument uidoc;


        public AutoTagRoomVM(UIDocument uidoc)
        {
            var source = GetData.instance.GetAllTagRoom(uidoc.Document);
            MessageBox.Show(source.Count.ToString());
            view.cbtagRoom.ItemsSource = source;
            view.cbtagRoom.DisplayMemberPath = "Name";
            this.uidoc = uidoc;
        }
    }
}
