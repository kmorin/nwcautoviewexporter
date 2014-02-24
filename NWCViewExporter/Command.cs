#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Events;
#endregion

namespace NWCViewExporter
{
    [Transaction(TransactionMode.Manual)]
    public class Options : IExternalCommand
    {
        public static string _folderExport = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string _NWCFileName = String.Empty;
        public static View3D _exportView = null;
        public static View3D _default3dView;
        public static NavisworksExportOptions m_neo;

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
#if !VERSION2014
            Debug.Print("Version 2013");
#else
            Debug.Print("Version 2014");
#endif // VERSION2014
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            bool isNavisAvailable = OptionalFunctionalityUtils.IsNavisworksExporterAvailable();
            if (isNavisAvailable)
            {

                _default3dView = new FilteredElementCollector(doc).OfClass(typeof(View3D)).FirstElement() as View3D;

                NavisworksExportOptions neo = new NavisworksExportOptions();
                m_neo = neo;
                m_neo.ExportScope = NavisworksExportScope.View;
                m_neo.FindMissingMaterials = false;

                Form1 formDialog = new Form1(doc, m_neo);
                System.Windows.Forms.DialogResult dr = formDialog.ShowDialog();

                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Succeeded;
                }
                else
                    return Result.Cancelled;
            }
            else
            {
                TaskDialog.Show("Error", "No Compatible Navisworks plugins found. Please install a compatible Navisworks exporter to use with Revit.");
                return Result.Failed;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class SubscribeToEvent : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            if (OptionalFunctionalityUtils.IsNavisworksExporterAvailable() == false)
            {
                TaskDialog.Show("Error", "No Compatible Navisworks plugins found. Please install a compatible Navisworks exporter to use with Revit.");
                return Result.Failed;
            }
            else
            {

                doc.DocumentSaved += new EventHandler<DocumentSavedEventArgs>(Helpers.OnDocumentSaved);

                List<RibbonPanel> ribbons = uiapp.GetRibbonPanels();
                foreach (RibbonPanel p in ribbons)
                {
                    if (p.Name == "NWC View Exporter")
                    {
                        IList<RibbonItem> items = p.GetItems();
                        foreach (PushButton pb in items)
                        {
                            if (pb.Name == "Subscribe")
                                pb.Enabled = false;
                            if (pb.Name == "Unsubscribe")
                                pb.Enabled = true;
                        }
                    }
                }


                return Result.Succeeded;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class UnsubscribeFromEvent : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            doc.DocumentSaved -= new EventHandler<DocumentSavedEventArgs>(Helpers.OnDocumentSaved);

            List<RibbonPanel> ribbons = uiapp.GetRibbonPanels();
            foreach (RibbonPanel p in ribbons)
            {
                if (p.Name == "NWC View Exporter")
                {
                    IList<RibbonItem> items = p.GetItems();
                    foreach (PushButton pb in items)
                    {
                        if (pb.Name == "Subscribe")
                            pb.Enabled = true;
                        if (pb.Name == "Unsubscribe")
                            pb.Enabled = false;
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
