using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace Mvc1VaccinDemo.Services.Krisinformation
{
    public class KrisInfoService : IKrisInfoService
    {
        public class Test
        {
            public List<KrisInfo> ThemeList { get; set; } = new List<KrisInfo>();
        }
        public List<KrisInfo> GetAllKrisInformation()
        {
            //var client = new HttpClient();
            //string result = client.GetStringAsync("http://api.krisinformation.se/v1/themes?format=json").Result;

            //var listan = JsonConvert.DeserializeObject<Test>(result);
            //return listan.ThemeList;
            return new List<KrisInfo>()
            {
                new KrisInfo {Id="123",Title="Viktiga råd", Emergency = false,Text="Förutom de nationella råden gäller flera restriktioner och förbud som kan påverka dig.", LinkUrl = "https://www.krisinformation.se/detta-kan-handa/handelser-och-storningar/20192/myndigheterna-om-det-nya-coronaviruset/restriktioner-och-forbud/"},
                new KrisInfo{Id="998",Title="Om du känner oro", Emergency = false,Text="ur du kan ta hand om din eller andras psykiska hälsa under denna kris.",LinkUrl = "https://www.krisinformation.se/detta-kan-handa/handelser-och-storningar/20192/myndigheterna-om-det-nya-coronaviruset/for-dig-som-ar-orolig/"},
                new KrisInfo{Id="1224",Title="Information om covid-19 via SMS", Emergency = false,Text="Bla bla bla", LinkUrl = "https://www.krisinformation.se/detta-kan-handa/handelser-och-storningar/20192/myndigheterna-om-det-nya-coronaviruset/sms-om-covid-19/"}
            };
        }

        public KrisInfo GetKrisInformation(string id)
        {
            return GetAllKrisInformation().FirstOrDefault(r => r.Id == id);
        }


        public List<KrisInfo> GetEmergencies()
        {
            return GetAllKrisInformation().Where(r => r.Emergency).ToList();
        }
    }
}