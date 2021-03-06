﻿
using System;
using System.Diagnostics;
using OneNorth.FieldMigrator.Configuration;
using OneNorth.FieldMigrator.Helpers;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Data;

namespace OneNorth.FieldMigrator
{
    public class Migrator : IMigrator
    {
        private static readonly IMigrator _instance = new Migrator();

        public static IMigrator Instance
        {
            get { return _instance; }
        }

        private readonly IFieldMigratorConfiguration _configuration;
        private readonly IMigrationHelper _migrationHelper;

        private Migrator() : this(
            FieldMigratorConfiguration.Instance,
            MigrationHelper.Instance)
        {

        }

        internal Migrator(
            IFieldMigratorConfiguration configuration,
            IMigrationHelper migrationHelper)
        {
            _configuration = configuration;
            _migrationHelper = migrationHelper;
        }

        public void Migrate()
        {
            try
            {
                IndexCustodian.PauseIndexing();

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var database = Database.GetDatabase(_configuration.TargetDatabase);
                using (new DatabaseSwitcher(database))
                {
                    using (new Sitecore.SecurityModel.SecurityDisabler())
                    {
                        foreach (var root in _configuration.Roots)
                        {
                            _migrationHelper.MigrateRoot(root.SourceItemId);
                        }
                    }
                }

                stopWatch.Stop();

                Sitecore.Diagnostics.Log.Info(String.Format("[FieldMigrator] Migration Completed in {0}", stopWatch.Elapsed), this);
            }
            finally
            {
                IndexCustodian.ResumeIndexing();
            }
            
        }
    }
}