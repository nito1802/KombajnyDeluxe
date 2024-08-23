using System.Collections.ObjectModel;

namespace Apps_Dashboard.Models
{
    public class AppsGroupModel
    {
        public string GroupName { get; set; }
        public ObservableCollection<SingleAppModel> Apps { get; set; } = [];
    }
}