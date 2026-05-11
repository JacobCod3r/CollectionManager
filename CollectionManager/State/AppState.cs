using System.Collections.ObjectModel;
using CollectionManager.Models;
using CollectionManager.Services;

namespace CollectionManager.State;

public class AppState
{
    public ObservableCollection<CollectionModel> Collections { get; set; } =
        new ObservableCollection<CollectionModel>();

    public CollectionModel? SelectedCollection { get; set; }
    public CollectionItemModel? SelectedItem { get; set; }

    public DataService DataService { get; }
    public ImportExportService ImportExportService { get; }

    public AppState()
    {
        DataService = new DataService();
        ImportExportService = new ImportExportService();
    }
}