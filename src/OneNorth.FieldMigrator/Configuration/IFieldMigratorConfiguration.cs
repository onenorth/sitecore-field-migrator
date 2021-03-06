﻿using System;
using System.Collections.Generic;

namespace OneNorth.FieldMigrator.Configuration
{
    public interface IFieldMigratorConfiguration
    {
        string SourceEndpointAddress { get; }
        string SourceDatabase { get; }
        string SourceUserName { get; }
        string SourcePassword { get; }
        string TargetDatabase { get; }
        string TargetUserName { get; }

        List<Guid> MediaItemTemplateIds { get; }
        List<IRootConfiguration> Roots { get; }
        List<TemplateInclude> TemplateIncludes { get; }
        List<TemplateExclude> TemplateExcludes { get; }
    }
}
