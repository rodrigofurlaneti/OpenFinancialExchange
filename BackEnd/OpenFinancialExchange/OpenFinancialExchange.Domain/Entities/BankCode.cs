using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

/// <summary>Tabela de referência BACEN — códigos de compensação bancária.</summary>
public sealed class BankCode : Entity
{
    private BankCode() : base(0) { }

    /// <summary>Código de compensação BACEN (ex: 237 = Bradesco).</summary>
    public int CodigoCompensacao { get; private set; }

    public string Cnpj { get; private set; } = string.Empty;

    public string NomeInstituicao { get; private set; } = string.Empty;

    public string Segmento { get; private set; } = string.Empty;

    public static BankCode Create(int codigoCompensacao, string cnpj, string nomeInstituicao, string segmento)
        => new()
        {
            Id = codigoCompensacao,
            CodigoCompensacao = codigoCompensacao,
            Cnpj = cnpj.Trim(),
            NomeInstituicao = nomeInstituicao.Trim(),
            Segmento = segmento.Trim(),
        };
}
