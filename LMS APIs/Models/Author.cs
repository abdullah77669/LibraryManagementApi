using System;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models
{
    public class Author
    {

        public int AuthorID { get; set; }


        [Required]
        [StringLength(255)]
        public string Name { get; set; }


        public string? Biography { get; set; }


        [Required]
        public DateTime DateOfBirth { get; set; }
    }
}