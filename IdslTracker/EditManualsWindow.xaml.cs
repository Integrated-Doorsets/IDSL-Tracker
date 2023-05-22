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
    /// Interaction logic for EditManualsWindow.xaml
    /// </summary>
    public partial class EditManualsWindow : Window
    {
        ManualsLine manualsLine;
        public EditManualsWindow(ManualsLine selectedManualsLine)
        {
            InitializeComponent();

            this.manualsLine = selectedManualsLine;
            this.DataContext = this.manualsLine;
            jobTypeComboBox.Items.Add("IRON ONLY");
        }

        private void UpdateManuals_Button_Click(object sender, RoutedEventArgs e)
        {
            decimal valueDecimal;
            bool valueParsedOk = decimal.TryParse(valueTextBox.Text, out valueDecimal);

            if (valueParsedOk == false)
            {
                MessageBox.Show(this, "Please check value does not contain letters or symbols, aborted.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.UPDATE_MANUALS_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", manualsLine.Id);

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


            this.DialogResult = true;
            this.Close();

        }
    }
}
