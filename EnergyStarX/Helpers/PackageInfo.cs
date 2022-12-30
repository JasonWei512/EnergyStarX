using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel;

namespace EnergyStarX.Helpers;

public class PackageInfo
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

    public static bool IsMSIX
    {
        get
        {
            int length = 0;

            return GetCurrentPackageFullName(ref length, null) != 15700L;
        }
    }

    public static Version Version
    {
        get
        {
            Version version;

            if (IsMSIX)
            {
                PackageVersion packageVersion = Package.Current.Id.Version;
                version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }
            else
            {
                version = Assembly.GetExecutingAssembly().GetName().Version!;
            }

            return version;
        }
    }

    public static string VersionString
    {
        get
        {
            Version version = Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
