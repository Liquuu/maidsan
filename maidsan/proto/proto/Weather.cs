using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace madesan
{
    class Weather
    {
        const string NO_VALUE = "---";

        public static string GetWeatherText(bool enableSituation=false)
        {
            var url = "http://weather.livedoor.com/forecast/webservice/json/v1?city=130010";
            var req = WebRequest.Create(url);

            using (var res = req.GetResponse())
            using (var s = res.GetResponseStream())
            {
                dynamic json = DynamicJson.Parse(s);

                //天気(今日)
                dynamic today = json.forecasts[0];

                string dateLabel = today.dateLabel;
                string date = today.date;
                string telop = today.telop;

                var sbTempMax = new StringBuilder();
                dynamic todayTemperatureMax = today.temperature.max;
                if (todayTemperatureMax != null)
                {
                    sbTempMax.AppendFormat("{0}℃", todayTemperatureMax.celsius);
                }
                else
                {
                    sbTempMax.Append(NO_VALUE);
                }

                var sbTempMin = new StringBuilder();
                dynamic todayTemperatureMin = today.temperature.min;
                if (todayTemperatureMin != null)
                {
                    sbTempMin.AppendFormat("{0}℃", todayTemperatureMin.celsius);
                }
                else
                {
                    sbTempMin.Append(NO_VALUE);
                }

                //天気概況文
                var situation = json.description.text;

                //Copyright
                var link = json.copyright.link;
                var title = json.copyright.title;

                if (enableSituation)
                {
                    //return string.Format("{0}\n本日の天気は、{1}です。\n最高気温は、{2}、\n最低気温は、{3}みたいです。\n\n{4}\n\n{5}\n{6}、以上です。",
                    return string.Format("本日の天気は、{0}です。\n最高気温は、{1}、\n最低気温は、{2}みたいです。\n\n{3}、以上です。",
                        //date,
                        telop,
                        sbTempMax.ToString(),
                        sbTempMin.ToString(),
                        situation
                        //link,
                        //title
                        );
                }
                else
                {
                    //return string.Format("{0}\n本日の天気は、{1}です。\n最高気温は、{2}、\n最低気温は、{3}みたいです。\n\n{4}\n\n{5}\n{6}、以上です。",
                    return string.Format("本日の天気は、{0}です。\n最高気温は、{1}、\n最低気温は、{2}みたいです。以上です。",
                        //date,
                        telop,
                        sbTempMax.ToString(),
                        sbTempMin.ToString()
                        //situation
                        //link,
                        //title
                        );
                }
            }
        }
    }
}
