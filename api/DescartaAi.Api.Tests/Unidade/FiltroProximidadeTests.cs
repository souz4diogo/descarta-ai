using DescartaAi.Api.Modulos.Pontos;

namespace DescartaAi.Api.Tests.Unidade;

public class FiltroProximidadeTests
{
    private const double Lat = -21.2089, Lng = -50.4329;

    [Fact]
    public void Filtro_valido_nao_tem_erros()
    {
        var filtro = new FiltroProximidade(Lat, Lng, 5000, MaterialId: 1);

        Assert.Empty(filtro.Validar());
    }

    [Fact]
    public void Sem_raio_usa_o_padrao_de_10_km()
    {
        var filtro = new FiltroProximidade(Lat, Lng, Raio: null, MaterialId: null);

        Assert.Empty(filtro.Validar());
        Assert.Equal(10_000, filtro.RaioEfetivo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void Latitude_ausente_ou_fora_do_intervalo_e_invalida(double? lat)
    {
        var erros = new FiltroProximidade(lat, Lng, null, null).Validar();

        Assert.Equal(["lat"], erros.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void Longitude_ausente_ou_fora_do_intervalo_e_invalida(double? lng)
    {
        var erros = new FiltroProximidade(Lat, lng, null, null).Validar();

        Assert.Equal(["lng"], erros.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(50_001)]
    public void Raio_fora_do_intervalo_e_invalido(double raio)
    {
        var erros = new FiltroProximidade(Lat, Lng, raio, null).Validar();

        Assert.Equal(["raio"], erros.Keys);
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void Limites_exatos_das_coordenadas_sao_aceitos(double lat, double lng)
    {
        Assert.Empty(new FiltroProximidade(lat, lng, FiltroProximidade.RaioMaximo, null).Validar());
    }

    [Fact]
    public void Varios_erros_sao_informados_juntos()
    {
        var erros = new FiltroProximidade(null, null, 0, null).Validar();

        Assert.Equal(3, erros.Count);
    }
}
