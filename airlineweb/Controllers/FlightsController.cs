using System;
using System.Threading.Tasks;
using AirlineWeb.Data;
using AirlineWeb.Dtos;
using AirlineWeb.MessageBus;
using AirlineWeb.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineWeb.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FlightsController : ControllerBase
  {
    private readonly AirlineDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMessageBusClient _messageBus;

    public FlightsController(AirlineDbContext context, IMapper mapper, IMessageBusClient messageBus)
    {
      _mapper = mapper;
      _messageBus = messageBus;
      _context = context;
    }

    [HttpGet("{flightCode}", Name = "GetFlightDetailsByCode")]
    public async Task<ActionResult<FlightDetailReadDto>> GetFlightDetailsByCode(string flightCode)
    {
      var flight = await _context.FlightDetails.FirstOrDefaultAsync(e => e.FlightCode == flightCode);

      if (flight == null) return NotFound();

      return Ok(_mapper.Map<FlightDetailReadDto>(flight));
    }

    [HttpPost]
    public async Task<ActionResult<FlightDetailReadDto>> CreateFlightDetail(FlightDetailCreateDto flightDetailCreateDto)
    {
      var flight = await _context.FlightDetails
                                       .FirstOrDefaultAsync(
                                           e => e.FlightCode == flightDetailCreateDto.FlightCode);
      if (flight != null)
      {
        return NoContent();
      }

      flight = _mapper.Map<FlightDetail>(flightDetailCreateDto);

      try
      {
        await _context.FlightDetails.AddAsync(flight);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }

      return CreatedAtRoute(nameof(GetFlightDetailsByCode),
      new { flightCode = flight.FlightCode },
      _mapper.Map<FlightDetailReadDto>(flight));
    }

    [HttpPut("{flightCode}")]
    public async Task<ActionResult> UpdateFlightDetails(string flightCode, [FromBody] FlightDetailUpdateDto flightDetailsUpdateDto)
    {
      var flight = await _context.FlightDetails
                                     .FirstOrDefaultAsync(
                                         e => e.FlightCode == flightCode);
      if (flight == null)
      {
        return NoContent();
      }

      decimal oldPrice = flight.Price;

      _mapper.Map(flightDetailsUpdateDto, flight);

      try
      {
        await _context.SaveChangesAsync();
        if (oldPrice != flight.Price) 
        {
           Console.WriteLine("Price changed - PLace message on bus");
           var message = new Dtos.NotificationMessageDto {
             Id = flight.Id.ToString(),
             FlightCode = flight.FlightCode,
             NewPrice = flight.Price,
             OldPrice = oldPrice,
             WebhookType = "pricechange"
           }; 

           _messageBus.SendMessage(message);
        }
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }

      return NoContent();
    }
  }
}