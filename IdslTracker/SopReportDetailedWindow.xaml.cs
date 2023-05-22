using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System.Diagnostics;

namespace IdslTracker
{
    /// <summary>
    /// Interaction logic for SopReportDetailedWindow.xaml
    /// </summary>
    public partial class SopReportDetailedWindow : Window
    {
        private string mDataGridName;
        private string mCatagory;
        private DateTime mStartDate;
        public SopReportDetailedWindow(string dataGridName, DateTime startDate, string catagory)
        {
            InitializeComponent();
            mDataGridName = dataGridName;
            mStartDate = startDate;
            mCatagory = catagory;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //PopulateMonthComboBox();
            //MonthComboBox.SelectedIndex = 0;

            ////MonthComboBox.DropDownClosed += MonthComboBox_SelectionChanged;
            //MonthComboBox.DropDownClosed += MonthComboBox_DropDownClosed;

            //MonthComboBox.SelectionChanged -= EventHandler < SelectedIndexChangedEventArgs > SomeEventHandler;
            //MonthComboBox.SelectedIndex = 0;
            //MonthComboBox.SelectedIndexChanged += EventHandler < SelectedIndexChangedEventArgs > SomeEventHandler;

            //TypeLabel.Visibility = Visibility.Hidden;
            //TypeComboBox.Visibility = Visibility.Hidden;

            switch (mDataGridName)
            {
                case "TotalOverallSalesDataGrid":
                    PopulateDataGrids("GET_SOP_OVERALL_SALES_REPORT",1 , mCatagory);
                    this.Title = string.Format("Sop Report Window | OVERALL | {0} | {1:MMM-yy}", mCatagory, mStartDate);
                    break;
                case "ManufactureAndMiscSalesDataGrid":
                    PopulateDataGrids("GET_SOP_MANUFACTURE_AND_MISC_SALES_REPORT", 1, mCatagory);
                    this.Title = string.Format("Sop Report Window | MANUFACTURE | {0} | {1:MMM-yy}", mCatagory, mStartDate);
                    break;
                case "IronmongerySalesDataGrid":
                    PopulateDataGrids("GET_SOP_IRONMONGERY_SALES_REPORT", 1, mCatagory);
                    this.Title = string.Format("Sop Report Window | IRONMONGERY | {0} | {1:MMM-yy}", mCatagory, mStartDate);
                    break;
                case "ThirdPartySalesDataGrid":
                    PopulateDataGrids("GET_SOP_THIRDPARTY_SALES_REPORT", 1, mCatagory);
                    this.Title = string.Format("Sop Report Window | THIRD PARTY | {0} | {1:MMM-yy}", mCatagory, mStartDate);
                    break;
                case "MiscSalesDataGrid":
                    PopulateDataGrids("GET_SOP_MISC_SALES_REPORT", 1, mCatagory);
                    this.Title = string.Format("Sop Report Window | MISC | {0} | {1:MMM-yy}", mCatagory, mStartDate);
                    break;
                case "SopSummaryDataGrid":
                    PopulateDataGrids("GET_SOP_SUMMARY_VALUES", 1, mCatagory);
                    this.Title = string.Format("Sop Report Window | SUMMARY | {0} | {1:MMM-yy}", mCatagory, mStartDate);
                    break;
                default:
                    MessageBox.Show(this, "Unexpected Error - Speak to Systems Development");
                    break;

            }
            PopulateTotalFigureBasedOnSummary();
            //FilterDataGrid();
        }

        //private void FilterDataGrid()
        //{
        //    //DateTime comboboxDate = DateTime.Parse(MonthComboBox.Text);
        //    //var firstDayOfMonth = new DateTime(comboboxDate.Year, comboboxDate.Month, 1);
        //    //var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        //    //List<SopReportDetailedLine> sopReportDetailedLines = MainDataGrid.ItemsSource as List<SopReportDetailedLine>;
        //    //foreach (SopReportDetailedLine sopReportDetailedLine in sopReportDetailedLines)
        //    //{
        //    //    if (sopReportDetailedLine.Date >= firstDayOfMonth && sopReportDetailedLine.Date <= lastDayOfMonth)
        //    //    {
        //    //        sopReportDetailedLine.FilteredOut = false;
        //    //    }
        //    //    else
        //    //    {
        //    //        sopReportDetailedLine.FilteredOut = true;
        //    //    }
        //    //}
        //    MainDataGrid.Items.Refresh();


        //    if (MainDataGrid.ItemsSource != null)
        //    {
        //        ICollectionView view = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource);
        //        Predicate<object> yourCostumFilter = null;

        //        //yourCostumFilter = new Predicate<object>(item =>
        //        //               ((SopReportDetailedLine)item).Date >= firstDayOfMonth &&
        //        //               ((SopReportDetailedLine)item).Date <= lastDayOfMonth
        //        //               );
        //        view.Filter = yourCostumFilter;
        //        MainDataGrid.ItemsSource = view;
        //    }
        //    //PopulateTotalFigure();
        //}

        private void PopulateTotalFigureBasedOnSummary()
        {
            List<SopReportSummaryLine> lines = new List<SopReportSummaryLine>();
            Decimal total = 0;
            try
            {
                lines = ((List<SopReportSummaryLine>)SummaryDataGrid.ItemsSource) as List<SopReportSummaryLine>;

            }
            catch
            {
                lines = CollectionViewSource.GetDefaultView(SummaryDataGrid.ItemsSource).Cast<SopReportSummaryLine>().ToList();
            }
            
            foreach (SopReportSummaryLine line in lines)
            {
                //if(sopReportDetailedLine.FilteredOut == false)
                //{
                total = total + line.Sales;
                //}
            }


            AllTotalTextBox.Text = string.Format(("{0:C}"), total);
        }

        //private void PopulateMonthComboBox()
        //{
        //    using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
        //    {
        //        using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_SOP_TABLE_HEADER_DATES", connection))
        //        {
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.Parameters.AddWithValue("@StartMonth", mStartDate);
        //            connection.Open();

        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {

        //                while (reader.Read())
        //                {
        //                    MonthComboBox.Items.Add(reader.GetString(0));

        //                }
        //            }
        //        }

        //    }
        //}

        private void PopulateDataGrids(string sp, int reportId, string catagory)
        {
            List<SopReportDetailedLine> sopReportDetailedLines = new List<SopReportDetailedLine>();
            //Decimal total = 0;

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand(string.Format("Tracker.dbo.{0}", sp), connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", mStartDate);
                    command.Parameters.AddWithValue("@ReportId", reportId);
                    command.Parameters.AddWithValue("@Category", catagory);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SopReportDetailedLine sopReportDetailedLine = new SopReportDetailedLine();

                            sopReportDetailedLine.Source = reader.GetString(0);
                            sopReportDetailedLine.Department = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            sopReportDetailedLine.Sales = Convert.ToDecimal(reader.GetValue(2));
                            sopReportDetailedLine.Date = reader.GetDateTime(3);
                            sopReportDetailedLine.Type = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            sopReportDetailedLine.Job = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            sopReportDetailedLine.ContractName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                            sopReportDetailedLine.WipStatus = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
                            sopReportDetailedLine.FtbIndexId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
                            sopReportDetailedLine.Scheduler = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);

                            //total = total + sopReportDetailedLine.Sales;
                            sopReportDetailedLines.Add(sopReportDetailedLine);

                        }
                    }
                    connection.Close();
                }
                MainDataGrid.ItemsSource = sopReportDetailedLines;



                List<SopReportSummaryLine> sopReportSumamryLines = new List<SopReportSummaryLine>();
                using (SqlCommand command = new SqlCommand(string.Format("Tracker.dbo.{0}", sp), connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartMonth", mStartDate);
                    command.Parameters.AddWithValue("@ReportId", 2);
                    command.Parameters.AddWithValue("@Category", catagory);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SopReportSummaryLine sopReportSummaryLine = new SopReportSummaryLine();

                            sopReportSummaryLine.ContractName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            sopReportSummaryLine.Sales = Convert.ToDecimal(reader.GetValue(1));
                            sopReportSummaryLine.Type = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);

                            sopReportSumamryLines.Add(sopReportSummaryLine);

                        }
                    }
                }

                List<string> unequeListOfTypes = new List<string>();
                unequeListOfTypes.Add("ALL");
                foreach(SopReportSummaryLine line in sopReportSumamryLines)
                {
                    if (!unequeListOfTypes.Contains(line.Type))
                    {
                        unequeListOfTypes.Add(line.Type);
                    }
                }
                TypeComboBox.ItemsSource = unequeListOfTypes;
                TypeComboBox.SelectedIndex = 0;
                SummaryDataGrid.ItemsSource = sopReportSumamryLines;
            }
        }


        System.Windows.Threading.DispatcherTimer _selectingMainDataGridTimer;

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_selectingMainDataGridTimer == null)
            {
                _selectingMainDataGridTimer = new System.Windows.Threading.DispatcherTimer();
                _selectingMainDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

                _selectingMainDataGridTimer.Tick += new EventHandler(this.handleSelectingTimerTimeout);
            }
            _selectingMainDataGridTimer.Stop();
            //_selectingTimer.Tag = (sender as TextBox).Text; 
            _selectingMainDataGridTimer.Start();
        }

        private void handleSelectingTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            if (timer == null)
            {
                return;
            }

            decimal highlightedTotal = 0;

            //var dg = (DataGrid)sender as DataGrid;
            var tabHeader = ((MainTabControl.SelectedItem as TabItem).Header.ToString());
            if (tabHeader == "Detail")
            {
                foreach (SopReportDetailedLine sopReportDetailedLine in MainDataGrid.SelectedItems)
                {
                    highlightedTotal = highlightedTotal + sopReportDetailedLine.Sales;
                }
            }
            else if (tabHeader == "Summary")
            {
                foreach (SopReportSummaryLine sopReportSummaryLine in SummaryDataGrid.SelectedItems)
                {
                    highlightedTotal = highlightedTotal + sopReportSummaryLine.Sales;
                }
            }

            
            HighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



            timer.Stop();
        }


        //System.Windows.Threading.DispatcherTimer _selectingSummaryDataGridTimer;

        //private void SummaryDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (_selectingSummaryDataGridTimer == null)
        //    {
        //        _selectingSummaryDataGridTimer = new System.Windows.Threading.DispatcherTimer();
        //        _selectingSummaryDataGridTimer.Interval = TimeSpan.FromMilliseconds(100);

        //        _selectingSummaryDataGridTimer.Tick += new EventHandler(this.SummaryDataGrid_handleSelectingTimerTimeout);
        //    }
        //    _selectingSummaryDataGridTimer.Stop();
        //    //_selectingTimer.Tag = (sender as TextBox).Text; 
        //    _selectingSummaryDataGridTimer.Start();
        //}

        //private void SummaryDataGrid_handleSelectingTimerTimeout(object sender, EventArgs e)
        //{
        //    var timer = sender as System.Windows.Threading.DispatcherTimer;
        //    if (timer == null)
        //    {
        //        return;
        //    }

        //    decimal highlightedTotal = 0;
        //    foreach (SopReportSummaryLine sopReportSummaryLine in SummaryDataGrid.SelectedItems)
        //    {
        //        highlightedTotal = highlightedTotal + sopReportSummaryLine.Sales;
        //    }
        //    HighlightedTotalTextBox.Text = string.Format(("{0:C}"), highlightedTotal);



        //    timer.Stop();
        //}

        //private void MonthComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (MonthComboBox.SelectedIndex != -1 && MonthComboBox.SelectedValue.ToString() != "")
        //    {
        //        FilterDataGrid();
        //    }

        //}


        //private void MonthComboBox_DropDownClosed(object sender, EventArgs e)
        //{
        //    if (MonthComboBox.SelectedIndex != -1 && MonthComboBox.SelectedValue.ToString() != "")
        //    {
        //        FilterDataGrid();
        //    }
        //}

        private void Window_Closing(object sender, EventArgs e)
        {
            this.DialogResult = true;
            //this.Close();
        }
        private string filteredContractName;
        private void SummaryDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            filteredContractName = (((DataGrid)sender as DataGrid).SelectedItem as SopReportSummaryLine).ContractName;
            //MainTabControl.SelectedIndex = 1;
            Dispatcher.BeginInvoke((Action)(() => MainTabControl.SelectedIndex = 1));
            FilterDetailedDataGrid();
        }

        private void FilterDetailedDataGrid()
        {

            MainDataGrid.Items.Refresh();


            if (MainDataGrid.ItemsSource != null)
            {
                string cbText = TypeComboBox.SelectedItem as string;
                ICollectionView view = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource);
                Predicate<object> yourCostumFilter = null;

                if (filteredContractName != "None")
                {
                    if (cbText == "ALL")
                    {
                        yourCostumFilter = new Predicate<object>(item =>
                                   ((SopReportDetailedLine)item).ContractName == filteredContractName);
                    }
                    else
                    {
                        yourCostumFilter = new Predicate<object>(item =>
                            ((SopReportDetailedLine)item).Type == cbText && ((SopReportDetailedLine)item).ContractName == filteredContractName);
                    }

                    FitlerLabel.Content = string.Format("Showing data for contract: {0} only", filteredContractName);
                    ClearFilterBtn.Visibility = Visibility.Visible;

                }
                else
                {
                    if (cbText == "ALL")
                    {
                        yourCostumFilter = new Predicate<object>(item => true);
                    }
                    else
                    {
                        yourCostumFilter = new Predicate<object>(item =>
                            ((SopReportDetailedLine)item).Type == cbText);
                    }
                }


                view.Filter = yourCostumFilter;
                MainDataGrid.ItemsSource = view;

                PopulateTotalFigureBasedOnDetail();
            }

        }

        private void PopulateTotalFigureBasedOnDetail()
        {
            Decimal total = 0;

            List<SopReportDetailedLine> lines = new List<SopReportDetailedLine>();

            try
            {
                lines = ((List<SopReportDetailedLine>)MainDataGrid.ItemsSource) as List<SopReportDetailedLine>;

            }
            catch
            {
                lines = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource).Cast<SopReportDetailedLine>().ToList();
            }
            //ICollectionView view = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource);
            foreach (SopReportDetailedLine sopReportDetailedLine in lines)
            {
                //if(sopReportDetailedLine.FilteredOut == false)
                //{
                total = total + sopReportDetailedLine.Sales;
                //}
            }


            AllTotalTextBox.Text = string.Format(("{0:C}"), total);
        }

        private void ClearFilter_Button_Click(object sender, RoutedEventArgs e)
        {
            filteredContractName = "None";
            FilterDetailedDataGrid();
            FitlerLabel.Content = string.Empty;
            ClearFilterBtn.Visibility = Visibility.Hidden;
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (((TabItem)(((TabControl)sender as TabControl).SelectedItem) as TabItem).Header.ToString() == "Summary")
                {
                    ClearFilter_Button_Click(null, null);
                    TypeComboBox_SelectionChanged(null, null);
                    //TypeLabel.Visibility = Visibility.Hidden;
                    //TypeComboBox.Visibility = Visibility.Hidden;
                }
                else
                {
                    //filteredContractName = "None";
                    FilterDetailedDataGrid();
                    //TypeLabel.Visibility = Visibility.Visible;
                    //TypeComboBox.Visibility = Visibility.Visible;
                }
                DataGrid_SelectionChanged(null, null);
            }


        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cbText = TypeComboBox.SelectedItem as string;
            DataGrid dg = GetVisibleDataGrid();
            dg.Items.Refresh();


            if (dg.ItemsSource != null)
            {
                if (dg.Name == "MainDataGrid")
                {
                    FilterDetailedDataGrid();
                }
                else if (dg.Name == "SummaryDataGrid")
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(dg.ItemsSource);
                    Predicate<object> yourCostumFilter = null;
                    if (cbText == "ALL")
                    {
                        yourCostumFilter = new Predicate<object>(item => true);
                    }
                    else
                    {
                        yourCostumFilter = new Predicate<object>(item =>
                            ((SopReportSummaryLine)item).Type == cbText);
                    }

                    view.Filter = yourCostumFilter;
                    dg.ItemsSource = view;

                    PopulateTotalFigureBasedOnSummary();

                }
            }
        }

        private DataGrid GetVisibleDataGrid()
        {
            DataGrid dg = null;

            switch ((MainTabControl.SelectedItem as TabItem).Header.ToString())
            {
                case "Summary":

                    dg = SummaryDataGrid;
                    break;
                case "Detail":
                    //CreatePoCsv(glassDataGrid);
                    dg = MainDataGrid;
                    break;
                default:
                    //MessageBox.Show(this, "Unexcpected error... Aborting operation");
                    break;

            }

            return dg;
        }

        private void OpenFtb_Click(object sender, RoutedEventArgs e)
        {
            List<SopReportDetailedLine> selectedLines = MainDataGrid.SelectedItems.OfType<SopReportDetailedLine>().ToList();

            if (selectedLines.Count != 1)
            {
                MessageBox.Show(this, "Function only available when selecting one row only");
                return;
            }
            if (selectedLines[0].FtbIndexId == 0)
            {
                MessageBox.Show(this, "No Ftb associated with this row");
                return;
            }

            String path = null;

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("FTB.dbo.GET_PATH_FROM_FTBID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ID", selectedLines[0].FtbIndexId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            path = reader.GetString(0);
                        }
                    }
                }

            }


            if (path == null)
            {

                MessageBox.Show(this, "Quite unexpected error... aborting");
                return;
            }


            ProcessStartInfo psi = new ProcessStartInfo(path);
            psi.UseShellExecute = true;
            Process.Start(psi);

        }
    }
}
