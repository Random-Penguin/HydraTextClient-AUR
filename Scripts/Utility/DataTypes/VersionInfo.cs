using System.Collections.Generic;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Utility.DataTypes;

public struct VersionInfo(int majorVersion, int minorVersion, int patchVersion, string extVersion, string[] content,
    Dictionary<string, string> fileHashes)
{
    public int MajorVersion = majorVersion;
    public int MinorVersion = minorVersion;
    public int PatchVersion = patchVersion;
    public string ExtVersion = extVersion;
    public string[] Content = content;
    public Dictionary<string, string> FileHashes = fileHashes;

    [JsonIgnore]
    public string VersionText => $"v{MajorVersion}.{MinorVersion}.{PatchVersion}{ExtVersion}";

    public static bool operator ==(VersionInfo i1, VersionInfo i2) => i1.MajorVersion == i2.MajorVersion
                                                                      && i1.MinorVersion == i2.MinorVersion
                                                                      && i1.PatchVersion == i2.PatchVersion
                                                                      && i1.ExtVersion == i2.ExtVersion;

    public static bool operator !=(VersionInfo i1, VersionInfo i2) => !(i1 == i2);

    public static bool operator >(VersionInfo i1, VersionInfo i2)
    {
        if (i1.MajorVersion != i2.MajorVersion) return i1.MajorVersion > i2.MajorVersion;
        if (i1.MinorVersion != i2.MinorVersion) return i1.MinorVersion > i2.MinorVersion;
        if (i1.PatchVersion != i2.PatchVersion) return i1.PatchVersion > i2.PatchVersion;
        var e1 = i1.ExtVersion;
        var e2 = i2.ExtVersion;
        if (e1 is "" || e2 is "") return e1 is "";
        if (e1 == e2) return false;
        if (e1[1] != e2[1]) return e1[1] > e2[1];
        if (e1.Contains('.') && !e2.Contains('.')) return true;
        if (!e1.Contains('.') && e2.Contains('.')) return false;
        var n1 = int.Parse(e1.Split('.')[1]);
        var n2 = int.Parse(e2.Split('.')[2]);
        return n1 > n2;
    }

    public static bool operator <(VersionInfo i1, VersionInfo i2) => !(i1 == i2 || i1 > i2);

    public static VersionInfo CreateFrom(string version, string[] content, Dictionary<string, string> fileHashes)
    {
        version.SplitVersionNumber(
            out var majorVersion, out var minorVersion, out var patchVersion, out var extVersion
        );
        return new VersionInfo(majorVersion, minorVersion, patchVersion, extVersion, content, fileHashes);
    }

    public static VersionInfo CreateEmpty(string version) => CreateFrom(version, [], []);
}