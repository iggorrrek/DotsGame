using DotsGame.Forms;
namespace DotsGame
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            SettingsUnits();
        }
        private void SettingsUnits()
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
        private void txtPassword_IconRightClick(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            txtPassword.PasswordChar = txtPassword.UseSystemPasswordChar ? '*' : '\0';
        }
        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            Form registerform = new Registration();
            registerform.Owner = this;
            registerform.StartPosition = FormStartPosition.Manual;
            registerform.Location = this.Location;
            registerform.Show();
            this.LocationChanged += (s, e) => {
                registerform.Location = new Point(
                    this.Location.X + (this.Width - registerform.Width) / 2,
                    this.Location.Y + (this.Height - registerform.Height) / 2
                );
            };
            registerform.LocationChanged += (s, e) => {
                this.Location = new Point(
                    registerform.Location.X + (registerform.Width - this.Width) / 2,
                    registerform.Location.Y + (registerform.Height - this.Height) / 2
                );
            };
        }
    }
}
