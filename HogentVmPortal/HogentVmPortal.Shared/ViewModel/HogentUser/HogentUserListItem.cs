using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Shared.ViewModel
{
    public class HogentUserListItem : ViewModelBase<HogentUserListItem, HogentUser>
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public static HogentUserListItem ToViewModel(HogentUser user)
        {
            return new HogentUserListItem
            {
                Id = user.Id,
                Name = user.UserName!,
            };
        }

        public static List<HogentUserListItem> ToViewModel(List<HogentUser> users)
        {
            return users.Select(ToViewModel).ToList();
        }
    }
}
