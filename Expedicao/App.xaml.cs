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
        public string CurrentVersion => CURRENT_VERSION;

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
            StyleManager.ApplicationTheme = new Office2016Theme();
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
        private bool checkingForUpdates;
        public async Task CheckForUpdatesAsync(bool showUpToDate = false)
        {
            if (checkingForUpdates) return;
            checkingForUpdates = true;
            var showErrors = showUpToDate;
            try
            {
                if (string.IsNullOrWhiteSpace(BaseSettings.UpdateInfoUrl))
                    throw new InvalidOperationException("Endereco de atualizacao nao configurado.");
                var update = await new UpdateChecker(BaseSettings.UpdateInfoUrl, CURRENT_VERSION).CheckForUpdatesAsync();
                if (update == null)
                {
                    if (showUpToDate)
                        MessageBox.Show($"O sistema ja esta atualizado.\nVersao: {CURRENT_VERSION}", "Atualizacao");
                    return;
                }
                if (MessageBox.Show($"Nova versao: {update.updateVersion}\n\n{string.Join("\n", update.changelog)}\n\nDeseja atualizar agora?",
                    "Atualizacao disponivel", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    return;
                showErrors = true;
                var executable = System.IO.Path.Combine(AppContext.BaseDirectory, "Update.exe");
                if (!System.IO.File.Exists(executable))
                    throw new System.IO.FileNotFoundException("O atualizador Update.exe nao foi encontrado.");
                var start = new ProcessStartInfo(executable)
                {
                    UseShellExecute = true,
                    WorkingDirectory = AppContext.BaseDirectory
                };
                start.ArgumentList.Add(JsonSerializer.Serialize(update));
                start.ArgumentList.Add("Expedicao.exe");
                if (Process.Start(start) == null)
                    throw new InvalidOperationException("Nao foi possivel iniciar o atualizador.");
                Shutdown();
            }
            catch (Exception ex)
            {
                if (showErrors) MessageBox.Show(ex.Message, "Erro ao atualizar", MessageBoxButton.OK, MessageBoxImage.Error);
                else Trace.TraceWarning("Falha na verificacao automatica: {0}", ex.Message);
            }
            finally
            {
                checkingForUpdates = false;
            }
        }
    }
}
