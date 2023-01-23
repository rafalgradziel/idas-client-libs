﻿using Gandalan.IDAS.WebApi.Client.DTOs.Rechnung;
using Gandalan.IDAS.WebApi.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gandalan.IDAS.WebApi.Client.Contracts.DataServices
{
    public interface IRechnungenService
    {
        Task ErstelleRechnungenAsync(List<BelegartWechselDTO> belegeWechsel);
        Task<List<FakturierbarerBelegeDTO>> GetABFakturierbarListeAsync();
    }
}