using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Common.Dto;
using Lorry.Dto;
using Lorry.Models;
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
        private readonly string _url;
        private const int Port = 8877;        

        public LoaderConnector(string gw)
        {
            _url = gw;
        }

        public Task<LoadResponse> TryConnect(TripModel timeslot)
        {
            return Task.Run(() => 
            {
                try
                {
                    var request = new LoadRequest
                    {
                        Plate = timeslot.Plate,
                        Driver = timeslot.DriverSurname + " "
                            + timeslot.DriverName?[0] + "."
                            + timeslot.DriverPatronymic?[0] + ".",
                        FarmIndex = timeslot.FarmIndex,
                        Field = timeslot.LoadPlace
                    };
                   
                    var json = JsonConvert.SerializeObject(request);
                    var url ="http://" + _url + ":" + Port + "/api/connect";
                    var result = url.PostJsonToUrl(json);
                    var response = JsonConvert.DeserializeObject<LoadResponse>(result);
                  
                    return response;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            });
        }     
    }
}