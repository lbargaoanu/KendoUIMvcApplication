using System.Data.Entity.Infrastructure;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using KendoUIMvcApplication;
using Xunit;

namespace Test.Controllers.Integration
{
    public abstract class ControllerTests<TController, TEntity> where TController : CrudController<TEntity>, new() where TEntity : Entity, new()
    {
        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldDelete(ProductServiceContext createContext, ProductServiceContext readContext)
        {
            // arrange
            var entity = createContext.AddAndSave(new TEntity());
            // act
            var result = new TController().DeleteAndSave(entity.Id);
            // assert
            result.AssertIsOk(entity);
            Assert.Null(readContext.Set<TEntity>().Find(entity.Id));
        }

        [RepeatFact(100)]
        public virtual void ShouldNotDeleteNotExisting()
        {
            new TController().DeleteAndSave(0).AssertIsNotFound();
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldAdd(ProductServiceContext readContext)
        {
            // arrange
            var entity = new TEntity();
            // act
            var result = new TController().PostAndSave(entity);
            // assert
            result.AssertIsCreatedAtRoute(entity);
            var entities = readContext.Set<TEntity>();
            entities.Find(entity.Id).ShouldBeEquivalentTo(entity);
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldGetAll(TEntity[] newEntities, ProductServiceContext createContext)
        {
            // arrange
            createContext.AddAndSave(newEntities);
            // act
            var response = new TController().HandleGetAll();
            // assert
            response.Count().Should().BeGreaterOrEqualTo(newEntities.Length, "Se poate sa avem date din alte teste.");
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldGetById(ProductServiceContext createContext)
        {
            // arrange
            var entity = createContext.AddAndSave(new TEntity());
            // act
            var response = new TController().HandleGetById(entity.Id);
            // assert
            response.AssertIsOk(entity);
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldNotGetByNonExistingId()
        {
            // act
            var response = new TController().HandleGetById(0);
            // assert
            response.AssertIsNotFound();
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldModify(TEntity modified, ProductServiceContext createContext, ProductServiceContext readContext)
        {
            // arrange
            var entity = createContext.AddAndSave(new TEntity());
            modified.Id = entity.Id;
            modified.RowVersion = entity.RowVersion;
            Mapper.Map(modified, entity);
            var controller = new TController();
            // act
            var response = controller.PutAndSave(entity);
            // assert
            response.AssertIsOk(entity);
            var entities = readContext.Set<TEntity>();
            entity = controller.Include(entities).Single(p => p.Id == entity.Id);
            entity.ShouldBeQuasiEquivalentTo(modified);
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldNotModifyId(int id, TEntity modified)
        {
            // act
            var response = new TController().Put(id, modified);
            // assert
            response.AssertIsBadRequest();
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldNotModifyNotExisting()
        {
            // act
            var response = new TController().PutAndSave(new TEntity());
            // assert
            response.AssertIsNotFound();
        }

        [RepeatTheory(100), MyAutoData]
        public virtual void ShouldNotModifyConcurrent(ProductServiceContext createContext, ProductServiceContext modifyContext)
        {
            // arrange
            var entity = createContext.AddAndSave(new TEntity());
            modifyContext.Set<TEntity>().Find(entity.Id).RowVersion = new byte[] { 1, 2, 3 };
            modifyContext.SaveChanges();
            // act & assert
            Assert.Throws<DbUpdateConcurrencyException>(()=>new TController().PutAndSave(entity));
        }
    }
}