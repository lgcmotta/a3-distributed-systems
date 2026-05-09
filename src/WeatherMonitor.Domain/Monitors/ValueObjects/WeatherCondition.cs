using WeatherMonitor.Domain.Core;

namespace WeatherMonitor.Domain.Monitors.ValueObjects;

public sealed record WeatherCondition : Enumeration
{
    private WeatherCondition(int key, string code, string description)
        : base(key, code)
    {
        Description = description;
    }

    public string Code => Value;

    public string Description { get; }

    public static WeatherCondition OvercastWithIsolatedRain { get; } = new(1, "ec", "Encoberto com Chuvas Isoladas");
    public static WeatherCondition IsolatedRain { get; } = new(2, "ci", "Chuvas Isoladas");
    public static WeatherCondition Rain { get; } = new(3, "c", "Chuva");
    public static WeatherCondition Unstable { get; } = new(4, "in", "Instável");
    public static WeatherCondition PossibilityOfRainShowers { get; } = new(5, "pp", "Poss. de Pancadas de Chuva");
    public static WeatherCondition MorningRain { get; } = new(6, "cm", "Chuva pela Manhã");
    public static WeatherCondition NightRain { get; } = new(7, "cn", "Chuva a Noite");
    public static WeatherCondition AfternoonRainShowers { get; } = new(8, "pt", "Pancadas de Chuva a Tarde");
    public static WeatherCondition MorningRainShowers { get; } = new(9, "pm", "Pancadas de Chuva pela Manhã");
    public static WeatherCondition CloudyWithRainShowers { get; } = new(10, "np", "Nublado e Pancadas de Chuva");
    public static WeatherCondition RainShowers { get; } = new(11, "pc", "Pancadas de Chuva");
    public static WeatherCondition PartlyCloudy { get; } = new(12, "pn", "Parcialmente Nublado");
    public static WeatherCondition Drizzle { get; } = new(13, "cv", "Chuvisco");
    public static WeatherCondition Rainy { get; } = new(14, "ch", "Chuvoso");
    public static WeatherCondition Storm { get; } = new(15, "t", "Tempestade");
    public static WeatherCondition MostlySunny { get; } = new(16, "ps", "Predomínio de Sol");
    public static WeatherCondition Overcast { get; } = new(17, "e", "Encoberto");
    public static WeatherCondition Cloudy { get; } = new(18, "n", "Nublado");
    public static WeatherCondition ClearSky { get; } = new(19, "cl", "Céu Claro");
    public static WeatherCondition Fog { get; } = new(20, "nv", "Nevoeiro");
    public static WeatherCondition Frost { get; } = new(21, "g", "Geada");
    public static WeatherCondition Snow { get; } = new(22, "ne", "Neve");
    public static WeatherCondition Undefined { get; } = new(23, "nd", "Não Definido");
    public static WeatherCondition NightRainShowers { get; } = new(24, "pnt", "Pancadas de Chuva a Noite");
    public static WeatherCondition PossibilityOfRain { get; } = new(25, "psc", "Possibilidade de Chuva");
    public static WeatherCondition PossibilityOfMorningRain { get; } = new(26, "pcm", "Possibilidade de Chuva pela Manhã");
    public static WeatherCondition PossibilityOfAfternoonRain { get; } = new(27, "pct", "Possibilidade de Chuva a Tarde");
    public static WeatherCondition PossibilityOfNightRain { get; } = new(28, "pcn", "Possibilidade de Chuva a Noite");
    public static WeatherCondition CloudyWithAfternoonRainShowers { get; } = new(29, "npt", "Nublado com Pancadas a Tarde");
    public static WeatherCondition CloudyWithNightRainShowers { get; } = new(30, "npn", "Nublado com Pancadas a Noite");
    public static WeatherCondition CloudyWithPossibilityOfNightRain { get; } = new(31, "ncn", "Nublado com Poss. de Chuva a Noite");
    public static WeatherCondition CloudyWithPossibilityOfAfternoonRain { get; } = new(32, "nct", "Nublado com Poss. de Chuva a Tarde");
    public static WeatherCondition CloudyWithPossibilityOfMorningRain { get; } = new(33, "ncm", "Nubl. c/ Poss. de Chuva pela Manhã");
    public static WeatherCondition CloudyWithMorningRainShowers { get; } = new(34, "npm", "Nublado com Pancadas pela Manhã");
    public static WeatherCondition CloudyWithPossibilityOfRain { get; } = new(35, "npp", "Nublado com Possibilidade de Chuva");
    public static WeatherCondition VariableCloudiness { get; } = new(36, "vn", "Variação de Nebulosidade");
    public static WeatherCondition AfternoonRain { get; } = new(37, "ct", "Chuva a Tarde");
    public static WeatherCondition PossibilityOfNightRainShowers { get; } = new(38, "ppn", "Poss. de Panc. de Chuva a Noite");
    public static WeatherCondition PossibilityOfAfternoonRainShowers { get; } = new(39, "ppt", "Poss. de Panc. de Chuva a Tarde");
    public static WeatherCondition PossibilityOfMorningRainShowers { get; } = new(40, "ppm", "Poss. de Panc. de Chuva pela Manhã");

    public static WeatherCondition FromCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return ParseByValue<WeatherCondition>(code.Trim().ToLowerInvariant());
    }

    public static bool TryFromCode(string code, out WeatherCondition? condition)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            return TryParseByValue(code.Trim().ToLowerInvariant(), out condition);
        }

        condition = null;
        return false;
    }
}