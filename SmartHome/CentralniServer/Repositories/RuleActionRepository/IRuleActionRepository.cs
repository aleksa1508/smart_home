using Common.Models;
using System.Collections.Generic;

namespace CentralniServer.Repositories.RuleActionRepository
{
    public interface IRuleActionRepository
    {
        IEnumerable<RuleAction> GetAllActions();
        void AddRuleAction(int ruleId, string functionName, string value, string deviceGroup, int? functionId, int? deviceId);
        IEnumerable<RuleAction> GetAllActionsByRuleId(int ruleId);
        void DeleteRuleAction(int id);
    }
}
