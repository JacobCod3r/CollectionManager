using System.Collections.ObjectModel;
using System.Windows.Input;
using CollectionManager.Models;

namespace CollectionManager.ViewModels;

public class CollectionsViewModel : BaseViewModel
{
    public ObservableCollection<CollectionModel> Collections
    {
        get
        {
            return App.State.Collections;
        }
    }

    public ICommand AddCollectionCommand { get; }
    public ICommand EditCollectionCommand { get; }
    public ICommand OpenCollectionCommand { get; }
    public ICommand DeleteCollectionCommand { get; }
    public ICommand ExportCollectionCommand { get; }
    public ICommand ImportCollectionCommand { get; }

    public CollectionsViewModel()
    {
        AddCollectionCommand = new Command(async () => await AddCollectionAsync());
        EditCollectionCommand = new Command<CollectionModel>(async (collection) => await EditCollectionAsync(collection));
        OpenCollectionCommand = new Command<CollectionModel>(async (collection) => await OpenCollectionAsync(collection));
        DeleteCollectionCommand = new Command<CollectionModel>(async (collection) => await DeleteCollectionAsync(collection));
        ExportCollectionCommand = new Command<CollectionModel>(async (collection) => await ExportCollectionAsync(collection));
        ImportCollectionCommand = new Command(async () => await ImportCollectionAsync());
    }

    public async Task LoadAsync()
    {
        Collections.Clear();

        List<CollectionModel> loadedCollections = await App.State.DataService.LoadAsync();

        foreach (CollectionModel collection in loadedCollections)
        {
            collection.OnPropertyChanged(nameof(collection.ItemsCount));
            Collections.Add(collection);
        }
    }

    private async Task AddCollectionAsync()
    {
        string name = await Shell.Current.DisplayPromptAsync("Nowa kolekcja", "Podaj nazwę kolekcji:");

        if (string.IsNullOrWhiteSpace(name))
            return;

        string type = await Shell.Current.DisplayPromptAsync("Typ", "Podaj typ kolekcji:", initialValue: "Książki");

        if (string.IsNullOrWhiteSpace(type))
            type = "Inne";

        string description = await Shell.Current.DisplayPromptAsync("Opis", "Podaj opis kolekcji:");

        if (description == null)
            description = string.Empty;

        int newId;

        if (Collections.Count == 0)
            newId = 1;
        else
            newId = Collections.Max(x => x.Id) + 1;

        CollectionModel collection = new CollectionModel();
        collection.Id = newId;
        collection.Name = name.Trim();
        collection.Type = type.Trim();
        collection.Description = description.Trim();

        Collections.Add(collection);
        await SaveAsync();
    }

    private async Task EditCollectionAsync(CollectionModel collection)
    {
        if (collection == null)
            return;

        string name = await Shell.Current.DisplayPromptAsync("Edycja kolekcji", "Podaj nazwę kolekcji:", initialValue: collection.Name);

        if (string.IsNullOrWhiteSpace(name))
            return;

        string type = await Shell.Current.DisplayPromptAsync("Typ", "Podaj typ kolekcji:", initialValue: collection.Type);

        if (string.IsNullOrWhiteSpace(type))
            type = collection.Type;

        string description = await Shell.Current.DisplayPromptAsync("Opis", "Podaj opis kolekcji:", initialValue: collection.Description);

        if (description == null)
            description = collection.Description;

        collection.Name = name.Trim();
        collection.Type = type.Trim();
        collection.Description = description.Trim();

        await SaveAsync();
    }

    private async Task OpenCollectionAsync(CollectionModel collection)
    {
        if (collection == null)
            return;

        App.State.SelectedCollection = collection;
        await Shell.Current.Navigation.PushAsync(new Views.CollectionDetailsPage());
    }

    private async Task DeleteCollectionAsync(CollectionModel collection)
    {
        if (collection == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Usuwanie",
            "Czy na pewno usunąć kolekcję \"" + collection.Name + "\"?",
            "Tak",
            "Nie");

        if (!confirm)
            return;

        if (Collections.Contains(collection))
        {
            Collections.Remove(collection);
            await App.State.DataService.DeleteCollectionFilesAsync(collection.Id);
            await SaveAsync();
        }
    }

    private async Task ExportCollectionAsync(CollectionModel collection)
    {
        if (collection == null)
            return;

        string exportPath = await App.State.ImportExportService.ExportCollectionAsync(collection);

        if (string.IsNullOrWhiteSpace(exportPath))
            await Shell.Current.DisplayAlert("Eksport", "Nie udało się wyeksportować kolekcji.", "OK");
        else
            await Shell.Current.DisplayAlert("Eksport", "Eksport zapisano w:\n" + exportPath, "OK");
    }

    private async Task ImportCollectionAsync()
    {
        CollectionModel? importedCollection = await App.State.ImportExportService.ImportCollectionAsync();

        if (importedCollection == null)
        {
            await Shell.Current.DisplayAlert("Import", "Import anulowany albo nieudany.", "OK");
            return;
        }

        App.State.ImportExportService.MergeCollection(importedCollection);
        await SaveAsync();

        await Shell.Current.DisplayAlert("Import", "Import zakończony.", "OK");
    }

    public async Task SaveAsync()
    {
        await App.State.DataService.SaveAsync(Collections);

        foreach (CollectionModel collection in Collections)
        {
            collection.OnPropertyChanged(nameof(collection.ItemsCount));
        }
    }
}