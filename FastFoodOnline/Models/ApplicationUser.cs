using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFoodOnline.Models
{
    [Table("ApplicationUser")]
    public class ApplicationUser : IdentityUser<Guid>
    {
        // Chỉ thêm field mở rộng
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public ICollection<Order> Orders { get; set; }

        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
        public virtual ICollection<IdentityUserClaim<Guid>> UserClaims { get; set; } = new List<IdentityUserClaim<Guid>>();
        public virtual ICollection<IdentityUserLogin<Guid>> UserLogins { get; set; } = new List<IdentityUserLogin<Guid>>();

    }
}
