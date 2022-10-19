using DependencyPropertyGenerator;
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
    public DependencyObject? AssociatedObject { get; private set; }

    private TextBlock? textBlock => AssociatedObject as TextBlock;

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
        // TODO: Realase resource and unregister event handlers
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

        textBlock?.Inlines.Clear();

        if (newValue is not null)
        {
            foreach (string line in newValue)
            {
                textBlock?.Inlines.Add(new Run() { Text = line + Environment.NewLine });
            }

            newValue.CollectionChanged += ObservableCollectionChanged;
        }
    }

    // Currently it only supports ObservableCollection's "Add", "Remove" and "Reset (Clear)" action.
    private void ObservableCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems is not null)
            {
                foreach (string newLine in e.NewItems.OfType<string>())
                {
                    textBlock?.Inlines.Add(new Run() { Text = newLine + Environment.NewLine });
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            textBlock?.Inlines.Clear();
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            textBlock?.Inlines.RemoveAt(e.OldStartingIndex);            
        }
        else
        {
            throw new NotImplementedException($@"Currently {nameof(TextBlockBehavior)} only supports ObservableCollection's ""Add"", ""Remove"" and ""Reset (Clear)"" action.");
        }
    }
}
