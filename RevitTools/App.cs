using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitTools
{
    public class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbon(application, "RevitTools", "Auto Tag");
            return Result.Succeeded;
        }
        private void CreateRibbon(UIControlledApplication application, string tabName, string panelName)
        {
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {
                throw;
            }
            RibbonPanel panel = null;
            List<RibbonPanel> panelList = application.GetRibbonPanels(tabName);
            foreach (RibbonPanel p in panelList)
            {
                if (p.Name == panelName)
                {
                    panel = p;
                    break;
                }
            }
            if (panel == null)
            {
                panel = application.CreateRibbonPanel(tabName, panelName);
            }
            Image image = Properties.Resources.autotag_32x32;
            ImageSource imageSource = GetImageSource(image);

            PushButtonData pushButtonData = new PushButtonData("AutoTag", "RoomTag", Assembly.GetExecutingAssembly().Location, "RevitTools.AutoTagRoomCMD")
            {
                ToolTip = "tool lỗi thì liên hệ em Ninh",
                LongDescription = "Nhấn để chạy thôi.",
                Image = imageSource,
                LargeImage = imageSource
            };
            PushButton pushButton = panel.AddItem(pushButtonData) as PushButton;
            pushButton.Enabled = true;
        }
        private BitmapSource GetImageSource(Image image)
        {
            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = null;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
            }
            return bitmap;
        }
    }
}
