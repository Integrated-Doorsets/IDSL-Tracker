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
    /// Interaction logic for ManageProformaWindow.xaml
    /// </summary>
    public partial class ManageProformaWindow : Window
    {
        public ManageProformaWindow()
        {
            InitializeComponent();
            PopulateMainProformaDataGrid();
        }

        private void PopulateMainProformaDataGrid()
        {
            List<ProformaLine> proformaLines = new List<ProformaLine>();


            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_PROFORMA_DETAILS", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            ProformaLine proformaLine = new ProformaLine();


                            proformaLine.JobNumber = reader.GetString(0);
                            proformaLine.CustomerCode = reader.GetString(1);
                            proformaLine.CustomerName = reader.GetString(2);
                            proformaLine.ContractName = reader.GetString(3);
                            proformaLine.ProformaRef = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            proformaLine.DocRef = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            proformaLine.DocDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);
                            proformaLine.GoodsValue = reader.IsDBNull(7) ? 0 : Convert.ToDecimal(reader.GetValue(7));
                            proformaLine.SalesValue = reader.IsDBNull(8) ? 0 : Convert.ToDecimal(reader.GetValue(8));
                            proformaLine.Comments = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                            proformaLine.DeliveryDate = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10);
                            proformaLine.Complete = reader.GetBoolean(11);
                            proformaLine.Id = reader.GetInt32(12);

                            proformaLines.Add(proformaLine);

                        }
                    }
                }
            }

            MainProformaDataGrid.ItemsSource = proformaLines;
        }

        private void EditProforma_Button_Click(object sender, RoutedEventArgs e)
        {
            ProformaLine selectedProformaLine = MainProformaDataGrid.SelectedItem as ProformaLine;



            EditProformaWindow editProformaWindow = new EditProformaWindow(selectedProformaLine);
            editProformaWindow.Owner = this;
            editProformaWindow.ShowDialog();
            if (editProformaWindow.DialogResult == true)
            {
                PopulateMainProformaDataGrid();
            }
        }

        private void CompleteProforma_Button_Click(object sender, RoutedEventArgs e)
        {

            ProformaLine selectedProformaLine = MainProformaDataGrid.SelectedItem as ProformaLine;


            String msg = string.Format($"Are you sure that you want to complete {selectedProformaLine.JobNumber}, with goods value of {selectedProformaLine.GoodsValue:c} ");
            MessageBoxResult result = MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
                {
                    using (SqlCommand command = new SqlCommand("Tracker.dbo.COMPLETE_PROFORMA_LINE", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Id", selectedProformaLine.Id);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                PopulateMainProformaDataGrid();
            }
        }

        private void AddProforma_Button_Click(object sender, RoutedEventArgs e)
        {
            AddProformaWindow addProformaWindow = new AddProformaWindow();
            addProformaWindow.Owner = this;
            addProformaWindow.ShowDialog();
            if (addProformaWindow.DialogResult == true)
            {
                PopulateMainProformaDataGrid();
            }

        }
    }
}
