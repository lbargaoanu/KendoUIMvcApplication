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

    public class SaveChangesEventArgs : EventArgs
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
            SaveChangesEventArgs args = null;

            ExecuteHandler(SavingChanges, ref args);

            var result = base.SaveChanges();

            ExecuteHandler(SavedChanges, ref args);

            return result;
        }

        private void ExecuteHandler(EventHandler<SaveChangesEventArgs> handler, ref SaveChangesEventArgs args)
        {
            if(handler == null || executingEvent)
            {
                return;
            }
            try
            {
                executingEvent = true;
                handler(this, args ?? (args = new SaveChangesEventArgs { Context = this }));
            }
            finally
            {
                executingEvent = false;
            }
        }
    }
}