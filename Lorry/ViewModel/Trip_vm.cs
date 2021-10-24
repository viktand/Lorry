using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Lorry.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;

namespace Lorry.ViewModel
{
    public partial class Trip_vm
    {
        private readonly Activity _act;
        private TripModel _trip;
        private Timer _showTimer, _serverTimer;
        private Connector _connector;
        private UserModel _user;
        private Dictionary<int, Statuses> _statuses;

        public Trip_vm(Activity act)
        {
            _act = act;
            var user = GerUser();
            _connector = new Connector(user);
            _showTimer = new Timer(2000);
            _showTimer.Elapsed += _showTimer_Elapsed;
            _serverTimer = new Timer(2000);
            _serverTimer.Elapsed += _serverTimer_Elapsed;            
            var tripJson = Preferences.Get("trip", "");
            _trip = JsonConvert.DeserializeObject<TripModel>(tripJson) ?? new TripModel();
            _statuses = JsonConvert.DeserializeObject<Dictionary<int, Statuses>>(Preferences.Get("statuses", "")) ?? new Dictionary<int, Statuses>();
            if (Preferences.Get("isTryUnloadStatus", false)) // Если приложение помнит, что статус погрузки не отправлен на сервер
            {
                _serverTimer.Start();
            }
        }

        private UserModel GerUser()
        {
            var user = Preferences.Get("user", "");
            return JsonConvert.DeserializeObject<UserModel>(user);
        }

        private void SaveTrip()
        {
            Preferences.Set("trip", JsonConvert.SerializeObject(_trip));
        }

        public void Show()
        {
            if(_connector.Server == null)
            {
                var user = GerUser();
                _connector = new Connector(user);
            }
            ShowThis();
            if (_trip.Status != Statuses.Null)
            {
                ShowTrip();
            }
            else
            {
                SpinnerOnly();
            }
            _showTimer.Start();
        }

        private void ShowThis()
        {
            if(_trip.Status != Statuses.Appointed)
            {
                _act.FindViewById<Button>(Resource.Id.connectbutton).Click -= Connect_Click;
                _act.FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Gone;
            }
            else
            {
                _act.FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Visible;
                _act.FindViewById<Button>(Resource.Id.connectbutton).Click += Connect_Click;
            }
            switch (_trip.Status) {
                case Statuses.Canseled:
                    {
                        break;
                    }
                case Statuses.Ended:
                    {
                        break;
                    }
                case Statuses.None:
                    {
                        break;
                    }
                case Statuses.Paused:
                    {
                        break;
                    }
                default:
                    {                       
                        ShowView(Views.Trip);
                        break;
                    }
            }
        }

        private void ShowView(Views v, bool spn = false)
        {
            var act = _act as ILorryActivity;
            act.SelectViewMode(v);
            act.ShowSpinner(spn);
        }

        private async void _showTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await UpdateTrip();
        }

        public void StopUpdate()
        {
            _serverTimer.Stop();
        }

        private async void _serverTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!await _connector.SetLoad(_trip.Id, _trip.DriverId, _trip.LoadtimeFact))
            {
                _serverTimer.Start();
                return;
            }
            Preferences.Set("isTryUnloadStatus", false);
        }

        // connect to loader
        private void Connect_Click(object sender, EventArgs e)
        {
            if (_trip.IsNeedLoader)
            {
                WiFiConnect();
            }
            else // установка статуса загрузки водителем
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(_act);
                alert.SetTitle("Загрузка");
                alert.SetMessage("Вы уверены, что хотите перевести рейс в статус 'Загружен'?");
                alert.SetPositiveButton("Да", async (senderAlert, args) => {
                    _trip.LoadtimeFact = DateTime.Now.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT+0300'", CultureInfo.CreateSpecificCulture("en-US")); // "Tue Sep 28 2021 12:08:19 GMT+0300"
                    _trip.Status = Statuses.Loaded;
                    SaveTrip();
                    var statusBar = _act.FindViewById<TextView>(Resource.Id.status);
                    statusBar.SetTextColor(Android.Graphics.Color.Black);
                    statusBar.SetBackgroundColor(Android.Graphics.Color.Green);
                    statusBar.Text = "Загружен";
                    _act.FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Gone;
                    if (!await _connector.SetLoad(_trip.Id, _trip.DriverId, _trip.LoadtimeFact))
                    {
                        _serverTimer.Start();
                    }
                });
                alert.SetNegativeButton("Нет", (senderAlert, args) => {
                    Toast.MakeText(_act, "Cancelled!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
                var t = ((AlertDialog)dialog);
                var b = t.GetButton((int)DialogButtonType.Negative);
                b.BackgroundTintMode = Android.Graphics.PorterDuff.Mode.Overlay;
                b.SetBackgroundColor(Android.Graphics.Color.Red);
            }
        }

        private void SpinnerOnly()
        {
            ShowView(Views.None, true);
            (_act as ILorryActivity).ShowSpinner(true);
        }

        private Task UpdateTrip()
        {
            return Task.Run(() =>
            {
                if(_trip.Status == Statuses.Connected)
                {
                    return;
                }

                var trip = GetCurrentTrip();            
                if (trip != null && trip.GetHashCode() != _trip.GetHashCode())
                {
                    _trip = trip;
                    SaveTrip();
                    ShowTrip();
                }
                _showTimer.Start();
            });
        }

        private TripModel GetCurrentTrip()
        {
            var request = _connector.LoadTrip().Result;
            if (request == null)
            {
                return null;
            }
            return new TripModel(request)
            {
                Status = _statuses[request.timeslot.status_id]
            };
        }

        private void ShowTrip()
        {
            switch (_trip.Status)
            {
                case Statuses.Canseled:
                    {
                        break;
                    }
                case Statuses.Ended:
                    {
                        break;
                    }
                case Statuses.None:
                    {
                        break;
                    }
                case Statuses.Paused:
                    {
                        break;
                    }
                default:
                    {
                        ShowJobTrip();
                        break;
                    }
            }           
        }

        private void ShowJobTrip()
        {
            _act.RunOnUiThread(() =>
            {
                // select view
                _act.FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Visible;
                _act.FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                _act.FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                _act.FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Gone;
                _act.FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Gone;
                // Spinner off
                _act.FindViewById<ProgressBar>(Resource.Id.spinner).Visibility = ViewStates.Gone;
                // load date to view
                _act.FindViewById<TextView>(Resource.Id.indexload).Text = _trip.FarmIndex;
                _act.FindViewById<TextView>(Resource.Id.loadTime).Text = _trip.Loadtime;
                _act.FindViewById<TextView>(Resource.Id.unloadTime).Text = _trip.Unloadtime;
                _act.FindViewById<TextView>(Resource.Id.loadPlace).Text = _trip.LoadPlace;
                _act.FindViewById<TextView>(Resource.Id.unloadPlace).Text = _trip.UnloadPlace;
                _act.FindViewById<TextView>(Resource.Id.tripnumber).Text = _trip.TripNumber;
                _act.FindViewById<TextView>(Resource.Id.car).Text = _trip.Plate;
                var statusBar = _act.FindViewById<TextView>(Resource.Id.status);
                switch (_trip.Status)
                {
                    case Statuses.Appointed:
                        statusBar.SetTextColor(Android.Graphics.Color.Black);
                        statusBar.Text = "Назначен";
                        statusBar.SetTextColor(Android.Graphics.Color.Orange);
                        statusBar.SetBackgroundResource(Resource.Drawable.shape_border_newtrip);
                        _act.FindViewById<Button>(Resource.Id.connectbutton).Text = _trip.IsNeedLoader ? "Подключиться к погрузчику" : "Машина загружена";
                        break;
                    case Statuses.Loaded:
                        statusBar.SetTextColor(Android.Graphics.Color.DarkGreen);
                        statusBar.SetBackgroundResource(Resource.Drawable.shape_border_loaded);
                        statusBar.Text = "Загружен";
                        break;
                    case Statuses.Arrived:
                        statusBar.SetTextColor(Android.Graphics.Color.ParseColor("#ff33b5e5"));
                        statusBar.SetBackgroundResource(Resource.Drawable.shape_border_arived);
                        statusBar.Text = "Прибыл";
                        break;
                    case Statuses.OnPlant:
                        statusBar.SetTextColor(Android.Graphics.Color.ParseColor("#ff0000ff"));
                        statusBar.SetBackgroundResource(Resource.Drawable.shape_border_unload);
                        statusBar.Text = "На заводе";
                        break;
                    case Statuses.Connected:
                        statusBar.SetTextColor(Android.Graphics.Color.ParseColor("#ffff0000"));
                        statusBar.SetBackgroundResource(Resource.Drawable.shape_border_unload);
                        statusBar.Text = "Подключен";
                        break;
                }
            });
        }

        private void Alert(string text, string head)
        {
            _act.RunOnUiThread(() =>
            {
                var alert = new AlertDialog.Builder(_act);
                alert.SetTitle(head);
                alert.SetMessage(text);
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    return;
                });
                Dialog dialog = alert.Create();
                dialog.Show();
                return;
            });
        }
    }
}