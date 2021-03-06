using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Beacons
{
    public class BeaconsRetrieveFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorageDao storageDao;

        public BeaconsRetrieveFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorageDao storage)
        {
            this.functionAuthenticator =
                functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.storageDao = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        [FunctionName("Beacons-Retrieve")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "beacons")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<IEnumerable<BeaconDto>>(this.functionAuthenticator, async user =>
                    (await this.storageDao.BeaconsAsync(user.UserId, cancellationToken))
                    .Select(b => new BeaconDto
                    {
                        Id = b.RowKey,
                        RegisteredTimeStamp = b.RegisteredTimeStamp
                    })
                    .ToList(),
                cancellationToken);
        
        private class BeaconDto
        {
            public string Id { get; set; }

            public DateTime RegisteredTimeStamp { get; set; }
        }
    }
}