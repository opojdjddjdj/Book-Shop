using System;
using System.Collections.Generic;

namespace Book_Shop.Models;

public partial class BookLoan
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public int IdBook { get; set; }

    public DateOnly DateIssued { get; set; }

    public DateOnly DateDue { get; set; }

    public DateOnly? DateReturned { get; set; }

    public int IdStatus { get; set; }

    public virtual Book IdBookNavigation { get; set; } = null!;

    public virtual LoanStatus IdStatusNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
