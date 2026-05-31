using Common.DTOs;
using Common.Models;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Client.Helpers
{
    public static class ValidateCommand
    {
        public static bool CommandValidation(Regex regex, KeyValuePair<int, Function> selectedFunction, string text, Dashboard parentWindow, Device device)
        {
            if (regex.IsMatch(selectedFunction.Value.Name))
            {
                if (!Int32.TryParse(text, out int value))
                {
                    parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value must be a number", NotificationType.Error));
                    return false;
                }
                if (device.Functions.FirstOrDefault(x => x.Value.Name.Equals("state")).Value.Value.Equals("OFF"))
                {
                    parentWindow?.ShowToastNotification(new ToastNotification("Error", "First,you can turn on a device", NotificationType.Error));
                    return false;
                }
                return true;

            }
            else
            {
                if (device.Name.Contains("Door") || device.Name.Contains("Vault"))
                {
                    if (selectedFunction.Value.Value.Equals("OPEN"))
                    {
                        if (text.Equals("OPEN") || text.Equals("CLOSED"))
                        {
                            if (!text.Equals("CLOSED"))
                            {
                                parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device state has already OPEN", NotificationType.Error));
                                return false;
                            }
                        }
                        else
                        {
                            parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device must be OPEN/CLOSED", NotificationType.Error));
                            return false;
                        }
                    }
                    else
                    {
                        if (text.Equals("OPEN") || text.Equals("CLOSED"))
                        {

                            if (!text.Equals("OPEN"))
                            {
                                parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device state has already CLOSED", NotificationType.Error));
                                return false;
                            }
                        }
                        else
                        {
                            parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device must be OPEN/CLOSED", NotificationType.Error));
                            return false;
                        }

                    }
                }
                else
                {
                    if (selectedFunction.Value.Value.Equals("ON"))
                    {
                        if (text.Equals("ON") || text.Equals("OFF"))
                        {
                            if (!text.Equals("OFF"))
                            {
                                parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device state has already ON", NotificationType.Error));
                                return false;
                            }
                        }
                        else
                        {
                            parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device must be ON/OFF", NotificationType.Error));
                            return false;
                        }
                    }
                    else
                    {
                        if (text.Equals("ON") || text.Equals("OFF"))
                        {
                            if (!text.Equals("ON"))
                            {
                                parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device state has already OFF", NotificationType.Error));
                                return false;
                            }
                        }
                        else
                        {
                            parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device must be ON/OFF", NotificationType.Error));
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}
