# Stratum <img src="https://raw.githubusercontent.com/H3VR-Modding/Stratum/main/media/icon/128.png" height="92" align="left" />

[![releases](https://img.shields.io/github/v/release/H3VR-Modding/Stratum)](https://github.com/H3VR-Modding/Stratum/releases) [![nuget](https://img.shields.io/nuget/v/Stratum)](https://www.nuget.org/packages/Stratum) [![codecov](https://codecov.io/gh/H3VR-Modding/Stratum/branch/main/graph/badge.svg?token=ZEAV3UHK8S)](https://codecov.io/gh/H3VR-Modding/Stratum) [![chat](https://img.shields.io/discord/777351065950879744?label=chat&logo=discord&logoColor=white)](https://discord.com/invite/g8xeFyt42j)

Stratum is a lite stage and asset management framework, designed with [Thunderstore](https://thunderstore.io) in mind. Combined with [Mason](https://github.com/H3VR-Modding/Mason), they are an evolution of the pre-Thunderstore asset management framework, [Deli](https://github.com/Deli-Collective/Deli).

## Installation
Stratum has [a Thunderstore package](https://h3vr.thunderstore.io/package/Stratum/Stratum), otherwise it can be downloaded from [the releases section](https://github.com/H3VR-Modding/Stratum/releases).

## Usage
Stratum is intended to be used as a library, so for non-developers, there is nothing to use Stratum for.  
For developers, simply add the `Stratum` NuGet package to your project. Be sure to copy [the nuget.config file](nuget.config) from this repository as well, as Stratum references libraries from BepInEx's NuGet source.  
Inherit from `StratumPlugin` and apply the usual BepInEx attributes. Then, implement the two stage callbacks. These callbacks pass an `IStageContext<TRet>` specific to your plugin, to which you can add loaders and access other plugins' contexts.
