using CollectionManager.ViewModels;

namespace CollectionManager.Views;

public partial class CollectionItemEditPage : ContentPage
{
    private readonly CollectionItemEditViewModel _vm;

    public CollectionItemEditPage()
    {
        InitializeComponent();
        _vm = new CollectionItemEditViewModel();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (App.State.SelectedItem == null)
            _vm.LoadForAdd();
        else
            _vm.LoadForEdit(App.State.SelectedItem);
    }
}