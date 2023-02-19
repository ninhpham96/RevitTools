using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DupSheet.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DupSheet.ViewModel
{
    public partial class DupSheetViewModel : ObservableObject
    {
        private DupSheetView dupSheetView;
        private UIDocument uidoc { get; }
        private Document doc { get; }
        public DupSheetView DupSheetView
        {
            get
            {
                if (dupSheetView == null)
                {
                    dupSheetView = new DupSheetView() { DataContext = this };
                }
                return dupSheetView;
            }
            set
            {
                dupSheetView = value;
                OnPropertyChanged();
            }
        }
        [RelayCommand]
        private void Clickme()
        {
            this.DupSheetView.Close();
            var select = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            var ele = doc.GetElement(select);
            var para = ele.LookupParameter("Comments");
            MessageBox.Show(para.AsString());
            try
            {
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("!!!");
                    para.Set("100");
                    t.Commit();                    
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }
        public DupSheetViewModel(UIApplication uiApp)
        {
            this.uidoc = uiApp.ActiveUIDocument;
            this.doc = uiApp.ActiveUIDocument.Document;
        }
    }
}
