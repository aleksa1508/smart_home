using Common.DTOs;
using Common.Repositories.DevicesRepositories;
using System.Collections.Generic;
using System.Linq;

namespace Common.Models
{
    public class RuleEngine
    {
        private bool DeviceMatchesAction(Device device, RuleAction action)
        {
            if (action.DeviceId.HasValue)
                return device.Id == action.DeviceId.Value;

            switch (action.DeviceGroup)
            {
                case "ALL_LIGHTS":
                    return device.Name.Contains("Light");

                case "ALL_DOORS":
                    return device.Name.Contains("Door");

                case "ALL_CLIMATES":
                    return device.Name.Contains("Climate");

                case "ALL_VAULTS":
                    return device.Name.Contains("Vault");

                default:
                    return false;
            }
        }
        public bool BlockCommand(List<SmartRule> rules, List<RuleAction> actions, CommandDTO command, out string message)
        {
            message = string.Empty;

            foreach (var rule in rules.Where(r => r.IsEnabled))
            {
                foreach (var action in actions.Where(a => a.RuleId == rule.Id))
                {
                    if (!DeviceMatchesAction(command.SelectedDevice, action))
                        continue;

                    if (!command.Function.Equals(action.FunctionName))
                        continue;

                    // TEMPERATURE / BRIGHTNESS
                    if (int.TryParse(action.Value, out int limit) &&
                        int.TryParse(command.Value, out int requestedValue))
                    {
                        if (requestedValue > limit)
                        {
                            message = $"Rule '{rule.Name}' limits {action.FunctionName} to {limit}.";
                            return true;
                        }

                        continue;
                    }

                    // STATE RULES
                    if (action.Value != command.Value)
                    {
                        message = $"Rule '{rule.Name}' set {action.FunctionName} on value {action.Value}.You can`t change that.";
                        return true;
                    }
                }
            }

            return false;
        }
        public void ApplyRuleEffects(SmartRule rule, List<RuleAction> actions, IDeviceRepository deviceRepository)
        {
            var devices = deviceRepository.GetAllDevices().ToList();

            foreach (var action in actions.Where(a => a.RuleId == rule.Id))
            {
                var targetDevices = devices.Where(d => DeviceMatchesAction(d, action)).ToList();

                foreach (var device in targetDevices)
                {
                    var functionPair = device.Functions
                        .FirstOrDefault(f => f.Value.Name == action.FunctionName);

                    if (functionPair.Value == null)
                        continue;

                    int functionId = functionPair.Key;
                    Function function = functionPair.Value;

                    // TEMPERATURE / BRIGHTNESS LIMITS
                    if (int.TryParse(action.Value, out int limit) &&
                        int.TryParse(function.Value, out int current))
                    {
                        if (current > limit)
                        {
                            function.Value = limit.ToString();

                            deviceRepository.UpdateDeviceFunction(device.Id, functionId, function.Name, limit.ToString());
                        }

                        continue;
                    }

                    // STATE RULES

                    if (function.Value != action.Value)
                    {
                        function.Value = action.Value;

                        deviceRepository.UpdateDeviceFunction(device.Id, functionId, function.Name, action.Value);
                    }
                }
            }
        }
    }
}
