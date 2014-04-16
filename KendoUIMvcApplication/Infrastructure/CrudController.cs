using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using StructureMap.Attributes;

namespace KendoUIMvcApplication
{
    public class CrudController<TEntity> : BaseController where TEntity : Entity
    {
        [NonAction]
        public virtual IQueryable<TEntity> Include(IQueryable<TEntity> entities)
        {
            return entities;
        }

        public DataSourceResult GetAll([ModelBinder] DataSourceRequest request)
        {
            return GetAll().ToDataSourceResult(request);
        }

        [NonAction]
        public virtual IQueryable<TEntity> GetAll()
        {
            return Include(Context.Set<TEntity>()).AsNoTracking();
        }

        public IHttpActionResult Get(int id)
        {
            var entity = GetById(id);
            if(entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        protected virtual TEntity GetById(int id)
        {
            var entities = Context.Set<TEntity>();
            return Include(entities).SingleOrDefault(e => e.Id == id);
        }

        public IHttpActionResult Put(int id, TEntity entity)
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
            catch(DbUpdateConcurrencyException)
            {
                Context.Entry(entity).State = EntityState.Detached;
                if(!Exists(id))
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

        protected internal OkNegotiatedContentResult<Wrapper> Ok(TEntity entity)
        {
            return Ok(new Wrapper{ Data = new[] { entity }, Total = 1 });
        }

        protected virtual void Modify(TEntity entity)
        {
            Context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        public IHttpActionResult Post(TEntity entity)
        {
            Add(entity);
            Context.SaveChanges();
            return Created(entity);
        }

        protected internal CreatedAtRouteNegotiatedContentResult<Wrapper> Created(TEntity entity)
        {
            return CreatedAtRoute("DefaultApi", new { id = entity.Id }, new Wrapper{ Data = new[] { entity }, Total = 1 });
        }

        protected virtual void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public IHttpActionResult Delete(int id)
        {
            var entity = Context.Set<TEntity>().Find(id);
            if(entity == null)
            {
                return NotFound();
            }
            Delete(entity);
            return Ok();
        }

        protected virtual void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        private bool Exists(int id)
        {
            return Context.Set<TEntity>().Count(e => e.Id == id) > 0;
        }
    }

    public class BaseController : ApiController
    {
        [SetterProperty]
        public DbContext Context { get; set; }

        [SetterProperty]
        public IMediator Mediator { get; set; }

        [NonAction]
        public TResponse Get<TResponse>(IQuery<TResponse> query)
        {
            return Mediator.Get(query);
        }

        [NonAction]
        public TResult Send<TResult>(ICommand<TResult> command)
        {
            return Mediator.Send(command);
        }
    }

    public class Wrapper
    {
        public object[] Data { get; set; }
        public int Total { get; set; }
    }

    public abstract class Entity
    {
        [Key]
        public int Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}