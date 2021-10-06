using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Lorry.Dto;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Lorry
{
    public class LoaderConnector
    {
        private const string url = "192.168.43.1";
        private const int port = 8080;

        public Task<int> Connect(Timeslot timeslot)
        {
            return Task.Run(() => 
            {
                try
                {
                    var loadSt = new LoadingStation
                    {
                        id = timeslot.loading_cargo_station.id,
                        farm_id = timeslot.loading_cargo_station.farm_id,
                        name = timeslot.loading_cargo_station.name,
                        farm = new Farm
                        {
                            id = timeslot.loading_cargo_station.id,
                            name = timeslot.loading_cargo_station.farm.name,
                            alternative_name = timeslot.loading_cargo_station.farm.alternative_name
                        }
                    };
                    var request = new ToLoaderMessage
                    {
                        status = "new",
                        dqId = timeslot.dq_id,
                        id = timeslot.id,
                        loadingDate = timeslot.loading_date,
                        loadingPeriod = timeslot.loading_time_from + " - " + timeslot.loading_time_to,
                        loadingPlace = timeslot.loading_place,
                        alternativeName = timeslot.loading_cargo_station.farm.alternative_name,
                        loadingStation = loadSt,
                        phone = timeslot.driver.phone,
                        driver = timeslot.driver.profile.fullName
                    };

                    var json = JsonConvert.SerializeObject(request);
                    var result = url.PostJsonToUrl(json);
                    var response = JsonConvert.DeserializeObject(result);

                    return 200;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    return 0;
                }
            });
        }

        public Task<int> Send(Timeslot timeslot)
        {
            return Task.Run(() =>
            {
                try
                {
                    var ipAddr = IPAddress.Parse(url);
                    var ipEndPoint = new IPEndPoint(ipAddr, port);

                    var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(ipEndPoint);
                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    string SummaryMessage = GetJson(timeslot);

                    byte[] msg = Encoding.ASCII.GetBytes(SummaryMessage);

                    sender.Send(msg);
                    byte[] buffer = new byte[1024];
                    int lengthOfReturnedBuffer = sender.Receive(buffer);
                    char[] chars = new char[lengthOfReturnedBuffer];

                    Decoder d = Encoding.UTF8.GetDecoder();
                    int charLen = d.GetChars(buffer, 0, lengthOfReturnedBuffer, chars, 0);
                    var returnedJson = new string(chars);
                    Console.WriteLine("The Json:{0}", returnedJson);
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return 200;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex.ToString());
                    return 0;
                }
            });
        }

        private string GetJson(Timeslot timeslot)
        {
            var loadSt = new LoadingStation
            {
                id = timeslot.loading_cargo_station.id,
                farm_id = timeslot.loading_cargo_station.farm_id,
                name = timeslot.loading_cargo_station.name,
                farm = new Farm
                {
                    id = timeslot.loading_cargo_station.id,
                    name = timeslot.loading_cargo_station.farm.name,
                    alternative_name = timeslot.loading_cargo_station.farm.alternative_name
                }
            };
            var request = new ToLoaderMessage
            {
                status = "new",
                dqId = timeslot.dq_id,
                id = timeslot.id,
                loadingDate = timeslot.loading_date,
                loadingPeriod = timeslot.loading_time_from + " - " + timeslot.loading_time_to,
                loadingPlace = timeslot.loading_place,
                alternativeName = timeslot.loading_cargo_station.farm.alternative_name,
                loadingStation = loadSt,
                phone = timeslot.driver.phone,
                driver = timeslot.driver.profile.fullName
            };

            var json = JsonConvert.SerializeObject(request);
            return json;
        }
    }
}