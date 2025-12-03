
namespace Expedicao
{
    public sealed class DataBase
    {
        private static readonly DataBase instance = new();
        public string? Host { get; set; }
        public string? Database { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConnectionString { get; set; }
        public string CaminhoSistema { get; set; } = $@"C:\SIG\Expedicao S.I.G\";
        private DataBase() { }
        public static DataBase Instance => DataBase.instance;
    }
}
