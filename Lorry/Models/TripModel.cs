using Lorry.Dto;
using System;

namespace Lorry.Models
{
    public class TripModel
    {
        public TripModel(Trip request)
        {
            Id = request.timeslot.id;
            TripNumber = request.timeslot.dq_id;
            DriverId = request.timeslot.driver_id;
            FarmIndex = request.timeslot.loading_cargo_station.farm.alternative_name;
            Loadtime = DateTime.Parse(request.timeslot.loading_date).ToString("dd.MM.yyyy") + " c " + request.timeslot.loading_time_from + " до " + request.timeslot.loading_time_to;
            Unloadtime = DateTime.Parse(request.timeslot.unloading_date).ToString("dd.MM.yyyy") + " c " + request.timeslot.unloading_time_from + " до " + request.timeslot.unloading_time_to;
            LoadPlace = request.timeslot.loading_cargo_station.farm.name + " " + request.timeslot.loading_cargo_station.name;
            UnloadPlace = request.timeslot.unloading_cargo_station.name;
            Plate = request.timeslot.car.number;
            IsNeedLoader = request.isLoaderInUse;
            DriverName = request.timeslot.driver.profile.name;
            DriverSurname = request.timeslot.driver.profile.surname;
            DriverPatronymic = request.timeslot.driver.profile.patronymic;
        }

        public TripModel() { }       

        public override int GetHashCode()
        {            
            var hash = Id ^ DriverId ^ (FarmIndex?.GetHashCode() ?? 0) ^
                (Loadtime?.GetHashCode() ?? 0) ^ (Unloadtime?.GetHashCode() ?? 0) ^
                (LoadPlace?.GetHashCode() ?? 0) ^ (UnloadPlace?.GetHashCode() ?? 0) ^
                (Plate?.GetHashCode() ?? 0) ^ ((int)Status).GetHashCode() ^
                IsNeedLoader.GetHashCode();
            return hash;
        }

        public int Id { get; set; } 
        public string TripNumber { get; set; }
        public Statuses Status { get; set; }
        public int DriverId { get; set; }
        public string FarmIndex { get; set; }
        public string Loadtime { get; set; }
        public string Unloadtime { get; set; }
        public string LoadPlace { get; set; }
        public string UnloadPlace { get; set; }
        public string Plate { get; set; }
        public bool IsNeedLoader { get; set; }
        public string LoadtimeFact { get; set; }
        public string DriverSurname { get; internal set; }
        public string DriverName { get; internal set; }
        public string DriverPatronymic { get; internal set; }
    }   
}