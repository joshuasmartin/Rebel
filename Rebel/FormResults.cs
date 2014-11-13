/*
 * FormResult.cs - Rebel Cleaner and Optimizer
 * 
 * Copyright 2014 by Joshua Shane Martin <smokeygrowth.com>
 * 
 * This file is part of Rebel.
 * 
 * Rebel is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Rebel is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Rebel.  If not, see <http://www.gnu.org/licenses/>.
 */

#region Using Directives

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#endregion

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
                    AddWarningRow((string)result[1], (string)result[2]);
                else if ((string)result[0] == "Danger")
                    AddDangerRow((string)result[1], (string)result[2]);
        }

        private void Setup()
        {
            // Setup default styles.
            grdResults.RowTemplate.MinimumHeight = 30;

            // If more than 10.
            //AddWarningRow("Excessive Startup Programs", 0);

            // If more than 5.
            //AddWarningRow("Too Many Running Programs", 0);

            // Is this possible?
            //AddWarningRow("Missing or Corrupted Drivers", 0);
        }

        private void AddInformationRow(string message)
        {
            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Information", message, null, null);

            // Set the styling of the cell.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
            grdResults.Rows[index].Cells[0].Style.ForeColor = Color.White;
            grdResults.Rows[index].Cells[0].Style.BackColor = Color.Gray;
        }

        private void AddWarningRow(string message, string param)
        {
            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Warning", message, "Fix this Problem", param);

            // Set the styling of the cell.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
            grdResults.Rows[index].Cells[0].Style.ForeColor = Color.White;
            grdResults.Rows[index].Cells[0].Style.BackColor = Color.DarkOrange;
        }

        private void AddDangerRow(string message, string param)
        {
            // Add the row and get the row index.
            int index = grdResults.Rows.Add("Danger", message, "Fix this Problem", param);

            // Set the styling of the cell.
            grdResults.Rows[index].Cells[0].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
            grdResults.Rows[index].Cells[0].Style.ForeColor = Color.White;
            grdResults.Rows[index].Cells[0].Style.BackColor = Color.DarkRed;
        }

        /// <summary>
        /// Using the default Web browser, a URL is opened for the issue on
        /// the given row. The URL is to an article on the Rebel Website
        /// regarding additional information about the issue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdResults_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Only respond to clicks in the link column.
            if (e.ColumnIndex == 2)
            {
                object value = grdResults.Rows[e.RowIndex].Cells[3].Value;
                if (value is DBNull) { return; }

                string url = string.Format("http://www.getrebel.com/articles/{0}", value.ToString());

                Process.Start(url);
            }
        }

        private void setStylingForLink(int index)
        {
            grdResults.Rows[index].Cells[3].Style.Font = new Font(grdResults.Font, FontStyle.Bold);
            grdResults.Rows[index].Cells[3].Style.ForeColor = Color.Green;
            grdResults.Rows[index].Cells[3].Style.BackColor = Color.White;
        }
    }
}
