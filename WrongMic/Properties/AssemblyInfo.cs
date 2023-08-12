﻿using System.Reflection;
using Kafe.WrongMic;
using Kafe.WrongMic.Properties;
using MelonLoader;


[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(WrongMic))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(WrongMic))]

[assembly: MelonInfo(
    typeof(WrongMic),
    nameof(Kafe.WrongMic),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    downloadLink: "https://github.com/kafeijao/Kafe_CVR_Mods"
)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
[assembly: MelonColor(255, 0, 255, 0)]
[assembly: MelonAuthorColor(255, 119, 77, 79)]

namespace Kafe.WrongMic.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "0.0.4";
    public const string Author = "kafeijao";
}
