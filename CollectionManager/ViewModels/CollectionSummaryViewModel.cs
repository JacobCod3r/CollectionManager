using CollectionManager.Enums;
using CollectionManager.Models;

namespace CollectionManager.ViewModels;

public class CollectionSummaryViewModel : BaseViewModel
{
    private int _totalItems;
    private int _soldItems;
    private int _forSaleItems;
    private int _wantedItems;
    private decimal _totalValue;
    private double _averageRating;

    public int TotalItems
    {
        get
        {
            return _totalItems;
        }
        set
        {
            _totalItems = value;
            OnPropertyChanged();
        }
    }

    public int SoldItems
    {
        get
        {
            return _soldItems;
        }
        set
        {
            _soldItems = value;
            OnPropertyChanged();
        }
    }

    public int ForSaleItems
    {
        get
        {
            return _forSaleItems;
        }
        set
        {
            _forSaleItems = value;
            OnPropertyChanged();
        }
    }

    public int WantedItems
    {
        get
        {
            return _wantedItems;
        }
        set
        {
            _wantedItems = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalValue
    {
        get
        {
            return _totalValue;
        }
        set
        {
            _totalValue = value;
            OnPropertyChanged();
        }
    }

    public double AverageRating
    {
        get
        {
            return _averageRating;
        }
        set
        {
            _averageRating = value;
            OnPropertyChanged();
        }
    }

    public Task LoadAsync()
    {
        if (App.State.SelectedCollection == null)
            return Task.CompletedTask;

        CollectionModel collection = App.State.SelectedCollection;

        int totalItems = 0;
        int soldItems = 0;
        int forSaleItems = 0;
        int wantedItems = 0;
        decimal totalValue = 0m;
        int ratingSum = 0;

        foreach (CollectionItemModel item in collection.Items)
        {
            totalItems++;
            totalValue += item.Price;
            ratingSum += item.Rating;

            if (item.Status == CollectionItemStatus.Sold)
                soldItems++;

            if (item.Status == CollectionItemStatus.ForSale)
                forSaleItems++;

            if (item.Status == CollectionItemStatus.Wanted)
                wantedItems++;
        }

        TotalItems = totalItems;
        SoldItems = soldItems;
        ForSaleItems = forSaleItems;
        WantedItems = wantedItems;
        TotalValue = totalValue;

        if (totalItems == 0)
            AverageRating = 0;
        else
            AverageRating = Math.Round((double)ratingSum / totalItems, 2);

        return Task.CompletedTask;
    }
}