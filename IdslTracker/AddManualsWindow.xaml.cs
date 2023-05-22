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
    /// Interaction logic for AddManualsWindow.xaml
    /// </summary>
    public partial class AddManualsWindow : Window
    {
        public AddManualsWindow()
        {
            InitializeComponent();

            jobTypeComboBox.Items.Add("IRON ONLY");
        }
        private void AddManuals_Button_Click(object sender, RoutedEventArgs e)
        {
            if(manualsDatePicker.SelectedDate == null)
            {
                MessageBox.Show(this, "Please pick a month, aborted.");
                return;
            }


            if(jobTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show(this, "Please pick a job type, aborted.");
                return;
            }


            decimal valueDecimal;
            bool valueParsedOk = decimal.TryParse(valueTextBox.Text, out valueDecimal);

            if (valueParsedOk == false)
            {
                MessageBox.Show(this, "Please check value does not contain letters or symbols, aborted.");
                return;
            }



            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.ADD_MANUALS_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@JobType", jobTypeComboBox.SelectedItem);
                    command.Parameters.AddWithValue("@ContractName", contractNameTextBox.Text);
                    command.Parameters.AddWithValue("@ContractNumber", contractNumberTextBox.Text);
                    command.Parameters.AddWithValue("@Scheduler", SchedulerTextBox.Text);
                    command.Parameters.AddWithValue("@Month", manualsDatePicker.SelectedDate.Value.Date);
                    command.Parameters.AddWithValue("@Value", valueDecimal);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }


            this.Close();
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            this.DialogResult = true;
            //this.Close();
        }
    }

}
