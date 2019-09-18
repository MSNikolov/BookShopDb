using BookShop.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BookShop.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string Title { get; set; }

        [Column(TypeName = "NVARCHAR (1000)")]
        public string Description { get; set; }

        
        public DateTime? ReleaseDate { get; set; }

        public int Copies { get; set; }

        [Column(TypeName = "DECIMAL (16,2)")]
        public decimal Price { get; set; }

        public EditionType EditionType { get; set; }

        public AgeRestriction AgeRestriction { get; set; }

        public int AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; }

        public HashSet<BookCategory> BookCategories { get; set; } = new HashSet<BookCategory>();
    }
}
