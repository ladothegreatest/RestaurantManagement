using RestaurantManagement.Entities;
using System.ComponentModel.DataAnnotations;
using RestaurantManagement.Common.Enums;

namespace RestaurantManagement.Dtos.Reservation
{
    public class UpdateReservationDto
    {

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ReservationTime { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }
    }

    public class UpdateReservationStatusDto
    {
        //enumdataType enum tipis mappings rtavs nebas database entityze
        [Required]
        [EnumDataType(typeof(ReservationStatus))]
        public ReservationStatus Status { get; set; }
    }
}