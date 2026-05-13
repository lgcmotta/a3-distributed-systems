using WeatherMonitor.Domain.Core;

namespace WeatherMonitor.Domain.Monitors.ValueObjects;

public sealed record BrazilianState : Enumeration
{
    private BrazilianState(int key, string value, string name) : base(key, value)
    {
        Name = name;
    }

    public string Name { get; }

    public static BrazilianState Acre { get; } = new(1, "AC", "Acre");
    public static BrazilianState Alagoas { get; } = new(2, "AL", "Alagoas");
    public static BrazilianState Amapa { get; } = new(3, "AP", "Amapá");
    public static BrazilianState Amazonas { get; } = new(4, "AM", "Amazonas");
    public static BrazilianState Bahia { get; } = new(5, "BA", "Bahia");
    public static BrazilianState Ceara { get; } = new(6, "CE", "Ceará");
    public static BrazilianState DistritoFederal { get; } = new(7, "DF", "Distrito Federal");
    public static BrazilianState EspiritoSanto { get; } = new(8, "ES", "Espírito Santo");
    public static BrazilianState Goias { get; } = new(9, "GO", "Goiás");
    public static BrazilianState Maranhao { get; } = new(10, "MA", "Maranhão");
    public static BrazilianState MatoGrosso { get; } = new(11, "MT", "Mato Grosso");
    public static BrazilianState MatoGrossoDoSul { get; } = new(12, "MS", "Mato Grosso do Sul");
    public static BrazilianState MinasGerais { get; } = new(13, "MG", "Minas Gerais");
    public static BrazilianState Para { get; } = new(14, "PA", "Pará");
    public static BrazilianState Paraiba { get; } = new(15, "PB", "Paraíba");
    public static BrazilianState Parana { get; } = new(16, "PR", "Paraná");
    public static BrazilianState Pernambuco { get; } = new(17, "PE", "Pernambuco");
    public static BrazilianState Piaui { get; } = new(18, "PI", "Piauí");
    public static BrazilianState RioDeJaneiro { get; } = new(19, "RJ", "Rio de Janeiro");
    public static BrazilianState RioGrandeDoNorte { get; } = new(20, "RN", "Rio Grande do Norte");
    public static BrazilianState RioGrandeDoSul { get; } = new(21, "RS", "Rio Grande do Sul");
    public static BrazilianState Rondonia { get; } = new(22, "RO", "Rondônia");
    public static BrazilianState Roraima { get; } = new(23, "RR", "Roraima");
    public static BrazilianState SantaCatarina { get; } = new(24, "SC", "Santa Catarina");
    public static BrazilianState SaoPaulo { get; } = new(25, "SP", "São Paulo");
    public static BrazilianState Sergipe { get; } = new(26, "SE", "Sergipe");
    public static BrazilianState Tocantins { get; } = new(27, "TO", "Tocantins");
}