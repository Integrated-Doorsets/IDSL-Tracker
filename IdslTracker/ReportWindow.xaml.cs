using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace IdslTracker
{
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();
            loadingGif.Visibility = Visibility.Hidden;
        }

        private void LoadingGif_MediaEnded(object sender, RoutedEventArgs e)
        {
            loadingGif.Position = new TimeSpan(0, 0, 1);
            loadingGif.Play();
        }

        public void RefreshSopReportDataGrid()
        {

            DateTime startDate = DateTime.Parse(MonthPickComboBox.SelectedValue.ToString());


            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MainTabControl.Visibility = Visibility.Hidden;
                    loadingGif.Visibility = Visibility.Visible;
                });
                PopulateHeaderDates(startDate);

                //PopulateThirdPartySalesReport(startDate);
                //PopulateIronmongerySalesReport(startDate);
                //PopulateManufactureSalesReport(startDate);
                //PopulateOverallSalesReport(startDate);
                PopulateSopSummaryReport(startDate);
                //PopulateMiscSalesReport(startDate);
                PopulateSalesReportLines(startDate);

                PopulatManufactureStartWeekNr();
            };



            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                });
            };
            bgw.RunWorkerAsync();

        }

        private void PopulateSalesReportLines(DateTime startDate)
        {
            List<GenericSalesReportLine> overallSalesReportLines = new List<GenericSalesReportLine>();
            List<GenericSalesReportLine> manufactureSalesReportLines = new List<GenericSalesReportLine>();
            List<GenericSalesReportLine> ironmongerySalesReportLines = new List<GenericSalesReportLine>();
            List<GenericSalesReportLine> thirdPartySalesReportLines = new List<GenericSalesReportLine>();
            List<GenericSalesReportLine> miscSalesReportLines = new List<GenericSalesReportLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                //using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_ALL_SALES_REPORT", connection))
                using (SqlCommand command = new SqlCommand("select * from Tracker.dbo.Reports_SOP_ALL_SALES_REPORT", connection))
                {
                    command.CommandType = CommandType.Text;
                   // command.Parameters.AddWithValue("@StartMonth", startDate);
                    command.CommandTimeout = 350;
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        int i;
                        for (i = 0; i <= dt.Rows.Count - 1; i++)
                        {
                            GenericSalesReportLine salesReportLine = new GenericSalesReportLine();
                            salesReportLine.Catagory = Convert.ToString(dt.Rows[i]["Category"]); //reader.GetString(0);
                            salesReportLine.M1Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M1"]) : 0;// reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                            salesReportLine.M2Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M2"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M2"]) : 0;//reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                            salesReportLine.M3Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M3"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M3"]) : 0;// reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            salesReportLine.M4Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M4"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M4"]) : 0;//reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                            salesReportLine.M5Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M5"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M5"]) : 0;//reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                            salesReportLine.M6Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M6"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M6"]) : 0;//reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));
                            salesReportLine.Table = Convert.ToString(dt.Rows[i]["Table"]); //reader.GetString(7);
                            switch (salesReportLine.Table)
                            {
                                case "OVERALL":
                                    overallSalesReportLines.Add(salesReportLine);
                                    break;
                                case "MANUFACTURE":
                                    manufactureSalesReportLines.Add(salesReportLine);
                                    break;
                                case "IRONMONGERY":
                                    ironmongerySalesReportLines.Add(salesReportLine);
                                    break;
                                case "Third Party":
                                    thirdPartySalesReportLines.Add(salesReportLine);
                                    break;
                                case "MISC":
                                    miscSalesReportLines.Add(salesReportLine);
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                    else { return; }


                    //        using (SqlDataReader reader = command.ExecuteReader())
                    //{

                    //    while (reader.Read())
                    //    {
                    //        GenericSalesReportLine salesReportLine = new GenericSalesReportLine();


                    //        salesReportLine.Catagory = reader.GetString(0);
                    //        salesReportLine.M1Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                    //        salesReportLine.M2Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                    //        salesReportLine.M3Value = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                    //        salesReportLine.M4Value = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                    //        salesReportLine.M5Value = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                    //        salesReportLine.M6Value = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));
                    //        salesReportLine.Table = reader.GetString(7);

                    //        switch(salesReportLine.Table)
                    //        {
                    //            case "OVERALL":
                    //                overallSalesReportLines.Add(salesReportLine);
                    //                break;
                    //            case "MANUFACTURE":
                    //                manufactureSalesReportLines.Add(salesReportLine);
                    //                break;
                    //            case "IRONMONGERY":
                    //                ironmongerySalesReportLines.Add(salesReportLine);
                    //                break;
                    //            case "Third Party":
                    //                thirdPartySalesReportLines.Add(salesReportLine);
                    //                break;
                    //            case "MISC":
                    //                miscSalesReportLines.Add(salesReportLine);
                    //                break;
                    //            default:
                    //                break;
                    //        }


                    //    }
                    //}
                }
                this.Dispatcher.Invoke(() =>
                {
                    TotalOverallSalesDataGrid.ItemsSource = overallSalesReportLines;
                    ManufactureAndMiscSalesDataGrid.ItemsSource = manufactureSalesReportLines;
                    IronmongerySalesDataGrid.ItemsSource = ironmongerySalesReportLines;
                    ThirdPartySalesDataGrid.ItemsSource = thirdPartySalesReportLines;
                    MiscSalesDataGrid.ItemsSource = miscSalesReportLines;
                });
            }
        }

        private void PopulateMiscSalesReport(DateTime startDate)
        {
            List<GenericSalesReportLine> miscSalesReportLines = new List<GenericSalesReportLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_MISC_SALES_REPORT", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", startDate);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            GenericSalesReportLine miscSalesReportLine = new GenericSalesReportLine();


                            miscSalesReportLine.Catagory = reader.GetString(0);
                            miscSalesReportLine.M1Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                            miscSalesReportLine.M2Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                            miscSalesReportLine.M3Value = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            miscSalesReportLine.M4Value = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                            miscSalesReportLine.M5Value = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                            miscSalesReportLine.M6Value = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));

                            miscSalesReportLines.Add(miscSalesReportLine);

                        }
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    MiscSalesDataGrid.ItemsSource = miscSalesReportLines;
                });
            }
        }

        private void PopulateOverallSalesReport(DateTime startDate)
        {
            List<GenericSalesReportLine> overallSalesReportLines = new List<GenericSalesReportLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_OVERALL_SALES_REPORT", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", startDate);
                    command.Parameters.AddWithValue("@ReportId", 0);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            GenericSalesReportLine overallSalesReportLine = new GenericSalesReportLine();


                            overallSalesReportLine.Catagory = reader.GetString(0);
                            overallSalesReportLine.M1Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                            overallSalesReportLine.M2Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                            overallSalesReportLine.M3Value = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            overallSalesReportLine.M4Value = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                            overallSalesReportLine.M5Value = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                            overallSalesReportLine.M6Value = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));

                            overallSalesReportLines.Add(overallSalesReportLine);

                        }
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    TotalOverallSalesDataGrid.ItemsSource = overallSalesReportLines;
                });
            }
        }

        private void PopulateManufactureSalesReport(DateTime startDate)
        {
            List<GenericSalesReportLine> manufactureSalesReportLines = new List<GenericSalesReportLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_MANUFACTURE_AND_MISC_SALES_REPORT", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", startDate);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            GenericSalesReportLine manufactureSalesReportLine = new GenericSalesReportLine();


                            manufactureSalesReportLine.Catagory = reader.GetString(0);
                            manufactureSalesReportLine.M1Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                            manufactureSalesReportLine.M2Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                            manufactureSalesReportLine.M3Value = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            manufactureSalesReportLine.M4Value = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                            manufactureSalesReportLine.M5Value = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                            manufactureSalesReportLine.M6Value = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));

                            manufactureSalesReportLines.Add(manufactureSalesReportLine);

                        }
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    ManufactureAndMiscSalesDataGrid.ItemsSource = manufactureSalesReportLines;
                });
            }
        }


        private void PopulateIronmongerySalesReport(DateTime startDate)
        {
            List<GenericSalesReportLine> ironmongerySalesReportLines = new List<GenericSalesReportLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_IRONMONGERY_SALES_REPORT", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", startDate);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            GenericSalesReportLine ironmongerySalesReportLine = new GenericSalesReportLine();


                            ironmongerySalesReportLine.Catagory = reader.GetString(0);
                            ironmongerySalesReportLine.M1Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                            ironmongerySalesReportLine.M2Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                            ironmongerySalesReportLine.M3Value = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            ironmongerySalesReportLine.M4Value = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                            ironmongerySalesReportLine.M5Value = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                            ironmongerySalesReportLine.M6Value = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));

                            ironmongerySalesReportLines.Add(ironmongerySalesReportLine);

                        }
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    IronmongerySalesDataGrid.ItemsSource = ironmongerySalesReportLines;
                });
            }
        }

        private void PopulateHeaderDates(DateTime startDate)
        {
            DateTime startDate1;
            Globals.HeaderDates = new List<string>();
            int i;
            for (i = 0; i <= 5; i++)
            {
                startDate1 = startDate.AddMonths(i);
                Globals.HeaderDates.Add(startDate1.ToString("MMM")+" " +startDate1.ToString("yy"));
            }


            //using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            //{
            //    using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_TABLE_HEADER_DATES", connection))
            //    {
            //        command.CommandType = CommandType.StoredProcedure;
            //        command.Parameters.AddWithValue("@StartMonth", startDate);
            //        connection.Open();

            //        using (SqlDataReader reader = command.ExecuteReader())
            //        {

            //            while (reader.Read())
            //            {
            //                Globals.HeaderDates.Add(reader.GetString(0));

            //            }
            //        }
            //    }

            //}

            this.Dispatcher.Invoke(() =>
            {
                ThirdPartySalesDataGrid.Columns[1].Header = Globals.HeaderDates[0];
                ThirdPartySalesDataGrid.Columns[2].Header = Globals.HeaderDates[1];
                ThirdPartySalesDataGrid.Columns[3].Header = Globals.HeaderDates[2];
                ThirdPartySalesDataGrid.Columns[4].Header = Globals.HeaderDates[3];
                ThirdPartySalesDataGrid.Columns[5].Header = Globals.HeaderDates[4];
                ThirdPartySalesDataGrid.Columns[6].Header = Globals.HeaderDates[5];


                IronmongerySalesDataGrid.Columns[1].Header = Globals.HeaderDates[0];
                IronmongerySalesDataGrid.Columns[2].Header = Globals.HeaderDates[1];
                IronmongerySalesDataGrid.Columns[3].Header = Globals.HeaderDates[2];
                IronmongerySalesDataGrid.Columns[4].Header = Globals.HeaderDates[3];
                IronmongerySalesDataGrid.Columns[5].Header = Globals.HeaderDates[4];
                IronmongerySalesDataGrid.Columns[6].Header = Globals.HeaderDates[5];


                ManufactureAndMiscSalesDataGrid.Columns[1].Header = Globals.HeaderDates[0];
                ManufactureAndMiscSalesDataGrid.Columns[2].Header = Globals.HeaderDates[1];
                ManufactureAndMiscSalesDataGrid.Columns[3].Header = Globals.HeaderDates[2];
                ManufactureAndMiscSalesDataGrid.Columns[4].Header = Globals.HeaderDates[3];
                ManufactureAndMiscSalesDataGrid.Columns[5].Header = Globals.HeaderDates[4];
                ManufactureAndMiscSalesDataGrid.Columns[6].Header = Globals.HeaderDates[5];


                TotalOverallSalesDataGrid.Columns[1].Header = Globals.HeaderDates[0];
                TotalOverallSalesDataGrid.Columns[2].Header = Globals.HeaderDates[1];
                TotalOverallSalesDataGrid.Columns[3].Header = Globals.HeaderDates[2];
                TotalOverallSalesDataGrid.Columns[4].Header = Globals.HeaderDates[3];
                TotalOverallSalesDataGrid.Columns[5].Header = Globals.HeaderDates[4];
                TotalOverallSalesDataGrid.Columns[6].Header = Globals.HeaderDates[5];


            });
        }

        private void PopulateThirdPartySalesReport(DateTime startDate)
        {
            List<GenericSalesReportLine> thirdPartySalesReportLines = new List<GenericSalesReportLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_THIRDPARTY_SALES_REPORT", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", startDate);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            GenericSalesReportLine thirdPartySalesReportLine = new GenericSalesReportLine();


                            thirdPartySalesReportLine.Catagory = reader.GetString(0);
                            thirdPartySalesReportLine.M1Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                            thirdPartySalesReportLine.M2Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                            thirdPartySalesReportLine.M3Value = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            thirdPartySalesReportLine.M4Value = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                            thirdPartySalesReportLine.M5Value = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                            thirdPartySalesReportLine.M6Value = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));

                            thirdPartySalesReportLines.Add(thirdPartySalesReportLine);

                        }
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    ThirdPartySalesDataGrid.ItemsSource = thirdPartySalesReportLines;
                });
            }
        }

        private void PopulateSopSummaryReport(DateTime startDate)
        {
            List<SopSummaryLine> sopSummaryLines = new List<SopSummaryLine>();

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
               // using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_SUMMARY_VALUES", connection))
                using (SqlCommand command = new SqlCommand("select * from Tracker.dbo.Reports_SOP_SUMMARY_VALUES", connection))
                {
                    command.CommandType = CommandType.Text;
                 //   command.Parameters.AddWithValue("@StartMonth", startDate);
                    command.CommandTimeout = 350;
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        int i;
                        for(i=0;i<=dt.Rows.Count - 1; i++)
                        {
                            SopSummaryLine sopSummaryLine = new SopSummaryLine();
                            sopSummaryLine.Catagory = Convert.ToString(dt.Rows[i]["Category"]);
                            sopSummaryLine.Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Value"] + "")) ? Convert.ToDecimal(dt.Rows[i]["Value"]) : 0;// reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                            sopSummaryLines.Add(sopSummaryLine);
                        }


                    }
                    else { return; }

                    //using (SqlDataReader reader = command.ExecuteReader())
                    //{

                    //    while (reader.Read())
                    //    {
                    //        SopSummaryLine sopSummaryLine = new SopSummaryLine();


                    //        sopSummaryLine.Catagory = reader.GetString(0);
                    //        sopSummaryLine.Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));

                    //        sopSummaryLines.Add(sopSummaryLine);

                    //    }
                    //}
                }
                this.Dispatcher.Invoke(() =>
                {
                    SopSummaryDataGrid.ItemsSource = sopSummaryLines;
                });
            }
        }

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    //MainMenuWindow mainMenuWindow = new MainMenuWindow();
        //    //mainMenuWindow.Show();
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DateTime dtNow = DateTime.Now;

            for (int i = -6; i <= 18; i++)
            {
                MonthPickComboBox.Items.Add(dtNow.AddMonths(i).ToString("MMM yy"));
            }
            MonthPickComboBox.SelectedIndex = 6;

            RefreshSopReportDataGrid();
            DateTime startDate = DateTime.Parse(MonthPickComboBox.SelectedValue.ToString());
            PopulateCustomManufactureReport(startDate);
            PopulateAccrualsDataGrid();


        }
        

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(((DataGrid)sender as DataGrid).CurrentColumn == null)
            {
                return;
            }
            if (((DataGrid)sender as DataGrid).CurrentColumn.DisplayIndex == 0)
            {
                return;
            }
            //string categoryStr = (((DataGrid)sender as DataGrid).SelectedItem as SopSummaryLine).Catagory;
            //GenericSalesReportLine
            SopReportDetailedWindow sopReportDetailedWindow = null;
            var catagory = string.Empty;
            var clickedDate = DateTime.Parse(Globals.HeaderDates[((DataGrid)sender as DataGrid).CurrentColumn.DisplayIndex - 1]);
            //var TESTDATE = ((DataGrid)sender as DataGrid).CurrentColumn.DisplayIndex;

            switch (((DataGrid)sender as DataGrid).Name)
            {
                case "TotalOverallSalesDataGrid":
                    catagory = (((DataGrid)sender as DataGrid).SelectedItem as GenericSalesReportLine).Catagory;
                    if (catagory.Contains("BUDGET"))
                    {
                        return;
                    }
                    sopReportDetailedWindow = new SopReportDetailedWindow("TotalOverallSalesDataGrid", clickedDate, catagory);
                    break;
                case "ManufactureAndMiscSalesDataGrid":
                    catagory = (((DataGrid)sender as DataGrid).SelectedItem as GenericSalesReportLine).Catagory;
                    if (catagory.Contains("BUDGET"))
                    {
                        return;
                    }
                    sopReportDetailedWindow = new SopReportDetailedWindow("ManufactureAndMiscSalesDataGrid", clickedDate, catagory);
                    break;
                case "IronmongerySalesDataGrid":
                    catagory = (((DataGrid)sender as DataGrid).SelectedItem as GenericSalesReportLine).Catagory;
                    if (catagory.Contains("BUDGET"))
                    {
                        return;
                    }
                    if (catagory == "Secured & Closing Out Risk")
                    {
                        //ManualWipWindow manualWipWindow = new ManualWipWindow();
                        //manualWipWindow.Owner = this;
                        //manualWipWindow.ShowDialog();
                        //if (manualWipWindow.DialogResult == true)
                        //{
                        //    RefreshMainReportDataGrid();
                        //}
                        MessageBox.Show(this, "These figure are managed via the \"Production - IRON WIP.xlsm\" file.");

                        catagory = (((DataGrid)sender as DataGrid).SelectedItem as GenericSalesReportLine).Catagory;
                        sopReportDetailedWindow = new SopReportDetailedWindow("IronmongerySalesDataGrid", clickedDate, catagory);
                    }
                    else
                    {

                        catagory = (((DataGrid)sender as DataGrid).SelectedItem as GenericSalesReportLine).Catagory;
                        sopReportDetailedWindow = new SopReportDetailedWindow("IronmongerySalesDataGrid", clickedDate, catagory);

                    }


                    break;
                case "ThirdPartySalesDataGrid":
                    catagory = (((DataGrid)sender as DataGrid).SelectedItem as GenericSalesReportLine).Catagory;
                    if (catagory.Contains("BUDGET"))
                    {
                        return;
                    }
                    sopReportDetailedWindow = new SopReportDetailedWindow("ThirdPartySalesDataGrid", clickedDate, catagory);
                    break;
                case "MiscSalesDataGrid":
                    catagory = (((DataGrid)sender as DataGrid).SelectedItem as GenericSalesReportLine).Catagory;
                    if (catagory.Contains("BUDGET"))
                    {
                        return;
                    }
                    sopReportDetailedWindow = new SopReportDetailedWindow("MiscSalesDataGrid", clickedDate, catagory);
                    break;
                case "SopSummaryDataGrid":
                    catagory = (((DataGrid)sender as DataGrid).SelectedItem as SopSummaryLine).Catagory;
                    if (catagory.Contains("BUDGET"))
                    {
                        return;
                    }
                    if (catagory.Contains("Accruals"))
                    {
                        //ManageAccrualsWindow manageAccrualsWindow = new ManageAccrualsWindow(catagory);
                        //manageAccrualsWindow.Owner = this;
                        //manageAccrualsWindow.ShowDialog();
                        //if (manageAccrualsWindow.DialogResult == true)
                        //{
                        //    RefreshSopReportDataGrid();
                        //}
                        MessageBox.Show(this, "Accruals are now managed by the \"Manage Accruals\" Tab");
                    }
                    else
                    {
                        catagory = (((DataGrid)sender as DataGrid).SelectedItem as SopSummaryLine).Catagory;
                        sopReportDetailedWindow = new SopReportDetailedWindow("SopSummaryDataGrid", clickedDate, catagory);
                    }



                    break;
            }

            if (sopReportDetailedWindow != null)
            {
                sopReportDetailedWindow.Owner = this;
                sopReportDetailedWindow.ShowDialog();
                if (sopReportDetailedWindow.DialogResult == true)
                {
                    //RefreshMainReportDataGrid();
                }
            }

        }

        //private void ManageAccruals_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    ManageAccrualsWindow manageAccrualsWindow = new ManageAccrualsWindow();
        //    manageAccrualsWindow.Owner = this;
        //    manageAccrualsWindow.ShowDialog();
        //    if (manageAccrualsWindow.DialogResult == true)
        //    {
        //        RefreshSopReportDataGrid();
        //    }
        //}

        public void RefreshManufactureReportDataGrid()
        {

            DateTime startDate = GetStartDate();

            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MainTabControl.Visibility = Visibility.Hidden;
                    loadingGif.Visibility = Visibility.Visible;
                });


                PopulateCustomManufactureReport(startDate);
            };



            bgw.RunWorkerCompleted += (_, __) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    loadingGif.Visibility = Visibility.Hidden;
                    MainTabControl.Visibility = Visibility.Visible;
                });
            };
            bgw.RunWorkerAsync();

        }

        private DateTime GetStartDate()
        {
            DateTime dt = DateTime.MinValue;
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUFACTURE_DETAIL_START_DATE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
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

        private void PopulateCustomManufactureReport(DateTime startDate)
        {


            PopulateManufactureHeaderDates(startDate);
            PopulateSummaryFigures(startDate);
            PopulateDetailFigures(startDate);
            PopulateCurrentMonthManufacturedTextBox();
            PopulateOverallMonthManufacturedTextBox();
        }

        private void PopulateOverallMonthManufacturedTextBox()
        {
            decimal overallMonthManufacturedValue = 0;

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
               // using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUFACTURE_CURRENT_MONTH_OVERALL_VALUE", connection))
                using (SqlCommand command = new SqlCommand("select convert(decimal,[Value]) from Tracker.dbo.Reports_CURRENT_MONTH_FACTORY_OVERALL_VALUE where ValueType='OVERALL_VALUE'", connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            overallMonthManufacturedValue = reader.GetDecimal(0);

                        }
                    }
                }

            }

            this.Dispatcher.Invoke(() =>
            {
                OverallManfFigure.Content = string.Format("£{0:N0}", overallMonthManufacturedValue);
            });
        }

        private void PopulateCurrentMonthManufacturedTextBox()
        {
            decimal currentMonthManufacturedValue = 0;

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                //using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUFACTURE_CURRENT_MONTH_FACTORY_VALUE", connection))
                using (SqlCommand command = new SqlCommand("select convert(decimal,[Value]) from Tracker.dbo.Reports_CURRENT_MONTH_FACTORY_OVERALL_VALUE where ValueType='FACTORY_VALUE'", connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            currentMonthManufacturedValue = reader.GetDecimal(0);

                        }
                    }
                }

            }

            this.Dispatcher.Invoke(() =>
            {
                FactoryManfFigure.Content = string.Format("£{0:N0}", currentMonthManufacturedValue);
            });
        }



        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    MainMenuWindow mainMenuWindow = new MainMenuWindow();
        //    mainMenuWindow.Show();
        //}


        private void PopulateDetailFigures(DateTime startDate)
        {
            List<ManufactureDetailLine> manufactureDetailLines = new List<ManufactureDetailLine>();


            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                // using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUFACTURE_DETAIL_VALUES", connection))
                using (SqlCommand command = new SqlCommand("select * from Tracker.dbo.Reports_MANUFACTURE_DETAIL_VALUES", connection))
                {
                    command.CommandType = CommandType.Text;
                    //  command.Parameters.AddWithValue("@StartMonth", startDate);
                    //  connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        int i;
                        for (i = 0; i <= dt.Rows.Count - 1; i++)
                        {
                            ManufactureDetailLine manufactureDetailLine = new ManufactureDetailLine();
                            manufactureDetailLine.Contract = Convert.ToString(dt.Rows[i]["Category"]);
                            manufactureDetailLine.M1cValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1c"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M1c"]) : 0;
                            manufactureDetailLine.M1oValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1o"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M1o"]) : 0;
                            manufactureDetailLine.M2cValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M2c"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M2c"]) : 0;
                            manufactureDetailLine.M2oValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M2o"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M2o"]) : 0;
                            manufactureDetailLine.M3cValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M3c"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M3c"]) : 0;
                            manufactureDetailLine.M3oValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M3o"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M3o"]) : 0;
                            manufactureDetailLine.M4cValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M4c"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M4c"]) : 0;
                            manufactureDetailLine.M4oValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M4o"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M4o"]) : 0;
                            manufactureDetailLine.M5cValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M5c"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M5c"]) : 0;
                            manufactureDetailLine.M5oValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M5o"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M5o"]) : 0;
                            manufactureDetailLine.M6cValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M6c"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M6c"]) : 0;
                            manufactureDetailLine.M6oValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M6o"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M6o"]) : 0;
                            manufactureDetailLine.M7cValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M7c"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M7c"]) : 0;
                            manufactureDetailLine.M7oValue = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M7o"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M7o"]) : 0;

                            manufactureDetailLines.Add(manufactureDetailLine);
                        }
                    }
                    else { return; }

                    //      using (SqlDataReader reader = command.ExecuteReader())
                    //{

                    //    while (reader.Read())
                    //    {
                    //        ManufactureDetailLine manufactureDetailLine = new ManufactureDetailLine();
                    //        manufactureDetailLine.Contract = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    //        manufactureDetailLine.M1cValue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    //        manufactureDetailLine.M1oValue = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                    //        manufactureDetailLine.M2cValue = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                    //        manufactureDetailLine.M2oValue = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                    //        manufactureDetailLine.M3cValue = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                    //        manufactureDetailLine.M3oValue = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                    //        manufactureDetailLine.M4cValue = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                    //        manufactureDetailLine.M4oValue = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                    //        manufactureDetailLine.M5cValue = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                    //        manufactureDetailLine.M5oValue = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                    //        manufactureDetailLine.M6cValue = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                    //        manufactureDetailLine.M6oValue = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
                    //        manufactureDetailLine.M7cValue = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                    //        manufactureDetailLine.M7oValue = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);

                    //        manufactureDetailLines.Add(manufactureDetailLine);

                    //    }
                    //}
                }

            }

            this.Dispatcher.Invoke(() =>
            {
                ManufacturingReportDetailDataGrid.ItemsSource = manufactureDetailLines;
            });
        }

        private void PopulateSummaryFigures(DateTime startDate)
        {
            List<ManufactureSummaryLine> manufactureSummaryLines = new List<ManufactureSummaryLine>();


            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                //using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUFACTURE_SUMMARISED_VALUES", connection))
                using (SqlCommand command = new SqlCommand("select * from Tracker.dbo.Reports_MANUFACTURE_SUMMARISED_VALUES", connection))
                {
                    command.CommandType = CommandType.Text;
                    //command.Parameters.AddWithValue("@StartMonth", startDate);
                    // connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        int i;
                        for (i = 0; i <= dt.Rows.Count - 1; i++)
                        {
                            ManufactureSummaryLine manufactureSummaryLine = new ManufactureSummaryLine();
                            manufactureSummaryLine.Catagory = Convert.ToString(dt.Rows[i]["Category"]);
                            manufactureSummaryLine.M1Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M1"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M1"]) : 0;
                            manufactureSummaryLine.M2Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M2"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M2"]) : 0;
                            manufactureSummaryLine.M3Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M3"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M3"]) : 0;
                            manufactureSummaryLine.M4Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M4"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M4"]) : 0;
                            manufactureSummaryLine.M5Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M5"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M5"]) : 0;
                            manufactureSummaryLine.M6Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M6"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M6"]) : 0;
                            manufactureSummaryLine.M7Value = !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["M7"] + "")) ? Convert.ToDecimal(dt.Rows[i]["M7"]) : 0;
                            manufactureSummaryLines.Add(manufactureSummaryLine);
                        }
                    }
                    else { return; }

                    //      using (SqlDataReader reader = command.ExecuteReader())
                    //{

                    //    while (reader.Read())
                    //    {
                    //        ManufactureSummaryLine manufactureSummaryLine = new ManufactureSummaryLine();
                    //        manufactureSummaryLine.Catagory = reader.GetString(0);
                    //        manufactureSummaryLine.M1Value = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    //        manufactureSummaryLine.M2Value = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                    //        manufactureSummaryLine.M3Value = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                    //        manufactureSummaryLine.M4Value = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                    //        manufactureSummaryLine.M5Value = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                    //        manufactureSummaryLine.M6Value = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                    //        manufactureSummaryLine.M7Value = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);

                    //        manufactureSummaryLines.Add(manufactureSummaryLine);

                    //    }
                    //}
                }

            }

            this.Dispatcher.Invoke(() =>
            {
                ManufacturingReportSummaryDataGrid.ItemsSource = manufactureSummaryLines;
            });
        }

        private void PopulateManufactureHeaderDates(DateTime startDate)
        {
            List<string> headerDates = new List<string>();


            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUFACTURE_TABLE_HEADER_DATES", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", startDate);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            headerDates.Add(reader.GetString(0));

                        }
                    }
                }

            }

            this.Dispatcher.Invoke(() =>
            {

                HeaderMonth1TextBox.Text = headerDates[0];
                HeaderMonth2TextBox.Text = headerDates[1];
                HeaderMonth3TextBox.Text = headerDates[2];
                HeaderMonth4TextBox.Text = headerDates[3];
                HeaderMonth5TextBox.Text = headerDates[4];
                HeaderMonth6TextBox.Text = headerDates[5];
                HeaderMonth7TextBox.Text = headerDates[6];

            });
        }


        private void PopulatManufactureStartWeekNr()
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_CURRENT_WEEK_NUMBER", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                labelCurrentWeekNr.Content = string.Format("Current week: {0}", reader.GetString(0));
                            });

                        }
                    }
                }

            }
            
        }


        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshActiveReport();
        }
        

        internal void RefreshActiveReport()
        {
            switch ((MainTabControl.SelectedItem as TabItem).Header.ToString())
            {
                case "SOP Report":
                    RefreshSopReportDataGrid();
                    break;
                case "Manufacture Report":
                    RefreshManufactureReportDataGrid();
                    break;
                default:
                    //MessageBox.Show(this, "Unexcpected error... Aborting operation");
                    break;

            }
        }





        private void PopulateAccrualsDataGrid()
        {
            List<string> categories = new List<string>();

            List<AccrualsLine> accrualsLines = new List<AccrualsLine>();
            //MessageBox.Show(this, "TODO: PopulateMainDataGrid");
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_ACCRUALS", connection))

                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 300;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            AccrualsLine accrualsLine = new AccrualsLine();


                            accrualsLine.Id = reader.GetInt32(0);
                            accrualsLine.ContractName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            accrualsLine.JobNr = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            accrualsLine.TrackerValue = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            accrualsLine.Value2099 = reader.IsDBNull(4) ? 0 : Convert.ToDecimal(reader.GetValue(4));
                            accrualsLine.InvoicedValue = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5));
                            accrualsLine.StatusPerTracker = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                            accrualsLine.Date = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7);
                            accrualsLine.DbVal = reader.IsDBNull(8) ? 0 : Convert.ToDecimal(reader.GetValue(8));
                            accrualsLine.DocNr = reader.GetString(9);
                            accrualsLine.Category = reader.GetString(10);
                            if (!categories.Contains(accrualsLine.Category))
                            {
                                categories.Add(accrualsLine.Category);
                            }
                            accrualsLines.Add(accrualsLine);

                        }
                    }
                }
            }

            AccrualsDataGrid.ItemsSource = accrualsLines;
            StatusComboBox.Items.Clear();
            StatusComboBox.Items.Add("ALL");
            foreach (string category in categories)
            {
                StatusComboBox.Items.Add(category);
            }
            StatusComboBox.SelectedIndex = 0;
        }


        private void EditAccrual_Button_Click(object sender, RoutedEventArgs e)
        {
            AccrualsLine selectedAccrualLine = AccrualsDataGrid.SelectedItem as AccrualsLine;

            EditAccrualsWindow editAccrualsWindow = new EditAccrualsWindow(selectedAccrualLine);
            editAccrualsWindow.Owner = this;
            editAccrualsWindow.ShowDialog();
            if (editAccrualsWindow.DialogResult == true)
            {
                PopulateAccrualsDataGrid();
            }


        }

        private void DeleteAccrual_Button_Click(object sender, RoutedEventArgs e)
        {
            AccrualsLine selectedAccrualLine = AccrualsDataGrid.SelectedItem as AccrualsLine;



            String msg = string.Format("Are you sure that you want to delete {0}, with tracker value of {1:c} ", selectedAccrualLine.JobNr, selectedAccrualLine.TrackerValue);
            MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                //MessageBox.Show(this, "Removed");
                RemoveAccrual(selectedAccrualLine);
                PopulateAccrualsDataGrid();
            }

        }

        private void RemoveAccrual(AccrualsLine selectedAccrualLine)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.DELETE_ACCRUAL_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", selectedAccrualLine.Id);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private void AddAccrual_Button_Click(object sender, RoutedEventArgs e)
        {
            AddAccrualsWindow addAccrualsWindow = new AddAccrualsWindow();
            addAccrualsWindow.Owner = this;
            addAccrualsWindow.ShowDialog();
            if (addAccrualsWindow.DialogResult == true)
            {
                PopulateAccrualsDataGrid();
            }

        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Decimal filteredTotal = 0;

            string filterText = (sender as ComboBox).SelectedItem as string;
            ICollectionView view = CollectionViewSource.GetDefaultView(AccrualsDataGrid.ItemsSource);
            Predicate<object> yourCostumFilter = null;
            //var query1 = new Predicate<object>(x => ((AccrualsLine)x).Category.ToLower().Contains(filterText));


            if (filterText == "ALL")
            {
                yourCostumFilter = new Predicate<object>(item => true);
            }
            else
            {
                yourCostumFilter = new Predicate<object>(item =>
                               ((AccrualsLine)item).Category == filterText);
            }

            view.Filter = yourCostumFilter;
            AccrualsDataGrid.ItemsSource = view;

            List<AccrualsLine> lines = new List<AccrualsLine>();
            try
            {
                lines = ((List<AccrualsLine>)AccrualsDataGrid.ItemsSource) as List<AccrualsLine>;

            }
            catch
            {
                lines = CollectionViewSource.GetDefaultView(AccrualsDataGrid.ItemsSource).Cast<AccrualsLine>().ToList();
            }

            foreach (AccrualsLine line in lines)
            {
                filteredTotal =
                    filteredTotal +
                    line.TrackerValue +
                    line.InvoicedValue +
                    line.Value2099;
            }

            FilteredTotalTextBox.Text = string.Format(("{0:C}"), filteredTotal);

        }

        System.Windows.Threading.DispatcherTimer _selectingMainTrackerDataGridTimer;
        private void MainDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            foreach (AccrualsLine line in AccrualsDataGrid.SelectedItems)
            {
                highlightedTotal =
                    highlightedTotal +
                    line.TrackerValue +
                    line.InvoicedValue +
                    line.Value2099;
            }

            HighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }

        private void AccrualsRefresh_Button_Click(object sender, RoutedEventArgs e)
        {
            PopulateAccrualsDataGrid();
        }
    }
}