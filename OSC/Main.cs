﻿using ABI_RC.Systems.InputManagement;
using Kafe.OSC.Components;
using Kafe.OSC.Handlers;
using Kafe.OSC.Properties;
using Kafe.OSC.Utils;
using MelonLoader;

namespace Kafe.OSC;

public class OSC : MelonMod {

    public static OSC Instance { get; private set; }

    private MelonPreferences_Category _mcOsc;

    // Connection
    public MelonPreferences_Entry<int> meOSCServerInPort;
    public MelonPreferences_Entry<string> meOSCServerOutIp;
    public MelonPreferences_Entry<int> meOSCServerOutPort;
    public MelonPreferences_Entry<bool> meOSCServerOutUseBundles;

    // Avatar Module
    public MelonPreferences_Entry<bool> meOSCAvatarModule;
    public MelonPreferences_Entry<bool> meOSCAvatarModuleTriggers;
    public MelonPreferences_Entry<bool> meOSCAvatarModuleSetAvatar;
    public MelonPreferences_Entry<bool> meOSCAvatarModuleBypassJsonConfig;

    // Json config
    public MelonPreferences_Entry<bool> meOSCJsonConfigUuidPrefixes;
    public MelonPreferences_Entry<bool> meOSCJsonConfigAlwaysReplace;
    public MelonPreferences_Entry<bool> meOSCJsonConfigOverridePathEnabled;
    public MelonPreferences_Entry<string> meOSCJsonConfigOverridePath;

    // Input Module
    public MelonPreferences_Entry<bool> meOSCInputModule;
    public MelonPreferences_Entry<List<string>> meOSCInputModuleBlacklist;

    // Spawnable Module
    public MelonPreferences_Entry<bool> meOSCSpawnableModule;

    // Tracking Module
    public MelonPreferences_Entry<bool> meOSCTrackingModule;
    public MelonPreferences_Entry<float> meOSCTrackingModuleUpdateInterval;

    // Chat Box Module
    public MelonPreferences_Entry<bool> meOSCChatBoxModule;

    // Misc
    public MelonPreferences_Entry<bool> meOSCDebug;
    public MelonPreferences_Entry<bool> meOSCDebugConfigWarnings;
    public MelonPreferences_Entry<bool> meOSCPerformanceMode;
    public MelonPreferences_Entry<bool> meOSCCompatibilityVRCFaceTracking;

    private HandlerOsc _handlerOsc;

    private const string RugOscCoreLogicalName = "Rug.Osc.Core.dll";

    // public override void OnEarlyInitializeMelon() {
    //     // Save the Rug.Osc.Core lib into UserLibs
    //     try {
    //         var path = Path.GetFullPath(Path.Combine("UserLibs", RugOscCoreLogicalName));
    //         using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RugOscCoreLogicalName);
    //         using var fileStream = new FileStream(path, FileMode.Create);
    //         stream!.CopyTo(fileStream);
    //     }
    //     catch (Exception) {
    //         MelonLogger.Error($"Failed exporting {RugOscCoreLogicalName} to /UserLibs/.");
    //         throw;
    //     }
    // }

    public override void OnInitializeMelon() {

        Instance = this;

        // Melon Config
        _mcOsc = MelonPreferences.CreateCategory(nameof(OSC));


        // OSC Server
        meOSCServerInPort = _mcOsc.CreateEntry("ServerInPort", 9000,
            description: "Port that external programs should use to send messages into CVR.",
            oldIdentifier: "inPort");

        meOSCServerOutIp = _mcOsc.CreateEntry("ServerOutIp", "127.0.0.1",
            description: "Ip of the external program to listen to messages from CVR.",
            oldIdentifier: "outIp");

        meOSCServerOutPort = _mcOsc.CreateEntry("ServerOutPort", 9001,
            description: "Port that external programs should use listen to messages from CVR.",
            oldIdentifier: "outPort");

        meOSCServerOutUseBundles = _mcOsc.CreateEntry("ServerOutUseBundles", false,
            description: "Whether the OSC Messages should be sent in Bundles (OSC Spec) or not. Bundles are limited " +
                         "to 10 OSC messages at a time (to avoid hitting the max packet size limit). This should " +
                         "improve the performance as results in less network requests, but some OSC apps might not " +
                         "work.");




        // Avatar Module
        meOSCAvatarModule = _mcOsc.CreateEntry("AvatarModule", true,
            description: "Whether the mod is listening/writing for avatar parameters/changes on the /avatar/ address or not.");

        meOSCAvatarModuleTriggers = _mcOsc.CreateEntry("AvatarModuleTriggerParameters", false,
            description: "Whether the mod listens and sends events for trigger parameters (by sending/receiving osc " +
                         "messages with just the address and no value) or not.", oldIdentifier: "enableTriggerParameter");

        meOSCAvatarModuleSetAvatar = _mcOsc.CreateEntry("AvatarModuleSetAvatar", true,
            description: "Whether the mod accepts avatar change requests, by sending the avatar guid to the address: " +
                         "/avatar/change", oldIdentifier: "enableSetAvatar");

        meOSCAvatarModuleBypassJsonConfig = _mcOsc.CreateEntry("AvatarModuleBypassJsonConfig", true,
            description: "Whether the mod allows parameters to be set/read without being in the json config or not.");

        // Avatar OSC json Configs
        meOSCJsonConfigUuidPrefixes = _mcOsc.CreateEntry("JsonConfigUuidPrefixes", true,
            description: "Whether the mod should prefix the OSC config file path/name with usr_ and avtr_",
            oldIdentifier: "addUuidPrefixes");

        meOSCJsonConfigAlwaysReplace = _mcOsc.CreateEntry("JsonConfigReplaceEvenIfExists", true,
            description: "Whether the mod should replace the avatar OSC config (if already exists) when loading into the avatar.",
            oldIdentifier: "replaceConfigIfExists");

        meOSCJsonConfigOverridePathEnabled = _mcOsc.CreateEntry("JsonConfigOverridePathEnable", false,
            description: "Whether the mode should use the override path to store/read OSC configs.",
            oldIdentifier: "enableOverridePath");

        var defaultVrcFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"),
            "VRChat", "VRChat");
        meOSCJsonConfigOverridePath = _mcOsc.CreateEntry("JsonConfigOverrideFolder", defaultVrcFolder,
            description: "The override path to store/read configs. Note: Only used if JsonConfigOverridePathEnable = true");


        // Input Module
        meOSCInputModule = _mcOsc.CreateEntry("InputModule", true,
            description: "Whether the mod is listening for inputs on the /input/ address or not.",
            oldIdentifier: "enableInputs");

        meOSCInputModuleBlacklist = _mcOsc.CreateEntry("InputModuleBlacklist", new List<string> { "Reload", "Respawn", "QuitGame" },
            description: "List of blocked inputs. Some of them can be quite annoying and abuse-able.",
            oldIdentifier: "inputBlacklist");


        // Spawnable
        meOSCSpawnableModule = _mcOsc.CreateEntry("PropModule", true,
            description: "Whether the mod will listen/write data for props on the /prop/ address or not.");


        // Tracking
        meOSCTrackingModule = _mcOsc.CreateEntry("TrackingModule", true,
            description: "Whether the mod will send tracking data updates on the /tracking/ address or not.");

        meOSCTrackingModuleUpdateInterval = _mcOsc.CreateEntry("TrackingModuleUpdateInterval", 0f,
            description: "Minimum of seconds between each tracking data update. Default: 0 (will update every frame) " +
                         "Eg: 0.050 will update every 50 milliseconds.");


        // Chat Box
        meOSCChatBoxModule = _mcOsc.CreateEntry("ChatBoxModule", true,
            description: "Whether the mod will listen on the /chatbox/ address or not.");


        // Misc
        meOSCDebug = _mcOsc.CreateEntry("Debug", false,
            description: "Whether should spam the console with debug messages or not.");
        meOSCDebugConfigWarnings = _mcOsc.CreateEntry("DebugConfigWarnings", true,
            description: "Whether it should warn everytime you send an osc msg  and it gets ignored because a config " +
                         "setting. (for example sending a trigger parameter when triggers are disabled).");
        meOSCPerformanceMode = _mcOsc.CreateEntry("PerformanceMode", false,
            description: "If performance mode is activated the mod will stop listening to most game events. " +
                         "This will result on a lot of the osc messages going out from our mod to not work. If you" +
                         "only want the mod to listen to osc messages going into CVR you should have this option on! " +
                         "As there's a lot of processing/msg sending when listening the all the game events.");
        meOSCCompatibilityVRCFaceTracking = _mcOsc.CreateEntry("VRCFaceTrackingCompatibility", false,
            description: "Whether should configure the mod to be compatible with VRCFaceTracking or not. Keep " +
                         "in mind that a lot of features this mod provide will be disabled, because VRCFaceTracking " +
                         "was implemented specifically for VRChat and breaks very easily. This will enable uuid " +
                         "prefixes, enable the override path, disable trigger parameters, change the override " +
                         "path to VRC's folder, disable prop interactions, and the tracking info endpoints.");


        // Load env variables (may change the melon config)
        EnvVariables.Load();

        // Set all options required for VRCFaceTracking
        void SetVrcFaceTrackingCompatibility() {
            if (!meOSCCompatibilityVRCFaceTracking.Value) return;
            // Enable the uuid prefixes, enable the override folder, disable trigger parameters, and set the override
            // folder to vrc, props interactions, and tracking info
            meOSCJsonConfigUuidPrefixes.Value = true;
            meOSCJsonConfigOverridePathEnabled.Value = true;
            meOSCAvatarModuleTriggers.Value = false;
            meOSCJsonConfigOverridePath.Value = defaultVrcFolder;
            meOSCTrackingModule.Value = false;
            meOSCSpawnableModule.Value = false;
            MelonLogger.Msg("[Config] Enabled VRCFaceTracking compatibility! The following features will be disabled:");
            MelonLogger.Msg("[Config] \t- Parameters of the trigger type");
            MelonLogger.Msg("[Config] \t- All props interactions");
            MelonLogger.Msg("[Config] \t- All tracking info endpoints");
        }
        meOSCCompatibilityVRCFaceTracking.OnEntryValueChanged.Subscribe((_, _) => SetVrcFaceTrackingCompatibility());
        SetVrcFaceTrackingCompatibility();

        // Attach OSC Input Module and handle their disabling/enabling
        Events.Scene.InputManagerCreated += () => {
            var inputModuleOsc = new InputModuleOSC();
            CVRInputManager.Instance.AddInputModule(inputModuleOsc);
            inputModuleOsc.InputEnabled = meOSCInputModule.Value;
            MelonLogger.Msg("[Input] OSC Input Module Initialized.");
            meOSCInputModule.OnEntryValueChanged.Subscribe((_, newValue) => inputModuleOsc.InputEnabled = newValue);
        };

        // Start OSC server
        _handlerOsc = new HandlerOsc();

        // Check for ChatBox
        if (RegisteredMelons.FirstOrDefault(m => m.Info.Name == AssemblyInfoParams.ChatBoxName) != null) {
            MelonLogger.Msg($"Detected ChatBox mod, we're adding the integration!");
            Integrations.ChatBox.InitializeChatBox();
        }
    }

    public override void OnApplicationQuit() {
        _handlerOsc.Close();
    }
}
