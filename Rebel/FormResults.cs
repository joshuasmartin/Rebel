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
            int index = grdResults.Rows.Add("Information", message, null, null);
            
            // Set the styling of the row.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
        }

        private void AddWarningRow(string message, int code)
        {
            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Warning", message, "Fix this Problem", code);

            // Set the styling of the row.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
            grdResults.Rows[index].Cells[0].Style.ForeColor = Color.DarkOrange;
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
                case 7:
                    {
                        return "graylisted-application-detected";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        private void grdResults_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Only respond to clicks on links.
            if (e.ColumnIndex == 2)
            {
                object value = grdResults.Rows[e.RowIndex].Cells[3].Value;
                if (value is DBNull) { return; }

                string valueString = value.ToString();
                int valueInt = int.Parse(valueString);
                string url = string.Format("http://www.getrebel.com/issues/{0}", textForCode(valueInt));

                Process.Start(url);
            }
        }
    }
}
