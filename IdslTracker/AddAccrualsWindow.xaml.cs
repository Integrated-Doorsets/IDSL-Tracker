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
    /// Interaction logic for AddAccrualsWindow.xaml
    /// </summary>
    public partial class AddAccrualsWindow : Window
    {
        private List<AccrualDropDownItem> masterAccrualDropDownItems = new List<AccrualDropDownItem>();
        public AddAccrualsWindow()
        {
            InitializeComponent();
            GatherMasterComboboxItems();
            PopulateFilteredCombobox();
        }

        private void GatherMasterComboboxItems()
        {
            //accrualDropDownItems = new List<AccrualDropDownItem>();
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.GET_ACCRUAL_DROPDOWN_LIST", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 600;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            AccrualDropDownItem accrualDropDownItem = new AccrualDropDownItem();


                            accrualDropDownItem.JobNrPipeDocNr = string.Format("{1} | {0}", reader.GetString(0), reader.GetString(1));
                            accrualDropDownItem.JobNr = reader.GetString(1);
                            accrualDropDownItem.DocNr = reader.GetString(0);
                            accrualDropDownItem.Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));




                            //docNrJobNrComboBox.Items.Add(accrualDropDownItem.JobNrPipeDocNr);
                            masterAccrualDropDownItems.Add(accrualDropDownItem);

                        }
                    }
                }
            }
        }


        private void DocNrJobNrComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //valueTextBox.Text = masterAccrualDropDownItems[docNrJobNrComboBox.SelectedIndex].Value.ToString();

            valueTextBox.Text = (docNrJobNrComboBox.ItemsSource as List<AccrualDropDownItem>)[docNrJobNrComboBox.SelectedIndex].Value.ToString();
            accrualDatePicker.SelectedDate = new DateTime(2099, 12, 31);
        }

        private void SetDatepickerToday_Button_Click(object sender, RoutedEventArgs e)
        {
            accrualDatePicker.SelectedDate = DateTime.Today;
        }

        private void SetDatepicker2099_Button_Click(object sender, RoutedEventArgs e)
        {
            accrualDatePicker.SelectedDate = new DateTime(2099, 12, 31);
        }

        private void AddAccrual_Button_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                AccrualDropDownItem selectedAccrualDropDownItem = (docNrJobNrComboBox.ItemsSource as List<AccrualDropDownItem>)[docNrJobNrComboBox.SelectedIndex];

                using (SqlCommand command = new SqlCommand("Tracker.dbo.ADD_ACCRUAL_LINE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@DocNr", selectedAccrualDropDownItem.DocNr);
                    command.Parameters.AddWithValue("@Value", valueTextBox.Text);
                    command.Parameters.AddWithValue("@Date", accrualDatePicker.SelectedDate.Value.Date);

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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            docNrJobNrComboBox.SelectionChanged -= new SelectionChangedEventHandler(DocNrJobNrComboBox_SelectionChanged);

            docNrJobNrComboBox.SelectedIndex = -1;
            PopulateFilteredCombobox();

            docNrJobNrComboBox.SelectionChanged += new SelectionChangedEventHandler(DocNrJobNrComboBox_SelectionChanged);
        }


        private void PopulateFilteredCombobox()
        {
            List<AccrualDropDownItem> filteredAccrualDropDownItems = new List<AccrualDropDownItem>();
            
            foreach(AccrualDropDownItem item in masterAccrualDropDownItems)
            {
                if (
                    item.DocNr.ToUpper().Contains(filterTextBox.Text.ToUpper()) ||
                    item.JobNr.ToUpper().Contains(filterTextBox.Text.ToUpper()))
                {
                    filteredAccrualDropDownItems.Add(item);
                }
            }

            docNrJobNrComboBox.ItemsSource = filteredAccrualDropDownItems;

        }
    }
}
