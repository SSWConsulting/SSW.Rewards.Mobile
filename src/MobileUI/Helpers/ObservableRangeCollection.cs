using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SSW.Rewards.Mobile.Helpers;

/// <summary>
/// Observable collection which can Add, Remove and Replace a range of collection
/// without firing PropertyChanged event on every action.
/// Adopted from https://github.com/xamarin/XamarinCommunityToolkit/blob/main/src/CommunityToolkit/Xamarin.CommunityToolkit/ObjectModel/ObservableRangeCollection.shared.cs
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    public ObservableRangeCollection()
    {
    }

    public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        base.OnCollectionChanged(args);
    }

    public void AddRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        CheckReentrancy();

        var startIndex = Count;
        var itemsAdded = false;
        foreach (var item in collection)
        {
            Items.Add(item);
            itemsAdded = true;
        }

        if (itemsAdded)
        {
            RaiseChangeNotificationEvents(
                NotifyCollectionChangedAction.Add,
                changedItems: (List<T>)collection,
                startingIndex: startIndex);
        }
    }

    public void ReplaceRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        CheckReentrancy();

        var previouslyEmpty = Items.Count == 0;

        var itemsAdded = false;
        Items.Clear();
        foreach (var item in collection)
        {
            Items.Add(item);
            itemsAdded = true;
        }

        var currentlyEmpty = Items.Count == 0;

        if (previouslyEmpty && currentlyEmpty)
            return;

        if (itemsAdded)
        {
            RaiseChangeNotificationEvents(NotifyCollectionChangedAction.Reset);
        }
    }

    public void RemoveRange(IEnumerable<T> collection)
    {
        throw new NotImplementedException(
            $"{nameof(RemoveRange)} method is not implemented for {nameof(ObservableRangeCollection<T>)}");
    }

    private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, List<T>? changedItems = null, int startingIndex = -1)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        OnCollectionChanged(changedItems == null
            ? new NotifyCollectionChangedEventArgs(action)
            : new NotifyCollectionChangedEventArgs(action, changedItems: changedItems, startingIndex: startingIndex));
    }
}