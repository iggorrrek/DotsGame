using DotsGame.Data;
using DotsGame.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows.Forms;

namespace DotsGame
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowError(e.ExceptionObject as Exception);

            try
            {
                if (!InitializeDatabase())
                {
                    MessageBox.Show("Не удалось инициализировать базу данных. Приложение будет закрыто.",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                ApplicationConfiguration.Initialize();
                Application.Run(new Login());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        static bool InitializeDatabase()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    if (!db.Database.CanConnect())
                    {
                        MessageBox.Show("Не удалось подключиться к базе данных. Проверьте строку подключения.",
                                      "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    db.Database.EnsureCreated();

                    if (!db.Users.Any())
                    {
                        var user = new Models.User
                        {
                            Login = "test",
                            Email = "test@example.com"
                        };
                        user.SetPassword("test123");
                        db.Users.Add(user);
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return false;
            }
        }

        static void ShowError(Exception ex)
        {
            MessageBox.Show($"Critical error: {ex?.Message}\n\n{ex?.StackTrace}",
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }
}