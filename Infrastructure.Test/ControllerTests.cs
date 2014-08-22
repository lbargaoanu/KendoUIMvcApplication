using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Infrastructure.Web;
using Ploeh.AutoFixture;
using StructureMap;
using Xunit;
using Xunit.Extensions;

namespace Infrastructure.Test
{
    public abstract class ControllerTests<TController, TContext, TEntity> where TController : CrudController<TContext, TEntity>, new() where TEntity : VersionedEntity where TContext : BaseContext
    {
        private static readonly Func<TEntity, bool> True = e => true;

        [Theory, ContextAutoData]
        public virtual void ShouldDelete(TEntity newEntity, TContext createContext, TContext readContext)
        {
            // arrange
            createContext.AddAndSave(newEntity);
            // act
            var result = new TController().DeleteAndSave(newEntity.Id);
            // assert
            result.AssertIsOk();
            Assert.Null(readContext.Set<TEntity>().Find(newEntity.Id));
        }

        [Fact]
        public virtual void ShouldNotDeleteNotExisting()
        {
            new TController().DeleteAndSave(0).AssertIsNotFound();
        }

        [Theory, ContextAutoData]
        public virtual void ShouldAdd(TEntity newEntity, TContext readContext)
        {
            // act
            var result = new TController().PostAndSave(newEntity);
            // assert
            result.AssertIsCreatedAtRoute(newEntity);
            var entities = readContext.Set<TEntity>();
            entities.Find(newEntity.Id).ShouldBeEquivalentTo(newEntity);
        }

        [Theory, ContextAutoData]
        public virtual void ShouldGetAll(TEntity[] newEntities, TContext createContext, Func<TEntity, bool> where = null)
        {
            // arrange
            createContext.AddAndSave(newEntities);
            // act
            var response = new TController().HandleGetAll();
            // assert
            response.AssertIs<TEntity>(newEntities,where ?? True);
        }

        [Theory, ContextAutoData]
        public virtual void ShouldGetById(TEntity newEntity, TContext createContext)
        {
            // arrange
            createContext.AddAndSave(newEntity);
            // act
            var response = new TController().HandleGetById(newEntity.Id);
            // assert
            response.AssertIsOk(newEntity);
        }

        [Theory, ContextAutoData]
        public virtual void ShouldNotGetByNonExistingId()
        {
            // act
            var response = new TController().HandleGetById(0);
            // assert
            response.AssertIsNotFound();
        }

        [Theory, ContextAutoData]
        public virtual void ShouldModify(TEntity newEntity, TEntity modified, TContext createContext, TContext readContext)
        {
            // arrange
            createContext.AddAndSave(newEntity);
            modified.Id = newEntity.Id;
            modified.RowVersion = newEntity.RowVersion;

            Map(modified, newEntity);

            var controller = new TController();
            // act
            var response = controller.PutAndSave(newEntity);
            // assert
            response.AssertIsOk(newEntity);
            var entities = readContext.Set<TEntity>();
            newEntity = controller.Include(entities).Single(p => p.Id == newEntity.Id);
            newEntity.ShouldBeQuasiEquivalentTo(modified);
        }

        protected virtual void Map(TEntity source, TEntity destination)
        {
            Mapper.Map(source, destination);
        }

        [Theory, ContextAutoData]
        public virtual void ShouldNotModifyId(int id, TEntity modified)
        {
            // act
            var response = new TController().Put(id, modified);
            // assert
            response.AssertIsBadRequest();
        }

        [Theory, ContextAutoData]
        public virtual void ShouldNotModifyNotExisting(TEntity entity)
        {
            // arrange
            entity.Id = 0;
            // act
            var response = new TController().PutAndSave(entity);
            // assert
            response.AssertIsNotFound();
        }

        [Theory, ContextAutoData]
        public virtual void ShouldNotModifyConcurrent(TEntity entity, TContext createContext, TContext modifyContext, byte[] rowVersion)
        {
            // arrange
            createContext.AddAndSave(entity);
            modifyContext.Set<TEntity>().Find(entity.Id).RowVersion = rowVersion;
            modifyContext.SaveChanges();
            // act & assert
            Assert.Throws<DbUpdateConcurrencyException>(()=>new TController().PutAndSave(entity));
        }
    }
}