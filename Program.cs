using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParseWiki
{
    class Item
    {
        public string Name { get; set; }
        public string TermStart { get; set; }
        public string TermEnd { get; set; }
        public string Party { get; set; }

        public override string ToString() => $"{Name}\t{TermStart}\t{TermEnd}\t{Party}";
    }

    class Program
    {
        private const string US_STATES_JSON = @"
[
    {
        ""name"": ""Alabama"",
        ""abbreviation"": ""AL""
    },
    {
        ""name"": ""Alaska"",
        ""abbreviation"": ""AK""
    },
    {
        ""name"": ""American Samoa"",
        ""abbreviation"": ""AS""
    },
    {
        ""name"": ""Arizona"",
        ""abbreviation"": ""AZ""
    },
    {
        ""name"": ""Arkansas"",
        ""abbreviation"": ""AR""
    },
    {
        ""name"": ""California"",
        ""abbreviation"": ""CA""
    },
    {
        ""name"": ""Colorado"",
        ""abbreviation"": ""CO""
    },
    {
        ""name"": ""Connecticut"",
        ""abbreviation"": ""CT""
    },
    {
        ""name"": ""Delaware"",
        ""abbreviation"": ""DE""
    },
    {
        ""name"": ""District Of Columbia"",
        ""abbreviation"": ""DC""
    },
    {
        ""name"": ""Federated States Of Micronesia"",
        ""abbreviation"": ""FM""
    },
    {
        ""name"": ""Florida"",
        ""abbreviation"": ""FL""
    },
    {
        ""name"": ""Georgia"",
        ""abbreviation"": ""GA""
    },
    {
        ""name"": ""Guam"",
        ""abbreviation"": ""GU""
    },
    {
        ""name"": ""Hawaii"",
        ""abbreviation"": ""HI""
    },
    {
        ""name"": ""Idaho"",
        ""abbreviation"": ""ID""
    },
    {
        ""name"": ""Illinois"",
        ""abbreviation"": ""IL""
    },
    {
        ""name"": ""Indiana"",
        ""abbreviation"": ""IN""
    },
    {
        ""name"": ""Iowa"",
        ""abbreviation"": ""IA""
    },
    {
        ""name"": ""Kansas"",
        ""abbreviation"": ""KS""
    },
    {
        ""name"": ""Kentucky"",
        ""abbreviation"": ""KY""
    },
    {
        ""name"": ""Louisiana"",
        ""abbreviation"": ""LA""
    },
    {
        ""name"": ""Maine"",
        ""abbreviation"": ""ME""
    },
    {
        ""name"": ""Marshall Islands"",
        ""abbreviation"": ""MH""
    },
    {
        ""name"": ""Maryland"",
        ""abbreviation"": ""MD""
    },
    {
        ""name"": ""Massachusetts"",
        ""abbreviation"": ""MA""
    },
    {
        ""name"": ""Michigan"",
        ""abbreviation"": ""MI""
    },
    {
        ""name"": ""Minnesota"",
        ""abbreviation"": ""MN""
    },
    {
        ""name"": ""Mississippi"",
        ""abbreviation"": ""MS""
    },
    {
        ""name"": ""Missouri"",
        ""abbreviation"": ""MO""
    },
    {
        ""name"": ""Montana"",
        ""abbreviation"": ""MT""
    },
    {
        ""name"": ""Nebraska"",
        ""abbreviation"": ""NE""
    },
    {
        ""name"": ""Nevada"",
        ""abbreviation"": ""NV""
    },
    {
        ""name"": ""New Hampshire"",
        ""abbreviation"": ""NH""
    },
    {
        ""name"": ""New Jersey"",
        ""abbreviation"": ""NJ""
    },
    {
        ""name"": ""New Mexico"",
        ""abbreviation"": ""NM""
    },
    {
        ""name"": ""New York"",
        ""abbreviation"": ""NY""
    },
    {
        ""name"": ""North Carolina"",
        ""abbreviation"": ""NC""
    },
    {
        ""name"": ""North Dakota"",
        ""abbreviation"": ""ND""
    },
    {
        ""name"": ""Northern Mariana Islands"",
        ""abbreviation"": ""MP""
    },
    {
        ""name"": ""Ohio"",
        ""abbreviation"": ""OH""
    },
    {
        ""name"": ""Oklahoma"",
        ""abbreviation"": ""OK""
    },
    {
        ""name"": ""Oregon"",
        ""abbreviation"": ""OR""
    },
    {
        ""name"": ""Palau"",
        ""abbreviation"": ""PW""
    },
    {
        ""name"": ""Pennsylvania"",
        ""abbreviation"": ""PA""
    },
    {
        ""name"": ""Puerto Rico"",
        ""abbreviation"": ""PR""
    },
    {
        ""name"": ""Rhode Island"",
        ""abbreviation"": ""RI""
    },
    {
        ""name"": ""South Carolina"",
        ""abbreviation"": ""SC""
    },
    {
        ""name"": ""South Dakota"",
        ""abbreviation"": ""SD""
    },
    {
        ""name"": ""Tennessee"",
        ""abbreviation"": ""TN""
    },
    {
        ""name"": ""Texas"",
        ""abbreviation"": ""TX""
    },
    {
        ""name"": ""Utah"",
        ""abbreviation"": ""UT""
    },
    {
        ""name"": ""Vermont"",
        ""abbreviation"": ""VT""
    },
    {
        ""name"": ""Virgin Islands"",
        ""abbreviation"": ""VI""
    },
    {
        ""name"": ""Virginia"",
        ""abbreviation"": ""VA""
    },
    {
        ""name"": ""Washington"",
        ""abbreviation"": ""WA""
    },
    {
        ""name"": ""West Virginia"",
        ""abbreviation"": ""WV""
    },
    {
        ""name"": ""Wisconsin"",
        ""abbreviation"": ""WI""
    },
    {
        ""name"": ""Wyoming"",
        ""abbreviation"": ""WY""
    }
]";
        class StateItem
        {
            public string Name { get; set; }
            public string Abbreviation { get; set; }
            public override string ToString() => $"{Name} ({Abbreviation})";
        }

        static async Task<int> Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (var stateName in args)
                {
                    await PrintData(stateName);
                }
            }
            else
            {
                var allStates = JsonConvert.DeserializeObject<List<StateItem>>(US_STATES_JSON);
                foreach (var state in allStates)
                {
                    await PrintData(state.Name);
                }
            }
            return 0;
        }

        private static async Task PrintData(string stateName)
        {
            try
            {
                Console.WriteLine("-----------------");
                Console.WriteLine(stateName);
                Console.WriteLine();
                var items = await ParseWikiArticleAsync($"https://en.wikipedia.org/wiki/List_of_governors_of_{stateName.Replace(' ', '_')}");
                items.ForEach(Console.WriteLine);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        public static async Task<List<Item>> ParseWikiArticleAsync(string url)
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            });
            var htmlResponse = await client.GetAsync(url);
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(await htmlResponse.Content.ReadAsStreamAsync());
            var table = htmlDoc.DocumentNode.SelectSingleNode($"//table[//th[contains(text(),'Governor')]]");
            var rows = table.SelectNodes(".//tr");
            if (rows == null)
            {
                throw new Exception("Failed to find the table");
            }
            var items = new List<Item>();
            int partyRowSpan = 0;
            var hasTimeInOffice = rows[0].ChildNodes.Any(o => o.InnerText == "Time in office\n");
            var partyIndex = 4 + (hasTimeInOffice ? 2 : 0);
            for (int i = 1; i < rows.Count; ++i)
            {
                HtmlNode row = rows[i];
                var td = row.SelectSingleNode("td");
                if (td == null)
                {
                    continue;
                }

                int governorRowSpan = int.Parse(td.Attributes["rowspan"]?.Value ?? "1");
                i += governorRowSpan - 1;
                var thIndex = row.ChildNodes.IndexOf(row.SelectSingleNode("th"));
                if (thIndex < 0)
                {
                    throw new Exception($"Failed to parse row #{i}");
                }

                var termInOffice = row.ChildNodes[thIndex + 2].InnerText.Trim().Split(new[] { '-', '\u2013' }, StringSplitOptions.RemoveEmptyEntries);
                termInOffice[1] = Regex.Replace(termInOffice[1], @"\s*\(.+\)", "");
                items.Add(new Item
                {
                    Name = row.ChildNodes[thIndex].SelectSingleNode("a/text()").InnerText,
                    TermStart = Regex.Replace(HtmlEntity.DeEntitize(termInOffice[0]), @"\s*\[.+\]", ""),
                    TermEnd = Regex.Replace(HtmlEntity.DeEntitize(termInOffice[1]), @"\s*\[.+\]", ""),
                    Party = partyRowSpan > 0 ? items.Last().Party : row.ChildNodes[thIndex + partyIndex].SelectSingleNode("a").InnerText
                });
                if (partyRowSpan <= 0)
                {
                    partyRowSpan = int.Parse(row.ChildNodes[thIndex + partyIndex].Attributes["rowspan"]?.Value ?? "1") - governorRowSpan;
                }
            }

            return items;
        }
    }
}
