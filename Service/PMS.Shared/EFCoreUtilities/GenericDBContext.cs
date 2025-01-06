using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PMS.Shared.EFCoreUtilities
{
    public class GenericDBContext:DbContext
    {
        public GenericDBContext(DbContextOptions options) : base(options)
        {
        }
    }
}
