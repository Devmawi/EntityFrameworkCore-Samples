using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RawSqlConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("1.) DbContext Initialization");

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = "RawSqlConsoleAppDb",
                IntegratedSecurity = true
            };
            var connectionString = connectionStringBuilder.ToString();

            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                                        .UseSqlServer(connectionString)
                                            .Options;

            var context = new ApplicationDbContext(contextOptions);
            var wasCreated = await context.Database.EnsureCreatedAsync();

            Console.WriteLine($"2.) Database created: {wasCreated}");

            await context.Database.BeginTransactionAsync();

            try
            {
                var blog = new Blog
                {
                    Url = "http://blogs.msdn.com/dotnet",
                    Posts = new List<Post>
                    {
                        new Post { Title = "Intro to C#" },
                        new Post { Title = "Intro to VB.NET" },
                        new Post { Title = "Intro to F#" }
                    }
                };

                context.Blogs.Add(blog);
                context.SaveChanges();
                await context.Database.CommitTransactionAsync();
            }
            catch (Exception)
            {

                throw;
            }

            var blogs = await context.Blogs.Where(b  => b.Rating > 0).ToArrayAsync();
            Console.WriteLine($"Blogs with Rating < 3: {blogs.Where(b => b.Rating < 3).Count()}");

            Console.WriteLine($"3.) FromSqlInterpolated");
            var createdAt = DateTime.Now.AddSeconds(-5);
            var blogPosts = context.BlogPosts
                                   .FromSqlInterpolated($"EXECUTE [dbo].[uspGetBlogs] {createdAt}")
                                   .AsAsyncEnumerable();

            await foreach (var p in blogPosts)
            {
                Console.WriteLine($"Title: {p.Title}");
            }

            Console.WriteLine($"4.) FromSqlRaw");
            createdAt = DateTime.Now.AddSeconds(-5);
            var createdAtSqlParam = new SqlParameter("@createdAt", createdAt);
            var blogPosts2 = await context.BlogPosts
                                   .FromSqlRaw("EXECUTE [dbo].[uspGetBlogs] @createdAt=@createdAt", createdAtSqlParam)
                                   // .Include(p => p.Blog) Can use in Stored Procedure: https://docs.microsoft.com/en-us/ef/core/querying/raw-sql#including-related-data
                                   .ToArrayAsync();

            foreach (var p in blogPosts2)
            {
                Console.WriteLine($"Title: {p.Title}");
            }
        }
    }
}
