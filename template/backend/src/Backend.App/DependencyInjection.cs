using Cortex.Mediator.Behaviors.FluentValidation.DependencyInjection;
using Cortex.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.App;

public static class DependencyInjection
{
    public static IServiceCollection AddApp(this IServiceCollection services, params Type[] additionalHandlerMarkers)
    {
        Type[] markers = [typeof(AppAssemblyMarker), .. additionalHandlerMarkers];
        services.AddFluentValidationValidators(markers);
        services.AddCortexMediator(markers, options => options.AddFluentValidationBehaviors());
        return services;
    }
}
