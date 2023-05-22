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
    /// Interaction logic for ManualWipWindow.xaml
    /// </summary>
    public partial class ManualWipWindow : Window
    {
        public ManualWipWindow()
        {
            InitializeComponent();
            PopulateMainDataGrid();
        }

        private void PopulateMainDataGrid()
        {
            List<ManualsLine> manualsLines = new List<ManualsLine>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_MANUALS", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            ManualsLine manualsLine = new ManualsLine();


                            manualsLine.Id = reader.GetInt32(0);
                            manualsLine.JobType = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            manualsLine.ContractName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            manualsLine.ContractNumber = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            manualsLine.Scheduler = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            manualsLine.Month = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                            manualsLine.Value = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6));

                            manualsLines.Add(manualsLine);

                        }
                    }
                }
            }

            MainDataGrid.ItemsSource = manualsLines;
        }



        private void Window_Closing(object sender, EventArgs e)
        {
            this.DialogResult = true;
            //this.Close();
        }

        private void EditManuals_Button_Click(object sender, RoutedEventArgs e)
        {
            ManualsLine selectedManuslsLine = MainDataGrid.SelectedItem as ManualsLine;

            EditManualsWindow editManualsWindow = new EditManualsWindow(selectedManuslsLine);
            editManualsWindow.Owner = this;
            editManualsWindow.ShowDialog();
            if (editManualsWindow.DialogResult == true)
            {
                PopulateMainDataGrid();
            }


        }

        private void DeleteManuals_Button_Click(object sender, RoutedEventArgs e)
        {
            ManualsLine selectedManualsLine = MainDataGrid.SelectedItem as ManualsLine;



            String msg = string.Format("Are you sure that you want to value of {1:c} for {0:MMM.yyyy} ", selectedManualsLine.Month, selectedManualsLine.Value);
            MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                //MessageBox.Show(this, "Removed");
                RemoveManuals(selectedManualsLine);
                PopulateMainDataGrid();
            }

        }

        private void RemoveManuals(ManualsLine selectedAccrualLine)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.DELETE_MANUALS_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", selectedAccrualLine.Id);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private void AddManuals_Button_Click(object sender, RoutedEventArgs e)
        {
            AddManualsWindow addManualsWindow = new AddManualsWindow();
            addManualsWindow.Owner = this;
            addManualsWindow.ShowDialog();
            if (addManualsWindow.DialogResult == true)
            {
                PopulateMainDataGrid();
            }

        }
    }
}
