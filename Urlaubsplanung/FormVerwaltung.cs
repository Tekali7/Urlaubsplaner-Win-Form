using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            // Und hier wie das DataGridView befüllt wird:
            DataSet myData = this.HoleDatenVariante1("Mitarbeiter");
            this.dataGridView1.DataSource = myData;
            this.dataGridView1.DataMember = "Mitarbeiter";

            HinzufügenKalenderwochen();
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
            string sqlCommand = String.Concat("SELECT Name, Urlaubsanspruch, Fehlstunden, DatumBeginn, DatumEnde FROM ", tableName, " FULL OUTER JOIN Urlaubsantrag ON Mitarbeiter.MitarbeiterID = Urlaubsantrag.MitarbeiterID WHERE DatumBeginn IS NULL OR DatumBeginn IS NOT NULL");

            // Erstelle mir einen SQL-Befehl für meine DB Verbindung
            SqlDataAdapter cmd = new SqlDataAdapter(sqlCommand, cn);

            // Führe den SQL-Befehl aus, und hole die Daten aus der DB, speichere die Daten in dem DataSet
            cmd.Fill(retValue, tableName);

            // Schließe die DB Verbindung nun wieder
            // cn.Close();

            // gib die Daten zurück
            return retValue;
        }

        private void HinzufügenKalenderwochen()
        {
            for (int i = 1; i <= 52; i++)
            {
                this.dataGridView1.Columns.Add("KW" + i, "KW" + i);
            }

            // Durch jede Zeile im DataGridView iterieren
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // Stelle sicher, dass die Zeile gültige Daten hat
                if (row.Cells["DatumBeginn"].Value != DBNull.Value && row.Cells["DatumEnde"].Value != DBNull.Value)
                {
                    DateTime datumBeginn = Convert.ToDateTime(row.Cells["DatumBeginn"].Value);
                    DateTime datumEnde = Convert.ToDateTime(row.Cells["DatumEnde"].Value);

                    // Berechne die Kalenderwochen für DatumBeginn und DatumEnde
                    Calendar cal = CultureInfo.InvariantCulture.Calendar;

                    // Berechne die Kalenderwochen für den Beginn und das Ende
                    int startKW = cal.GetWeekOfYear(datumBeginn, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    int endKW = cal.GetWeekOfYear(datumEnde, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                    // Setze die entsprechenden Kalenderwochen im DataGridView
                    for (int i = startKW; i <= endKW; i++)
                    {
                        // Finde die Spalte für die aktuelle Kalenderwoche
                        DataGridViewColumn kwColumn = dataGridView1.Columns["KW" + i];
                        if (kwColumn != null)
                        {
                            row.Cells[kwColumn.Index].Value = datumBeginn.ToShortDateString() + "-" + datumEnde.ToShortDateString();
                        }
                    }
                }
            }
        }



    }
}
