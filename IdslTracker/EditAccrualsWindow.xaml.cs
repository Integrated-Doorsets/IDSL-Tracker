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
    /// Interaction logic for EditAccrualsWindow.xaml
    /// </summary>
    public partial class EditAccrualsWindow : Window
    {
        AccrualsLine accrualsLine;

        public EditAccrualsWindow()
        {
            InitializeComponent();
            //MessageBox.Show(this, "Add");
            ActionBtn.Content = "Add Accrual";
        }

        public EditAccrualsWindow(AccrualsLine selectedAccrualLine)
        {
            InitializeComponent();
            this.accrualsLine = selectedAccrualLine;
            this.DataContext = this.accrualsLine;
            //MessageBox.Show(this, "EDIT");
            ActionBtn.Content = "Update Accrual";
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {

            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.PUT_V2_TRACKER_ARCHIVE_LINE_BULK_DELIVERY_DATE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocNr", tbDocNr.Text);

                    command.Parameters.AddWithValue("@DeliveryDate", accrualDatePicker.SelectedDate.Value);
                    command.Parameters.AddWithValue("@Username", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                    command.Parameters.AddWithValue("@JobNr", JobNrTb.Text);


                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }


            this.DialogResult = true;
            this.Close();

        }


        private void SetDatepickerToday_Button_Click(object sender, RoutedEventArgs e)
        {
            accrualDatePicker.SelectedDate = DateTime.Today;
        }

        private void SetDatepicker2099_Button_Click(object sender, RoutedEventArgs e)
        {
            accrualDatePicker.SelectedDate = new DateTime(2099, 12, 31);
        }

    }


}
