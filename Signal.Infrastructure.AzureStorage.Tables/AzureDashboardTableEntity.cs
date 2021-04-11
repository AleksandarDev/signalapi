﻿using Signal.Core;

namespace Signal.Infrastructure.AzureStorage.Tables
{
    internal class AzureDashboardTableEntity : AzureTableEntityBase, IDashboardTableEntity
    {
        public string Name { get; set; }

        public string? ConfigurationSerialized { get; set; }
    }
}