using System;
using System.Windows;

namespace Expedicao
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjQyN0AzMjMwMkUzMzJFMzBPcFQ2bVNrT1RpOWNSQlo5cDduRG83STVVUlhGcmcrNnRKOU9GTk5JV2o0PQ==");

            DataBase BaseSettings = DataBase.Instance;
            BaseSettings.Database = DateTime.Now.Year.ToString();
            BaseSettings.Host = "192.168.0.23";
            BaseSettings.Username = Environment.UserName;
            BaseSettings.Password = "123mudar";
        }
    }
}
