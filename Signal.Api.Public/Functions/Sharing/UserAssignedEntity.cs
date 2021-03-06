﻿using Signal.Core.Storage;

namespace Signal.Api.Public.Functions.Sharing
{
    public class UserAssignedEntity : ITableEntity
    {
        public UserAssignedEntity(string userId, string entityId)
        {
            this.PartitionKey = userId;
            this.RowKey = entityId;
        }

        public string PartitionKey { get; }
        public string RowKey { get; }
    }
}