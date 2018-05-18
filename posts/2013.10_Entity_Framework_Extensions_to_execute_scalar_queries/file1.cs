public static class DbContextScalarQueryExtensions {

    public static object GetInternalContext (this DbContext context) {
        var provProp = context.GetType ().GetProperty ("InternalContext", BindingFlags.Instance | BindingFlags.NonPublic);
        return provProp.GetValue (context, null);
    }

    public static IQueryProvider GetRootObjectQueryProvider (this DbContext context) {
        var ctx = (context as IObjectContextAdapter).ObjectContext;

        var provProp = ctx.GetType ().GetProperty ("QueryProvider", BindingFlags.Instance | BindingFlags.NonPublic);
        var provider = provProp.GetValue (ctx, null) as IQueryProvider; //ObjectQueryProvider
        return provider;
    }

    static IQueryProvider CreateRootDbQueryProvider (DbContext context) {
        var dbQueryProviderType = Type.GetType ("System.Data.Entity.Internal.Linq.DbQueryProvider, EntityFramework");
        var dbQueryProvider = Activator.CreateInstance (dbQueryProviderType, context.GetInternalContext (), context.GetRootObjectQueryProvider ())
        as IQueryProvider;
        return dbQueryProvider as IQueryProvider;
    }

    static Expression<Func<TResult>> RefactorExpression<TArgument, TResult> (Expression<Func<TArgument, TResult>> expOriginal, TArgument arg) {
        Expression<Func<TArgument>> expTemplate = () => arg;

        var arg2 = expTemplate.Body;
        var newBody = new Rewriter (expOriginal.Parameters[0], arg2).Visit (expOriginal.Body);
        return Expression.Lambda<Func<TResult>> (newBody);
    }

    class Rewriter : ExpressionVisitor {
        private readonly Expression candidate_;
        private readonly Expression replacement_;

        public Rewriter (Expression candidate, Expression replacement) {
            candidate_ = candidate;
            replacement_ = replacement;
        }

        public override Expression Visit (Expression node) {
            return node == candidate_ ? replacement_ : base.Visit (node);
        }
    }

    public static IQueryable<TResult> CreateScalarQuery<TDbContext, TResult> (this TDbContext context, Expression<Func<TDbContext, TResult>> expression)
    where TDbContext : DbContext {
        var resExp = RefactorExpression<TDbContext, TResult> (expression, (TDbContext) context);
        var realExp = Expression.Call (
            method : GetMethodInfo (() => Queryable.Select<int, TResult> (null, (Expression<Func<int, TResult>>) null)),
            arg0 : Expression.Call (
                method : GetMethodInfo (() => Queryable.AsQueryable<int> (null)),
                arg0 : Expression.NewArrayInit (typeof (int), Expression.Constant (1))),
            arg1 : Expression.Lambda (body: resExp.Body, parameters: new [] { Expression.Parameter (typeof (int)) }));

        return CreateRootDbQueryProvider (context).CreateQuery<TResult> (realExp);
    }

    public static IQueryable<TResult> SelectScalar<TElement, TResult> (this IEnumerable<TElement> q, Expression<Func<IQueryable<TElement>, TResult>> expression) {
        IQueryProvider queryProvider = (q as IQueryable).Provider;

        var expr = RefactorExpression (expression, (IQueryable<TElement>) q);

        var realExp = Expression.Call (
            method : GetMethodInfo (() => Queryable.Select<int, TResult> (null, (Expression<Func<int, TResult>>) null)),
            arg0 : Expression.Call (
                method : GetMethodInfo (() => Queryable.AsQueryable<int> (null)),
                arg0 : Expression.NewArrayInit (typeof (int), Expression.Constant (1))),
            arg1 : Expression.Lambda (body: expr.Body, parameters: new [] { Expression.Parameter (typeof (int)) }));

        return queryProvider.CreateQuery<TResult> (realExp);
    }

    static MethodInfo GetMethodInfo (Expression<Action> expression) {
        return ((MethodCallExpression) expression.Body).Method;
    }

}