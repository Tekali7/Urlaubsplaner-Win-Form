using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Urlaubsplanung.Enums;

namespace Urlaubsplanung
{
    public partial class FormVerwaltung : Form
    {

        SqlConnection cn;
        SqlDataReader dr;

        int MitarbeiterID;
        int Urlaubsanspruch;
        int Fehlstunden;
        public FormVerwaltung(int mitarbeiterID)
        {
            InitializeComponent();
            MitarbeiterID = mitarbeiterID;
        }

        private void FormVerwaltung_Load(object sender, EventArgs e)
        {
            string pw = "test";

            System.Security.SecureString strsec = new System.Security.SecureString();
            foreach (char c in pw.ToCharArray())
            {
                strsec.AppendChar(c);
            }
            strsec.MakeReadOnly();

            System.Data.SqlClient.SqlCredential sqlCred = new System.Data.SqlClient.SqlCredential("urlaubdbuser", strsec);

            System.Data.SqlClient.SqlConnection sqlCon = new System.Data.SqlClient.SqlConnection("Persist Security Info=False;Data Source=PN-PRECISION;Initial Catalog=urlaubdb", sqlCred);
            sqlCon.Open();

            cn = sqlCon;

            // DataSet erstellen
            DataSet übersicht = new DataSet();

            DataTable übersichtTable = übersicht.Tables.Add("Übersicht");

            DataColumn columns =
            übersichtTable.Columns.Add("Name");
            übersichtTable.Columns.Add("Urlaubsanspruch");
            übersichtTable.Columns.Add("Fehlstunden");
            for (int i = 1; i <= 52; i++)
            {
                übersichtTable.Columns.Add("KW" + i);
            }

            // Und hier wie das DataGridView befüllt wird:
            DataSet myData = this.HoleDatenVariante1("Mitarbeiter");

            if (myData != null)
            {
                foreach (DataRow row in myData.Tables["Mitarbeiter"].Rows)
                {
                    DataRow[] existingRows = übersichtTable.Select("Name = '" + row["Name"].ToString() + "'");

                    DataRow targetRow;

                    if (existingRows.Length > 0)
                    {
                        targetRow = existingRows[0];
                    }
                    else
                    {
                        targetRow = übersichtTable.NewRow();
                        targetRow["Name"] = row["Name"];
                        targetRow["Urlaubsanspruch"] = row["Urlaubsanspruch"];
                        targetRow["Fehlstunden"] = row["Fehlstunden"];
                        übersichtTable.Rows.Add(targetRow);
                    }

                    var urlaubRows = myData.Tables["Mitarbeiter"].AsEnumerable()
                        .Where(urlaubRow => urlaubRow["Name"].ToString() == row["Name"].ToString());

                    foreach (var urlaubRow in urlaubRows)
                    {
                        if (!DateTime.TryParse(urlaubRow["DatumBeginn"].ToString(), out DateTime datumBeginn) ||
                            !DateTime.TryParse(urlaubRow["DatumEnde"].ToString(), out DateTime datumEnde))
                        {
                            continue;
                        }

                        int kwBeginn = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(datumBeginn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                        int kwEnde = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(datumEnde, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                        for (int kw = kwBeginn; kw <= kwEnde; kw++)
                        {
                            targetRow["KW" + kw] = datumBeginn.ToShortDateString() + "-" + datumEnde.ToShortDateString();
                        }
                    }
                }
            }
            this.dataGridView1.DataSource = übersicht;
            this.dataGridView1.DataMember = "Übersicht";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public DataSet HoleDatenVariante1(string tableName)
        {
            // lokale Variablendefinitionen
            DataSet retValue = new DataSet();

            // Bastle einen SQL-Befehl in Form eines Strings, der mir die Daten aus der DB holen soll
            string sqlCommand = String.Concat("SELECT Name, Urlaubsanspruch, Fehlstunden, DatumBeginn, DatumEnde FROM ", tableName, " FULL OUTER JOIN Urlaubsantrag ON Mitarbeiter.MitarbeiterID = Urlaubsantrag.MitarbeiterID");

            // Erstelle mir einen SQL-Befehl für meine DB Verbindung
            SqlDataAdapter cmd = new SqlDataAdapter(sqlCommand, cn);

            // Führe den SQL-Befehl aus, und hole die Daten aus der DB, speichere die Daten in dem DataSet
            cmd.Fill(retValue, tableName);

            // Schließe die DB Verbindung nun wieder
            // cn.Close();

            // gib die Daten zurück
            return retValue;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateSelectedCell(EnumStatus.Status.Genehmigt, Color.Green, "Der Antrag wurde genehmigt.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateSelectedCell(EnumStatus.Status.Abgelehnt, Color.Red, "Der Antrag wurde abgelehnt.");
        }

        private void UpdateSelectedCell(EnumStatus.Status status, Color color, string message)
        {
            if (dataGridView1.CurrentCell != null)
            {
                var cell = dataGridView1.CurrentCell;

                if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    cell.Tag = status;
                    cell.Style.BackColor = color;
                    MessageBox.Show(message, "Meldung", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ungültige Auswahl. Wählen Sie eine gültige Zelle mit Daten aus.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Keine Zelle ausgewählt.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
