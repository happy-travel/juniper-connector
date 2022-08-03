﻿namespace HappyTravel.JuniperConnector.Common;

public static class Constants
{
    public static string HttpStaticDataClientNames = "JuniperStaticDataServiceClient";
    public static string HttpAvailClientName = "JuniperAvailServiceClient";
    public const string DefaultLanguageCode = "en";
    public static string Version = "1.1";
    public static string LanguageFieldName = "Language";
    public static string VersionFieldName = "Version";
    public static readonly Dictionary<string, string> VisibleCultures = new Dictionary<string, string>()
    {
       { "AD", "Andorra" },
       { "AE", "United Arab Emirates" },
       { "AF", "Afghanistan" },
       { "AG", "Antigua And Barbuda" },
       { "AI", "Anguilla" },
       { "AL", "Albania" },
       { "AM", "Armenia" },
       { "AO", "Angola" },
       { "AR", "Argentina" },
       { "AS", "American Samoa" },
       { "AT", "Austria" },
       { "AU", "Australia" },
       { "AW", "Aruba" },
       { "AZ", "Azerbaijan" },
       { "BA", "Bosnia and Herzegovina" },
       { "BB", "Barbados" },
       { "BD", "Bangladesh" },
       { "BE", "Belgium" },
       { "BF", "Burkina Faso" },
       { "BG", "Bulgaria" },
       { "BH", "Bahrain" },
       { "BI", "Burundi" },
       { "BJ", "Benin" },
       { "BL", "Saint Barthélemy" },
       { "BM", "Bermuda" },
       { "BN", "Brunei Darussalam" },
       { "BO", "Bolivia" },
       { "BR", "Brazil" },
       { "BS", "Bahamas" },
       { "BT", "Bhutan" },
       { "BW", "Botswana" },
       { "BY", "Belarus" },
       { "BZ", "Belize" },
       { "CA", "Canada" },
       { "CC", "Coco Islands" },
       { "CD", "Democratic Republic of the Congo" },
       { "CF", "Central African Republic" },
       { "CG", "Republic of the Congo" },
       { "CH", "Switzerland" },
       { "CK", "Cook Islands" },
       { "CL", "Chile" },
       { "CM", "Cameroon" },
       { "CN", "China" },
       { "CO", "Colombia" },
       { "CR", "Costa Rica" },
       { "CU", "Cuba" },
       { "CUR", "Dutch Antilles" },
       { "CV", "Cape Verde" },
       { "CW", "Curacao" },
       { "CX", "Christmas Island" },
       { "CY", "Cyprus" },
       { "CZ", "Czech Republic" },
       { "DE", "Germany" },
       { "DJ", "Djibouti" },
       { "DK", "Denmark" },
       { "DM", "Dominica" },
       { "DO", "Dominican Republic" },
       { "DZ", "Algeria" },
       { "EC", "Ecuador" },
       { "EE", "Estonia" },
       { "EG", "Egypt" },
       { "ER", "Eritrea" },
       { "ES", "Spain" },
       { "ET", "Ethiopia" },
       { "FI", "Finland" },
       { "FJ", "Fiji Islands" },
       { "FK", "Falkland Islands" },
       { "FM", "Micronesia" },
       { "FO", "Faroe Islands" },
       { "FR", "France" },
       { "GA", "Gabon" },
       { "GB", "United Kingdom" },
       { "GD", "Grenada" },
       { "GE", "Georgia" },
       { "GF", "French Guayana" },
       { "GH", "Ghana" },
       { "GI", "Gibraltar" },
       { "GL", "Greenland" },
       { "GM", "Gambia" },
       { "GN", "Guinea" },
       { "GP", "Guadeloupe" },
       { "GR", "Greece" },
       { "GT", "Guatemala" },
       { "GU", "Guam" },
       { "GW", "Guinea Bissau" },
       { "GY", "Guyana" },
       { "HN", "Honduras" },
       { "HR", "Croatia" },
       { "HT", "Haiti" },
       { "HU", "Hungary" },
       { "ID", "Indonesia" },
       { "IE", "Ireland" },
       { "IL", "Israel" },
       { "IM", "Isle of Man" },
       { "IN", "India" },
       { "IQ", "Iraq" },
       { "IR", "Iran" },
       { "IS", "Iceland" },
       { "IT", "Italy" },
       { "JM", "Jamaica" },
       { "JO", "Jordan" },
       { "JP", "Japan" },
       { "KE", "Kenya" },
       { "KG", "Kyrgyzstan" },
       { "KH", "Cambodia" },
       { "KI", "Kiribati" },
       { "KM", "Comoros" },
       { "KN", "Saint Kitts and Nevis" },
       { "KR", "South Korea" },
       { "KW", "Kuwait" },
       { "KY", "Cayman Islands" },
       { "KZ", "Kazakhstan" },
       { "LA", "Laos" },
       { "LB", "Lebanon" },
       { "LC", "Saint Lucia" },
       { "LI", "Liechtenstein" },
       { "LK", "Sri Lanka" },
       { "LR", "Liberia" },
       { "LS", "Lesotho" },
       { "LT", "Lithuania" },
       { "LU", "Luxembourg" },
       { "LV", "Latvia" },
       { "LY", "Libya" },
       { "MA", "Morocco" },
       { "MC", "Monaco" },
       { "MD", "Moldova" },
       { "ME", "Montenegro" },
       { "MF", "Saint Martin Island (French)" },
       { "MG", "Madagascar" },
       { "MH", "Marshall Islands" },
       { "MK", "Macedonia " },
       { "ML", "Mali" },
       { "MM", "Myanmar " },
       { "MN", "Mongolia" },
       { "MP", "Northern Mariana Islands" },
       { "MQ", "Martinique" },
       { "MR", "Mauritania" },
       { "MT", "Malta" },
       { "MU", "Mauritius" },
       { "MV", "Maldives" },
       { "MW", "Malawi" },
       { "MX", "Mexico" },
       { "MY", "Malaysia" },
       { "MZ", "Mozambique" },
       { "NA", "Namibia" },
       { "NC", "New Caledonia" },
       { "NE", "Niger" },
       { "NG", "Nigeria" },
       { "NI", "Nicaragua" },
       { "NL", "Netherlands" },
       { "NO", "Norway" },
       { "NP", "Nepal" },
       { "NR", "Nauru" },
       { "NU", "Niue" },
       { "NZ", "New Zealand" },
       { "OM", "Oman" },
       { "PA", "Republic of Panama" },
       { "PE", "Peru" },
       { "PF", "French Polynesia" },
       { "PG", "Papua New Guinea" },
       { "PH", "Philippines" },
       { "PK", "Pakistan" },
       { "PL", "Poland" },
       { "PM", "Saint Pierre and Miquelon" },
       { "PR", "Puerto Rico" },
       { "PS", "Palestinian Territories" },
       { "PT", "Portugal" },
       { "PW", "Palau" },
       { "PY", "Paraguay" },
       { "QA", "Qatar" },
       { "RO", "Romania" },
       { "RS", "Serbia" },
       { "RU", "Russia" },
       { "RW", "Rwanda" },
       { "SA", "Saudi Arabia" },
       { "SB", "Solomon Islands" },
       { "SC", "Seychelles" },
       { "SD", "Sudan" },
       { "SE", "Sweden" },
       { "SG", "Singapore" },
       { "SH", "Saint Helena" },
       { "SI", "Slovenia" },
       { "SK", "Slovakia" },
       { "SL", "Sierra Leone" },
       { "SM", "San Marino" },
       { "SN", "Senegal" },
       { "SO", "Somalia" },
       { "SR", "Suriname" },
       { "SS", "South Sudan" },
       { "ST", "Sao Tomé and Príncipe" },
       { "SV", "El Salvador" },
       { "SY", "Syria" },
       { "TC", "Turks And Caicos Island" },
       { "TD", "Chad" },
       { "TG", "Togo" },
       { "TH", "Thailand" },
       { "TJ", "Tajikistan" },
       { "TL", "East Timor" },
       { "TM", "Turkmenistan" },
       { "TN", "Tunisia" },
       { "TO", "Tonga" },
       { "TR", "Turkey" },
       { "TT", "Trinidad And Tobago" },
       { "TW", "Taiwan" },
       { "TZ", "Tanzania" },
       { "UA", "Ukraine" },
       { "UG", "Uganda" },
       { "US", "USA" },
       { "UY", "Uruguay" },
       { "UZ", "Uzbekistan" },
       { "VC", "Saint Vincent and the Grenadines" },
       { "VE", "Venezuela" },
       { "VG", "Virgin Islands (U.K.)" },
       { "VI", "Virgin Islands (USA)" },
       { "VN", "Vietnam" },
       { "VU", "Vanuatu" },
       { "WF", "Wallis and Futuna" },
       { "WS", "Western Samoa" },
       { "XK", "Kosovo" },
       { "YE", "Yemen" },
       { "YT", "Mayotte" },
       { "ZA", "South Africa" },
       { "ZM", "Zambia" },
       { "ZW", "Zimbabwe" }
    };
}
