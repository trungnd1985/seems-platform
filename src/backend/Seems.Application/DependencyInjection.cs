using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Seems.Application.Common.Behaviors;

namespace Seems.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey= "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzg4MDQ4MDAwIiwiaWF0IjoiMTc1NjU2NDI4NSIsImFjY291bnRfaWQiOiIwMTk4ZmI2MzgxMmE3NDkyODRhMTA2NGNhZDQ4YzY3ZiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazN4cDd0NGdibm1qNDI4Mjk4bXk3Nnd5Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.HTHCw2evDz7roEyFJnBtd_E33yUkk0RUOkuZ48HNolkOZHp6O8-2toANWwTQgpn72Ly3ZBmJS3H0ixONpHoc7OZ7TUwU8x4AJwTOBuygPQR4hkWQA4nw7mJ2oY7iTCEWKd_kuLXa5GdhE1l0pD_Pp7E_dWvpRDHrMqfcAHL8Y2E31I4Z7W9o1NfiIo2ROkY-3cnwcaGeO-YtSXIYTEE5wAeX2i__NXlpjd7p91HLfLfCwALHtH0Qm5ErQfo7xUQXHGLab10PvhPzFAHsmBXR9awP7BbEdBRQibHuiy3PozNmflqhxOX5vqcS5GKcP-ZfenPf1FKCw7VryflE7gjOgA";
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);

        return services;
    }
}
