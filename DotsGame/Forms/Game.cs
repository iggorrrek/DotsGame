using DotsGame.Models;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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

        private Socket _socket;
        private bool _isHost = false;
        private bool _isMyTurn = false;
        private string _opponentName = "Ожидание игрока...";

        public Game(User user, bool isHost = false, string opponentIP = null)
        {
            _currentUser = user;
            _isHost = isHost;
            InitializeComponent();
            InitializeForm();
            InitializePlayersPanel();
            InitializeGameBoard();
            InitializeControls();

            if (isHost) StartServer();
            else if (!string.IsNullOrEmpty(opponentIP)) ConnectToServer(opponentIP);
        }

        private void InitializeForm()
        {
            this.Text = $"DotsGame - {_currentUser.Login}";
            this.ClientSize = new Size(1200, 750);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void Game_Load(object sender, EventArgs e) { }

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
                Text = _opponentName,
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

        private void StartServer()
        {
            Task.Run(() =>
            {
                try
                {
                    var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    listener.Bind(new IPEndPoint(IPAddress.Any, 8888));
                    listener.Listen(1);

                    _socket = listener.Accept();
                    _isMyTurn = true;
                    SendPlayerName();
                    BeginReceiveData();
                }
                catch (SocketException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            });
        }

        private void ConnectToServer(string ip)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(new IPEndPoint(IPAddress.Parse(ip), 8888));
            _isMyTurn = false;
            SendPlayerName();
            BeginReceiveData();
        }

        private void SendPlayerName()
        {
            if (_socket == null || !_socket.Connected) return;
            string message = $"NAME:{_currentUser.Login}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            _socket.Send(data);
        }

        private void BeginReceiveData()
        {
            Task.Run(() =>
            {
                byte[] buffer = new byte[1024];
                while (_socket != null && _socket.Connected)
                {
                    try
                    {
                        int bytesRead = _socket.Receive(buffer);
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessReceivedData(receivedData);
                    }
                    catch (SocketException) { break; }
                }
            });
        }

        private void ProcessReceivedData(string data)
        {
            if (data.StartsWith("NAME:"))
            {
                _opponentName = data.Substring(5);
                UpdateOpponentName();
            }
            else
            {
                var parts = data.Split(',');
                if (parts.Length == 4 && int.TryParse(parts[0], out int x1))
                {
                    DrawOpponentMove(x1, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                    _isMyTurn = true;
                }
            }
        }

        private void UpdateOpponentName()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateOpponentName));
                return;
            }
            player2Label.Text = _opponentName;
        }

        private void DrawOpponentMove(int x1, int y1, int x2, int y2)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DrawOpponentMove(x1, y1, x2, y2)));
                return;
            }
            foreach (Control control in gameBoard.Controls)
            {
                if (control is Guna2CircleButton dot && dot.Tag is Point pos && pos.X == x1 && pos.Y == y1)
                {
                    dot.FillColor = Color.Blue; 
                    break;
                }
            }
        }

        private void SendMove(int x1, int y1, int x2, int y2)
        {
            if (_socket == null || !_socket.Connected) return;
            string move = $"{x1},{y1},{x2},{y2}";
            byte[] data = Encoding.UTF8.GetBytes(move);
            _socket.Send(data);
        }

        private void Dot_Click(object sender, EventArgs e)
        {
            if (!_isMyTurn) return;

            var dot = (Guna2CircleButton)sender;
            var position = (Point)dot.Tag;
            dot.FillColor = Color.Red;
            SendMove(position.X, position.Y, position.X, position.Y); 
            _isMyTurn = false;
        }

        private void UpdateScore(int player1Score, int player2Score)
        {
            scoreLabel.Text = $"{player1Score} : {player2Score}";
        }

        private void SurrenderGame()
        {
            if (MessageBox.Show("Вы уверены, что хотите сдаться?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _socket?.Close();
                this.Close();
            }
        }

        private void UndoMove()
        {
            MessageBox.Show("Ход отменён", "Информация");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _socket?.Close();
            base.OnFormClosing(e);
        }
    }
}