﻿using eShop.ClientApp.Helpers;
using eShop.ClientApp.Models.Marketing;
using eShop.ClientApp.Services.FixUri;
using eShop.ClientApp.Services.RequestProvider;
using System.Collections.ObjectModel;

namespace eShop.ClientApp.Services.Marketing;

public class CampaignService : ICampaignService
{
    private readonly IRequestProvider _requestProvider;
    private readonly IFixUriService _fixUriService;

    private const string ApiUrlBase = "m/api/v1/campaigns";

    public CampaignService(IRequestProvider requestProvider, IFixUriService fixUriService)
    {
        _requestProvider = requestProvider;
        _fixUriService = fixUriService;
    }

    public async Task<IEnumerable<CampaignItem>> GetAllCampaignsAsync(string token)
    {
        var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayMarketingEndpoint, $"{ApiUrlBase}/user");

        CampaignRoot campaign = await _requestProvider.GetAsync<CampaignRoot>(uri, token).ConfigureAwait(false);

        if (campaign?.Data != null)
        {
            _fixUriService.FixCampaignItemPictureUri(campaign?.Data);
            return campaign?.Data ?? Enumerable.Empty<CampaignItem>();
        }

        return new ObservableCollection<CampaignItem>();
    }

    public async Task<CampaignItem> GetCampaignByIdAsync(int campaignId, string token)
    {
        var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayMarketingEndpoint, $"{ApiUrlBase}/{campaignId}");

        return await _requestProvider.GetAsync<CampaignItem>(uri, token).ConfigureAwait(false);
    }
}
