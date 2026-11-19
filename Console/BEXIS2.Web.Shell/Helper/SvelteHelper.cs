using System.Text.Json;

public class SvelteAssetService
{
    private readonly IWebHostEnvironment _env;

    public SvelteAssetService(IWebHostEnvironment env)
    {
        _env = env;
    }

    // Liefert z.B. "/svelte/_app/immutable/entry/start.D2Lw0WLT.js"
    public string GetScript(string module = "shell")
        => GetEntryFile(module, "entry/start.");

    // Liefert z.B. "/svelte/_app/immutable/entry/app.B123xyz.js"
    public string GetApp(string module = "shell")
        => GetEntryFile(module, "entry/app.");

    private string GetEntryFile(string module, string fileSuffixPrefix)
    {
        bool isShell = string.IsNullOrEmpty(module) ||
                       module.Equals("shell", StringComparison.OrdinalIgnoreCase);

        // 1. Pfad auf der FESTPLATTE zur manifest.json
        // Passe "svelte" und "vite" an deinen echten Build-Ordner an
        string baseFolder = isShell
            ? Path.Combine(_env.WebRootPath, "svelte")
            : Path.Combine(_env.WebRootPath, "Areas", module, "wwwroot", "svelte");

        string manifestPath = Path.Combine(baseFolder, ".vite", "manifest.json");
        if (!File.Exists(manifestPath))
        {
            // Fallback, falls manifest.json direkt unter vite/ liegt
            manifestPath = Path.Combine(baseFolder, "vite", "manifest.json");
        }

        if (!File.Exists(manifestPath))
            return string.Empty;

        // 2. JSON lesen und passende "file" suchen
        string json = File.ReadAllText(manifestPath);
        using JsonDocument doc = JsonDocument.Parse(json);

        foreach (JsonProperty entry in doc.RootElement.EnumerateObject())
        {
            if (entry.Value.TryGetProperty("file", out JsonElement fileProp))
            {
                string? file = fileProp.GetString(); // z.B. "_app/immutable/entry/start.xyz.js"
                if (!string.IsNullOrEmpty(file) && file.Contains(fileSuffixPrefix))
                {
                    // 3. URL-Pfad für den BROWSER bauen (ohne wwwroot!)
                    string urlBase = isShell
                        ? "/svelte"
                        : $"/Areas/{module}/svelte";

                    return $"{urlBase}/{file.TrimStart('/')}";
                }
            }
        }

        return string.Empty;
    }
}