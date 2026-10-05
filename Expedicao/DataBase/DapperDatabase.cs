using Dapper;
using Microsoft.Data.Sqlite;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Expedicao;

public sealed class DapperDatabase : IDisposable
{
    public DbConnection Connection { get; }
    static DapperDatabase()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        SqlMapper.AddTypeHandler(new DateHandler());
        SqlMapper.AddTypeHandler(new TimeHandler());
    }
    public DapperDatabase(bool local = false)
    {
        var settings = DataBase.Instance;
        if (local)
        {
            SQLitePCL.Batteries_V2.Init();
            Connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = System.IO.Path.Combine(settings.CaminhoSistema, "Database.db"),
                Mode = SqliteOpenMode.ReadWrite
            }.ToString());
        }
        else Connection = new NpgsqlConnection(settings.ConnectionString);
    }

    public DapperDatabase(DbConnection connection) => Connection = connection;

    public static string Quote(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";
    private static PropertyInfo[] Properties<T>() => typeof(T).GetProperties()
        .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<NotMappedAttribute>() == null).ToArray();
    private static string Column(PropertyInfo p) => Quote(p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name);
    public static string Table<T>()
    {
        var table = typeof(T).GetCustomAttribute<TableAttribute>() ?? throw new InvalidOperationException("Modelo sem tabela.");
        return (string.IsNullOrEmpty(table.Schema) ? "" : Quote(table.Schema) + ".") + Quote(table.Name);
    }
    public static string SelectSql<T>() => "SELECT " + string.Join(", ", Properties<T>()
        .Select(p => Column(p) + " AS " + Quote(p.Name))) + " FROM " + Table<T>();

    // Filters refer to the projected property names. Values must always be parameters.
    public async Task<List<T>> ReadAsync<T>(string? filter = null, object? parameters = null)
    {
        var sql = SelectSql<T>();
        if (!string.IsNullOrWhiteSpace(filter)) sql = "SELECT * FROM (" + sql + ") AS source WHERE " + filter;
        return (await Connection.QueryAsync<T>(sql, parameters, commandTimeout: 300)).ToList();
    }
    public async Task InsertAsync<T>(T model, bool generatedKey = true)
    {
        var properties = Properties<T>();
        var key = properties.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        if (key == null || key.PropertyType == typeof(string)) generatedKey = false;
        var values = properties.Where(p => !generatedKey || p != key).ToArray();
        var sql = $"INSERT INTO {Table<T>()} ({string.Join(", ", values.Select(Column))}) VALUES ({string.Join(", ", values.Select(p => "@" + p.Name))})";
        if (generatedKey)
        {
            var value = await Connection.ExecuteScalarAsync<long>(sql + " RETURNING " + Column(key!), model);
            key!.SetValue(model, Convert.ChangeType(value, Nullable.GetUnderlyingType(key.PropertyType) ?? key.PropertyType));
        }
        else await Connection.ExecuteAsync(sql, model);
    }
    public async Task UpdateAsync<T>(T model, params string[] fields)
    {
        var properties = Properties<T>();
        var key = properties.Single(p => p.GetCustomAttribute<KeyAttribute>() != null);
        if (key.GetValue(model) == null) throw new InvalidOperationException("Registro sem chave para atualizar.");
        var values = properties.Where(p => p != key && (fields.Length == 0 || fields.Contains(p.Name))).ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Nenhum campo para atualizar.");
        var sql = $"UPDATE {Table<T>()} SET {string.Join(", ", values.Select(p => Column(p) + " = @" + p.Name))} WHERE {Column(key)} = @{key.Name}";
        if (await Connection.ExecuteAsync(sql, model) != 1) throw new InvalidOperationException("Registro nao encontrado para atualizar.");
    }
    public void Dispose() => Connection.Dispose();

    private sealed class DateHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override DateTime Parse(object value) => value is DateOnly date ? date.ToDateTime(TimeOnly.MinValue) : Convert.ToDateTime(value);
        public override void SetValue(IDbDataParameter parameter, DateTime value) { parameter.DbType = DbType.DateTime; parameter.Value = value; }
    }
    private sealed class TimeHandler : SqlMapper.TypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value) => value is TimeOnly time ? time.ToTimeSpan() : value is TimeSpan span ? span : TimeSpan.Parse(value.ToString()!);
        public override void SetValue(IDbDataParameter parameter, TimeSpan value) { parameter.DbType = DbType.Time; parameter.Value = value; }
    }
}
