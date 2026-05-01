using System.Linq.Expressions;

namespace CleanArch.Domain.Specifications;

/// <summary>
/// Specification pattern — encapsulates query logic in the domain layer.
/// Supports filtering (Where), ordering, includes, and pagination.
/// Composable via And(), Or(), Not() operators.
/// EF Core compatible — uses parameter replacement instead of Expression.Invoke.
/// </summary>
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderByExpression { get; private set; }
    public Expression<Func<T, object>>? OrderByDescendingExpression { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderByExpression = orderByExpression;
        OrderByDescendingExpression = null;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescendingExpression = orderByDescendingExpression;
        OrderByExpression = null;
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    public bool IsSatisfiedBy(T entity)
    {
        return ToExpression().Compile()(entity);
    }

    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

internal sealed class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    internal AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        
        var parameter = Expression.Parameter(typeof(T));
        var leftBody = new ParameterReplacer(leftExpr.Parameters[0], parameter).Visit(leftExpr.Body);
        var rightBody = new ParameterReplacer(rightExpr.Parameters[0], parameter).Visit(rightExpr.Body);
        
        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftBody!, rightBody!), parameter);
    }
}

internal sealed class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    internal OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        
        var parameter = Expression.Parameter(typeof(T));
        var leftBody = new ParameterReplacer(leftExpr.Parameters[0], parameter).Visit(leftExpr.Body);
        var rightBody = new ParameterReplacer(rightExpr.Parameters[0], parameter).Visit(rightExpr.Body);
        
        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(leftBody!, rightBody!), parameter);
    }
}

internal sealed class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _spec;

    internal NotSpecification(Specification<T> spec)
    {
        _spec = spec;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = _spec.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var body = new ParameterReplacer(expr.Parameters[0], parameter).Visit(expr.Body);
        
        return Expression.Lambda<Func<T, bool>>(
            Expression.Not(body!), parameter);
    }
}

/// <summary>
/// Expression visitor that replaces parameter references.
/// This makes composed expressions EF Core translatable.
/// </summary>
internal sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}
