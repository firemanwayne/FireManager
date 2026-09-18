using FireManager.Concrete;
using System;
using System.Linq;

namespace FireManager.Entities
{
    public class FireManagerMember
    {
        public FireManagerMember() { }

        FireManagerMember(Member Member)
        {
            if (Member == null)
                throw new ArgumentNullException(nameof(Member));

            string AttributeValue(int id) => Member.Attributes?.Attribute?
                .FirstOrDefault(a => a != null && a.Id == id)?.Value?.value;

            Name = Member.Name?.Value ?? "Unknown";
            MemberId = Member.Id.ToString();
            Email = AttributeValue(9) ?? "Unknown";
            PhoneNumber = AttributeValue(7) ?? "Unknown";
            EmployeeTypeId = Member.Attributes?.Attribute?
                .FirstOrDefault(a => a != null && a.Id == 34)?.Id.ToString();
            EmployeeType = AttributeValue(34) ?? "Unknown";
            HireDate = AttributeValue(5) ?? "Unknown";
            Rank = AttributeValue(53) ?? "Unknown";
            Station = AttributeValue(45) ?? "Unknown";
            PrNumber = AttributeValue(PRNumberAttributeId) ?? "Unknown";
            Status = AttributeValue(104);
        }

        public string MemberId { get; }
        public string Name { get; }
        public string Email { get; }
        public string PhoneNumber { get; }
        public string EmployeeTypeId { get; }
        public string EmployeeType { get; }
        public string HireDate { get; }
        public string Rank { get; }
        public string Station { get; }
        public string PrNumber { get; }
        public string Status { get; }

        private const int PRNumberAttributeId = 46;

        public static implicit operator FireManagerMember(Member e) => new(e);
    }
}
