using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Urlaubsplanung.Enums;

namespace Urlaubsplanung
{
    public partial class FormLogin : Form
    {
        SqlCommand cmd;
        SqlConnection cn;
        SqlDataReader dr;
        public FormLogin()
        {
            InitializeComponent();
        }

        private void FormLogin_Load(object sender, EventArgs e)
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
            }



        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty || textBox2.Text != string.Empty)
            {

                cmd = new SqlCommand("SELECT MitarbeiterID FROM Mitarbeiter WHERE Benutzername='" + textBox1.Text + "' AND Passwort='" + textBox2.Text + "'", cn);
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    int mitarbeiterID = dr.GetInt32 (0);
                    dr.Close();
                    this.Hide();
                    FormMitarbeiter formMitarbeiter = new FormMitarbeiter(mitarbeiterID);
                    formMitarbeiter.ShowDialog();
                }
                else
                {
                    dr.Close();
                    MessageBox.Show("Kein Konto mit diesem Benutzernamen verfügbar. ", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("In alle Felder einen Wert eingeben.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormRegistrierung registration = new FormRegistrierung();
            registration.ShowDialog();
        }
    }
}
