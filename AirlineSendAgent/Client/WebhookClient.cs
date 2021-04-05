using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using AirlineSendAgent.Dtos;

namespace AirlineSendAgent.Client
{
  public class WebhookClient : IWebhookClient
  {
    private readonly IHttpClientFactory _httpClientFactory;
    public WebhookClient(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    public async Task SendWebhookNotification(FlightDetailChangePayloadDto flightDetailChangePayloadDto)
    {
      var serializedPayload  = JsonSerializer.Serialize(flightDetailChangePayloadDto);
      
      var httpClient = _httpClientFactory.CreateClient();
      var request = new HttpRequestMessage(HttpMethod.Post, flightDetailChangePayloadDto.WebhookUri);
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      request.Content = new StringContent(serializedPayload);
      request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

      try 
      {
        using (var response = await httpClient.SendAsync(request))
        {
          Console.WriteLine("Send webhook successful.");

          response.EnsureSuccessStatusCode();
        }
      } 
      catch (HttpRequestException ex) 
      {
        Console.WriteLine("Send webhook failed: {0}: {1}: {2}", ex.Message, ex.InnerException?.Message, ex.StackTrace);
      }
    }
  }
}