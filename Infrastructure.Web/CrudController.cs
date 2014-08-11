using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using System.Web.Http.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using StructureMap.Attributes;

namespace Infrastructure.Web
{
    public class ContextController<TContext, TViewModel> : ApiController where TContext : BaseContext where TViewModel : IEntity
    {
        protected virtual IQueryable<TViewModel> GetAllEntities() { return Enumerable.Empty<TViewModel>().AsQueryable(); }
        protected virtual TViewModel GetById(int id) { return default(TViewModel); }
        protected virtual void Add(TViewModel entity) { }
        protected virtual void Delete(TViewModel entity) { }
        protected virtual void Modify(TViewModel entity) { }
        
        protected virtual void OnModifyError(TViewModel entity) { }

        [SetterProperty]
        public IMediator Mediator { get; set; }

        [SetterProperty]
        public TContext Context { get; set; }

        protected TResponse Get<TResponse>(IQuery<TResponse> query)
        {
            return Mediator.Get(query);
        }

        protected TResult Send<TResult>(ICommand<TResult> command)
        {
            return Mediator.Send(command);
        }

        public virtual DataSourceResult GetAll(DataSourceRequest request)
        {
            return GetAllEntities().ToDataSourceResult(request);
        }

        public virtual TViewModel Find(int id)
        {
            return GetById(id);
        }

        public virtual IHttpActionResult Get(int id)
        {
            var entity = GetById(id);
            if(entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        public virtual IHttpActionResult Post(TViewModel entity)
        {
            Add(entity);
            Context.SaveChanges();
            return Created(entity);
        }

        public virtual IHttpActionResult Delete(int id)
        {
            var entity = Find(id);
            if(entity == null)
            {
                return NotFound();
            }
            Delete(entity);
            return Ok();
        }

        public virtual IHttpActionResult Put(int id, TViewModel entity)
        {
            if(id != entity.Id)
            {
                return BadRequest();
            }
            try
            {
                Modify(entity);
                Context.SaveChanges();
            }
            catch(Exception)
            {
                OnModifyError(entity);
                if(Find(entity.Id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(entity);
        }

        protected internal OkNegotiatedContentResult<Wrapper> Ok(TViewModel entity)
        {
            return Ok(new Wrapper(entity));
        }

        protected internal CreatedAtRouteNegotiatedContentResult<Wrapper> Created(TViewModel entity)
        {
            return CreatedAtRoute("DefaultApi", new { id = entity.Id }, new Wrapper(entity));
        }
    }

    public class CrudController<TContext, TEntity> : ContextController<TContext, TEntity> where TEntity : VersionedEntity where TContext : BaseContext
    {
        protected void SetRowVersion(TEntity source, TEntity destination)
        {
            destination.SetRowVersion(source, Context);
        }

        protected internal virtual IQueryable<TEntity> Include(IQueryable<TEntity> entities)
        {
            return entities;
        }

        protected override IQueryable<TEntity> GetAllEntities()
        {
            return GetAllEntities(null);
        }

        protected virtual IQueryable<TEntity> GetAllEntities(Expression<Func<TEntity, bool>> where)
        {
            IQueryable<TEntity> entities = Context.Set<TEntity>();
            if(where != null)
            {
                entities = entities.Where(where);
            }
            return Include(entities).AsNoTracking();
        }

        protected override TEntity GetById(int id)
        {
            var entities = Context.Set<TEntity>();
            return Include(entities).SingleOrDefault(e => e.Id == id);
        }

        protected override void OnModifyError(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Detached;
        }

        protected override void Modify(TEntity entity)
        {
            Context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        protected override void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        protected override void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public override TEntity Find(int id)
        {
            return Context.Set<TEntity>().SingleOrDefault(e => e.Id == id);
        }
    }

    public class Wrapper
    {
        public Wrapper(params IEntity[] entities)
        {
            Data = entities;
            Total = entities.Length;
        }
        public IEntity[] Data { get; set; }
        public int Total { get; set; }
    }

    public abstract class VersionedEntity : Entity
    {
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public interface IEntity
    {
        int Id { get; set; }
    }

    public abstract class Entity : IEntity
    {
        [Key]
        public int Id { get; set; }
        
        [NotMapped]
        public bool Exists
        {
            get
            {
                return Id > 0;
            }
        }
    }
}