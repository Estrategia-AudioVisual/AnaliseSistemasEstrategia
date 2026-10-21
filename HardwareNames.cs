namespace ResetExplode;

/// <summary>Nomes curtos de CPU e GPU para exibir no painel, no benchmark e no overclock.</summary>
public static class HardwareNames
{
    /// <summary>"NVIDIA GeForce RTX 5070" → "RTX 5070".</summary>
    public static string ShortGpuName(string name)
    {
        string s = name.Replace("(TM)", "").Replace("(R)", "");
        foreach (var w in new[] { "NVIDIA", "GeForce", "AMD", "Intel" }) s = s.Replace(w, "");
        if (s.Contains("Radeon RX")) s = s.Replace("Radeon", "");             // "Radeon RX 9070 XT" → "RX 9070 XT"
        s = string.Join(' ', s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (s.StartsWith("Arc") && s.EndsWith(" Graphics")) s = s[..^9];        // "Arc B580 Graphics" → "Arc B580"
        return s;                                                              // iGPU AMD fica "Radeon Graphics"
    }

    /// <summary>"AMD Ryzen 7 7700X 8-Core Processor" → "Ryzen 7 7700X".</summary>
    public static string ShortCpuName(string name)
    {
        string s = name.Replace("AMD", "").Replace("Intel(R)", "").Replace("(R)", "").Replace("(TM)", "").Replace("Core(TM)", "Core");
        int i = s.IndexOf("-Core", StringComparison.OrdinalIgnoreCase);
        if (i > 0) s = s[..s.LastIndexOf(' ', i)];       // "Ryzen 7 7700X 8-Core Processor" → "Ryzen 7 7700X"
        s = s.Replace("Processor", "").Replace("CPU", "");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\d+th Gen\s*", ""); // "13th Gen Core i7-13700K"
        int at = s.IndexOf('@');
        if (at > 0) s = s[..at];                          // "Core i5-12400F CPU @ 2.50GHz"
        return string.Join(' ', s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
