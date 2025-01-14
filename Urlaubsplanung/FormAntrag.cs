﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Urlaubsplanung.Enums;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Urlaubsplanung
{
    public partial class FormAntrag : Form
    {
        SqlCommand cmd;
        SqlConnection cn;
        SqlDataReader dr;
        public FormAntrag()
        {
            InitializeComponent();
        }
        private void FormAntrag_Load(object sender, EventArgs e)
        {
            using (cn)
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

                SqlCommand cmd = new SqlCommand("SELECT Name FROM Mitarbeiter", cn);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    comboBox1.Items.Add(dr["Name"].ToString());
                }
            dr.Close();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {   using (cn)
            {
                cmd = new SqlCommand(
                "SELECT MitarbeiterID FROM Mitarbeiter WHERE Name = @Name", cn);
                cmd.Parameters.AddWithValue("@Name", comboBox1.Text);

                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    int? nMitarbeiterID = dr.GetInt32(0);

                    dr.Close();

                    cmd = new SqlCommand(
                        "INSERT INTO Urlaubsantrag (MitarbeiterID, DatumBeginn, DatumEnde, Grund, Status) VALUES (@ID, @DatumBeginn, @DatumEnde, @Grund, @Status)", cn);

                    cmd.Parameters.AddWithValue("@ID", nMitarbeiterID);
                    cmd.Parameters.AddWithValue("@DatumBeginn", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@DatumEnde", dateTimePicker2.Value);
                    cmd.Parameters.AddWithValue("@Grund", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Status", 0);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Ihr Antrag wurde eingereicht.", "Meldung", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    dr.Close();

                    MessageBox.Show("Mitarbeiter nicht gefunden.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormMitarbeiter formMitarbeiter = new FormMitarbeiter();
            formMitarbeiter.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
    }
}

