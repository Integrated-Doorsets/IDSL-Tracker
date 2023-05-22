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
    /// Interaction logic for AddProformaWindow.xaml
    /// </summary>
    public partial class AddProformaWindow : Window
    {
        public AddProformaWindow()
        {
            InitializeComponent();
        }

        private void AddProforma_Button_Click(object sender, RoutedEventArgs e)
        {
            if (jobNumberPart1TextBox.Text.Length != 4)
            {
                MessageBox.Show(this, "Contract number needs to be 4 digits");
                return;
            }
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
                using (SqlCommand command = new SqlCommand("Tracker.dbo.ADD_PROFORMA_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@JobNumber", $"{jobNumberPart1TextBox.Text}{ jobNumberPart2Label.Content}");
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

        private void jobNumberPart1TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if(tb.Text.Length == 0)
            {
                jobNumberPart2Label.Visibility = Visibility.Hidden;
            }
            else if(tb.Text.Length == 4)
            {
                jobNumberPart2Label.Visibility = Visibility.Visible;
                PopulateBasicInfoFromFtb();
                PopulateBasicInfoFromPegasus();
            }

        }

        private void PopulateBasicInfoFromPegasus()
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("PegasusCopy.dbo.GET_INACCURATE_PROFORMA_DATA", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ContractNum", jobNumberPart1TextBox.Text);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProformaRefTextBox.Text = reader.IsDBNull(0) ? String.Empty : reader.GetString(0);
                        }
                    }


                    connection.Close();
                }
            }
        }

        private void PopulateBasicInfoFromFtb()
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("FTB.dbo.GET_INACCURATE_PROFORMA_DATA", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ContractNum", jobNumberPart1TextBox.Text);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customerCodeTextBox.Text = reader.IsDBNull(0) ? String.Empty : reader.GetString(0);
                            CustomerNameTextBox.Text = reader.IsDBNull(1) ? String.Empty : reader.GetString(1);
                            ContractNameTextBox.Text = reader.IsDBNull(2) ? String.Empty : reader.GetString(2);
                            SalesValueTextBox.Text = reader.IsDBNull(3) ? String.Empty : reader.GetDecimal(3).ToString();
                        }
                    }


                    connection.Close();
                }
            }
        }
    }
}
