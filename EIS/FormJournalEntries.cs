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
    public partial class FormJournalEntries : Form
    {
        private int idJO = -1;
        public int IdJO { set { idJO = value; } }
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();
        private string sPath = Path.Combine(Application.StartupPath, Program.sPath);

        public FormJournalEntries()
        {
            InitializeComponent();
            dateTimePickerFrom.Enabled = false;
            dateTimePickerTo.Enabled = false;
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
            dataGridView.DataSource = ds;
            dataGridView.DataMember = ds.Tables[0].ToString();
            dataGridView.Update();
            dataGridView.Refresh();
            connect.Close();
        }

        private void updateGrid()
        {
            string ConnectionString = @"Data Source=" + sPath +
";New=False;Version=3";
            String selectCommand = "Select IdJournalEntries, Date, F.AccountNumber AS DT, SubcontoDt1, SubcontoDt2, " +
                "S.AccountNumber AS KT, SubcontoKt1, SubcontoKt2, Count, Sum, IdJournalOfOperations " +
                "FROM JournalEntries " +
                "Join ChartOfAccounts F On F.idChartOfAccounts = Dt " +
                "Join ChartOfAccounts S On S.idChartOfAccounts = Kt";
            if (!checkBoxAll.Checked)
            {
                selectCommand += " Where Date >= '" + dateTimePickerFrom.Value.ToString("yyyy-MM-dd") + 
                    "' and Date <= '" + dateTimePickerTo.Value.ToString("yyyy-MM-dd") + "'";
            }
            if (idJO != -1)
            {
                if (!selectCommand.Contains("Where"))
                {
                    selectCommand += " Where ";
                }
                else
                {
                    selectCommand += " and ";
                }
                selectCommand += "IdJournalOfOperations = '" + idJO + "'";
            }
            selectTable(ConnectionString, selectCommand);
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            updateGrid();
        }

        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            updateGrid();
        }

        private void checkBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            updateGrid();
            dateTimePickerFrom.Enabled = !checkBoxAll.Checked;
            dateTimePickerTo.Enabled = !checkBoxAll.Checked;
        }

        private void FormJournalEntries_Load(object sender, EventArgs e)
        {
            updateGrid();
        }
    }
}
