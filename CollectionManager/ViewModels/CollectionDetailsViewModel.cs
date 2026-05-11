using System.Collections.ObjectModel;
using System.Windows.Input;
using CollectionManager.Models;

namespace CollectionManager.ViewModels;

public class CollectionDetailsViewModel : BaseViewModel
{
    private CollectionModel? _collection;

    public CollectionModel? Collection
    {
        get
        {
            return _collection;
        }
        set
        {
            _collection = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<CollectionItemModel> Items { get; set; }
        = new ObservableCollection<CollectionItemModel>();

    public ICommand AddItemCommand { get; }
    public ICommand EditItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand SummaryCommand { get; }

    public CollectionDetailsViewModel()
    {
        AddItemCommand = new Command(async () => await AddItemAsync());
        EditItemCommand = new Command<CollectionItemModel>(async (item) => await EditItemAsync(item));
        DeleteItemCommand = new Command<CollectionItemModel>(async (item) => await DeleteItemAsync(item));
        SummaryCommand = new Command(async () => await SummaryAsync());
    }

    public Task LoadAsync()
    {
        Collection = App.State.SelectedCollection;
        RefreshItems();

        return Task.CompletedTask;
    }

    public void RefreshItems()
    {
        Items.Clear();

        if (Collection == null)
            return;

        List<CollectionItemModel> sorted = Collection.Items
            .OrderBy(x => x.IsSold)
            .ThenBy(x => x.Name)
            .ToList();

        foreach (CollectionItemModel item in sorted)
        {
            Items.Add(item);
        }

        Collection.OnPropertyChanged(nameof(Collection.ItemsCount));
    }

    private async Task AddItemAsync()
    {
        App.State.SelectedItem = null;
        await Shell.Current.Navigation.PushAsync(new Views.CollectionItemEditPage());
    }

    private async Task EditItemAsync(CollectionItemModel item)
    {
        if (item == null)
            return;

        App.State.SelectedItem = item;
        await Shell.Current.Navigation.PushAsync(new Views.CollectionItemEditPage());
    }

    public async Task DeleteItemAsync(CollectionItemModel item)
    {
        if (Collection == null)
            return;

        if (item == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Usuwanie",
            "Czy na pewno usunąć element \"" + item.Name + "\"?",
            "Tak",
            "Nie");

        if (!confirm)
            return;

        if (Collection.Items.Contains(item))
        {
            Collection.Items.Remove(item);
            await SaveAsync();
            RefreshItems();
        }
    }

    private async Task SummaryAsync()
    {
        await Shell.Current.Navigation.PushAsync(new Views.CollectionSummaryPage());
    }

    public async Task AddItemToCollectionAsync(CollectionItemModel item)
    {
        if (Collection == null)
            return;

        int newId;

        if (Collection.Items.Count == 0)
            newId = 1;
        else
            newId = Collection.Items.Max(x => x.Id) + 1;

        item.Id = newId;

        Collection.Items.Add(item);
        await SaveAsync();
        RefreshItems();
    }

    public async Task UpdateItemInCollectionAsync(CollectionItemModel existingItem, CollectionItemModel sourceItem)
    {
        existingItem.Name = sourceItem.Name;
        existingItem.Price = sourceItem.Price;
        existingItem.Status = sourceItem.Status;
        existingItem.Rating = sourceItem.Rating;
        existingItem.Comment = sourceItem.Comment;

        await SaveAsync();
        RefreshItems();
    }

    public bool ExistsDuplicate(string name, int currentItemId)
    {
        if (Collection == null)
            return false;

        foreach (CollectionItemModel item in Collection.Items)
        {
            if (item.Id != currentItemId &&
                item.Name.Trim().ToLower() == name.Trim().ToLower())
            {
                return true;
            }
        }

        return false;
    }

    public async Task SaveAsync()
    {
        await App.State.DataService.SaveAsync(App.State.Collections);
    }
}