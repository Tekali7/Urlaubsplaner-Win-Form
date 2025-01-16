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
            string query = "SELECT DatumBeginn, DatumEnde FROM Urlaubsantrag WHERE MitarbeiterID = @MitarbeiterID";

            
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@MitarbeiterID", MitarbeiterID);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    DateTime DatumBeginn = dr.GetDateTime(0);
                    DateTime DatumEnde = dr.GetDateTime(1);

                    for (DateTime date = DatumBeginn; date <= DatumEnde; date = date.AddDays(1))
                    {
                        monthCalendar1.AddBoldedDate(date);
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
            int Resturlaub = Urlaubsanspruch - Fehlstunden;
            label8.Text = Resturlaub.ToString() + " h";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormAntrag login = new FormAntrag();
            login.ShowDialog();
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            
        }
    }
}
