using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace BlogService
{
    /// <summary>
    /// BlogService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://bearhuchao.top/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class BlogService : System.Web.Services.WebService
    {
        readonly WebServiceDbContext DB = new WebServiceDbContext();
        [WebMethod]
        public string AddUser(string username)
        {
            var targetUser = DB.Users.Where(x => x.Username == username).FirstOrDefault();
            if (targetUser != null)
            {
                return targetUser.Id;
            }
            User newUser = new User { Username = username, Blogs = new List<Blog>() };
            DB.Users.Add(newUser);
            try
            {
                DB.SaveChanges();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return newUser.Id;
        }
        [WebMethod]
        public UserInfo GetUser(string username)
        {
            var targetUser = DB.Users.Where(x => x.Username == username).FirstOrDefault();
            if (targetUser == null)
            {
                return new UserInfo();
            }
            return new UserInfo() { Id = targetUser.Id, Username = targetUser.Username, Blogs = targetUser.Blogs.Count };
        }

        [WebMethod]
        public UserInfo[] GetUsers()
        {
            return DB.Users.Select(user => new UserInfo()
            {
                Id = user.Id,
                Username = user.Username,
                Blogs = user.Blogs.Count
            }).ToArray();
        }

        [WebMethod]
        public string AddBlog(string title, string author, string content)
        {
            var targetUser = DB.Users.Where(x => x.Username == author).FirstOrDefault();
            if (targetUser == null)
            {
                var newUserId = AddUser(author);
                targetUser = DB.Users.Find(newUserId);
            }
            Blog newBlog = new Blog
            {
                Title = title,
                Author = targetUser,
                Content = content,
                Comments = new List<Comment>()
            };
            targetUser.Blogs.Add(newBlog);
            DB.Blogs.Add(newBlog);
            try
            {
                DB.SaveChanges();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return newBlog.Id;
        }

        [WebMethod]
        public BlogInfo[] GetBlogs(string id)
        {
            return DB.Blogs.Where(x => x.Author.Id == id)
                .Select(blog => new BlogInfo()
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    CommentNumber = blog.Comments.Count
                }).ToArray();
        }

        [WebMethod]
        public BlogDetail GetBlogDetail(string id)
        {
            return DB.Blogs.Where(x => x.Id == id)
                .Select(blog => new BlogDetail
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    Author = blog.Author.Username,
                    UpdateTime = blog.CreatedAt.ToString(),
                    Content = blog.Content
                }).FirstOrDefault();
        }
        [WebMethod]
        public CommentInfo[] GetBlogComments(string BlogId)
        {
            var blog = DB.Blogs.Find(BlogId);
            return blog == null ? null : blog.Comments.Select(x => new CommentInfo() { Author = x.Author.Username, Content = x.Content }).ToArray();
        }

        [WebMethod]
        public string AddBlogComment(string BlogId, string author, string content)
        {
            var targetUser = DB.Users.Where(x => x.Username == author).FirstOrDefault();
            if (targetUser == null)
            {
                var newUserId = AddUser(author);
                targetUser = DB.Users.Find(newUserId);
            }
            var targetBlog = DB.Blogs.Find(BlogId);
            if (targetBlog == null)
            {
                return null;
            }
            Comment newComment = new Comment
            {
                Author = targetUser,
                Blog = targetBlog,
                Content = content
            };
            DB.Comments.Add(newComment);
            try
            {
                DB.SaveChanges();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return newComment.Id;
        }
    }
}
