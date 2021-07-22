using MelonLoader;
using Main = ADOLib.ADOLib;

#region Assembly attributes

[assembly: MelonInfo(typeof(Main), ModInfo.Name, ModInfo.Version, ModInfo.Author, ModInfo.DownloadLink)]
[assembly: MelonColor()]
[assembly: MelonGame(null, null)]

#endregion

public static class ModInfo
{
    public const string Name = "ADOLib";
    public const string Version = ADOLib.ADOLib._Version;
    public const string Author = "PERIOT";
    public const string DownloadLink = null;
}