﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMApp.Models
{
    public class CustomerIndividual
    {
        public int CustomerId { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; } = string.Empty;  // معمولا اسم اجباریه، ولی اگر اختیاریه nullable کن

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; } = string.Empty;   // همینطور

        [Display(Name = "نام پدر")]
        public string? FatherName { get; set; }  // nullable برای اختیاری بودن

        [DataType(DataType.Date)]
        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }  // nullable

        [Display(Name = "کد ملی")]
        public string? NationalCode { get; set; }  // nullable

        [Display(Name = "شماره شناسنامه")]
        public string? IdentityNumber { get; set; }  // nullable

        [Display(Name = "جنسیت")]
        public string? Gender { get; set; }  // nullable

        [Display(Name = "وضعیت تاهل")]
        public string? MaritalStatus { get; set; }  // nullable

        public List<Address> Addresses { get; set; } = new List<Address>();
        public List<ContactPhone> ContactPhones { get; set; } = new List<ContactPhone>();
        public List<Email> Emails { get; set; } = new List<Email>();

        public ICollection<CustomerCompanyRelation> CustomerCompanyRelations { get; set; } = new List<CustomerCompanyRelation>();

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
