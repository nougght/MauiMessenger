using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Plugin.Maui.Audio;
using Microsoft.Maui.Controls;


namespace MauiMessenger.Services
{
  public class FileService
  {

    private static readonly IAudioManager _audioManager = new AudioManager();

    private static IAudioRecorder? _audioRecorder;

    private static string? _filePath;

    public FileService()
    {

    }

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

    public static async Task<bool> StartRecorder()
    {
      try
      {

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
        {

          var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
          if (status != PermissionStatus.Granted)
          {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
          }

          if (status != PermissionStatus.Granted)
          {
            // НЕЛЬЗЯ продолжать
            throw new Exception("Microphone permission denied");
          }

          _audioRecorder = _audioManager.CreateRecorder();
          _filePath = Path.Combine(FileSystem.CacheDirectory, $"voice_{Guid.NewGuid()}.m4a");

          await _audioRecorder.StartAsync(_filePath);
          return true;
        }
        else
        {
          return false;
        }


      }
      catch (Exception ex)
      {
        throw;
      }
    }


    public static async Task<string?> StopRecorder()
    {
      try
      {
        if (_audioRecorder == null || _filePath == null)
        {
          return null;
        }
        await _audioRecorder.StopAsync();
        return _filePath;
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    public static string? GetRecordedFilePath()
    {
      return _filePath;
    }
  }
}
