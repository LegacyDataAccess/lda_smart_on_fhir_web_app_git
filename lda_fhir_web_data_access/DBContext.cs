using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace lda_fhir_web_data_access
{
    public class DBContext : DbContext
    {

        public DBContext() : base("DBContext")
        {

            Database.SetInitializer<DBContext>(null);
        }

        public DbSet<user> User { get; set; }
        public DbSet<userlocation> UserLocation { get; set; }

        public DbSet<ehrclient> EhrClient { get; set; }
        public DbSet<ehrsystem> EhrSystem { get; set; }


        public DbSet<patient> Patient { get; set; }
        public DbSet<patientextension> PatientExtension { get; set; }

        public DbSet<session_state> SessionState { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            modelBuilder.Entity<user>()
              .ToTable("user", "lda")
              .HasKey(p => p.id);

            modelBuilder.Entity<userlocation>()
                .ToTable("user_location", "lda")
                .HasKey(p => p.id);

            modelBuilder.Entity<ehrclient>()
                  .ToTable("ehr_client", "lda")
                  .HasKey(p => p.id);

            modelBuilder.Entity<ehrsystem>()
                .ToTable("ehr_system", "lda")
                .HasKey(p => p.id);

            modelBuilder.Entity<patient>()
                .ToTable("v_patient")
                .HasKey(p => p.id);

             modelBuilder.Entity<patientextension>()
                .ToTable("patient_extension")
                .HasKey(p => p.id);

            modelBuilder.Entity<session_state>()
             .ToTable("session_state", "lda")
             .HasKey(p => p.id);

        }
    }
}