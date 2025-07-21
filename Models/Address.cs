namespace CRMApp.Models
{
    public class Address
    {
        public int AddressId { get; set; }

        public int? IndividualCustomerId { get; set; }
        public CustomerIndividual IndividualCustomer { get; set; }

        public int? CompanyCustomerId { get; set; }
        public CustomerCompany CompanyCustomer { get; set; }

        public string AddressType { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string FullAddress { get; set; }
    }
}
