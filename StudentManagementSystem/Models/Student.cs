using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [ EmailAddress, StringLength(100)]
        [Index("IX_Email", IsUnique = true)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Course is required")]
        public Course Course { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be exactly 10 digits")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Only numbers are allowed")]
        [Phone(ErrorMessage = "Invalid mobile number")]
        public string MobileNumber { get; set; }
    }
}