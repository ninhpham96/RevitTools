using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace RevitTools
{
    [Transaction(TransactionMode.Manual)]
    public class ExportToExcel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<myWall> walls = new List<myWall>();
            var picks = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
            foreach (var pick in picks)
            {
                var ele = doc.GetElement(pick) as Wall;
                if (ele != null)
                {
                    walls.Add(new myWall(ele));
                }
            }
            List<string> result = new List<string>() { "1", "2", "3" };
            try
            {
                SaveExcelFile(walls, "Test");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return Result.Succeeded;
        }
        private static void SaveExcelFile<T>(IEnumerable<T> data, string WorksheetsName)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Chon noi luu file";
            dialog.Filter = "Excel file (*xlsx)|*xlsx";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var str = dialog.FileName;
                var file = new FileInfo(str);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                DeleteIfExists(file);
                using var package = new ExcelPackage(file);
                var ws = package.Workbook.Worksheets.Add(WorksheetsName);
                var range = ws.Cells["A1"].LoadFromCollection(data, true);
                range.AutoFitColumns();
                package.Save();
            }
        }
        private static void DeleteIfExists(FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
        }
    }
    class myWall
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string LevelId { get; set; }
        public string Width { get; set; }
        public string Id { get; set; }
        public myWall(Wall wall)
        {
            Name = wall.Name;
            Category = wall.Category.Name;
            LevelId = wall.LevelId.ToString();
            Width = wall.Width.ToString();
            Id = wall.Id.ToString();
        }
    }
}
