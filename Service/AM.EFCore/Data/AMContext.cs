using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AM.EFCore.Data
{
    public class AMContextHO:AMContextBase
    {
        public AMContextHO(DbContextOptions<AMContextHO> options):base(options)
        {

        }
    }

    public class AMContextEstate : AMContextBase
    {
        public AMContextEstate(DbContextOptions<AMContextEstate> options) : base(options)
        {

        }
    }
}
