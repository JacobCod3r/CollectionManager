using CollectionManager.ViewModels;

namespace CollectionManager.Views;

public partial class CollectionDetailsPage : ContentPage
{
    private readonly CollectionDetailsViewModel _vm;

    public CollectionDetailsPage()
    {
        InitializeComponent();
        _vm = new CollectionDetailsViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}