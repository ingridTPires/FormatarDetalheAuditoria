namespace FormatarDetalheAuditoria
{
    public class AuditoriaDetalhe
    {
        public int IdAuditoria { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }
}
