using MassTransit;

namespace wrench.billing.application.Extensions;

public static class PublishExtensions
{
    public const string HeaderCorrelationId = "correlationId";

    /// <summary>
    /// Atributo que o filter policy das assinaturas SNS avalia. Sem ele a mensagem e descartada
    /// antes de chegar na fila do consumidor.
    /// </summary>
    public const string HeaderTipo = "tipo";

    /// <summary>
    /// Publica a mensagem propagando o <c>correlationId</c> da Saga. Ele vai sempre no header
    /// <c>correlationId</c> (texto livre, como no execucao-service) e, quando for um GUID,
    /// também como CorrelationId do transporte do MassTransit.
    /// </summary>
    public static Task PublishComCorrelacao<T>(
        this IPublishEndpoint publisher, T mensagem, string correlationId, CancellationToken ct)
        where T : class
        => publisher.Publish(mensagem, ctx =>
        {
            if (Guid.TryParse(correlationId, out var guid))
                ctx.CorrelationId = guid;

            if (!string.IsNullOrWhiteSpace(correlationId))
                ctx.Headers.Set(HeaderCorrelationId, correlationId);

            ctx.Headers.Set(HeaderTipo, typeof(T).Name);
        }, ct);
}