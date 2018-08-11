namespace PQT.Domain.Entities
{
    public class Country : EntityBase
    {
        public Country()
        {
        }
        public Country(Country country)
        {
            ID = country.ID;
            Code = country.Code;
            Name = country.Name;
            DialingCode = country.DialingCode;
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DialingCode { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}