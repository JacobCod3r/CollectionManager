using System.ComponentModel;
using System.Runtime.CompilerServices;
using CollectionManager.Enums;

namespace CollectionManager.Models;

public class CollectionItemModel : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private decimal _price;
    private CollectionItemStatus _status = CollectionItemStatus.Owned;
    private int _rating = 1;
    private string _comment = string.Empty;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            OnPropertyChanged();
        }
    }

    public CollectionItemStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSold));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public int Rating
    {
        get => _rating;
        set
        {
            _rating = value;
            OnPropertyChanged();
        }
    }

    public string Comment
    {
        get => _comment;
        set
        {
            _comment = value;
            OnPropertyChanged();
        }
    }

    public bool IsSold
    {
        get
        {
            return Status == CollectionItemStatus.Sold;
        }
    }

    public string StatusText
    {
        get
        {
            switch (Status)
            {
                case CollectionItemStatus.Owned:
                    return "Posiadam";

                case CollectionItemStatus.ForSale:
                    return "Na sprzedaż";

                case CollectionItemStatus.Sold:
                    return "Sprzedane";

                case CollectionItemStatus.Wanted:
                    return "Chcę kupić";

                default:
                    return "Posiadam";
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}