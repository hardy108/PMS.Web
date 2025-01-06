using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class PMSContextHO: PMSContextBase
    {
        public PMSContextHO(DbContextOptions<PMSContextHO> options) : base(options)
        {

        }
    }

    public class PMSContextEstate : PMSContextBase
    {
        public PMSContextEstate(DbContextOptions<PMSContextEstate> options) : base(options)
        {

        }
    }
}
