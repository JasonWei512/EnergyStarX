using Flurl;
using Windows.System;

namespace EnergyStarX.Helpers;

// UWP's EmailManager is not supported in WinUI 1.1
// https://github.com/microsoft/microsoft-ui-xaml/issues/7300
public static class EmailHelper
{
    private const string MailProtocolPrefix = "mailto:";

    public static async Task ShowEmail(string address, string subject, string body)
    {
        Uri mailUri = new Url(MailProtocolPrefix + address).SetQueryParams(new { subject, body }).ToUri();
        await Launcher.LaunchUriAsync(mailUri);
    }
}
