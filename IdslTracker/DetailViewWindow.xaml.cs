using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace IdslTracker
{
    /// <summary>
    /// Interaction logic for DetailViewWindow.xaml
    /// </summary>
    public partial class DetailViewWindow : Window
    {

        IdslTrackerLine trackerLine;

        public DetailViewWindow(IdslTrackerLine selectedTrackerLine, string dataGridName)
        {
            InitializeComponent();

            this.trackerLine = selectedTrackerLine;
            this.DataContext = this.trackerLine;
            Title = String.Format("IDSL | Production Tracker | Edit | {0} | {1} | {2}", trackerLine.Contract, trackerLine.JobNo, trackerLine.DocNumber);

            if(dataGridName == "IronTrackerDataGrid")
            {
                LabelDoorComment.Content = "Production Comment Iron:";
                LabelFrameComment.Visibility = Visibility.Hidden;
                ProductionCommentFrameTextBox.Visibility = Visibility.Hidden;
            }
            else
            {
                LabelDoorComment.Content = "Production Comment Door:";
                LabelFrameComment.Visibility = Visibility.Visible;
                ProductionCommentFrameTextBox.Visibility = Visibility.Visible;

            }

            foreach (string name in Globals.ManfSites)
            {
                ManfSiteComboBox.Items.Add(name);
            }

            foreach (string name in Globals.PrintedByNames)
            {
                FilePrintedByComboBox.Items.Add(name);
            }

            foreach (string name in Globals.ManufactureReps)
            {
                ManufactureRepComboBox.Items.Add(name);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //if (Globals.IsPowerUser == true)
            //{
            //    ClearPrintedDateBtn.Visibility = Visibility.Visible;
            //}

            PopulateHistoryTable();
            PopulateSalesOrdersTable();
            PopulateMaterialAllocationsTable();
            PopulateContactsTable();
            PopulateWipHistoryTable();
            PopulateAtRiskTable();
        }

        private void PopulateWipHistoryTable()
        {
            List<WipCommentary> wipCommentaryLines = new List<WipCommentary>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_WIP_COMMENTARY_HISTORY", connection))
                {
                    command.CommandType = CommandType.StoredProcedure; ;
                    command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                    command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WipCommentary wipCommentaryLine = new WipCommentary
                            {
                                ProductionComment = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                WIPStation = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                Risk = reader.GetBoolean(2),
                                Timestamp = reader.GetDateTime(3),
                                Username = reader.GetString(4),
                                LastUpdatedBy = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                LastUpdatedDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                UpdateRemark = reader.IsDBNull(5) ? string.Empty : reader.GetString(7)

                            };




                            wipCommentaryLines.Add(wipCommentaryLine);
                        }
                    }
                }


            }
            WipHistoryDataGrid.ItemsSource = wipCommentaryLines;
        }



        private void PopulateAtRiskTable()
        {
            List<WipCommentary> wipCommentaryLines = new List<WipCommentary>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("select * from v_TrackerAtRisk where DocNr=@DocNr and JobNo=@JobNr", connection))
                {
                    command.CommandType = CommandType.Text; ;
                    command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                    command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        int i;
                        for (i = 0; i <= dt.Rows.Count - 1; i++)
                        {
                            WipCommentary wipCommentaryAtRisk = new WipCommentary();
                            wipCommentaryAtRisk.DocNr = Convert.ToString(dt.Rows[i]["DocNr"]);
                            wipCommentaryAtRisk.ProductionComment = Convert.ToString(dt.Rows[i]["ProductionComment"]);
                            wipCommentaryAtRisk.WIPStation = Convert.ToString(dt.Rows[i]["WIPStation"]);
                            wipCommentaryAtRisk.Risk = Convert.ToBoolean(dt.Rows[i]["Risk"]);
                            wipCommentaryAtRisk.Timestamp = Convert.ToDateTime(dt.Rows[i]["Timestamp"]);
                            wipCommentaryAtRisk.Username = Convert.ToString(dt.Rows[i]["Username"]);
                            wipCommentaryAtRisk.LastUpdatedBy = Convert.ToString(dt.Rows[i]["LastUpdatedBy"]);
                            wipCommentaryAtRisk.UpdateRemark = Convert.ToString(dt.Rows[i]["UpdateRemark"]);
                            if (DateTime.TryParse(Convert.ToString(dt.Rows[i]["LastUpdatedDate"]), out DateTime Temp) == true)
                            {
                                wipCommentaryAtRisk.LastUpdatedDate = Convert.ToDateTime(dt.Rows[i]["LastUpdatedDate"]);

                            }
                            else { }

                            wipCommentaryLines.Add(wipCommentaryAtRisk);
                        }
                    }

                                            
                }


            }
            AtRiskDataGrid.ItemsSource = wipCommentaryLines;
        }

        private void PopulateContactsTable()
        {
            List<ContactLine> contactLines = new List<ContactLine>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("FTB.dbo.GET_CONTACT_DETAILS", connection))
                {
                    command.CommandType = CommandType.StoredProcedure; ;
                    command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ContactLine contactLine = new ContactLine();

                            contactLine.Contact = reader.IsDBNull(0) ? null : reader.GetString(0);
                            contactLine.Name = reader.IsDBNull(1) ? null : reader.GetString(1);
                            contactLine.Number = reader.IsDBNull(2) ? null : reader.GetString(2);




                            contactLines.Add(contactLine);
                        }
                    }
                }


            }
            ContactsDataGrid.ItemsSource = contactLines;
        }

        private void PopulateMaterialAllocationsTable()
        {
            List<PegasusOrdersLine> pegasusOrderLines = new List<PegasusOrdersLine>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("PegasusCopy.dbo.GET_PEGASUS_MATERIAL_ALLOCATIONS_BY_PHASE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;;
                    command.Parameters.AddWithValue("@Phase", trackerLine.JobNo);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PegasusOrdersLine pegasusOrderLine = new PegasusOrdersLine();

                            pegasusOrderLine.Reference = reader.GetString(0);
                            pegasusOrderLine.CostCode = reader.IsDBNull(1) ? null : reader.GetString(1);
                            pegasusOrderLine.StockCode = reader.GetString(2);
                            pegasusOrderLine.Desc = reader.GetString(3);
                            pegasusOrderLine.ExtendedDesc = reader.GetString(4);
                            pegasusOrderLine.Quantity = reader.IsDBNull(5) ? (Nullable<Decimal>)null : Convert.ToDecimal(reader.GetValue(5));
                            pegasusOrderLine.Price = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                            pegasusOrderLine.SupplierName = reader.IsDBNull(7) ? null : reader.GetString(7);
                            pegasusOrderLine.DateRequired = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8);
                            pegasusOrderLine.DateReceived = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9);
                            pegasusOrderLine.RowERR = reader.GetInt32(10);
                            pegasusOrderLine.Warehouse = reader.GetString(11);
                            pegasusOrderLine.QuantityReceived = reader.IsDBNull(12) ? (Nullable<Decimal>)null : Convert.ToDecimal(reader.GetValue(12));
                            pegasusOrderLine.ProcurementComments = reader.IsDBNull(13) ? string.Empty : reader.GetString(13);
                            pegasusOrderLine.DateQuoted = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14);
                            pegasusOrderLine.DateCreated = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15);




                            pegasusOrderLines.Add(pegasusOrderLine);
                        }
                    }
                }


            }
            PegasusOrdersDataGrid.ItemsSource = pegasusOrderLines;
        }

        private void PopulateSalesOrdersTable()
        {
            List<SalesOrderLine> salesOrderLines = new List<SalesOrderLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("PegasusCopy.dbo.GET_SALES_ORDER", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                    //command.Parameters.AddWithValue("@Orders", trackerLine.PegasusOrders);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SalesOrderLine salesOrderLine = new SalesOrderLine();

                            salesOrderLine.Stock = reader.GetString(0);
                            salesOrderLine.Description = reader.GetString(1);
                            salesOrderLine.Ordered = reader.IsDBNull(2) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(2));
                            salesOrderLine.Delivered = reader.IsDBNull(3) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(3));
                            salesOrderLine.Invoiced = reader.IsDBNull(4) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(4));
                            salesOrderLine.Memo = reader.IsDBNull(5) ? null : reader.GetString(5);




                            salesOrderLines.Add(salesOrderLine);
                        }
                    }
                }

            }
            SalesOrderDataGrid.ItemsSource = salesOrderLines;
        }

        private void PopulateHistoryTable()
        {
            List<HistoryLine> historyLines = new List<HistoryLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_HISTORICAL_VIEW", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HistoryLine historyLine = new HistoryLine();


                            historyLine.ManfSite = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            historyLine.ProductionCommentDoor = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            historyLine.DoorQty = reader.IsDBNull(3) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(3));
                            historyLine.FrameQty = reader.IsDBNull(4) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(4));
                            historyLine.PanelQty = reader.IsDBNull(5) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(5));
                            historyLine.ScreenQty = reader.IsDBNull(6) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(6));
                            historyLine.MiscQty = reader.IsDBNull(7) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(7));
                            historyLine.IronmongeryQty = reader.IsDBNull(8) ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(8));
                            historyLine.DeliveryDate = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9);
                            historyLine.Timestamp = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10);
                            historyLine.Username = reader.GetString(11);
                            historyLine.FilePrintedDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12);
                            historyLine.FilePrintedBy = reader.IsDBNull(13) ? string.Empty : reader.GetString(13);
                            historyLine.ManufactureCompleted = reader.GetBoolean(15);
                            historyLine.ProductionCommentFrame = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
                            historyLine.ManualMaterialComment = reader.IsDBNull(17) ? string.Empty : reader.GetString(17);




                            historyLines.Add(historyLine);
                        }
                    }
                }

            }
            HistoryDataGrid.ItemsSource = historyLines;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ItemChanged(object sender, RoutedEventArgs e)
        {
            updateBtn.Visibility = Visibility.Visible;
            //NotImplementedException ff = new NotImplementedException();
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ManfSiteComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(this, "ManfSite cannot be blank.");
                return;
            }

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
            //if(DeliveryDateDatePicker.SelectedDate != null && ManufactureEndDateDatePicker.SelectedDate != null)
            //{
            //    if (DeliveryDateDatePicker.SelectedDate.Value.Date < ManufactureEndDateDatePicker.SelectedDate.Value.Date)
            //    {
            //        MessageBox.Show(this, "Delivery date cannot be earlier than manufacure end date.");
            //        return;

            //    }
            //}
            


            PutTrackerArchiveLine();
            this.DialogResult = true;
            this.Close();


        }

        private void DeliveryDateTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime selectedDate = ((DatePicker)sender).SelectedDate.Value.Date;

            DeliveryMonthTextBox.Text = selectedDate.ToString("MMMM yy");
            DeliveryWeekNumberTextBox.Text = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(selectedDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString();

        }

        private void PutTrackerArchiveLine()
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.PUT_V2_TRACKER_ARCHIVE_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                    command.Parameters.AddWithValue("@ManfSite", ManfSiteComboBox.Text);                                                    //trackerLine.ManfSite = ManfSiteComboBox.Text;
                    command.Parameters.AddWithValue("@ManualMaterialComment", MaterialCommentTextBox.Text);                                 //trackerLine.MaterialComment = MaterialCommentTextBox.Text;
                    command.Parameters.AddWithValue("@ProductionCommentDoor", ProductionCommentDoorTextBox.Text);                           //trackerLine.ProductionCommentDoor = ProductionCommentDoorTextBox.Text;
                    command.Parameters.AddWithValue("@ProductionCommentFrame", ProductionCommentFrameTextBox.Text);                         //trackerLine.ProductionCommentFrame = ProductionCommentFrameTextBox.Text;
                    command.Parameters.AddWithValue("@Username", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                    command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);

                    if (ManufactureCompleteCheckBox.IsChecked == true)
                    {
                        command.Parameters.AddWithValue("@ManufactureCompleted", 1);                                                        //trackerLine.ManufactureCompleted = true;
                        if(trackerLine.ManufactureCompleted == false)
                        {
                            command.Parameters.AddWithValue("@ManufactureEndDate", DateTime.Now);                                           //trackerLine.ManufactureEndDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@ManufactureCompleted", 0);                                                        //trackerLine.ManufactureCompleted = false;
                    }

                    if (IsHighEndFinishCheckBox.IsChecked == true)
                    {
                        command.Parameters.AddWithValue("@IsHighEndFinish", 1); 
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@IsHighEndFinish", 0);                                                        //trackerLine.ManufactureCompleted = false;
                    }



                    if (FilePrintedDateDatePicker.SelectedDate != null)
                    {
                        command.Parameters.AddWithValue("@FilePrintedDate", FilePrintedDateDatePicker.SelectedDate.Value.Date);             //trackerLine.FilePrintedDate = FilePrintedDateDatePicker.SelectedDate.Value.Date;
                    }


                    command.Parameters.AddWithValue("@FilePrintedBy", FilePrintedByComboBox.Text);                                          //trackerLine.FilePrintedBy = FilePrintedByComboBox.Text;
                    command.Parameters.AddWithValue("@ManufactureRep", ManufactureRepComboBox.Text);                                        //trackerLine.ManufactureRep = ManufactureRepComboBox.Text;

                    if (DoorQtyTextBox.Text != trackerLine.PegasusDoorQty.ToString())
                    {
                        command.Parameters.AddWithValue("@DoorQty", DoorQtyTextBox.Text);                                                   //trackerLine.DoorQty = int.Parse(DoorQtyTextBox.Text);
                    }
                    if (FrameQtyTextBox.Text != trackerLine.PegasusFrameQty.ToString())
                    {
                        command.Parameters.AddWithValue("@FrameQty", FrameQtyTextBox.Text);                                                 //trackerLine.FrameQty = int.Parse(FrameQtyTextBox.Text);
                    }
                    if (PanelQtyTextBox.Text != trackerLine.PegasusPanelQty.ToString())
                    {
                        command.Parameters.AddWithValue("@PanelQty", PanelQtyTextBox.Text);                                                 //trackerLine.PanelQty = int.Parse(PanelQtyTextBox.Text);
                    }
                    if (ScreenQtyTextBox.Text != trackerLine.PegasusScreenQty.ToString())
                    {
                        command.Parameters.AddWithValue("@ScreenQty", ScreenQtyTextBox.Text);                                               //trackerLine.ScreenQty = int.Parse(ScreenQtyTextBox.Text);
                    }
                    if (MiscQtyTextBox.Text != trackerLine.PegasusMiscQty.ToString())
                    {
                        command.Parameters.AddWithValue("@MiscQty", MiscQtyTextBox.Text == "" ? 0 : Int32.Parse(MiscQtyTextBox.Text));                                                   //trackerLine.MiscQty = int.Parse(MiscQtyTextBox.Text);
                    }
                    if (IronmongeryQtyTextBox.Text != trackerLine.PegasusIronmongeryQty.ToString())
                    {
                        command.Parameters.AddWithValue("@IronmongeryQty", IronmongeryQtyTextBox.Text);                                     //trackerLine.IronmongeryQty = int.Parse(IronmongeryQtyTextBox.Text);
                    }

                    if (DeliveryDateDatePicker.SelectedDate != null)
                    {
                        if (DeliveryDateDatePicker.SelectedDate.Value.Date != trackerLine.FtbDeliveryDate || trackerLine.DeliveryDateOverride == 1)
                        {
                            command.Parameters.AddWithValue("@DeliveryDate", DeliveryDateDatePicker.SelectedDate.Value.Date);               //trackerLine.DeliveryDate = DeliveryDateDatePicker.SelectedDate.Value.Date;
                            command.Parameters.AddWithValue("@DeliveryDateOverride", 1);                                                    //trackerLine.DeliveryDateOverride = 1;
                        }
                    }






                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                                    
                    SqlConnection con = new SqlConnection(Properties.Resources.db);
                    SqlCommand com = new SqlCommand("PegasusCopy.dbo.[Update_Insert_In_ProductionTrackerLine]", con);
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("@JobNo", trackerLine.JobNo);
                    com.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                    if (con.State == ConnectionState.Open) { con.Close(); }
                    con.Open();
                    com.ExecuteNonQuery();
                    con.Close();


                }



                
            }
        }

        private DateTime GetLastMaterialDate(string jobNo)
        {
            DateTime dt = DateTime.MinValue;

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("PegasusCopy.dbo.GET_LAST_MATERIAL_DATE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Phase", jobNo);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            dt = reader.GetDateTime(0);
                        }
                    }
                }

            }
            return dt;
        }

        private void ResetPrintDate_Button_Click(object sender, RoutedEventArgs e)
        {
            FilePrintedByComboBox.SelectedIndex = -1;

            FilePrintedDateDatePicker.SelectedDate = null;
        }

        private void MaterialCommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void UpdateAtRisk_Button_Click(object sender, RoutedEventArgs e)
        {
        
            for (int i = 0; i < AtRiskDataGrid.Items.Count; i++)
            {
                try { 
                DataGridRow gv = (DataGridRow)AtRiskDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                WipCommentary wipCommentaryAtRisk = (WipCommentary)gv.Item;

                if(wipCommentaryAtRisk.DocNr +"" != "") { 
                string str;
                str = "update IdslV2TrackerWipCommentary set Risk=@Risk,LastUpdatedBy=@LastUpdatedBy,LastUpdatedDate=getdate(),UpdateRemark=@UpdateRemark where DocNr=@DocNr ";
                str += " and WIPStation=@WIPStation and convert(datetime,Timestamp)=convert(datetime,@Timestamp) and Username=@Username and ProductionComment=@ProductionComment";
                SqlConnection connection = new SqlConnection(Properties.Resources.db);
                SqlCommand com = new SqlCommand(str, connection);
                com.Parameters.AddWithValue("@Risk", wipCommentaryAtRisk.Risk);
                com.Parameters.AddWithValue("@LastUpdatedBy", System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper());
                com.Parameters.AddWithValue("@UpdateRemark", wipCommentaryAtRisk.UpdateRemark);
                com.Parameters.AddWithValue("@DocNr", wipCommentaryAtRisk.DocNr);
                com.Parameters.AddWithValue("@WIPStation", wipCommentaryAtRisk.WIPStation);
                com.Parameters.AddWithValue("@Timestamp", wipCommentaryAtRisk.Timestamp);
                com.Parameters.AddWithValue("@Username", wipCommentaryAtRisk.Username);
                com.Parameters.AddWithValue("@ProductionComment", wipCommentaryAtRisk.ProductionComment);
                try {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Open();
                com.ExecuteNonQuery();
                connection.Close();


                            if (wipCommentaryAtRisk.Risk == false)
                            {
                                str = "Update PegasusCopy.dbo.ProductionTracker set Risk=0,RiskComment='"+ Convert.ToString( wipCommentaryAtRisk.UpdateRemark)+"" + "' where DocNr=@DocNr and JobNo=@JobNo ";
                                SqlCommand com1 = new SqlCommand(str, connection);
                                com1.Parameters.AddWithValue("@DocNr", wipCommentaryAtRisk.DocNr);
                                com1.Parameters.AddWithValue("@JobNo", trackerLine.JobNo);
                                if (connection.State == ConnectionState.Open) { connection.Close(); }
                                connection.Open();
                                com1.ExecuteNonQuery();
                                connection.Close();
                            }



                }
                catch { 
                }
                }
                }
                catch { }

            }

            PopulateWipHistoryTable();
            PopulateAtRiskTable();



        }



    }

    internal class WipCommentary
    {
        public string ProductionComment { get; set; }
        public string WIPStation { get; set; }
        public bool Risk { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string LastUpdatedBy { get; set; }
        public string UpdateRemark { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string DocNr { get; set; }
    }
}
