using DotsGame.Models;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DotsGame.Forms
{
    public partial class Game : Form
    {
        private const int BoardWidth = 39;
        private const int BoardHeight = 32;
        private const int CellSize = 15;

        private User _currentUser;
        private Guna2Panel playersPanel;
        private Guna2Panel gameBoard;
        private Guna2Panel availablePlayersPanel;
        private Guna2Button surrenderButton;
        private Guna2Button undoButton;
        private Label player1Label;
        private Label player2Label;
        private Label scoreLabel;

        public Game(User user)
        {
            _currentUser = user;
            InitializeComponent();
            InitializeForm();
            InitializePlayersPanel();
            InitializeGameBoard();
            InitializeControls();
        }

        private void InitializeForm()
        {
            this.Text = $"DotsGame - {_currentUser.Login}";
            this.ClientSize = new Size(1200, 750);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void Game_Load(object sender, EventArgs e)
        {
        }

        private void InitializePlayersPanel()
        {
            playersPanel = new Guna2Panel()
            {
                Size = new Size(250, 650),
                Location = new Point(20, 20),
                BackColor = Color.White,
                BorderRadius = 15,
                ShadowDecoration = { Enabled = true }
            };

            var playersTitle = new Label()
            {
                Text = "Игроки",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var availableTitle = new Label()
            {
                Text = "Свободны:",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 60),
                AutoSize = true
            };

            availablePlayersPanel = new Guna2Panel()
            {
                Size = new Size(210, 100),
                Location = new Point(20, 90),
                BackColor = Color.FromArgb(245, 245, 245),
                BorderRadius = 10,
                BorderThickness = 1,
                BorderColor = Color.LightGray
            };

            var profileButton = new Guna2Button()
            {
                Text = "Профиль",
                Size = new Size(200, 40),
                Location = new Point(25, 200),
                BorderRadius = 10,
                Font = new Font("Segoe UI", 10)
            };
            profileButton.Click += (s, e) =>
            {
                if (_currentUser == null)
                {
                    MessageBox.Show("Ошибка: данные пользователя не загружены");
                    return;
                }

                var mainForm = new Main(_currentUser);
                mainForm.Show();
                this.Hide();

                mainForm.FormClosed += (s, args) => this.Show();
            };

            playersPanel.Controls.Add(profileButton);
            playersPanel.Controls.AddRange(new Control[] { playersTitle, availableTitle, availablePlayersPanel });
            this.Controls.Add(playersPanel);
        }

        private void InitializeGameBoard()
        {
            var playersInfoPanel = new Guna2Panel()
            {
                Size = new Size(BoardWidth * CellSize + 40, 50),
                Location = new Point(300, 20),
                BackColor = Color.White,
                BorderRadius = 10,
                ShadowDecoration = { Enabled = true }
            };

            player1Label = new Label()
            {
                Text = _currentUser.Login,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            scoreLabel = new Label()
            {
                Text = "0 : 0",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(playersInfoPanel.Width / 2 - 20, 15),
                AutoSize = true
            };

            player2Label = new Label()
            {
                Text = "Игрок 2",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(playersInfoPanel.Width - 120, 15),
                AutoSize = true
            };

            playersInfoPanel.Controls.AddRange(new Control[] { player1Label, scoreLabel, player2Label });
            this.Controls.Add(playersInfoPanel);

            gameBoard = new Guna2Panel()
            {
                Size = new Size(BoardWidth * CellSize + 40, BoardHeight * CellSize + 40),
                Location = new Point(300, 80),
                BackColor = Color.White,
                BorderRadius = 15,
                ShadowDecoration = { Enabled = true },
                AutoScroll = true
            };

            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    var dot = new Guna2CircleButton()
                    {
                        Size = new Size(10, 10),
                        Location = new Point(20 + x * CellSize, 20 + y * CellSize),
                        FillColor = Color.LightGray,
                        Enabled = true,
                        Tag = new Point(x, y)
                    };
                    dot.Click += Dot_Click;
                    gameBoard.Controls.Add(dot);
                }
            }

            this.Controls.Add(gameBoard);
        }

        private void InitializeControls()
        {
            var controlsPanel = new Guna2Panel()
            {
                Size = new Size(BoardWidth * CellSize + 40, 60),
                Location = new Point(300, gameBoard.Bottom + 10),
                BackColor = Color.White,
                BorderRadius = 10,
                ShadowDecoration = { Enabled = true }
            };

            undoButton = new Guna2Button()
            {
                Text = "Отменить ход",
                Size = new Size(150, 40),
                Location = new Point(20, 10),
                BorderRadius = 10,
                Font = new Font("Segoe UI", 10)
            };
            undoButton.Click += (s, e) => UndoMove();

            surrenderButton = new Guna2Button()
            {
                Text = "Сдаться",
                Size = new Size(150, 40),
                Location = new Point(controlsPanel.Width - 170, 10),
                BorderRadius = 10,
                Font = new Font("Segoe UI", 10),
                FillColor = Color.FromArgb(255, 128, 128)
            };
            surrenderButton.Click += (s, e) => SurrenderGame();

            controlsPanel.Controls.AddRange(new Control[] { undoButton, surrenderButton });
            this.Controls.Add(controlsPanel);
        }

        private void Dot_Click(object sender, EventArgs e)
        {
            var dot = (Guna2CircleButton)sender;
            var position = (Point)dot.Tag;

            dot.FillColor = dot.FillColor == Color.LightGray ? Color.Red : Color.LightGray;
            UpdateScore(1, 0);
        }

        private void UpdateScore(int player1Score, int player2Score)
        {
            scoreLabel.Text = $"{player1Score} : {player2Score}";
        }

        private void SurrenderGame()
        {
            if (MessageBox.Show("Вы уверены, что хотите сдаться?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void UndoMove()
        {
            MessageBox.Show("Ход отменен", "Информация");
        }
    }
}