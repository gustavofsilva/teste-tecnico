using System.ComponentModel.DataAnnotations;

namespace UserProfile.Api.Endpoints;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null) return await next(context);
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, new ValidationContext(model), results, true)) return await next(context);
        var errors = results.SelectMany(r => r.MemberNames.DefaultIfEmpty("request").Select(name => (name, r.ErrorMessage ?? "Valor inválido.")))
            .GroupBy(x => char.ToLowerInvariant(x.name[0]) + x.name[1..])
            .ToDictionary(g => g.Key, g => g.Select(x => x.Item2).ToArray());
        return Results.ValidationProblem(errors);
    }
}
