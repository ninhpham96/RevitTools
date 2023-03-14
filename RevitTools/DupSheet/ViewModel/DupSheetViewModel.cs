using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DupSheet.Revit;
using DupSheet.View;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using vView = Autodesk.Revit.DB.View;

namespace DupSheet.ViewModel
{
    public partial class DupSheetViewModel : ObservableObject
    {
        #region properties and field
        private DupSheetView dupSheetView;
        private UIDocument uidoc { get; }
        private Document doc { get; }
        List<ViewSheet> selectedSheets = new List<ViewSheet>();
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
        #endregion
        #region command
        [RelayCommand]
        private void Run()
        {
            DuplicateSelectedSheet(selectedSheets.FirstOrDefault());
        }
        [RelayCommand]
        private void Clickme(ViewSheet vs)
        {
            if (selectedSheets.Contains(vs))
                selectedSheets.Remove(vs);
            else selectedSheets.Add(vs);
        }
        #endregion
        #region constructor
        public DupSheetViewModel(UIApplication uiApp)
        {
            this.uidoc = uiApp.ActiveUIDocument;
            this.doc = uiApp.ActiveUIDocument.Document;
            DupSheetView.lsvDuplicateSheet.ItemsSource = Select.instance.GetAllViewSheet(doc);
        }
        #endregion
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

        void DuplicateSelectedSheet(ViewSheet vs)
        {
            var title_block = GetSheetTitleBlock(vs.Id);
            if (title_block != null)
            {
                using (Transaction tran = new Transaction(doc, "create sheet"))
                {
                    tran.Start();
                    var new_sheet = ViewSheet.Create(doc, title_block);
                    DuplicateSchedules(vs.Id, new_sheet.Id);
                    DuplicateViews(vs, new_sheet);
                    tran.Commit();
                }
            }
        }
        void DuplicateSchedules(ElementId oldsheetID, ElementId newsheetID)
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
        void DuplicateViews(ViewSheet oldsheet, ViewSheet newsheet)
        {
            ICollection<ElementId> viewports_ids = oldsheet.GetAllViewports();
            foreach (ElementId viewport_id in viewports_ids)
            {
                Viewport viewport = doc.GetElement(viewport_id) as Viewport;
                var viewport_type_id = viewport.GetTypeId();
                var viewport_origin = viewport.GetBoxCenter();
                var view_id = viewport.ViewId;
                vView view = doc.GetElement(view_id) as vView;
                vView new_view = null;

                if (view.ViewType == ViewType.Legend)
                    continue;
                else if (view.ViewType == ViewType.ThreeD)
                {
                    var NewViewId = view.Duplicate(ViewDuplicateOption.Duplicate);
                    new_view = doc.GetElement(NewViewId) as vView;
                }
                else
                {
                    if (DupSheetView.ckbOp1.IsChecked == true)
                    {
                        var NewViewId = view.Duplicate(ViewDuplicateOption.Duplicate);
                        new_view = doc.GetElement(NewViewId) as vView;
                    }
                    else if (DupSheetView.ckbOp2.IsChecked == true)
                    {
                        var NewViewId = view.Duplicate(ViewDuplicateOption.WithDetailing);
                        new_view = doc.GetElement(NewViewId) as vView;
                    }
                    else if (DupSheetView.ckbOp3.IsChecked == true)
                    {
                        var NewViewId = view.Duplicate(ViewDuplicateOption.AsDependent);
                        new_view = doc.GetElement(NewViewId) as vView;
                    }

                }
                if (new_view != null)
                {
                    Viewport new_vp = Viewport.Create(doc, newsheet.Id, new_view.Id, viewport_origin);
                    var new_viewport_type_id = new_vp.GetTypeId();
                    if (viewport_type_id != new_viewport_type_id)
                    {
                        new_vp.ChangeTypeId(viewport_type_id);
                    }
                }
            }
        }
        void DuplicateElements(ViewSheet sourceview, ViewSheet destinationview, ElementId elementIds)
        {
            CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
            ElementTransformUtils.CopyElements(sourceview, (ICollection<ElementId>)elementIds, destinationview, null, copyPasteOptions);
        }
        void check(bool b)
        {
            foreach (var item in DupSheetView.lsvDuplicateSheet.Items)
            {
                var container = DupSheetView.lsvDuplicateSheet.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
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
