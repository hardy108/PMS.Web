using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PMS.EFCore.Helper
{
    public class GenericDBContext:DbContext
    {
        public GenericDBContext(DbContextOptions options) : base(options)
        {
        }
    }
}
