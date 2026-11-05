import { describe, expect, it } from "vitest";
import { act, renderHook, waitFor } from "@testing-library/react";
import { formatarDistancia, numeroDaUrl, plural, urlRota, useConsulta } from "./util";

describe("formatarDistancia", () => {
  it.each([
    [0, "0 m"],
    [850, "850 m"],
    [999.4, "999 m"],
    [1000, "1,0 km"],
    [1534, "1,5 km"],
    [12060, "12,1 km"],
  ])("%d metros → %s", (metros, esperado) => {
    expect(formatarDistancia(metros)).toBe(esperado);
  });
});

describe("plural", () => {
  it("usa o singular só para 1", () => {
    expect(plural(1, "ponto", "pontos")).toBe("1 ponto");
    expect(plural(0, "ponto", "pontos")).toBe("0 pontos");
    expect(plural(2, "ponto", "pontos")).toBe("2 pontos");
  });
});

describe("numeroDaUrl", () => {
  const params = new URLSearchParams("ok=5&zero=0&negativo=-3&texto=abc&vazio=");

  it("lê números positivos", () => expect(numeroDaUrl(params, "ok")).toBe(5));

  it.each(["zero", "negativo", "texto", "vazio", "inexistente"])("ignora %s", chave => {
    expect(numeroDaUrl(params, chave)).toBeUndefined();
  });
});

describe("urlRota", () => {
  it("monta o link de rota do Google Maps até o destino", () => {
    expect(urlRota(-21.2, -50.43)).toBe("https://www.google.com/maps/dir/?api=1&destination=-21.2,-50.43");
  });
});

describe("useConsulta", () => {
  it("entrega os dados quando a consulta termina", async () => {
    const { result } = renderHook(() => useConsulta(async () => "ok", []));

    expect(result.current.carregando).toBe(true);
    await waitFor(() => expect(result.current.dados).toBe("ok"));
    expect(result.current.carregando).toBe(false);
  });

  it("marca erro e permite tentar de novo", async () => {
    let tentativas = 0;
    const { result } = renderHook(() =>
      useConsulta(async () => { if (++tentativas === 1) throw new Error("falhou"); return "recuperado"; }, []));

    await waitFor(() => expect(result.current.erro).toBe(true));
    act(() => result.current.tentarDeNovo());

    await waitFor(() => expect(result.current.dados).toBe("recuperado"));
    expect(result.current.erro).toBe(false);
  });

  it("descarta resposta antiga que chega depois da nova (fora de ordem)", async () => {
    const pendentes: Record<string, (v: string) => void> = {};
    const { result, rerender } = renderHook(({ termo }) =>
      useConsulta(sinal => new Promise<string>((resolver, rejeitar) => {
        pendentes[termo] = resolver;
        sinal.addEventListener("abort", () => rejeitar(new DOMException("cancelada", "AbortError")));
      }), [termo]), { initialProps: { termo: "lam" } });

    rerender({ termo: "lampada" });
    await act(async () => pendentes["lampada"]("resultado novo"));
    await act(async () => pendentes["lam"]("resultado antigo"));

    expect(result.current.dados).toBe("resultado novo");
    expect(result.current.erro).toBe(false);
  });

  it("não trata cancelamento como erro", async () => {
    const { result, rerender } = renderHook(({ id }) =>
      useConsulta(sinal => new Promise<number>((resolver, rejeitar) => {
        sinal.addEventListener("abort", () => rejeitar(new DOMException("cancelada", "AbortError")));
        if (id === 2) resolver(2);
      }), [id]), { initialProps: { id: 1 } });

    rerender({ id: 2 });

    await waitFor(() => expect(result.current.dados).toBe(2));
    expect(result.current.erro).toBe(false);
  });
});
