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
using Android.Content.PM;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Lorry.ViewModel;

namespace Lorry
{
    [Activity(Theme = "@style/AppTheme", //MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)] //This is what controls orientation
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener, ILorryActivity
    {
        private Connector _connector;
        private Timer  _timer3, _timerRequest;
        private Views _timerTask;
        private bool _searhMode;
        private string[] _carsList;
        private AlertDialog _dialog;
        private int _activCarId;
        private string _phone;
        private bool _stg;       
        private Trip_vm _tripvm;
        private Profile_vm _profile;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
                        
            Platform.Init(this, savedInstanceState);            
            ShowSpinner(false);
            _profile = new Profile_vm(this);
                      
            if (Preferences.Get("isTryUnloadStatus", false)) // Если приложение помнит, что статус погрузки не отправлен на сервер
            {
                _tripvm = new Trip_vm(this);
                _tripvm.SendLoaded();
            }
            ButtonsFunctions();
          
            var navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
                             
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
                case Views.StartJob:
                    {
                        RunOnUiThread(() =>
                        {
                            RequestJob();
                        });
                        break;
                    }
            }
        }

        private void ButtonsFunctions()
        {
            FindViewById<Button>(Resource.Id.loginbutton).Click += LgButton_Click;
            FindViewById<Button>(Resource.Id.exitButton).Click += Exit_Clic;
           
            FindViewById<Button>(Resource.Id.goButton).Click += ToJob_Click;
            FindViewById<Button>(Resource.Id.nogoButton).Click += ToRelax_Click;
            FindViewById<Switch>(Resource.Id.switchJob).Click += ReturnToJob_Click;
            FindViewById<TextView>(Resource.Id.editPhone).FocusChange += Phone_FocusChange;
            FindViewById<Button>(Resource.Id.newpassbutton).Click += GetPass_Click;
        }

        private async void GetPass_Click(object sender, EventArgs e)
        {
            var tv = FindViewById<TextView>(Resource.Id.editPhone);
            Phone_FocusChange(tv, new View.FocusChangeEventArgs(false));
            ShowSpinner(true);
            if (!_phone.StartsWith("7"))
            {
                Alert("Укажите действительный телефонный номер в соответствующем поле", "Ошибка");
                return;
            }
            var result = await _connector.GetPassword(_phone);
            ShowSpinner(false);
            switch (result)
            {
                case 200:
                    {
                        Alert(_stg ? "Пароль 987654 (авторизация на тестовом сервере)" : "Пароль отправлен в СМС на указанный телефонный номер", "Успешно");
                        return;
                    }
                case 401:
                    {
                        Alert("Водитель с таким номером телефона не найден", "Ошибка");
                        return;
                    }
                case 0:
                    {
                        Alert("Ошибка запроса. Проверьте доступность Интернета или обратитесь к диспетчеру", "Ошибка");
                        return;
                    }
            }            
        }

        public void Alert(string text, string head)
        {
            RunOnUiThread(() =>
            {
                var alert = new AlertDialog.Builder(this);
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

        private void Phone_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus) return;
            var textView = (sender as TextView);
            _phone = textView.Text;
            _stg = _phone.StartsWith("#");
            var s = new StringBuilder();
            foreach (char c in _phone)
            {
                if ("0123456789".Contains(c))
                {
                    s.Append(c);
                }
            }
            _phone = s.ToString();
            if (!((_phone.StartsWith("79") && _phone.Length == 11) ||
                (_phone.StartsWith("9") && _phone.Length == 10) ||
                (_phone.StartsWith("89") && _phone.Length == 11)))
            {
                Alert("Текст в строке телефонного номера не удалось распознать как телефонный номер", "Ошибка");
                return;
            }
            if (_phone.Length == 11) _phone = _phone.Substring(1);
            _phone = "7" + _phone;
            textView.Text = "+" + _phone.Insert(1, "(").Insert(5, ")").Insert(9, "-").Insert(12, "-");           
        }

        private void ReturnToJob_Click(object sender, EventArgs e)
        {
            var cars = Preferences.Get("cars", "");
            if (cars.Equals(""))
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Проверьте настройки");
                alert.SetMessage("Сервер сообщает, что у вас нет машины для получения рейса. Обратитесь к диспетчеру для назначения машины");
                alert.SetPositiveButton("Ок", (senderAlert, args) => {
                    return;
                });              
                Dialog dialog = alert.Create();
                dialog.Show();
                return;
            }
            _carsList = cars.Split(";");
            if(_carsList.Length > 1)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Выбор машины");
                alert.SetMessage("У вас несколько машин. Чтобы получить рейс, выберите одну из них:");
                alert.SetView(Resource.Layout.selectcar_view);
                alert.SetPositiveButton("Отмена", (senderAlert, args) => {
                    return;
                });
                _dialog = alert.Create();         
                _dialog.Show();                
                var list = _dialog.FindViewById<ListView>(Resource.Id.listcars);
                var numbers = new List<string>();
                _carsList.ToList().ForEach(x => numbers.Add(x.Split("-")[0]));
                list.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, numbers);
                list.ItemClick += SelectCar_Click;
                return;
            }
        }      

        private void SelectCar_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            var list = sender as ListView;
            var car = (string) list.GetItemAtPosition(e.Position);
            _activCarId = int.Parse(_carsList.First(x => x.Contains(car)).Split("-")[1]);
            _dialog.Dismiss();
            Toast.MakeText(this, car, ToastLength.Short).Show();
            RequestJob();
        }

        /// <summary>
        /// Выход из режима неактивности
        /// </summary>
        private async void RequestJob()
        {
            ShowSpinner(true);
            _searhMode = true;
            SelectViewMode(Views.SearchTrip);
            var result = await _connector.RequestJob(_activCarId);
            if(result == 401)
            {
                ShowSpinner(false);
                SelectViewMode(Views.Login);
                _searhMode = false;
                return;
            }
            if (result == 0) // что-то пошло не так todo: это надо протестировать. выглядит подозрительно
            {
                ShowSpinner(false);
                _timerTask = Views.StartJob;
                SelectViewMode(Views.StartJob);
                _searhMode = false;
                _timerRequest.Start();
            }

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
            var result = await _connector.NextTrip(v, 0); // _timeslotId);            
            if(result == 401) // отвалилась авторизация
            {
                ShowSpinner(false);
                SelectViewMode(Views.Login);
                _searhMode = false;
                return;
            }
            if(result == 0) // что-то пошло не так todo: это надо протестировать. выглядит подозрительно
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
                Toast.MakeText(this, "Выход отменен", ToastLength.Short).Show();
            });            
            Dialog dialog = alert.Create();            
            dialog.Show();
            var t = ((AlertDialog)dialog);
            var b = t.GetButton((int)DialogButtonType.Negative);
            b.BackgroundTintMode = Android.Graphics.PorterDuff.Mode.Overlay;            
            b.SetBackgroundColor(Android.Graphics.Color.Red);            
        }     

        private async void LgButton_Click(object sender, EventArgs e)
        {
            var imm = (InputMethodManager)GetSystemService(InputMethodService);
            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
            var phone = FindViewById<TextView>(Resource.Id.editPhone).Text;
            var password = FindViewById<TextView>(Resource.Id.editPassword).Text;
            ShowSpinner(true);
            await _profile.Auth(phone, password, _stg);
            ShowSpinner(false);
        }       

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
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
                    _profile.Start();
;                    return true;
                case Resource.Id.navigation_dashboard: // trip                                        
                    if(_tripvm == null) _tripvm = new Trip_vm(this);
                    _profile.Stop();
                    _tripvm.Show();
                    return true;
                case Resource.Id.navigation_notifications:
                    SelectViewMode(Views.Notification);
                    return true;
            }
            return false;
        }   

        public void ShowSpinner(bool state)
        {
            FindViewById<ProgressBar>(Resource.Id.spinner).Visibility = state ? ViewStates.Visible : ViewStates.Gone;
        }

        public void SelectViewMode(Views mode)
        {
            switch (mode){
                case Views.Login:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Gone;
                        FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Gone;
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
                        FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Gone;
                        FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Gone;
                        break;
                    }
                case Views.Trip:
                    {
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Gone;
                        FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Gone;                        
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
                        FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Gone;
                        FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Gone;
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
                        FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Gone;
                        FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Gone;
                        break;
                    }
                case Views.StartJob:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Visible;
                        FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Gone;
                        break;
                    }
                case Views.None:
                    {
                        FindViewById<BottomNavigationView>(Resource.Id.navigation).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.authpanel).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.homeview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.closetripview).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.tripSearch).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.startJob).Visibility = ViewStates.Gone;
                        FindViewById<RelativeLayout>(Resource.Id.loadview).Visibility = ViewStates.Visible;
                        break;
                    }
            }
        }
       
    }
}

