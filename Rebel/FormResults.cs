using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
                    AddWarningRow((string)result[1]);
        }

        private void Setup()
        {
            // Setup default styles.
            grdResults.RowTemplate.MinimumHeight = 30;

            // If less than 4GB.
            AddWarningRow("Low Disk Space");

            // If more than 10.
            AddWarningRow("Excessive Startup Programs");

            // If more than 5.
            AddWarningRow("Too Many Running Programs");

            // Is this possible?
            AddWarningRow("Missing or Corrupted Drivers");
        }

        private void AddInformationRow(string message)
        {
            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Information", message, null);
            
            // Set the styling of the row.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
        }

        private void AddWarningRow(string message)
        {
            LinkLabel lnk = new LinkLabel();
            lnk.Text = "Fix this Problem";

            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Warning", message, lnk);

            // Set the styling of the row.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
            grdResults.Rows[index].Cells[0].Style.ForeColor = Color.DarkOrange;
        }
    }
}
