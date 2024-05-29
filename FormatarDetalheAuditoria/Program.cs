using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text.Json;

namespace FormatarDetalheAuditoria
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            using var conn = new SqlConnection(config.GetConnectionString("Default"));

            var auditoriaDetalhes = await conn.QueryAsync<AuditoriaDetalhe>("SELECT [IdAuditoria], [PropertyName], [OldValue], [NewValue] FROM [tbAuditoriaDetalhe]");

            foreach (var auditoria in auditoriaDetalhes.GroupBy(x => x.IdAuditoria))
            {
                var result = auditoria.ToDictionary(x => x.PropertyName, x => new { de = x.OldValue, para = x.NewValue });

                var resultJson = JsonSerializer.Serialize(result);

                await conn.ExecuteAsync(@"UPDATE [tbAuditoria]
                                        SET [Detalhes] = @Detalhes 
                                        WHERE [Id] = @IdAuditoria;", new { Detalhes = resultJson, IdAuditoria = auditoria.Key });

                Console.WriteLine($"Auditoria {auditoria.Key} atualizada.");
            }

            Console.WriteLine("\nAtualização Finalizada!");
            Console.ReadLine();
        }
    }
}
