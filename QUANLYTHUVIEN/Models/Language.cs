using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class Language
{
    public int LanguageId { get; set; }

    public string LanguageName { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
