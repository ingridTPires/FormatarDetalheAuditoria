using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text.Json;
using Z.Dapper.Plus;

namespace FormatarDetalheAuditoria
{
    internal class Program
    {
        private const int FETCH = 100;
        private const string SQL_AUDITORIA = @"SELECT [Id] FROM [tbAuditoria] ORDER BY [Id]
                                                    OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";
        private const string SQL_DETALHES = "SELECT [PropertyName], [OldValue], [NewValue] FROM [tbAuditoriaDetalhe] WHERE [IdAuditoria] = @IdAuditoria;";
        static async Task Main(string[] args)
        {
            try
            {
                var config = BuildConfiguration();

                using var conn = new SqlConnection(config.GetConnectionString("Default"));

                var offset = 0;
                var count = 0;

                do
                {
                    var auditorias = (await conn.QueryAsync<tbAuditoria>(SQL_AUDITORIA, new { Offset = offset, Fetch = FETCH })).ToList();
                    count = auditorias.Count;

                    if (count > 0)
                    {
                        foreach (var auditoria in auditorias)
                        {
                            var detalhes = await conn.QueryAsync<tbAuditoriaDetalhe>(SQL_DETALHES, new { IdAuditoria = auditoria.Id });

                            if (!detalhes.Any())
                                continue;

                            var result = detalhes.ToDictionary(x => x.PropertyName, x => new { De = x.OldValue, Para = x.NewValue });
                            auditoria.Detalhes = JsonSerializer.Serialize(result);
                        }

                        await conn.BulkUpdateAsync(auditorias);

                        Console.WriteLine($"Atualização de {offset} a {FETCH} realizada.");
                        offset += FETCH;
                    }

                } while (count > 0);

                Console.WriteLine("\nAtualização Finalizada!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }
        }
        static IConfiguration BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", false)
                            .AddJsonFile($"appsettings.{environment}.json", true)
                            .Build();
        }
    }
}
