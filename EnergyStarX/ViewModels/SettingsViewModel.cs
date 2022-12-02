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

    [ObservableProperty]
    private bool initializing = true;

    [ObservableProperty]
    private string versionDescription;

    [ObservableProperty]
    private bool runAtStartup = false;

    partial void OnRunAtStartupChanged(bool value)
    {
        if (Initializing) { return; }
        ToggleRunAtStartupCommand.Execute(value);
    }

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

    public SettingsViewModel(EnergyService energyService, DialogService dialogService)
    {
        versionDescription = GetVersionDescription();
        this.energyService = energyService;
        this.dialogService = dialogService;

        _ = Initialize();
    }

    private async Task Initialize()
    {
        StartupTask? startupTask = await StartupTask.GetAsync(App.Guid);
        RunAtStartup = startupTask.State == StartupTaskState.Enabled;

        Initializing = false;
    }

    [RelayCommand]
    private async Task ToggleRunAtStartup(bool enable)
    {
        StartupTask? startupTask = await StartupTask.GetAsync(App.Guid);
        if (enable)
        {
            await startupTask.RequestEnableAsync();
        }
        else
        {
            startupTask.Disable();
        }
    }

    [RelayCommand]
    private void ShowProcessWhitelistEditorDialog()
    {
        ProcessWhitelistString = Settings.ProcessWhitelistString;
        ProcessWhitelistEditorDialogShowRequested?.Invoke(this, new EventArgs());
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

        await EmailHelper.ShowEmail(address, subject, body);
    }

    private Lazy<string> feedbackMailBody = new(() =>
    {
        HardwareInfo hardware = new();
        hardware.RefreshCPUList(false);
        hardware.RefreshMemoryList();
        hardware.RefreshVideoControllerList();
        hardware.RefreshBatteryList();

        string JoinItems<T>(IEnumerable<T> items, Func<T, string> selector) => items.Count() != 0 ? string.Join(" + ", items.Select(selector)) : "N/A";

        return $"""
            ----------
            Windows: {Environment.OSVersion.Version}
            Device: {new EasClientDeviceInformation().SystemProductName}
            CPU: {JoinItems(hardware.CpuList, c => c.Name)}
            RAM: {hardware.MemoryList.Select(m => m.Capacity).Aggregate((a, b) => a + b) / 1024 / 1024} MB
            GPU: {JoinItems(hardware.VideoControllerList, v => v.Name)}
            Battery: {(hardware.BatteryList.Count > 0 ? "Yes" : "No")}
            ----------



            """;
    });

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
