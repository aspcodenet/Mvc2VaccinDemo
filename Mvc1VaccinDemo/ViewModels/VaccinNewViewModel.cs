using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mvc1VaccinDemo.ViewModels
{
    public class VaccinNewViewModel 
    {
        [Range(1,100000, ErrorMessage = "Välj en din dumbom")]
        public int SelectedSupplierId { get; set; }
        public List<SelectListItem> AllSuppliers { get; set; } = new List<SelectListItem>();


        [MaxLength(50)]
        public string Namn { get; set; }
        public DateTime? EuOkStatus { get; set; }

        public int Type { get; set; }

        public List<SelectListItem> Types { get; set; } = new List<SelectListItem>();

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; }

        [Range(1, 1000000)]// 
        public int AntalDoser { get; set; }

    }
}