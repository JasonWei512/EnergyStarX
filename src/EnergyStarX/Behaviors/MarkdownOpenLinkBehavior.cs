using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Xaml.Interactivity;
using Windows.System;

namespace EnergyStarX.Behaviors;

public class MarkdownOpenLinkBehavior : Behavior<MarkdownTextBlock>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.LinkClicked += AssociatedObject_LinkClicked;
        AssociatedObject.ImageClicked += AssociatedObject_ImageClicked;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.LinkClicked -= AssociatedObject_LinkClicked;
        AssociatedObject.ImageClicked -= AssociatedObject_ImageClicked;
        base.OnDetaching();
    }

    private async void AssociatedObject_LinkClicked(object? sender, LinkClickedEventArgs e)
    {
        await TryOpenLink(e.Link);
    }

    private async void AssociatedObject_ImageClicked(object? sender, LinkClickedEventArgs e)
    {
        await TryOpenLink(e.Link);
    }

    private async Task TryOpenLink(string link)
    {
        if (Uri.TryCreate(link, UriKind.Absolute, out Uri? uri))
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
