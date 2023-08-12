﻿using Kafe.OSC.Components;
using MelonLoader;
using Rug.Osc.Core;
using UnityEngine;

namespace Kafe.OSC.Handlers.OscModules;

public class Input : OscHandler {

    internal const string AddressPrefixInput = "/input/";

    private bool _enabled;
    private List<string> _inputBlacklist;
    private bool _debugConfigWarnings;

    public Input() {

        // Enable according to the config and setup the config listeners
        if (OSC.Instance.meOSCInputModule.Value) Enable();
        OSC.Instance.meOSCInputModule.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
            if (newValue && !oldValue) Enable();
            else if (!newValue && oldValue) Disable();
        });

        // Set the blacklist and listen for it's changes according to the config
        _inputBlacklist = OSC.Instance.meOSCInputModuleBlacklist.Value;
        OSC.Instance.meOSCInputModuleBlacklist.OnEntryValueChanged.Subscribe((_, newValue) => _inputBlacklist = newValue);

        // Handle the warning when blocked osc command by config
        _debugConfigWarnings = OSC.Instance.meOSCDebugConfigWarnings.Value;
        OSC.Instance.meOSCDebugConfigWarnings.OnEntryValueChanged.Subscribe((_, enabled) => _debugConfigWarnings = enabled);
    }

    internal sealed override void Enable() {
        _enabled = true;
    }

    internal sealed override void Disable() {
        _enabled = false;
    }

    internal sealed override void ReceiveMessageHandler(OscMessage oscMsg) {
        if (!_enabled) {
            if (_debugConfigWarnings) {
                MelonLogger.Msg($"[Config] Sent an osc msg to {AddressPrefixInput}, but this module is disabled " +
                                $"in the configuration file, so this will be ignored.");
            }
            return;
        }

        // Get only the first value and assume no values to be null
        var valueObj = oscMsg.Count > 0 ? oscMsg[0] : null;

        var inputName =oscMsg.Address.Substring(AddressPrefixInput.Length);

        // Reject blacklisted inputs (ignore case)
        if (_inputBlacklist.Contains(inputName, StringComparer.OrdinalIgnoreCase)) {
            if (_debugConfigWarnings) {
                MelonLogger.Msg($"[Config] The OSC config has {inputName} blacklisted. Edit the config to allow.");
            }
            return;
        }

        // Axes
        if (Enum.TryParse<AxisNames>(inputName, true, out var axisName)) {
            void UpdateAxisValue(AxisNames axis, float value) {
                if (value is > 1f or < -1f) {
                    MelonLogger.Msg($"[Error] The input name {inputName} is an Axis, so the allowed values " +
                                    $"are floats between -1f and 1f (inclusive). Value provided: {valueObj}");
                    return;
                }
                InputModuleOSC.InputAxes[axis] = value;
            }
            if (valueObj is float floatValue) UpdateAxisValue(axisName, floatValue);
            else if (valueObj is int intValue) UpdateAxisValue(axisName, intValue);
            else if (valueObj is string valueStr && float.TryParse(valueStr, out var valueFloat)) UpdateAxisValue(axisName, valueFloat);
            else UpdateAxisValue(axisName, float.NaN);
        }

        // Buttons
        else if (Enum.TryParse<ButtonNames>(inputName, true, out var buttonName)) {
            void UpdateButtonValue(ButtonNames button, bool? value) {
                if (!value.HasValue) {
                    MelonLogger.Msg($"[Error] The input name {inputName} is a Button, so the allowed values " +
                                    $"are booleans, that can be represented with bool values, 0 and 1, or " +
                                    $"true/false as a string. Value provided: {valueObj}");
                    return;
                }
                InputModuleOSC.InputButtons[button] = value.Value;
            }
            if (valueObj is bool boolValue) UpdateButtonValue(buttonName, boolValue);
            else if (valueObj is int intValue) UpdateButtonValue(buttonName, intValue == 1 ? true : intValue == 0 ? false : null);
            else if (valueObj is float floatValue) UpdateButtonValue(buttonName, Mathf.Approximately(floatValue, 1f) ? true : Mathf.Approximately(floatValue, 0f) ? false : null);
            else if (valueObj is string valueStr) {
                if (int.TryParse(valueStr, out var valueInt)) UpdateButtonValue(buttonName, valueInt == 1 ? true : valueInt == 0 ? false : null);
                else if (bool.TryParse(valueStr, out var valueBool)) UpdateButtonValue(buttonName, valueBool);
                else UpdateButtonValue(buttonName, null);
            }
            else UpdateButtonValue(buttonName, null);
        }

        // Values
        else if (Enum.TryParse<ValueNames>(inputName, true, out var valueName)) {
            void UpdateValueValue(ValueNames valName, float value) {
                if (float.IsNaN(value)) {
                    MelonLogger.Msg($"[Error] The input name {inputName} is a Value, so the allowed values " +
                                    $"are any numbers floats/ints. Value provided: {valueObj}");
                    return;
                }
                InputModuleOSC.InputValues[valName] = value;
            }
            if (valueObj is float floatValue) UpdateValueValue(valueName, floatValue);
            else if (valueObj is int intValue) UpdateValueValue(valueName, intValue);
            else if (valueObj is string valueStr && float.TryParse(valueStr, out var valueFloat)) UpdateValueValue(valueName, valueFloat);
            else UpdateValueValue(valueName, float.NaN);
        }

        // Yes
        else {
            MelonLogger.Msg($"[Error] The input name {inputName} is not supported! \n" +
                            $"Supported Axis Names: {Enum.GetNames(typeof(AxisNames))}, \n" +
                            $"Supported Button Names: {Enum.GetNames(typeof(ButtonNames))}, \n" +
                            $"Supported Value Names: {Enum.GetNames(typeof(ValueNames))}.");
        }
    }

}
