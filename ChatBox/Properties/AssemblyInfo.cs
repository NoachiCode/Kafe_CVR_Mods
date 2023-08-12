﻿using System.Reflection;
using Kafe.ChatBox;
using Kafe.ChatBox.Properties;
using MelonLoader;


[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(Kafe.ChatBox))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(Kafe.ChatBox))]

[assembly: MelonInfo(
    typeof(ChatBox),
    nameof(Kafe.ChatBox),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    downloadLink: "https://github.com/kafeijao/Kafe_CVR_Mods"
)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
[assembly: MelonColor(255, 0, 255, 0)]
[assembly: MelonAuthorColor(255, 119, 77, 79)]
[assembly: MelonAdditionalDependencies(AssemblyInfoParams.BTKUILibName)]
[assembly: MelonAdditionalCredits(AssemblyInfoParams.AstroDoge)]

namespace Kafe.ChatBox.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.0.10";
    public const string Author = "kafeijao";
    public const string AstroDoge = "AstroDoge";
    public const string BTKUILibName = "BTKUILib";
}
