using ABI_RC.Core;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Vivox;
using HarmonyLib;
using MelonLoader;

namespace Kafe.WrongMic;

public class WrongMic : MelonMod {

    private const string MicSettingName = "AudioInputDevice";

    public override void OnInitializeMelon() {
        ModConfig.InitializeMelonPrefs();
    }

    private static void SetSettingAudioInputSilently(string microphoneName) {
        if (!MetaPort.Instance.settings._settings.TryGetValue(MicSettingName, out var setting)) {
            MelonLogger.Error($"[SetSettingAudioInputSilently] Something went wrong, we failed to find the Microphone Setting...");
            return;
        }
        setting.SetValueString(microphoneName);
        MetaPort.Instance.settings.settingsHaveChanged = true;
        VivoxCvarHandler.InputDevice.Value = microphoneName;
    }

    [HarmonyPatch]
    internal class HarmonyPatches {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CVRTools), nameof(CVRTools.ConfigureHudAffinity))]
        public static void After_CVRTools_ConfigureHudAffinity() {
            // This gets called after DesktopVRSwitch mod did its thing
            if (RootLogic.Instance != null) After_RootLogic_Start(RootLogic.Instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RootLogic), nameof(RootLogic.Start))]
        public static void After_RootLogic_Start(RootLogic __instance) {
            try {
                if (MetaPort.Instance.isUsingVr) {
                    if (ModConfig.MeMicVR.Value.Equals(ModConfig.Undefined)) {
                        ModConfig.MeMicVR.Value = MetaPort.Instance.settings.GetSettingsString(MicSettingName);
                        MelonLogger.Warning($"There's no mic configured for VR. Going to set to the current mic [{ModConfig.MeMicVR.Value}]. " +
                                            $"If it's wrong just change on the Game's normal menus.");
                    }
                    else {
                        SetSettingAudioInputSilently(ModConfig.MeMicVR.Value);
                    }
                }
                else {
                    if (ModConfig.MeMicDesktop.Value.Equals(ModConfig.Undefined)) {
                        ModConfig.MeMicDesktop.Value = MetaPort.Instance.settings.GetSettingsString(MicSettingName);
                        MelonLogger.Warning($"There's no mic configured for Desktop. Going to set to the current mic [{ModConfig.MeMicDesktop.Value}]. " +
                                            $"If it's wrong just change on the Game's normal menus.");
                    }
                    else {
                        SetSettingAudioInputSilently(ModConfig.MeMicDesktop.Value);
                    }
                }
            }
            catch (Exception e) {
                MelonLogger.Error($"Error during the patched function {nameof(After_RootLogic_Start)}.");
                MelonLogger.Error(e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RootLogic), nameof(RootLogic.SettingsStringChanged))]
        public static void After_RootLogic_SettingsStringChanged(RootLogic __instance, string name, string value) {
            try {
                if (name != "AudioInputDevice") return;
                if (MetaPort.Instance.isUsingVr) {
                    ModConfig.MeMicVR.Value = value;
                    MelonLogger.Msg($"Set the VR Microphone to: {value}");
                }
                else {
                    ModConfig.MeMicDesktop.Value = value;
                    MelonLogger.Msg($"Set the Desktop Microphone to: {value}");
                }
                ModConfig.MelonCategory.SaveToFile(false);
            }
            catch (Exception e) {
                MelonLogger.Error($"Error during the patched function {nameof(After_RootLogic_Start)}.");
                MelonLogger.Error(e);
            }
        }
    }
}
