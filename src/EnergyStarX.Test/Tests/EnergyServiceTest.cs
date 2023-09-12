using EnergyStarX.Contracts.Services;
using EnergyStarX.Services;
using EnergyStarX.Test.Fakes;

namespace EnergyStarX.Test.Tests;

[TestClass]
public class EnergyServiceTest
{
    [DataTestMethod]
    [DynamicData(nameof(Data_ApplyAndSaveProcessBlacklist))]
    public void ApplyProcessBlacklist(string processBlacklistString, IReadOnlySet<string> expected)
    {
        IWindowService stubWindowService = new FakeWindowService();
        ISettingsService stubSettingsService = Mock.Of<ISettingsService>();
        EnergyService energyService = new(stubWindowService, stubSettingsService);

        energyService.ApplyProcessBlacklist(processBlacklistString);

        Assert.IsTrue(energyService.ProcessBlacklist.SetEquals(expected));
    }

    [DataTestMethod]
    [DynamicData(nameof(Data_ApplyAndSaveProcessBlacklist))]
    public void ApplyAndSaveProcessBlacklist(string processBlacklistString, IReadOnlySet<string> expected)
    {
        IWindowService stubWindowService = new FakeWindowService();
        Mock<ISettingsService> mockSettingsService = new Mock<ISettingsService>().SetupProperty(s => s.ProcessBlacklistString);
        EnergyService energyService = new(stubWindowService, mockSettingsService.Object);

        energyService.ApplyAndSaveProcessBlacklist(processBlacklistString);

        Assert.IsTrue(energyService.ProcessBlacklist.SetEquals(expected));
        mockSettingsService.VerifySet(s => s.ProcessBlacklistString = processBlacklistString);
    }

    public static DynamicDataSource<string, IReadOnlySet<string>> Data_ApplyAndSaveProcessBlacklist => Data_ParseProcessList;

    [DataTestMethod]
    [DynamicData(nameof(Data_ApplyAndSaveProcessWhitelist))]
    public void ApplyProcessWhitelist(string processWhitelistString, IReadOnlySet<string> expected)
    {
        IWindowService stubWindowService = new FakeWindowService();
        ISettingsService stubSettingsService = Mock.Of<ISettingsService>();
        EnergyService energyService = new(stubWindowService, stubSettingsService);

        energyService.ApplyProcessWhitelist(processWhitelistString);

        Assert.IsTrue(energyService.ProcessWhitelist.SetEquals(expected));
    }

    [DataTestMethod]
    [DynamicData(nameof(Data_ApplyAndSaveProcessWhitelist))]
    public void ApplyAndSaveProcessWhitelist(string processWhitelistString, IReadOnlySet<string> expected)
    {
        IWindowService stubWindowService = new FakeWindowService();
        Mock<ISettingsService> mockSettingsService = new Mock<ISettingsService>().SetupProperty(s => s.ProcessWhitelistString);
        EnergyService energyService = new(stubWindowService, mockSettingsService.Object);

        energyService.ApplyAndSaveProcessWhitelist(processWhitelistString);

        Assert.IsTrue(energyService.ProcessWhitelist.SetEquals(expected));
        mockSettingsService.VerifySet(s => s.ProcessWhitelistString = processWhitelistString);
    }

    public static DynamicDataSource<string, IReadOnlySet<string>> Data_ApplyAndSaveProcessWhitelist
    {
        get
        {
            DynamicDataSource<string, IReadOnlySet<string>> result = new();

            foreach (object?[] testDataRow in Data_ParseProcessList)
            {
                string processWhitelistString = (string)testDataRow[0]!;
                HashSet<string> expectedProcessWhitelist = ((IReadOnlySet<string>)testDataRow[1]!).ToHashSet();
#if DEBUG
                expectedProcessWhitelist.Add("devenv.exe");
#endif
                result.Add(processWhitelistString, expectedProcessWhitelist);
            }

            return result;
        }
    }

    public static DynamicDataSource<string, IReadOnlySet<string>> Data_ParseProcessList = new()
    {
        { "", new HashSet<string>() {} },
        { "    ", new HashSet<string>() {} },

        { "notepad.exe", new HashSet<string>() { "notepad.exe" } },
        { "    notepad.exe  ", new HashSet<string>() { "notepad.exe" } },
        { "notep?d.exe", new HashSet<string>() { "notep?d.exe" } },
        { "note*.exe", new HashSet<string>() { "note*.exe" } },

        { "// notepad.exe", new HashSet<string>() {} },
        { "//notepad.exe", new HashSet<string>() {} },
        { "  // notepad.exe    ", new HashSet<string>() {} },
        { "// notep?d.exe", new HashSet<string>() {} },
        { "// note*.exe", new HashSet<string>() {} },

        { "notepad.exe//mspaint.exe", new HashSet<string>() { "notepad.exe" } },
        { "notepad.exe//mspaint.exe//code.exe", new HashSet<string>() { "notepad.exe" } },
        { "  notep?d.exe  // mspa*nt.exe  // code.exe    ", new HashSet<string>() { "notep?d.exe" } },

        {
            """
            notepad.exe

            """,
            new HashSet<string>() { "notepad.exe" }
        },
        {
            """

            // Comment
            notepad.exe

            """,
            new HashSet<string>() { "notepad.exe" }
        },
        {
            """
            code.exe

            // Comment 
            notepad.exe
            mspaint.exe
            """,
            new HashSet<string>() { "code.exe", "notepad.exe", "mspaint.exe" }
        },
        {
            """
            // Comment
            code.exe
            //notepad.exe
            // mspaint.exe
            """,
            new HashSet<string>() { "code.exe" }
        },
        {
            """
            // Comment
            co?e.exe
            //notepad.exe
            powershell.exe
            // mspaint.exe
            v*m.exe
            """,
            new HashSet<string>() { "co?e.exe", "powershell.exe", "v*m.exe" }
        }
    };

    // I know I know testing private methods is bad
    [DataTestMethod]
    [DynamicData(nameof(Data_ShouldBypassProcess))]
    public void ShouldBypassProcess(string processName, ThrottleStatus throttleStatus, string processBlacklistString, string processWhitelistString, bool expected)
    {
        IWindowService stubWindowService = new FakeWindowService();
        ISettingsService stubSettingsService = Mock.Of<ISettingsService>();
        EnergyService energyService = new(stubWindowService, stubSettingsService);

        energyService.ApplyProcessBlacklist(processBlacklistString);
        energyService.ApplyProcessWhitelist(processWhitelistString);

        Assert.AreEqual(energyService.Invoke<bool>("ShouldBypassProcess", processName, throttleStatus), expected);
    }

    public static DynamicDataSource<string, ThrottleStatus, string, string, bool> Data_ShouldBypassProcess => new()
    {
        // Process not in blacklist or whitelist
        { "notepad.exe", ThrottleStatus.Stopped, "", "", true },
        { "notepad.exe", ThrottleStatus.OnlyBlacklist, "", "", true },
        { "notepad.exe", ThrottleStatus.BlacklistAndAllButWhitelist, "", "", false },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("mspaint.exe", "powershell.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("mspaint.exe", "powershell.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("mspaint.exe", "powershell.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        // (Wildcards) Process not in blacklist or whitelist
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("mspa?nt.exe", "powe*shell.exe"),
            Lines("c?d.exe", "p*sh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("mspa?nt.exe", "powe*shell.exe"),
            Lines("c?d.exe", "p*sh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("mspa?nt.exe", "powe*shell.exe"),
            Lines("c?d.exe", "p*sh.exe"),
            false
        },

        // Process only in blacklist
        { "notepad.exe", ThrottleStatus.Stopped, "notepad.exe", "", true },
        { "notepad.exe", ThrottleStatus.OnlyBlacklist, "notepad.exe", "", false },
        { "notepad.exe", ThrottleStatus.BlacklistAndAllButWhitelist, "notepad.exe", "", false },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("notepad.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("notepad.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("notepad.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        // (Wildcards) Process only in blacklist
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("note*.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("note*.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("note*.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("notep?d.exe", "note*.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("notep?d.exe", "note*.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("notep?d.exe", "note*.exe", "mspaint.exe"),
            Lines("cmd.exe", "pwsh.exe"),
            false
        },

        // Process only in whitelist
        { "notepad.exe", ThrottleStatus.Stopped, "", "notepad.exe", true },
        { "notepad.exe", ThrottleStatus.OnlyBlacklist, "", "notepad.exe", true },
        { "notepad.exe", ThrottleStatus.BlacklistAndAllButWhitelist, "", "notepad.exe", true },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notepad.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notepad.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notep?d.exe", "mspaint.exe"),
            true
        },
        // (Wildcards) Process only in whitelist
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notep?d.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notep?d.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notep?d.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("note*.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("note*.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("note*.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notep?d.exe", "note*.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notep?d.exe", "note*.exe", "mspaint.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("cmd.exe", "pwsh.exe"),
            Lines("notep?d.exe", "note*.exe", "mspaint.exe"),
            true
        },

        // Process in both blacklist and whitelist
        { "notepad.exe", ThrottleStatus.Stopped, "notepad.exe", "notepad.exe", true },
        { "notepad.exe", ThrottleStatus.OnlyBlacklist, "notepad.exe", "notepad.exe", false },
        { "notepad.exe", ThrottleStatus.BlacklistAndAllButWhitelist, "notepad.exe", "notepad.exe", false },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("notepad.exe", "mspaint.exe"),
            Lines("notepad.exe", "cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("notepad.exe", "mspaint.exe"),
            Lines("notepad.exe", "cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("notepad.exe", "mspaint.exe"),
            Lines("notepad.exe", "cmd.exe", "pwsh"),
            false
        },
        // (Wildcards) Process in both blacklist and whitelist
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("notep?d.exe", "cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("notep?d.exe", "cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("notep?d.exe", "cmd.exe", "pwsh"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("note*.exe", "cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("note*.exe", "cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("notep?d.exe", "mspaint.exe"),
            Lines("note*.exe", "cmd.exe", "pwsh"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("note*.exe", "mspaint.exe"),
            Lines("notep?d.exe", "cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("note*.exe", "mspaint.exe"),
            Lines("notep?d.exe", "cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("note*.exe", "mspaint.exe"),
            Lines("notep?d.exe", "cmd.exe", "pwsh"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.Stopped,
            Lines("note*.exe", "mspaint.exe"),
            Lines("note*.exe", "cmd.exe", "pwsh.exe"),
            true
        },
        {
            "notepad.exe",
            ThrottleStatus.OnlyBlacklist,
            Lines("note*.exe", "mspaint.exe"),
            Lines("note*.exe", "cmd.exe", "pwsh.exe"),
            false
        },
        {
            "notepad.exe",
            ThrottleStatus.BlacklistAndAllButWhitelist,
            Lines("note*.exe", "mspaint.exe"),
            Lines("note*.exe", "cmd.exe", "pwsh"),
            false
        },
    };

    private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);
}
