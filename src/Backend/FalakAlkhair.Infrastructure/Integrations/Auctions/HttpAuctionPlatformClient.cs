using System.Net.Http.Json;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FalakAlkhair.Infrastructure.Integrations.Auctions;

/// <summary>
/// تنفيذ حقيقي (وليس Mock) لعقد التكامل مع منصة المزادات — يُجري نداءات HTTP
/// فعلية فقط عند توفر تكوين حقيقي (BaseUrl + ApiKey). دون ذلك يرمي
/// BusinessRuleException صريحًا: لا ادّعاء بتكامل غير موجود، تحقيقًا لمتطلب
/// المشروع "أنشئ Interface + Mock/Stub واضح. ولا تدّعِ أن التكامل الحقيقي موجود".
/// </summary>
public class HttpAuctionPlatformClient : IAuctionPlatformClient
{
    private readonly HttpClient _httpClient;
    private readonly AuctionPlatformSettings _settings;

    public HttpAuctionPlatformClient(HttpClient httpClient, IOptions<AuctionPlatformSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> PublishAuctionAsync(Auction auction, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var response = await _httpClient.PostAsJsonAsync(
            $"{_settings.BaseUrl!.TrimEnd('/')}/api/auctions",
            new
            {
                auctionNumber = auction.AuctionNumber,
                startingPrice = auction.StartingPrice,
                reservePrice = auction.ReservePrice,
                startDate = auction.StartDate,
                endDate = auction.EndDate
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PublishAuctionResponse>(cancellationToken: cancellationToken);
        return result?.ExternalAuctionId
            ?? throw new BusinessRuleException("لم تُعِد منصة المزادات معرّفًا صالحًا للمزاد المنشور.");
    }

    public async Task CloseAuctionAsync(string externalAuctionId, CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var response = await _httpClient.PostAsync(
            $"{_settings.BaseUrl!.TrimEnd('/')}/api/auctions/{externalAuctionId}/close",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private void EnsureConfigured()
    {
        if (!_settings.IsConfigured)
        {
            throw new BusinessRuleException(
                "لم يتم ربط منصة المزادات المستقلة بعد (AuctionIntegration:BaseUrl / ApiKey غير مُكوَّنة). " +
                "العملية الداخلية للمزاد تتم بنجاح رغم ذلك؛ المزامنة مع المنصة الخارجية معطَّلة حتى تفعيلها.");
        }
    }

    private class PublishAuctionResponse
    {
        public string? ExternalAuctionId { get; set; }
    }
}
