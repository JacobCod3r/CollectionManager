using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CollectionManager.Enums;
using CollectionManager.Models;

namespace CollectionManager.Services;

public class DataService
{
    private readonly string _basePath;
    private readonly string _collectionsPath;

    public DataService()
    {
        _basePath = FileSystem.AppDataDirectory;
        _collectionsPath = Path.Combine(_basePath, "collections.txt");

#if DEBUG
        Debug.WriteLine("Ścieżka danych aplikacji:");
        Debug.WriteLine(_basePath);
#endif
    }

    public string GetBasePath()
    {
        return _basePath;
    }

    public async Task<List<CollectionModel>> LoadAsync()
    {
        EnsureFiles();

        List<CollectionModel> result = new List<CollectionModel>();
        string[] collectionLines = await File.ReadAllLinesAsync(_collectionsPath);

        foreach (string line in collectionLines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(';');

            if (parts.Length < 4)
                continue;

            CollectionModel collection = new CollectionModel();
            collection.Id = ParseInt(parts[0]);
            collection.Name = Restore(parts[1]);
            collection.Type = Restore(parts[2]);
            collection.Description = Restore(parts[3]);
            collection.Items = new ObservableCollection<CollectionItemModel>();

            await LoadItemsAsync(collection);
            result.Add(collection);
        }

        return result;
    }

    public async Task SaveAsync(IEnumerable<CollectionModel> collections)
    {
        EnsureFiles();

        List<string> lines = new List<string>();

        foreach (CollectionModel collection in collections)
        {
            lines.Add(
                collection.Id + ";" +
                Sanitize(collection.Name) + ";" +
                Sanitize(collection.Type) + ";" +
                Sanitize(collection.Description));

            await SaveItemsAsync(collection);
        }

        await File.WriteAllLinesAsync(_collectionsPath, lines);
    }

    public Task DeleteCollectionFilesAsync(int collectionId)
    {
        string itemsPath = GetItemsPath(collectionId);

        if (File.Exists(itemsPath))
            File.Delete(itemsPath);

        return Task.CompletedTask;
    }

    private async Task LoadItemsAsync(CollectionModel collection)
    {
        string itemsPath = GetItemsPath(collection.Id);

        if (!File.Exists(itemsPath))
        {
            File.WriteAllText(itemsPath, string.Empty);
            return;
        }

        string[] lines = await File.ReadAllLinesAsync(itemsPath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(';');

            if (parts.Length < 6)
                continue;

            CollectionItemModel item = new CollectionItemModel();
            item.Id = ParseInt(parts[0]);
            item.Name = Restore(parts[1]);
            item.Price = ParseDecimal(parts[2]);
            item.Status = ParseStatus(parts[3]);
            item.Rating = ParseInt(parts[4]);
            item.Comment = Restore(parts[5]);

            collection.Items.Add(item);
        }
    }

    private async Task SaveItemsAsync(CollectionModel collection)
    {
        string itemsPath = GetItemsPath(collection.Id);
        List<string> lines = new List<string>();

        foreach (CollectionItemModel item in collection.Items)
        {
            lines.Add(
                item.Id + ";" +
                Sanitize(item.Name) + ";" +
                item.Price.ToString(CultureInfo.InvariantCulture) + ";" +
                item.Status + ";" +
                item.Rating + ";" +
                Sanitize(item.Comment));
        }

        await File.WriteAllLinesAsync(itemsPath, lines);
    }

    private void EnsureFiles()
    {
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);

        string exportsPath = Path.Combine(_basePath, "exports");

        if (!Directory.Exists(exportsPath))
            Directory.CreateDirectory(exportsPath);

        if (!File.Exists(_collectionsPath))
            File.WriteAllText(_collectionsPath, string.Empty);
    }

    private string GetItemsPath(int collectionId)
    {
        return Path.Combine(_basePath, "items_" + collectionId + ".txt");
    }

    private int ParseInt(string value)
    {
        int result;

        if (int.TryParse(value, out result))
            return result;

        return 0;
    }

    private decimal ParseDecimal(string value)
    {
        decimal result;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            return result;

        return 0m;
    }

    private CollectionItemStatus ParseStatus(string value)
    {
        CollectionItemStatus status;

        if (Enum.TryParse(value, out status))
            return status;

        return CollectionItemStatus.Owned;
    }

    private string Sanitize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text
            .Replace(";", ",")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }

    private string Restore(string text)
    {
        if (text == null)
            return string.Empty;

        return text;
    }
}