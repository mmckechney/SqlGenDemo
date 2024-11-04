using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;


namespace SqlGenDemo.Library.SK
{
   public partial class SkAgents
   {
      private sealed class ApprovalTerminationStrategy : TerminationStrategy
      {
         public ApprovalTerminationStrategy()
         {
            MaximumIterations = 5;
         }
         // Terminate when the final message contains the term "approve"
         protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
             => Task.FromResult(history[history.Count - 1].Content?.Contains("-----", StringComparison.OrdinalIgnoreCase) ?? false);
      }
   }
}
