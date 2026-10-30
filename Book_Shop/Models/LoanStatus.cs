using System;
using System.Collections.Generic;

namespace Book_Shop.Models;

public partial class LoanStatus
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
}
