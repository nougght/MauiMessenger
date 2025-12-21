using MauiMessenger.Models;
using MauiMessenger.Services;
using MauiMessenger.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using MauiMessenger.Models;

namespace MauiMessenger.Views;


public partial class VoiceMessage : ContentView
{
  public static readonly BindableProperty AudioUrlProperty =
      BindableProperty.Create(
          nameof(AudioUrl),
          typeof(string),
          typeof(VoiceMessage),
          default(string),
          propertyChanged: OnAudioUrlChanged
      );

  
  public string AudioUrl
  {
    get => (string)GetValue(AudioUrlProperty);
    set => SetValue(AudioUrlProperty, value);
  }


  public VoiceMessage()
  {
    InitializeComponent();

  }

  private static void OnAudioUrlChanged(BindableObject bindable, object oldValue, object newValue)
  {
    var view = (VoiceMessage)bindable;
    var url = newValue as string;

    // Здесь можно инициализировать AudioPlayer
    if (!string.IsNullOrEmpty(url))
    {
      view.PrepareAudio(url);
    }
  }

  private void PrepareAudio(string url)
  {
    // Пример: создать плеер
    // _player = AudioManager.Current.CreatePlayer(url);
  }
}
