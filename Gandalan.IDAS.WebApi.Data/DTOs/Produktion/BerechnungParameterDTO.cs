﻿using Gandalan.IDAS.WebApi.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gandalan.IDAS.WebApi.Data.DTOs.Produktion
{
    public class BerechnungParameterDTO
    {
        public BelegPositionAVDTO BelegPositionAVDTO { get; set; }
        public long VorgangsNummer { get; set; } = -999;
        public long BelegNummer { get; set; } = -999;
    }
}
