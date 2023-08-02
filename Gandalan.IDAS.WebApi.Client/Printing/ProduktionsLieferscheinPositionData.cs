﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Gandalan.IDAS.WebApi.Client.Printing
{
    public class ProduktionsLieferscheinPositionData
    {
        public string Positionsnummer { get; set; }
        public string Variante { get; set; }
        public string Artikel { get; set; }
        public decimal Menge { get; set; }
        public string Breite { get; set; }
        public string Hoehe { get; set; }
        public string Farbe { get; set; }
        public string Gewebe { get; set; }
        public string Text { get; set; }
        public string SonderwuenscheText { get; set; }
        public string PCode { get; set; }
        public string Ablagefach { get
            {
                return string.Join(", ", AblagefachList.Distinct().OrderBy(x => x));
            }
        }
        public List<string> AblagefachList { get; set; } = new List<string>();
        public string Besonderheiten { get; set; }
        public Guid BelegPositionGuid { get; set; }

    }
}