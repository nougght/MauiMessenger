using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Services;

namespace MauiMessenger.Views
{

  public class BoolToOnlineTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool isOnline)
        return isOnline ? "Online" : "Offline";

      return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // Обратное преобразование обычно не требуется для метки
      throw new NotImplementedException();
    }
  }
  //public class EnumConverter : IValueConverter
  //{
  //  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  //  {
  //    switch (value)
  //    {
  //      case MemberRole._2:
  //        return "Owner";
  //      case MemberRole._1:
  //        return "Admin";
  //      case MemberRole._0:
  //        return "Member";
  //      default:
  //        return "______";
  //    }
  //  }


  //  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo cultrue)
  //  {
  //    return value!;
  //  }
  //}
}
