using Microsoft.WindowsAzure.Mobile.Service;

namespace todolist_cordova_dotnetService.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}