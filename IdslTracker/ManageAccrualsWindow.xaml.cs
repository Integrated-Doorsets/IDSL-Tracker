using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ManageAccrualsWindow.xaml
    /// </summary>
    public partial class ManageAccrualsWindow : Window
    {
        private bool hasBeenEdited;
        private string mCatagory;

        public ManageAccrualsWindow(string catagory)
        {
            InitializeComponent();
            this.hasBeenEdited = false;
            this.mCatagory = catagory;
            PopulateMainDataGrid();
        }

        private void PopulateMainDataGrid()
        {
            List<string> categories = new List<string>();

            List<AccrualsLine> accrualsLines = new List<AccrualsLine>();
            //MessageBox.Show(this, "TODO: PopulateMainDataGrid");
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_ACCRUALS", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
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

            MainDataGrid.ItemsSource = accrualsLines;
            StatusComboBox.Items.Add("ALL");
            int i = 1;
            foreach(string category in categories)
            {
                StatusComboBox.Items.Add(category);
                if(category == mCatagory)
                {

                    //StatusComboBox.SelectionChanged -= new SelectionChangedEventHandler(StatusComboBox_SelectionChanged);
                    StatusComboBox.SelectedIndex = i;
                    //break;
                    //StatusComboBox.SelectionChanged += new SelectionChangedEventHandler(StatusComboBox_SelectionChanged);

                }

                i++;

            }
            if (mCatagory == "Accruals Total")
            {
                StatusComboBox.SelectedIndex = 0;
            }
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            this.DialogResult = this.hasBeenEdited;
            //this.Close();
        }

        private void EditAccrual_Button_Click(object sender, RoutedEventArgs e)
        {
            this.hasBeenEdited = true;
            AccrualsLine selectedAccrualLine = MainDataGrid.SelectedItem as AccrualsLine;

            EditAccrualsWindow editAccrualsWindow = new EditAccrualsWindow(selectedAccrualLine);
            editAccrualsWindow.Owner = this;
            editAccrualsWindow.ShowDialog();
            if (editAccrualsWindow.DialogResult == true)
            {
                PopulateMainDataGrid();
            }
                       

        }

        private void DeleteAccrual_Button_Click(object sender, RoutedEventArgs e)
        {
            this.hasBeenEdited = true;
            AccrualsLine selectedAccrualLine = MainDataGrid.SelectedItem as AccrualsLine;
            


            String msg = string.Format("Are you sure that you want to delete {0}, with tracker value of {1:c} ", selectedAccrualLine.JobNr, selectedAccrualLine.TrackerValue);
            MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                //MessageBox.Show(this, "Removed");
                RemoveAccrual(selectedAccrualLine);
                PopulateMainDataGrid();
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
            this.hasBeenEdited = true;
            AddAccrualsWindow addAccrualsWindow = new AddAccrualsWindow();
            addAccrualsWindow.Owner = this;
            addAccrualsWindow.ShowDialog();
            if (addAccrualsWindow.DialogResult == true)
            {
                PopulateMainDataGrid();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TrackerValueColumn.Header = string.Format("{0:MMMM yy} Tracker Value", DateTime.Now);

        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Decimal filteredTotal = 0;

            string filterText = (sender as ComboBox).SelectedItem as string;
            ICollectionView view = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource);
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
            MainDataGrid.ItemsSource = view;

            List<AccrualsLine> lines = new List<AccrualsLine>();
            try
            {
                lines = ((List<AccrualsLine>)MainDataGrid.ItemsSource) as List<AccrualsLine>;

            }
            catch
            {
                lines = CollectionViewSource.GetDefaultView(MainDataGrid.ItemsSource).Cast<AccrualsLine>().ToList();
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

            foreach (AccrualsLine line in MainDataGrid.SelectedItems)
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

    }
}
