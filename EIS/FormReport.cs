using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EIS
{
    public partial class FormReport : Form
    {
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();
        private string sPath = Path.Combine(Application.StartupPath, Program.sPath);

        public FormReport()
        {
            InitializeComponent();
        }

        private void FormReport_Load(object sender, EventArgs e)
        {
            comboBoxReport.Items.Add("Ведомость заявок");
            comboBoxReport.Items.Add("Ведомость закупленных материалов");
            comboBoxReport.SelectedIndex = -1;
        }

        private void updateTable()
        {
            if (comboBoxReport.SelectedIndex != -1)
            {
                string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";
                string dateFrom = dateTimePickerFrom.Value.ToString("yyyy-MM-dd");
                string dateTo = dateTimePickerTo.Value.ToString("yyyy-MM-dd");

                if (dateTimePickerFrom.Value.Date > dateTimePickerTo.Value.Date)
                {
                    MessageBox.Show("Дата начала периода должна быть меньше дата конца периода");
                    return;
                }
                if (comboBoxReport.SelectedIndex == 0)
                {
                    string selectCommand = "select R.RequestDate, R.IdRequest, (select SUM(RM.Count * M.CostMaterial) " +
                        "from RequestMaterial RM join Material M on M.IdMaterial = RM.IdMaterial where RM.IdRequest = R.IdRequest) AS RequestedPrice, " +
                        "(select SUM(TP.Price) from TablePartOperation TP where TP.IdRequest = R.IdRequest) AS BuyedPrice " +
                        "from Request R where R.RequestDate >= '" + dateFrom + "' and R.RequestDate <= '" + dateTo + "'";
                    selectTable(ConnectionString, selectCommand);
                }
                if (comboBoxReport.SelectedIndex == 1)
                {
                    string selectCommand = "SELECT M.Name, JO.Date, JO.IdRequest, SUM(TP.CountMaterial) AS CountMaterial, M.CostMaterial, M.NDS " +
                        "FROM TablePartOperation TP JOIN Material M ON M.IdMaterial = TP.IdMaterial " +
                        "JOIN JournalOfOperations JO ON TP.IdRequest = JO.IdRequest GROUP BY M.Name";
                    selectTable(ConnectionString, selectCommand);
                }
            }
        }

        private void comboBoxReport_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateTable();
        }

        public void selectTable(string ConnectionString, String selectCommand)
        {
            SQLiteConnection connect = new
           SQLiteConnection(ConnectionString);
            connect.Open();
            SQLiteDataAdapter dataAdapter = new
           SQLiteDataAdapter(selectCommand, connect);
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);
            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = ds.Tables[0].ToString();
            connect.Close();
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            updateTable();
        }

        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            updateTable();
        }
    }
}
