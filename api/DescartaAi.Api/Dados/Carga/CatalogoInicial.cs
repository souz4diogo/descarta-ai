using DescartaAi.Api.Dominio;

namespace DescartaAi.Api.Dados.Carga;

/// <summary>Categorias e materiais com as instruções de descarte da versão 1.</summary>
public static class CatalogoInicial
{
    public static IEnumerable<Categoria> Criar()
    {
        var oleo = Categoria("Óleo e gordura", "droplet");
        var pilhas = Categoria("Pilhas e baterias", "battery");
        var eletronicos = Categoria("Eletrônicos", "cpu");
        var lampadas = Categoria("Lâmpadas", "lightbulb");
        var medicamentos = Categoria("Medicamentos", "pill");
        var entulho = Categoria("Entulho e volumosos", "hammer");
        var reciclaveis = Categoria("Recicláveis", "recycle");

        Adicionar(oleo, "Óleo de cozinha usado",
            "Deixe esfriar, coloque em uma garrafa PET bem fechada e leve a um ponto de coleta de óleo. Nunca jogue na pia ou no ralo.",
            cuidados: "Não misture com água ou detergente.", perigoso: false, "óleo de fritura", "gordura de cozinha");
        Adicionar(pilhas, "Pilha comum e alcalina",
            "Leve a um ponto de coleta de pilhas, comum em supermercados e lojas de eletrônicos. Não descarte no lixo comum.",
            cuidados: "Não perfure nem queime.", perigoso: true, "pilha");
        Adicionar(pilhas, "Bateria de celular e notebook",
            "Entregue em ponto de coleta de baterias ou na assistência técnica. Baterias estufadas devem ser levadas com cuidado.",
            cuidados: "Evite calor e impacto: podem pegar fogo.", perigoso: true, "bateria de lítio", "bateria de celular");
        Adicionar(eletronicos, "Eletrônicos e eletrodomésticos pequenos",
            "Leve a um ecoponto ou a pontos de coleta de lixo eletrônico. Apague seus dados antes de entregar.",
            cuidados: null, perigoso: true, "celular", "carregador", "cabo", "lixo eletrônico");
        Adicionar(lampadas, "Lâmpada fluorescente",
            "Entregue em ponto de coleta de lâmpadas, sem quebrar. Contém mercúrio.",
            cuidados: "Se quebrar, ventile o ambiente e não aspire os cacos.", perigoso: true, "lâmpada tubular", "lâmpada compacta");
        Adicionar(lampadas, "Lâmpada de LED",
            "Leve a um ponto de coleta de lâmpadas ou ecoponto.", cuidados: null, perigoso: false, "led");
        Adicionar(medicamentos, "Medicamentos vencidos",
            "Leve a farmácias e postos de saúde que recebem medicamentos vencidos. Não jogue no lixo comum nem no vaso sanitário.",
            cuidados: "Mantenha na embalagem original.", perigoso: true, "remédio", "remédio vencido");
        Adicionar(entulho, "Entulho de obra",
            "Contrate caçamba licenciada ou leve ao ecoponto, dentro do limite de volume aceito.",
            cuidados: null, perigoso: false, "restos de obra");
        Adicionar(entulho, "Móveis e colchões",
            "Procure o serviço de coleta de volumosos da prefeitura ou leve ao ecoponto.",
            cuidados: null, perigoso: false, "sofá", "colchão", "móvel velho");
        Adicionar(reciclaveis, "Papel, plástico, vidro e metal",
            "Separe, lave rapidamente e entregue na coleta seletiva ou em cooperativas de reciclagem.",
            cuidados: null, perigoso: false, "reciclável", "garrafa", "lata", "papelão");

        return [oleo, pilhas, eletronicos, lampadas, medicamentos, entulho, reciclaveis];
    }

    private static Categoria Categoria(string nome, string icone) => new() { Nome = nome, Icone = icone };

    private static void Adicionar(Categoria categoria, string nome, string instrucoes,
        string? cuidados, bool perigoso, params string[] sinonimos) =>
        categoria.Materiais.Add(new Material
        {
            Nome = nome, Instrucoes = instrucoes, Cuidados = cuidados, Perigoso = perigoso,
            Sinonimos = sinonimos.Length > 0 ? sinonimos : null
        });
}
