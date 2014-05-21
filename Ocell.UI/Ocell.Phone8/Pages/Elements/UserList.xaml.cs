using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;

namespace Ocell.Pages.Elements
{
    [ViewModel(typeof(UserListModel))]
    public partial class UserList : PhoneApplicationPage
    {
        public UserList()
        {
            InitializeComponent();
        }
    }
}
