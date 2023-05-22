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
    /// Interaction logic for EditProformaWindow.xaml
    /// </summary>
    public partial class EditProformaWindow : Window
    {
        ProformaLine _proformaLine;
        public EditProformaWindow(ProformaLine selectedProformaLine)
        {
            InitializeComponent();
            this._proformaLine = selectedProformaLine;
            this.DataContext = this._proformaLine;
        }

        private void UpdateProforma_Button_Click(object sender, RoutedEventArgs e)
        {
            if (customerCodeTextBox.Text.Length != 6)
            {
                MessageBox.Show(this, "Customer code needs to be 6 digits");
                return;
            }
            if (CustomerNameTextBox.Text.Length == 0)
            {
                MessageBox.Show(this, "Customer name cannot be blank");
                return;
            }
            if (ContractNameTextBox.Text.Length == 0)
            {
                MessageBox.Show(this, "Contract name cannot be blank");
                return;
            }
            if (GoodsValueTextBox.Text.Length == 0)
            {
                MessageBox.Show(this, "Goods value cannot be blank");
                return;
            }
            if (SalesValueTextBox.Text.Length == 0)
            {
                MessageBox.Show(this, "Sales value cannot be blank");
                return;
            }


            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.UPDATE_PROFORMA_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", _proformaLine.Id);
                    command.Parameters.AddWithValue("@CustomerCode", customerCodeTextBox.Text);
                    command.Parameters.AddWithValue("@CustomerName", CustomerNameTextBox.Text);
                    command.Parameters.AddWithValue("@ContractName", ContractNameTextBox.Text);
                    command.Parameters.AddWithValue("@ProformaRef", ProformaRefTextBox.Text == "" ? null : ProformaRefTextBox.Text);
                    command.Parameters.AddWithValue("@DocRef", DocRefTextBox.Text == "" ? null : DocRefTextBox.Text);
                    command.Parameters.AddWithValue("@DocDate", DocDateDatePicker.SelectedDate);
                    command.Parameters.AddWithValue("@GoodsValue", GoodsValueTextBox.Text);
                    command.Parameters.AddWithValue("@SalesValue", SalesValueTextBox.Text);
                    command.Parameters.AddWithValue("@Comments", CommentsTextBox.Text == "" ? null : CommentsTextBox.Text);
                    command.Parameters.AddWithValue("@DeliveryDate", DeliveryDateDatePicker.SelectedDate);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            this.DialogResult = true;
            this.Close();

        }
    }
}
