using CollectionManager.ViewModels;

namespace CollectionManager.Views;

public partial class CollectionSummaryPage : ContentPage
{
    private readonly CollectionSummaryViewModel _vm;

    public CollectionSummaryPage()
    {
        InitializeComponent();
        _vm = new CollectionSummaryViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}