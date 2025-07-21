﻿namespace CRMApp.ViewModels
{
    public class CompanyEditViewModel
    {
        public int CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string EconomicCode { get; set; }
        public string NationalId { get; set; }
        public string RegisterNumber { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public string IndustryField { get; set; }
        public string Website { get; set; }

        public List<EmailViewModel> Emails { get; set; } = new();
        public List<PhoneViewModel> ContactPhones { get; set; } = new();
        public List<AddressViewModel> Addresses { get; set; } = new();
    }
}
