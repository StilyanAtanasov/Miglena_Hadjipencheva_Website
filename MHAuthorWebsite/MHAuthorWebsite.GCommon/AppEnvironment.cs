namespace MHAuthorWebsite.GCommon;

public static class AppEnvironment
{
    public static string EnvironmentName { get; private set; } = "Unknown";

    public static bool IsDevelopment => EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);

    public static bool IsProduction => EnvironmentName.Equals("Production", StringComparison.OrdinalIgnoreCase);

    public static bool IsStaging => EnvironmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase);

    public static void Initialize(string environmentName) => EnvironmentName = environmentName;
}