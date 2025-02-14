﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Urlaubsplanung.Enums;

namespace Urlaubsplanung
{
    public partial class FormMitarbeiter : Form
    {
        SqlCommand cmd;
        SqlConnection cn;
        SqlDataReader dr;

        int MitarbeiterID;
        int Urlaubsanspruch;
        int Fehlstunden;

        public FormMitarbeiter(int mitarbeiterID)
        {
            InitializeComponent();
            MitarbeiterID = mitarbeiterID;
        }

        private void FormMitarbeiter_Load(object sender, EventArgs e)
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


            LoadBoldedDates();
            LoadUserLabel();
            LoadUrlaubsanspruchLabel();
            LoadFehlstundenLabel();
            LoadResturlaubLabel();
        }

        // Beantragten Urlaub im Kalender fett formatieren
        private void LoadBoldedDates()
        {
            string query = "SELECT DatumBeginn, DatumEnde, Status FROM Urlaubsantrag WHERE MitarbeiterID = @MitarbeiterID";


            SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@MitarbeiterID", MitarbeiterID);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                DateTime DatumBeginn = dr.GetDateTime(0);
                DateTime DatumEnde = dr.GetDateTime(1);
                EnumStatus.Status status = (EnumStatus.Status)dr.GetInt32(2);

                if (status == EnumStatus.Status.Ausstehend || status == EnumStatus.Status.Genehmigt)
                {
                    for (DateTime date = DatumBeginn; date <= DatumEnde; date = date.AddDays(1))
                    {
                        monthCalendar1.AddBoldedDate(date);
                    }
                }
            }

            dr.Close();
            monthCalendar1.UpdateBoldedDates();
        }

        // Angemeldeten Mitarbeiter anzeigen
        private void LoadUserLabel()
        {
            string query = "SELECT Name FROM Mitarbeiter WHERE MitarbeiterID = @MitarbeiterID";

            SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@MitarbeiterID", MitarbeiterID);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                label5.Text = dr.GetString(0);
            }
            dr.Close();
        }

        // Urlaubsanspruch anzeigen
        private void LoadUrlaubsanspruchLabel()
        {
            string query = "SELECT Urlaubsanspruch FROM Mitarbeiter WHERE MitarbeiterID = @MitarbeiterID";
            SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@MitarbeiterID", MitarbeiterID);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Urlaubsanspruch = Convert.ToInt32(dr.GetDouble(0));
                label6.Text = dr.GetDouble(0).ToString() + " h";
            }
            dr.Close();
        }

        // Fehlstunden anzeigen
        private void LoadFehlstundenLabel()
        {
            string query = "SELECT Fehlstunden FROM Mitarbeiter WHERE MitarbeiterID = @MitarbeiterID";
            SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@MitarbeiterID", MitarbeiterID);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Fehlstunden = Convert.ToInt32(dr.GetDouble(0));
                label7.Text = dr.GetDouble(0).ToString() + " h";
            }
            dr.Close();
        }

        // Resturlaub berechnen/anzeigen
        private void LoadResturlaubLabel()
        {
            double Resturlaub = Urlaubsanspruch - Fehlstunden;

            string query = "SELECT Urlaubsantrag.DatumBeginn, Urlaubsantrag.DatumEnde, Urlaubsantrag.Status FROM Urlaubsantrag WHERE Urlaubsantrag.MitarbeiterID = @MitarbeiterID AND Urlaubsantrag.Status = @StatusGenehmigt";

            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@MitarbeiterID", MitarbeiterID);
                cmd.Parameters.AddWithValue("@StatusGenehmigt", (int)EnumStatus.Status.Genehmigt);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        DateTime datumBeginn = dr.GetDateTime(0);
                        DateTime datumEnde = dr.GetDateTime(1);

                        for (DateTime date = datumBeginn; date <= datumEnde; date = date.AddDays(1))
                        {
                            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                                continue; 

                            if (date.DayOfWeek == DayOfWeek.Friday)
                                Resturlaub -= 5.50;
                            else
                                Resturlaub -= 8.25;
                        }
                    }
                }
            }

            label8.Text = Resturlaub.ToString("0.##") + " h"; 
        }

        private void LoadGenehmigteUrlTab()
        {
            // MitarbeiterID finden für eingeloggten Mitarbeiter
            // Urlaubsanträge vom Mitarbeiter finden mit Status
            // Falls Status "Genehmigt", in Tabelle eintragen
            // Tabelle: Grund | Datum von bis
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormAntrag login = new FormAntrag(MitarbeiterID);
            login.ShowDialog();
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            
        }
    }
}
