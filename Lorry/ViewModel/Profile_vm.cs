using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Lorry.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;

namespace Lorry.ViewModel
{
    public class Profile_vm
    {
        private UserModel _user;
        private Connector _connector;
        private Activity _act;
        private Timer _profileTimer;
        private Dictionary<int, Statuses> _statusDict;

        public Profile_vm(Activity act)
        {
            _act = act;
            var user = Preferences.Get("user", "");
            _user = user.IsNullOrEmpty() ? new UserModel() : JsonConvert.DeserializeObject<UserModel>(user);
            _connector = new Connector(_user);
            _profileTimer = new Timer(2000);
            _profileTimer.Elapsed += _profileTimer_Elapsed;
            var st = Preferences.Get("statuses", "");
            _statusDict = JsonConvert.DeserializeObject<Dictionary<int, Statuses>>(st) ?? new Dictionary<int, Statuses>();
            Start();
        }

        public void Start()
        {
            if (_user.IsAuth)
            {               
                StartWithAuth();
                SelectView(Views.Profile);
                _profileTimer.Start();
                return;
            }
            SelectView(Views.Login);
        }

        public void Stop()
        {
            _profileTimer.Stop();
        }

        private async void _profileTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (LoginResults.Ok == await TryLoadUserData())
            {
                _act.RunOnUiThread(() =>
                {
                    Start();
                });
            }
            _profileTimer.Start();
        }

        public bool IsAuth()
        {
            return _user.IsAuth;
        }

        // Press auth button
        public async Task<bool> Auth(string phone, string password, bool stg = false)
        {
            if (phone.IsNullOrEmpty())
            {
                if (!_user.Phone.IsNullOrEmpty())
                {
                    _user.IsAuth = true;
                    Start();
                    return true;
                }
                Alert("Не указаны данные для авторизации (логин или пароль)", "Oшибка");
                return false;
            }
            _user.IsStaging = stg;
            _user.Phone = phone.ToClearPhone();
            _user.CurrentPassword = password;
            _connector.AuthServer = _user.IsStaging ? "http://api.trucks-online.antagosoft.com" : "https://tms.prodimex.ru";
            _connector.SllOff();
            switch(await _connector.Login(_user.Phone, _user.CurrentPassword)) // успешная авторизация
            {
                case 200:
                    {
                        _user.Server = _connector.Server;
                        _user.Token = _connector.Token;
                        _user.TokenType = _connector.TokenType;
                        _user.IsAuth = true;
                        SaveUser();
                        switch (await TryLoadUserData())
                        {
                            case LoginResults.Ok:
                                Start();
                                return true;
                            case LoginResults.ConnectionError:
                                Alert("Ошибка связи с сервером", "Ошибка");
                                break;
                            case LoginResults.LoginError:
                                Alert("Ошибка в номере телефона или пароле", "Ошибка");
                                break;
                        }
                        return false;
                    }
                case 401:
                    {
                        Alert("Ошибка в номере телефона или пароле", "Ошибка");
                        return false;
                    }
             }
            Alert("Нет связи с сервером, попробуйте повторить", "Ошибка");
            return false;
        }

        private async Task<LoginResults> TryLoadUserData()
        {
            if (_user.CurrentPassword.IsNullOrEmpty())
            {
                return LoginResults.LoginError;
            }


            var user = await _connector.LoadProfile();
            if (user == null)
            {
                // net error
                return LoginResults.ConnectionError;
            }
            if (user.driver.id == 0)
            {
                // 401
                return LoginResults.ConnectionError;
            }
            var usr = new UserModel
            {
                Surname = user.driver.profile.surname,
                Name = user.driver.profile.name,
                Patronymic = user.driver.profile.patronymic,
                Cars = user.driver.cars.Select(x => new DriverCar { Id = x.id, Plate = x.number }).ToList(),
                WorkShift = user.driver.working_turns,
                BaseFarm = user.driver.base_farm.name
            };
            UpdateRipStatusDict(user);
            if (usr.GetHashCode() != _user.GetHashCode())
            {
                _user.Surname = usr.Surname;
                _user.Name = usr.Name;
                _user.Patronymic = usr.Patronymic;
                _user.Cars = usr.Cars;
                _user.WorkShift = usr.WorkShift;
                _user.BaseFarm = usr.BaseFarm;
                SaveUser();
                return LoginResults.Ok;
            }
            return LoginResults.NotUpdate;
        }

        private void UpdateRipStatusDict(Dto.DriverProfile user)
        {
            var statusDict = new Dictionary<int, Statuses>();
            foreach(var st in user.settings.tripStatuses)
            {
                statusDict.Add(st.id, st.slug.ToStatusName());
            }
            
            if(_statusDict.NotEquals(statusDict))
            {
                _statusDict = statusDict;
                Preferences.Set("statuses", JsonConvert.SerializeObject(statusDict));
            }
        }

        private void SaveUser()
        {
            var user = JsonConvert.SerializeObject(_user);
            Preferences.Set("user", user);
        }

        private void StartWithAuth()
        {
            SelectView(Views.Profile);
            LoadUserData();
        }    

        private void LoadUserData()
        {           
            var tStr = _user.Surname + " " + (_user.Name?[0].ToString() ?? "") +
                "." + (_user.Patronymic?[0].ToString() ?? "") + ".";
            _act.FindViewById<TextView>(Resource.Id.fio).Text = tStr;
            _act.FindViewById<TextView>(Resource.Id.driverphone).Text = _user.Phone;
            tStr = "";
            foreach (var car in _user.Cars ?? new List<DriverCar>())
            {
                if (tStr.Length > 1)
                {
                    tStr += "\n";
                }
                tStr += car.Plate;
            }
            _act.FindViewById<TextView>(Resource.Id.trucks).Text = "Транспорт:\n" + tStr;
            _act.FindViewById<TextView>(Resource.Id.farm).Text = "Базовое хозяйство:\n" + _user.BaseFarm;
            tStr = "";
            foreach (var sm in _user.WorkShift ?? new List<int>())
            {
                if (tStr.Length > 0) tStr += ", ";
                tStr += sm;
            }
            _act.FindViewById<TextView>(Resource.Id.smens).Text = "Рабочие смены: " + tStr;
        }

        private void SelectView(Views v)
        {
            (_act as ILorryActivity).SelectViewMode(v);
        }

        private void Alert(string message, string head)
        {
            (_act as ILorryActivity).Alert(message, head);
        }
    }
}