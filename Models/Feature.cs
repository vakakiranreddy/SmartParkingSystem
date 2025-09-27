//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using Microsoft.EntityFrameworkCore;

//namespace SmartParkingSystem.Models
//{
//    [Index(nameof(Name), IsUnique = true)]
//    public class Feature
//    {
//        [Key]
//        public int Id { get; set; }

//        [Required]
//        [StringLength(50)]
//        public string Name { get; set; }

//        [StringLength(200)]
//        public string Description { get; set; }

//        [StringLength(255)]
//        [Url]
//        public string IconUrl { get; set; }

//        [Range(0, double.MaxValue)]
//        [Column(TypeName = "decimal(10,2)")]
//        public decimal PriceModifier { get; set; } = 0;

//        public bool IsActive { get; set; } = true;

//        // Navigation Properties
//        public ICollection<SlotFeature> SlotFeatures { get; set; } = new List<SlotFeature>();
//    }
//}

using SmartParkingSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Feature
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(200)]
    public string Description { get; set; }

    public byte[] IconData { get; set; } // Change from IconUrl to IconData

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceModifier { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public ICollection<SlotFeature> SlotFeatures { get; set; } = new List<SlotFeature>();
}
