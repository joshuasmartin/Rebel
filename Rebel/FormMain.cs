/*
 * FormMain.cs - Rebel Cleaner and Optimizer
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace Rebel
{
    public partial class FormMain : Form
    {
        public delegate void CheckForUpdateDelegate();
        public delegate void SetStartAndEndDelegate(DateTime start, DateTime end);
        public delegate void ShowUpdateDialogDelegate();

        private BackgroundWorker bwCleanup = new BackgroundWorker();
        private DateTime start = DateTime.MinValue;
        private DateTime end = DateTime.MinValue;
        private ArrayList results = new ArrayList();
        private Advisor advisor = new Advisor();
        private SecurityAdvisor securityAdvisor = new SecurityAdvisor();

        public FormMain()
        {
            InitializeComponent();

            bwCleanup.WorkerReportsProgress = true;
            bwCleanup.WorkerSupportsCancellation = true;
            bwCleanup.DoWork += new DoWorkEventHandler(bwCleanup_DoWork);
            bwCleanup.ProgressChanged += new ProgressChangedEventHandler(bwCleanup_ProgressChanged);
            bwCleanup.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwCleanup_RunWorkerCompleted);

            // Check for updates on another thread.
            CheckForUpdateDelegate d = checkForUpdate;
            d.BeginInvoke(null, null);

            //rchMarketing.Rtf = "REBEL is simple, powerful, and easy to use.\par REBEL clears system caches, temporary files, empties the Recycle Bin, and performs routine maintenance; but, REBEL will not delete any of your personal files.\par"
            //rchMarketing.Rtf = @"{\rtf1\ansi{\fonttbl\f0\fswiss Arial;}\f0\pard REBEL is simple, powerful, and easy to use.\par REBEL clears system caches, temporary files, empties the Recycle Bin, and performs routine maintenance; but, REBEL will not delete any of your personal files.\par}";
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            // Remove prior results.
            results.Clear();

            // Show the cancel button and disable the start button.
            lblStatus.Visible = true;
            btnCancel.Visible = true;
            btnClean.Enabled = false;

            // Reset the progress bar and the status label.
            progressBar1.Value = 0;
            progressBar1.Visible = true;

            lblStatus.Text = "";
            lblStatus.Visible = true;

            if (bwCleanup.IsBusy != true)
            {
                bwCleanup.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Cancels the background worker when the user clicks the button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (bwCleanup.IsBusy)
            {
                bwCleanup.CancelAsync();
            }
        }

        /// <summary>
        /// Performs the work of the cleanup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwCleanup_DoWork(object sender, DoWorkEventArgs e)
        {
            // Record the start time.
            this.Invoke(new SetStartAndEndDelegate(setStartAndEnd), new object[] { DateTime.Now, DateTime.MinValue });

            Cleaner cleaner = new Cleaner();

            // Check for cancellation, then wait to allow the user to read the status.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Empty the Recycle Bin.
            bwCleanup.ReportProgress(10);
            Thread.Sleep(500);
            cleaner.EmptyRecycleBin();
            results.Add(new object[] { "Information", "Recycle Bin Emptied", null });

            // Check for cancellation, then wait to allow the user to read the status.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Delete Windows Update Download Cache.
            bwCleanup.ReportProgress(25);
            Thread.Sleep(500);
            cleaner.DeleteWindowsUpdateDownloadCache();
            results.Add(new object[] { "Information", "Windows Update Download Cache Cleared", null });

            // Check for cancellation, then wait to allow the user to read the status.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Delete Windows Prefetch Files.
            bwCleanup.ReportProgress(35);
            Thread.Sleep(500);
            cleaner.DeleteWindowsPrefetchFiles();
            results.Add(new object[] { "Information", "Windows Prefetch Cleared", null });

            // Check for cancellation, then wait to allow the user to read the status.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Flush the DNS Cache.
            bwCleanup.ReportProgress(40);
            Thread.Sleep(500);
            cleaner.FlushDnsCache();
            results.Add(new object[] { "Information", "DNS Cache Flushed", null });

            // Check for cancellation, then wait to allow the user to read the status.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Delete Thumbnail Cache.
            bwCleanup.ReportProgress(44);
            Thread.Sleep(500);
            cleaner.DeleteThumbnailCaches();
            results.Add(new object[] { "Information", "Thumbnail Caches Cleared", null });

            // Check for cancellation, then wait to allow the user to read the status.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Delete Windows Temporary Files.
            bwCleanup.ReportProgress(50);
            Thread.Sleep(500);
            cleaner.DeleteWindowsTemporaryFiles();
            results.Add(new object[] { "Information", "Windows Temporary Files Deleted", null });

            // Check for cancellation, then wait.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Delete User Temporary Files.
            bwCleanup.ReportProgress(55);
            Thread.Sleep(500);
            cleaner.DeleteUserTemporaryFiles();
            results.Add(new object[] { "Information", "User Temporary Files Deleted", null });

            // Check for cancellation, then wait.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            // Perform system health checks.
            bwCleanup.ReportProgress(60);
            Thread.Sleep(500);

            // Check for high memory usage.
            if (advisor.HasHighMemoryConsumption())
                results.Add(new object[] { "Warning", "High Memory Usage", "high-memory-usage" });

            // Check for insufficient total memory.
            if (advisor.HasInsufficientMemory())
                results.Add(new object[] { "Warning", "Less than 2GB of Physical Memory", "low-physical-memory" });

            // Check for low hard disk free space.
            if (advisor.HasHighDiskUsage())
                results.Add(new object[] { "Warning", "Less than 3GB of Free Space on System Disk", "low-free-space" });
            
            // Check for cancellation, then wait.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            bwCleanup.ReportProgress(80);
            Thread.Sleep(500);

            // Check for old (possibly insecure) Java runtimes.
            if (advisor.HasOutdatedJava())
                results.Add(new object[] { "Danger", "Outdated Java Runtimes Detected", "java-outdated" });

            // Check for old (possibly insecure) Flash instances.
            if (advisor.HasOutdatedFlash())
                results.Add(new object[] { "Danger", "Outdated Flash Player Detected", "flash-outdated" });

            // Check for old (possibly insecure) Adobe Reader instances.
            if (advisor.HasOutdatedReader())
                results.Add(new object[] { "Danger", "Outdated Adobe Reader Detected", "reader-outdated" });

            // Check for cancellation, then wait.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            bwCleanup.ReportProgress(90);
            Thread.Sleep(500);

            // Check for graylisted applications.
            securityAdvisor.DownloadGraylist();

            // Check for cancellation, then wait.
            if (bwCleanup.CancellationPending)
            {
                e.Cancel = true;
                bwCleanup.ReportProgress(0);
                return;
            }

            List<string> graylistedApplications = securityAdvisor.GetGraylistedApplications();
            if (graylistedApplications.Count > 0)
                foreach (string application in graylistedApplications)
                    results.Add(new object[] { "Warning", "Graylisted Application \"" + application + "\" Detected", "graylisted-application-detected" });

            // Record the end time.
            this.Invoke(new SetStartAndEndDelegate(setStartAndEnd), new object[] { DateTime.MinValue, DateTime.Now });

            // Mark complete, which will show the results.
            bwCleanup.ReportProgress(100);
            Thread.Sleep(500);
        }

        private void bwCleanup_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                complete(showResults: false);
            }
            else if (e.Error != null)
            {
                complete(showResults: false);
            }
            else
            {
                complete(showResults: true);
            }
        }

        private void bwCleanup_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

            switch (e.ProgressPercentage)
            {
                case 10:
                    lblStatus.Text = "Emptying the Recycle Bin";
                    break;
                case 25:
                    lblStatus.Text = "Deleting Windows Update Download Cache";
                    break;
                case 35:
                    lblStatus.Text = "Deleting Windows Prefetch Files";
                    break;
                case 40:
                    lblStatus.Text = "Flushing the DNS Cache";
                    break;
                case 44:
                    lblStatus.Text = "Deleting Thumbnail Cache";
                    break;
                case 50:
                    lblStatus.Text = "Deleting Windows Temporary Files";
                    break;
                case 55:
                    lblStatus.Text = "Deleting User Temporary Files";
                    break;
                case 60:
                    lblStatus.Text = "Performing System Health Checks";
                    break;
                case 80:
                    lblStatus.Text = "Running Cloud Security Advisor";
                    break;
                case 90:
                    lblStatus.Text = "Running Cloud Performance Advisor";
                    break;
                case 100:
                    lblStatus.Text = "";
                    break;
            }
        }

        void checkForUpdate()
        {
            if (advisor.HasOutdatedRebel())
                this.Invoke(new ShowUpdateDialogDelegate(showUpdateDialog), new object[] { });
        }

        /// <summary>
        /// Sets the start and end time in order to track
        /// how long the cleanup takes.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        void setStartAndEnd(DateTime start, DateTime end)
        {
            if (start != DateTime.MinValue)
            {
                this.start = start;
            }

            if (end != DateTime.MinValue)
            {
                this.end = end;
            }
        }

        /// <summary>
        /// Shows a dialog letting the user know that there is
        /// an update available at the Rebel Website.
        /// </summary>
        void showUpdateDialog()
        {
            MessageBox.Show("A newer version of Rebel is available! Download it from www.GetRebel.com",
                            "Newer Version", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Opens the Rebel Website with the default Web browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.getrebel.com");
        }

        private void complete(bool showResults)
        {
            // Hide the cancel button and re-enable the start button.
            lblStatus.Visible = false;
            btnCancel.Visible = false;
            btnClean.Enabled = true;

            // Hide the progress and status.
            progressBar1.Visible = false;
            lblStatus.Visible = false;

            if (showResults)
            {
                // Show the results.
                FormResults frm = new FormResults(start, end, results);
                frm.Show();
            }
        }
    }
}
