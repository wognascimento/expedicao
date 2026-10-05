using Dapper;
using Expedicao;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

SQLitePCL.Batteries_V2.Init();
using var db = new DapperDatabase(new SqliteConnection("Data Source=:memory:"));
await db.Connection.OpenAsync();
await db.Connection.ExecuteAsync("CREATE TABLE registros(codigo INTEGER PRIMARY KEY AUTOINCREMENT, texto TEXT, data TEXT)");
var row = new Registro { Nome = "D'Avila", Data = new DateTime(2026, 10, 2) };
await db.InsertAsync(row);
Check(row.Id > 0, "generated key");
var read = (await db.ReadAsync<Registro>("\"Nome\" = @nome", new { nome = row.Nome })).Single();
Check(read.Nome == row.Nome && read.Data == row.Data, "aliases and date roundtrip");
row.Nome = null;
await db.UpdateAsync(row, nameof(Registro.Nome));
Check((await db.ReadAsync<Registro>()).Single().Nome == null, "null update");
Check((await db.ReadAsync<Registro>("\"Nome\" = @nome", new { nome = "' OR 1=1 --" })).Count == 0, "parameterized filter");
using (var transaction = await db.Connection.BeginTransactionAsync())
{
    await db.Connection.ExecuteAsync("UPDATE registros SET texto = 'rollback'", transaction: transaction);
    await transaction.RollbackAsync();
}
Check((await db.ReadAsync<Registro>()).Single().Nome == null, "rollback");
try
{
    await db.UpdateAsync(new Registro { Id = 999, Nome = "Missing" });
    throw new Exception("Missing row accepted.");
}
catch (InvalidOperationException) { Console.WriteLine("PASS missing row rejected"); }
Check(!DapperDatabase.SelectSql<Registro>().Contains("Ignorado"), "NotMapped excluded");
Console.WriteLine("PASS Dapper regression. No production database accessed.");
static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }

[Table("registros")]
public class Registro
{
    [Key, Column("codigo")] public long? Id { get; set; }
    [Column("texto")] public string? Nome { get; set; }
    [Column("data")] public DateTime? Data { get; set; }
    [NotMapped] public string? Ignorado { get; set; }
}

namespace Expedicao
{
    // Connection settings are intentionally unavailable in this isolated test executable.
    public class DataBase
    {
        public static DataBase Instance => throw new InvalidOperationException("Production settings forbidden in tests.");
        public string CaminhoSistema => "";
        public string ConnectionString => "";
    }
}
