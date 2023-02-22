using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DupSheet.Revit;
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
        #region properties and field
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
        private List<_Sheet> source;
        private List<ElementId> SelectedSheets = new List<ElementId>();
        public List<_Sheet> Source { get => source; set => source = value; }
        #endregion
        #region command
        [RelayCommand]
        private void Run()
        {
        }
        [RelayCommand]
        private void Clickme(_Sheet ob)
        {
            if (ob.isChecked)
            {
                SelectedSheets.Add(ob.id);
            }
            else
            {
                SelectedSheets.Remove(ob.id);
            }
        }
        #endregion
        #region constructor
        public DupSheetViewModel(UIApplication uiApp)
        {
            this.uidoc = uiApp.ActiveUIDocument;
            this.doc = uiApp.ActiveUIDocument.Document;

            //set ItemsSource 
            Source = new List<_Sheet>();
            List<ViewSheet> listViewSheet = Select.instance.GetAllViewSheet(doc);
            foreach (var item in listViewSheet)
            {
                Source.Add(new _Sheet(item));
            }
            DupSheetView.lsvDuplicateSheet.ItemsSource = Source;
        }
        #endregion
    }
    public class _Sheet
    {
        public string sheetName { get; set; }
        public string sheetNumber { get; set; }
        public ElementId id { get; set; }
        public bool isChecked { get; set; }
        public _Sheet(ViewSheet vs)
        {
            this.sheetName = vs.Name;
            this.sheetNumber = vs.SheetNumber;
            this.id = vs.Id;
        }
    }
    public partial class DupSheetViewModel
    {
        ElementId GetSheetTitleBlock(ElementId id)
        {
            var all_title_block = new FilteredElementCollector(doc).
                WhereElementIsNotElementType().
                OfCategory(BuiltInCategory.OST_TitleBlocks).
                ToElements();
            foreach (Element item in all_title_block)
            {
                if (item.OwnerViewId == id)
                {
                    return item.GetTypeId();
                }
            }
            return null;
        }
        void duplicate_schedules(ElementId oldsheetID, ElementId newsheetID)
        {
            List<ScheduleSheetInstance> viewSchedule = Select.instance.GetAllViewSchedule(doc);
            foreach (ScheduleSheetInstance item in viewSchedule)
            {
                if (item.OwnerViewId == oldsheetID)
                {
                    if (!item.IsTitleblockRevisionSchedule)
                    {
                        var origin = item.Point;
                        var scheduleID = item.ScheduleId;
                        if (scheduleID == ElementId.InvalidElementId) continue;
                        ViewSchedule viewschedule = doc.GetElement(scheduleID) as ViewSchedule;
                        var schedule_view_id = viewschedule.Duplicate(ViewDuplicateOption.Duplicate);

                        ScheduleSheetInstance.Create(doc, newsheetID, schedule_view_id, origin);
                    }
                }
            }
        }
    }
}
