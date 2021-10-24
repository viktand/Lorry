using Android.App;
using Android.Net.Wifi;
using Android.Text.Format;
using Common;
using Lorry.Models;
using System;
using System.Linq;
using System.Timers;

namespace Lorry.ViewModel
{
    public partial class Trip_vm
    {
        private string _gw;
        private string _lastNetwork;
        private Timer _wifiTimer;
        private LoaderConnector _loaderConnector;

       
        private void WiFiConnect()
        {
            _wifiTimer = new Timer(2000);
            _wifiTimer.Elapsed += _wifiTimer_Elapsed;
            Connect();
        }

         [Obsolete]
        private void Connect() 
        {            
            var wifiManager = (WifiManager)_act.GetSystemService("wifi");
            _gw = Formatter.FormatIpAddress(wifiManager.DhcpInfo.ServerAddress);
            var ssid = "field";
            var curSsid = "";
            if (wifiManager.ConnectionInfo.SupplicantState == SupplicantState.Completed)
            {
                curSsid = wifiManager.ConnectionInfo.SSID;
            }
            _lastNetwork = wifiManager.ConnectionInfo.SSID;
            wifiManager.SetWifiEnabled(true);
            var config = new WifiConfiguration
            {
                Ssid = "\"" + ssid + "\"",
            };
            var list = wifiManager.ConfiguredNetworks;
            foreach (var i in list.Where(x => x.Ssid.Contains(ssid) || x.Ssid.Contains(_lastNetwork)))
            {
                wifiManager.RemoveNetwork(i.NetworkId);
            }
            wifiManager.AddNetwork(config);
            list = wifiManager.ConfiguredNetworks;
            var sid = list.First(x => x.Ssid.Contains(ssid)).NetworkId;

            wifiManager.Disconnect();
            wifiManager.EnableNetwork(sid, true);
            _wifiTimer.Start();
        }

        private async void _wifiTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_trip.Status != Statuses.Connected)
            {
                var wifiManager = (WifiManager)_act.GetSystemService("wifi");
                var gw = Formatter.FormatIpAddress(wifiManager.DhcpInfo.ServerAddress);
                if (gw != _gw)
                {
                    _loaderConnector = new LoaderConnector(gw);
                    var result = await _loaderConnector.TryConnect(_trip);
                    if (result?.Status == LoaderMessages.Ok)
                    {
                        Alert("Машина добавлена в очередь, ожидайте приглашения на погрузку", "Вы подключены");
                        _trip.Status = Statuses.Connected;
                        _wifiTimer.Start();
                        return;
                    }
                }
                else
                {
                    Connect();
                    return;
                }
            }
            else
            {
                // ping loader
            }
            _wifiTimer.Start();
        }

        internal void SendLoaded()
        {
            throw new NotImplementedException();
        }
    }
}