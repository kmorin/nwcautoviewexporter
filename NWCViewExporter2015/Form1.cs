using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace NWCViewExporter
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        public Document m_doc;
        public NavisworksExportOptions m_neo;        

        public Form1(Document doc, NavisworksExportOptions neo)
        {
            InitializeComponent();
            m_doc = doc;
            m_neo = neo;
        }

        private void btnDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            DialogResult ddr = fbd.ShowDialog();

            if (ddr == System.Windows.Forms.DialogResult.OK)
            {
                Options._folderExport = fbd.SelectedPath;
                lblFolderPath.Text = Options._folderExport;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Set NEO options based on form input
            m_neo.ExportScope = NavisworksExportScope.View;
            
            Options._exportView = cbViewList.SelectedItem as View3D;
            m_neo.ViewId = Options._exportView.Id;

            // Set extensible storage stuff
            bool writeout = Helpers.WriteExportOptionsInfo(m_doc, m_neo);
            
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void Form1_Load(object sender, EventArgs e)
        {           

            //read from exportOptions for project
            if (Helpers.GetExportOptionsInfo(m_doc, m_neo))
            {
                //

                lblFolderPath.Text = Options._folderExport;

                FilteredElementCollector fec = new FilteredElementCollector(m_doc).OfClass(typeof(View3D));

                foreach (Element elem in fec)
                {
                    View3D v = elem as View3D;
                    cbViewList.Items.Add(v);
                }
                cbViewList.DisplayMember = "ViewName";

                //Set selection to property                            
                //cbViewList.SelectedItem = selectedView;
                cbViewList.Text = Options._exportView.Name;
            }
            else
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void cbViewList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options._exportView = cbViewList.SelectedItem as View3D;
            //MessageBox.Show(cbViewList.SelectedItem.ToString());
        }
    }
}
