using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMApp.ViewModels
{
    public class RoleEditViewModel
    {
        public Guid RoleId { get; set; }

        [Required(ErrorMessage = "نام نقش الزامی است")]
        public string RoleName { get; set; }

        public List<UserCheckboxViewModel> Users { get; set; } = new();
    }

    public class UserCheckboxViewModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}
