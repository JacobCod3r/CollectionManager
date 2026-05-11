using CollectionManager.ViewModels;

namespace CollectionManager.Views;

public partial class CollectionsPage : ContentPage
{
    private readonly CollectionsViewModel _vm;

    public CollectionsPage()
    {
        InitializeComponent();
        _vm = new CollectionsViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}