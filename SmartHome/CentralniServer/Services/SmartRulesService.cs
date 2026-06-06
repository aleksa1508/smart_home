using CentralniServer.Repositories.RuleActionRepository;
using CentralniServer.Repositories.SmartRulesRepositories;
using Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace CentralniServer.Services
{
    public class SmartRulesService
    {
        private readonly ISmartRulesRepository smartRulesRepository;
        private readonly IRuleActionRepository ruleActionRepository;

        public SmartRulesService(ISmartRulesRepository smartRulesRepository, IRuleActionRepository ruleActionRepository)
        {
            this.smartRulesRepository = smartRulesRepository;
            this.ruleActionRepository = ruleActionRepository;
        }

        public List<SmartRule> GetSmartRules()
        {
            var smartRules = smartRulesRepository.GetAllSmartRules().ToList();
            foreach (var smartRule in smartRules)
            {
                var actions = ruleActionRepository.GetAllActionsByRuleId(smartRule.Id);
                smartRule.Actions = actions.ToList();
            }
            return smartRules;
        }
        public void AddNewSmartRules()
        {
            if (smartRulesRepository.GetAllSmartRules().ToList().Count == 0)
            {

                List<SmartRule> rules1 = new List<SmartRule>
                {
                    new SmartRule{ IsEnabled=false, Name="NightMode",Description="Limits temperature to 20°C, restricts lights and locks garage during night hours.",Actions=new List<RuleAction>{ new RuleAction { RuleId = 1,DeviceGroup="ALL_CLIMATES" ,FunctionName = "temperature", Value = "25" } }},
                    new SmartRule{ IsEnabled=false, Name="SecurityMode",Description="Lock all doors and vaults.",Actions=new List<RuleAction>{ new RuleAction { RuleId = 2, DeviceGroup = "ALL_DOORS", FunctionName = "state", Value = "CLOSED" }, new RuleAction { RuleId = 2, DeviceGroup = "ALL_VAULTS", FunctionName = "state", Value = "CLOSED" } } },
                    new SmartRule{ IsEnabled=false, Name="EnergySaving",Description="Limits brightness and reduces energy usage.",Actions=new List<RuleAction>{ new RuleAction { RuleId = 3, DeviceId = 1, FunctionId = 1, FunctionName = "brightness",Value="40" } }},
                };

                foreach (var s in rules1)
                {
                    smartRulesRepository.AddSmartRule(s.Name, s.Description, s.IsEnabled);
                    int id = smartRulesRepository.GetSmartRuleByName(s.Name);
                    foreach (var a in s.Actions)
                    {
                        if (a?.DeviceId == null)
                        {
                            ruleActionRepository.AddRuleAction(id, a.FunctionName, a.Value, a.DeviceGroup, null, null);
                        }
                        else
                        {
                            ruleActionRepository.AddRuleAction(id, a.FunctionName, a.Value, null, a?.FunctionId, a?.DeviceId);

                        }
                    }
                }
            }
        }
    }
}
