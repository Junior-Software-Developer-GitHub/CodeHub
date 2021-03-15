using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CodeHub.Models
{
    [MetadataType(typeof(UserMetaData))]
    public partial class User//extended class for adding validation
    {
        public string ConfirmPassword { get; set; }//added here bcs dont have in db 

    }
    public class UserMetaData
    {
        [Display(Name ="First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage ="First name required!")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name required!")]
        public string LastName { get; set; }


        [Display(Name ="Email ID")]
        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings =false,ErrorMessage ="Email ID required!")]
        public string EmailID { get; set; }

        [Display(Name ="Date of birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode =true,DataFormatString ="{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Pass is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Min 6 characters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm pass")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "diff pass")]
        public string ConfirmPassword { get; set; }
    }

}