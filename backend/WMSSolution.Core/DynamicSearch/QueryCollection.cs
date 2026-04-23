using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace WMSSolution.Core.DynamicSearch
{
    /// <summary>
    /// Dynamic Query
    /// </summary>
    public class QueryCollection : Collection<SearchObject>
    {
        /// <summary>
        /// Expression
        /// </summary>
        /// <typeparam name="T">entity</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        /// <summary>
        /// Expression
        /// </summary>
        /// <typeparam name="T">entity</typeparam>
        /// <param name="condition">condition</param>
        /// <returns></returns>
        public Expression<Func<T, bool>> AsExpression<T>(Condition? condition = Condition.AndAlso) where T : class
        {
            if (this.Count == 0)
            {
                return True<T>();
            }
            Type targetType = typeof(T);
            TypeInfo typeInfo = targetType.GetTypeInfo();
            var parameter = Expression.Parameter(targetType, "m");
            Expression expression = null;
            Func<Expression, Expression, Expression> Append = (exp1, exp2) =>
            {
                if (exp1 == null)
                {
                    return exp2;
                }
                return (condition ?? Condition.OrElse) == Condition.OrElse ? Expression.OrElse(exp1, exp2) : Expression.AndAlso(exp1, exp2);
            };
            foreach (var item in this)
            {
                var property = typeInfo.GetProperty(item.Name);
                if (property == null ||
                    !property.CanRead ||
                    (item.Text.Trim().Length == 0))
                {
                    continue;
                }
                if (item.Text.Length == 0)
                {
                    item.Text = item.Value.ToString();
                }
                Type realType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (item.Text.Length > 0)
                {
                    if (item.Type.ToUpper().Equals("DATETIMEPICKER")
                        && (item.Operator == Operators.LessThanOrEqual || item.Operator == Operators.LessThan))
                    {
                        item.Text = Convert.ToDateTime(item.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                    }
                    item.Value = realType.IsEnum
                        ? Enum.Parse(realType, item.Text)
                        : Convert.ChangeType(item.Text, realType);
                }
                Expression<Func<object>> valueLamba = () => item.Value;
                switch (item.Operator)
                {
                    case Operators.Equal:
                        {
                            expression = Append(expression, Expression.Equal(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.GreaterThan:
                        {
                            expression = Append(expression, Expression.GreaterThan(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.GreaterThanOrEqual:
                        {
                            expression = Append(expression, Expression.GreaterThanOrEqual(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.LessThan:
                        {
                            expression = Append(expression, Expression.LessThan(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.LessThanOrEqual:
                        {
                            expression = Append(expression, Expression.LessThanOrEqual(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.Contains:
                        {
                            var propertyExp = Expression.Property(parameter, item.Name);
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, propertyExp));
                            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });

                            // Prepare Lowercase Property Expression
                            var propertyToLower = Expression.Call(propertyExp, toLowerMethod);

                            // Prepare Lowercase Value
                            var lowerValue = item.Value?.ToString()?.ToLower() ?? "";
                            var constantLower = Expression.Constant(lowerValue, typeof(string));

                            if (containsMethod is not null)
                            {
                                var contains = Expression.Call(propertyToLower, containsMethod, constantLower);
                                expression = Append(expression, Expression.AndAlso(nullCheck, contains));
                            }
                            break;
                        }
                }
            }
            if (expression == null)
            {
                return null;
            }

            return ((Expression<Func<T, bool>>)Expression.Lambda(expression, parameter));
        }


        /// <summary>
        ///  logic Grouped Expression
        /// </summary>
        public Expression<Func<T, bool>> AsGroupedExpression<T>(Condition outerCondition = Condition.AndAlso) where T : class
        {
            if (Count == 0)
            {
                return True<T>();
            }

            var targetType = typeof(T);
            var parameter = Expression.Parameter(targetType, "m");
            var typeInfo = targetType.GetTypeInfo();

            Expression? finalExpression = null;

            Condition innerCondition = outerCondition == Condition.AndAlso ? Condition.OrElse : Condition.AndAlso;


            var simpleItems = this.Where(x => string.IsNullOrEmpty(x.Group));
            foreach (var item in simpleItems)
            {
                var property = typeInfo.GetProperty(item.Name);
                if (property is null || !property.CanRead) continue;
                if (string.IsNullOrWhiteSpace(item.Text) && item.Value is null) continue;

                if (item.Text.Length == 0 && item.Value is not null)
                    item.Text = item?.Value?.ToString().ToLower() ?? string.Empty;

                Type realType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (item?.Text.Length > 0)
                {
                    if (string.Equals(item.Type, "DATETIMEPICKER", StringComparison.OrdinalIgnoreCase)
                        && (item.Operator == Operators.LessThanOrEqual || item.Operator == Operators.LessThan))
                    {
                        if (DateTime.TryParse(item.Text, out var dateVal))
                            item.Text = dateVal.ToString("yyyy-MM-dd") + " 23:59:59";
                    }
                    try { item.Value = Convert.ChangeType(item.Text, realType); } catch { continue; }
                }

                Expression constant = Expression.Constant(item.Value, property.PropertyType);
                Expression propertyExp = Expression.Property(parameter, item.Name);
                Expression? componentExp = null;

                switch (item.Operator)
                {
                    case Operators.Equal: componentExp = Expression.Equal(propertyExp, constant); break;
                    case Operators.GreaterThan: componentExp = Expression.GreaterThan(propertyExp, constant); break;
                    case Operators.GreaterThanOrEqual: componentExp = Expression.GreaterThanOrEqual(propertyExp, constant); break;
                    case Operators.LessThan: componentExp = Expression.LessThan(propertyExp, constant); break;
                    case Operators.LessThanOrEqual: componentExp = Expression.LessThanOrEqual(propertyExp, constant); break;
                    case Operators.Contains:
                        var nullCheck = Expression.Not(Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, propertyExp));
                        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);

                        // Prepare Lowercase Property Expression
                        var propertyToLower = Expression.Call(propertyExp, toLowerMethod);

                        // Prepare Lowercase Constant Value
                        var lowerValue = item.Value?.ToString()?.ToLower() ?? "";
                        var constantLower = Expression.Constant(lowerValue, typeof(string));

                        if (containsMethod is not null)
                            componentExp = Expression.AndAlso(nullCheck, Expression.Call(propertyToLower, containsMethod, constantLower));
                        break;
                }


                if (componentExp is not null)
                {
                    finalExpression = finalExpression is null
                        ? componentExp
                        : (outerCondition == Condition.OrElse
                            ? Expression.OrElse(finalExpression, componentExp)
                            : Expression.AndAlso(finalExpression, componentExp));
                }
            }

            var groupedItems = this.Where(x => !string.IsNullOrEmpty(x.Group)).GroupBy(x => x.Group);
            foreach (var group in groupedItems)
            {
                Expression? groupExpression = null;

                foreach (var item in group)
                {
                    var property = typeInfo.GetProperty(item.Name);
                    if (property is null || !property.CanRead) continue;
                    if (string.IsNullOrWhiteSpace(item.Text) && item.Value is null) continue;

                    if (item.Text.Length == 0 && item.Value is not null)
                        item.Text = item.Value.ToString() ?? string.Empty;

                    Type realType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    if (item.Text.Length > 0)
                    {
                        if (string.Equals(item.Type, "DATETIMEPICKER", StringComparison.OrdinalIgnoreCase)
                            && (item.Operator == Operators.LessThanOrEqual || item.Operator == Operators.LessThan))
                        {
                            if (DateTime.TryParse(item.Text, out var dateVal))
                                item.Text = dateVal.ToString("yyyy-MM-dd") + " 23:59:59";
                        }
                        try
                        {
                            item.Value = realType.IsEnum
                                ? Enum.Parse(realType, item.Text)
                                : Convert.ChangeType(item.Text, realType);
                        }
                        catch { continue; }
                    }

                    Expression constant = Expression.Constant(item.Value, property.PropertyType);
                    Expression propertyExp = Expression.Property(parameter, item.Name);
                    Expression? componentExp = null;

                    switch (item.Operator)
                    {
                        case Operators.Equal: componentExp = Expression.Equal(propertyExp, constant); break;
                        case Operators.GreaterThan: componentExp = Expression.GreaterThan(propertyExp, constant); break;
                        case Operators.GreaterThanOrEqual: componentExp = Expression.GreaterThanOrEqual(propertyExp, constant); break;
                        case Operators.LessThan: componentExp = Expression.LessThan(propertyExp, constant); break;
                        case Operators.LessThanOrEqual: componentExp = Expression.LessThanOrEqual(propertyExp, constant); break;
                        case Operators.Contains:
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, propertyExp));
                            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });

                            // Prepare Lowercase Property Expression
                            var propertyToLower = Expression.Call(propertyExp, toLowerMethod);

                            // Prepare Lowercase Constant Value
                            var lowerValue = item.Value?.ToString()?.ToLower() ?? "";
                            var constantLower = Expression.Constant(lowerValue, typeof(string));

                            if (containsMethod is not null)
                                componentExp = Expression.AndAlso(nullCheck, Expression.Call(propertyToLower, containsMethod, constantLower));
                            break;
                    }

                    // Nối vào Group Expression bằng INNER CONDITION
                    if (componentExp is not null)
                    {
                        groupExpression = groupExpression is null
                            ? componentExp
                            : (innerCondition == Condition.OrElse
                                ? Expression.OrElse(groupExpression, componentExp)
                                : Expression.AndAlso(groupExpression, componentExp));
                    }
                }

                if (groupExpression is not null)
                {
                    finalExpression = finalExpression is null
                        ? groupExpression
                        : (outerCondition == Condition.OrElse
                            ? Expression.OrElse(finalExpression, groupExpression)
                            : Expression.AndAlso(finalExpression, groupExpression));
                }
            }

            if (finalExpression is null) return True<T>();

            return Expression.Lambda<Func<T, bool>>(finalExpression, parameter);
        }
    }
}
