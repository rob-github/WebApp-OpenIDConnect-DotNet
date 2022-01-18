using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;

namespace WebApp_OpenIDConnect_DotNet
{
    public static class GraphClientFactory
    {
        /// <summary>
        /// Build a client for the MS Graph API, it can be shared by all users
        /// </summary>
        /// <param name="clientId">value from app registration</param>
        /// <returns></returns>
        public static GraphServiceClient Create(
            string clientId)
        {
            var scopes = new[] {"User.Read"};

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "common";

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // Callback function that receives the user prompt
            // Prompt contains the generated device code that use must
            // enter during the auth process in the browser
            Func<DeviceCodeInfo, CancellationToken, Task> callback = (code, cancellation) =>
            {
                Console.WriteLine(code.Message);
                return Task.FromResult(0);
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.devicecodecredential
            var deviceCodeCredential = new DeviceCodeCredential(
                callback, tenantId, clientId, options);

            var graphClient = new GraphServiceClient(deviceCodeCredential, scopes);

            return graphClient;
        }
    }
}
