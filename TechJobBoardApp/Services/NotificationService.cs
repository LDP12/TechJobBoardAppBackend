using FreelanceJobBoard.Models;
using System.Threading.Tasks;
using FreelanceJobBoard.Data;

namespace FreelanceJobBoard.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _Context;

        public NotificationService(AppDbContext context)
        {
            _Context = context;
        }

        public async Task CreateNotification(int userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _Context.Notifications.Add(notification);
            await _Context.SaveChangesAsync();
        }
    }
}