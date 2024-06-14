using Microsoft.Extensions.Configuration;

namespace MsGraphFile
{
    internal class Settings
    {
        public string ClientID { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TenantID { get; set; } = string.Empty;

        public string DriveID {  get; set; } = string.Empty;

        public string ExeDirectory { get; set; } = string.Empty;

        public static Settings LoadSettings()
        {
            var exeDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            if (string.IsNullOrEmpty(exeDirectory))
            {
                throw new SystemException("failed to retrieve base executable's directory");
            }

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(exeDirectory, "appsettings.json"), optional: false)
                .AddJsonFile(Path.Combine(exeDirectory, "appsettings.development.json"), optional: true)
                .Build();

            var ret = new Settings();
            ret.ExeDirectory = exeDirectory;

            #region retreive app settings
            var jsonSetting = config.GetRequiredSection("Settings");
            ret.ClientID = jsonSetting["clientId"] ??
                throw new Exception("Could not load app settings. See README for configuration instructions.");
            ret.TenantID = jsonSetting["tenantId"] ??
                throw new Exception("Could not load app settings. See README for configuration instructions.");
            ret.DriveID = jsonSetting["driveId"] ??
                throw new Exception("Could not load app settings. See README for configuration instructions.");
            #endregion

            #region get secret key
            //these value retrieved from Azure Portal -> Microsoft Elentra ID -> App Registration -> select the app IT provision for this project
            //The app this project chosen is <IT -general evaluation>
            //to grant API permission, please login to elentra admin centre with IT account
            //https://entra.microsoft.com/

            //value is stored at:
            var clientSecretFilePath = Path.Combine(exeDirectory, "secret.txt");
            ret.ClientSecret = File.ReadAllText(clientSecretFilePath);
            #endregion

            return ret;
        }
    }
}
