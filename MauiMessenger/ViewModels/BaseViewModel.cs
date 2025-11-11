using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.ViewModels
{
  public partial class BaseViewModel : ObservableObject
  {
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool hasError;
    [ObservableProperty] private string errorMessage;
  }
}
