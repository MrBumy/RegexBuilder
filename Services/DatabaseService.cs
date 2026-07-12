using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dapper;
using RegexBuilder.Models;

namespace RegexBuilder.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RegexBuilder");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            _dbPath = Path.Combine(folder, "favorites.db");
            _connectionString = $"Data Source={_dbPath}";
        }

        public async Task InitializeDatabaseAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                CREATE TABLE IF NOT EXISTS Favorites (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Pattern TEXT NOT NULL,
                    Description TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
            await connection.ExecuteAsync(sql);
        }

        public async Task<IEnumerable<FavoritePattern>> GetFavoritesAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<FavoritePattern>("SELECT * FROM Favorites ORDER BY CreatedAt DESC");
        }

        public async Task AddFavoriteAsync(FavoritePattern favorite)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "INSERT INTO Favorites (Name, Pattern, Description, CreatedAt) VALUES (@Name, @Pattern, @Description, @CreatedAt)";
            await connection.ExecuteAsync(sql, favorite);
        }

        public async Task DeleteFavoriteAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM Favorites WHERE Id = @Id", new { Id = id });
        }
    }
}
