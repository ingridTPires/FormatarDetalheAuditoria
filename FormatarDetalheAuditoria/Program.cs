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
        private const string SELECT_AUDITORIAS = @"SELECT [Id] FROM [tbAuditoria] ORDER BY [Id]
                                                    OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";
        private const string SELECT_DETALHES = "SELECT [PropertyName], [OldValue], [NewValue] FROM [tbAuditoriaDetalhe] WHERE [IdAuditoria] = @IdAuditoria;";
        static async Task Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                using var conn = new SqlConnection(config.GetConnectionString("Default"));

                var offset = 0;
                var auditorias = new List<tbAuditoria>();

                do
                {
                    auditorias = (await conn.QueryAsync<tbAuditoria>(SELECT_AUDITORIAS, new { Offset = offset, Fetch = FETCH })).ToList();

                    foreach (var auditoria in auditorias)
                    {
                        var detalhes = await conn.QueryAsync<tbAuditoriaDetalhe>(SELECT_DETALHES, new { IdAuditoria = auditoria.Id });

                        if (!detalhes.Any())
                            continue;

                        var result = detalhes.ToDictionary(x => x.PropertyName, x => new { De = x.OldValue, Para = x.NewValue });
                        auditoria.Detalhes = JsonSerializer.Serialize(result);
                    }

                    await conn.BulkUpdateAsync(auditorias);

                    Console.WriteLine($"Atualização de ${offset} a ${FETCH} realizada.");
                    offset += FETCH;
                } while (auditorias.Any());

                Console.WriteLine("\nAtualização Finalizada!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }
        }
    }
}
