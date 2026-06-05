using Common.Models;
using System.Collections.Generic;

namespace Common.Repositories.SmartRulesRepositories
{
    public interface ISmartRulesRepository
    {
        IEnumerable<SmartRule> GetAllSmartRules();
        int GetSmartRuleByName(string name);
        void UpdateSmartRule(SmartRule smartRule);
        void AddSmartRule(string name, string description, bool isEnabled);
        void PrintAllSmartRules();
    }
}
