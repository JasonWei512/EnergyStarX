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
    }

    protected override void OnDetaching()
    {
        AssociatedObject.LinkClicked -= AssociatedObject_LinkClicked;
        base.OnDetaching();
    }

    private async void AssociatedObject_LinkClicked(object? sender, LinkClickedEventArgs e)
    {
        if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri? link))
        {
            await Launcher.LaunchUriAsync(link);
        }
    }
}
