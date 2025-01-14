using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
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
        public FormMitarbeiter()
        {
            InitializeComponent();
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormAntrag login = new FormAntrag();
            login.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
