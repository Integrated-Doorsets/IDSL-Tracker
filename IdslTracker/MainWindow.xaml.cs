using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IdslTracker
{
    public partial class MainWindow : Window
    {
        const double TIMER_TIMEOUT = 60000 * 30;
        private Timer REFRESH_TIMER;
        public MainWindow()
        {
            InitializeComponent();
            loadingGif.Visibility = Visibility.Hidden;

            REFRESH_TIMER = new Timer(TIMER_TIMEOUT);
            REFRESH_TIMER.Elapsed += DisplayRefreshPrompt;
            REFRESH_TIMER.AutoReset = false;
            REFRESH_TIMER.Enabled = true;
            REFRESH_TIMER.Start();


            Globals.ManfSites = GetManfSites();
            Globals.PrintedByNames = GetPrintedByNames();
            Globals.ManufactureReps = GetManufactureReps();


            manfRepComboBox.Items.Add("ALL");
            foreach (string manfRep in Globals.ManufactureReps)
            {
                manfRepComboBox.Items.Add(manfRep);
            }
            manfRepComboBox.SelectionChanged -= new SelectionChangedEventHandler(ManfRepComboBox_SelectionChanged);
            manfRepComboBox.SelectedIndex = 0;
            manfRepComboBox.SelectionChanged += new SelectionChangedEventHandler(ManfRepComboBox_SelectionChanged);






            manfCompleteComboBox.Items.Add("ALL");
            manfCompleteComboBox.Items.Add("Yes");
            manfCompleteComboBox.Items.Add("No");
            manfCompleteComboBox.Items.Add("Accrued");
            manfCompleteComboBox.SelectionChanged -= new SelectionChangedEventHandler(ManfCompleteComboBox_SelectionChanged);
            manfCompleteComboBox.SelectedIndex = 0;
            manfCompleteComboBox.SelectionChanged += new SelectionChangedEventHandler(ManfCompleteComboBox_SelectionChanged);






            RefreshWarning.Visibility = Visibility.Hidden;
        }

        private void DisplayRefreshPrompt(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                RefreshWarning.Visibility = Visibility.Visible;
            });
        }

        private List<string> GetManufactureReps()
        {
            List<string> manufactureReps = new List<string>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUFACTURE_REPS", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            manufactureReps.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return manufactureReps;
        }

        private List<string> GetPrintedByNames()
        {
            List<string> printedByNames = new List<string>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_PRINTED_BY_NAMES", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            printedByNames.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return printedByNames;
        }

        private List<string> GetManfSites()
        {
            List<string> manfSiteNames = new List<string>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANF_SITES", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            manfSiteNames.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return manfSiteNames;
        }

        private void LoadingGif_MediaEnded(object sender, RoutedEventArgs e)
        {
            loadingGif.Position = new TimeSpan(0, 0, 1);
            loadingGif.Play();
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            //RefreshPrompt.Visibility = Visibility.Hidden;

            DataGrid dg = GetVisibleDataGrid();
            RefreshDataGrid(dg);

        }

        private void RefreshDataGrid(DataGrid dg)
        {
            string dgName = dg.Name;
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MainTabControl.Visibility = Visibility.Hidden;
                    HeaderControlsGrid.Visibility = Visibility.Hidden;
                    loadingGif.Visibility = Visibility.Visible;
                });


                //PopulateMainTrackerDataGrid();
                //PopulateMiscTrackerDataGrid();
                //PopulateThirdPartyTrackerDataGrid();
                //PopulateIronmongeryTrackerDataGrid();
                //PopulateVestingTrackerDataGrid();
                if (dgName == "MaterialReviewDataGrid")
                {
                    PopulateMaterialReviewDataGrid();
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(this, "Only the material review tab has been refreshed.");

                    });
                }
                else if (dgName == "CreditStopReviewDataGrid")
                {
                    PopulateCreditStopReviewDataGrid();
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(this, "Only the Credit Stop review tab has been refreshed.");

                    });

                }
                else
                {
                    PopulateTrackerDataGrids();
                }
            };



            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                    HeaderControlsGrid.Visibility = Visibility.Visible;
                    ApplyFilter();
                    if (Globals.reportWindow != null)
                    {
                        //Globals.reportWindow.RefreshActiveReport();
                    }
                    //if (Globals.manufacturingReportWindow != null)
                    //{
                    //    Globals.manufacturingReportWindow.RefreshMainReportDataGrid();
                    //}


                    REFRESH_TIMER.Interval = TIMER_TIMEOUT;
                    RefreshWarning.Visibility = Visibility.Hidden;
                    REFRESH_TIMER.Start();
                });
            };
            bgw.RunWorkerAsync();
        }

        private void PopulateCreditStopReviewDataGrid()
        {
            List<CreditStopReviewLine> creditStopReviewLines = new List<CreditStopReviewLine>();
            List<DateTime?> deliveryMonthList = new List<DateTime?>();
            List<string> customerStatusList = new List<string>();
            customerStatusList.Add("ALL");

            if (universalDeliveryMonthComboBox.Items.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {

                    universalDeliveryMonthComboBox.Items.Add("ALL");
                    universalDeliveryMonthComboBox.SelectionChanged -= new SelectionChangedEventHandler(DeliveryMonthComboBox_SelectionChanged);
                    universalDeliveryMonthComboBox.SelectedIndex = 0;
                    universalDeliveryMonthComboBox.SelectionChanged += new SelectionChangedEventHandler(DeliveryMonthComboBox_SelectionChanged);

                });
            }

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_IDSL_V2_TRACKER_LINES", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 600;
                    command.Parameters.AddWithValue("@ReportId", 1);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            CreditStopReviewLine creditStopReviewLine = new CreditStopReviewLine();


                            creditStopReviewLine.Customer = reader.GetString(0);
                            creditStopReviewLine.DeliveryMonth = reader.GetDateTime(1);
                            creditStopReviewLine.Sales = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                            creditStopReviewLine.CustomerStatus = reader.GetString(3);
                            creditStopReviewLine.Contract = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            creditStopReviewLine.Comment = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            creditStopReviewLine.EarliestDeliveryDate = reader.GetDateTime(6);
                            creditStopReviewLine.CreditLimit = reader.IsDBNull(7) ? 0 : Convert.ToDecimal(reader.GetValue(7));
                            creditStopReviewLine.CurrentDebtBalance = reader.IsDBNull(8) ? 0 : Convert.ToDecimal(reader.GetValue(8));
                            creditStopReviewLine.CumulativeGrossSales = reader.IsDBNull(9) ? 0 : Convert.ToDecimal(reader.GetValue(9));
                            creditStopReviewLine.PotentialDebtBalance = reader.IsDBNull(10) ? 0 : Convert.ToDecimal(reader.GetValue(10));
                            creditStopReviewLine.AccountNumber = reader.GetString(11);

                            if (creditStopReviewLine.PotentialDebtBalance > creditStopReviewLine.CreditLimit)
                            {
                                creditStopReviewLine.PotentialOverLimit = true;
                            }
                            else
                            {
                                creditStopReviewLine.PotentialOverLimit = false;
                            }



                            if (!deliveryMonthList.Contains(creditStopReviewLine.DeliveryMonth))
                            {
                                deliveryMonthList.Add(creditStopReviewLine.DeliveryMonth);
                            }

                            if (!customerStatusList.Contains(creditStopReviewLine.CustomerStatus))
                            {
                                customerStatusList.Add(creditStopReviewLine.CustomerStatus);
                            }



                            creditStopReviewLines.Add(creditStopReviewLine);

                        }
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    CreditStopReviewDataGrid.ItemsSource = creditStopReviewLines;
                    customerStatusComboBox.ItemsSource = customerStatusList;
                    customerStatusComboBox.SelectedIndex = 0;
                    //universalDeliveryMonthComboBox.ItemsSource = deliveryMonthList;

                    foreach (DateTime dt in deliveryMonthList)
                    {
                        if (!universalDeliveryMonthComboBox.Items.Contains(dt))
                        {
                            universalDeliveryMonthComboBox.Items.Add(dt);
                        }
                    }

                });

            }
        }

        private void PopulateMaterialReviewDataGrid()
        {
            List<string> materialRiskList = new List<string>();
            materialRiskList.Add("ALL");

            if (universalDeliveryMonthComboBox.Items.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {

                    universalDeliveryMonthComboBox.Items.Add("ALL");

                    universalDeliveryMonthComboBox.SelectionChanged -= new SelectionChangedEventHandler(DeliveryMonthComboBox_SelectionChanged);
                    universalDeliveryMonthComboBox.SelectedIndex = 0;
                    universalDeliveryMonthComboBox.SelectionChanged += new SelectionChangedEventHandler(DeliveryMonthComboBox_SelectionChanged);
                });
            }

            if (universalDeliveryWeekComboBox.Items.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {

                    universalDeliveryWeekComboBox.Items.Add("ALL");
                    universalDeliveryWeekComboBox.SelectedIndex = 0;
                });
            }

            List<MaterialReviewLine> materialReviewLines = new List<MaterialReviewLine>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MATERIAL_REVIEW_LINES", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 600;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            MaterialReviewLine materialReviewLine = new MaterialReviewLine();


                            materialReviewLine.MaterialStatus = reader.GetString(0);
                            materialReviewLine.Supplier = reader.GetString(1);
                            materialReviewLine.PorNr = reader.GetString(2);
                            materialReviewLine.DateCreated = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                            materialReviewLine.DateRequired = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);
                            materialReviewLine.DateQuoted = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                            materialReviewLine.Description = reader.GetString(6);
                            materialReviewLine.ExtendedDescription = reader.GetString(7);
                            materialReviewLine.QuantityOrdered = Convert.ToInt32(reader.GetValue(8)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(8));
                            materialReviewLine.QuantityReceived = Convert.ToInt32(reader.GetValue(9)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(9));
                            materialReviewLine.ManufactureStartDate = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10);
                            materialReviewLine.DeliveryDate = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11);
                            materialReviewLine.Contract = reader.GetString(12);
                            materialReviewLine.JobNr = reader.GetString(13);
                            materialReviewLine.ProcurementComment = reader.IsDBNull(14) ? string.Empty : reader.GetString(14);
                            materialReviewLine.PjlSupplier = reader.IsDBNull(15) ? string.Empty : reader.GetString(15);
                            materialReviewLine.WeekNum = GetIso8601WeekNumber(materialReviewLine.DeliveryDate ?? DateTime.MinValue);




                            if (!materialRiskList.Contains(materialReviewLine.MaterialStatus))
                            {
                                materialRiskList.Add(materialReviewLine.MaterialStatus);
                            }




                            materialReviewLines.Add(materialReviewLine);

                        }
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    MaterialReviewDataGrid.ItemsSource = materialReviewLines;


                    MaterialRiskComboBox.ItemsSource = materialRiskList;
                    MaterialRiskComboBox.SelectedIndex = 0;

                });

            }
        }

        private void PopulateTrackerDataGrids()
        {

            if (universalDeliveryMonthComboBox.Items.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {

                    universalDeliveryMonthComboBox.Items.Add("ALL");
                    universalDeliveryMonthComboBox.SelectionChanged -= new SelectionChangedEventHandler(DeliveryMonthComboBox_SelectionChanged);
                    universalDeliveryMonthComboBox.SelectedIndex = 0;
                    universalDeliveryMonthComboBox.SelectionChanged += new SelectionChangedEventHandler(DeliveryMonthComboBox_SelectionChanged);
                });
            }

            if (SearchColourComboBox.Items.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    ColourComboBoxItem colourComboBoxItem = new ColourComboBoxItem();
                    colourComboBoxItem.cbItemText = "ALL";
                    SearchColourComboBox.Items.Add(colourComboBoxItem);
                    colourComboBoxItem = new ColourComboBoxItem();
                    colourComboBoxItem.cbItemText = "No Fill";
                    SearchColourComboBox.Items.Add(colourComboBoxItem);
                    SearchColourComboBox.SelectionChanged -= new SelectionChangedEventHandler(SearchColourComboBox_SelectionChanged);
                    SearchColourComboBox.SelectedIndex = 0;
                    SearchColourComboBox.SelectionChanged += new SelectionChangedEventHandler(SearchColourComboBox_SelectionChanged);
                });
            }


            if (universalDeliveryWeekComboBox.Items.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {

                    universalDeliveryWeekComboBox.Items.Add("ALL");
                    universalDeliveryWeekComboBox.SelectedIndex = 0;
                });
            }


            List<IdslTrackerLine> mainTrackerLines = new List<IdslTrackerLine>();
            List<IdslTrackerLine> thirdPartyTrackerLines = new List<IdslTrackerLine>();
            List<IdslTrackerLine> miscTrackerLines = new List<IdslTrackerLine>();
            List<IdslTrackerLine> ironmongeryTrackerLines = new List<IdslTrackerLine>();
            List<IdslTrackerLine> vestingTrackerLines = new List<IdslTrackerLine>();
            List<IdslTrackerLine> TrackerAtRiskLines = new List<IdslTrackerLine>();



            List<DateTime> deliveryMonthList = new List<DateTime>();
            List<int> deliveryWeekList = new List<int>();

            List<string> commentColourHexs = new List<string>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                // using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_IDSL_V2_TRACKER_LINES", connection))
                //using (SqlCommand command = new SqlCommand("select * from PegasusCopy.dbo.v_ProductionTracker where  isnull(DoorQty,0)+isnull(FrameQty,0)+isnull(PanelQty,0)+isnull(MiscQty,0)+isnull(IronmongeryQty,0)>0  and convert(date,InvoiceDate)=convert(date,'1899-12-30') ", connection))
                using (SqlCommand command = new SqlCommand("select * from PegasusCopy.dbo.v_ProductionTracker", connection))

                {
                    command.CommandTimeout = 180;
                  

                    command.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                     if(dt.Rows.Count > 0)
                    { int i;
                        for(i=0;i<=dt.Rows.Count - 1; i++)
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
                            {trackerLine.FilePrintedDate = Convert.ToDateTime(dt.Rows[i]["FilePrintedDate"]);}
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

                            trackerLine.CountWeeksHeld= Convert.ToInt32(dt.Rows[i]["CountWeeksHeld"]);

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
                            trackerLine.RiskComment= Convert.ToString(dt.Rows[i]["RiskComment"]);
                            //trackerLine.RiskRowColorHex = Convert.ToString(dt.Rows[i]["RiskRowColorHex"]);

                            if (Convert.ToBoolean(dt.Rows[i]["Risk"]) == true)
                            {
                                TrackerAtRiskLines.Add(trackerLine);
                            }





                            switch (trackerLine.ReportId)
                                {
                                    case 2:
                                        thirdPartyTrackerLines.Add(trackerLine);
                                        break;
                                    case 3:
                                        miscTrackerLines.Add(trackerLine);
                                        break;
                                    case 4:
                                        ironmongeryTrackerLines.Add(trackerLine);
                                        break;
                                    case 5:
                                        vestingTrackerLines.Add(trackerLine);
                                        break;
                                default:
                                   
                                        mainTrackerLines.Add(trackerLine);
                                                                     
                                        break;
                            }

                           

                            if (trackerLine.DeliveryDate != null)
                                {
                                    if (!deliveryMonthList.Contains(ConvertToLastDayOfMonth(trackerLine.DeliveryDate ?? DateTime.MinValue)))
                                    {
                                        deliveryMonthList.Add(ConvertToLastDayOfMonth(trackerLine.DeliveryDate ?? DateTime.MinValue));
                                    }
                                    if (!deliveryWeekList.Contains(trackerLine.WeekNum))
                                    {
                                        deliveryWeekList.Add(trackerLine.WeekNum);
                                    }
                                }

                                if (trackerLine.ProductionCommentDoorColourHex != null)
                                {
                                    if (!commentColourHexs.Contains(trackerLine.ProductionCommentDoorColourHex))
                                    {
                                        commentColourHexs.Add(trackerLine.ProductionCommentDoorColourHex.ToString());
                                    }
                                }

                                if (trackerLine.ProductionCommentFrameColourHex != null)
                                {
                                    if (!commentColourHexs.Contains(trackerLine.ProductionCommentFrameColourHex))
                                    {
                                        commentColourHexs.Add(trackerLine.ProductionCommentFrameColourHex.ToString());
                                    }
                                }

                            
                        }
                    }
                    else { return; }

                    //using (SqlDataReader reader = command.ExecuteReader())
                    //{

                    //    while (reader.Read())
                    //    {
                    //        IdslTrackerLine trackerLine = new IdslTrackerLine();


                    //        trackerLine.DocNumber = reader.GetString(0);
                    //        trackerLine.ManfSite = reader.GetString(1);
                    //        trackerLine.Contract = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    //        trackerLine.JobNo = reader.GetString(3);
                    //        trackerLine.BatchRef = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                    //        trackerLine.ProductType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                    //        trackerLine.DoorQty = Convert.ToInt32(reader.GetValue(6)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(6));
                    //        trackerLine.FrameQty = Convert.ToInt32(reader.GetValue(7)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(7));
                    //        trackerLine.PanelQty = Convert.ToInt32(reader.GetValue(8)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(8));
                    //        trackerLine.ScreenQty = Convert.ToInt32(reader.GetValue(9)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(9));
                    //        trackerLine.MiscQty = Convert.ToInt32(reader.GetValue(10)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(10));
                    //        trackerLine.IronmongeryQty = Convert.ToInt32(reader.GetValue(11)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(11));
                    //        trackerLine.Sales = reader.IsDBNull(12) ? 0 : Convert.ToDecimal(reader.GetValue(12));
                    //        trackerLine.Customer = reader.GetString(13);
                    //        trackerLine.DeliveryDate = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14);
                    //        trackerLine.FtbDeliveryDate = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15);
                    //        trackerLine.LastStageDoor = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
                    //        trackerLine.ProductionCommentDoor = reader.IsDBNull(17) ? string.Empty : reader.GetString(17);
                    //        trackerLine.ReportId = Convert.ToInt32(reader.GetValue(18));
                    //        trackerLine.FilePrintedBy = reader.IsDBNull(19) ? string.Empty : reader.GetString(19);
                    //        trackerLine.FilePrintedDate = reader.IsDBNull(20) ? (DateTime?)null : reader.GetDateTime(20);
                    //        trackerLine.PegasusDoorQty = Convert.ToInt32(reader.GetValue(21)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(21));
                    //        trackerLine.PegasusFrameQty = Convert.ToInt32(reader.GetValue(22)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(22));
                    //        trackerLine.PegasusPanelQty = Convert.ToInt32(reader.GetValue(23)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(23));
                    //        trackerLine.PegasusScreenQty = Convert.ToInt32(reader.GetValue(24)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(24));
                    //        trackerLine.PegasusMiscQty = Convert.ToInt32(reader.GetValue(25)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(25));
                    //        trackerLine.PegasusIronmongeryQty = Convert.ToInt32(reader.GetValue(26)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(26));
                    //        trackerLine.DeliveryDateOverride = Convert.ToInt32(reader.GetValue(27));
                    //        trackerLine.DeliveryRiskMaterials = reader.IsDBNull(28) ? string.Empty : reader.GetString(28);
                    //        trackerLine.CustomerStatus = reader.IsDBNull(29) ? string.Empty : reader.GetString(29);
                    //        trackerLine.MaterialComment = reader.IsDBNull(30) ? string.Empty : reader.GetString(30);
                    //        trackerLine.SchedulingContact = reader.IsDBNull(31) ? string.Empty : reader.GetString(31);
                    //        trackerLine.SalesContact = reader.IsDBNull(32) ? string.Empty : reader.GetString(32);
                    //        trackerLine.ProcurementContact = reader.IsDBNull(33) ? string.Empty : reader.GetString(33);
                    //        trackerLine.PjlFileHasBeenPrinted = reader.GetBoolean(34);
                    //        trackerLine.ManufactureCompleted = reader.GetBoolean(35);
                    //        trackerLine.ManufactureEndDate = reader.IsDBNull(36) ? (DateTime?)null : reader.GetDateTime(36);
                    //        trackerLine.ManufactureRep = reader.IsDBNull(37) ? string.Empty : reader.GetString(37);
                    //        trackerLine.StorageRef = reader.IsDBNull(38) ? string.Empty : reader.GetString(38);
                    //        trackerLine.ProductionCommentFrame = reader.IsDBNull(39) ? string.Empty : reader.GetString(39);
                    //        trackerLine.LastStageFrame = reader.IsDBNull(40) ? string.Empty : reader.GetString(40);
                    //        trackerLine.HasBeenProcured = reader.GetBoolean(41);
                    //        trackerLine.ManufactureStartDate = reader.IsDBNull(42) ? (DateTime?)null : reader.GetDateTime(42);
                    //        trackerLine.InvoicedDate = reader.IsDBNull(43) ? (DateTime?)null : reader.GetDateTime(43);
                    //        trackerLine.CountWeeksHeld = reader.IsDBNull(44) ? 0 : Convert.ToDecimal(reader.GetValue(44));
                    //        trackerLine.SopCreatedDate = reader.IsDBNull(45) ? (DateTime?)null : reader.GetDateTime(45);
                    //        trackerLine.ManualMaterialComment = reader.IsDBNull(46) ? string.Empty : reader.GetString(46);
                    //        trackerLine.WeekNum = GetIso8601WeekNumber(trackerLine.DeliveryDate ?? DateTime.MinValue);
                    //        trackerLine.ProductionCommentDoorColourHex = reader.IsDBNull(47) ? string.Empty : reader.GetString(47);
                    //        trackerLine.ProductionCommentFrameColourHex = reader.IsDBNull(48) ? string.Empty : reader.GetString(48);
                    //        trackerLine.ManfRepAbv = reader.IsDBNull(50) ? string.Empty : reader.GetString(50);
                    //        trackerLine.ShopfloorComment = reader.IsDBNull(51) ? string.Empty : reader.GetString(51);
                    //        trackerLine.IsAccrued = reader.GetBoolean(52);
                    //        trackerLine.IsHighEndFinish = reader.GetBoolean(56);

                    //        switch (trackerLine.ReportId)
                    //        {
                    //            case 2:
                    //                thirdPartyTrackerLines.Add(trackerLine);
                    //                break;
                    //            case 3:
                    //                miscTrackerLines.Add(trackerLine);
                    //                break;
                    //            case 4:
                    //                ironmongeryTrackerLines.Add(trackerLine);
                    //                break;
                    //            case 5:
                    //                vestingTrackerLines.Add(trackerLine);
                    //                break;
                    //            default:
                    //                mainTrackerLines.Add(trackerLine);
                    //                break;

                    //        }

                    //        if (trackerLine.DeliveryDate != null)
                    //        {
                    //            if (!deliveryMonthList.Contains(ConvertToLastDayOfMonth(trackerLine.DeliveryDate ?? DateTime.MinValue)))
                    //            {
                    //                deliveryMonthList.Add(ConvertToLastDayOfMonth(trackerLine.DeliveryDate ?? DateTime.MinValue));
                    //            }
                    //            if (!deliveryWeekList.Contains(trackerLine.WeekNum))
                    //            {
                    //                deliveryWeekList.Add(trackerLine.WeekNum);
                    //            }
                    //        }

                    //        if (trackerLine.ProductionCommentDoorColourHex != null)
                    //        {
                    //            if (!commentColourHexs.Contains(trackerLine.ProductionCommentDoorColourHex))
                    //            {
                    //                commentColourHexs.Add(trackerLine.ProductionCommentDoorColourHex.ToString());
                    //            }
                    //        }

                    //        if (trackerLine.ProductionCommentFrameColourHex != null)
                    //        {
                    //            if (!commentColourHexs.Contains(trackerLine.ProductionCommentFrameColourHex))
                    //            {
                    //                commentColourHexs.Add(trackerLine.ProductionCommentFrameColourHex.ToString());
                    //            }
                    //        }

                    //    }
                    //}
                }

                mainTrackerLines = AddWipDetails(mainTrackerLines);
                mainTrackerLines = Filter_Zero_Qty_From_MainTracker(mainTrackerLines);

                this.Dispatcher.Invoke(() =>
                {
                    MainTrackerDataGrid.ItemsSource = mainTrackerLines.OrderBy(x => x.DeliveryDate).ThenBy(y => y.JobNo).ToList();
                    ThirdPartyTrackerDataGrid.ItemsSource = thirdPartyTrackerLines;
                    MiscTrackerDataGrid.ItemsSource = miscTrackerLines;
                    IronTrackerDataGrid.ItemsSource = ironmongeryTrackerLines;
                    VestingTrackerDataGrid.ItemsSource = vestingTrackerLines;
                    TrackerAtRiskDataGrid.ItemsSource = TrackerAtRiskLines;

                    deliveryMonthList.Sort((x, y) => DateTime.Compare(x, y));

                    foreach (DateTime dt in deliveryMonthList)
                    {
                        if (dt != null)
                        {
                            if (!universalDeliveryMonthComboBox.Items.Contains(dt))
                            {
                                universalDeliveryMonthComboBox.Items.Add(dt);
                            }
                        }

                    }

                    deliveryWeekList.Sort();
                    foreach (int wk in deliveryWeekList)
                    {
                        if (wk != 0)
                        {
                            if (!universalDeliveryWeekComboBox.Items.Contains(wk))
                            {
                                universalDeliveryWeekComboBox.Items.Add(wk);
                            }
                        }

                    }

                    foreach (string hex in commentColourHexs)
                    {
                        if (hex != "")
                        {
                            ColourComboBoxItem colourComboBoxItem = new ColourComboBoxItem();
                            var converter = new System.Windows.Media.BrushConverter();
                            colourComboBoxItem.cbItemColour = (Brush)converter.ConvertFromString(hex);
                            SearchColourComboBox.Items.Add(colourComboBoxItem);
                        }

                    }

                    //universalDeliveryWeekComboBox.Items.SortDescriptions = SortDescription;

                });
            }
        }

        private List<IdslTrackerLine> AddWipDetails(List<IdslTrackerLine> mainTrackerLines)
        {
            //List<TrackerWipDetail> trackerWipDetails = new List<TrackerWipDetail>();

            //using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            //{
            //    using (SqlCommand command = new SqlCommand("SELECT * FROM Tracker.dbo.vIDSL_V2_TRACKER_WIP_COMMENTARY", connection))
            //    {
            //        connection.Open();

            //        using (SqlDataReader reader = command.ExecuteReader())
            //        {

            //            while (reader.Read())
            //            {
            //                TrackerWipDetail trackerWipDetail = new TrackerWipDetail();
            //                trackerWipDetail.Doc = reader.GetString(0);
            //                trackerWipDetail.DoorWip = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            //                trackerWipDetail.FrameWip = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            //                trackerWipDetails.Add(trackerWipDetail);

            //            }
            //        }
            //    }
            //}

            //foreach (IdslTrackerLine line in mainTrackerLines)
            //{
            //    foreach (TrackerWipDetail wipDetail in trackerWipDetails)
            //    {
            //        if (wipDetail.Doc == line.DocNumber)
            //        {
            //            line.WipCommentary = wipDetail.DoorWip;
            //            line.WipFrameCommentary = wipDetail.FrameWip;
            //        }
            //        continue;
            //    }
            //}

            return mainTrackerLines;

        }


        private List<IdslTrackerLine> Filter_Zero_Qty_From_MainTracker(List<IdslTrackerLine> mainTrackerLines)
        {
            List<IdslTrackerLine> NonZeroMainTrackerLines = new List<IdslTrackerLine>();

            foreach (IdslTrackerLine line in mainTrackerLines)
            {
                int DoorQty = line.DoorQty??0;
                int FrameQty = line.FrameQty ?? 0;
                int MiscQty = line.MiscQty ?? 0;
                int ScreenQty = line.ScreenQty ?? 0;
                int PanelQty = line.PanelQty ?? 0;
                int IronmongeryQty = line.IronmongeryQty ?? 0;



                if (DoorQty +FrameQty +MiscQty +ScreenQty +PanelQty +IronmongeryQty >0)
                    {
                    NonZeroMainTrackerLines.Add(line);
                    }
                
               
            }

            return NonZeroMainTrackerLines;

        }


        //private DateTime WeekNumToDate(int weekNum, bool startOfWeek)
        //{
        //    throw new NotImplementedException();
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

        //private DateTime ConvertToFirstOfWeek(DateTime dateTime)
        //{
        //    DayOfWeek startOfWeek = DayOfWeek.Monday;
        //    int diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
        //    return dateTime.AddDays(-1 * diff).Date;
        //}

        public static DateTime ConvertToLastDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }
        public static DateTime ConvertToFirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
            //return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        private DataGrid GetVisibleDataGrid()
        {
            DataGrid dg = null;

            switch ((MainTabControl.SelectedItem as TabItem).Header.ToString())
            {
                case "Production Tracker":

                    dg = MainTrackerDataGrid;
                    break;
                case "3rd Party Tracker":
                    //CreatePoCsv(glassDataGrid);
                    dg = ThirdPartyTrackerDataGrid;
                    break;
                case "Misc Tracker":
                    //CreatePoCsv(miscDataGrid);
                    dg = MiscTrackerDataGrid;
                    break;
                case "Ironmongery Tracker":
                    //CreatePoCsv(miscDataGrid);
                    dg = IronTrackerDataGrid;
                    break;
                case "Invoiced & Stored":
                    //CreatePoCsv(miscDataGrid);
                    dg = VestingTrackerDataGrid;
                    break;
                case "Material Review":
                    //CreatePoCsv(miscDataGrid);
                    dg = MaterialReviewDataGrid;
                    break;
                case "Credit Stop Review":
                    //CreatePoCsv(miscDataGrid);
                    dg = CreditStopReviewDataGrid;
                    break;
                case "Tracker At Risk":
                    //CreatePoCsv(miscDataGrid);
                    dg = TrackerAtRiskDataGrid;
                    break;
                default:
                    //MessageBox.Show(this, "Unexcpected error... Aborting operation");
                    break;

            }

            return dg;
        }




        System.Windows.Threading.DispatcherTimer _typingTimer;
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (_typingTimer == null)
            {
                _typingTimer = new DispatcherTimer();
                _typingTimer.Interval = TimeSpan.FromMilliseconds(500);

                _typingTimer.Tick += new EventHandler(this.HandleTypingTimerTimeout);
            }
            _typingTimer.Stop(); // Resets the timer
            _typingTimer.Tag = (sender as TextBox).Text; // This should be done with EventArgs
            _typingTimer.Start();
        }

        private void HandleTypingTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            ApplyFilter();
            //MessageBox.Show(this,"Not Implimented Yet");
            timer.Stop();
        }

        private void TrackerItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            IdslTrackerLine selectedTrackerLine = dg.SelectedItem as IdslTrackerLine;

            DetailViewWindow detailViewWindow = new DetailViewWindow(selectedTrackerLine, dg.Name);
            detailViewWindow.Owner = this;
            detailViewWindow.ShowDialog();
            if (detailViewWindow.DialogResult == true)
            {
                //RefreshDataGrid(dg);

                var bgw = new BackgroundWorker();
                bgw.DoWork += (_, __) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MainTabControl.Visibility = Visibility.Hidden;
                        HeaderControlsGrid.Visibility = Visibility.Hidden;
                        loadingGif.Visibility = Visibility.Visible;
                    });



                    RefreshSelectedLine(ref selectedTrackerLine);
                };

                bgw.RunWorkerCompleted += (_, __) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        dg.Items.Refresh();
                        loadingGif.Visibility = Visibility.Hidden;
                        MainTabControl.Visibility = Visibility.Visible;
                        HeaderControlsGrid.Visibility = Visibility.Visible;
                    });
                };
                bgw.RunWorkerAsync();

            }

        }

        private void RefreshSelectedLine(ref IdslTrackerLine selectedTrackerLine)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_IDSL_V2_TRACKER_LINES", connection))
                {
                    command.CommandTimeout = 180;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ReportId", 3);
                    command.Parameters.AddWithValue("@Doc", selectedTrackerLine.DocNumber);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            selectedTrackerLine.DocNumber = reader.GetString(0);
                            selectedTrackerLine.ManfSite = reader.GetString(1);
                            selectedTrackerLine.Contract = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            selectedTrackerLine.JobNo = reader.GetString(3);
                            selectedTrackerLine.BatchRef = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            selectedTrackerLine.ProductType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            selectedTrackerLine.DoorQty = Convert.ToInt32(reader.GetValue(6)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(6));
                            selectedTrackerLine.FrameQty = Convert.ToInt32(reader.GetValue(7)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(7));
                            selectedTrackerLine.PanelQty = Convert.ToInt32(reader.GetValue(8)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(8));
                            selectedTrackerLine.ScreenQty = Convert.ToInt32(reader.GetValue(9)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(9));
                            selectedTrackerLine.MiscQty = Convert.ToInt32(reader.GetValue(10)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(10));
                            selectedTrackerLine.IronmongeryQty = Convert.ToInt32(reader.GetValue(11)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(11));
                            selectedTrackerLine.Sales = reader.IsDBNull(12) ? 0 : Convert.ToDecimal(reader.GetValue(12));
                            selectedTrackerLine.Customer = reader.GetString(13);
                            selectedTrackerLine.DeliveryDate = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14);
                            //trackerLine.FtbDeliveryDate = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15);
                            selectedTrackerLine.LastStageDoor = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
                            selectedTrackerLine.ProductionCommentDoor = reader.IsDBNull(17) ? string.Empty : reader.GetString(17);
                            selectedTrackerLine.ReportId = Convert.ToInt32(reader.GetValue(18));
                            selectedTrackerLine.FilePrintedBy = reader.IsDBNull(19) ? string.Empty : reader.GetString(19);
                            selectedTrackerLine.FilePrintedDate = reader.IsDBNull(20) ? (DateTime?)null : reader.GetDateTime(20);
                            selectedTrackerLine.PegasusDoorQty = Convert.ToInt32(reader.GetValue(21)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(21));
                            selectedTrackerLine.PegasusFrameQty = Convert.ToInt32(reader.GetValue(22)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(22));
                            selectedTrackerLine.PegasusPanelQty = Convert.ToInt32(reader.GetValue(23)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(23));
                            selectedTrackerLine.PegasusScreenQty = Convert.ToInt32(reader.GetValue(24)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(24));
                            selectedTrackerLine.PegasusMiscQty = Convert.ToInt32(reader.GetValue(25)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(25));
                            selectedTrackerLine.PegasusIronmongeryQty = Convert.ToInt32(reader.GetValue(26)) == 0 ? (Nullable<int>)null : Convert.ToInt32(reader.GetValue(26));
                            selectedTrackerLine.DeliveryDateOverride = Convert.ToInt32(reader.GetValue(27));
                            selectedTrackerLine.DeliveryRiskMaterials = reader.IsDBNull(28) ? string.Empty : reader.GetString(28);
                            selectedTrackerLine.CustomerStatus = reader.IsDBNull(29) ? string.Empty : reader.GetString(29);
                            selectedTrackerLine.MaterialComment = reader.IsDBNull(30) ? string.Empty : reader.GetString(30);
                            selectedTrackerLine.SchedulingContact = reader.IsDBNull(31) ? string.Empty : reader.GetString(31);
                            selectedTrackerLine.SalesContact = reader.IsDBNull(32) ? string.Empty : reader.GetString(32);
                            selectedTrackerLine.ProcurementContact = reader.IsDBNull(33) ? string.Empty : reader.GetString(33);
                            selectedTrackerLine.PjlFileHasBeenPrinted = reader.GetBoolean(34);
                            selectedTrackerLine.ManufactureCompleted = reader.GetBoolean(35);
                            selectedTrackerLine.ManufactureEndDate = reader.IsDBNull(36) ? (DateTime?)null : reader.GetDateTime(36);
                            selectedTrackerLine.ManufactureRep = reader.IsDBNull(37) ? string.Empty : reader.GetString(37);
                            selectedTrackerLine.StorageRef = reader.IsDBNull(38) ? string.Empty : reader.GetString(38);
                            selectedTrackerLine.ProductionCommentFrame = reader.IsDBNull(39) ? string.Empty : reader.GetString(39);
                            selectedTrackerLine.LastStageFrame = reader.IsDBNull(40) ? string.Empty : reader.GetString(40);
                            selectedTrackerLine.HasBeenProcured = reader.GetBoolean(41);
                            selectedTrackerLine.ManufactureStartDate = reader.IsDBNull(42) ? (DateTime?)null : reader.GetDateTime(42);
                            selectedTrackerLine.InvoicedDate = reader.IsDBNull(43) ? (DateTime?)null : reader.GetDateTime(43);
                            selectedTrackerLine.CountWeeksHeld = reader.IsDBNull(44) ? 0 : Convert.ToDecimal(reader.GetValue(44));
                            selectedTrackerLine.SopCreatedDate = reader.IsDBNull(45) ? (DateTime?)null : reader.GetDateTime(45);
                            selectedTrackerLine.ManualMaterialComment = reader.IsDBNull(46) ? string.Empty : reader.GetString(46);
                            selectedTrackerLine.WeekNum = GetIso8601WeekNumber(selectedTrackerLine.DeliveryDate ?? DateTime.MinValue);
                            selectedTrackerLine.ProductionCommentDoorColourHex = reader.IsDBNull(47) ? string.Empty : reader.GetString(47);
                            selectedTrackerLine.ProductionCommentFrameColourHex = reader.IsDBNull(48) ? string.Empty : reader.GetString(48);
                            selectedTrackerLine.ManfRepAbv = reader.IsDBNull(50) ? string.Empty : reader.GetString(50);
                            selectedTrackerLine.ShopfloorComment = reader.IsDBNull(51) ? string.Empty : reader.GetString(51);
                            selectedTrackerLine.IsAccrued = reader.GetBoolean(52);
                            selectedTrackerLine.IsHighEndFinish = reader.GetBoolean(56);




                        }
                    }
                }
            }
        }

        private void ApplyFilter()
        {
            Decimal filteredTotal = 0;
            DataGrid dg = GetVisibleDataGrid();

            if (dg.Name == "CreditStopReviewDataGrid")
            {

                universalDeliveryWeekComboBox.Visibility = Visibility.Hidden;
                universalDeliveryWeekLabel.Visibility = Visibility.Hidden;
                manfRepLabel.Visibility = Visibility.Hidden;
                manfRepComboBox.Visibility = Visibility.Hidden;
                manfCompleteLabel.Visibility = Visibility.Hidden;
                manfCompleteComboBox.Visibility = Visibility.Hidden;
                SearchColourComboBox.Visibility = Visibility.Hidden;
            }
            else if (dg.Name == "MaterialReviewDataGrid")
            {
                universalDeliveryWeekComboBox.Visibility = Visibility.Visible;
                universalDeliveryWeekLabel.Visibility = Visibility.Visible;
                manfRepLabel.Visibility = Visibility.Hidden;
                manfRepComboBox.Visibility = Visibility.Hidden;
                manfCompleteLabel.Visibility = Visibility.Hidden;
                manfCompleteComboBox.Visibility = Visibility.Hidden;
                SearchColourComboBox.Visibility = Visibility.Hidden;
            }
            else
            {
                universalDeliveryWeekComboBox.Visibility = Visibility.Visible;
                universalDeliveryWeekLabel.Visibility = Visibility.Visible;
                manfRepLabel.Visibility = Visibility.Visible;
                manfRepComboBox.Visibility = Visibility.Visible;
                manfCompleteLabel.Visibility = Visibility.Visible;
                manfCompleteComboBox.Visibility = Visibility.Visible;
                SearchColourComboBox.Visibility = Visibility.Visible;
            }


            string filterText = SearchBox.Text.ToLower();

            dg.Items.Refresh();

            if (dg.ItemsSource != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(dg.ItemsSource);
                Predicate<object> yourCostumFilter = null;

                if (dg.Name == "MaterialReviewDataGrid")
                {
                    var query1 = new Predicate<object>(x => ((MaterialReviewLine)x).JobNr.ToLower().Contains(filterText));
                    var query2 = new Predicate<object>(x => ((MaterialReviewLine)x).Contract.ToLower().Contains(filterText));
                    var query4 =
                        universalDeliveryWeekComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((MaterialReviewLine)x).WeekNum == int.Parse(universalDeliveryWeekComboBox.SelectedValue.ToString())) : null;
                    var query5 =
                        universalDeliveryMonthComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((MaterialReviewLine)x).DeliveryDate <= DateTime.Parse(universalDeliveryMonthComboBox.SelectedValue.ToString())) : null;
                    var query6 =
                        universalDeliveryMonthComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((MaterialReviewLine)x).DeliveryDate >= ConvertToFirstDayOfMonth(DateTime.Parse(universalDeliveryMonthComboBox.SelectedValue.ToString()))) : null;
                    var query7 =
                        MaterialRiskComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((MaterialReviewLine)x).MaterialStatus == MaterialRiskComboBox.SelectedValue.ToString()) : null;


                    System.Diagnostics.Debug.Print("Q1{0}, Q2{1}, Q5{2}, Q6{3}", query1, query2, query5, query6);

                    yourCostumFilter = new Predicate<object>(x =>
                        (query1(x) || query2(x)) &&
                        (query4 == null ? true : query4(x)) &&
                        (query5 == null ? true : query5(x)) &&
                        (query6 == null ? true : query6(x)) &&
                        (query7 == null ? true : query7(x)));

                    view.Filter = yourCostumFilter;
                    dg.ItemsSource = view;


                }
                else if (dg.Name == "CreditStopReviewDataGrid")
                {
                    var query1 =
                        universalDeliveryMonthComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => DateTime.Compare(((CreditStopReviewLine)x).DeliveryMonth, (DateTime)universalDeliveryMonthComboBox.SelectedItem) == 0) : null;
                    var query2 =
                        customerStatusComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((CreditStopReviewLine)x).CustomerStatus == customerStatusComboBox.SelectedValue.ToString()) : null;
                    var query3 = new Predicate<object>(x => ((CreditStopReviewLine)x).Customer.ToLower().Contains(filterText));
                    var query4 = new Predicate<object>(x => ((CreditStopReviewLine)x).Contract.ToLower().Contains(filterText));

                    yourCostumFilter = new Predicate<object>(x =>
                        (query3(x) || query4(x)) &&
                        (query1 == null ? true : query1(x)) &&
                        (query2 == null ? true : query2(x)));

                    view.Filter = yourCostumFilter;
                    dg.ItemsSource = view;

                }
                else
                {

                    var query1 = new Predicate<object>(x => ((IdslTrackerLine)x).JobNo.ToLower().Contains(filterText));
                    var query2 = new Predicate<object>(x => ((IdslTrackerLine)x).Contract.ToLower().Contains(filterText));

                    if (filterText.ToLower().Contains("mmh"))
                    {
                        query2 = new Predicate<object>(x => (((IdslTrackerLine)x).Contract.ToLower().Contains(filterText) || ((IdslTrackerLine)x).Contract.ToLower().Contains("midland metro")));

                    }

                    var query3 = new Predicate<object>(x => ((IdslTrackerLine)x).ProductionCommentDoor.ToLower().Contains(filterText));
                    var query4 = new Predicate<object>(x => ((IdslTrackerLine)x).ProductionCommentFrame.ToLower().Contains(filterText));
                    var query5 = new Predicate<object>(x => ((IdslTrackerLine)x).Customer.ToLower().Contains(filterText));
                    var query6 =
                        universalDeliveryWeekComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((IdslTrackerLine)x).WeekNum == int.Parse(universalDeliveryWeekComboBox.SelectedValue.ToString())) : null;
                    var query7 =
                        universalDeliveryMonthComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((IdslTrackerLine)x).DeliveryDate <= DateTime.Parse(universalDeliveryMonthComboBox.SelectedValue.ToString())) : null;
                    var query8 =
                        universalDeliveryMonthComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((IdslTrackerLine)x).DeliveryDate >= ConvertToFirstDayOfMonth(DateTime.Parse(universalDeliveryMonthComboBox.SelectedValue.ToString()))) : null;
                    var query10 =
                        manfRepComboBox.SelectedValue.ToString() != "ALL" ?
                        new Predicate<object>(x => ((IdslTrackerLine)x).ManufactureRep.Equals(manfRepComboBox.SelectedValue.ToString())) : null;



                    var query11 = new Predicate<object>(x => true);
                    if (manfCompleteComboBox.SelectedValue.ToString() == "Yes")
                    {
                        query11 = new Predicate<object>(x => ((IdslTrackerLine)x).ManufactureCompleted == true);
                    }
                    else if (manfCompleteComboBox.SelectedValue.ToString() == "No")
                    {
                        query11 = new Predicate<object>(x => ((IdslTrackerLine)x).ManufactureCompleted == false);

                    }
                    else if (manfCompleteComboBox.SelectedValue.ToString() == "Accrued")
                    {
                        query11 = new Predicate<object>(x => ((IdslTrackerLine)x).IsAccrued == true);
                    }

                    //var query12 =
                    //        (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemText != "ALL" ?
                    //        new Predicate<object>(x =>
                    //            (((IdslTrackerLine)x).ProductionCommentDoorColourHex.ToString() == (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemColour.ToString()
                    //            || ((IdslTrackerLine)x).ProductionCommentFrameColourHex.ToString() == (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemColour.ToString()
                    //            || (((IdslTrackerLine)x).ProductionCommentDoorColourHex.ToString() == string.Empty && (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemText == "No Fill")
                    //            || (((IdslTrackerLine)x).ProductionCommentFrameColourHex.ToString() == string.Empty && (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemText == "No Fill")


                    //            )) : null;

                    var query12 =
                            (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemText != "ALL" ?
                            new Predicate<object>(x =>
                                (

                                ((IdslTrackerLine)x).ProductionCommentDoorColourHex.ToString() ==
                                    ((SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemText == "No Fill" ? string.Empty : (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemColour.ToString())
                                || ((IdslTrackerLine)x).ProductionCommentFrameColourHex.ToString() ==
                                    ((SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemText == "No Fill" ? string.Empty : (SearchColourComboBox.SelectedItem as ColourComboBoxItem).cbItemColour.ToString())



                                )) : null;





                    yourCostumFilter = new Predicate<object>(x =>
                        (query1(x) || query2(x) || query3(x) || query4(x) || query5(x)) &&
                        (query6 == null ? true : query6(x)) &&
                        (query7 == null ? true : query7(x)) &&
                        (query8 == null ? true : query8(x)) &&
                        (query10 == null ? true : query10(x)) &&
                        (query11 == null ? true : query11(x)) &&
                        (query12 == null ? true : query12(x)));

                    view.Filter = yourCostumFilter;
                    dg.ItemsSource = view;



                    List<IdslTrackerLine> lines = new List<IdslTrackerLine>();
                    try
                    {
                        lines = ((List<IdslTrackerLine>)dg.ItemsSource) as List<IdslTrackerLine>;

                    }
                    catch
                    {
                        lines = CollectionViewSource.GetDefaultView(dg.ItemsSource).Cast<IdslTrackerLine>().ToList();
                    }

                    foreach (IdslTrackerLine line in lines)
                    {
                        filteredTotal = filteredTotal + line.Sales;
                    }
                }


                ProductionTrackerFilteredTotalTextBox.Text = string.Format(("{0:C}"), filteredTotal);
                ThirdPartyFilteredTotalTextBox.Text = string.Format(("{0:C}"), filteredTotal);
                MiscFilteredTotalTextBox.Text = string.Format(("{0:C}"), filteredTotal);
                IronmongeryFilteredTotalTextBox.Text = string.Format(("{0:C}"), filteredTotal);
                InvoicedAndStoredFilteredTotalTextBox.Text = string.Format(("{0:C}"), filteredTotal);

            }
            ////////////////////////////////////////

        }


        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                ApplyFilter();
            }
        }

        //private void Window_Closing(object sender, CancelEventArgs e)
        //{
        //    MainMenuWindow mainMenuWindow = new MainMenuWindow();
        //    mainMenuWindow.Show();
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DataGrid dg = GetVisibleDataGrid();
            RefreshDataGrid(dg);
        }





        private void BulkUpdateManufactureInfo_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            BulkManufactureWindow bulkManufactureWindow = new BulkManufactureWindow();

            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    bulkManufactureWindow.Owner = this;
                    bulkManufactureWindow.trackerLines = dg.SelectedItems.OfType<IdslTrackerLine>().ToList();
                    bulkManufactureWindow.ShowDialog();
                    if (bulkManufactureWindow.DialogResult == true)
                    {
                        MainTabControl.Visibility = Visibility.Hidden;
                        HeaderControlsGrid.Visibility = Visibility.Hidden;
                        loadingGif.Visibility = Visibility.Visible;
                        List<IdslTrackerLine> updatedLines = bulkManufactureWindow.updatedTrackerLines;

                        foreach (IdslTrackerLine updatedLine in updatedLines)
                        {
                            for (int i = 0; i < dg.Items.Count; i++)
                            {
                                if (((IdslTrackerLine)dg.Items[i]).DocNumber == updatedLine.DocNumber)
                                {
                                    ((IdslTrackerLine)dg.Items[i]).ProductionCommentDoorColourHex = updatedLine.ProductionCommentDoorColourHex;
                                    ((IdslTrackerLine)dg.Items[i]).ProductionCommentFrameColourHex = updatedLine.ProductionCommentFrameColourHex;
                                    ((IdslTrackerLine)dg.Items[i]).DeliveryDate = updatedLine.DeliveryDate;
                                    ((IdslTrackerLine)dg.Items[i]).DeliveryDateOverride = updatedLine.DeliveryDateOverride;
                                    ((IdslTrackerLine)dg.Items[i]).ProductionCommentDoor = updatedLine.ProductionCommentDoor;
                                    ((IdslTrackerLine)dg.Items[i]).ProductionCommentFrame = updatedLine.ProductionCommentFrame;
                                    ((IdslTrackerLine)dg.Items[i]).ManufactureCompleted = updatedLine.ManufactureCompleted;
                                    ((IdslTrackerLine)dg.Items[i]).ManufactureEndDate = updatedLine.ManufactureEndDate;
                                    ((IdslTrackerLine)dg.Items[i]).ManufactureRep = updatedLine.ManufactureRep;
                                    ((IdslTrackerLine)dg.Items[i]).IsHighEndFinish = updatedLine.IsHighEndFinish;
                                    break;
                                }
                            }
                        }
                      
                        RefreshDataGrid(dg);

                    }
                    else
                    {
                        MessageBox.Show(this, "Bulk Operation cancelled by user.");
                    }
                });

            };

            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    dg.Items.Refresh();
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                    HeaderControlsGrid.Visibility = Visibility.Visible;
                });
            };
            bgw.RunWorkerAsync();
           
        }


        private void CopyContract_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.Contract}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }


        private void CopyBatchRef_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.BatchRef}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }

        private void CopyCustomer_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.Customer}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }

        private void CopyDocNumber_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.DocNumber}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }
        private void CopyMiscComments_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.ProductionCommentDoor}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }
        private void CopyThirdPartyComments_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.ProductionCommentDoor}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }
        private void CopyShopfloorComments_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.ShopfloorComment}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }
        private void CopyIronmongeryComments_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.ProductionCommentDoor}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }


        private void CopyJobNo_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.JobNo}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }

        private void CopyProductionCommentsDoor_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.ProductionCommentDoor}\n";
            }
            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }

        private void CopyProductionCommentsFrame_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            string s = string.Empty;
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                s = $"{s}{trackerLine.ProductionCommentFrame}\n";
            }

            s = s.Substring(0, s.Length - 2);
            Clipboard.SetText(s);
        }

        private void BulkUpdatePrinted_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //List<string> docNumbers = new List<string>();
            DataGrid dg = GetVisibleDataGrid();
            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                if (trackerLine.FilePrintedDate != null)
                {
                    MessageBox.Show(this, "Cannot apply bulk operation as one or more items has already been printed.");
                    return;
                }
                //docNumbers.Add(trackerLine.DocNumber);
            }
            BulkPrintWindow bulkPrintWindow = new BulkPrintWindow();
            bulkPrintWindow.Owner = this;
            bulkPrintWindow.trackerLines = dg.SelectedItems.OfType<IdslTrackerLine>().ToList();
            bulkPrintWindow.ShowDialog();
            if (bulkPrintWindow.DialogResult == true)
            {
                //RefreshDataGrid(dg);
                dg.Items.Refresh();
            }
            else
            {
                MessageBox.Show(this, "Bulk Operation cancelled by user.");
            }



        }

        //private void BulkUpdateProductionComment_MenuItem_Click(object sender, RoutedEventArgs e)
        //{

        //    DataGrid dg = GetVisibleDataGrid();
        //    BulkProductionCommentWindow bulkProductionCommentWindow = new BulkProductionCommentWindow();

        //    var bgw = new BackgroundWorker();
        //    bgw.DoWork += (_, __) =>
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {

        //            bulkProductionCommentWindow.Owner = this;
        //            bulkProductionCommentWindow.trackerLines = dg.SelectedItems.OfType<IdslTrackerLine>().ToList(); ;
        //            bulkProductionCommentWindow.ShowDialog();
        //            if (bulkProductionCommentWindow.DialogResult == true)
        //            {

        //                MainTabControl.Visibility = Visibility.Hidden;
        //                HeaderControlsGrid.Visibility = Visibility.Hidden;
        //                loadingGif.Visibility = Visibility.Visible;
        //                List<IdslTrackerLine> updatedLines = bulkProductionCommentWindow.updatedTrackerLines;

        //                foreach (IdslTrackerLine updatedLine in updatedLines)
        //                {
        //                    for (int i = 0; i < dg.Items.Count; i++)
        //                    {
        //                        if (((IdslTrackerLine)dg.Items[i]).DocNumber == updatedLine.DocNumber)
        //                        {
        //                            ((IdslTrackerLine)dg.Items[i]).ProductionCommentDoor = updatedLine.ProductionCommentDoor;
        //                            ((IdslTrackerLine)dg.Items[i]).ProductionCommentFrame = updatedLine.ProductionCommentFrame;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show(this, "Bulk Operation cancelled by user.");
        //            }
        //        });

        //    };

        //    bgw.RunWorkerCompleted += (_, __) =>
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            dg.Items.Refresh();
        //            loadingGif.Visibility = Visibility.Hidden;
        //            MainTabControl.Visibility = Visibility.Visible;
        //            HeaderControlsGrid.Visibility = Visibility.Visible;
        //        });
        //    };
        //    bgw.RunWorkerAsync();
        //}


        private void BulkUpdateMaterialComment_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            BulkMaterialCommentWindow bulkMaterialCommentWindow = new BulkMaterialCommentWindow();
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    bulkMaterialCommentWindow.Owner = this;
                    bulkMaterialCommentWindow.trackerLines = dg.SelectedItems.OfType<IdslTrackerLine>().ToList();
                    bulkMaterialCommentWindow.ShowDialog();
                    if (bulkMaterialCommentWindow.DialogResult == true)
                    {
                        MainTabControl.Visibility = Visibility.Hidden;
                        HeaderControlsGrid.Visibility = Visibility.Hidden;
                        loadingGif.Visibility = Visibility.Visible;
                        List<IdslTrackerLine> updatedLines = bulkMaterialCommentWindow.updatedTrackerLines;

                        foreach (IdslTrackerLine updatedLine in updatedLines)
                        {
                            for (int i = 0; i < dg.Items.Count; i++)
                            {
                                if (((IdslTrackerLine)dg.Items[i]).DocNumber == updatedLine.DocNumber)
                                {
                                    ((IdslTrackerLine)dg.Items[i]).ManualMaterialComment = updatedLine.ManualMaterialComment;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "Bulk Operation cancelled by user.");
                    }
                });

            };

            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    dg.Items.Refresh();
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                    HeaderControlsGrid.Visibility = Visibility.Visible;
                });
            };
            bgw.RunWorkerAsync();
        }

        private void BulkUpdateManufactureComplete_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            List<IdslTrackerLine> selectedItems = dg.SelectedItems.OfType<IdslTrackerLine>().ToList();
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {

                    MainTabControl.Visibility = Visibility.Hidden;
                    HeaderControlsGrid.Visibility = Visibility.Hidden;
                    loadingGif.Visibility = Visibility.Visible;
                });

                string docNrs = string.Empty;

                foreach (IdslTrackerLine trackerLine in selectedItems)
                {
                    docNrs = string.Format("{0}{1}|", docNrs, trackerLine.DocNumber);
                    if (trackerLine.ManufactureCompleted == true)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(this, string.Format("Bulk operation skipped for {0} as it is already complete", trackerLine.JobNo));
                        });
                    }
                    else
                    {
                        using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
                        {
                            using (SqlCommand command = new SqlCommand("Tracker.dbo.PUT_V2_TRACKER_ARCHIVE_LINE_BULK_MANUFACTURE_COMPLETE", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                                command.Parameters.AddWithValue("@ManufactureCompleted", 1);
                                command.Parameters.AddWithValue("@ManufactureEndDate", DateTime.Now);
                                command.Parameters.AddWithValue("@Username", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                                command.Parameters.AddWithValue("@JobNr", trackerLine.JobNo);


                                connection.Open();
                                command.ExecuteNonQuery();
                                connection.Close();

                            }
                        }
                    }

                }

                docNrs = docNrs.Remove(docNrs.Length - 1, 1);
                List<IdslTrackerLine> updatedTrackerLines = new List<IdslTrackerLine>();
                using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
                {
                    using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_IDSL_V2_TRACKER_LINES", connection))
                    {
                        command.CommandTimeout = 180;
                        command.CommandType = CommandType.StoredProcedure;
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

                foreach (IdslTrackerLine updatedLine in updatedTrackerLines)
                {
                    for (int i = 0; i < dg.Items.Count; i++)
                    {
                        if (((IdslTrackerLine)dg.Items[i]).DocNumber == updatedLine.DocNumber)
                        {
                            ((IdslTrackerLine)dg.Items[i]).ManufactureCompleted = updatedLine.ManufactureCompleted;
                            ((IdslTrackerLine)dg.Items[i]).ManufactureEndDate = updatedLine.ManufactureEndDate;
                            break;
                        }
                    }
                }

            };

            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    dg.Items.Refresh();
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                    HeaderControlsGrid.Visibility = Visibility.Visible;
                });
            };
            bgw.RunWorkerAsync();
        }

        //private void BulkUpdateDeliveryDate_MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    DataGrid dg = GetVisibleDataGrid();
        //    BulkDeliveryDateWindow bulkDeliveryDateWindow = new BulkDeliveryDateWindow();
        //    var bgw = new BackgroundWorker();
        //    bgw.DoWork += (_, __) =>
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            bulkDeliveryDateWindow.Owner = this;
        //            bulkDeliveryDateWindow.trackerLines = dg.SelectedItems.OfType<IdslTrackerLine>().ToList();
        //            bulkDeliveryDateWindow.ShowDialog();
        //            if (bulkDeliveryDateWindow.DialogResult == true)
        //            {
        //                MainTabControl.Visibility = Visibility.Hidden;
        //                HeaderControlsGrid.Visibility = Visibility.Hidden;
        //                loadingGif.Visibility = Visibility.Visible;
        //                List<IdslTrackerLine> updatedLines = bulkDeliveryDateWindow.updatedTrackerLines;

        //                foreach (IdslTrackerLine updatedLine in updatedLines)
        //                {
        //                    for (int i = 0; i < dg.Items.Count; i++)
        //                    {
        //                        if (((IdslTrackerLine)dg.Items[i]).DocNumber == updatedLine.DocNumber)
        //                        {
        //                            ((IdslTrackerLine)dg.Items[i]).DeliveryDate = updatedLine.DeliveryDate;
        //                            ((IdslTrackerLine)dg.Items[i]).DeliveryDateOverride = updatedLine.DeliveryDateOverride;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show(this, "Bulk Operation cancelled by user.");
        //            }
        //        });

        //    };

        //    bgw.RunWorkerCompleted += (_, __) =>
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            dg.Items.Refresh();
        //            loadingGif.Visibility = Visibility.Hidden;
        //            MainTabControl.Visibility = Visibility.Visible;
        //            HeaderControlsGrid.Visibility = Visibility.Visible;
        //        });
        //    };
        //    bgw.RunWorkerAsync();
        //}

        //private void BulkUpdateManufactureStartDate_MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    DataGrid dg = GetVisibleDataGrid();
        //    BulkManufactureStartDateWindow bulkManufactureStartDateWindow = new BulkManufactureStartDateWindow();

        //    var bgw = new BackgroundWorker();
        //    bgw.DoWork += (_, __) =>
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            bulkManufactureStartDateWindow.Owner = this;
        //            bulkManufactureStartDateWindow.trackerLines = dg.SelectedItems.OfType<IdslTrackerLine>().ToList();
        //            bulkManufactureStartDateWindow.ShowDialog();
        //            if (bulkManufactureStartDateWindow.DialogResult == true)
        //            {

        //                MainTabControl.Visibility = Visibility.Hidden;
        //                HeaderControlsGrid.Visibility = Visibility.Hidden;
        //                loadingGif.Visibility = Visibility.Visible;
        //                List<IdslTrackerLine> updatedLines = bulkManufactureStartDateWindow.updatedTrackerLines;

        //                foreach (IdslTrackerLine updatedLine in updatedLines)
        //                {
        //                    for (int i = 0; i < dg.Items.Count; i++)
        //                    {
        //                        if (((IdslTrackerLine)dg.Items[i]).DocNumber == updatedLine.DocNumber)
        //                        {
        //                            ((IdslTrackerLine)dg.Items[i]).ManufactureStartDate = updatedLine.ManufactureStartDate;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show(this, "Bulk Operation cancelled by user.");
        //            }
        //        });

        //    };

        //    bgw.RunWorkerCompleted += (_, __) =>
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            dg.Items.Refresh();
        //            loadingGif.Visibility = Visibility.Hidden;
        //            MainTabControl.Visibility = Visibility.Visible;
        //            HeaderControlsGrid.Visibility = Visibility.Visible;
        //        });
        //    };
        //    bgw.RunWorkerAsync();
        //}


        private void BulkAddToAccruals_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();

            foreach (IdslTrackerLine trackerLine in dg.SelectedItems)
            {
                using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
                {
                    using (SqlCommand command = new SqlCommand("Tracker.dbo.ADD_ACCRUAL_LINE", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@DocNr", trackerLine.DocNumber);
                        command.Parameters.AddWithValue("@Value", trackerLine.Sales);
                        command.Parameters.AddWithValue("@Date", new DateTime(2099, 12, 31));


                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                    }
                }
            }


            RefreshDataGrid(dg);
        }

        private void CustomerStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DeliveryMonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            universalDeliveryWeekComboBox.SelectedIndex = 0;
            ApplyFilter();
            //if(universalDeliveryMonthComboBox.SelectedIndex != 0)
            //{
            PopulatedFilteredDeliveryWeekComboBox();
            //}

        }

        private void PopulatedFilteredDeliveryWeekComboBox()
        {
            DataGrid dg = GetVisibleDataGrid();
            List<int> deliveryWeekList = new List<int>();

            if (dg.Name != "CreditStopReviewDataGrid")
            {
                if (dg.Name == "MaterialReviewDataGrid")
                {
                    List<MaterialReviewLine> lines = new List<MaterialReviewLine>();
                    try
                    {
                        lines = ((List<MaterialReviewLine>)dg.ItemsSource) as List<MaterialReviewLine>;

                    }
                    catch
                    {
                        lines = CollectionViewSource.GetDefaultView(dg.ItemsSource).Cast<MaterialReviewLine>().ToList();
                    }


                    foreach (MaterialReviewLine line in lines)
                    {

                        if (!deliveryWeekList.Contains(line.WeekNum))
                        {
                            deliveryWeekList.Add(line.WeekNum);
                        }



                    }
                }
                else
                {
                    List<IdslTrackerLine> lines = new List<IdslTrackerLine>();
                    try
                    {
                        lines = ((List<IdslTrackerLine>)dg.ItemsSource) as List<IdslTrackerLine>;

                    }
                    catch
                    {
                        lines = CollectionViewSource.GetDefaultView(dg.ItemsSource).Cast<IdslTrackerLine>().ToList();
                    }

                    foreach (IdslTrackerLine line in lines)
                    {

                        if (!deliveryWeekList.Contains(line.WeekNum))
                        {
                            deliveryWeekList.Add(line.WeekNum);
                        }



                    }

                }

                deliveryWeekList.Sort();
                //universalDeliveryWeekComboBox.Items.Clear();
                //universalDeliveryWeekComboBox.Items.Add("All");

                universalDeliveryWeekComboBox.SelectedIndex = 0;
                for (int i = universalDeliveryWeekComboBox.Items.Count - 1; i > 0; i--)
                {
                    universalDeliveryWeekComboBox.Items.RemoveAt(i);
                }
                System.Diagnostics.Debug.Print(universalDeliveryWeekComboBox.Items.Count.ToString());

                //universalDeliveryWeekComboBox.SelectedIndex = 0;
                foreach (int wk in deliveryWeekList)
                {
                    if (wk != 0)
                    {
                        if (!universalDeliveryWeekComboBox.Items.Contains(wk))
                        {
                            universalDeliveryWeekComboBox.Items.Add(wk);
                        }
                    }

                }
            }




        }

        //private void CreaitStopReviewClearFiltersButton_Click(object sender, RoutedEventArgs e)
        //{
        //    DeliveryMonthComboBox.SelectedIndex = -1;
        //    CustomerStatusComboBox.SelectedIndex = -1;
        //}


        System.Windows.Threading.DispatcherTimer _selectingCreditStopReviewDataGridTimer;

        private void CreditStopReviewDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectingCreditStopReviewDataGridTimer == null)
            {
                _selectingCreditStopReviewDataGridTimer = new System.Windows.Threading.DispatcherTimer();
                _selectingCreditStopReviewDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

                _selectingCreditStopReviewDataGridTimer.Tick += new EventHandler(this.handleCreditStopReviewSelectingTimerTimeout);
            }
            _selectingCreditStopReviewDataGridTimer.Stop();
            //_selectingTimer.Tag = (sender as TextBox).Text; 
            _selectingCreditStopReviewDataGridTimer.Start();
        }

        private void handleCreditStopReviewSelectingTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            decimal highlightedTotal = 0;
            foreach (CreditStopReviewLine creditStopReviewLine in CreditStopReviewDataGrid.SelectedItems)
            {
                highlightedTotal = highlightedTotal + creditStopReviewLine.CreditLimit;
            }

            CreditStopreviewHighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }

        private void MaterialRiskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        System.Windows.Threading.DispatcherTimer _selectingMainTrackerDataGridTimer;

        private void MainTrackerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectingMainTrackerDataGridTimer == null)
            {
                _selectingMainTrackerDataGridTimer = new System.Windows.Threading.DispatcherTimer();
                _selectingMainTrackerDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

                _selectingMainTrackerDataGridTimer.Tick += new EventHandler(this.handleSelectingMainTrackerDataGridTimerTimeout);
            }
            _selectingMainTrackerDataGridTimer.Stop();
            //_selectingTimer.Tag = (sender as TextBox).Text; 
            _selectingMainTrackerDataGridTimer.Start();
        }

        private void handleSelectingMainTrackerDataGridTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            decimal highlightedTotal = 0;

            foreach (IdslTrackerLine line in MainTrackerDataGrid.SelectedItems)
            {
                highlightedTotal = highlightedTotal + line.Sales;
            }

            ProductionTrackerHighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }


        System.Windows.Threading.DispatcherTimer _selectingThirdPartyTrackerDataGridTimer;

        private void ThirdPartyTrackerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectingThirdPartyTrackerDataGridTimer == null)
            {
                _selectingThirdPartyTrackerDataGridTimer = new System.Windows.Threading.DispatcherTimer();
                _selectingThirdPartyTrackerDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

                _selectingThirdPartyTrackerDataGridTimer.Tick += new EventHandler(this.handleSelectingThirdPartyTrackerDataGridTimerTimeout);
            }
            _selectingThirdPartyTrackerDataGridTimer.Stop();
            //_selectingTimer.Tag = (sender as TextBox).Text; 
            _selectingThirdPartyTrackerDataGridTimer.Start();
        }

        private void handleSelectingThirdPartyTrackerDataGridTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            decimal highlightedTotal = 0;

            foreach (IdslTrackerLine line in ThirdPartyTrackerDataGrid.SelectedItems)
            {
                highlightedTotal = highlightedTotal + line.Sales;
            }

            ThirdPartyHighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }

        System.Windows.Threading.DispatcherTimer _selectingMiscTrackerDataGridTimer;

        private void MiscTrackerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectingMiscTrackerDataGridTimer == null)
            {
                _selectingMiscTrackerDataGridTimer = new System.Windows.Threading.DispatcherTimer();
                _selectingMiscTrackerDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

                _selectingMiscTrackerDataGridTimer.Tick += new EventHandler(this.handleSelectingMiskTrackerDataGridTimerTimeout);
            }
            _selectingMiscTrackerDataGridTimer.Stop();
            //_selectingTimer.Tag = (sender as TextBox).Text; 
            _selectingMiscTrackerDataGridTimer.Start();
        }

        private void handleSelectingMiskTrackerDataGridTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            decimal highlightedTotal = 0;

            foreach (IdslTrackerLine line in MiscTrackerDataGrid.SelectedItems)
            {
                highlightedTotal = highlightedTotal + line.Sales;
            }

            MiscHighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }

        System.Windows.Threading.DispatcherTimer _selectingIronTrackerDataGridTimer;

        private void IronTrackerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectingIronTrackerDataGridTimer == null)
            {
                _selectingIronTrackerDataGridTimer = new System.Windows.Threading.DispatcherTimer();
                _selectingIronTrackerDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

                _selectingIronTrackerDataGridTimer.Tick += new EventHandler(this.handleSelectingIronTrackerDataGridTimerTimeout);
            }
            _selectingIronTrackerDataGridTimer.Stop();
            //_selectingTimer.Tag = (sender as TextBox).Text; 
            _selectingIronTrackerDataGridTimer.Start();
        }

        private void handleSelectingIronTrackerDataGridTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            decimal highlightedTotal = 0;

            foreach (IdslTrackerLine line in IronTrackerDataGrid.SelectedItems)
            {
                highlightedTotal = highlightedTotal + line.Sales;
            }

            IronmongeryHighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }

        System.Windows.Threading.DispatcherTimer _selectingVestingTrackerDataGridTimer;

        private void VestingTrackerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectingVestingTrackerDataGridTimer == null)
            {
                _selectingVestingTrackerDataGridTimer = new System.Windows.Threading.DispatcherTimer();
                _selectingVestingTrackerDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

                _selectingVestingTrackerDataGridTimer.Tick += new EventHandler(this.handleSelectingVestingTrackerDataGridTimerTimeout);
            }
            _selectingVestingTrackerDataGridTimer.Stop();
            //_selectingTimer.Tag = (sender as TextBox).Text; 
            _selectingVestingTrackerDataGridTimer.Start();
        }

        private void handleSelectingVestingTrackerDataGridTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            decimal highlightedTotal = 0;

            foreach (IdslTrackerLine line in VestingTrackerDataGrid.SelectedItems)
            {
                highlightedTotal = highlightedTotal + line.Sales;
            }

            InvoicedAndStoredHighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }

        private void UniversalDeliveryWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void NinthCharacterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();

        }

        private void ManfRepComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ManfCompleteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void MainTrackerDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var dgSender = (DataGrid)sender;
            var cView = CollectionViewSource.GetDefaultView(dgSender.ItemsSource);

            //Alternate between ascending/descending if the same column is clicked 
            ListSortDirection direction = ListSortDirection.Ascending;
            if (cView.SortDescriptions.FirstOrDefault().PropertyName == e.Column.SortMemberPath)
                direction = cView.SortDescriptions.FirstOrDefault().Direction == ListSortDirection.Descending ? ListSortDirection.Ascending : ListSortDirection.Descending;

            cView.SortDescriptions.Clear();
            AddSortColumn((DataGrid)sender, e.Column.SortMemberPath, direction);
            //To this point the default sort functionality is implemented

            //Now check the wanted columns and add multiple sort 
            //if (e.Column.SortMemberPath == "WantedColumn")
            //{
            AddSortColumn((DataGrid)sender, "JobNo", direction);
            //}
            e.Handled = true;
        }

        private void AddSortColumn(DataGrid sender, string sortColumn, ListSortDirection direction)
        {
            var cView = CollectionViewSource.GetDefaultView(sender.ItemsSource);
            cView.SortDescriptions.Add(new SortDescription(sortColumn, direction));
            //Add the sort arrow on the DataGridColumn
            foreach (var col in sender.Columns.Where(x => x.SortMemberPath == sortColumn))
            {
                col.SortDirection = direction;
            }
        }

        private void CreditStopReviewDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();

            CreditStopCommentWindow detailViewWindow = new CreditStopCommentWindow();
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    detailViewWindow.Owner = this;
                    detailViewWindow.creditStopReviewLine = dg.SelectedItem as CreditStopReviewLine;
                    detailViewWindow.ShowDialog();
                    if (detailViewWindow.DialogResult == true)
                    {
                        MainTabControl.Visibility = Visibility.Hidden;
                        HeaderControlsGrid.Visibility = Visibility.Hidden;
                        loadingGif.Visibility = Visibility.Visible;

                    }

                });
                PopulateCreditStopReviewDataGrid();

            };

            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                    HeaderControlsGrid.Visibility = Visibility.Visible;
                    ApplyFilter();
                });
            };
            bgw.RunWorkerAsync();
        }

        private void BulkUpdateMaterialReveiwComments_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            CreditStopReviewLine selectedLine = dg.SelectedItem as CreditStopReviewLine;

            CreditStopCommentWindow detailViewWindow = new CreditStopCommentWindow();
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    detailViewWindow.Owner = this;
                    detailViewWindow.creditStopReviewLines = dg.SelectedItems.OfType<CreditStopReviewLine>().ToList();
                    detailViewWindow.ShowDialog();
                    if (detailViewWindow.DialogResult == true)
                    {
                        MainTabControl.Visibility = Visibility.Hidden;
                        HeaderControlsGrid.Visibility = Visibility.Hidden;
                        loadingGif.Visibility = Visibility.Visible;
                    }
                });


                PopulateCreditStopReviewDataGrid();

            };

            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                    HeaderControlsGrid.Visibility = Visibility.Visible;
                });
            };
            bgw.RunWorkerAsync();



        }

        private void SearchColourComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ExportStockAndWip_Button_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dg = GetVisibleDataGrid();
            ICollectionView view = CollectionViewSource.GetDefaultView(dg.ItemsSource);
            view.Filter = new Predicate<object>(x => true);
            IWorkbook workbook = new XSSFWorkbook();


            XSSFFont myFont = (XSSFFont)workbook.CreateFont();
            myFont.FontHeightInPoints = 11;
            myFont.FontName = "Calibri";


            XSSFCellStyle headerCellStyleY = (XSSFCellStyle)workbook.CreateCellStyle();
            headerCellStyleY.SetFont(myFont);
            headerCellStyleY.BorderLeft = BorderStyle.Medium;
            headerCellStyleY.BorderTop = BorderStyle.Medium;
            headerCellStyleY.BorderRight = BorderStyle.Medium;
            headerCellStyleY.BorderBottom = BorderStyle.Medium;
            headerCellStyleY.FillPattern = FillPattern.SolidForeground;
            headerCellStyleY.FillForegroundColor = IndexedColors.Yellow.Index;

            XSSFCellStyle headerCellStyleG = (XSSFCellStyle)workbook.CreateCellStyle();
            headerCellStyleG.SetFont(myFont);
            headerCellStyleG.BorderLeft = BorderStyle.Medium;
            headerCellStyleG.BorderTop = BorderStyle.Medium;
            headerCellStyleG.BorderRight = BorderStyle.Medium;
            headerCellStyleG.BorderBottom = BorderStyle.Medium;
            headerCellStyleG.FillPattern = FillPattern.SolidForeground;
            headerCellStyleG.FillForegroundColor = IndexedColors.Grey25Percent.Index;

            XSSFCellStyle headerCellStyleW = (XSSFCellStyle)workbook.CreateCellStyle();
            headerCellStyleW.SetFont(myFont);
            headerCellStyleW.BorderLeft = BorderStyle.Medium;
            headerCellStyleW.BorderTop = BorderStyle.Medium;
            headerCellStyleW.BorderRight = BorderStyle.Medium;
            headerCellStyleW.BorderBottom = BorderStyle.Medium;
            headerCellStyleW.FillPattern = FillPattern.SolidForeground;
            headerCellStyleW.FillForegroundColor = IndexedColors.White.Index;

            XSSFCellStyle rowCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            rowCellStyle.SetFont(myFont);
            rowCellStyle.BorderLeft = BorderStyle.Thin;
            rowCellStyle.BorderTop = BorderStyle.Thin;
            rowCellStyle.BorderRight = BorderStyle.Thin;
            rowCellStyle.BorderBottom = BorderStyle.Thin;



            ISheet sheet = workbook.CreateSheet("Stock and WIP");


            IRow HeaderRow = sheet.CreateRow(0);

            CreateCell(HeaderRow, 0, "Manf Site", headerCellStyleY);
            CreateCell(HeaderRow, 1, "CONTRACT", headerCellStyleG);
            CreateCell(HeaderRow, 2, "JOB NO", headerCellStyleG);
            CreateCell(HeaderRow, 3, "DOOR QTY", headerCellStyleG);
            CreateCell(HeaderRow, 4, "FRAME QTY", headerCellStyleG);
            CreateCell(HeaderRow, 5, "SALES", headerCellStyleG);
            CreateCell(HeaderRow, 6, "Customer", headerCellStyleG);
            CreateCell(HeaderRow, 7, "Delivery Date", headerCellStyleG);
            CreateCell(HeaderRow, 8, "Production Comments Door", headerCellStyleG);
            CreateCell(HeaderRow, 9, "Production Comments Frame", headerCellStyleG);

            CreateCell(HeaderRow, 10, "PRE-KITTED, Still at Ratcher", headerCellStyleW);
            CreateCell(HeaderRow, 11, "", headerCellStyleW);
            CreateCell(HeaderRow, 12, "PRE-KITTED, at Concorde", headerCellStyleW);
            CreateCell(HeaderRow, 13, "", headerCellStyleW);
            CreateCell(HeaderRow, 14, "Beamsaw", headerCellStyleW);
            CreateCell(HeaderRow, 15, "", headerCellStyleW);
            CreateCell(HeaderRow, 16, "Lipper", headerCellStyleW);
            CreateCell(HeaderRow, 17, "", headerCellStyleW);
            CreateCell(HeaderRow, 18, "Press", headerCellStyleW);
            CreateCell(HeaderRow, 19, "", headerCellStyleW);
            CreateCell(HeaderRow, 20, "Clean Off", headerCellStyleW);
            CreateCell(HeaderRow, 21, "", headerCellStyleW);
            CreateCell(HeaderRow, 22, "Spray Shop Ratcher", headerCellStyleW);
            CreateCell(HeaderRow, 23, "", headerCellStyleW);
            CreateCell(HeaderRow, 24, "Spray Shop Concorde", headerCellStyleW);
            CreateCell(HeaderRow, 25, "", headerCellStyleW);
            CreateCell(HeaderRow, 26, "Polish Line Ratcher", headerCellStyleW);
            CreateCell(HeaderRow, 27, "", headerCellStyleW);
            CreateCell(HeaderRow, 28, "Polish Line Concorde", headerCellStyleW);
            CreateCell(HeaderRow, 29, "", headerCellStyleW);
            CreateCell(HeaderRow, 30, "CNC", headerCellStyleW);
            CreateCell(HeaderRow, 31, "", headerCellStyleW);
            CreateCell(HeaderRow, 32, "Framing", headerCellStyleW);
            CreateCell(HeaderRow, 33, "", headerCellStyleW);
            CreateCell(HeaderRow, 34, "Bench Ratcher", headerCellStyleW);
            CreateCell(HeaderRow, 35, "", headerCellStyleW);
            CreateCell(HeaderRow, 36, "Bench Concorde", headerCellStyleW);
            CreateCell(HeaderRow, 37, "", headerCellStyleW);
            CreateCell(HeaderRow, 38, "Postform Area", headerCellStyleW);
            CreateCell(HeaderRow, 39, "", headerCellStyleW);
            CreateCell(HeaderRow, 40, "In Storage/Finished Goods", headerCellStyleW);
            CreateCell(HeaderRow, 41, "", headerCellStyleW);


            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 10, 11));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 12, 13));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 14, 15));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 16, 17));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 18, 19));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 20, 21));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 22, 23));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 24, 25));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 26, 27));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 28, 29));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 30, 31));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 32, 33));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 34, 35));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 36, 37));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 38, 39));
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 40, 41));

            List<HexStyle> hexStyles = new List<HexStyle>();
            foreach (IdslTrackerLine trackerLine in view.Cast<IdslTrackerLine>().ToList())
            {
                if (trackerLine.ProductionCommentDoorColourHex.ToString() != "" && hexStyles.FindIndex(f => f.StyleHex == trackerLine.ProductionCommentDoorColourHex.ToString()) == -1)
                {
                    XSSFCellStyle customCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                    customCellStyle.SetFont(myFont);
                    customCellStyle.BorderLeft = BorderStyle.Medium;
                    customCellStyle.BorderTop = BorderStyle.Medium;
                    customCellStyle.BorderRight = BorderStyle.Medium;
                    customCellStyle.BorderBottom = BorderStyle.Medium;
                    customCellStyle.FillPattern = FillPattern.SolidForeground;


                    byte r = Convert.ToByte(trackerLine.ProductionCommentDoorColourHex.ToString().Substring(3, 2).ToUpper(), 16);
                    byte g = Convert.ToByte(trackerLine.ProductionCommentDoorColourHex.ToString().Substring(5, 2), 16);
                    byte b = Convert.ToByte(trackerLine.ProductionCommentDoorColourHex.ToString().Substring(7, 2), 16);
                    XSSFColor color = new XSSFColor(new byte[] { r, g, b });
                    customCellStyle.SetFillForegroundColor(color);
                    hexStyles.Add(new HexStyle(customCellStyle, trackerLine.ProductionCommentDoorColourHex.ToString()));
                }

                if (trackerLine.ProductionCommentFrameColourHex.ToString() != "" && hexStyles.FindIndex(f => f.StyleHex == trackerLine.ProductionCommentFrameColourHex.ToString()) == -1)
                {
                    XSSFCellStyle customCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                    customCellStyle.SetFont(myFont);
                    customCellStyle.BorderLeft = BorderStyle.Medium;
                    customCellStyle.BorderTop = BorderStyle.Medium;
                    customCellStyle.BorderRight = BorderStyle.Medium;
                    customCellStyle.BorderBottom = BorderStyle.Medium;
                    customCellStyle.FillPattern = FillPattern.SolidForeground;


                    byte r = Convert.ToByte(trackerLine.ProductionCommentFrameColourHex.ToString().Substring(3, 2).ToUpper(), 16);
                    byte g = Convert.ToByte(trackerLine.ProductionCommentFrameColourHex.ToString().Substring(5, 2), 16);
                    byte b = Convert.ToByte(trackerLine.ProductionCommentFrameColourHex.ToString().Substring(7, 2), 16);
                    XSSFColor color = new XSSFColor(new byte[] { r, g, b });
                    customCellStyle.SetFillForegroundColor(color);
                    hexStyles.Add(new HexStyle(customCellStyle, trackerLine.ProductionCommentFrameColourHex.ToString()));
                }


            }


            int currentRowInt = 0;
            foreach (IdslTrackerLine trackerLine in view.Cast<IdslTrackerLine>().ToList())
            {
                currentRowInt++;
                IRow currentRow = sheet.CreateRow(currentRowInt);

                CreateCell(currentRow, 0, trackerLine.ManfSite, rowCellStyle);
                CreateCell(currentRow, 1, trackerLine.Contract, rowCellStyle);
                CreateCell(currentRow, 2, trackerLine.JobNo, rowCellStyle);
                CreateCell(currentRow, 3, trackerLine.DoorQty.ToString(), rowCellStyle);
                CreateCell(currentRow, 4, trackerLine.FrameQty.ToString(), rowCellStyle);
                CreateCell(currentRow, 5, trackerLine.Sales.ToString("C", new CultureInfo("en-GB")), rowCellStyle);
                CreateCell(currentRow, 6, trackerLine.Customer, rowCellStyle);
                CreateCell(currentRow, 7, (trackerLine.DeliveryDate ?? DateTime.MinValue).ToString("dd/MM/yyyy"), rowCellStyle);





                if (trackerLine.ProductionCommentDoorColourHex.ToString() != "")
                {
                    CreateCell(currentRow, 8, trackerLine.ProductionCommentDoor, hexStyles[hexStyles.FindIndex(f => f.StyleHex == trackerLine.ProductionCommentDoorColourHex.ToString())].CustomCellStyle);
                }
                else
                {
                    CreateCell(currentRow, 8, trackerLine.ProductionCommentDoor, rowCellStyle);
                }



                if (trackerLine.ProductionCommentFrameColourHex.ToString() != "")
                {
                    CreateCell(currentRow, 9, trackerLine.ProductionCommentFrame, hexStyles[hexStyles.FindIndex(f => f.StyleHex == trackerLine.ProductionCommentFrameColourHex.ToString())].CustomCellStyle);
                }
                else
                {
                    CreateCell(currentRow, 9, trackerLine.ProductionCommentFrame, rowCellStyle);
                }


                CreateCell(currentRow, 10, "", rowCellStyle);
                CreateCell(currentRow, 11, "", rowCellStyle);
                CreateCell(currentRow, 12, "", rowCellStyle);
                CreateCell(currentRow, 13, "", rowCellStyle);
                CreateCell(currentRow, 14, "", rowCellStyle);
                CreateCell(currentRow, 15, "", rowCellStyle);
                CreateCell(currentRow, 16, "", rowCellStyle);
                CreateCell(currentRow, 17, "", rowCellStyle);
                CreateCell(currentRow, 18, "", rowCellStyle);
                CreateCell(currentRow, 19, "", rowCellStyle);
                CreateCell(currentRow, 20, "", rowCellStyle);
                CreateCell(currentRow, 21, "", rowCellStyle);
                CreateCell(currentRow, 22, "", rowCellStyle);
                CreateCell(currentRow, 23, "", rowCellStyle);
                CreateCell(currentRow, 24, "", rowCellStyle);
                CreateCell(currentRow, 25, "", rowCellStyle);
                CreateCell(currentRow, 26, "", rowCellStyle);
                CreateCell(currentRow, 27, "", rowCellStyle);
                CreateCell(currentRow, 28, "", rowCellStyle);
                CreateCell(currentRow, 29, "", rowCellStyle);
                CreateCell(currentRow, 30, "", rowCellStyle);
                CreateCell(currentRow, 31, "", rowCellStyle);
                CreateCell(currentRow, 32, "", rowCellStyle);
                CreateCell(currentRow, 33, "", rowCellStyle);
                CreateCell(currentRow, 34, "", rowCellStyle);
                CreateCell(currentRow, 35, "", rowCellStyle);
                CreateCell(currentRow, 36, "", rowCellStyle);
                CreateCell(currentRow, 37, "", rowCellStyle);
                CreateCell(currentRow, 38, "", rowCellStyle);
                CreateCell(currentRow, 39, "", rowCellStyle);
                CreateCell(currentRow, 40, "", rowCellStyle);
                CreateCell(currentRow, 41, "", rowCellStyle);




            }



            string filePath = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\exportStockAndWip.xlsx";
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(stream);
            }


            ProcessStartInfo psi = new ProcessStartInfo(filePath);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void CreateCell(IRow CurrentRow, int CellIndex, string Value, XSSFCellStyle Style)
        {
            ICell Cell = CurrentRow.CreateCell(CellIndex);
            Cell.SetCellValue(Value);
            Cell.CellStyle = Style;
        }
    }

    internal class HexStyle
    {
        public HexStyle(XSSFCellStyle customCellStyle, string styleHex)
        {
            CustomCellStyle = customCellStyle;
            StyleHex = styleHex;
        }

        public XSSFCellStyle CustomCellStyle { get; }
        public string StyleHex { get; }
    }

    internal class ColourComboBoxItem
    {
        public string cbItemText { get; internal set; }
        public Brush cbItemColour { get; internal set; }
    }
}

internal class TrackerWipDetail
{
    public string Doc { get; internal set; }
    public string DoorWip { get; internal set; }
    public string FrameWip { get; internal set; }
}