using DotsGame.Data;
using DotsGame.Models;
using Guna.UI2.WinForms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DotsGame.Forms
{
    public partial class Main : Form
    {
        private User _currentUser;
        private Guna2CirclePictureBox avatarPictureBox;
        private Label lblGamesCount;
        private Label lblWinsCount;
        private Guna2DataGridView historyGridView;
        private Guna2Button btnChangeAvatar;
        private Guna2Button btnBack;

        public Main(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            InitializeComponent();
            InitializeUI();
            EnsureAvatarsDirectoryExists();
            LoadUserData();
        }

        private void EnsureAvatarsDirectoryExists()
        {
            try
            {
                var avatarsDir = Path.Combine(Application.StartupPath, "Avatars");
                if (!Directory.Exists(avatarsDir))
                {
                    Directory.CreateDirectory(avatarsDir);
                    Console.WriteLine($"Created Avatars directory at: {avatarsDir}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Avatars directory: {ex.Message}");
            }
        }

        private void InitializeUI()
        {
            this.Text = $"Профиль игрока - {_currentUser?.Login ?? "Неизвестный"}";
            this.ClientSize = new Size(600, 500);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            avatarPictureBox = new Guna2CirclePictureBox()
            {
                Size = new Size(120, 120),
                Location = new Point(30, 30),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            btnChangeAvatar = new Guna2Button()
            {
                Text = "Сменить аватар",
                Size = new Size(120, 30),
                Location = new Point(30, 160),
                Font = new Font("Segoe UI", 9),
                BorderRadius = 5
            };
            btnChangeAvatar.Click += BtnChangeAvatar_Click;

            var statsPanel = new Guna2Panel()
            {
                Size = new Size(400, 80),
                Location = new Point(170, 30),
                BackColor = Color.White,
                BorderRadius = 10,
                ShadowDecoration = { Enabled = true }
            };

            lblGamesCount = new Label()
            {
                Text = "Количество игр: 0",
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblWinsCount = new Label()
            {
                Text = "Количество побед: 0",
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 50),
                AutoSize = true
            };

            statsPanel.Controls.Add(lblGamesCount);
            statsPanel.Controls.Add(lblWinsCount);

            var historyLabel = new Label()
            {
                Text = "История игр",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(30, 210),
                AutoSize = true
            };

            historyGridView = new Guna2DataGridView()
            {
                Size = new Size(540, 200),
                Location = new Point(30, 240),
                BackgroundColor = Color.White,
                ColumnHeadersHeight = 30,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            historyGridView.DefaultCellStyle.BackColor = Color.White;
            historyGridView.DefaultCellStyle.SelectionBackColor = Color.LightSteelBlue;
            historyGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            historyGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            historyGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10);

            historyGridView.Columns.Add("Date", "Дата");
            historyGridView.Columns.Add("Opponent", "Соперник");
            historyGridView.Columns.Add("Result", "Результат");
            historyGridView.Columns.Add("Score", "Счёт");

            btnBack = new Guna2Button()
            {
                Text = "Назад",
                Size = new Size(100, 40),
                Location = new Point(470, 450),
                Font = new Font("Segoe UI", 10),
                BorderRadius = 5
            };
            btnBack.Click += BtnBack_Click;

            this.Controls.AddRange(new Control[] {
                avatarPictureBox,
                btnChangeAvatar,
                statsPanel,
                historyLabel,
                historyGridView,
                btnBack
            });
        }

        private void LoadUserData()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users
                        .Include(u => u.Games)
                        .FirstOrDefault(u => u.Id == _currentUser.Id);

                    if (user != null)
                    {
                        _currentUser = user;
                        LoadAvatar();

                        lblGamesCount.Text = $"Количество игр: {user.Games.Count}";
                        lblWinsCount.Text = $"Количество побед: {user.Games.Count(g => g.IsWinner)}";

                        historyGridView.Rows.Clear();
                        foreach (var game in user.Games.OrderByDescending(g => g.Date))
                        {
                            historyGridView.Rows.Add(
                                game.Date.ToString("dd.MM.yyyy"),
                                game.OpponentName,
                                game.IsWinner ? "Победа" : "Поражение",
                                $"{game.PlayerScore}:{game.OpponentScore}"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                lblGamesCount.Text = "Количество игр: 0";
                lblWinsCount.Text = "Количество побед: 0";
                historyGridView.Rows.Clear();
            }
        }

        private void LoadAvatar()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentUser.AvatarPath) && File.Exists(_currentUser.AvatarPath))
                {
                    avatarPictureBox.Image = Image.FromFile(_currentUser.AvatarPath);
                }
                else
                {
                    avatarPictureBox.Image = CreateDefaultAvatar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки аватара: {ex.Message}");
                avatarPictureBox.Image = CreateDefaultAvatar();
            }
        }

        private Bitmap CreateDefaultAvatar()
        {
            try
            {
                var bmp = new Bitmap(120, 120);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.LightGray);
                    using (var font = new Font("Arial", 48))
                    {
                        g.DrawString(
                            _currentUser?.Login?[0].ToString() ?? "?",
                            font,
                            Brushes.White,
                            new PointF(30, 20));
                    }
                }
                return bmp;
            }
            catch
            {
                return new Bitmap(120, 120);
            }
        }

        private void BtnChangeAvatar_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var avatarsDir = Path.Combine(Application.StartupPath, "Avatars");
                        var newPath = Path.Combine(avatarsDir, $"avatar_{_currentUser.Id}{Path.GetExtension(openFileDialog.FileName)}");

                        File.Copy(openFileDialog.FileName, newPath, true);

                        using (var db = new AppDbContext())
                        {
                            var user = db.Users.Find(_currentUser.Id);
                            if (user != null)
                            {
                                user.AvatarPath = newPath;
                                db.SaveChanges();
                                _currentUser.AvatarPath = newPath;
                                LoadAvatar();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при смене аватара: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}