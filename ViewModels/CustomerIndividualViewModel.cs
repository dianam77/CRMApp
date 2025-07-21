using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMApp.ViewModels
{
    public class CustomerIndividualViewModel
    {
        public int CustomerId { get; set; }

        [Display(Name = "نام")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "نام پدر")]
        public string FatherName { get; set; }

        [Display(Name = "تاریخ تولد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "شماره شناسنامه")]
        public string IdentityNumber { get; set; }

        [Display(Name = "جنسیت")]
        public string Gender { get; set; }

        [Display(Name = "وضعیت تاهل")]
        public string MaritalStatus { get; set; }

        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();
        public List<PhoneViewModel> ContactPhones { get; set; } = new List<PhoneViewModel>();
        public List<EmailViewModel> Emails { get; set; } = new List<EmailViewModel>();
    }

}
