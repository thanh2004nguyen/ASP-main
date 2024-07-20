using Group5.Models;

namespace Group5.ViewModels
{
    public class IndexViewModel
    {
        public List<RoomViewModel>? Rooms { get; set; }
        public List<Message>? UnseenMessages { get; set; }
    }
}
