using Common.DTOs;
using Common.Repositories.DevicesRepositories;
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
                    message = "Night Mode limits climate temperature to 20°C.";
                    return true;
                }
            }

            if (_rules.Any(r => r.Name == "SecurityMode" && r.IsEnabled))
            {
                if ((command.SelectedDevice.Name.Contains("Door") ||
                     command.SelectedDevice.Name.Contains("Vault")) &&
                     command.Value == "OPEN")
                {
                    message = "Security Mode blocks door unlocking.";
                    return true;
                }
            }

            if (_rules.Any(r => r.Name == "EnergySaving" && r.IsEnabled))
            {
                if (command.Function == "brightness" &&
                    int.Parse(command.Value) > 50)
                {
                    message = "Energy Saving Mode limits brightness to 50°C.";
                    return true;
                }
            }

            return false;
        }
        public void ApplyRuleEffects(SmartRule rule, IDeviceRepository deviceRepository)
        {
            List<Device> devices = deviceRepository.GetAllDevices().ToList();
            if (rule.Name == "NightMode" && rule.IsEnabled)
            {
                foreach (var device in devices)
                {
                    foreach (var f in device.Functions)
                    {
                        if (f.Value.Name.Equals("temperature") && int.Parse(f.Value.Value) > 20)
                        {
                            f.Value.Value = "20";
                            deviceRepository.UpdateDeviceFunction(device.Id, f.Key, "temperature", "20");
                        }
                    }

                }
            }

            if (rule.Name == "EnergySaving" && rule.IsEnabled)
            {
                foreach (var device in devices)
                {
                    foreach (var f in device.Functions)
                    {
                        if (f.Value.Name.Equals("brightness") && int.Parse(f.Value.Value) > 50)
                        {
                            f.Value.Value = "50";
                            deviceRepository.UpdateDeviceFunction(device.Id, f.Key, "brightness", "50");
                        }
                    }

                }

            }
            if (rule.Name == "SecurityMode" && rule.IsEnabled)
            {
                foreach (var device in devices.Where(x => x.Name.Contains("Door") || x.Name.Contains("Vault")))
                {
                    foreach (var f in device.Functions)
                    {
                        if (f.Value.Name.Equals("state") && f.Value.Value.Equals("OPEN"))
                        {
                            f.Value.Value = "CLOSED";
                            deviceRepository.UpdateDeviceFunction(device.Id, f.Key, "state", "CLOSED");
                        }
                    }

                }

            }
        }
    }
}
