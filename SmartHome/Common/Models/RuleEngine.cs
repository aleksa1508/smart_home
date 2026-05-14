using Common.DTOs;
using Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Common.Models
{
    public class RuleEngine
    {
        public bool BlockCommand(List<SmartRule> _rules, CommandDTO command, User user, out string message)
        {
            message = string.Empty;

            if (_rules.Any(r => r.Name == "NightMode" && r.IsEnabled))
            {
                if (command.Function == "temperature" &&
                    int.Parse(command.Value) > 20)
                {
                    message = "Night Mode limits temperature to 20°C.";
                    return true;
                }
            }

            if (_rules.Any(r => r.Name == "SecurityMode" && r.IsEnabled))
            {
                if ((command.SelectedDevice.Name.Contains("Door") ||
                     command.SelectedDevice.Name.Contains("Vault")) &&
                     command.Value == "OPEN")
                {
                    if (user.Role != UserRole.OWNER)
                    {
                        message = "Security Mode blocks door unlocking.";
                        return true;
                    }
                }
            }

            if (_rules.Any(r => r.Name == "EnergySaving" && r.IsEnabled))
            {
                if (command.Function == "brightness" &&
                    int.Parse(command.Value) > 50)
                {
                    message = "Energy Saving Mode limits brightness.";
                    return true;
                }
            }

            return false;
        }
    }
}
