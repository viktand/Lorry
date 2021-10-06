using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lorry.Dto
{
    public class Car
    {
        public int id { get; set; }
        public string number { get; set; }
        public object mark { get; set; }
        public object model { get; set; }
        public object color { get; set; }
        public object vin { get; set; }
        public int load_capacity { get; set; }
        public object holding_capacity { get; set; }
        public string glonass { get; set; }     
        public int type { get; set; }
        public object company_id { get; set; }
        public int status { get; set; }
        public object external_id { get; set; }
        public bool hidden { get; set; }
        public int tms_id { get; set; }
        public List<object> trailer_types { get; set; }
    }

    public class Profile
    {
        public int user_id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string patronymic { get; set; }
        public object email { get; set; }
        public int created_by { get; set; }
        public int? updated_by { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public object deleted_at { get; set; }
        public string passport_serial { get; set; }
        public object license_serial { get; set; }
        public string fullName { get; set; }
        public string shortName { get; set; }
    }

    public class Role
    {
        public int id { get; set; }
        public string name { get; set; }
        public string guard_name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string label { get; set; }
        public Pivot pivot { get; set; }
    }

    public class BaseFarm
    {
        public int id { get; set; }
        public string name { get; set; }
        public string alternative_name { get; set; }
        public List<Role> roles { get; set; }
    }

    public class Driver
    {
        public int id { get; set; }
        public string phone { get; set; }   
        public object company_id { get; set; }
        public bool isForeign { get; set; }
        public int status { get; set; }
        public object external_id { get; set; }
        public string app_version { get; set; }
        public bool? ready_to_trip { get; set; }
        public string ready_at { get; set; }
        public string ready_changed_at { get; set; }
        public object unready_at { get; set; }
        public int? active_car_id { get; set; }
        public object factory_id { get; set; }
        public bool? is_factory_worker { get; set; }
        public int? base_farm_id { get; set; }
        public List<int> working_turns { get; set; }
        public string on_line_from { get; set; }
        public string notification_token { get; set; }
        public bool hidden { get; set; }
        public int tms_id { get; set; }
        public bool take_car_communication_from_core { get; set; }
        public List<object> license_types { get; set; }
        public Profile profile { get; set; }
        public object company { get; set; }
        public List<Role> roles { get; set; }
        public List<object> permissions { get; set; }
        public List<Car> cars { get; set; }
        public BaseFarm base_farm { get; set; }
    }

    public class Farm
    {
        public int id { get; set; }
        public string name { get; set; }
        public string alternative_name { get; set; }
        public List<Role> roles { get; set; }
    }

    public class LoadingCargoStation
    {
        public int id { get; set; }
        public int farm_id { get; set; }
        public string name { get; set; }
        public object short_name { get; set; }
        public Farm farm { get; set; }
    }

    public class UnloadingCargoStation
    {
        public int id { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
    }

    public class Timeslot
    {
        public int id { get; set; }
        public int quota_id { get; set; }
        public object sub_quota_id { get; set; }
        public string loading_date { get; set; }
        public string loading_time_from { get; set; }
        public string loading_time_to { get; set; }
        public string loading_place { get; set; }
        public int loading_cargo_station_id { get; set; }
        public string unloading_date { get; set; }
        public string unloading_time_from { get; set; }
        public string unloading_time_to { get; set; }
        public object unloading_place { get; set; }
        public int unloading_cargo_station_id { get; set; }
        public int car_id { get; set; }
        public object trailer_id { get; set; }
        public int driver_id { get; set; }
        public int status_id { get; set; }
        public string weight { get; set; }
        public object actual_weight { get; set; }
        public string dq_id { get; set; }
        public object registration_on_factory_datetime { get; set; }
        public object created_by { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public object deleted_at { get; set; }
        public object updated_by { get; set; }
        public object external_id { get; set; }
        public object finish_confirmed_at { get; set; }
        public int quota_period_id { get; set; }
        public string estimated_unloading_datetime { get; set; }
        public string estimated_loading_datetime { get; set; }
        public object actual_loading_datetime { get; set; }
        public object actual_unloading_datetime { get; set; }
        public object canceled_at { get; set; }
        public object arrival_datetime { get; set; }
        public object custom_changed_by { get; set; }
        public bool is_planned_late { get; set; }
        public string preliminary_loading_datetime { get; set; }
        public string preliminary_unloading_datetime { get; set; }
        public object move_reason { get; set; }
        public Car car { get; set; }
        public object trailer { get; set; }
        public Driver driver { get; set; }
        public LoadingCargoStation loading_cargo_station { get; set; }
        public UnloadingCargoStation unloading_cargo_station { get; set; }
    }

    public class Trip
    {
        public Timeslot timeslot { get; set; }
        public bool isLoaderInUse { get; set; }
        public string serverName { get; set; }

        public override int GetHashCode()
        {
            var hash = timeslot.id ^ timeslot.driver_id ^
                timeslot.loading_cargo_station.farm.alternative_name.GetHashCode() ^
                timeslot.loading_time_from.GetHashCode() ^
                timeslot.loading_time_to.GetHashCode() ^
                timeslot.unloading_time_from.GetHashCode() ^
                timeslot.unloading_time_to.GetHashCode() ^
                timeslot.loading_cargo_station.name.GetHashCode() ^
                timeslot.dq_id.GetHashCode() ^
                timeslot.car.number.GetHashCode() ^
                timeslot.status_id ^
                isLoaderInUse.GetHashCode();
            return hash;
        }
    }
}