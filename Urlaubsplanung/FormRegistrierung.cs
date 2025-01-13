using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Urlaubsplanung.Enums;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Urlaubsplanung
{
    public partial class FormRegistrierung : Form
    {
        SqlCommand cmd;
        SqlConnection cn;
        SqlDataReader dr;

        public FormRegistrierung()
        {
            InitializeComponent();
        }

        private void FormRegistrierung_Load(object sender, EventArgs e)
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

            comboBox1.DataSource = Enum.GetValues(typeof(EnumRolle.Rolle));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != string.Empty || textBox3.Text != string.Empty || textBox1.Text != string.Empty)
            {
                if (textBox2.Text == textBox3.Text)
                {
                    cmd = new SqlCommand("SELECT * FROM Mitarbeiter WHERE Benutzername='" + textBox1.Text + "'", cn);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        dr.Close();
                        MessageBox.Show("Der Benutzername ist bereits vergeben. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        dr.Close();
                        cmd = new SqlCommand("INSERT INTO Mitarbeiter (Name, Urlaubsanspruch, Fehlstunden, Benutzername, Passwort, Rolle) VALUES (@Name, @Urlaubsanspruch, @Fehlstunden, @Benutzername, @Passwort, @Rolle)", cn);
                        cmd.Parameters.AddWithValue("Name", textBox4.Text);
                        cmd.Parameters.AddWithValue("Urlaubsanspruch", 200);
                        cmd.Parameters.AddWithValue("Fehlstunden", 0);
                        cmd.Parameters.AddWithValue("Benutzername", textBox1.Text);
                        cmd.Parameters.AddWithValue("Passwort", textBox2.Text);
                        cmd.Parameters.AddWithValue("Rolle", comboBox1.Text);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Ihr Konto wurde erstellt. Bitte einloggen. ", "Meldung", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Die Passwörter müssen übereinstimmen.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("In alle Felder einen Wert eingeben.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
