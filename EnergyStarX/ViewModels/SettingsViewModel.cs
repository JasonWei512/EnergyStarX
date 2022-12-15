using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyStarX.Helpers;
using EnergyStarX.Services;
using Hardware.Info;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System;

namespace EnergyStarX.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly EnergyService energyService;
    private readonly DialogService dialogService;
    private readonly StartupService startupService;

    [ObservableProperty]
    private bool initializing = true;

    [ObservableProperty]
    private string versionDescription;

    private bool runAtStartup;

    public bool RunAtStartup
    {
        get => runAtStartup;
        set
        {
            bool oldRunAtStartup = runAtStartup;
            bool oldRunAtStartupAsAdmin = runAtStartupAsAdmin;
            bool newRunAtStartup = value;
            bool newRunAtStartupAsAdmin = runAtStartupAsAdmin;

            if (SetProperty(ref runAtStartup, value))
            {
                OnPropertyChanged(nameof(IsRunAtStartupAsAdminToggleable));
                if (Initializing || IsTogglingRunAtStartup) { return; }
                _ = ToggleRunAtStartup(oldRunAtStartup, oldRunAtStartupAsAdmin, newRunAtStartup, newRunAtStartupAsAdmin);
            }
        }
    }

    private bool runAtStartupAsAdmin;

    public bool RunAtStartupAsAdmin
    {
        get => runAtStartupAsAdmin;
        set
        {
            bool oldRunAtStartup = runAtStartup;
            bool oldRunAtStartupAsAdmin = runAtStartupAsAdmin;
            bool newRunAtStartup = runAtStartup;
            bool newRunAtStartupAsAdmin = value;

            if (SetProperty(ref runAtStartupAsAdmin, value))
            {
                if (Initializing || IsTogglingRunAtStartup) { return; }
                _ = ToggleRunAtStartup(oldRunAtStartup, oldRunAtStartupAsAdmin, newRunAtStartup, newRunAtStartupAsAdmin);
            }
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRunAtStartupToggleable))]
    [NotifyPropertyChangedFor(nameof(IsRunAtStartupAsAdminToggleable))]
    private bool isTogglingRunAtStartup;

    public bool IsRunAtStartupToggleable => !IsTogglingRunAtStartup;

    public bool IsRunAtStartupAsAdminToggleable => RunAtStartup && !IsTogglingRunAtStartup;

    public bool ThrottleWhenPluggedIn
    {
        get => energyService.ThrottleWhenPluggedIn;
        set => SetProperty(ThrottleWhenPluggedIn, value, x => energyService.ThrottleWhenPluggedIn = x);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProcessWhitelistModified))]
    [NotifyPropertyChangedFor(nameof(ProcessWhitelistEditorDialogTitle))]
    private string processWhitelistString = Settings.ProcessWhitelistString;

    // The line ending of user inputed text (from TextBox) is CRLF, while "Settings.ProcessWhitelistString"'s is LF
    public bool ProcessWhitelistModified => ProcessWhitelistString.ReplaceLineEndings() != Settings.ProcessWhitelistString.ReplaceLineEndings();

    public string ProcessWhitelistEditorDialogTitle =>
        "ProcessWhitelistEditorDialogTitle".ToLocalized()
        + (ProcessWhitelistModified ? $" ({"Modified".ToLocalized()})" : string.Empty);

    public event EventHandler? ProcessWhitelistEditorDialogShowRequested;

    public SettingsViewModel(EnergyService energyService, DialogService dialogService, StartupService startupService)
    {
        versionDescription = GetVersionDescription();
        this.energyService = energyService;
        this.dialogService = dialogService;
        this.startupService = startupService;

        _ = Initialize();
    }

    private async Task Initialize()
    {
        StartupService.StartupType startupType = await startupService.GetStartupType();
        (RunAtStartup, RunAtStartupAsAdmin) = startupType switch
        {
            StartupService.StartupType.None => (false, false),
            StartupService.StartupType.User => (true, false),
            StartupService.StartupType.Admin => (true, true),
            _ => throw new ArgumentException("Unknown StartupService.StartupType")
        };

        Initializing = false;
    }

    [RelayCommand]
    private void ShowProcessWhitelistEditorDialog()
    {
        ProcessWhitelistString = Settings.ProcessWhitelistString;
        ProcessWhitelistEditorDialogShowRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ApplyProcessWhitelist()
    {
        energyService.ApplyAndSaveProcessWhitelist(ProcessWhitelistString);
    }

    [RelayCommand]
    private async Task RestoreToDefaultProcessWhitelist()
    {
        if (await dialogService.ShowConfirmationDialog("Restore_to_default_process_whitelist".ToLocalized()))
        {
            energyService.ApplyAndSaveProcessWhitelist("DefaultProcessWhitelist".ToLocalized());
        }
    }

    [RelayCommand]
    private async Task RateThisApp()
    {
        await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9NF7JTB3B17P"));
    }

    [RelayCommand]
    private async Task ContactTheDeveloper()
    {
        string address = "asknickjohn@outlook.com";
        string subject = $"{VersionDescription} {"Feedback".ToLocalized()}";
        string body = await Task.Run(() => feedbackMailBody.Value);

        string errorLogContent = await LogHelper.GetErrorLogContent();
        if (!string.IsNullOrEmpty(errorLogContent))
        {
            body += $"""
                ----------
                Error log:
                {errorLogContent}
                """;
        }

        await EmailHelper.ShowEmail(address, subject, body);
    }

    private Lazy<string> feedbackMailBody = new(() =>
    {
        HardwareInfo hardware = new();
        hardware.RefreshCPUList(false);
        hardware.RefreshMemoryList();
        hardware.RefreshVideoControllerList();
        hardware.RefreshBatteryList();

        string JoinItems<T>(IEnumerable<T> items, Func<T, string> selector) => items.Any() ? string.Join(" + ", items.Select(selector)) : "N/A";

        return $"""



            ----------
            Windows: {Environment.OSVersion.Version}
            Device: {new EasClientDeviceInformation().SystemProductName}
            CPU: {JoinItems(hardware.CpuList, c => c.Name)}
            RAM: {hardware.MemoryList.Select(m => m.Capacity).Aggregate((a, b) => a + b) / 1024 / 1024} MB
            GPU: {JoinItems(hardware.VideoControllerList, v => v.Name)}
            Battery: {(hardware.BatteryList.Count > 0 ? "Yes" : "No")}

            """;
    });

    private async Task ToggleRunAtStartup(
        bool oldRunAtStartup, bool oldRunAtStartupAsAdmin,
        bool newRunAtStartup, bool newRunAtStartupAsAdmin
        )
    {
        if (IsTogglingRunAtStartup) { return; }

        try
        {
            IsTogglingRunAtStartup = true;
            StartupService.StartupType startupType = (newRunAtStartup, newRunAtStartupAsAdmin) switch
            {
                (false, _) => StartupService.StartupType.None,
                (true, false) => StartupService.StartupType.User,
                (true, true) => StartupService.StartupType.Admin
            };

            bool startupTypeSetSuccessfully = await startupService.SetStartupType(startupType);

            if (!startupTypeSetSuccessfully)    // Rollback if setting startup type failed
            {
                RunAtStartup = oldRunAtStartup;
                RunAtStartupAsAdmin = oldRunAtStartupAsAdmin;
            }
        }
        finally
        {
            IsTogglingRunAtStartup = false;
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            PackageVersion packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".ToLocalized()} ({Package.Current.Id.Architecture}) - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
