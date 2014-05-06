using System.Data.Entity.Infrastructure;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using Xunit;
using Xunit.Extensions;

namespace Test.Controllers.Integration
{
    public abstract class ControllerTests<TController, TEntity> where TController : NorthwindController<TEntity>, new() where TEntity : Entity
    {
        [Theory, MyAutoData]
        public virtual void ShouldDelete(TEntity newEntity, ProductServiceContext createContext, ProductServiceContext readContext)
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

        [Theory, MyAutoData]
        public virtual void ShouldAdd(TEntity newEntity, ProductServiceContext readContext)
        {
            // act
            var result = new TController().PostAndSave(newEntity);
            // assert
            result.AssertIsCreatedAtRoute(newEntity);
            var entities = readContext.Set<TEntity>();
            entities.Find(newEntity.Id).ShouldBeEquivalentTo(newEntity);
        }

        [Theory, MyAutoData]
        public virtual void ShouldGetAll(TEntity[] newEntities, ProductServiceContext createContext)
        {
            // arrange
            createContext.AddAndSave(newEntities);
            // act
            var response = new TController().HandleGetAll();
            // assert
            response.Count().Should().BeGreaterOrEqualTo(newEntities.Length, "Se poate sa avem date din alte teste.");
        }

        [Theory, MyAutoData]
        public virtual void ShouldGetById(TEntity newEntity, ProductServiceContext createContext)
        {
            // arrange
            createContext.AddAndSave(newEntity);
            // act
            var response = new TController().HandleGetById(newEntity.Id);
            // assert
            response.AssertIsOk(newEntity);
        }

        [Theory, MyAutoData]
        public virtual void ShouldNotGetByNonExistingId()
        {
            // act
            var response = new TController().HandleGetById(0);
            // assert
            response.AssertIsNotFound();
        }

        [Theory, MyAutoData]
        public virtual void ShouldModify(TEntity newEntity, TEntity modified, ProductServiceContext createContext, ProductServiceContext readContext)
        {
            // arrange
            createContext.AddAndSave(newEntity);
            modified.Id = newEntity.Id;
            modified.RowVersion = newEntity.RowVersion;
            Mapper.Map(modified, newEntity);
            var controller = new TController();
            // act
            var response = controller.PutAndSave(newEntity);
            // assert
            response.AssertIsOk(newEntity);
            var entities = readContext.Set<TEntity>();
            newEntity = controller.Include(entities).Single(p => p.Id == newEntity.Id);
            newEntity.ShouldBeQuasiEquivalentTo(modified);
        }

        [Theory, MyAutoData]
        public virtual void ShouldNotModifyId(int id, TEntity modified)
        {
            // act
            var response = new TController().Put(id, modified);
            // assert
            response.AssertIsBadRequest();
        }

        [Theory, MyAutoData]
        public virtual void ShouldNotModifyNotExisting(TEntity entity)
        {
            // arrange
            entity.Id = 0;
            // act
            var response = new TController().PutAndSave(entity);
            // assert
            response.AssertIsNotFound();
        }

        [Theory, MyAutoData]
        public virtual void ShouldNotModifyConcurrent(TEntity entity, ProductServiceContext createContext, ProductServiceContext modifyContext, byte[] rowVersion)
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