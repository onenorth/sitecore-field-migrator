using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneNorth.FieldMigrator.Configuration
{
    public interface IFieldMigratorConfiguration
    {
        string SourceDatabase { get; }
        string SourceUserName { get; }
        string SourcePassword { get; }
        string TargetDatabase { get; }
        string TargetUserName { get; }

        List<IRootConfiguration> Roots { get; }
        List<ITemplateConfiguration> Templates { get; }
    }
}
