using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DotsGame.Data;
using DotsGame.Models;

namespace DotsGame.Forms
{
    public partial class Registration : Form
    {
        private ToolTip emailToolTip = new ToolTip();

        public Registration()
        {
            InitializeComponent();
            ConfigureEmailField();
            Settings();
        }

        private void ConfigureEmailField()
        {
            txtEmail.PlaceholderText = "example@mail.ru";
            txtEmail.TextChanged += (sender, e) => ValidateEmailField();
            emailToolTip.SetToolTip(txtEmail, "Введите email в формате: user@example.com");
            emailToolTip.AutomaticDelay = 100;
            emailToolTip.AutoPopDelay = 5000;
        }
        private bool ValidatePassword(string password)
        {
            return password.Length >= 6
                   && password.Any(char.IsDigit)
                   && password.Any(char.IsUpper);
        }
        private void ValidateEmailField()
        {
            if (string.IsNullOrEmpty(txtEmail.Text))
            {
                txtEmail.BorderColor = Color.FromArgb(213, 218, 223); 
                return;
            }

            bool isValid = Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            txtEmail.BorderColor = isValid ? Color.Green : Color.Red;
        }

        private void Settings()
        {
            txtLogin.KeyPress += txtLogin_KeyPress;
            btnRegister.Click += btnRegister_Click;
            btnExit.Click += btnExit_Click;
        }

        private void Registration_Load(object sender, EventArgs e)
        {
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLogin.Text) ||
                string.IsNullOrEmpty(txtPassword.Text) ||
                string.IsNullOrEmpty(txtPassword2.Text) ||
                string.IsNullOrEmpty(txtEmail.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ValidatePassword(txtPassword.Text))
            {
                MessageBox.Show("Пароль должен быть:\n• Не менее 6 символов\n• Содержать цифру\n• Содержать заглавную букву",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Некорректный email! Пример: user@example.com", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtPassword.Text != txtPassword2.Text)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    if (db.Users.Any(u => u.Login == txtLogin.Text))
                    {
                        MessageBox.Show("Логин уже занят!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var user = new User
                    {
                        Login = txtLogin.Text,
                        Email = txtEmail.Text
                    };
                    user.SetPassword(txtPassword.Text);

                    db.Users.Add(user);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация успешна!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != '\b')
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
    }
}