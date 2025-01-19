using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZealandLokaleBooking.Pages
{
    [AllowAnonymous] // Gør siden tilgængelig uden login
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
