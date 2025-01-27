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
                        targetRow["UrlaubsantragID"] = row["UrlaubsantragID"];

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
                            targetRow["KW" + kw] = datumBeginn.ToShortDateString() + " - " + datumEnde.ToShortDateString();
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
                dataGridView1.Columns["UrlaubsantragID"].Visible = true;
            }

            // Spalte "Name" an der Seite fixieren
            this.dataGridView1.Columns["Name"].Frozen = true;

            // Spalte "Urlaubsanspruch" an der Seite fixieren und Spaltenbreite autom. anpassen
            this.dataGridView1.Columns["Urlaubsanspruch"].Frozen = true;
            DataGridViewColumn columnUrlaubsanspruch = dataGridView1.Columns[1];
            columnUrlaubsanspruch.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            // Spalte "Fehlstunden" an der Seite fixieren und Spaltenbreite autom. anpassen
            this.dataGridView1.Columns["Fehlstunden"].Frozen = true;
            DataGridViewColumn columnFehlstunden = dataGridView1.Columns[2];
            columnFehlstunden.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            LoadExistingStatusColors();
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
            string sqlCommand = String.Concat("SELECT Mitarbeiter.MitarbeiterID, Mitarbeiter.Name, Mitarbeiter.Urlaubsanspruch, Mitarbeiter.Fehlstunden, Urlaubsantrag.UrlaubsantragID, Urlaubsantrag.DatumBeginn, Urlaubsantrag.DatumEnde, Urlaubsantrag.Grund, Urlaubsantrag.Status FROM ", tableName, " FULL OUTER JOIN Urlaubsantrag ON Mitarbeiter.MitarbeiterID = Urlaubsantrag.MitarbeiterID");

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
                int mitarbeiterID = GetMitarbeiterID();

                DateTime datumBeginn = DateTime.MinValue;
                DateTime datumEnde = DateTime.MaxValue;

                var result = DateParsing();
                if (result.HasValue) 
                {
                    datumBeginn = result.Value.Item1;
                    datumEnde = result.Value.Item2;
                }

                int urlaubsantragID = GetUrlaubsantragID(mitarbeiterID, datumBeginn, datumEnde);
                UpdateStatusAndCell(urlaubsantragID, EnumStatus.Status.Genehmigt, Color.LightGreen, "Status aktualisiert");
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
                int mitarbeiterID = GetMitarbeiterID();

                DateTime datumBeginn = DateTime.MinValue;
                DateTime datumEnde = DateTime.MaxValue;

                var result = DateParsing();
                if (result.HasValue)
                {
                    datumBeginn = result.Value.Item1;
                    datumEnde = result.Value.Item2;
                }

                int urlaubsantragID = GetUrlaubsantragID(mitarbeiterID, datumBeginn, datumEnde);
                UpdateStatusAndCell(urlaubsantragID, EnumStatus.Status.Abgelehnt, Color.Red, "Status aktualisiert");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusAndCell(int urlaubsantragID, EnumStatus.Status status, Color color, string successMessage)
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
                    return;
                }
            }

            // Zelle einfärben
            if (dataGridView1.CurrentRow != null)
            {
                var cell = dataGridView1.CurrentCell;

                if (cell.Value != null && DateTime.TryParse(cell.Value.ToString().Split('-')[0], out _))
                {
                    cell.Style.BackColor = color;
                    MessageBox.Show(successMessage, "Meldung", MessageBoxButtons.OK, MessageBoxIcon.Information);
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


        // MitarbeiterID bekommen
        private int GetMitarbeiterID()
        {
            // Von Zeile Name holen
            string sCurrName = dataGridView1.CurrentRow.Cells["Name"].Value.ToString();

            string query = "SELECT MitarbeiterID FROM Mitarbeiter WHERE Name = @Name";
            
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("Name", sCurrName);                

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    int mitarbeiterID = Convert.ToInt32(result);
                    return mitarbeiterID;
                }
            }
            return 0;
        }

        private (DateTime, DateTime)? DateParsing()
        {
            string query = "SELECT DatumBeginn, DatumEnde FROM Urlaubsantrag WHERE DatumBeginn = @DatumBeginn AND DatumEnde = @DatumEnde";

            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                string spaltenName = dataGridView1.CurrentCell.OwningColumn.Name;

                if (dataGridView1.CurrentRow.Cells[spaltenName].Value != null)
                {
                    string datumString = dataGridView1.CurrentRow.Cells[spaltenName].Value.ToString();
                    string[] datumsArray = datumString.Split('-');

                    if (datumsArray.Length == 2 &&
                        DateTime.TryParse(datumsArray[0].Trim(), out DateTime datumBeginnGeparst) &&
                        DateTime.TryParse(datumsArray[1].Trim(), out DateTime datumEndeGeparst))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@DatumBeginn", datumBeginnGeparst);
                        cmd.Parameters.AddWithValue("@DatumEnde", datumEndeGeparst);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {

                            if (dr.Read())
                            {
                                DateTime datumBeginn = dr.GetDateTime(0);
                                DateTime datumEnde = dr.GetDateTime(1);
                                return (datumBeginn, datumEnde);
                            }
                        }
                    }                   
                }
                
            }
            return null;
        }



        private int GetUrlaubsantragID(int mitarbeiterID, DateTime datumBeginn, DateTime datumEnde)
        {
            string query = "SELECT UrlaubsantragID FROM Urlaubsantrag WHERE MitarbeiterID = @MitarbeiterID AND DatumBeginn = @DatumBeginn AND DatumEnde = @DatumEnde";

            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("MitarbeiterID", mitarbeiterID);
                cmd.Parameters.AddWithValue("DatumBeginn", datumBeginn);
                cmd.Parameters.AddWithValue("DatumEnde", datumEnde);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    int urlaubsantragID = Convert.ToInt32(dr["UrlaubsantragID"]);
                    dr.Close();
                    return urlaubsantragID;
                }
            }
            return 0;
        }

        // Übersicht Verwaltung je nach Antragsstatus einfärben beim Laden
        private void LoadExistingStatusColors()
        {
            string query = "SELECT UrlaubsantragID, DatumBeginn, DatumEnde, Status FROM Urlaubsantrag";

            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                SqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    DateTime datumBeginn = reader.GetDateTime(reader.GetOrdinal("DatumBeginn"));
                    DateTime datumEnde = reader.GetDateTime(reader.GetOrdinal("DatumEnde"));
                    int status = reader.GetInt32(reader.GetOrdinal("Status"));

                    // Kalenderwochen berechnen
                    int kwBeginn = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(datumBeginn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    int kwEnde = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(datumEnde, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                    // Farbe basierend auf Status
                    Color cellColor = Color.White;
                    if (status == (int)EnumStatus.Status.Genehmigt)
                    {
                        cellColor = Color.LightGreen;
                    }
                    else if (status == (int)EnumStatus.Status.Abgelehnt)
                    {
                        cellColor = Color.Red;
                    }
                    else if (status == (int)EnumStatus.Status.Ausstehend)
                    {
                        cellColor = Color.LightGray;
                    }

                    // Spalten und Zellen im DataGridView einfärben
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        for (int kw = kwBeginn; kw <= kwEnde; kw++)
                        {
                            string columnName = "KW" + kw;
                            if (dataGridView1.Columns.Contains(columnName))
                            {
                                var cell = row.Cells[columnName];
                                if (cell != null && cell.Value != null && cell.Value.ToString().Contains(datumBeginn.ToShortDateString()))
                                {
                                    cell.Style.BackColor = cellColor;
                                    cell.Tag = (EnumStatus.Status)status;
                                }
                            }
                        }
                    }
                }
                reader.Close();
            }            
        }
    }
}