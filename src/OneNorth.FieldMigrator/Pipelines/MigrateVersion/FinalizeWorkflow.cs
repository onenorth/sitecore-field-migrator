using System.Linq;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class FinalizeWorkflow : IMigrateVersionPipelineProcessor
    {
        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            var source = args.Source;
            var item = args.Item;

            if (!source.HasWorkflow || (source.HasWorkflow && source.InFinalWorkflowState))
            {
                var workflow = item.Database.WorkflowProvider.GetWorkflow(item);
                if (workflow != null)
                {
                    var states = workflow.GetStates();
                    var finalState = states.First(x => x.FinalState);
                    item.Fields["__Workflow state"].Value = finalState.StateID;
                }
            }
        }
    }
}