using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Linq;
using System.Web;

namespace BlogService
{
    public class BaseModel
    {
        [Key]
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public BaseModel()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }

    public class User : BaseModel
    {
        public string Username { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }

        public int Blogs { get; set; }

        public UserInfo()
        {
            Username = null;
            Blogs = -1;
        }
    }


    public class Blog : BaseModel
    {
        public virtual User Author { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public virtual IList<Comment> Comments { get; set; }
    }
    public class BlogInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int CommentNumber { get; set; }

        public BlogInfo()
        {
            Id = null;
            Title = null;
            CommentNumber = -1;
        }

        public BlogInfo(Blog blog)
        {
            Title = blog.Title;
            CommentNumber = blog.Comments.Count;
        }
    }

    public class BlogDetail
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string UpdateTime { get; set; }
        public CommentInfo[] Comments { get; set; }

        public BlogDetail() { }

        public BlogDetail(Blog blog)
        {
            Author = blog.Author.Username;
            Title = blog.Title;
            Content = blog.Content;
            CommentInfo[] CommentInfos = blog.Comments.Select(x => new CommentInfo(x)).ToArray();
            Comments = CommentInfos;
        }
    }

    public class Comment : BaseModel
    {
        public virtual User Author { get; set; }
        public virtual Blog Blog { get; set; }
        public string Content { get; set; }
    }

    public class CommentInfo
    {
        public string Author { get; set; }
        public string Content { get; set; }

        public CommentInfo()
        {
        }

        public CommentInfo(Comment comment)
        {
            Author = comment.Author.Username;
            Content = comment.Content;
        }
    }

    public class WebServiceDbContext : DbContext
    {

        public WebServiceDbContext() :
            base(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\database.mdf;Integrated Security=True;Connect Timeout=30")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Comment>()
                .HasRequired(x => x.Blog)
                .WithMany(x => x.Comments)
                .WillCascadeOnDelete();
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries().ToList();
            var updateEntries = entries.Where(e => (e.Entity is BaseModel)
            && e.State == EntityState.Modified).ToList();

            updateEntries.ForEach(e =>
            {
                ((BaseModel)e.Entity).UpdatedAt = DateTime.Now;
            });
            return base.SaveChanges();
        }
    }
}