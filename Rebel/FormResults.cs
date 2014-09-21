using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace Rebel
{
    public partial class FormResults : Form
    {
        public FormResults()
        {
            InitializeComponent();

            Setup();
        }

        public FormResults(DateTime start, DateTime end, ArrayList results)
        {
            InitializeComponent();

            Setup();

            // Clear prior results.
            grdResults.Rows.Clear();

            lblStartValue.Text = start.ToShortTimeString();
            lblEndValue.Text = end.ToShortTimeString();

            foreach (object[] result in results)
                if ((string)result[0] == "Information")
                    AddInformationRow((string)result[1]);
                else if ((string)result[0] == "Warning")
                    AddWarningRow((string)result[1], (int)result[2]);
        }

        private void Setup()
        {
            // Setup default styles.
            grdResults.RowTemplate.MinimumHeight = 30;

            // If more than 10.
            AddWarningRow("Excessive Startup Programs", 0);

            // If more than 5.
            AddWarningRow("Too Many Running Programs", 0);

            // Is this possible?
            AddWarningRow("Missing or Corrupted Drivers", 0);
        }

        private void AddInformationRow(string message)
        {
            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Information", message, null);
            
            // Set the styling of the row.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
        }

        private void AddWarningRow(string message, int code)
        {
            LinkLabel lnk = new LinkLabel();
            lnk.Text = "Fix this Problem";
            lnk.LinkClicked += new LinkLabelLinkClickedEventHandler(lnk_LinkClicked);


            // Add the link URL.
            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "http://www.getrebel.com/issues/" + textForCode(code);
	        lnk.Links.Add(link);

            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Warning", message, lnk);

            // Set the styling of the row.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
            grdResults.Rows[index].Cells[0].Style.ForeColor = Color.DarkOrange;
        }

        void lnk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData as string);
        }

        private string textForCode(int code)
        {
            switch (code)
            {
                case 1:
                    {
                        return "high-memory-usage";
                    }
                case 2:
                    {
                        return "low-physical-memory";
                    }
                case 3:
                    {
                        return "low-free-space";
                    }
                case 4:
                    {
                        return "java-outdated";
                    }
                case 5:
                    {
                        return "flash-outdated";
                    }
                case 6:
                    {
                        return "reader-outdated";
                    }
                default:
                    {
                        return "";
                    }
            }
        }
    }
}
