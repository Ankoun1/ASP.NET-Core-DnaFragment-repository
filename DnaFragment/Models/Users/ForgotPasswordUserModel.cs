﻿namespace DnaFragment.Models.Users
{
    using System.ComponentModel.DataAnnotations;
    public class ForgotPasswordUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }        
      
    }
}
