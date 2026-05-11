using System.Globalization;
using CollectionManager.Enums;
using CollectionManager.Models;

namespace CollectionManager.Services;

public class ImportExportService
{
    public async Task<string> ExportCollectionAsync(CollectionModel collection)
    {
        try
        {
            string exportRoot = Path.Combine(FileSystem.AppDataDirectory, "exports");

            if (!Directory.Exists(exportRoot))
            {
                Directory.CreateDirectory(exportRoot);
            }

            string fileName = "export_" + collection.Name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            fileName = SafeFileName(fileName);

            string exportPath = Path.Combine(exportRoot, fileName);

            List<string> lines = new List<string>();

            lines.Add(
                "COLLECTION;" +
                collection.Id + ";" +
                Safe(collection.Name) + ";" +
                Safe(collection.Type) + ";" +
                Safe(collection.Description));

            foreach (CollectionItemModel item in collection.Items)
            {
                lines.Add(
                    "ITEM;" +
                    item.Id + ";" +
                    Safe(item.Name) + ";" +
                    item.Price.ToString(CultureInfo.InvariantCulture) + ";" +
                    item.Status + ";" +
                    item.Rating + ";" +
                    Safe(item.Comment));
            }

            await File.WriteAllLinesAsync(exportPath, lines);

            return exportPath;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<CollectionModel?> ImportCollectionAsync()
    {
        try
        {
            PickOptions options = new PickOptions();
            options.PickerTitle = "Wybierz plik eksportu";
            options.FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".txt" } },
                { DevicePlatform.Android, new[] { "text/plain" } },
                { DevicePlatform.iOS, new[] { "public.plain-text" } },
                { DevicePlatform.MacCatalyst, new[] { "public.plain-text" } }
            });

            FileResult? result = await FilePicker.PickAsync(options);

            if (result == null)
            {
                return null;
            }

            string[] lines = File.ReadAllLines(result.FullPath);
            CollectionModel collection = new CollectionModel();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(';');

                if (parts.Length == 0)
                {
                    continue;
                }

                if (parts[0] == "COLLECTION")
                {
                    if (parts.Length >= 5)
                    {
                        collection.Id = ParseInt(parts[1]);
                        collection.Name = parts[2];
                        collection.Type = parts[3];
                        collection.Description = parts[4];
                    }
                }
                else if (parts[0] == "ITEM")
                {
                    if (parts.Length >= 7)
                    {
                        CollectionItemModel item = new CollectionItemModel();
                        item.Id = ParseInt(parts[1]);
                        item.Name = parts[2];
                        item.Price = ParseDecimal(parts[3]);
                        item.Status = ParseStatus(parts[4]);
                        item.Rating = ParseInt(parts[5]);
                        item.Comment = parts[6];

                        collection.Items.Add(item);
                    }
                }
            }

            return collection;
        }
        catch
        {
            return null;
        }
    }

    public void MergeCollection(CollectionModel importedCollection)
    {
        CollectionModel? existingCollection = null;

        foreach (CollectionModel collection in App.State.Collections)
        {
            if (collection.Name.Trim().ToLower() == importedCollection.Name.Trim().ToLower())
            {
                existingCollection = collection;
                break;
            }
        }

        if (existingCollection == null)
        {
            int newCollectionId = 1;

            if (App.State.Collections.Count > 0)
            {
                foreach (CollectionModel collection in App.State.Collections)
                {
                    if (collection.Id >= newCollectionId)
                    {
                        newCollectionId = collection.Id + 1;
                    }
                }
            }

            importedCollection.Id = newCollectionId;
            App.State.Collections.Add(importedCollection);
            return;
        }

        foreach (CollectionItemModel importedItem in importedCollection.Items)
        {
            bool exists = false;

            foreach (CollectionItemModel existingItem in existingCollection.Items)
            {
                if (existingItem.Name.Trim().ToLower() == importedItem.Name.Trim().ToLower())
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                int newItemId = 1;

                if (existingCollection.Items.Count > 0)
                {
                    foreach (CollectionItemModel existingItem in existingCollection.Items)
                    {
                        if (existingItem.Id >= newItemId)
                        {
                            newItemId = existingItem.Id + 1;
                        }
                    }
                }

                importedItem.Id = newItemId;
                existingCollection.Items.Add(importedItem);
            }
        }

        existingCollection.OnPropertyChanged(nameof(existingCollection.ItemsCount));
    }

    private string Safe(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return text
            .Replace(";", ",")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }

    private string SafeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        return fileName;
    }

    private int ParseInt(string value)
    {
        int result;

        if (int.TryParse(value, out result))
        {
            return result;
        }

        return 0;
    }

    private decimal ParseDecimal(string value)
    {
        decimal result;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        return 0m;
    }

    private CollectionItemStatus ParseStatus(string value)
    {
        CollectionItemStatus status;

        if (Enum.TryParse(value, out status))
        {
            return status;
        }

        return CollectionItemStatus.Owned;
    }
}