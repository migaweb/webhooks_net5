using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgentWeb.Data;
using TravelAgentWeb.Dtos;

namespace TravelAgentWeb.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class NotificationsController : ControllerBase
  {
    private readonly TravelAgentDbContext _context;

    public NotificationsController(TravelAgentDbContext context)
    {
      _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> FlightChanged(FlightDetailUpdateDto flightDetailUpdateDto)
    {
      Console.WriteLine($"webhook received from: {flightDetailUpdateDto.Publisher}");

      var secretModel = await _context.SubscriptionSecrets.FirstOrDefaultAsync(
            e => e.Secret == flightDetailUpdateDto.Secret && e.Publisher == flightDetailUpdateDto.Publisher
          );

      if (secretModel == null)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid secret");
        Console.ResetColor();
        return Ok();
      }

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("Valid webhook");
      Console.WriteLine("Old price: {0}, New Price: {1}", flightDetailUpdateDto.OldPrice, flightDetailUpdateDto.NewPrice);
      Console.ResetColor();
      return Ok();
    }
  }
}