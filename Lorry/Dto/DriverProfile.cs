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
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class _1
    {
        public string from { get; set; }
        public int to { get; set; }
    }

    public class _2
    {
        public string from { get; set; }
        public int to { get; set; }
    }

    public class FormattedWorkingTurns
    {
        public _1 _1 { get; set; }
        public _2 _2 { get; set; }
    }

    public class Addresses
    {
        public string actual_address { get; set; }
        public string legal_address { get; set; }
        public string post_address { get; set; }
    }

    public class Signatory
    {
        public object signatory_documents { get; set; }
        public object signatory_duration_end { get; set; }
        public string signatory_duration_start { get; set; }
        public string signatory_name { get; set; }
        public string signatory_position { get; set; }
        public string signatory_reason { get; set; }
    }

    public class Pivot
    {
        public int model_id { get; set; }
        public int role_id { get; set; }
        public string model_type { get; set; }
        public int user_id { get; set; }
        public int car_id { get; set; }
    }

    public class Company
    {
        public int id { get; set; }
        public string inn { get; set; }
        public string name { get; set; }
        public object created_by { get; set; }
        public object updated_by { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string full_name { get; set; }
        public object ticker { get; set; }
        public object entity_type { get; set; }
        public object tax_type { get; set; }
        public string ogrn { get; set; }
        public string kpp { get; set; }
        public object ati_code { get; set; }
        public bool has_ecp { get; set; }
        public object passport_main_spread { get; set; }
        public object passport_registration { get; set; }
        public object passport_number { get; set; }
        public object passport_issued_by_code { get; set; }
        public object passport_date { get; set; }
        public object passport_birthday { get; set; }
        public object passport_registration_address { get; set; }
        public string bank_bic { get; set; }
        public string bank { get; set; }
        public string correspondent_number { get; set; }
        public string accounting_number { get; set; }
        public Addresses addresses { get; set; }
        public List<Signatory> signatories { get; set; }
        public object insurance { get; set; }
        public object related_legal_entities { get; set; }
        public object agreements { get; set; }
        public object registration_documents { get; set; }
        public object deleted_at { get; set; }
        public object assurance { get; set; }
        public int status { get; set; }
        public object canceled_reason { get; set; }
        public object actual_address { get; set; }
        public object legal_address { get; set; }
        public object post_address { get; set; }
        public object passport_issued_by_name { get; set; }
        public int legal_check_status { get; set; }
        public object fields { get; set; }
        public object external_id { get; set; }
        public object weekly_schedule { get; set; }
        public object alternative_name { get; set; }
        public object loading_capacity { get; set; }
        public object active_loading_station_id { get; set; }
        public object working_hours { get; set; }
        public bool continuous_loading { get; set; }
        public List<Role> roles { get; set; }
    }

    public class CurrentTurn
    {
        public int index { get; set; }
        public string techDate { get; set; }
        public string periodFrom { get; set; }
        public string periodTo { get; set; }
        public string realDateTimeFrom { get; set; }
        public string realDateTimeTo { get; set; }
    }

    public class TripStatus
    {
        public int id { get; set; }
        public string name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int? external_code { get; set; }
        public string slug { get; set; }
    }

    public class Settings
    {
        public CurrentTurn currentTurn { get; set; }
        public List<TripStatus> tripStatuses { get; set; }
    }

    public class DriverProfile
    {
        public Driver driver { get; set; }
        public Settings settings { get; set; }
        public string serverName { get; set; }
    }


}