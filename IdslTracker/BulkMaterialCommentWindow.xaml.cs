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
    /// Interaction logic for BulkMaterialCommentWindow.xaml
    /// </summary>
    public partial class BulkMaterialCommentWindow : Window
    {
        internal List<IdslTrackerLine> trackerLines;
        internal List<IdslTrackerLine> updatedTrackerLines;

        public BulkMaterialCommentWindow()
        {
            InitializeComponent();
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {

            if (MaterialCommentDoorTextBox.Text.Length == 0)
            {
                MessageBox.Show(this, "Please fill in a comment.");
                return;
            }

            PutArchiveLineMaterialComment();
            GetUpdatedLines();
            
            this.DialogResult = true;
            this.Close();
        }

        private void GetUpdatedLines()
        {

            this.updatedTrackerLines = new List<IdslTrackerLine>();

            string docNrs = string.Empty;
            foreach (IdslTrackerLine trackerLine in trackerLines)
            {
                docNrs = string.Format("{0}{1}|", docNrs, trackerLine.DocNumber);

            }
            docNrs = docNrs.Remove(docNrs.Length - 1, 1);

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                // using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_IDSL_V2_TRACKER_LINES", connection))

                using (SqlCommand command = new SqlCommand("select * from  PegasusCopy .dbo.ProductionTracker where ReportID=@ReportId and DocNr =@Doc", connection))
                {
                    command.CommandTimeout = 180;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ReportId", 3);
                    command.Parameters.AddWithValue("@Doc", docNrs);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            IdslTrackerLine trackerLine = new IdslTrackerLine();


                            trackerLine.DocNumber = reader.GetString(0);
                            trackerLine.ManfSite = reader.GetString(1);
                            trackerLine.Contract = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            trackerLine.JobNo = reader.GetString(3);
                            trackerLine.BatchRef = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            trackerLine.ProductType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            trackerLine.DoorQty = Convert.ToInt32(reader.GetValue(6)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(6));
                            trackerLine.FrameQty = Convert.ToInt32(reader.GetValue(7)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(7));
                            trackerLine.PanelQty = Convert.ToInt32(reader.GetValue(8)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(8));
                            trackerLine.ScreenQty = Convert.ToInt32(reader.GetValue(9)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(9));
                            trackerLine.MiscQty = Convert.ToInt32(reader.GetValue(10)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(10));
                            trackerLine.IronmongeryQty = Convert.ToInt32(reader.GetValue(11)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(11));
                            trackerLine.Sales = reader.IsDBNull(12) ? 0 : Convert.ToDecimal(reader.GetValue(12));
                            trackerLine.Customer = reader.GetString(13);
                            trackerLine.DeliveryDate = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14);
                            //trackerLine.FtbDeliveryDate = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15);
                            trackerLine.LastStageDoor = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
                            trackerLine.ProductionCommentDoor = reader.IsDBNull(17) ? string.Empty : reader.GetString(17);
                            trackerLine.ReportId = Convert.ToInt32(reader.GetValue(18));
                            trackerLine.FilePrintedBy = reader.IsDBNull(19) ? string.Empty : reader.GetString(19);
                            trackerLine.FilePrintedDate = reader.IsDBNull(20) ? (DateTime?)null : reader.GetDateTime(20);
                            trackerLine.PegasusDoorQty = Convert.ToInt32(reader.GetValue(21)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(21));
                            trackerLine.PegasusFrameQty = Convert.ToInt32(reader.GetValue(22)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(22));
                            trackerLine.PegasusPanelQty = Convert.ToInt32(reader.GetValue(23)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(23));
                            trackerLine.PegasusScreenQty = Convert.ToInt32(reader.GetValue(24)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(24));
                            trackerLine.PegasusMiscQty = Convert.ToInt32(reader.GetValue(25)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(25));
                            trackerLine.PegasusIronmongeryQty = Convert.ToInt32(reader.GetValue(26)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(26));
                            trackerLine.DeliveryDateOverride = Convert.ToInt32(reader.GetValue(27));
                            trackerLine.DeliveryRiskMaterials = reader.IsDBNull(28) ? string.Empty : reader.GetString(28);
                            trackerLine.CustomerStatus = reader.IsDBNull(29) ? string.Empty : reader.GetString(29);
                            trackerLine.MaterialComment = reader.IsDBNull(30) ? string.Empty : reader.GetString(30);
                            trackerLine.SchedulingContact = reader.IsDBNull(31) ? string.Empty : reader.GetString(31);
                            trackerLine.SalesContact = reader.IsDBNull(32) ? string.Empty : reader.GetString(32);
                            trackerLine.ProcurementContact = reader.IsDBNull(33) ? string.Empty : reader.GetString(33);
                            trackerLine.PjlFileHasBeenPrinted = reader.GetBoolean(34);
                            trackerLine.ManufactureCompleted = reader.GetBoolean(35);
                            trackerLine.ManufactureEndDate = reader.IsDBNull(36) ? (DateTime?)null : reader.GetDateTime(36);
                            trackerLine.ManufactureRep = reader.IsDBNull(37) ? string.Empty : reader.GetString(37);
                            trackerLine.StorageRef = reader.IsDBNull(38) ? string.Empty : reader.GetString(38);
                            trackerLine.ProductionCommentFrame = reader.IsDBNull(39) ? string.Empty : reader.GetString(39);
                            trackerLine.LastStageFrame = reader.IsDBNull(40) ? string.Empty : reader.GetString(40);
                            trackerLine.HasBeenProcured = reader.GetBoolean(41);
                            trackerLine.ManufactureStartDate = reader.IsDBNull(42) ? (DateTime?)null : reader.GetDateTime(42);
                            trackerLine.InvoicedDate = reader.IsDBNull(43) ? (DateTime?)null : reader.GetDateTime(43);
                            trackerLine.CountWeeksHeld = reader.IsDBNull(44) ? 0 : Convert.ToDecimal(reader.GetValue(44));
                            trackerLine.SopCreatedDate = reader.IsDBNull(45) ? (DateTime?)null : reader.GetDateTime(45);
                            trackerLine.ManualMaterialComment = reader.IsDBNull(46) ? string.Empty : reader.GetString(46);
                            trackerLine.WeekNum = GetIso8601WeekNumber(trackerLine.DeliveryDate ?? DateTime.MinValue);

                            updatedTrackerLines.Add(trackerLine);
                        }
                    }
                }
            }
        }

        private int GetIso8601WeekNumber(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return 0;
            }

            var thursday = date.AddDays(3 - ((int)date.DayOfWeek + 6) % 7);
            return 1 + (thursday.DayOfYear - 1) / 7;
        }


        private void PutArchiveLineMaterialComment()
        {
            foreach (IdslTrackerLine trackerLine in trackerLines)
            {
                using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
                {
                    using (SqlCommand command = new SqlCommand("Tracker.dbo.PUT_V2_TRACKER_ARCHIVE_LINE_BULK_MATERIAL_COMMENT", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                        command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);

                        command.Parameters.AddWithValue("@MaterialComment", MaterialCommentDoorTextBox.Text);
                        
                        command.Parameters.AddWithValue("@Username", System.Security.Principal.WindowsIdentity.GetCurrent().Name);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        string str;
                        str = "update PegasusCopy.dbo.ProductionTracker set ManualMaterialComment='" + MaterialCommentDoorTextBox.Text.Trim().Replace("'", "") + "' where DocNr='" + trackerLine.DocNumber.Trim().Replace("'", "") + "' and JobNo='" + trackerLine.JobNo.Trim().Replace("'", "") + "' ";
                        SqlConnection con = new SqlConnection(Properties.Resources.db);
                        SqlCommand com = new SqlCommand(str, con);
                        if (con.State == ConnectionState.Open) { con.Close(); }
                        con.Open();
                        com.ExecuteNonQuery();
                        con.Close();

                    }
                }
            }
        }
    }
}
