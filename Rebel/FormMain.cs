using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rebel
{
    public partial class FormMain : Form
    {
        public delegate void StartCleanupDelegate();
        public delegate void UpdateProgressDelegate(int progress, string status);
        public delegate void SetStartAndEndDelegate(DateTime start, DateTime end);

        private DateTime start = DateTime.MinValue;
        private DateTime end = DateTime.MinValue;
        private ArrayList results = new ArrayList();

        public FormMain()
        {
            InitializeComponent();
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            // Remove prior results.
            results.Clear();

            // Show the cancel button and disable the start button.
            btnCancel.Visible = true;
            btnClean.Enabled = false;

            // Reset the progress bar and the status label.
            progressBar1.Value = 0;
            progressBar1.Visible = true;

            lblStatus.Text = "";
            lblStatus.Visible = true;

            // Start the cleanup process.
            StartCleanupDelegate d = startCleanup;
            d.BeginInvoke(null, null);
        }

        void startCleanup()
        {
            // Record the start time.
            this.Invoke(new SetStartAndEndDelegate(setStartAndEnd), new object[] { DateTime.Now, DateTime.MinValue });

            Cleaner cleaner = new Cleaner();

            // Empty the Recycle Bin.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 10, "Emptying the Recycle Bin" });
            cleaner.EmptyRecycleBin();
            results.Add(new object[] { "Information", "Recycle Bin Emptied", null });

            // Delete Windows Update Download Cache.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 20, "Deleting Windows Update Download Cache" });
            cleaner.DeleteWindowsUpdateDownloadCache();
            results.Add(new object[] { "Information", "Windows Update Download Cache Cleared", null });
            
            // Delete Windows Prefetch Files.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 30, "Deleting Windows Prefetch Files" });
            cleaner.DeleteWindowsPrefetchFiles();
            results.Add(new object[] { "Information", "Windows Prefetch Cleared", null });

            // Flush the DNS Cache.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 40, "Flushing the DNS Cache" });
            cleaner.FlushDnsCache();
            results.Add(new object[] { "Information", "DNS Cache Flushed", null });

            // Delete Thumbnail Cache.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 50, "Deleting Thumbnail Cache" });
            cleaner.DeleteThumbnailCaches();
            results.Add(new object[] { "Information", "Thumbnail Caches Cleared", null });

            // Delete Windows Temporary Files.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 60, "Deleting Windows Temporary Files" });
            cleaner.DeleteWindowsTemporaryFiles();
            results.Add(new object[] { "Information", "Windows Temporary Files Deleted", null });

            // Delete User Temporary Files.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 70, "Deleting User Temporary Files" });
            cleaner.DeleteUserTemporaryFiles();
            results.Add(new object[] { "Information", "User Temporary Files Deleted", null });

            Advisor advisor = new Advisor();

            // Check for low memory.
            if (advisor.HasHighMemoryConsumption())
                results.Add(new object[] { "Warning", "High Memory Consumption", null });

            // Record the end time.
            this.Invoke(new SetStartAndEndDelegate(setStartAndEnd), new object[] { DateTime.MinValue, DateTime.Now });

            // Mark complete, which will show the results.
            this.Invoke(new UpdateProgressDelegate(updateProgress), new object[] { 100, "Done" });
        }

        void updateProgress(int progress, string status)
        {
            progressBar1.Value = progress;
            lblStatus.Text = status;

            if (progress == 100)
            {
                // Hide the cancel button and re-enable the start button.
                btnCancel.Visible = false;
                btnClean.Enabled = true;

                // Hide the progress and status.
                progressBar1.Visible = false;
                lblStatus.Visible = false;

                // Show the results.
                FormResults frm = new FormResults(start, end, results);
                frm.Show();
            }
        }

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
    }
}
