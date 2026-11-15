using MassTransit;
using Wrench.Contracts;

namespace wrench.billing.infra.Mensageria;

/// <summary>
/// Nomes da topologia SNS/SQS, todos configuráveis em <c>Messaging:*</c>. Os valores padrão são a SUPOSIÇÃO
/// do Billing a partir do plano da Fase 4 (um tópico por publicador, uma fila com DLQ por consumidor) e
/// devem ser confirmados com quem provisiona a mensageria.
/// </summary>
public class MensageriaOptions
{
    public const string Secao = "Messaging";

    /// <summary>Tópico SNS onde o Billing publica TODOS os seus eventos (um tópico por publicador).</summary>
    public const string TopicoEventosPadrao = "wrench-billing-events";

    /// <summary>Tópico SNS do qual o Billing consome os comandos (publicados pelo orquestrador no OS Service).</summary>
    public const string TopicoComandosPadrao = "wrench-os-events";

    /// <summary>
    /// Uma fila SQS por comando, provisionadas pelo Terraform do infra-k8s. Cada assinatura filtra
    /// um unico valor do atributo <c>tipo</c>, o que permite entregar o JSON cru do contrato sem
    /// envelope: a fila ja determina o tipo da mensagem.
    /// </summary>
    public const string FilaGerarOrcamentoPadrao = "wrench-billing-gerar-orcamento";

    public const string FilaCancelarOrcamentoPadrao = "wrench-billing-cancelar-orcamento";

    public const string FilaEstornarPagamentoPadrao = "wrench-billing-estornar-pagamento";

    public string TopicoEventos { get; set; } = TopicoEventosPadrao;
    public string TopicoComandos { get; set; } = TopicoComandosPadrao;
    public string FilaGerarOrcamento { get; set; } = FilaGerarOrcamentoPadrao;
    public string FilaCancelarOrcamento { get; set; } = FilaCancelarOrcamentoPadrao;
    public string FilaEstornarPagamento { get; set; } = FilaEstornarPagamentoPadrao;
}

public static class MensageriaTopologia
{
    /// <summary>
    /// Aponta os eventos do Billing para um único tópico e os comandos consumidos para o tópico do orquestrador.
    /// Valor vazio mantém o padrão do MassTransit (um tópico por tipo de mensagem).
    /// A outra ponta precisa usar os MESMOS nomes (idealmente definidos no pacote Wrench.Contracts).
    /// </summary>
    public static void Aplicar(IAmazonSqsBusFactoryConfigurator cfg, MensageriaOptions opcoes)
    {
        if (!string.IsNullOrWhiteSpace(opcoes.TopicoEventos))
        {
            var t = opcoes.TopicoEventos.Trim();
            Topico<OrcamentoCriado>(cfg, t);
            Topico<OrcamentoFalhou>(cfg, t);
            Topico<OrcamentoAprovado>(cfg, t);
            Topico<OrcamentoRecusado>(cfg, t);
            Topico<PagamentoAprovado>(cfg, t);
            Topico<PagamentoFalhou>(cfg, t);
            Topico<PagamentoEstornado>(cfg, t);
            Topico<EstornoFalhou>(cfg, t);
        }

        if (!string.IsNullOrWhiteSpace(opcoes.TopicoComandos))
        {
            var t = opcoes.TopicoComandos.Trim();
            Topico<GerarOrcamento>(cfg, t);
            Topico<CancelarOrcamento>(cfg, t);
            Topico<EstornarPagamento>(cfg, t);
        }
    }

    private static void Topico<T>(IAmazonSqsBusFactoryConfigurator cfg, string nome) where T : class
        => cfg.Message<T>(m => m.SetEntityName(nome));
}
