using Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
  public class Context : IdentityDbContext<User>
  {
    //constructor
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }
  }
}
