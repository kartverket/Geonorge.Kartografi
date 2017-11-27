using Geonorge.Kartografi.Models.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Resources
{
    public class Resource
    {
        public static string Name(string culture)
        {
            return UI.Name + " " + GetCultureName(culture);
        }

        public static string Description(string culture)
        {
            return UI.Description + " " + GetCultureName(culture);
        }

        public static string Use(string culture)
        {
            return UI.Use + " " + GetCultureName(culture);
        }

        public static string Properties(string culture)
        {
            return UI.Properties + " " + GetCultureName(culture);
        }

        public static string GetCultureName(string culture)
        {
            return Culture.Languages[culture].ToLower();
        }
    }
}