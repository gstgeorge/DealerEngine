using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace DealerEngine;

static partial class Settings
{
    private static Config _config; // User-defined settings



    /// <summary>
    /// The path to the user's logo.
    /// Appears on each invoice/work summary.
    /// Returns null if unset or file does not exist.
    /// </summary>
    public static string Logo
    {
        get
        {
            if (!string.IsNullOrEmpty(_config.Logo))
            {
                string p = Path.Combine(SETTINGS_PATH, _config.Logo);

                if (File.Exists(p))
                {
                    return p;
                }

                else // Logo no longer exists at expected path
                {
                    Logo = null;
                }
            }

            return null;
        }

        set
        {
            // Remove a previously set logo if it exists.
            if (File.Exists(_config.Logo))
            {
                try
                {
                    File.Delete(_config.Logo);
                }

                catch (Exception e)
                {
                    throw new Exception($"Unable to remove old logo: {e.Message} ({e.GetType()})", e);
                }
            }

            // Copy the new logo to the settings dir and record its name.
            if (string.IsNullOrEmpty(value))
            {
                _config.Logo = null;
            }

            else
            {
                try
                {
                    string fileName = Path.GetFileName(value);
                    File.Copy(value, Path.Combine(SETTINGS_PATH, fileName), true);
                    _config.Logo = fileName;
                }

                catch (Exception e)
                {
                    _config.Logo = null;

                    throw new Exception($"Unable to set new logo: {e.Message} ({e.GetType()})", e);
                }
            }
        }
    }

    /// <summary>
    /// The user's business name. 
    /// Appears as the title of the document and at the top of each invoice/work summary.
    /// </summary>
    public static string BusinessName 
    {
        get => _config.BusinessName;
        set
        {
            string trimmed = value?.Trim();
            _config.BusinessName = string.IsNullOrEmpty(trimmed) ? String.Empty : trimmed;
        }
    }

    /// <summary>
    /// The user's contact info.
    /// Appears at the top of each invoice/work summary.
    /// Max 5 lines, any excess will be truncated.
    /// </summary>
    public static string[] BusinessContactInfo
    {
        get => _config.BusinessContactInfo;
        set => _config.BusinessContactInfo = value?.Where(x => !string.IsNullOrEmpty(x)).Take(5).ToArray() ?? [];
    }

    /// <summary>
    /// The terms to print on each invoice.
    /// Strings longer than 15 characters will be truncated.
    /// </summary>
    public static string InvoiceTerms 
    { 
        get => _config.InvoiceTerms;
        set
        {
            string trimmed = value?.Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                _config.InvoiceTerms = null;
            }

            else _config.InvoiceTerms = (trimmed.Length > 15) ? trimmed.Substring(0, 15) : trimmed;
        }
    }

    /// <summary>
    /// The accent color used on invoices/work summaries.
    /// </summary>
    public static System.Drawing.Color InvoiceAccentColor 
    {
        get => _config.InvoiceAccentColor;
        set => _config.InvoiceAccentColor = value;
    }

    /// <summary>
    /// Print grid on invoices to help with formatting
    /// </summary>
    public static bool DebugGrid
    { 
        get => _config.DebugGrid; 
        set => _config.DebugGrid = value; 
    }

    /// <summary>
    /// Write the user's configuration to a JSON at <see cref="CONFIG_PATH"/>.
    /// </summary>
    public static void SaveUserConfig()
    {
        try
        {
            File.WriteAllText(
                CONFIG_PATH,
                JsonConvert.SerializeObject(_config, Formatting.Indented)
                );
        }

        catch (Exception e)
        {
            // TODO: log failure to save config
        }
    }

    /// <summary>
    /// If a config file exists at <see cref="CONFIG_PATH"/>, load it.
    /// Otherwise, create a new config file and save it at <see cref="CONFIG_PATH"/>.
    /// </summary>
    private static void LoadUserConfig()
    {
        if (File.Exists(CONFIG_PATH))
        {
            try
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH));
            }

            catch (Exception e)
            {
                _config = new Config();
                return;
                // TODO: log failure to load existing config
            }
        }

        else _config = new Config();

        SaveUserConfig();
    }



    // Data structure to hold user-defined settings.
    private class Config
    {
        public string Logo { get; set; } = null;
        public string BusinessName { get; set; } = String.Empty;
        public string[] BusinessContactInfo { get; set; } = [];
        public string InvoiceTerms { get; set; } = String.Empty;
        public System.Drawing.Color InvoiceAccentColor { get; set; } = System.Drawing.Color.FromArgb(0, 0, 0);
        public bool DebugGrid { get; set; } = false;
    }
}
