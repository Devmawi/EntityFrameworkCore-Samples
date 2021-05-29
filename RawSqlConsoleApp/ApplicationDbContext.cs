using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawSqlConsoleApp
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Post> Posts { get; set; }
    }

    [Table("Post", Schema = "dbo")]
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Blog Blog { get; set; }
    }

    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public Blog Blog { get; set; }
    }


    public class ApplicationDbContext: DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        // see also https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#simple-dbcontext-initialization-with-new
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>()
                .ToTable("Blog")
                .Property(b => b.Rating)
                .HasDefaultValue(3);

            // HasIndex(b => b.Url)
            //.IsUnique()

            modelBuilder.Entity<Blog>()
                        .Property(b => b.CreatedAt)
                        .HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<Blog>()
                        .HasQueryFilter(b => b.Rating > 2);

            // Fluent API
            //modelBuilder.Entity<Post>()
            //   .ToTable("Post");
        }
    }
}
