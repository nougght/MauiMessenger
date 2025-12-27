using MauiMessenger.ViewModels;
using Syncfusion.Maui.ImageEditor;

namespace MauiMessenger.Views;

public partial class EditImagePage : ContentPage
{

  private TaskCompletionSource<Stream?> _tcs;

  public ImageSource Image { get; set; }

  public Command BackButtonClickedCommand { get; }

  public EditImagePage(ImageSource image)
	{
		InitializeComponent();

    BackButtonClickedCommand = new Command(async () => await Shell.Current.Navigation.PopModalAsync());

    Image = image;

		BindingContext = this;

  }
  public Task<Stream?> ShowAsync()
  {
    _tcs = new TaskCompletionSource<Stream?>();
    return _tcs.Task;
  }

  private async void OnSaveClicked(object sender, EventArgs e)
  {
    var stream = await ImageEditor.GetImageStream();


    //await viewModel.SaveResult(stream); 
    _tcs.SetResult(stream);
    await Shell.Current.Navigation.PopModalAsync();
  }
}