﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace RestApi.Standard.Shared.Resources.Users
{
    public class LoginCreateResource : CreateResource
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
