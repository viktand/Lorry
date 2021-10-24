using System.Collections.Generic;

namespace Lorry.Models
{
    public class UserModel
    {
        public bool IsAuth { get; set; }

        public string Phone { get; set; }

        public string CurrentPassword { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public List<DriverCar> Cars { get; set; }
        public string BaseFarm { get; set; }
        public List<int> WorkShift { get; set; }
        public bool IsStaging { get; set; }
        public string Server { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; }

        public override int GetHashCode()
        {
            var t = (Surname?.GetHashCode() ^ Name?.GetHashCode() ^ Patronymic?.GetHashCode() ^ BaseFarm?.GetHashCode()) ?? 0;
            foreach(var i in Cars?? new List<DriverCar>())
            {
                t ^= i.Plate?.GetHashCode() ?? 0;
            }
            foreach(var i in WorkShift ?? new List<int>())
            {
                t ^= i.GetHashCode();
            }
            return  t;
        }
    }

    public class DriverCar
    {
        public string Plate { get; set; }
        public int Id { get; set; }
    }
}