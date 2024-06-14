using Microsoft.Extensions.Logging;
using MsGraph.Core;
using MsGraphFile;
using System.Text;
using MsGraph.Core.FileLogger;

var setting = Settings.LoadSettings();
var logger = MakeLogger(setting);

logger.LogInformation("==begin==");

var graphAgent = new GraphHelper(logger);
graphAgent.InitializeGraphForAppOnlyAuth(setting.TenantID, setting.ClientID, setting.ClientSecret);

try
{
    if (args.Length != 3)
    {
        logger.LogError("application only accept 3 input arguments");
        Environment.Exit(-1);
    }

    var flag = args[0];
    var localFilePath = args[1];
    var driveFilePath = args[2];
    logger.LogInformation($"local file path: {localFilePath}");
    logger.LogInformation($"drive file path: {driveFilePath}");

    //get drive directory
    var parts = driveFilePath.Split('/');
    var strBuilder = new StringBuilder();
    for(int i=0; i<parts.Length - 2; i++)
        strBuilder.Append(parts[i]).Append("/");
    strBuilder.Append(parts[parts.Length - 2]);

    var driveDirectory = strBuilder.ToString();
    var driveFilename = parts[parts.Length - 1];
    logger.LogInformation($"drive directory: {driveDirectory}");
    logger.LogInformation($"drive file name: {driveFilename}");
    

    if (flag == "-upload")
    {
        logger.LogInformation("begin upload file");

        var (isSuccess, msg) = await graphAgent.UploadFile(
            setting.DriveID,
            driveDirectory,
            driveFilename,
            localFilePath);

        if (isSuccess)
        {
            logger.LogInformation("upload success");
            Environment.Exit(0);
        }
        else
        {
            logger.LogError($"upload failed, {msg}");
        }
    }
    else if(flag == "-download")
    {
        logger.LogInformation("begin download file");

        var (isSuccess, msg) = await graphAgent.DownloadFile(
            setting.DriveID,
            driveFilePath,
            localFilePath);

        if (isSuccess)
        {
            logger.LogInformation("download success");
            Environment.Exit(0);
        }
            
        else
        {
            logger.LogError($"download failed, {msg}");
        }
    }
    else
    {
        logger.LogError($"invalid flag detected: {flag}, expect -upload or -download");
    }
}
catch(Exception ex)
{
    logger.LogError(ex, "there error try to run the app sequence");
}

Environment.Exit(-1);

ILogger MakeLogger(Settings settings)
{
    using ILoggerFactory factory = LoggerFactory.Create(builder => { builder.AddConsole(); });
    factory.AddFile(settings.ExeDirectory);

    ILogger logger = factory.CreateLogger("MS Graph " + Guid.NewGuid());
    return logger;
}