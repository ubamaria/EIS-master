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
    public partial class FormTablePartOperation : Form
    {
        private int idJO = -1;
        private int idRequest = -1;
        private string nameBuy = "";
        private DateTime date = DateTime.Now;
        public int IdJO { set { idJO = value; } }
        public int IdRequest { set { idRequest = value; } }
        public string NameBuy { set { nameBuy = value; } }
        public DateTime Date { set { date = value; } }

        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();
        private string sPath = Path.Combine(Application.StartupPath, Program.sPath);
        public FormTablePartOperation()
        {
            InitializeComponent();
        }

        private void FormDocPartRequest_Load(object sender, EventArgs e)
        {
            string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";
            String selectCommand = "Select TP.Id, M.Name, TP.IdRequest," +
                " TP.CountMaterial, TP.IdMaterial, TP.IdProvider, TP.Price, TP.NDS" +
                " From TablePartOperation TP" +
                " Join Material M On TP.IdMaterial = M.IdMaterial" +
                " Where TP.IdRequest = '" + idRequest + "'";
            selectTable(ConnectionString, selectCommand);

            if (idRequest != -1)
            {
                String selectMaterial = "Select RM.IdMaterial, M.Name from RequestMaterial RM" +
                    " Join Material M On M.IdMaterial = RM.IdMaterial" +
                    " Where IdRequest = '" + idRequest + "'";
                selectCombo(ConnectionString, selectMaterial, toolStripComboBoxMaterial, "Name", "IdMaterial");
                toolStripComboBoxMaterial.SelectedIndex = -1;
                string selectJO = "select IdJournalOfOperations from JournalOfOperations where IdRequest = '" + idRequest + "'";
                object id = selectValue(ConnectionString, selectJO);
                if (id != DBNull.Value)
                {
                    IdJO = Convert.ToInt32(id);
                }
            }

            if (idJO == -1)
            {
                buttonJE.Enabled = false;
            }

            string selectProvider = "select IdProvider, Organization from Provider";
            selectCombo(ConnectionString, selectProvider, toolStripComboBoxProvider, "Organization", "IdProvider");
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
            SQLiteCommand command = new SQLiteCommand(selectCommand, connect);
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
            toolStripTextBoxSumBuy.Text = "";
            toolStripComboBoxMaterial.SelectedIndex = -1;
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
            //dataGridView1.Columns[4].Visible = false;
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

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";
            String selectCommand = "select MAX(Id) from TablePartOperation";
            object maxValue = selectValue(ConnectionString, selectCommand);
            int MaxValue = 0;
            if (Convert.ToString(maxValue) != "")
            {
                MaxValue = Convert.ToInt32(maxValue) + 1;
            }
            string selectDT = "select Sum(Count) from JournalEntries where SubcontoDt1 = '"
                + toolStripComboBoxMaterial.ComboBox.Text + "' and Date <= '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            object dtCount = selectValue(ConnectionString, selectDT);
            string selectKT = "select Sum(Count) from JournalEntries where SubcontoKt1 = '"
                + toolStripComboBoxMaterial.ComboBox.Text + "' and Date <= '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            object ktCount = selectValue(ConnectionString, selectKT);
            if (dtCount == DBNull.Value)
            {
                dtCount = 0;
            }
            if (ktCount == DBNull.Value)
            {
                ktCount = 0;
            }
            int countOst = Convert.ToInt32(dtCount) - Convert.ToInt32(ktCount);
            toolStripTextBoxRemains.Text = countOst.ToString();
            int countReq = Convert.ToInt32(textBoxRequested.Text);
            int count = countReq - countOst;
            double MPrice = Convert.ToDouble(toolStripTextBoxMPrice.Text);

            double price = count * MPrice;
            string selectNDS = "select nds from material where idmaterial = '" + toolStripComboBoxMaterial.ComboBox.SelectedValue + "'";
            double nds = Convert.ToDouble(selectValue(ConnectionString, selectNDS));
            double SumNDS = (nds*price)/100;

            string material = toolStripComboBoxMaterial.ComboBox.Text;
            string provider = toolStripComboBoxProvider.ComboBox.Text;

            if (count <= 0)
            {
                MessageBox.Show("Остатки материалов достаточны для удовлетворения заявки");
                return;
            }

            string txtSQLQuery = "insert into TablePartOperation (Id, IdMaterial, IdRequest, IdProvider, CountMaterial, Price, NDS) values (" + MaxValue+ ", '" +
                toolStripComboBoxMaterial.ComboBox.SelectedValue + "', '" + idRequest + "', '" + toolStripComboBoxProvider.ComboBox.SelectedValue + "', '" + count + "', '" + price + "', '" + SumNDS + "')";
            ExecuteQuery(txtSQLQuery);

            txtSQLQuery = "insert into JournalEntries (Date, Dt, " +
        "SubcontoDt1, SubcontoDt2, Kt, SubcontoKt1, SubcontoKt2, Count, Sum, " +
        "IdJournalOfOperations) values ('" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") +
        "', '1', '" + material + "', '', '2', '" + provider + "', '" + idRequest +
        "', '" + count + "', '" + price + "', '" + idJO + "')";
            ExecuteQuery(txtSQLQuery);
            txtSQLQuery = "insert into JournalEntries (Date, Dt, " +
        "SubcontoDt1, SubcontoDt2, Kt, SubcontoKt1, SubcontoKt2, Count, Sum, " +
        "IdJournalOfOperations) values ('" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") +
        "', '3', '', '', '2', '" + provider + "', '" + idRequest +
        "', '" + count + "', '" + SumNDS + "', '" + idJO + "')";
            ExecuteQuery(txtSQLQuery);

            selectCommand = "Select TP.Id, M.Name, TP.IdRequest," +
                " TP.CountMaterial, TP.IdMaterial, TP.IdProvider, TP.Price, TP.NDS" +
                " From TablePartOperation TP" +
                " Join Material M On TP.IdMaterial = M.IdMaterial" +
                " Where TP.IdRequest = '" + idRequest + "'";
            refreshForm(ConnectionString, selectCommand);
        }

        private void toolStripButtonChange_Click(object sender, EventArgs e)
        {
            int CurrentRow = dataGridView1.SelectedCells[0].RowIndex;
            string valueId = dataGridView1[0, CurrentRow].Value.ToString();
            string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";

            string selectDT = "select Sum(Count) from JournalEntries where SubcontoDt1 = '"
                + toolStripComboBoxMaterial.ComboBox.Text + "' and Date <= '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            object dtCount = selectValue(ConnectionString, selectDT);
            string selectKT = "select Sum(Count) from JournalEntries where SubcontoKt1 = '"
                + toolStripComboBoxMaterial.ComboBox.Text + "' and Date <= '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            object ktCount = selectValue(ConnectionString, selectKT);
            if (dtCount == DBNull.Value)
            {
                dtCount = 0;
            }
            if (ktCount == DBNull.Value)
            {
                ktCount = 0;
            }
            int countOst = Convert.ToInt32(dtCount) - Convert.ToInt32(ktCount);
            int countReq = Convert.ToInt32(textBoxRequested.Text);
            int count = countReq - countOst;
            if (count <= 0)
            {
                MessageBox.Show("Остатки материалов достаточны для удовлетворения заявки");
                return;
            }

            double MPrice = Convert.ToDouble(toolStripTextBoxMPrice.Text);
            double price = count * MPrice;
            string selectNDS = "select nds from material where idmaterial = '" + toolStripComboBoxMaterial.ComboBox.SelectedValue + "'";
            double nds = Convert.ToDouble(selectValue(ConnectionString, selectNDS));

            string changeCountMaterial = Convert.ToString(count);
            String selectCountMaterial = "update TablePartOperation set CountMaterial='" + changeCountMaterial + "'where Id = " + valueId;
            changeValue(ConnectionString, selectCountMaterial);

            string changeIdMaterial = toolStripComboBoxMaterial.ComboBox.SelectedValue.ToString();
            String selectIdMaterial = "update TablePartOperation set IdMaterial='" + changeIdMaterial + "'where Id = " + valueId;
            changeValue(ConnectionString, selectIdMaterial);

            string changeIdProvider = toolStripComboBoxProvider.ComboBox.SelectedValue.ToString();
            String selectIdProvider = "update TablePartOperation set IdProvider='" + changeIdProvider + "'where Id = " + valueId;
            changeValue(ConnectionString, selectIdProvider);

            String selectPrice = "update TablePartOperation set Price='" + price + "'where Id = " + valueId;
            changeValue(ConnectionString, selectPrice);

            String selectnds = "update TablePartOperation set NDS='" + nds + "'where Id = " + valueId;
            changeValue(ConnectionString, selectnds);

            string txtSQLQuery = "update JournalEntries set Count = '" + count + "' where IdJournalOfOperations = '" + idJO + "'";
            ExecuteQuery(txtSQLQuery);
            txtSQLQuery = "update JournalEntries set Sum = '" + price + "' where IdJournalOfOperations = '" + idJO + "' and DT = '1'";
            ExecuteQuery(txtSQLQuery);
            txtSQLQuery = "update JournalEntries set Sum = '" + nds + "' where IdJournalOfOperations = '" + idJO + "' and DT = '3'";
            ExecuteQuery(txtSQLQuery);

            string selectCommand = "Select TP.Id, M.Name, TP.IdRequest," +
                " TP.CountMaterial, TP.IdMaterial, TP.IdProvider, TP.Price, TP.NDS" +
                " From TablePartOperation TP" +
                " Join Material M On TP.IdMaterial = M.IdMaterial" +
                " Where TP.IdRequest = '" + idRequest + "'";
            refreshForm(ConnectionString, selectCommand);
        }

        private void toolStripButtonDel_Click(object sender, EventArgs e)
        {
            //выбрана строка CurrentRow
            int CurrentRow = dataGridView1.SelectedCells[0].RowIndex;
            string valueId = dataGridView1[0, CurrentRow].Value.ToString();
            String selectCommand = "delete from TablePartOperation where Id=" + valueId;
            string ConnectionString = @"Data Source=" + sPath +
           ";New=False;Version=3";
            changeValue(ConnectionString, selectCommand);
            selectCommand = "delete from JournalEntries where IdJournalOfOperations=" + idJO;
            ConnectionString = @"Data Source=" + sPath +
           ";New=False;Version=3";
            changeValue(ConnectionString, selectCommand);

            //обновление dataGridView1
            selectCommand = "Select TP.Id, M.Name, TP.IdRequest," +
                " TP.CountMaterial, TP.IdMaterial, TP.IdProvider, TP.Price, TP.NDS" +
                " From TablePartOperation TP" +
                " Join Material M On TP.IdMaterial = M.IdMaterial" +
                " Where TP.IdRequest = '" + idRequest + "'";
            refreshForm(ConnectionString, selectCommand);
        }

        private void toolStripComboBoxMaterial_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";

            String selectMPrice = "select CostMaterial from Material where IdMaterial='" + toolStripComboBoxMaterial.ComboBox.SelectedValue + "'";
            toolStripTextBoxMPrice.Text = Convert.ToString(selectValue(ConnectionString, selectMPrice));

            String selectRequested = "select Count from RequestMaterial where IdMaterial='" + toolStripComboBoxMaterial.ComboBox.SelectedValue + "'" +
                " and IdRequest = '" + idRequest + "'";
            textBoxRequested.Text = Convert.ToString(selectValue(ConnectionString, selectRequested));
            try
            {
                string selectDT = "select Sum(Count) from JournalEntries where SubcontoDt1 = '"
                + toolStripComboBoxMaterial.ComboBox.Text + "' and Date <= '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                object dtCount = selectValue(ConnectionString, selectDT);
                string selectKT = "select Sum(Count) from JournalEntries where SubcontoKt1 = '"
                    + toolStripComboBoxMaterial.ComboBox.Text + "' and Date <= '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                object ktCount = selectValue(ConnectionString, selectKT);
                if (dtCount == DBNull.Value)
                {
                    dtCount = 0;
                }
                if (ktCount == DBNull.Value)
                {
                    ktCount = 0;
                }
                int countOst = Convert.ToInt32(dtCount) - Convert.ToInt32(ktCount);
                int countReq = Convert.ToInt32(textBoxRequested.Text);
                int count = countReq - countOst;
                
                toolStripTextBoxRemains.Text = countOst.ToString();
                if (count <= 0)
                {
                    return;
                }
                double MPrice = Convert.ToDouble(toolStripTextBoxMPrice.Text);
                double SumBuy = MPrice * count;
                toolStripTextBoxSumBuy.Text = SumBuy.ToString();
            }
            catch (Exception) { }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int CurrentRow = dataGridView1.SelectedCells[0].RowIndex;

            int MaterialId = Convert.ToInt32(dataGridView1[4, CurrentRow].Value);
            toolStripComboBoxMaterial.ComboBox.SelectedValue = MaterialId;

            int ProviderId = Convert.ToInt32(dataGridView1[5, CurrentRow].Value);
            toolStripComboBoxProvider.ComboBox.SelectedValue = ProviderId;
        }

        private void FormTablePartOperation_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                string ConnectionString = @"Data Source=" + sPath + ";New=False;Version=3";
                string selectCountBuy = "select SUM(CountMaterial) from TablePartOperation where IdRequest = '" + idRequest + "'";
                int countBuy = Convert.ToInt32(selectValue(ConnectionString, selectCountBuy));
                string selectSumPrice = "select SUM(Price) from TablePartOperation where IdRequest = '" + idRequest + "'";
                double SumPrice = Convert.ToDouble(selectValue(ConnectionString, selectSumPrice));
                string selectNDS = "select SUM(NDS) from TablePartOperation TPO where IdRequest='" + idRequest + "'";
                double sumNDS = Convert.ToDouble(selectValue(ConnectionString, selectNDS));
                string selectCommand = "select MAX(IdJournalOfOperations) from JournalOfOperations";
                object maxValue = selectValue(ConnectionString, selectCommand);
                int MaxValue = 0;
                if (Convert.ToString(maxValue) != "")
                {
                    MaxValue = Convert.ToInt32(maxValue) + 1;
                }

                if (idJO == -1)//add
                {
                    string txtSQLQuery = "insert into JournalOfOperations (IdJournalOfOperations, NameBuy, Date, CountBuy, SumBuy, SumNDS, IdRequest) " +
                        "values ('" + MaxValue + "', '" + nameBuy + "', '" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', '" +
                        countBuy + "', '" + SumPrice + "', '" + sumNDS + "', '" + idRequest + "')";
                    ExecuteQuery(txtSQLQuery);
                    txtSQLQuery = "update JournalEntries set IdJournalOfOperations = '" + MaxValue + "' where IdJournalOfOperations = '-1'";
                    ExecuteQuery(txtSQLQuery);
                }
                else //change
                {
                    string txtSQLQuery = "update JournalOfOperations set CountBuy = '" + countBuy + "' where IdJournalOfOperations = '" + idJO + "'";
                    ExecuteQuery(txtSQLQuery);
                    txtSQLQuery = "update JournalOfOperations set SumBuy = '" + SumPrice + "' where IdJournalOfOperations = '" + idJO + "'";
                    ExecuteQuery(txtSQLQuery);
                    txtSQLQuery = "update JournalOfOperations set SumNDS = '" + sumNDS + "' where IdJournalOfOperations = '" + idJO + "'";
                    ExecuteQuery(txtSQLQuery);
                }
            }
            catch (Exception) { }
        }

        private void buttonJE_Click(object sender, EventArgs e)
        {
            var form = new FormJournalEntries();
            form.IdJO = idJO;
            form.Show();
        }
    }
}