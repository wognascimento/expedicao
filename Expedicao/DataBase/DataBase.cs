
namespace Expedicao
{
    public sealed class DataBase
    {
        private static readonly DataBase instance = new();
        public string? Host { get; set; }
        public string? Database { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        private DataBase() { }
        public static DataBase Instance => DataBase.instance;
    }
}
