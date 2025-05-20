using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotsGame.Forms
{
    public partial class Registration : Form
    {
        public Registration()
        {
            InitializeComponent();
            Settttings();
        }
        private void Settttings()
        {
            txtLogin.KeyPress += txtLogin_KeyPress;
        }
        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar != '\b' && !((e.KeyChar >= 'A' && e.KeyChar <= 'Z') ||
                                      (e.KeyChar >= 'a' && e.KeyChar <= 'z')))
            {
                e.Handled = true;
            }
        }
        private void txtPassword_IconRightClick_1(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            txtPassword.PasswordChar = txtPassword.UseSystemPasswordChar ? '*' : '\0';
        }
        private void txtPassword2_IconRightClick_1(object sender, EventArgs e)
        {
            txtPassword2.UseSystemPasswordChar = !txtPassword2.UseSystemPasswordChar;
            txtPassword2.PasswordChar = txtPassword2.UseSystemPasswordChar ? '*' : '\0';
        }

        private void Registration_Load(object sender, EventArgs e)
        {

        }
    }
}
