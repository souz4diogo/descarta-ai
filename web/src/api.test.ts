import { describe, expect, it } from "vitest";
import { api, foiCancelada } from "./api";
import { chamadas, falha, simularApi } from "./testes/auxiliares";

describe("api", () => {
  it("usa o endereço da API configurado no ambiente", async () => {
    const fetch = simularApi({ "/categorias": [] });

    await api.categorias();

    expect(String(fetch.mock.calls[0][0])).toBe("http://api.teste/v1/categorias");
  });

  it("omite filtros vazios da consulta", async () => {
    const fetch = simularApi({ "/materiais": [] });

    await api.materiais({ busca: "", categoriaId: undefined });
    await api.materiais({ busca: "óleo de cozinha", categoriaId: 3 });

    expect(chamadas(fetch)).toEqual([
      "/v1/materiais",
      "/v1/materiais?busca=%C3%B3leo+de+cozinha&categoriaId=3",
    ]);
  });

  it("envia coordenadas, raio e material da busca por proximidade", async () => {
    const fetch = simularApi({ "/pontos/proximos": [] });

    await api.proximos({ lat: -21.2, lng: -50.4, raio: 5000, materialId: 7 });

    expect(chamadas(fetch)).toEqual(["/v1/pontos/proximos?lat=-21.2&lng=-50.4&raio=5000&materialId=7"]);
  });

  it("lança erro com o código HTTP quando a API falha", async () => {
    simularApi({ "/materiais": falha(503) });

    await expect(api.material(1)).rejects.toThrow("Erro 503");
  });

  it("identifica requisição cancelada", () => {
    expect(foiCancelada(new DOMException("x", "AbortError"))).toBe(true);
    expect(foiCancelada(new Error("rede"))).toBe(false);
  });
});
