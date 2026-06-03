using System;
using Telerik.Windows.Controls;
using System.Windows;
using System.Configuration;
using System.DirectoryServices.AccountManagement;

namespace Expedicao
{
    /// <summary>
    /// Interação lógica para Login.xam
    /// </summary>
    public partial class Login : RadWindow
    {
        private readonly DataBase BaseSettings = DataBase.Instance;

        public Login()
        {
            InitializeComponent();
            txtLogin.Focus();
        }

        private void OnSair(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OnLogar(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(txtLogin.Text) && !string.IsNullOrWhiteSpace(txtSenha.Password))
            {
                try
                {
                    using var ctx = new PrincipalContext(
                        ContextType.Domain,
                        "192.168.0.254",
                        "cipodominio.com.br");

                    if (!ctx.ValidateCredentials(txtLogin.Text, txtSenha.Password))
                        throw new Exception("Credenciais inválidas.");

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (config.AppSettings.Settings["Username"] == null)
                        config.AppSettings.Settings.Add("Username", txtLogin.Text);
                    else
                        config.AppSettings.Settings["Username"].Value = txtLogin.Text;

                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");

                    BaseSettings.Username = txtLogin.Text;
                    BaseSettings.RefreshConnectionString();

                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Falha na autenticação: {ex.Message}");
                }
            }
        }
    }
}
