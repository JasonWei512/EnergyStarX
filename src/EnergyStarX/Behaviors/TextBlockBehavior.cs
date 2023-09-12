using DependencyPropertyGenerator;
using EnergyStarX.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Xaml.Interactivity;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace EnergyStarX.Behaviors;

// You cannot directly bind "TextBlock.Inlines" to a variable, because it's not a dependency property.
// So I create this behavior to work around this problem.
// Reference: https://stackoverflow.com/questions/1959856/data-binding-the-textblock-inlines
[DependencyProperty<ObservableCollection<string>>("BindableInlines")]
public partial class TextBlockBehavior : DependencyObject, IBehavior
{
    private readonly IWindowService windowService;

    private TextBlock? AssociatedTextBlock => AssociatedObject as TextBlock;

    public DependencyObject? AssociatedObject { get; private set; }

    public TextBlockBehavior()
    {
        windowService = App.GetService<IWindowService>();
        windowService.WindowShowing += WindowService_MainWindowShowing;
        windowService.WindowHiding += WindowService_MainWindowHiding;
    }

    // Called when AssociatedObject is added to xaml tree
    public void Attach(DependencyObject associatedObject)
    {
        if (AssociatedObject != associatedObject)
        {
            AssociatedObject = associatedObject;
        }
    }

    // Called when AssociatedObject is no long on xaml tree
    public void Detach()
    {
        // TODO: Release resource and unregister event handlers
        // Currently commented out, because LogPage's log TextBlock needs to log new lines even when current page is not LogPage.

        //if (BindableInlines is not null)
        //{
        //    BindableInlines.CollectionChanged -= ObservableCollectionChanged;
        //}
        //AssociatedObject = null;
    }

    partial void OnBindableInlinesChanged(ObservableCollection<string>? oldValue, ObservableCollection<string>? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.CollectionChanged -= ObservableCollectionChanged;
        }

        AssociatedTextBlock?.Inlines.Clear();

        if (newValue is not null)
        {
            if (windowService.WindowVisible)
            {
                StartUpdatingTextBlock(newValue);
            }
        }
    }

    private void WindowService_MainWindowShowing(object? sender, EventArgs e)
    {
        StartUpdatingTextBlock(BindableInlines);
    }

    private void WindowService_MainWindowHiding(object? sender, EventArgs e)
    {
        StopUpdatingTextBlock(BindableInlines);
    }

    private void StartUpdatingTextBlock(ObservableCollection<string>? bindableInlines)
    {
        if (bindableInlines is not null)
        {
            AssociatedTextBlock?.Inlines.Clear();
            foreach (string line in bindableInlines)
            {
                AddLineToTextBlock(AssociatedTextBlock, line);
            }

            bindableInlines.CollectionChanged -= ObservableCollectionChanged;
            bindableInlines.CollectionChanged += ObservableCollectionChanged;
        }
    }

    private void StopUpdatingTextBlock(ObservableCollection<string>? bindableInlines)
    {
        if (bindableInlines is not null)
        {
            AssociatedTextBlock?.Inlines.Clear();
            bindableInlines.CollectionChanged -= ObservableCollectionChanged;
        }
    }

    // Update TextBlock on UI when BindableInlines.CollectionChanged is triggered.
    // Currently it only supports ObservableCollection's "Add", "Remove" and "Reset (Clear)" action.
    private void ObservableCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems is not null)
            {
                foreach (string newLine in e.NewItems.OfType<string>())
                {
                    AddLineToTextBlock(AssociatedTextBlock, newLine);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            AssociatedTextBlock?.Inlines.Clear();
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            AssociatedTextBlock?.Inlines.RemoveAt(e.OldStartingIndex);
        }
        else
        {
            throw new NotImplementedException($@"Currently {nameof(TextBlockBehavior)} only supports ObservableCollection's ""Add"", ""Remove"" and ""Reset (Clear)"" action.");
        }
    }

    private void AddLineToTextBlock(TextBlock? textBlock, string line)
    {
        textBlock?.Inlines.Add(new Run() { Text = line + Environment.NewLine });
    }
}
