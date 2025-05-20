namespace DotsGame
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            SettingsUnits();
        }
        private char passwordMaskChar = '●';
        private bool isPasswordHidden = true; 
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
        private void txtPassword_IconLeftClick(object sender, EventArgs e)
        {
            if (isPasswordHidden)
            {
                txtPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = passwordMaskChar;
            }

            isPasswordHidden = !isPasswordHidden;
        }
        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}
