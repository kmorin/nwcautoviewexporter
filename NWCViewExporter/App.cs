#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;
#endregion

namespace NWCViewExporter
{
    class App : IExternalApplication
    {
        public static string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string assyPath = Path.Combine(dir, "NWCViewExporter.dll");
        static string _imgFolder = Path.Combine(dir, "Images");
        public const string Caption = "NWC Exporter";

        public Result OnStartup(UIControlledApplication a)
        {
            try
            {
                AddRibbonPanel(a);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ribbon", ex.ToString());
            }
            return Result.Succeeded;
        }

        private void AddRibbonPanel(UIControlledApplication app)
        {
            RibbonPanel panel = app.CreateRibbonPanel("NWC View Exporter");
            PushButtonData pbd_Options = new PushButtonData("Export Options", "Export Options", assyPath, "NWCViewExporter.Options");
            PushButtonData pbd_Sub = new PushButtonData("Subscribe", "Auto NWC ON", assyPath, "NWCViewExporter.SubscribeToEvent");
            pbd_Sub.LargeImage = NewBitmapImage("sub.png");
            pbd_Sub.ToolTip = "Turn ON Automatic NWC file creation after each save.";
            pbd_Sub.LongDescription = "Automatically creates an NWC Navisworks file after each time the project is saved. The selected view in the options dialog is used as the exported view. The NWC file is saved in the directory specified in the options dialog.";            
            PushButtonData pbd_Unsub = new PushButtonData("Unsubscribe", "Auto NWC OFF", assyPath, "NWCViewExporter.UnsubscribeFromEvent");
            pbd_Unsub.LargeImage = NewBitmapImage("unsub.png");
            pbd_Unsub.ToolTip = "Turn OFF Automatic NWC file creation.";
            pbd_Unsub.LongDescription = "Turns off the automatic NWC file creator.";

            PushButton pb_Options = panel.AddItem(pbd_Options) as PushButton;
            //pb_Options.LargeImage = NewBitmapImage("options.png");
            pb_Options.LargeImage = GetEmbeddedImage("NWCViewExporter.options.png");
            pb_Options.ToolTip = "Set Options for Automatic NWC Creation";
            pb_Options.LongDescription = "Sets the view to use for Automatic NWC creation on each save as well as the destination folder to save the NWC file.";
            
            panel.AddSeparator();
            IList<RibbonItem> stacked = panel.AddStackedItems(pbd_Sub, pbd_Unsub);

            //Context help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.ChmFile, dir + "/Resources/help.htm");

            pb_Options.SetContextualHelp(contextHelp);

            foreach (RibbonItem ri in stacked)
            {
                ri.SetContextualHelp(contextHelp); //set contextHelp for stacked items;
                if (ri.Name == "Subscribe")
                    ri.Enabled = true;
                else
                    ri.Enabled = false;
            }

        }

        static BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                Stream s = a.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }

        BitmapImage NewBitmapImage(string imgName)
        {
            return new BitmapImage(new Uri(Path.Combine(_imgFolder, imgName)));
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
