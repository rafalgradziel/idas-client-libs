using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.WebApi.Client.DTOs.Produktion;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Gandalan.IDAS.WebApi.Client.BusinessRoutinen
{
    public class ProduktionsInfoWebRoutinen : WebRoutinenBase
    {
        public ProduktionsInfoWebRoutinen(IWebApiConfig settings) : base(settings)
        {
        }

        public async Task<ProduktionsInfoDTO> GetProduktionsInfoAsync(Guid vorgangGuid)
        {
            try
            {
                return await GetAsync<ProduktionsInfoDTO>("ProduktionsInfo?vorgangGuid=" + vorgangGuid);
            }
            catch (WebException wex)
            {
                if (wex.Response is HttpWebResponse response)
                {
                    var code = response.StatusCode;
                    if (code == HttpStatusCode.NotFound)
                        return null;
                }

                throw;
            }
        }
    }
}
