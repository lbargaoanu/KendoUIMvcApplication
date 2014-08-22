using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using StructureMap.Attributes;
using System.Net;
using System.Net.Http;

namespace Infrastructure.Web
{
    public abstract class ContextController<TContext, TViewModel> : ApiController
        where TContext : BaseContext
        where TViewModel : IEntity
    {
        protected abstract IQueryable<TViewModel> GetAllEntities();
        protected abstract void Add(TViewModel entity);
        protected abstract void Delete(TViewModel entity);
        protected abstract void Modify(TViewModel entity);

        protected virtual TViewModel GetById(int id)
        {
            return GetAllEntities().SingleOrDefault(e => e.Id == id);
        }

        protected virtual void OnModifyError(TViewModel entity)
        {
        }

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

        public virtual DataSourceResult GetAll([ModelBinder]DataSourceRequest request)
        {
            return GetAllEntities().ToDataSourceResult(request);
        }
        //[NonAction]
        //public virtual object MapToDto(TViewModel entity)
        //{
        //    return entity;
        //}
        protected virtual TViewModel Find(int id)
        {
            return GetById(id);
        }

        public virtual IHttpActionResult Get(int id)
        {
            var entity = GetById(id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        public virtual IHttpActionResult Post(TViewModel entity)
        {
            if (ModelState.IsValid)
            {
                Add(entity);
                Context.SaveChanges();
                var result = GetById(entity.Id);
                return Created(result);
            }

            return new ResponseMessageResult(Request.CreateErrorResponse((HttpStatusCode)422, ModelState));
        }

        public virtual IHttpActionResult Delete(int id)
        {
            var entity = Find(id);
            if (entity == null)
            {
                return NotFound();
            }
            Delete(entity);
            return Ok();
        }

        public virtual IHttpActionResult Put(int id, TViewModel entity)
        {
            if (id != entity.Id)
            {
                return BadRequest();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    Modify(entity);
                    Context.SaveChanges();
                }
                else
                    return new ResponseMessageResult(Request.CreateErrorResponse((HttpStatusCode)422, ModelState));
            }
            catch (Exception)
            {
                OnModifyError(entity);
                if (Find(entity.Id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var result = GetById(entity.Id);
            return Ok(result);            
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

    public class CrudNomController<TContext, TEntity> : CrudController<TContext, TEntity>
        where TEntity : ValidFromUntilBaseType
        where TContext : BaseContext
    {
        protected override IQueryable<TEntity> GetAllEntities(Expression<Func<TEntity, bool>> where)
        {
            IQueryable<TEntity> entities = Context.Set<TEntity>();

            var currentDate = DateTime.Now.Date;
            entities = entities.Where(x => (x.ValidFrom == null || x.ValidFrom <= currentDate)
                                        && (x.ValidUntil == null || x.ValidUntil >= currentDate));
            
            if (where != null)
            {
                entities = entities.Where(where);
            }
            return Include(entities).AsNoTracking();
        }
    }

    public class CrudController<TContext, TEntity> : ContextController<TContext, TEntity>
        where TEntity : VersionedEntity
        where TContext : BaseContext
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
           
            if (where != null)
            {
                entities = entities.Where(where);
            }
            return Include(entities).AsNoTracking();
        }

        protected override TEntity GetById(int id)
        {
            return GetAllEntities(e => e.Id == id).SingleOrDefault();
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

        protected override TEntity Find(int id)
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

    public abstract class ValidFromUntilBaseType : VersionedEntity
    {
        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUntil { get; set; }
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