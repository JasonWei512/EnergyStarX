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
    private bool launchOnStartup = false;

    partial void OnLaunchOnStartupChanged(bool value)
    {
        if (Initializing) { return; }
        ToggleLaunchOnStartupCommand.Execute(value);
    }

    public bool ThrottleWhenPluggedIn
    {
        get => energyService.ThrottleWhenPluggedIn;
        set => SetProperty(ThrottleWhenPluggedIn, value, x => energyService.ThrottleWhenPluggedIn = x);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BypassProcessListModified))]
    [NotifyPropertyChangedFor(nameof(BypassProcessListEditorDialogTitle))]
    private string bypassProcessListString = LocalSettings.BypassProcessListString;

    // The line ending of user inputed text (from TextBox) is CRLF, while "LocalSettings.BypassProcessListString"'s is LF
    public bool BypassProcessListModified => BypassProcessListString.ReplaceLineEndings() != LocalSettings.BypassProcessListString.ReplaceLineEndings();

    public string BypassProcessListEditorDialogTitle =>
        "BypassProcessListEditorDialogTitle".GetLocalized()
        + (BypassProcessListModified ? $" ({"Modified".GetLocalized()})" : string.Empty);

    public event EventHandler? BypassProcessListEditorDialogShowRequested;

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
        LaunchOnStartup = startupTask.State == StartupTaskState.Enabled;

        Initializing = false;
    }

    [RelayCommand]
    private async Task ToggleLaunchOnStartup(bool enable)
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
    private void ShowBypassProcessListEditorDialog()
    {
        BypassProcessListString = LocalSettings.BypassProcessListString;
        BypassProcessListEditorDialogShowRequested?.Invoke(this, new EventArgs());
    }

    [RelayCommand]
    private void ApplyBypassProcessList()
    {
        energyService.ApplyAndSaveBypassProcessList(BypassProcessListString);
    }

    [RelayCommand]
    private async Task RestoreToDefaultBypassProcessList()
    {
        if (await dialogService.ShowConfirmationDialog("Restore_to_default_bypass_process_list".GetLocalized()))
        {
            energyService.ApplyAndSaveBypassProcessList("DefaultBypassProcessList".GetLocalized());
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
        string subject = $"{VersionDescription} {"Feedback".GetLocalized()}";
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

        return string.Join(Environment.NewLine, new[]
        {
            "----------",
            $"Windows: {Environment.OSVersion.Version}",
            $"Device: {new EasClientDeviceInformation().SystemProductName}",
            $"CPU: {JoinItems(hardware.CpuList, c => c.Name)}",
            $"RAM: {hardware.MemoryList.Select(m => m.Capacity).Aggregate((a, b) => a + b)/1024/1024} MB",
            $"GPU: {JoinItems(hardware.VideoControllerList, v => v.Name)}",
            $"Battery: {(hardware.BatteryList.Count > 0 ? "Yes" : "No")}",
            "----------",
            "",
            "",
            ""
        });
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

        return $"{"AppDisplayName".GetLocalized()} ({Package.Current.Id.Architecture}) - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
