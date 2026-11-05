using DescartaAi.Api.Dominio;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace DescartaAi.Api.Dados;

/// <summary>
/// Carga inicial: categorias, materiais e pontos de Araçatuba vindos do OpenStreetMap
/// (Dados/Carga/pontos-osm-aracatuba.json, © colaboradores do OpenStreetMap, licença ODbL).
/// </summary>
public static class Seed
{
    public static async Task ExecutarAsync(AppDbContext db)
    {
        if (!await db.Categorias.AnyAsync())
        {
            var cats = new Dictionary<string, Categoria>
            {
                ["Óleo e gordura"] = new() { Nome = "Óleo e gordura", Icone = "droplet", Cor = "#C98A00" },
                ["Pilhas e baterias"] = new() { Nome = "Pilhas e baterias", Icone = "battery", Cor = "#B3261E" },
                ["Eletrônicos"] = new() { Nome = "Eletrônicos", Icone = "cpu", Cor = "#2563EB" },
                ["Lâmpadas"] = new() { Nome = "Lâmpadas", Icone = "lightbulb", Cor = "#7C3AED" },
                ["Medicamentos"] = new() { Nome = "Medicamentos", Icone = "pill", Cor = "#0F766E" },
                ["Entulho e volumosos"] = new() { Nome = "Entulho e volumosos", Icone = "hammer", Cor = "#57534E" },
                ["Recicláveis"] = new() { Nome = "Recicláveis", Icone = "recycle", Cor = "#15803D" },
            };
            db.Categorias.AddRange(cats.Values);

            Material M(string cat, string nome, string instr, string? cuidados, bool perigoso, params string[] sin) => new()
            {
                Categoria = cats[cat], Nome = nome, Instrucoes = instr,
                Cuidados = cuidados, Perigoso = perigoso, Sinonimos = sin.Length > 0 ? sin : null
            };

            db.Materiais.AddRange(
                M("Óleo e gordura", "Óleo de cozinha usado",
                    "Deixe esfriar, coloque em uma garrafa PET bem fechada e leve a um ponto de coleta de óleo. Nunca jogue na pia ou no ralo.",
                    "Não misture com água ou detergente.", false, "óleo de fritura", "gordura de cozinha"),
                M("Pilhas e baterias", "Pilha comum e alcalina",
                    "Leve a um ponto de coleta de pilhas, comum em supermercados e lojas de eletrônicos. Não descarte no lixo comum.",
                    "Não perfure nem queime.", true, "pilha"),
                M("Pilhas e baterias", "Bateria de celular e notebook",
                    "Entregue em ponto de coleta de baterias ou na assistência técnica. Baterias estufadas devem ser levadas com cuidado.",
                    "Evite calor e impacto: podem pegar fogo.", true, "bateria de lítio", "bateria de celular"),
                M("Eletrônicos", "Eletrônicos e eletrodomésticos pequenos",
                    "Leve a um ecoponto ou a pontos de coleta de lixo eletrônico. Apague seus dados antes de entregar.",
                    null, true, "celular", "carregador", "cabo", "lixo eletrônico"),
                M("Lâmpadas", "Lâmpada fluorescente",
                    "Entregue em ponto de coleta de lâmpadas, sem quebrar. Contém mercúrio.",
                    "Se quebrar, ventile o ambiente e não aspire os cacos.", true, "lâmpada tubular", "lâmpada compacta"),
                M("Lâmpadas", "Lâmpada de LED",
                    "Leve a um ponto de coleta de lâmpadas ou ecoponto.", null, false, "led"),
                M("Medicamentos", "Medicamentos vencidos",
                    "Leve a farmácias e postos de saúde que recebem medicamentos vencidos. Não jogue no lixo comum nem no vaso sanitário.",
                    "Mantenha na embalagem original.", true, "remédio", "remédio vencido"),
                M("Entulho e volumosos", "Entulho de obra",
                    "Contrate caçamba licenciada ou leve ao ecoponto, dentro do limite de volume aceito.", null, false, "restos de obra"),
                M("Entulho e volumosos", "Móveis e colchões",
                    "Procure o serviço de coleta de volumosos da prefeitura ou leve ao ecoponto.", null, false, "sofá", "colchão", "móvel velho"),
                M("Recicláveis", "Papel, plástico, vidro e metal",
                    "Separe, lave rapidamente e entregue na coleta seletiva ou em cooperativas de reciclagem.", null, false, "reciclável", "garrafa", "lata", "papelão")
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Pontos.AnyAsync())
        {
            var caminho = Path.Combine(AppContext.BaseDirectory, "Dados", "Carga", "pontos-osm-aracatuba.json");
            var itens = System.Text.Json.JsonSerializer.Deserialize<List<PontoCarga>>(
                await File.ReadAllTextAsync(caminho),
                new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)) ?? [];
            var materiais = await db.Materiais.ToDictionaryAsync(m => m.Nome, m => m.Id);
            var f = new GeometryFactory(new PrecisionModel(), 4326);

            db.Pontos.AddRange(itens.Select(i => new PontoDescarte
            {
                Nome = i.Nome, Endereco = i.Endereco, Cidade = i.Cidade, Uf = i.Uf, Cep = i.Cep,
                Localizacao = f.CreatePoint(new Coordinate(i.Lng, i.Lat)),
                Status = StatusPonto.Aprovado,
                AprovadoEm = DateTime.UtcNow,
                Materiais = i.Materiais.Where(materiais.ContainsKey)
                    .Select(n => new PontoMaterial { MaterialId = materiais[n] }).ToList()
            }));
            await db.SaveChangesAsync();
        }
    }

    private record PontoCarga(string OsmId, string Nome, string Endereco, string Cidade, string Uf,
        string? Cep, double Lat, double Lng, string[] Materiais);
}
