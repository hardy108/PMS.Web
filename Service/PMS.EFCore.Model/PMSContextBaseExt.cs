using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using System.Data.SqlClient;
using PMS.EFCore.Helper;


namespace PMS.EFCore.Model
{
    public partial class PMSContextBase
    {
        private string _connectionString = string.Empty;
        public string ConnectionString { get { return _connectionString; } }

        public PMSContextBase()
        {
            throw new Exception("Invalid connection string");
        }

        public PMSContextBase(DbContextOptions options)
            : base(options)
        {
            ReadOptions(options);
        }

        private void ReadOptions(DbContextOptions options)
        {
            foreach (var extension in options.Extensions)
            {
                if (extension.GetType() == typeof(Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension))
                {
                    _connectionString = ((Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension)extension).ConnectionString;
                    break;
                }
            }
        }



        #region Views
        private void BuildModelForViews(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VBKUACTIVITY>(entity => { entity.HasKey(e => new { e.ACTID }); });
            modelBuilder.Entity<VBKUMATERIAL>(entity => { entity.HasKey(e => new { e.MATID }); });
            modelBuilder.Entity<VBKUEMPLOYEE>(entity => { entity.HasKey(e => new { e.EMPID }); });

            modelBuilder.Entity<VRKHACTIVITY>(entity => { entity.HasKey(e => new {  e.ACTID }); });
            modelBuilder.Entity<VRKHBLOCK>(entity => { entity.HasKey(e => new {  e.BLOCKID }); });
            modelBuilder.Entity<VRKHMANDOR>(entity => { entity.HasKey(e => new {  e.SPVID }); });
            modelBuilder.Entity<VRKHMATERIAL>(entity => { entity.HasKey(e => new {  e.MATID}); });

            modelBuilder.Entity<VLOADINGEMPLOYEE>(entity => { entity.HasKey(e => new { e.EMPID }); });
            modelBuilder.Entity<VLOADINGBLOCK>(entity => { entity.HasKey(e => new { e.BLOCKID }); });

            modelBuilder.Entity<VHARVESTBLOCK>(entity => { entity.HasKey(e => new { e.HARVESTCODE, e.BLOCKID }); });

            modelBuilder.Entity<VBLOCK>(entity => { entity.HasKey(e => e.BLOCKID); });
            //modelBuilder.Query<VBLOCK>().ToView("VBLOCK");
            modelBuilder.Entity<VCOMPANY>(entity => { entity.HasKey(e => e.LEGALID); });
            //modelBuilder.Query<VCOMPANY>().ToView("VCOMPANY");
            modelBuilder.Entity<VDIVISI>(entity => { entity.HasKey(e => e.DIVID); });

            //modelBuilder.Query<VDIVISI>().ToView("VDIVISI");
            modelBuilder.Query<VDOKET>().ToView("VDOKET");
            modelBuilder.Query<VORGANIZATION>().ToView("VORGANIZATION");
            modelBuilder.Query<VRKH>().ToView("VRKH");
            modelBuilder.Query<VSCHEDULEEMPLOYEE>().ToView("VSCHEDULEEMPLOYEE");
            modelBuilder.Query<VTTRANMILL>().ToView("VTTRANMILL");
            modelBuilder.Entity<VUNIT>(entity => { entity.HasKey(e => e.UNITCODE); });
            //modelBuilder.Query<VUNIT>().ToView("VUNIT");
            modelBuilder.Query<VEMPLOYEE>().ToView("VEMPLOYEE");
            //modelBuilder.Query<VLOANEMP>().ToView("VLOANEMP");

            modelBuilder.Entity<VRKH1MANDOR>(entity => { entity.HasKey(e => new { e.ACTID , e.MANDORID }); });
            modelBuilder.Entity<VRKH1BLOCK>(entity => { entity.HasKey(e => new { e.BLOCKID }); });
            modelBuilder.Entity<VRKH1EMPLOYEE>(entity => { entity.HasKey(e => new { e.EMPID }); });
            modelBuilder.Entity<VRKH1MATERIAL>(entity => { entity.HasKey(e => new { e.MATID }); });
            
        }

        public DbSet<VBLOCK> VBLOCK { get; set; }
        public DbSet<VCOMPANY> VCOMPANY { get; set; }
        public DbSet<VDIVISI> VDIVISI { get; set; }
        public DbQuery<VDOKET> VDOKET { get; set; }
        public DbQuery<VORGANIZATION> VORGANIZATION { get; set; }
        public DbQuery<VRKH> VRKH { get; set; }
        
        public DbQuery<VSCHEDULEEMPLOYEE> VSCHEDULEEMPLOYEE { get; set; }
        public DbQuery<VTTRANMILL> VTTRANMILL { get; set; }
        public DbSet<VUNIT> VUNIT { get; set; }
        public DbQuery<VEMPLOYEE> VEMPLOYEE { get; set; }
        //public DbQuery<VLOANEMP> VLOANEMP { get; set; }
       
        #endregion

        #region Function Scalars
        #endregion

        #region Function Table
        public DbQuery<fn_DHS_Doket_GetById_Result> fn_DHS_Doket_GetById_Result { get; set; }
        public IQueryable<fn_DHS_Doket_GetById_Result> fn_DHS_Doket_GetById(string DivisionId, string DoketId)
        {
            return fn_DHS_Doket_GetById_Result.FromSql($"Select * from fn_DHS_Doket_GetById({DivisionId},{DoketId})");
        }

        
        public IQueryable<DHS_DOKET> fn_DHS_Doket_List(string EstateIds, string DivisionIds,  string BlockIds,DateTime? StartDate, DateTime? EndDate,string Status,int?  RowNoStart,int? RowNoEnd)
        {
            return DHS_DOKET.FromSql($"Select * From fn_DHS_Doket_List({EstateIds},{DivisionIds},{BlockIds},{StartDate},{EndDate},{Status},{RowNoStart},{RowNoEnd})");
        }

        public DbQuery<fn_DHS_Doket_ListCount_Result> fn_DHS_Doket_ListCount_Result { get; set; }
        public IQueryable<fn_DHS_Doket_ListCount_Result> fn_DHS_Doket_ListCount(string EstateIds, string DivisionIds, string BlockIds, DateTime? StartDate, DateTime? EndDate, string Status)
        {
            return fn_DHS_Doket_ListCount_Result.FromSql($"Select * From fn_DHS_Doket_ListCount({EstateIds},{DivisionIds},{BlockIds},{StartDate},{EndDate},{Status})");
        }

        
       
       

        //public DbQuery<fn_PMSW_Token_MaxIdle_Result> fn_PMSW_Token_MaxIdle_Result { get; set; }
       

        public DbQuery<usf_SplitString_Result> usf_SplitString_Result { get; set; }
        public IQueryable<usf_SplitString_Result> usf_SplitString(string RowData,string  SplitOn)
        {
            return usf_SplitString_Result.FromSql($"select * from usf_SplitString({RowData},{SplitOn})");
        }

        #endregion

        #region Stored Procedure 
       

        #endregion
    }
}
