using BookShop.Data;
using BookShop.Initializer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BookShop
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var context = new BookShopContext(new DbContextOptions<BookShopContext>());


            Console.WriteLine(RemoveBooks(context));
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var books = context.Books
                .Where(b => b.AgeRestriction.ToString().ToLower() == command.ToLower())
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToList();

            var res = new StringBuilder();

            foreach (var book in books)
            {
                res.AppendLine(book);
            }

            return res.ToString().Trim();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var result = new StringBuilder();

            var books = context.Books
                .Where(b => b.Copies < 5000 && b.EditionType.ToString().ToLower() == "gold")
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList();

            foreach (var book in books)
            {
                result.AppendLine(book);
            }

            return result.ToString().Trim();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var result = new StringBuilder();

            var books = context.Books.Select(b => new
            {
                b.Title,
                b.Price
            })
                .OrderByDescending(b => b.Price);

            var expensiveBooks = books.Where(b => b.Price > 40).ToList();

            foreach (var book in expensiveBooks)
            {
                result.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return result.ToString().Trim();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(b => (int)b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    b.Title
                });

            var res = new StringBuilder();

            foreach (var book in books)
            {
                res.AppendLine(book.Title);
            }

            return res.ToString().Trim();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var cats = input.ToLower().Split();

            var result = new StringBuilder();

            var books = context.Books
                .Where(b => b.BookCategories.Any(bc => cats.Contains(bc.Category.Name.ToLower())))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            foreach (var book in books)
            {
                result.AppendLine(book);
            }

            return result.ToString().Trim();
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var data = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context.Books
                .Where(b => b.ReleaseDate < data)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price
                });

            var result = new StringBuilder();

            foreach (var book in books)
            {
                result.AppendLine($"{book.Title} - {book.EditionType.ToString()} - ${book.Price:f2}");
            }

            return result.ToString().Trim();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var res = new StringBuilder();

            var authors = context.Authors
                .Where(a => a.FirstName.EndsWith(input))
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName
                });

            foreach (var author in authors)
            {
                res.AppendLine($"{author.FirstName} {author.LastName}");
            }

            return res.ToString().Trim();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var res = context.Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToList();

            var result = new StringBuilder();

            foreach (var book in res)
            {
                result.AppendLine(book);
            }

            return result.ToString().Trim();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .Where(b => b.Title.Length > lengthCheck)
                .Select(b => b.BookId)
                .ToList();

            return books.Count;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Include(a => a.Books)
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName,
                    a.Books
                })
                .OrderByDescending(a => a.Books.Sum(b => b.Copies));

            var result = new StringBuilder();

            foreach (var author in authors)
            {
                result.AppendLine($"{author.FirstName} {author.LastName} - {author.Books.Sum(b => b.Copies)}");
            }

            return result.ToString().Trim();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var booksProfit = context.Categories
                .Include(c => c.CategoryBooks)
                .Select(c => new
                {
                    Profit = c.CategoryBooks.Sum(cb => cb.Book.Copies * cb.Book.Price),
                    c.Name
                })
                .OrderByDescending(c => c.Profit);

            var result = new StringBuilder();

            foreach (var cat in booksProfit)
            {
                result.AppendLine($"{cat.Name} ${(cat.Profit):f2}");
            }

            return result.ToString().Trim();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categories = context.Categories
                .Include(c => c.CategoryBooks)
                .Select(c => new
                {
                    Name = c.Name,
                    Books = c.CategoryBooks
                    .OrderByDescending(cb => cb.Book.ReleaseDate)
                    .Take(3)
                    .Select(b => new
                    {
                        BookTitle = b.Book.Title,
                        Year = b.Book.ReleaseDate.Value.Year
                    })
                    .ToList()
                })
                .OrderBy(c => c.Name)
                .ToList();

            var result = new StringBuilder();

            foreach (var category in categories)
            {
                result.AppendLine($"--{category.Name}");

                foreach (var book in category.Books)
                {
                    result.AppendLine($"{book.BookTitle} ({book.Year})");
                }
            }

            return result.ToString().Trim();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Copies < 4200)
                .ToList();

            foreach (var book in books)
            {
                context.Books.Remove(book);
            }

            context.SaveChanges();

            return books.Count;
        }
    }
}
