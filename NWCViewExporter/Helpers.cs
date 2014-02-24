using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Diagnostics;
using System.Reflection;
using Autodesk.Revit.DB.Events;

namespace NWCViewExporter
{
    public class Helpers
    {
        public static string fileExtension = "_navOpts.xpo";
        /// <summary>
        /// Returns true if any extensible storage exists in document
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool DoesAnyStorageExist(Document doc)
        {
            IList<Schema> schemas = Schema.ListSchemas();
            if (schemas.Count == 0)
                return false;
            else
            {
                foreach (Schema s in schemas)
                {
                    List<ElementId> ids = ElementsWithStorage(doc, s);
                    if (ids.Count > 0)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns formatted string with schema GUIDS and element info for all elements containing extensible storage
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string GetElementsWithAllSchemas(Document doc)
        {
            StringBuilder sBuilder = new StringBuilder();
            IList<Schema> schemas = Schema.ListSchemas();
            if (schemas.Count == 0)
                return "No schemas or storage.";
            else
            {
                foreach (Schema s in schemas)
                {
                    sBuilder.Append(GetElementsWithSchema(doc, s));
                }
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        private static string GetElementsWithSchema(Document doc, Schema schema)
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.AppendLine("Schema: " + schema.GUID.ToString() + ", " + schema.SchemaName);
            List<ElementId> elementsofSchema = ElementsWithStorage(doc, schema);
            if (elementsofSchema.Count == 0)
                sBuilder.AppendLine("No Elements");
            else
            {
                foreach (ElementId id in elementsofSchema)
                {
                    sBuilder.AppendLine(PrintElementInfo(id, doc));
                }
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="schema"></param>
        /// <returns>List of ElementIds that elements contain extensible storage</returns>
        private static List<ElementId> ElementsWithStorage(Document doc, Schema schema)
        {
            List<ElementId> ids = new List<ElementId>();
            FilteredElementCollector fec = new FilteredElementCollector(doc);
            fec.WherePasses(new ExtensibleStorageFilter(schema.GUID));
            ids.AddRange(fec.ToElementIds());
            return ids;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static string PrintElementInfo(ElementId id, Document doc)
        {
            Element element = doc.GetElement(id);
            string val = (element.Id.ToString() + ", " + element.Name + ", " + element.GetType().FullName);
            Debug.WriteLine(val);
            return val;
        }

        public static bool WriteExportOptionsInfo(Document doc, NavisworksExportOptions neo)
        {
            string projectPath = doc.PathName;
            string optionsFile = projectPath.Substring(0, projectPath.Length - 4) + fileExtension;

            StreamWriter sw = new StreamWriter(optionsFile,false);
            
            foreach (PropertyInfo p in neo.GetType().GetProperties())
            {
                string s = p.Name + "|" + p.GetValue(neo, null).ToString();
                sw.WriteLine(s);
            }

            //Write Path for save
            sw.WriteLine("Folder|" + Options._folderExport);
             
            //Write viewName for save
            sw.WriteLine("ViewName|" + Options._exportView.Name);

            sw.Close();
            return true;
        }

        public static bool GetExportOptionsInfo(Document doc, NavisworksExportOptions neo)
        {
            try
            {
                string projectPath = doc.PathName;
                string optionsFile = projectPath.Substring(0, projectPath.Length - 4) + fileExtension;

                if (File.Exists(optionsFile))
                {
                    StreamReader sr = new StreamReader(optionsFile);

                    while (!sr.EndOfStream)
                    {
                        string s = sr.ReadLine();
                        string[] splitter = s.Split('|');
                        if (splitter[0] == "ViewId")
                        {
                            int id = -1;
                            int.TryParse(splitter[1], out id);
                            neo.ViewId = new ElementId(id);
                            Options._exportView = doc.GetElement(neo.ViewId) as View3D;
                        }
                        if (splitter[0] == "Folder")
                        {
                            Options._folderExport = splitter[1];
                        }
                    }
                    sr.Close();
                    return true;
                }
                else
                {
                    Options._exportView = Options._default3dView;
                    return true;
                }
            }
            catch (Exception)
            {
                TaskDialog.Show("Error", "Please save your project before editing Navisworks Export Options.");
                return false;
            }
        }

        public static void OnDocumentSaved(object sender, DocumentSavedEventArgs e)
        {
            Document doc = e.Document;

            if (Options.m_neo != null && Options.m_neo.ViewId.ToString() != "-1")
            {
                doc.Export(Options._folderExport, Options._NWCFileName, Options.m_neo);
            }
            else
            {
                TaskDialog.Show("Export Options", "You must set your export options before Automatic NWC file creation.");
            }
        }
    }
}
