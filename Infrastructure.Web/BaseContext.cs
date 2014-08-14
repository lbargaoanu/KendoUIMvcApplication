using System;
using System.Data.Common;
using System.Data.Entity;
using AutoMapper;

namespace Infrastructure.Web
{
    public class ModelCreatingEventArgs : EventArgs
    {
        public DbModelBuilder ModelBuilder { get; set; }
    }

    public class SaveChangesEventArgs
    {
        public object State { get; set; }
        public BaseContext Context { get; set; }
    }

    public class BaseContext : DbContext
    {
        public event EventHandler<ModelCreatingEventArgs> ModelCreating;
        public event EventHandler<SaveChangesEventArgs> SavingChanges;
        public event EventHandler<SaveChangesEventArgs> SavedChanges;
        [ThreadStatic]
        private static bool executingEvent;

        protected BaseContext(DbConnection connection)
            : base(connection, false)
        {
            Init();
        }

        protected BaseContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Init();
        }

        private void Init()
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Types().Configure(c =>
            {
                c.Property("Id").HasColumnName(c.ClrType.Name + "ID");
                if(Mapper.FindTypeMapFor(c.ClrType, c.ClrType) == null)
                {
                    Mapper.CreateMap(c.ClrType, c.ClrType).IgnoreProperties(c.ClrType, p => p.IsNavigationProperty());
                }
            });
            var handler = ModelCreating;
            if(handler != null)
            {
                handler(this, new ModelCreatingEventArgs { ModelBuilder = modelBuilder });
            }
        }

        public override int SaveChanges()
        {
            int result;
            var savingHandler = SavingChanges;
            SaveChangesEventArgs args = null;
            if(!executingEvent && savingHandler != null)
            {
                executingEvent = true;
                try
                {
                        savingHandler(this, args = new SaveChangesEventArgs { Context = this });
                }
                finally
                {
                    executingEvent = false;
                }
            }

            result = base.SaveChanges();

            var savedHandler = SavedChanges;
            if(!executingEvent && savedHandler != null)
            {
                executingEvent = true;
                try
                {
                    savedHandler(this, args ?? new SaveChangesEventArgs { Context = this });
                }
                finally
                {
                    executingEvent = false;
                }
            }
            return result;
        }
    }
}