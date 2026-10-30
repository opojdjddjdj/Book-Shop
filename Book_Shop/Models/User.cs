using System;
using System.Collections.Generic;

namespace Book_Shop.Models;

public partial class User
{
    public int Id { get; set; }

    public int IdRole { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string CardNumber { get; set; } = null!;

    public string Login { get; set; } = null!;

    public virtual ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
