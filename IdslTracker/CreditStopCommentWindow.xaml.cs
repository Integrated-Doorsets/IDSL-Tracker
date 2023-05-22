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
    /// Interaction logic for CreditStopCommentWindow.xaml
    /// </summary>
    public partial class CreditStopCommentWindow : Window
    {

        internal CreditStopReviewLine creditStopReviewLine;
        internal List<CreditStopReviewLine> creditStopReviewLines;

        public CreditStopCommentWindow()
        {
            InitializeComponent();
        }

        private void Updated_Button_Click(object sender, RoutedEventArgs e)
        {
            if(creditStopReviewLine == null)
            {
                foreach(CreditStopReviewLine line in creditStopReviewLines)
                {
                    PutComment(line);
                }
            }
            else
            {

                PutComment(creditStopReviewLine);
            }

            


            this.DialogResult = true;
            this.Close();

        }

        private void PutComment(CreditStopReviewLine line)
        {
            using (SqlConnection connection = new SqlConnection(Properties.Resources.db))
            {
                using (SqlCommand command = new SqlCommand("Tracker.dbo.ADD_MATERIAL_REVIEW_COMMENT", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@AccountNr", line.AccountNumber);
                    command.Parameters.AddWithValue("@Comment", textBoxComment.Text);


                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(creditStopReviewLine != null)
            {
                textBoxComment.Text = creditStopReviewLine.Comment;
            }

        }
    }
}
