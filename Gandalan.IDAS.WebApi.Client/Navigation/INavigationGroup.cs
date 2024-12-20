using System.Collections.ObjectModel;

namespace Gandalan.Client.Contracts.Navigation;

public interface INavigationGroup
{
    object Icon { get; set; }
    string Caption { get; set; }
    ObservableCollection<INavigationSubGroup> SubGroups { get; set; }
    int Order { get; set; }
    bool IsVisible { get; set; }
}
