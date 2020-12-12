using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EIS
{
    public partial class FormJournalOperation : Form
    {
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();
        private string sPath = Path.Combine(Application.StartupPath, Program.sPath);

        public FormJournalOperation()
        {
            InitializeComponent();
        }

        private void FormJournalOperation_Load(object sender, EventArgs e)
        {
            string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";
            String selectCommand = "Select JO.IdJournalOfOperations, JO.NameBuy, JO.Date," +
                " JO.CountBuy, JO.SumBuy, JO.SumNDS, R.RequestDate, JO.IdRequest, B.FIO" +
                " From JournalOfOperations JO" +
                " Join Request R On R.IdRequest = JO.IdRequest" +
                " Join Buyer B On R.IdBuyer = B.IdBuyer";
            selectTable(ConnectionString, selectCommand);
            toolStripTextBoxName.Text = "Покупка по заявке";
            String selectRequest = "Select IdRequest from Request";
            selectCombo(ConnectionString, selectRequest, toolStripComboBoxRequest, "IdRequest", "IdRequest");
            toolStripComboBoxRequest.SelectedIndex = -1;
        }

        public void selectCombo(string ConnectionString, String selectCommand,
ToolStripComboBox comboBox, string displayMember, string valueMember)
        {
            SQLiteConnection connect = new
           SQLiteConnection(ConnectionString);
            connect.Open();
            SQLiteDataAdapter dataAdapter = new
           SQLiteDataAdapter(selectCommand, connect);
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);
            comboBox.ComboBox.DataSource = ds.Tables[0];
            comboBox.ComboBox.DisplayMember = displayMember;
            comboBox.ComboBox.ValueMember = valueMember;
            connect.Close();
        }

        public object selectValue(string ConnectionString, String selectCommand)
        {
            SQLiteConnection connect = new
           SQLiteConnection(ConnectionString);
            connect.Open();
            SQLiteCommand command = new SQLiteCommand(selectCommand,
connect);
            SQLiteDataReader reader = command.ExecuteReader();
            object value = "";
            while (reader.Read())
            {
                value = reader[0];
            }
            connect.Close();
            return value;
        }

        private void ExecuteQuery(string txtQuery)
        {
            sql_con = new SQLiteConnection("Data Source=" + sPath +
           ";Version=3;New=False;Compress=True;");
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = txtQuery;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }

        public void refreshForm(string ConnectionString, String selectCommand)
        {
            selectTable(ConnectionString, selectCommand);
            dataGridView1.Update();
            dataGridView1.Refresh();
            toolStripTextBoxName.Text = "Покупка по заявке";
            toolStripComboBoxRequest.SelectedIndex = -1;
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
            dataGridView1.Columns[8].Visible = false;
            connect.Close();
        }

        public void changeValue(string ConnectionString, String selectCommand)
        {
            SQLiteConnection connect = new
           SQLiteConnection(ConnectionString);
            connect.Open();
            SQLiteTransaction trans;
            SQLiteCommand cmd = new SQLiteCommand();
            trans = connect.BeginTransaction();
            cmd.Connection = connect;
            cmd.CommandText = selectCommand;
            cmd.ExecuteNonQuery();
            trans.Commit();
            connect.Close();
        }

        private void toolStripButtonBuy_Click(object sender, EventArgs e)
        {
            string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";
            var form = new FormTablePartOperation();
            form.IdRequest = Convert.ToInt32(toolStripComboBoxRequest.ComboBox.SelectedValue);
            form.NameBuy = toolStripTextBoxName.Text;
            string selectDate = "select RequestDate from request where idRequest = '" +
                Convert.ToInt32(toolStripComboBoxRequest.ComboBox.SelectedValue) + "'";
            DateTime reqDate = Convert.ToDateTime(selectValue(ConnectionString, selectDate));
            if (dateTimePicker1.Value < reqDate)
            {
                MessageBox.Show("Дата операции должна быть мень даты заявки - " + reqDate);
                return;
            }
            form.Date = dateTimePicker1.Value;
            form.FormClosed += new FormClosedEventHandler(formTPOclosed);
            form.Show();
        }

        private void toolStripButtonChange_Click(object sender, EventArgs e)
        {
            int CurrentRow = dataGridView1.SelectedCells[0].RowIndex;
            string valueId = dataGridView1[0, CurrentRow].Value.ToString();
            string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";

            string selectDate = "select RequestDate from request where idRequest = '" +
                Convert.ToInt32(toolStripComboBoxRequest.ComboBox.SelectedValue) + "'";
            DateTime reqDate = Convert.ToDateTime(selectValue(ConnectionString, selectDate));
            if (dateTimePicker1.Value < reqDate)
            {
                MessageBox.Show("Дата операции должна быть мень даты заявки - " + reqDate);
                return;
            }

            string changeName = toolStripTextBoxName.Text;
            String selectName = "update JournalOfOperations set NameBuy='" + changeName + "'where IdJournalOfOperations = " + valueId;
            changeValue(ConnectionString, selectName);

            string changeDate = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
            selectDate = "update JournalOfOperations set Date='" + changeDate + "'where IdJournalOfOperations = " + valueId;
            changeValue(ConnectionString, selectDate);

            string selectCommand = "Select JO.IdJournalOfOperations, JO.NameBuy, JO.Date," +
                " JO.CountBuy, JO.SumBuy, JO.SumNDS, R.RequestDate, JO.IdRequest, B.FIO" +
                " From JournalOfOperations JO" +
                " Join Request R On R.IdRequest = JO.IdRequest" +
                " Join Buyer B On R.IdBuyer = B.IdBuyer";
            refreshForm(ConnectionString, selectCommand);
        }

        private void toolStripButtonDel_Click(object sender, EventArgs e)
        {
            //выбрана строка CurrentRow
            int CurrentRow = dataGridView1.SelectedCells[0].RowIndex;
            //получить значение idMOL выбранной строки
            string valueId = dataGridView1[0, CurrentRow].Value.ToString();
            String selectCommand = "delete from JournalOfOperations where IdJournalOfOperations=" + valueId;
            string ConnectionString = @"Data Source=" + sPath +
           ";New=False;Version=3";
            changeValue(ConnectionString, selectCommand);
            selectCommand = "delete from TablePartOperation where IdRequest = '"
                + Convert.ToInt32(toolStripComboBoxRequest.ComboBox.SelectedValue) + "'";
            ConnectionString = @"Data Source=" + sPath +
           ";New=False;Version=3";
            changeValue(ConnectionString, selectCommand);
            //обновление dataGridView1
            selectCommand = "Select JO.IdJournalOfOperations, JO.NameBuy, JO.Date," +
                " JO.CountBuy, JO.SumBuy, JO.SumNDS, R.RequestDate, JO.IdRequest, B.FIO" +
                " From JournalOfOperations JO" +
                " Join Request R On R.IdRequest = JO.IdRequest" +
                " Join Buyer B On R.IdBuyer = B.IdBuyer";
            refreshForm(ConnectionString, selectCommand);
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //выбрана строка CurrentRow
            int CurrentRow = dataGridView1.SelectedCells[0].RowIndex;
            //получить значение Name выбранной строки
            string nameId = dataGridView1[1, CurrentRow].Value.ToString();
            toolStripTextBoxName.Text = nameId;
            string dateId = dataGridView1[2, CurrentRow].Value.ToString();
            dateTimePicker1.Value = Convert.ToDateTime(dateId);
            int RequestId = Convert.ToInt32(dataGridView1[7, CurrentRow].Value);
            toolStripComboBoxRequest.ComboBox.SelectedValue = RequestId;
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                string ConnectionString = @"Data Source=" + sPath +
               ";New=False;Version=3";
                var form = new FormTablePartOperation();
                int CurrentRow = Convert.ToInt32(dataGridView1.SelectedCells[0].Value);
                string selectCommand = "Select IdRequest From JournalOfOperations Where IdJournalOfOperations = '" + CurrentRow + "'";
                try
                {
                    form.IdRequest = Convert.ToInt32(selectValue(ConnectionString, selectCommand));
                } catch (Exception) { }

                form.IdJO = CurrentRow;
                form.NameBuy = toolStripTextBoxName.Text;
                form.Date = dateTimePicker1.Value;
                form.FormClosed += new FormClosedEventHandler(formTPOclosed);
                form.Show();
            }
        }

        private void formTPOclosed(object s, EventArgs e)
        {
            string ConnectionString = @"Data Source=" + sPath +
           ";New=False;Version=3";

            string selectCommand = "Select JO.IdJournalOfOperations, JO.NameBuy, JO.Date," +
                " JO.CountBuy, JO.SumBuy, JO.SumNDS, R.RequestDate, JO.IdRequest, B.FIO" +
                " From JournalOfOperations JO" +
                " Join Request R On R.IdRequest = JO.IdRequest" +
                " Join Buyer B On R.IdBuyer = B.IdBuyer";
            refreshForm(ConnectionString, selectCommand);
        }
    }
}
