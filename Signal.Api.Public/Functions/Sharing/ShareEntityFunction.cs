﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Signal.Api.Public.Auth;
using Signal.Api.Public.Exceptions;
using Signal.Core;
using Signal.Core.Exceptions;
using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Sharing
{
    public class ShareEntityFunction
    {
        private readonly IFunctionAuthenticator functionAuthenticator;
        private readonly IAzureStorageDao azureStorageDao;
        private readonly IAzureStorage storage;
        private readonly ILogger<ShareEntityFunction> logger;

        public ShareEntityFunction(
            IFunctionAuthenticator functionAuthenticator,
            IAzureStorageDao azureStorageDao,
            IAzureStorage storage,
            ILogger<ShareEntityFunction> logger)
        {
            this.functionAuthenticator = functionAuthenticator ?? throw new ArgumentNullException(nameof(functionAuthenticator));
            this.azureStorageDao = azureStorageDao ?? throw new ArgumentNullException(nameof(azureStorageDao));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("Share-Entity")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "share/entity")]
            HttpRequest req,
            CancellationToken cancellationToken) =>
            await req.UserRequest<ProcessSetDto>(this.functionAuthenticator,
                async (user, payload) =>
                {
                    if (payload.Type == null)
                        throw new ExpectedHttpException(HttpStatusCode.BadRequest, "Type is required");
                    if (string.IsNullOrWhiteSpace(payload.EntityId))
                        throw new ExpectedHttpException(HttpStatusCode.BadRequest, "EntityId is required");
                    if (payload.UserEmails == null || !payload.UserEmails.Any())
                        throw new ExpectedHttpException(HttpStatusCode.BadRequest, "UserEmails is required - at least one user email is required");

                    // TODO: Check user has entity assigned

                    foreach (var userEmail in payload.UserEmails)
                    {
                        if (string.IsNullOrWhiteSpace(userEmail)) continue;

                        try
                        {
                            var sanitizedEmail = userEmail.Trim().ToLowerInvariant();
                            var targetUserId = await this.azureStorageDao.UserIdByEmailAsync(sanitizedEmail, cancellationToken);
                            if (!string.IsNullOrWhiteSpace(targetUserId))
                            {
                                await this.storage.CreateOrUpdateItemAsync(
                                    ItemTableNames.UserAssignedEntity(payload.Type.Value),
                                    new UserAssignedEntity(targetUserId, payload.EntityId), cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogInformation(ex, "Failed to share with provided user.");
                        }
                    }
                }, cancellationToken);

        private class ProcessSetDto
        {
            [Required]
            public TableEntityType? Type { get; set; }

            [Required]
            public string? EntityId { get; set; }

            [Required]
            public List<string>? UserEmails { get; set; }
        }
    }
}
