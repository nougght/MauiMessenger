using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.Models
{
  public interface IHasCreatedAt
  {
    DateTime CreatedAt { get; }
  }

  public enum ChatItemType { Message, DaySeparator, UnreadMarker, ServiceMessage}
  public abstract class ChatItem
  { 
    public ChatItemType Type { get; set; }
    public bool IsVisible { get; set; } = false;
  }

  public class MessageItem : ChatItem, IHasCreatedAt
  {
    public MessageDTO Message { get; set; }

    public DateTime CreatedAt { get; set; }
  }

  public class DaySeparatorItem : ChatItem
  {
    public DateTime Date { get; set; }

   
  }

  public class UnreadMarkerItem : ChatItem
  {
    public string Text { get; set; } = "Непрочитанные сообщения";
  }

  public class ServiceMessageItem : ChatItem, IHasCreatedAt
  {
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
  }

  public partial class MessageFileDTO
  {
    public string URL { get; set; }

    public bool IsImage { get => this.FileType.Split('/')[0] == "image"; }
    public bool IsVideo { get => this.FileType.Split('/')[0] == "video"; }
        public bool IsAudio { get => this.FileType.Split('/')[0] == "audio"; }

    }
}
