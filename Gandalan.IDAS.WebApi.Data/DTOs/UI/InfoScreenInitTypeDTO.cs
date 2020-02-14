﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Gandalan.IDAS.WebApi.DTO
{
    /// <summary>
    /// DTO für die Dateninitialisierung der InfoScreens. Es wird festgelegt, mit welchen Daten der InfoScreen initialisiert wird und welche Datentyp als Basis für den Screen genutzt wird.
    /// </summary>
    public class InfoScreenInitTypeDTO
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public object Data { get; set; }

    }

}