using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.WebApi.Client.DTOs.Nachrichten;

namespace Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class TranslateWebRoutinen : WebRoutinenBase
{
    private readonly bool _validConfig;

    public TranslateWebRoutinen(IWebApiConfig settings) : base(settings)
    {
        if (string.IsNullOrEmpty(settings.TranslateUrl))
        {
            return;
        }

        var uri = new UriBuilder(settings.TranslateUrl)
        {
            Path = "/api/",
        };
        Settings.Url = uri.Uri.ToString(); // use Uri member!! to be identical to createWebClient later on - otherwise includes port etc
        _validConfig = true;
    }

    public async Task<String> Translate(string language, string text)
    {
        if (!_validConfig)
        {
            return null;
        }

        return await PostAsync<string>($"translate?lang={language}", text);

    }
}
