using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SP_WorkTimeApp_Website.Data;
using SP_WorkTimeApp_Website.Models;
using System.Security.Claims;
using System.Security.Policy;

namespace SP_WorkTimeApp_Website.Hubs
{
    public class NotificationHub : Hub
    {
        private ApplicationDbContext myDbContext;
        private readonly UserManager<Administrator> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationHub(ApplicationDbContext context,
            UserManager<Administrator> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            myDbContext = context;
        }

        public async Task CheckUrlopy(string user, string id)
        {
            using(var context = myDbContext)
            {
                var userInfo = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                var idFirmy = userInfo?.IdFirmy;
                if (context.Users.Any(x => x.UserName == user && x.Id == id && x.IdFirmy == idFirmy ))
                {
                    if(context.Wnioski.Any(x => x.CzyZatwierdzono == false && x.IdFirmy == idFirmy))
                        await Clients.User(id).SendAsync("NewUrlopy");
                }
                else
                {
                    await Clients.User(id).SendAsync("Error", "User not found");
                }
            }
            
        }
    }
}
