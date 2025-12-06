using System;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models
{
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}