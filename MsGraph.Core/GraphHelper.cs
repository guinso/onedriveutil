using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using File = System.IO.File;

namespace MsGraph.Core
{
    //sources:
    //https://learn.microsoft.com/en-us/graph/tutorials
    //https://learn.microsoft.com/en-us/graph/tutorials/dotnet-app-only?tabs=aad

    //https://learn.microsoft.com/en-us/graph/tutorials
    //https://learn.microsoft.com/en-us/graph/tutorials/dotnet-app-only?tabs=aad&tutorial-step=4
    //https://stackoverflow.com/questions/74922956/get-refresh-token-additionally-to-access-token-with-microsoft-identity-client
    //https://learn.microsoft.com/en-us/graph/api/driveitem-put-content?view=graph-rest-1.0&tabs=http

    //graph API: example get drive children with relative paths
    //reference: https://learn.microsoft.com/en-us/graph/onedrive-addressing-driveitems

    public class GraphHelper: IDisposable
    {
        private ClientSecretCredential? _clientSecretCredential;
        private GraphServiceClient? _appClient;
        private ILogger _logger;

        private string[] _scopes = new[] { "https://graph.microsoft.com/.default"};
        
        public GraphHelper(ILogger logger)
        {
            _logger = logger;
        }

        public void InitializeGraphForAppOnlyAuth(string tenantID, string clientID, string clientSecretValue)
        {
            if (_clientSecretCredential == null)
            {
                _clientSecretCredential = new ClientSecretCredential(tenantID, clientID, clientSecretValue);
            }

            if (_appClient == null)
            {
                _appClient = new GraphServiceClient(_clientSecretCredential, _scopes);
            }
        }

        /// <summary>
        /// Get App Only Token from MS Entra ID
        /// </summary>
        /// <remarks>to decode token please visit https://jwt.ms/</remarks>
        /// <returns>token</returns>
        /// <exception cref="System.NullReferenceException"></exception>
        public async Task<string> GetAppOnlyTokenAsync()
        {
            #region test with Graph library
            // Ensure credential isn't null
            _ = _clientSecretCredential ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            // Request token with given scopes
            var context = new TokenRequestContext(_scopes);
            var response = await _clientSecretCredential.GetTokenAsync(context);
            return response.Token;
            #endregion
        }

        public Task<UserCollectionResponse?> GetUsersAsync()
        {
            // Ensure client isn't null
            _ = _appClient ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            return _appClient.Users.GetAsync((config) =>
            {
                // Only request specific properties
                config.QueryParameters.Select = new[] { "displayName", "id", "mail" };
                // Get at most 25 results
                config.QueryParameters.Top = 25;
                // Sort by display name
                config.QueryParameters.Orderby = new[] { "displayName" };
            });
        }

        public async Task<(bool, string)> UploadFile(string driveID, string driveDirectory, string driveFilename, string localFilePath)
        {
            // Ensure client isn't null
            _ = _appClient ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            if(File.Exists(localFilePath) == false)
            {
                throw new FileNotFoundException(localFilePath);
            }

            var driveItem = await GetFileInfo(driveID, driveDirectory);
            if(driveItem is not null && string.IsNullOrEmpty(driveItem.Id) == false)
            {
                var filename = Path.GetFileName(localFilePath);

                using(var stream = File.OpenRead(localFilePath))
                {
                    var returnDriveItem = await _appClient.Drives[driveID]
                        .Items[driveItem.Id]
                        .ItemWithPath(driveFilename)
                        .Content
                        .PutAsync(stream);

                    if (returnDriveItem is null)
                        return (false, "no drive info return from OneDrive");
                    else
                        return (true, string.Empty);
                }
            }

            return (false, $"drive directory not found! {driveID}");
        }

        public async Task<(bool,string)> DownloadFile(string driveID, string driveFilePath, string saveFilePath)
        {
            var driveItem = await GetFileInfo(driveID, driveFilePath);
            if (driveItem is not null && string.IsNullOrEmpty(driveItem.Id) == false)
            {
                return await DownloadFileByID(driveID, driveItem.Id, saveFilePath);
            }

            return (false, $"drive file not found! {driveFilePath}");
        }

        public async Task<(bool,string)> DownloadFileByID(string driveID, string itemID, string filePath)
        {
            // Ensure client isn't null
            _ = _appClient ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            try
            {
                var streams = await _appClient.Drives[driveID].Items[itemID].Content.GetAsync();
                if (streams != null)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    using (var fileStream = File.Create(filePath))
                    {
                        streams.CopyTo(fileStream);
                        streams.Dispose();
                    };

                    return (true,string.Empty);
                }

                return (false, "no content return");
            }
            catch(Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<DriveItem?> GetFileInfo(string driveID, string driveFilePath)
        {
            // Ensure client isn't null
            _ = _appClient ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            
            return await _appClient.Drives[driveID].Root.ItemWithPath(driveFilePath).GetAsync();
        }

        public async Task<(bool, string)> AddRowIntoExcelTable(string driveID, string excelFilePath, string tableName, WorkbookTableRow rows)
        {
            //resource: https://learn.microsoft.com/en-us/graph/api/table-post-rows?view=graph-rest-1.0&tabs=http

            // Ensure client isn't null
            _ = _appClient ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            var driveItem = await GetFileInfo(driveID, excelFilePath);
            if (driveItem is not null && string.IsNullOrEmpty(driveItem.Id) == false)
            {
                var ret = await _appClient.Drives[driveID].Items[driveItem.Id].Workbook.Tables[tableName].Rows.PostAsync(rows);
                if (ret != null)
                    return (true, $"add row into excel table {tableName} success");
                else
                    return (false, $"failed add row into excel table {tableName}");
            }

            return (false, $"excel file not found! {excelFilePath}");
        }

        public void Dispose()
        {
            if(_appClient != null )
            {
                _appClient.Dispose();
            }
        }
    }
}
