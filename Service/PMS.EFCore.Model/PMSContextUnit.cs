using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class PMSContextUnit : PMSContextBase
    {
        public PMSContextUnit(DbContextOptions<PMSContextUnit> options) : base(options)
        {

        }
    }
}
