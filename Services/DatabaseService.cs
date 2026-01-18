using Microsoft.Data.Sqlite;
using PontBascule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PontBascule.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PontBascule",
                "pontbascule.db"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            _connectionString = $"Data Source={dbPath}";
        }

        public async Task InitializeDatabaseAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS Weighings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    TruckNumber TEXT NOT NULL,
                    Transporter TEXT,
                    Product TEXT,
                    Weight REAL NOT NULL,
                    WeighingType INTEGER NOT NULL,
                    SapDocumentNumber TEXT,
                    SentToSap INTEGER NOT NULL DEFAULT 0,
                    Notes TEXT
                );

                CREATE INDEX IF NOT EXISTS idx_timestamp ON Weighings(Timestamp DESC);
                CREATE INDEX IF NOT EXISTS idx_truck ON Weighings(TruckNumber);
            ";

            await createTableCommand.ExecuteNonQueryAsync();
        }

        public async Task<int> SaveWeighingAsync(Weighing weighing)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Weighings (Timestamp, TruckNumber, Transporter, Product, Weight, WeighingType, SapDocumentNumber, SentToSap, Notes)
                VALUES (@Timestamp, @TruckNumber, @Transporter, @Product, @Weight, @WeighingType, @SapDocumentNumber, @SentToSap, @Notes);
                SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("@Timestamp", weighing.Timestamp.ToString("o"));
            command.Parameters.AddWithValue("@TruckNumber", weighing.TruckNumber);
            command.Parameters.AddWithValue("@Transporter", weighing.Transporter ?? "");
            command.Parameters.AddWithValue("@Product", weighing.Product ?? "");
            command.Parameters.AddWithValue("@Weight", weighing.Weight);
            command.Parameters.AddWithValue("@WeighingType", (int)weighing.WeighingType);
            command.Parameters.AddWithValue("@SapDocumentNumber", weighing.SapDocumentNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SentToSap", weighing.SentToSap ? 1 : 0);
            command.Parameters.AddWithValue("@Notes", weighing.Notes ?? (object)DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Weighing?> GetWeighingByIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Weighings WHERE Id = @Id;
            ";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapWeighing(reader);
            }

            return null;
        }

        public async Task<List<Weighing>> GetRecentWeighingsAsync(int count = 50)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Weighings 
                ORDER BY Timestamp DESC 
                LIMIT @Count;
            ";
            command.Parameters.AddWithValue("@Count", count);

            var weighings = new List<Weighing>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                weighings.Add(MapWeighing(reader));
            }

            return weighings;
        }

        public async Task UpdateWeighingAsync(Weighing weighing)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Weighings 
                SET SapDocumentNumber = @SapDocumentNumber,
                    SentToSap = @SentToSap,
                    Notes = @Notes
                WHERE Id = @Id;
            ";

            command.Parameters.AddWithValue("@Id", weighing.Id);
            command.Parameters.AddWithValue("@SapDocumentNumber", weighing.SapDocumentNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SentToSap", weighing.SentToSap ? 1 : 0);
            command.Parameters.AddWithValue("@Notes", weighing.Notes ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        private static Weighing MapWeighing(SqliteDataReader reader)
        {
            return new Weighing
            {
                Id = reader.GetInt32(0),
                Timestamp = DateTime.Parse(reader.GetString(1)),
                TruckNumber = reader.GetString(2),
                Transporter = reader.GetString(3),
                Product = reader.GetString(4),
                Weight = (decimal)reader.GetDouble(5),
                WeighingType = (WeighingType)reader.GetInt32(6),
                SapDocumentNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                SentToSap = reader.GetInt32(8) == 1,
                Notes = reader.IsDBNull(9) ? null : reader.GetString(9)
            };
        }
    }
}
