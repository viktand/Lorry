using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.BottomNavigation;
using AlertDialog = Android.App.AlertDialog;
using Xamarin.Essentials;
using System;
using System.Timers;
using Lorry.Dto;
using System.Globalization;
using Android.Content.PM;

namespace Lorry
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)] //This is what controls orientation
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        private Connector _connector;
        private Timer _timer, _timer2, _timer3, _timerRequest;
        private bool _loadMode;
        private int _timeslotId;
        private int _driverId;
        private int _stamp;
        private string _loadtime;
        private Views _timerTask;
        private bool _searhMode;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _connector = new Connector
            {
                AuthServer = "http://api.trucks-online.antagosoft.com"
            };
                        
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            ShowSpinner(false);
            var isLogin = Preferences.Get("isLogin", "no");
            if (isLogin.Equals("no")) // Если нет авторизации
            {
                SelectViewMode(Views.Login);                
            }
            else
            {
                SelectViewMode(Views.Profile);
                _connector.Server = Preferences.Get("server", "");
                _connector.Token = Preferences.Get("token", "");
                _connector.TokenType = Preferences.Get("tokenType", "");
                LoadProfile();
            }

            ButtonsFunctions();
          
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            _timer = new Timer(2000)
            {
                AutoReset = false
            };
            _timer.Elapsed += UpdateTimerElapsed;
            _timer2 = new Timer(2000)
            {
                AutoReset = false
            };
            _timer2.Elapsed += ConnectTimerElapsed;            
            _timer3 = new Timer(2000)
            {
                AutoReset = false
            };
            _timer3.Elapsed += ProfileTimerElapsed;

            _timerRequest = new Timer(2000)
            {
                AutoReset = false
            };
            _timerRequest.Elapsed += TimerRequest_Elapsed;


            if (Preferences.Get("isTryUnloadStatus", false))
            {
                _timer2.Start();
            }
        }

        private void TimerRequest_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (_timerTask)
            {
                case Views.CloseTrip:
                    {
                        RunOnUiThread(() =>
                        {
                            NextTrip(Preferences.Get("nextTrip", false));
                        });
                        break;
                    }
            }
        }

        private void ButtonsFunctions()
        {
            FindViewById<Button>(Resource.Id.loginbutton).Click += LgButton_Click;
            FindViewById<Button>(Resource.Id.exitButton).Click += Exit_Clic;
            FindViewById<Button>(Resource.Id.connectbutton).Click += Connect_Click;
            FindViewById<Button>(Resource.Id.goButton).Click += ToJob_Click;
            FindViewById<Button>(Resource.Id.nogoButton).Click += ToRelax_Click;
        }

        private void ToRelax_Click(object sender, EventArgs e)
        {
            if (_searhMode) return;
            NextTrip(false);
        }

        private void ToJob_Click(object sender, EventArgs e)
        {
            if (_searhMode) return;
            NextTrip(true);
        }

        private async void NextTrip(bool v)
        {
            Preferences.Set("nextTrip", v); // todo надо сохранить режим при отключении !!!!! Додумать это
            SelectViewMode(Views.SearchTrip);
            _searhMode = true;
            ShowSpinner(true);
            var result = await _connector.NextTrip(v, _timeslotId);            
            if(result == 401) // отвалилась авторизация
            {
                ShowSpinner(false);
                SelectViewMode(Views.Login);
                _searhMode = false;
                return;
            }
            if(result == 0) // что-то пошло не так
            {
                ShowSpinner(false);
                _timerTask = Views.CloseTrip;
                SelectViewMode(Views.CloseTrip);
                _searhMode = false;
                _timerRequest.Start();
            }
        }

        private void ProfileTimerElapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void ConnectTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if(! await _connector.SetLoad(_timeslotId, _driverId, _loadtime))
            {
                _timer2.Start();
                return;
            }
            Preferences.Set("isTryUnloadStatus", false);
            _timer.Start();
        }

        private void Connect_Click(object sender, EventArgs e)
        {            
            if (_loadMode)
            {
                // todo
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Загрузка");
                alert.SetMessage("Вы уверены, что хотите перевести рейс в статус 'Загружен'?");
                alert.SetPositiveButton("Да", async (senderAlert, args) => {                   
                    if (!await _connector.SetLoad(_timeslotId, _driverId, _loadtime))
                    {
                        _loadtime = DateTime.Now.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT+0300'", CultureInfo.CreateSpecificCulture("en-US")); // "Tue Sep 28 2021 12:08:19 GMT+0300"
                        Preferences.Set("loadtime", _loadtime);
                        Preferences.Set("status", 3);
                        Preferences.Set("isTryUnloadStatus", true);
                        var statusBar = FindViewById<TextView>(Resource.Id.status);
                        statusBar.SetTextColor(Android.Graphics.Color.Black);
                        statusBar.SetBackgroundColor(Android.Graphics.Color.Green);
                        statusBar.Text = "Загружен";
                        FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Gone;
                        _timer.Stop(); // не обновлять рейс пока не уйдет статус
                        _timer2.Start();
                    }
                });
                alert.SetNegativeButton("Нет", (senderAlert, args) => {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
                var t = ((AlertDialog)dialog);
                var b = t.GetButton((int)DialogButtonType.Negative);
                b.BackgroundTintMode = Android.Graphics.PorterDuff.Mode.Overlay;
                b.SetBackgroundColor(Android.Graphics.Color.Red);
            }
        }

        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                LoadTrip(false);
            });
        }

        private void Exit_Clic(object sender, System.EventArgs e)
        {

            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Выход");
            alert.SetMessage("Вы уверены, что хотите вернуться к вводу пароля?");
            alert.SetPositiveButton("Да", (senderAlert, args) =>
            {
                Preferences.Set("isLogin", "no");
                SelectViewMode(Views.Login);
            });
            alert.SetNegativeButton("Нет", (senderAlert, args) =>
            {
                var t = _connector.LoadTrip().Result;
                Toast.MakeText(this, "Выход отменен", ToastLength.Short).Show();
            });            
            Dialog dialog = alert.Create();            
            dialog.Show();
            var t = ((AlertDialog)dialog);
            var b = t.GetButton((int)DialogButtonType.Negative);
            b.BackgroundTintMode = Android.Graphics.PorterDuff.Mode.Overlay;            
            b.SetBackgroundColor(Android.Graphics.Color.Red);            
        }     

        private async void LgButton_Click(object sender, System.EventArgs e)
        {
            var imm = (InputMethodManager)GetSystemService(InputMethodService);
            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
            var phone = FindViewById<EditText>(Resource.Id.editPhone).Text;
            var password = FindViewById<EditText>(Resource.Id.editPassword).Text;
            ShowSpinner(true);
            var request = await _connector.Login(phone, password);
            ShowSpinner(false);
            if (request)
            {
                var authPanel = FindViewById<LinearLayout>(Resource.Id.authpanel);
                authPanel.Visibility = ViewStates.Gone;
                FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
                FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Visible;
                Preferences.Set("phone", phone);
                Preferences.Set("password", password);
                Preferences.Set("token", _connector.Token);
                Preferences.Set("server", _connector.Server);
                Preferences.Set("tokenType", _connector.TokenType);
                Preferences.Set("isLogin", "yes");
                LoadProfile();
                return;
            }

            if (LastLogin(phone, password))
            {
                return;
            }

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Ошибка авторизации");
            alert.SetMessage("Сервер не принял авторизацию. Проверьте правильность номера телефона и пароля и попробуйте еще раз.");
            alert.SetPositiveButton("Ок", (senderAlert, args) => {
                //Toast.MakeText(this, "Deleted!", ToastLength.Short).Show();
            });
            //alert.SetNegativeButton("Cancel", (senderAlert, args) => {
            //    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
            //});
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private bool LastLogin(string phone, string password)
        {
            var phoneL = Preferences.Get("phone", "^");
            var passwordL = Preferences.Get("password", "^");
            if(! phone.Equals(phoneL) || !password.Equals(passwordL))
            {
                return false;
            }
            var authPanel = FindViewById<LinearLayout>(Resource.Id.authpanel);
            authPanel.Visibility = ViewStates.Gone;
            FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
            _connector.Server = Preferences.Get("server", "");
            _connector.Token = Preferences.Get("token", "");
            _connector.TokenType = Preferences.Get("tokenType", "");
            Preferences.Set("isLogin", "yes");
            return true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home: // profile
                    SelectViewMode(Views.Profile);
                    LoadProfile();
                    return true;
                case Resource.Id.navigation_dashboard: // trip
                    SelectViewMode(Views.Trip);
                    _stamp = -1;
                    LoadTrip(true);                    
                    return true;
                case Resource.Id.navigation_notifications:
                    SelectViewMode(Views.Notification);
                    return true;
            }
            return false;
        }

        private async void LoadProfile()
        {
            ShowSpinner(true);
            var request = await _connector.LoadProfile();
            ShowSpinner(false);
            if(request == null)
            {
                _timer3.Start();
                return;
            }
            if(request.driver.id == 0) // get error 401
            {
                Preferences.Set("isLogin", "no");
                SelectViewMode(Views.Login);
                return;

            }
            var tStr = request.driver.profile.surname + " " + (request.driver.profile.name?[0].ToString() ?? "") + 
                "." + (request.driver.profile.patronymic?[0].ToString() ?? "") + ".";
            FindViewById<TextView>(Resource.Id.fio).Text = tStr;
            FindViewById<TextView>(Resource.Id.driverphone).Text = request.driver.phone;
            tStr = "";
            foreach(var car in request.driver.cars)
            {
                if (tStr.Length > 1) tStr += "\n";
                tStr += car.number;
            }
            FindViewById<TextView>(Resource.Id.trucks).Text = "Транспорт:\n" + tStr;
            FindViewById<TextView>(Resource.Id.farm).Text = "Базовое хозяйство:\n" + request.driver.base_farm.name;
            tStr = "";
            foreach(var sm in request.driver.working_turns)
            {
                if (tStr.Length > 0) tStr += ", ";
                tStr += sm;
            }
            FindViewById<TextView>(Resource.Id.smens).Text = "Рабочие смены: " + tStr;
        }

        private async void LoadTrip(bool spin)
        {
            if (_timer2.Enabled) return;
            if (spin)
            {
                ShowSpinner(true);
                FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Invisible;
            }
            var request = await _connector.LoadTrip();
            if(request?.timeslot.status_id == 1 && _searhMode)
            {
                SelectViewMode(Views.Trip);
                _searhMode = false;
                ShowSpinner(false);
            }
            if (spin)
            {
                ShowSpinner(false);
            }
            var stamp = request != null ? request.GetHashCode() : -1;                       
            if (stamp != _stamp)
            {
                FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Visible;
                if (request == null)
                {
                    LoadLastTrip();
                    _stamp = stamp;                                        
                    _timer.Start();
                    return;
                }
                SaveTrip(request);
                LoadLastTrip();
            }

            if (spin)
            {
                LoadLastTrip();
            }
            _timer.Start();
        }

        private void SaveTrip(Trip request)
        {
            Preferences.Set("driverId", request.timeslot.driver_id);
            Preferences.Set("timeslotId", request.timeslot.id);
            Preferences.Set("farmIndex", request.timeslot.loading_cargo_station.farm.alternative_name);
            var tString = DateTime.Parse(request.timeslot.loading_date).ToString("dd.MM.yyyy") + " c " + request.timeslot.loading_time_from + " до " + request.timeslot.loading_time_to;
            Preferences.Set("loadTime", tString);
            tString = DateTime.Parse(request.timeslot.unloading_date).ToString("dd.MM.yyyy") + " c " + request.timeslot.unloading_time_from + " до " + request.timeslot.unloading_time_to;
            Preferences.Set("unloadTime", tString);
            tString = request.timeslot.loading_cargo_station.farm.name + " " + request.timeslot.loading_cargo_station.name;
            Preferences.Set("loadPlace", tString);
            Preferences.Set("inloadPlace", request.timeslot.unloading_cargo_station.name);
            Preferences.Set("tripNumber", request.timeslot.dq_id);
            Preferences.Set("plate", request.timeslot.car.number);
            Preferences.Set("status", request.timeslot.status_id);
            Preferences.Set("isLoader", request.isLoaderInUse);
            _stamp = request.GetHashCode();
        }

        private void ShowConnectButton(bool isLoaderInUse)
        {
            FindViewById<Button>(Resource.Id.connectbutton).Text = isLoaderInUse ? "Подключиться к погрузчику" : "Машина загружена";
            _loadMode = isLoaderInUse;
        }

        private void LoadLastTrip()
        {
            var status = Preferences.Get("status", 0);
            _timeslotId = Preferences.Get("timeslotId", 0);
            if (status == 5) // Рейс завершен
            {
                CloseTrip();
                return;
            }
            _driverId = Preferences.Get("driverId", 0);
            _loadtime = Preferences.Get("loadtime", "");
            FindViewById<TextView>(Resource.Id.indexload).Text = Preferences.Get("farmIndex", "");           
            FindViewById<TextView>(Resource.Id.loadTime).Text = Preferences.Get("loadTime", "");            
            FindViewById<TextView>(Resource.Id.unloadTime).Text = Preferences.Get("unloadTime", "");
            FindViewById<TextView>(Resource.Id.loadPlace).Text = Preferences.Get("loadPlace", "");
            FindViewById<TextView>(Resource.Id.unloadPlace).Text = Preferences.Get("inloadPlace", "");
            FindViewById<TextView>(Resource.Id.tripnumber).Text = Preferences.Get("tripNumber", "");
            FindViewById<TextView>(Resource.Id.car).Text = Preferences.Get("plate", "");
            var statusBar = FindViewById<TextView>(Resource.Id.status);
            switch (status)
            {
                case 1:
                    statusBar.SetTextColor(Android.Graphics.Color.Black);
                    statusBar.SetBackgroundColor(Android.Graphics.Color.Orange);
                    statusBar.Text = "Назначен";
                    FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Visible;
                    ShowConnectButton(Preferences.Get("isLoader", true));
                    break;
                case 3:
                    statusBar.SetTextColor(Android.Graphics.Color.Black);
                    statusBar.SetBackgroundColor(Android.Graphics.Color.Green);
                    statusBar.Text = "Загружен";
                    FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Gone;
                    break;
                case 4:
                    statusBar.SetTextColor(Android.Graphics.Color.Black);
                    statusBar.SetBackgroundColor(Android.Graphics.Color.LightBlue);
                    FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Gone;
                    statusBar.Text = "Прибыл";
                    break;
                case 10:
                    statusBar.SetTextColor(Android.Graphics.Color.LightGray);
                    statusBar.SetBackgroundColor(Android.Graphics.Color.Blue);
                    FindViewById<Button>(Resource.Id.connectbutton).Visibility = ViewStates.Gone;
                    statusBar.Text = "На заводе";
                    break;
            }
        }

        /// <summary>
        /// Форма закрытия рейса
        /// </summary>
        private void CloseTrip()
        {
            if (_searhMode)
            {
                SelectViewMode(Views.SearchTrip);
                return;
            }
            SelectViewMode(Views.CloseTrip);
        }

        private void ShowSpinner(bool state)
        {
            FindViewById<ProgressBar>(Resource.Id.spinner).Visibility = state ? ViewStates.Visible : ViewStates.Gone;
        }

        private void SelectViewMode(Views mode)
        {
            switch(mode){
                case Views.Login:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        break;
                    }
                case Views.Profile:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        break;
                    }
                case Views.Trip:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        break;
                    }
                case Views.CloseTrip:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        break;
                    }
                case Views.SearchTrip:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Visible;
                        break;
                    }
            }
        }
    }
}

