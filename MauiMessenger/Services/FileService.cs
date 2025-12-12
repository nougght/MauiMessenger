using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MauiMessenger.Services
{
  public class FileService
  {

    public static async Task<List<FileResult>> PickFileAsync()
    {
      try
      {
        var res = await FilePicker.Default.PickMultipleAsync(
          new PickOptions
          {
            PickerTitle = "Выберите файл(ы)",
            
          });
        return res.ToList();
      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
}
