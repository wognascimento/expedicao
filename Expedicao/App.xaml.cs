using BibliotecasSIG;
using Expedicao.Localization;
using System;
using System.Globalization;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Telerik.Windows.Controls;

namespace Expedicao
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private readonly string CURRENT_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private readonly DataBase BaseSettings = DataBase.Instance;

        public App()
        {
            BaseSettings.LoadFromConfiguration();
            var culture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            LocalizationManager.Manager = new SigTelerikLocalizationManager();
            StyleManager.ApplicationTheme = new Windows11Theme();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            /*
            CultureInfo culture = new("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            */
            // Verificação de atualização em segundo plano
            await CheckForUpdatesAsync();
        }
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BaseSettings.UpdateInfoUrl))
                    return;

                var updateChecker = new UpdateChecker(BaseSettings.UpdateInfoUrl, CURRENT_VERSION);
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                var updateInfoJson = JsonSerializer.Serialize<UpdateInfo>(updateInfo);

                if (updateInfo != null)
                {
                    // Pergunta ao usuário se deseja atualizar
                    var result = MessageBox.Show(
                        $"Nova versão disponível!\n\n" +
                        $"Versão atual: {CURRENT_VERSION}\n" +
                        $"Nova versão: {updateInfo.updateVersion}\n\n" +
                        "Changelog:\n" +
                        string.Join("\n", updateInfo.changelog) +
                        "\n\nDeseja baixar a atualização?",
                        "Atualização Disponível",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {

                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string jsonString = JsonSerializer.Serialize(updateInfo, options);

                        //Process.Start("Update.exe", @$"{updateInfoJson}, Expedicao.exe");

                        string jsonData = JsonSerializer.Serialize(updateInfo); // Garante que o JSON está bem formatado
                        string appName = "Expedicao.exe";

                        string arguments = $"\"{jsonData.Replace("\"", "\\\"")}\" \"{appName}\"";
                        Process.Start("Update.exe", arguments);
                        this.Shutdown();

                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log do erro ou tratamento de exceção
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
