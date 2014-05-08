using System;
using System.Data.Entity;
using System.Web.Http.Dependencies;
using StructureMap.Attributes;

namespace KendoUIMvcApplication
{
    public class Mediator : IMediator
    {
        public IDependencyScope DependencyResolver { get; private set; }

        public static Mediator Create(IDependencyScope dependencyScope)
        {
            return new Mediator(null) { DependencyResolver = dependencyScope };
        }

        public Mediator(IDependencyResolver dependencyResolver)
        {
            this.DependencyResolver = dependencyResolver;
        }

        public TResult Get<TResult>(IQuery<TResult> request)
        {
            return RunHandler<TResult>(typeof(IQueryHandler<,>), request);
        }

        private TResult RunHandler<TResult>(Type genericHandlerType, object request)
        {
            var handlerType = genericHandlerType.MakeGenericType(request.GetType(), typeof(TResult));
            dynamic handler = DependencyResolver.GetService(handlerType);
            if(handler == null)
            {
                throw new InvalidOperationException("Cannot find handler for " + request);
            }
            return (TResult)handler.Execute((dynamic)request);
        }

        public TResult Send<TResult>(ICommand<TResult> command)
        {
            return RunHandler<TResult>(typeof(ICommandHandler<,>), command);
        }
    }

    public struct Void
    {
        public static readonly Void Default = new Void();
    }

    public interface IMediator
    {
        TResult Get<TResult>(IQuery<TResult> query);
        TResult Send<TResult>(ICommand<TResult> command);
    }

    public interface IQuery<out TResponse> { }

    public interface ICommand<out TResult> { }

    public interface ICommand : ICommand<Void> { }

    public interface IQueryHandler<in TQuery, out TResponse> where TQuery : IQuery<TResponse>
    {
        TResponse Execute(TQuery query);
    }

    public abstract class QueryHandler<TContext, TQuery, TResponse> : IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse> where TContext : DbContext
    {
        [SetterProperty]
        public TContext Context { get; set; }

        public TResponse Execute(TQuery query)
        {
            return Handle(query);
        }

        public abstract TResponse Handle(TQuery query);
    }

    public interface ICommandHandler<in TCommand, out TResult> where TCommand : ICommand<TResult>
    {
        TResult Execute(TCommand command);
    }

    public abstract class CommandHandler<TContext, TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult> where TContext : DbContext
    {
        [SetterProperty]
        public TContext Context { get; set; }

        public TResult Execute(TCommand query)
        {
            return Handle(query);
        }

        public abstract TResult Handle(TCommand command);
    }

    public abstract class CommandHandler<TContext, TCommand> : ICommandHandler<TCommand, Void> where TCommand : ICommand where TContext : DbContext
    {
        public Void Execute(TCommand command)
        {
            Handle(command);
            return Void.Default;
        }

        protected void SetRowVersion<TEntity>(TEntity source, TEntity destination) where TEntity : Entity
        {
            destination.SetRowVersion(source, Context);
        }

        [SetterProperty]
        public TContext Context { get; set; }

        public abstract void Handle(TCommand command);
    }
}