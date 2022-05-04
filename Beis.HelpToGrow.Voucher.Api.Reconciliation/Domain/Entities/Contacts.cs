using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities
{
    [Table("Contacts", Schema = "dbo")]
    public class Contacts
    {
        [Key]
        public int Id { get; set; }
       [Required]
       [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        public bool IsFamily { get; set; }
        public string City { get; set; }
        public DateTime DateOfBirth { get; set; }
        // public IEnumerable<ContactEmails> EmailAddresses { get; set; }
        // public IEnumerable<ContactPhone> PhoneNumbers { get; set; }
        
    }
}