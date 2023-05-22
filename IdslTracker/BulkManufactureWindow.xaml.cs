using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;



namespace IdslTracker
{
    public partial class BulkManufactureWindow : Window
    {
        internal List<IdslTrackerLine> trackerLines;
        internal List<IdslTrackerLine> updatedTrackerLines;

        public BulkManufactureWindow()
        {
            InitializeComponent();
        }



        private void DeliveryDateTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
;


            DateTime selectedDate = ((DatePicker)sender).SelectedDate.Value.Date;

            DeliveryMonthTextBox.Text = selectedDate.ToString("MMMM yy");
            DeliveryWeekNumberTextBox.Text = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(selectedDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Globals.lastDoorCommentColour = ProductionCommentDoorColourPicker.SelectedColor;
            Globals.lastFrameCommentColour = ProductionCommentFrameColourPicker.SelectedColor;

            PutArchiveLines();
            GetUpdatedLines();

            this.DialogResult = true;
            this.Close();
        }

        private void PutArchiveLines()
        {
            List<string> ContractList = new List<string>();

            foreach (IdslTrackerLine trackerLine in trackerLines)
            {
                using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
                {
                    using (SqlCommand command = new SqlCommand("Tracker.dbo.PUT_V2_TRACKER_ARCHIVE_LINE_BULK_PRODUCTION_INFO", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                        command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);
                        if (ProductionCommentDoorColourPicker.SelectedColor != null)
                        {
                            command.Parameters.AddWithValue("@ProductionCommentDoorColourHex", ProductionCommentDoorColourPicker.SelectedColor.ToString());
                        }

                        if (ProductionCommentFrameColourPicker.SelectedColor != null)
                        {
                            command.Parameters.AddWithValue("@ProductionCommentFrameColourHex", ProductionCommentFrameColourPicker.SelectedColor.ToString());
                        }

                        if (DeliveryDateDatePicker.SelectedDate != null)
                        {
                            command.Parameters.AddWithValue("@DeliveryDate", DeliveryDateDatePicker.SelectedDate.Value.Date);
                            command.Parameters.AddWithValue("@DeliveryDateOverride", 1);
                        }


                        if (ProductionCommentDoorTextBox.Text.Length > 0)
                        {
                            command.Parameters.AddWithValue("@ProductionCommentDoor", ProductionCommentDoorTextBox.Text);
                        }

                        if (ProductionCommentFrameTextBox.Text.Length > 0)
                        {
                            command.Parameters.AddWithValue("@ProductionCommentFrame", ProductionCommentFrameTextBox.Text);
                        }


                        if (ManufactureCompleteCheckBox.IsEnabled == true)
                        {
                            command.Parameters.AddWithValue("@ManufactureCompleted", (bool)ManufactureCompleteCheckBox.IsChecked ? 1 : 0);

                            if (ManufactureCompleteCheckBox.IsChecked == true)
                            {
                                command.Parameters.AddWithValue("@ManufactureEndDate", DateTime.Now);
                            }
                        }

                        if(IsHighEndFinishCheckBox.IsChecked == true)
                        {

                            command.Parameters.AddWithValue("@IsHighEndFinish", 1);
                        }


                        if (ManufactureRepComboBox.SelectedIndex >= 0)
                        {
                            command.Parameters.AddWithValue("@ManufactureRep", ManufactureRepComboBox.SelectedValue);
                        }


                        command.Parameters.AddWithValue("@Username", System.Security.Principal.WindowsIdentity.GetCurrent().Name);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();



                        string str;
                        str = "update PegasusCopy.dbo.ProductionTracker set DocNr=@DocNr";
                        if (ProductionCommentDoorColourPicker.SelectedColor != null)
                        {
                            str += " ,ProductionCommentDoorColourHex=@ProductionCommentDoorColourHex";
                        }
                        if (ProductionCommentFrameColourPicker.SelectedColor != null)
                        {
                            str += " ,ProductionCommentFrameColourHex=@ProductionCommentFrameColourHex";
                       
                        }

                        if (DeliveryDateDatePicker.SelectedDate != null)
                        {
                            str += " ,DeliveryDate=@DeliveryDate";
                            str += " ,DeliveryDateOverride=@DeliveryDateOverride";
                           
                        }
                        if (ProductionCommentDoorTextBox.Text.Length > 0)
                        {
                            str += " ,ProductionCommentDoor=@ProductionCommentDoor";
                             }

                        if (ProductionCommentFrameTextBox.Text.Length > 0)
                        {
                            str += " ,ProductionCommentFrame=@ProductionCommentFrame";
                         
                        }

                        if (ManufactureCompleteCheckBox.IsEnabled == true)
                        {
                            str += " ,ManufactureCompleted=@ManufactureCompleted";
                           
                            if (ManufactureCompleteCheckBox.IsChecked == true)
                            {
                                str += " ,ManufactureEndDate=@ManufactureEndDate";
                               
                            }
                        }

                        if (IsHighEndFinishCheckBox.IsChecked == true)
                        {
                            str += " ,IsHighEndFinish=@IsHighEndFinish";
                           
                        }
                        if (ManufactureRepComboBox.SelectedIndex >= 0)
                        {
                            str += " ,ManufactureRep=@ManufactureRep";
                          
                        }
                    
                       
                        str += " where DocNr=@DocNr and JobNo=@JobNr ";
                        SqlConnection con = new SqlConnection(Properties.Resources.db);
                        SqlCommand com = new SqlCommand(str, con);
                        if (ProductionCommentDoorColourPicker.SelectedColor != null)
                        {
                            com.Parameters.AddWithValue("@ProductionCommentDoorColourHex", ProductionCommentDoorColourPicker.SelectedColor.ToString());
                        }

                        if (ProductionCommentFrameColourPicker.SelectedColor != null)
                        {
                            com.Parameters.AddWithValue("@ProductionCommentFrameColourHex", ProductionCommentFrameColourPicker.SelectedColor.ToString());
                        }

                        if (DeliveryDateDatePicker.SelectedDate != null)
                        {
                            com.Parameters.AddWithValue("@DeliveryDate", DeliveryDateDatePicker.SelectedDate.Value.Date);
                            com.Parameters.AddWithValue("@DeliveryDateOverride", 1);
                        }

                        if (ProductionCommentDoorTextBox.Text.Length > 0)
                        {
                            com.Parameters.AddWithValue("@ProductionCommentDoor", ProductionCommentDoorTextBox.Text);
                        }

                        if (ProductionCommentFrameTextBox.Text.Length > 0)
                        {
                            com.Parameters.AddWithValue("@ProductionCommentFrame", ProductionCommentFrameTextBox.Text);
                        }

                        if (ManufactureCompleteCheckBox.IsEnabled == true)
                        {
                            com.Parameters.AddWithValue("@ManufactureCompleted", (bool)ManufactureCompleteCheckBox.IsChecked ? 1 : 0);

                            if (ManufactureCompleteCheckBox.IsChecked == true)
                            {
                                com.Parameters.AddWithValue("@ManufactureEndDate", DateTime.Now);
                            }
                        }
                        if (IsHighEndFinishCheckBox.IsChecked == true)
                        {

                            com.Parameters.AddWithValue("@IsHighEndFinish", 1);
                        }
                        if (ManufactureRepComboBox.SelectedIndex >= 0)
                        {
                            com.Parameters.AddWithValue("@ManufactureRep", ManufactureRepComboBox.SelectedValue);
                        }

                        com.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                        com.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);



                        if (con.State == ConnectionState.Open) { con.Close(); }
                        con.Open();
                        com.ExecuteNonQuery();
                        con.Close();
                       
                        if (ManufactureCompleteCheckBox.IsEnabled == true || DeliveryDateDatePicker.SelectedDate != null)
                        {
                            if(ManufactureCompleteCheckBox.IsChecked == true || DeliveryDateDatePicker.SelectedDate != null)
                            {
                                ContractList.Add(trackerLine.Contract);
                               
                            }
                          
                        }

                     

                    }
                }
            }

            if (ContractList.Count > 0)
            {
                List<string> UniqueContractList = ContractList.Distinct().ToList<string>();
                if (UniqueContractList.Count > 0)
                {

                    foreach (String contract in UniqueContractList)
                    {
                        string str;
                        SqlConnection con = new SqlConnection(Properties.Resources.db);
                        str = "Tracker.dbo.Update_MANUFACTURE_DETAIL_VALUES_AfterTrackerUpdate";
                        SqlCommand comUpdate = new SqlCommand(str, con);
                        comUpdate.CommandType = CommandType.StoredProcedure;
                        comUpdate.CommandTimeout = 300;
                        comUpdate.Parameters.AddWithValue("@Contract", contract);
                        if (con.State == ConnectionState.Open) { con.Close(); }
                        con.Open();
                        comUpdate.ExecuteNonQuery();
                        con.Close();
                    }
                }

            }


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
                using (SqlCommand command = new SqlCommand("select * from PegasusCopy.dbo.v_ProductionTracker where ReportId=3 order by 5,16", connection))
               // using (SqlCommand command = new SqlCommand("select * from PegasusCopy.dbo.v_ProductionTracker  order by 5,16", connection))
                {
                    {
                        command.CommandTimeout = 180;


                        command.CommandType = CommandType.Text;
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            int i;
                            for (i = 0; i <= dt.Rows.Count - 1; i++)
                            {

                                IdslTrackerLine trackerLine = new IdslTrackerLine();

                                trackerLine.DocNumber = Convert.ToString(dt.Rows[i]["DocNr"]);
                                trackerLine.ManfSite = Convert.ToString(dt.Rows[i]["ManfSite"]);
                                trackerLine.Contract = Convert.ToString(dt.Rows[i]["Contract"] + "");
                                trackerLine.JobNo = Convert.ToString(dt.Rows[i]["JobNo"]);
                                trackerLine.BatchRef = Convert.ToString(dt.Rows[i]["BatchRef"] + "");
                                trackerLine.ProductType = Convert.ToString(dt.Rows[i]["ProductType"] + "");
                                trackerLine.DoorQty = Convert.ToInt32(dt.Rows[i]["DoorQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["DoorQty"]);
                                trackerLine.FrameQty = Convert.ToInt32(dt.Rows[i]["FrameQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["FrameQty"]);
                                trackerLine.PanelQty = Convert.ToInt32(dt.Rows[i]["PanelQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["PanelQty"]);
                                trackerLine.ScreenQty = Convert.ToInt32(dt.Rows[i]["ScreenQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["ScreenQty"]);
                                trackerLine.MiscQty = Convert.ToInt32(dt.Rows[i]["MiscQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["MiscQty"]);
                                trackerLine.IronmongeryQty = Convert.ToInt32(dt.Rows[i]["IronmongeryQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["IronmongeryQty"]);
                                trackerLine.Sales = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Sales"] + "")) ? Convert.ToDecimal(dt.Rows[i]["Sales"]) : 0;
                                trackerLine.Customer = Convert.ToString(dt.Rows[i]["Customer"]);
                                if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["DeliveryDate"]), out DateTime Temp) == true)
                                {
                                    trackerLine.DeliveryDate = Convert.ToDateTime(dt.Rows[i]["DeliveryDate"]);

                                }
                                else { }
                                if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["FtbDeliveryDate"]), out DateTime Temp1) == true)
                                {
                                    trackerLine.FtbDeliveryDate = Convert.ToDateTime(dt.Rows[i]["FtbDeliveryDate"]);

                                }
                                else { }

                                trackerLine.LastStageDoor = Convert.ToString(dt.Rows[i]["LastStageDoor"]);
                                trackerLine.ProductionCommentDoor = Convert.ToString(dt.Rows[i]["ProductionCommentDoor"]);
                                trackerLine.ReportId = Convert.ToInt32(dt.Rows[i]["ReportID"]);
                                trackerLine.FilePrintedBy = Convert.ToString(dt.Rows[i]["FilePrintedBy"]);
                                if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["FilePrintedDate"]), out DateTime Temp2) == true)
                                { trackerLine.FilePrintedDate = Convert.ToDateTime(dt.Rows[i]["FilePrintedDate"]); }
                                else { }
                                trackerLine.PegasusDoorQty = Convert.ToInt32(dt.Rows[i]["PegasusDoorQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["PegasusDoorQty"]);
                                trackerLine.PegasusFrameQty = Convert.ToInt32(dt.Rows[i]["PegasusFrameQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["PegasusFrameQty"]);
                                trackerLine.PegasusPanelQty = Convert.ToInt32(dt.Rows[i]["PegasusPanelQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["PegasusPanelQty"]);
                                trackerLine.PegasusScreenQty = Convert.ToInt32(dt.Rows[i]["PegasusScreenQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["PegasusScreenQty"]);
                                trackerLine.PegasusMiscQty = Convert.ToInt32(dt.Rows[i]["PegasusMiscQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["PegasusMiscQty"]);
                                trackerLine.PegasusIronmongeryQty = Convert.ToInt32(dt.Rows[i]["IronmongeryQty"]) == 0 ? (Nullable<int>)null : Convert.ToInt32(dt.Rows[i]["PegasusIronmongeryQty"]);
                                trackerLine.DeliveryDateOverride = Convert.ToInt32(dt.Rows[i]["DeliveryDateOverride"]);
                                trackerLine.DeliveryRiskMaterials = Convert.ToString(dt.Rows[i]["DeliveryRiskMaterials"]);
                                trackerLine.CustomerStatus = Convert.ToString(dt.Rows[i]["CustomerStatus"]);
                                trackerLine.MaterialComment = Convert.ToString(dt.Rows[i]["MaterialComment"]);
                                trackerLine.SchedulingContact = Convert.ToString(dt.Rows[i]["SchedulingContact"]);
                                trackerLine.SalesContact = Convert.ToString(dt.Rows[i]["SalesContact"]);
                                trackerLine.ProcurementContact = Convert.ToString(dt.Rows[i]["ProcurementContact"]);
                                trackerLine.PjlFileHasBeenPrinted = Convert.ToBoolean(dt.Rows[i]["PjlFileHasBeenPrinted"]);
                                trackerLine.ManufactureCompleted = Convert.ToBoolean(dt.Rows[i]["ManufactureCompleted"]);
                                if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["ManufactureEndDate"]), out DateTime Temp3) == true)
                                { trackerLine.ManufactureEndDate = Convert.ToDateTime(dt.Rows[i]["ManufactureEndDate"]); }
                                else { }
                                trackerLine.ManufactureRep = Convert.ToString(dt.Rows[i]["ManufactureRep"]);
                                trackerLine.StorageRef = Convert.ToString(dt.Rows[i]["StorageRef"]);
                                trackerLine.ProductionCommentFrame = Convert.ToString(dt.Rows[i]["ProductionCommentFrame"]);
                                trackerLine.LastStageFrame = Convert.ToString(dt.Rows[i]["LastStageFrame"]);
                                trackerLine.HasBeenProcured = Convert.ToBoolean(dt.Rows[i]["HasBeenProcured"]);
                                if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["ManufactureStartDate"]), out DateTime Temp4) == true)
                                { trackerLine.ManufactureStartDate = Convert.ToDateTime(dt.Rows[i]["ManufactureStartDate"]); }
                                else { }

                                if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["InvoicedDate"]), out DateTime Temp5) == true)
                                { trackerLine.InvoicedDate = Convert.ToDateTime(dt.Rows[i]["InvoicedDate"]); }
                                else { }

                                trackerLine.CountWeeksHeld = Convert.ToInt32(dt.Rows[i]["CountWeeksHeld"]);

                                if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["SopCreatedDate"]), out DateTime Temp6) == true)
                                { trackerLine.SopCreatedDate = Convert.ToDateTime(dt.Rows[i]["SopCreatedDate"]); }
                                else { }
                                trackerLine.ManualMaterialComment = Convert.ToString(dt.Rows[i]["ManualMaterialComment"]);
                                trackerLine.WeekNum = GetIso8601WeekNumber(trackerLine.DeliveryDate ?? DateTime.MinValue);
                                trackerLine.ProductionCommentDoorColourHex = Convert.ToString(dt.Rows[i]["ProductionCommentDoorColourHex"]);
                                trackerLine.ProductionCommentFrameColourHex = Convert.ToString(dt.Rows[i]["ProductionCommentFrameColourHex"]);
                                trackerLine.ManfRepAbv = Convert.ToString(dt.Rows[i]["ManfRepAbv"]);
                                trackerLine.ShopfloorComment = Convert.ToString(dt.Rows[i]["ShopfloorComment"]);
                                trackerLine.IsAccrued = Convert.ToBoolean(dt.Rows[i]["IsAccrued"]);
                                trackerLine.IsHighEndFinish = Convert.ToBoolean(dt.Rows[i]["IsHighEndFinish"]);
                                trackerLine.WipCommentary = Convert.ToString(dt.Rows[i]["WipCommentary"]);
                                trackerLine.WipFrameCommentary = Convert.ToString(dt.Rows[i]["WipFrameCommentary"]);
                                trackerLine.RiskComment = Convert.ToString(dt.Rows[i]["RiskComment"]);
                                //trackerLine.RiskRowColorHex = Convert.ToString(dt.Rows[i]["RiskRowColorHex"]);

                                                     


                                


                            }
                        }
                       

                                           }
                }
            }
        }

        //private void GetUpdatedLines()
        //{

        //    this.updatedTrackerLines = new List<IdslTrackerLine>();

        //    string docNrs = string.Empty;
        //    foreach (IdslTrackerLine trackerLine in trackerLines)
        //    {
        //        docNrs = string.Format("{0}{1}|", docNrs, trackerLine.DocNumber);

        //    }
        //    docNrs = docNrs.Remove(docNrs.Length - 1, 1);

        //    using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
        //    {
        //        using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_IDSL_V2_TRACKER_LINES", connection))
        //        {
        //            command.CommandTimeout = 180;
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.Parameters.AddWithValue("@ReportId", 3);
        //            command.Parameters.AddWithValue("@Doc", docNrs);
        //            connection.Open();

        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {

        //                while (reader.Read())
        //                {
        //                    IdslTrackerLine trackerLine = new IdslTrackerLine();


        //                    trackerLine.DocNumber = reader.GetString(0);
        //                    trackerLine.ManfSite = reader.GetString(1);
        //                    trackerLine.Contract = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
        //                    trackerLine.JobNo = reader.GetString(3);
        //                    trackerLine.BatchRef = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
        //                    trackerLine.ProductType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        //                    trackerLine.DoorQty = Convert.ToInt32(reader.GetValue(6)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(6));
        //                    trackerLine.FrameQty = Convert.ToInt32(reader.GetValue(7)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(7));
        //                    trackerLine.PanelQty = Convert.ToInt32(reader.GetValue(8)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(8));
        //                    trackerLine.ScreenQty = Convert.ToInt32(reader.GetValue(9)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(9));
        //                    trackerLine.MiscQty = Convert.ToInt32(reader.GetValue(10)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(10));
        //                    trackerLine.IronmongeryQty = Convert.ToInt32(reader.GetValue(11)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(11));
        //                    trackerLine.Sales = reader.IsDBNull(12) ? 0 : Convert.ToDecimal(reader.GetValue(12));
        //                    trackerLine.Customer = reader.GetString(13);
        //                    trackerLine.DeliveryDate = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14);
        //                    //trackerLine.FtbDeliveryDate = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15);
        //                    trackerLine.LastStageDoor = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
        //                    trackerLine.ProductionCommentDoor = reader.IsDBNull(17) ? string.Empty : reader.GetString(17);
        //                    trackerLine.ReportId = Convert.ToInt32(reader.GetValue(18));
        //                    trackerLine.FilePrintedBy = reader.IsDBNull(19) ? string.Empty : reader.GetString(19);
        //                    trackerLine.FilePrintedDate = reader.IsDBNull(20) ? (DateTime?)null : reader.GetDateTime(20);
        //                    trackerLine.PegasusDoorQty = Convert.ToInt32(reader.GetValue(21)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(21));
        //                    trackerLine.PegasusFrameQty = Convert.ToInt32(reader.GetValue(22)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(22));
        //                    trackerLine.PegasusPanelQty = Convert.ToInt32(reader.GetValue(23)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(23));
        //                    trackerLine.PegasusScreenQty = Convert.ToInt32(reader.GetValue(24)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(24));
        //                    trackerLine.PegasusMiscQty = Convert.ToInt32(reader.GetValue(25)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(25));
        //                    trackerLine.PegasusIronmongeryQty = Convert.ToInt32(reader.GetValue(26)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(26));
        //                    trackerLine.DeliveryDateOverride = Convert.ToInt32(reader.GetValue(27));
        //                    trackerLine.DeliveryRiskMaterials = reader.IsDBNull(28) ? string.Empty : reader.GetString(28);
        //                    trackerLine.CustomerStatus = reader.IsDBNull(29) ? string.Empty : reader.GetString(29);
        //                    trackerLine.MaterialComment = reader.IsDBNull(30) ? string.Empty : reader.GetString(30);
        //                    trackerLine.SchedulingContact = reader.IsDBNull(31) ? string.Empty : reader.GetString(31);
        //                    trackerLine.SalesContact = reader.IsDBNull(32) ? string.Empty : reader.GetString(32);
        //                    trackerLine.ProcurementContact = reader.IsDBNull(33) ? string.Empty : reader.GetString(33);
        //                    trackerLine.PjlFileHasBeenPrinted = reader.GetBoolean(34);
        //                    trackerLine.ManufactureCompleted = reader.GetBoolean(35);
        //                    trackerLine.ManufactureEndDate = reader.IsDBNull(36) ? (DateTime?)null : reader.GetDateTime(36);
        //                    trackerLine.ManufactureRep = reader.IsDBNull(37) ? string.Empty : reader.GetString(37);
        //                    trackerLine.StorageRef = reader.IsDBNull(38) ? string.Empty : reader.GetString(38);
        //                    trackerLine.ProductionCommentFrame = reader.IsDBNull(39) ? string.Empty : reader.GetString(39);
        //                    trackerLine.LastStageFrame = reader.IsDBNull(40) ? string.Empty : reader.GetString(40);
        //                    trackerLine.HasBeenProcured = reader.GetBoolean(41);
        //                    trackerLine.ManufactureStartDate = reader.IsDBNull(42) ? (DateTime?)null : reader.GetDateTime(42);
        //                    trackerLine.InvoicedDate = reader.IsDBNull(43) ? (DateTime?)null : reader.GetDateTime(43);
        //                    trackerLine.CountWeeksHeld = reader.IsDBNull(44) ? 0 : Convert.ToDecimal(reader.GetValue(44));
        //                    trackerLine.SopCreatedDate = reader.IsDBNull(45) ? (DateTime?)null : reader.GetDateTime(45);
        //                    trackerLine.ManualMaterialComment = reader.IsDBNull(46) ? string.Empty : reader.GetString(46);
        //                    trackerLine.WeekNum = GetIso8601WeekNumber(trackerLine.DeliveryDate ?? DateTime.MinValue);
        //                    trackerLine.ProductionCommentDoorColourHex = reader.IsDBNull(47) ? string.Empty : reader.GetString(47);
        //                    trackerLine.ProductionCommentFrameColourHex = reader.IsDBNull(48) ? string.Empty : reader.GetString(48);
        //                    trackerLine.ManfRepAbv = reader.IsDBNull(50) ? string.Empty : reader.GetString(50);
        //                    trackerLine.ShopfloorComment = reader.IsDBNull(51) ? string.Empty : reader.GetString(51);
        //                    trackerLine.IsAccrued = reader.GetBoolean(52);
        //                    trackerLine.IsHighEndFinish = reader.GetBoolean(56);

        //                    updatedTrackerLines.Add(trackerLine);
        //                }
        //            }
        //        }
        //    }
        //}

        private int GetIso8601WeekNumber(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return 0;
            }

            var thursday = date.AddDays(3 - ((int)date.DayOfWeek + 6) % 7);
            return 1 + (thursday.DayOfYear - 1) / 7;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            ProductionCommentDoorColourPicker.SelectedColor = Globals.lastDoorCommentColour;
            ProductionCommentFrameColourPicker.SelectedColor = Globals.lastFrameCommentColour;


            int countOfLines = trackerLines.Count;
            int countOfLinesManfComplete = 0;

            string baseDoorComment = trackerLines[0].ProductionCommentDoor;
            string baseFrameComment = trackerLines[0].ProductionCommentFrame;
            int countOfLinesWithSameDoorComment = 0;
            int countOfLinesWithSameFrameComment = 0;
            foreach (IdslTrackerLine trackerLine in trackerLines)
            {
                if(trackerLine.ManufactureCompleted == true)
                {
                    countOfLinesManfComplete++;
                }

                if(trackerLine.ProductionCommentDoor == baseDoorComment)
                {
                    countOfLinesWithSameDoorComment++;
                }

                if (trackerLine.ProductionCommentFrame == baseFrameComment)
                {
                    countOfLinesWithSameFrameComment++;
                }

            }

            if(countOfLinesManfComplete != 0 && countOfLinesManfComplete != countOfLines)
            {
                ManufactureCompleteCheckBox.IsEnabled = false;
                ManfCompleteDisabledAlert.Visibility = Visibility.Visible;
            }
            else
            {
                ManfCompleteDisabledAlert.Visibility = Visibility.Hidden;
                if (countOfLinesManfComplete == 0)
                {
                    ManufactureCompleteCheckBox.IsChecked = false;
                }
                else
                {
                    ManufactureCompleteCheckBox.IsChecked = true;
                }


            }

            if (countOfLinesWithSameDoorComment == countOfLines)
            {
                ProductionCommentDoorTextBox.Text = baseDoorComment;
            }

            if (countOfLinesWithSameFrameComment == countOfLines)
            {
                ProductionCommentFrameTextBox.Text = baseFrameComment;
            }

            foreach (string name in Globals.ManufactureReps)
            {
                ManufactureRepComboBox.Items.Add(name);
            }


        }

        private void ColourPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFrom(((ColorPicker)sender).SelectedColor.ToString());
            ((ColorPicker)sender).Background = brush;

        }

        private void ManufactureCompleteCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ManufactureCompleteCheckBox.Click -= new RoutedEventHandler(ManufactureCompleteCheckBox_Click);
            
            if(ManufactureCompleteCheckBox.IsChecked == false)
            {
                ManufactureCompleteCheckBox.IsChecked = true;
                MessageBoxResult resultMessageBox = System.Windows.MessageBox.Show(
                    "Job(s) already completed, would you like to mark the job(s) as not completed?",
                    "Are you sure?  ",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (resultMessageBox == MessageBoxResult.Yes)
                {
                    ManufactureCompleteCheckBox.IsChecked = false;
                }
            }

            ManufactureCompleteCheckBox.Click += new RoutedEventHandler(ManufactureCompleteCheckBox_Click);





        }
    }
}
