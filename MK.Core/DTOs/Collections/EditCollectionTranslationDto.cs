using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Collections
{
    public class EditCollectionTranslationDto
    {
        [Required]
        public Language? Language { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
