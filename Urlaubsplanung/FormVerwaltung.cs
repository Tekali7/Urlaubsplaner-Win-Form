using System;
using System.CodeDom;
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
            übersichtTable.Columns.Add("UrlaubsantragID");
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

                        // UrlaubsantragID hinzufügen
                        if (row["UrlaubsantragID"] != DBNull.Value)
                        {
                            targetRow["UrlaubsantragID"] = row["UrlaubsantragID"];
                        }

                        übersichtTable.Rows.Add(targetRow);
                    }

                    // Urlaubsdaten füllen (KWs)
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

            if (dataGridView1.Columns["UrlaubsantragID"] == null)
            {
                MessageBox.Show("UrlaubsantragID-Spalte wurde nicht geladen!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (dataGridView1.Columns["UrlaubsantragID"] != null)
            {
                dataGridView1.Columns["UrlaubsantragID"].Visible = false;
            }

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
            string sqlCommand = String.Concat("SELECT Mitarbeiter.Name, Mitarbeiter.Urlaubsanspruch, Mitarbeiter.Fehlstunden, Urlaubsantrag.UrlaubsantragID, Urlaubsantrag.DatumBeginn, Urlaubsantrag.DatumEnde, Urlaubsantrag.Grund, Urlaubsantrag.Status FROM ", tableName, " FULL OUTER JOIN Urlaubsantrag ON Mitarbeiter.MitarbeiterID = Urlaubsantrag.MitarbeiterID");

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
            try
            {
                int urlaubsantragID = GetSelectedUrlaubsantragID();
                UpdateStatus(urlaubsantragID, EnumStatus.Status.Genehmigt);
                UpdateSelectedCell(EnumStatus.Status.Genehmigt, Color.Green, "Status aktualisiert!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int urlaubsantragID = GetSelectedUrlaubsantragID();
                UpdateStatus(urlaubsantragID, EnumStatus.Status.Abgelehnt);
                UpdateSelectedCell(EnumStatus.Status.Abgelehnt, Color.Red, "Status aktualisiert!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSelectedCell(EnumStatus.Status status, Color color, string message)
        {
            if (dataGridView1.CurrentRow != null)
            {
                // Stelle sicher, dass die Spalte mit dem Datum (KW-Spalten) ausgewählt wurde
                if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.OwningColumn.Name.StartsWith("KW"))
                {
                    var cell = dataGridView1.CurrentCell;

                    // Überprüfe, ob die Zelle ein Datum enthält
                    if (cell.Value != null && DateTime.TryParse(cell.Value.ToString().Split('-')[0], out _))
                    {
                        // Setze den Status als Tag (falls später benötigt)
                        cell.Tag = status;

                        // Ändere die Hintergrundfarbe der Zelle
                        cell.Style.BackColor = color;

                        // Zeige die Erfolgsnachricht
                        MessageBox.Show(message, "Meldung", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Die ausgewählte Zelle enthält kein gültiges Datum.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Bitte wählen Sie eine gültige KW-Zelle aus.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Keine Zeile ausgewählt.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Update des enum status in der Datenbank, (0 = Ausstehend, 1 = Genehmigt, 2 = Abgelehnt)
        private void UpdateStatus(int urlaubsantragID, EnumStatus.Status status)
        {
            string query = "UPDATE Urlaubsantrag SET Status = @Status WHERE UrlaubsantragID = @UrlaubsantragID";

            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@Status", (int)status);
                cmd.Parameters.AddWithValue("@UrlaubsantragID", urlaubsantragID);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Aktualisieren des Status: " + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // UrlaubsantragID bekommen
        private int GetSelectedUrlaubsantragID()
        {
            if (dataGridView1.CurrentRow != null)
            {
                if (dataGridView1.Columns["UrlaubsantragID"] == null)
                {
                    throw new Exception("Die Spalte UrlaubsantragID konnte nicht gefunden werden.");
                }

                var cellValue = dataGridView1.CurrentRow.Cells["UrlaubsantragID"].Value;
                if (cellValue != null && int.TryParse(cellValue.ToString(), out int urlaubsantragID))
                {
                    return urlaubsantragID;
                }
            }

            throw new Exception("UrlaubsantragID konnte nicht ermittelt werden.");
        }
    }
}
