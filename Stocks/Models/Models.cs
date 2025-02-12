using Microsoft.Data.Sqlite;

namespace Stocks.Models
{
    public class DbConnection
    {
        public static SqliteConnection Get()
        {
            return new SqliteConnection("Data Source=Data/Banco.db");
        }

        /// <summary>
        /// Apaga o banco de dados e cria do zero a partir do arquivo SQL.
        /// </summary>
        public static void Reset()
        {
            // Apaga o banco de dados.
            File.Delete("Data/Banco.db");

            // Carrega o SQL.
            string sql = File.ReadAllText("Data/banco.sql");

            using SqliteConnection conn = Get();
            conn.Open();
            SqliteCommand cmd = conn.CreateCommand();

            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }

    public enum TipoOperacaoEnum
    {
        SwingTrade = 1,
        DayTrade,
        FII,
    }

    public enum TipoEventoEnum
    {
        Desdobramento = 1,
        Grupamento,
        Bonificacao,
    }

    public enum TipoAtivoEnum
    {
        Acao = 1,
        FII,
        ETF,
    }
}
