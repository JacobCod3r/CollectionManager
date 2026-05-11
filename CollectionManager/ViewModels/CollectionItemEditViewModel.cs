using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CollectionManager.Enums;
using CollectionManager.Models;

namespace CollectionManager.ViewModels;

public class CollectionItemEditViewModel : BaseViewModel
{
    private string _name = string.Empty;
    private string _priceText = "0";
    private CollectionItemStatus _selectedStatus = CollectionItemStatus.Owned;
    private int _rating = 1;
    private string _comment = string.Empty;
    private bool _isEditMode;

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public string PriceText
    {
        get
        {
            return _priceText;
        }
        set
        {
            _priceText = value;
            OnPropertyChanged();
        }
    }

    public CollectionItemStatus SelectedStatus
    {
        get
        {
            return _selectedStatus;
        }
        set
        {
            _selectedStatus = value;
            OnPropertyChanged();
        }
    }

    public int Rating
    {
        get
        {
            return _rating;
        }
        set
        {
            _rating = value;
            OnPropertyChanged();
        }
    }

    public string Comment
    {
        get
        {
            return _comment;
        }
        set
        {
            _comment = value;
            OnPropertyChanged();
        }
    }

    public bool IsEditMode
    {
        get
        {
            return _isEditMode;
        }
        set
        {
            _isEditMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageTitle));
        }
    }

    public string PageTitle
    {
        get
        {
            if (IsEditMode)
                return "Edycja elementu";

            return "Nowy element";
        }
    }

    public ObservableCollection<CollectionItemStatus> Statuses { get; set; }
        = new ObservableCollection<CollectionItemStatus>
        {
            CollectionItemStatus.Owned,
            CollectionItemStatus.ForSale,
            CollectionItemStatus.Sold,
            CollectionItemStatus.Wanted
        };

    public ICommand SaveCommand { get; }

    public CollectionItemEditViewModel()
    {
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public void LoadForAdd()
    {
        IsEditMode = false;
        Name = string.Empty;
        PriceText = "0";
        SelectedStatus = CollectionItemStatus.Owned;
        Rating = 1;
        Comment = string.Empty;
    }

    public void LoadForEdit(CollectionItemModel item)
    {
        IsEditMode = true;
        Name = item.Name;
        PriceText = item.Price.ToString(CultureInfo.InvariantCulture);
        SelectedStatus = item.Status;
        Rating = item.Rating;
        Comment = item.Comment;
    }

    private CollectionItemModel BuildItem(int existingId)
    {
        decimal price = 0m;

        decimal.TryParse(
            PriceText.Replace(',', '.'),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out price);

        CollectionItemModel item = new CollectionItemModel();
        item.Id = existingId;
        item.Name = Name.Trim();
        item.Price = price;
        item.Status = SelectedStatus;
        item.Rating = Rating;
        item.Comment = Comment.Trim();

        return item;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Błąd", "Nazwa elementu nie może być pusta.", "OK");
            return;
        }

        int currentItemId;

        if (App.State.SelectedItem == null)
            currentItemId = 0;
        else
            currentItemId = App.State.SelectedItem.Id;

        CollectionDetailsViewModel detailsViewModel = new CollectionDetailsViewModel();
        await detailsViewModel.LoadAsync();

        bool duplicate = detailsViewModel.ExistsDuplicate(Name, currentItemId);

        if (duplicate)
        {
            bool addAnyway = await Shell.Current.DisplayAlert(
                "Duplikat",
                "Taki element już istnieje w tej kolekcji. Czy chcesz dodać lub zapisać mimo to?",
                "Tak",
                "Nie");

            if (!addAnyway)
                return;
        }

        if (Rating < 1)
            Rating = 1;

        if (Rating > 10)
            Rating = 10;

        if (App.State.SelectedItem == null)
        {
            CollectionItemModel newItem = BuildItem(0);
            await detailsViewModel.AddItemToCollectionAsync(newItem);
        }
        else
        {
            CollectionItemModel updatedItem = BuildItem(App.State.SelectedItem.Id);
            await detailsViewModel.UpdateItemInCollectionAsync(App.State.SelectedItem, updatedItem);
        }

        await Shell.Current.Navigation.PopAsync();
    }
}