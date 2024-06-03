namespace FormatarDetalheAuditoria
{
    public class tbAuditoria
    {
        public int Id { get; set; }
        public string? Detalhes { get; set; }
    }
    public class tbAuditoriaDetalhe
    {
        public string PropertyName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }
}
