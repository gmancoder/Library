﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.API
{
    public class UpdateRequest
    {
        [Required]
        [Display(Name = "Entity")]
        public string Entity { get; set; }

        [Required]
        [Display(Name = "Id")]
        public object Id { get; set; }

        [Required]
        [Display(Name = "Data")]
        public Dictionary<string, object> Data { get; set; }
    }
}
