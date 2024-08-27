using System.IO;

namespace DealerEngine;

static partial class Settings
{
    // Constructor
    static Settings()
    {
        LoadUserConfig();
    }

    /// <summary>
    /// The path where the program's settings are stored.
    /// This path will be created if it does not already exist.
    /// </summary>
    public static string SETTINGS_PATH
    {
        get
        {
            string p = Path.Combine(Directory.GetCurrentDirectory(), "settings");
            Directory.CreateDirectory(p);
            return p;
        }
    }

    /// <summary>
    /// The path to the dealers config file.
    /// </summary>
    public static string DEALER_CONFIG_PATH
    {
        get => Path.Combine(SETTINGS_PATH, "dealers.json");
    }

    /// <summary>
    /// The path to the user's config file.
    /// </summary>
    private static string CONFIG_PATH
    {
        get => Path.Combine(SETTINGS_PATH, "config.json");
    }
}
