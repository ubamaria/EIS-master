using iTextSharp.text;
using iTextSharp.text.pdf;
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
        private string itogo = "";

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
                string dateFrom = dateTimePickerFrom.Value.AddDays(-1).ToString("yyyy-MM-dd H:mm");
                string dateTo = dateTimePickerTo.Value.AddDays(1).ToString("yyyy-MM-dd H:mm");
                labelSum.Text = "Итого: ";
                itogo = "";

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

                    double sum = 0;
                    for(int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        sum += Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);
                    }
                    itogo += sum + " ";

                    sum = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[3].Value != DBNull.Value)
                        {
                            sum += Convert.ToDouble(dataGridView1.Rows[i].Cells[3].Value);
                        }
                    }
                    itogo += sum;
                    labelSum.Text += itogo;
                }
                if (comboBoxReport.SelectedIndex == 1)
                {
                    string selectCommand = "SELECT M.Name, JO.Date, JO.IdRequest, SUM(TP.CountMaterial) AS CountMaterial, M.CostMaterial, M.NDS " +
                        "FROM TablePartOperation TP JOIN Material M ON M.IdMaterial = TP.IdMaterial " +
                        "JOIN JournalOfOperations JO ON TP.IdRequest = JO.IdRequest " +
                        "where JO.Date >= '" + dateFrom + "' and JO.Date <= '" + dateTo + "' GROUP BY M.Name";
                    selectTable(ConnectionString, selectCommand);

                    double sum = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        sum += Convert.ToDouble(dataGridView1.Rows[i].Cells[3].Value);
                    }
                    itogo += sum + " ";

                    sum = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        sum += Convert.ToDouble(dataGridView1.Rows[i].Cells[4].Value);
                    }
                    itogo += sum + " ";

                    sum = 0;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        sum += Convert.ToDouble(dataGridView1.Rows[i].Cells[5].Value);
                    }
                    itogo += sum;
                    labelSum.Text += itogo;
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

        private void buttonPDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "pdf|*.pdf"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string FONT_LOCATION = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.TTF"); //определяем В СИСТЕМЕ(чтобы не копировать файл) расположение шрифта arial.ttf
                    BaseFont baseFont = BaseFont.CreateFont(FONT_LOCATION, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED); //создаем шрифт
                    iTextSharp.text.Font fontParagraph = new iTextSharp.text.Font(baseFont, 17, iTextSharp.text.Font.NORMAL); //регистрируем + можно задать параметры для него(17 - размер, последний параметр - стиль)
                    string title = "";
                    if (comboBoxReport.SelectedIndex == 0)
                    {
                        title = "Ведомость заявок" + " с " + Convert.ToString(dateTimePickerFrom.Text) + " по " + Convert.ToString(dateTimePickerTo.Text) + "\n\n";
                    }
                    if (comboBoxReport.SelectedIndex == 1)
                    {
                        title = "Ведомость закупленных материалов" + " с " + Convert.ToString(dateTimePickerFrom.Text) + " по " + Convert.ToString(dateTimePickerTo.Text) + "\n\n";
                    }

                    var phraseTitle = new Phrase(title,
                    new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD));
                    iTextSharp.text.Paragraph paragraph = new
                   iTextSharp.text.Paragraph(phraseTitle)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 12
                    };

                    PdfPTable table = new PdfPTable(dataGridView1.Columns.Count);

                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        table.AddCell(new Phrase(dataGridView1.Columns[i].HeaderCell.Value.ToString(), fontParagraph));
                    }
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            table.AddCell(new Phrase(dataGridView1.Rows[i].Cells[j].Value.ToString(), fontParagraph));
                        }
                    }
                    PdfPTable table2 = new PdfPTable(dataGridView1.Columns.Count);
                    List<string> words = new List<string>();

                    words.Add("Итого:");
                    words.Add("");
                    if (comboBoxReport.SelectedIndex == 1)
                    {
                        words.Add("");
                    }
                    words.AddRange(itogo.Split(' '));
                    for (int j = 0; j < words.Count; j++)
                    {
                        table2.AddCell(new Phrase(words[j], fontParagraph));
                    }
                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A2, 10f, 10f, 10f, 0f);
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();
                        pdfDoc.Add(paragraph);
                        pdfDoc.Add(table);
                        pdfDoc.Add(table2);
                        pdfDoc.Close();
                        stream.Close();
                    }
                    MessageBox.Show("Выполнено", "Успех", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                }
            }
        }
    }
}
