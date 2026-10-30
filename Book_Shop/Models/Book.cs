using System;
using System.Collections.Generic;

namespace Book_Shop.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Isbn { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int IdAuthor { get; set; }

    public int IdGenre { get; set; }

    public int IdPublisher { get; set; }

    public int PublicationYear { get; set; }

    public int Pages { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public string? Description { get; set; }

    public string? CoverImage { get; set; }

    public virtual ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();

    public virtual Author IdAuthorNavigation { get; set; } = null!;

    public virtual Genre IdGenreNavigation { get; set; } = null!;

    public virtual Publisher IdPublisherNavigation { get; set; } = null!;
}
