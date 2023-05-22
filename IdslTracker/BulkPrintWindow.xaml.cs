using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IdslTracker
{
    /// <summary>
    /// Interaction logic for BulkPrintWindow.xaml
    /// </summary>
    public partial class BulkPrintWindow : Window
    {
        //internal List<string> docNumbers;
        internal List<IdslTrackerLine> trackerLines;

        public BulkPrintWindow()
        {
            InitializeComponent();
            foreach (string name in Globals.PrintedByNames)
            {
                FilePrintedByComboBox.Items.Add(name);
            }
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            //this.printBy = FilePrintedByComboBox.Text;
            //this.printDate = FilePrintedDateDatePicker.SelectedDate.Value.Date;

            if (FilePrintedDateDatePicker.SelectedDate != null && FilePrintedByComboBox.Text.Length == 0)
            {
                MessageBox.Show(this, "Please fill in printed by.");
                return;
            }

            if (FilePrintedDateDatePicker.SelectedDate == null && FilePrintedByComboBox.Text.Length > 0)
            {
                MessageBox.Show(this, "Please fill in printed date.");
                return;
            }

            if (FilePrintedDateDatePicker.SelectedDate == null && FilePrintedByComboBox.Text.Length == 0)
            {
                MessageBox.Show(this, "Please fill in both print by and printed date.");
                return;
            }

            PutArchiveLinePrintedInfo();

            this.DialogResult = true;
            this.Close();
        }

        private void PutArchiveLinePrintedInfo()
        {
            //System.Diagnostics.Debug.Print("");
            foreach (IdslTrackerLine trackerLine in trackerLines)
            {
                using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
                {
                    using (SqlCommand command = new SqlCommand("Tracker.dbo.PUT_V2_TRACKER_ARCHIVE_LINE_BULK_PRINT", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                        command.Parameters.AddWithValue("@FilePrintedDate", FilePrintedDateDatePicker.SelectedDate.Value.Date);
                        command.Parameters.AddWithValue("@FilePrintedBy", FilePrintedByComboBox.Text);
                        command.Parameters.AddWithValue("@Username", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                        command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);


                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                    }
                }
            }
        }
    }
}
