using Microsoft.Data.Sqlite;
using SafeVault.Web.Models;

namespace SafeVault.Web.Data;
public sealed class UserRepository
{
    private readonly string _connectionString;
    public UserRepository(IConfiguration config)
    {
        var configured = config.GetConnectionString("SafeVault");
        _connectionString = !string.IsNullOrEmpty(configured)
            ? configured : "Data Source=safevault.db";
    }
    private SqliteConnection OpenConnection() => new(_connectionString);
    public async Task InitializeAsync()
    {
        using var db = OpenConnection(); await db.OpenAsync();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
          CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL COLLATE NOCASE UNIQUE,
            Email TEXT NOT NULL COLLATE NOCASE UNIQUE,
            PasswordHash TEXT NOT NULL,
            Role TEXT NOT NULL CHECK (Role IN ('User','Admin'))
          );
          """;
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task<VaultUser?> FindByUsernameAsync(string username)
    {
        using var db = OpenConnection(); await db.OpenAsync();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, Email, PasswordHash, Role FROM Users WHERE Username = $username LIMIT 1";
        cmd.Parameters.AddWithValue("$username", username);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return new VaultUser {
            Id = reader.GetInt32(0), Username = reader.GetString(1), Email = reader.GetString(2),
            PasswordHash = reader.GetString(3), Role = reader.GetString(4)
        };
    }
    public async Task<bool> CreateAsync(VaultUser user)
    {
        using var db = OpenConnection(); await db.OpenAsync();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Username,Email,PasswordHash,Role) VALUES ($name,$email,$hash,$role)";
        cmd.Parameters.AddWithValue("$name", user.Username);
        cmd.Parameters.AddWithValue("$email", user.Email);
        cmd.Parameters.AddWithValue("$hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("$role", user.Role);
        try { await cmd.ExecuteNonQueryAsync(); return true; }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) { return false; }
    }
    public async Task<IReadOnlyList<VaultUser>> ListAsync()
    {
        var result = new List<VaultUser>();
        using var db = OpenConnection(); await db.OpenAsync();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, Email, Role FROM Users ORDER BY Username";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new VaultUser {
            Id = reader.GetInt32(0), Username = reader.GetString(1), Email = reader.GetString(2), Role = reader.GetString(3)
        });
        return result;
    }
}
