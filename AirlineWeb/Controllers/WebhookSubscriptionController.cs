using System;
using System.Threading.Tasks;
using AirlineWeb.Data;
using AirlineWeb.Dtos;
using AirlineWeb.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineWeb.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class WebhookSubscriptionController : ControllerBase
  {
    private readonly AirlineDbContext _context;
    private readonly IMapper _mapper;
    public WebhookSubscriptionController(AirlineDbContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    [HttpGet("{secret}", Name = "GetSubscriptionBySecret")]
    public async Task<ActionResult<WebhookSubscriptionReadDto>> GetSubscriptionBySecret(string secret)
    {
      var subscription = await _context.WebhookSubscriptions.FirstOrDefaultAsync(e => e.Secret == secret);

      if (subscription == null) return NotFound();

      return Ok(_mapper.Map<WebhookSubscriptionReadDto>(subscription));
    }

    [HttpPost]
    public async Task<ActionResult<WebhookSubscriptionReadDto>> CreateSubscription(WebhookSubscriptionCreateDto webSubscriptionCreateDto)
    {
      var subscription = await _context.WebhookSubscriptions
                                       .FirstOrDefaultAsync(
                                           e => e.WebhookURI == webSubscriptionCreateDto.WebhookURI);
      if (subscription != null)
      {
        return NoContent();
      }

      subscription = _mapper.Map<WebhookSubscription>(webSubscriptionCreateDto);
      subscription.Secret = Guid.NewGuid().ToString();
      subscription.WebhookPublisher = "PanAm";

      try
      {
        await _context.WebhookSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }

      return CreatedAtRoute(nameof(GetSubscriptionBySecret),
      new { secret = subscription.Secret },
      _mapper.Map<WebhookSubscriptionReadDto>(subscription));
    }
  }
}