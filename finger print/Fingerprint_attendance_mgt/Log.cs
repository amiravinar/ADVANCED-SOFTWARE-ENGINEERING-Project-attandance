using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace Fingerprint_attendance_mgt
{
    public partial class Log : Form
    {
        DataTable table;
        public Log()
        {
            InitializeComponent();
        }

        private void Log_Load(object sender, EventArgs e)
        {
            Database db = new Database();
            //var table = db.dailylog_retrieve();
            //dataGridView1.DataSource = table;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string date_from = textBox_from.Text.Replace(" ", string.Empty);
            string date_to = textBox_to.Text.Replace(" ", string.Empty);

            Database db = new Database();
            table = db.dailylog_retrieve(date_from, date_to);
            dataGridView1.DataSource = table;
        }

        public void pdf_gen(DataTable dt)
        {
            if (dt == null)
            {
                MessageBox.Show("Click Generate Button");
            }
            else
            {
                SaveFileDialog save = new SaveFileDialog();
                save.ShowDialog();
                using (FileStream stream = new FileStream(save.FileName + ".pdf", FileMode.Create))
                {
                    Document doc = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(doc, stream);

                    writer.PageEvent = new headerEvent();

                    doc.AddHeader("record", "Records");
                    doc.Open();
                    iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    // Header header = new Header("Records", "Records");

                    Paragraph p = new Paragraph("ATTENDANCE REPORT FOR " + textBox_from.Text.ToUpper() + " TO " + textBox_to.Text.ToUpper());
                    p.Alignment = Element.ALIGN_CENTER;

                    p.SetLeading(30, 0);
                    p.Font = FontFactory.GetFont("HELVETICA", 30, BaseColor.RED);
                    Paragraph p1 = new Paragraph(" ");
                    doc.Add(p);
                    doc.Add(p1);

                    PdfPTable table = new PdfPTable(dt.Columns.Count);
                    PdfPRow row = null;
                    float[] widths = new float[] { 4f, 4f, 4f, 4f, 4f,4f,4f };

                    table.SetWidths(widths);

                    table.WidthPercentage = 100;
                    //int iCol = 0;
                    //string colname = "";
                    PdfPCell cell = new PdfPCell(new Phrase("Products"));

                    cell.Colspan = dt.Columns.Count;



                    foreach (DataColumn c in dt.Columns)
                    {

                        table.AddCell(new Phrase(c.ColumnName, font5));
                    }

                    foreach (DataRow r in dt.Rows)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            table.AddCell(new Phrase(r[0].ToString(), font5));
                            table.AddCell(new Phrase(r[1].ToString(), font5));
                            table.AddCell(new Phrase(r[2].ToString(), font5));
                            table.AddCell(new Phrase(r[3].ToString(), font5));
                            table.AddCell(new Phrase(r[4].ToString(), font5));
                            table.AddCell(new Phrase(r[5].ToString(), font5));
                            table.AddCell(new Phrase(r[6].ToString(), font5));
                        }
                    }
                    doc.Add(table);
                    doc.Close();
                }


            }
        }

        public class headerEvent: PdfPageEventHelper
        {
            public  void onEndPage(PdfWriter writer, Document document)
            {
                // iTextSharp.text.Font ffont = new iTextSharp.text.Font(Font.FontFamily ;
                PdfContentByte cb = writer.DirectContent;
                Phrase header = new Phrase("this is a header");
                Phrase footer = new Phrase("this is a footer");
                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                        header,
                        (document.Right - document.Left) / 2 + document.LeftMargin,
                        document.Top + 10, 0);
                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                        footer,
                        (document.Right - document.Left) / 2 + document.LeftMargin,
                        document.Bottom - 10, 0);
            }
        }
     

        private void button2_Click(object sender, EventArgs e)
        {
            pdf_gen(table);
        }

        private void button_remove_Click(object sender, EventArgs e)
        {
            var db = new Database();
            db.remove_staff(textBox_staff_id.Text.Replace(" ", string.Empty));

        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            var db = new Database();
            db.clear_attendance();
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            var db = new Database();
            db.deleteAll();
        }
    }
}
