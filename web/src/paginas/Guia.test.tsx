import { describe, expect, it } from "vitest";
import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import Guia from "./Guia";
import {
  categorias, chamadas, detalhePilha, falha, materiais, renderizarNaRota, simularApi,
} from "../testes/auxiliares";

const filtrarMateriais = (url: URL) => {
  const busca = url.searchParams.get("busca")?.toLowerCase();
  const categoriaId = Number(url.searchParams.get("categoriaId")) || undefined;
  return materiais.filter(m =>
    (!busca || m.nome.toLowerCase().includes(busca)) && (!categoriaId || m.categoriaId === categoriaId));
};

const api = (extras = {}) => simularApi({
  "/categorias": categorias, "/materiais": filtrarMateriais, "/materiais/20": detalhePilha, ...extras,
});

const itens = () => screen.getAllByRole("button", { expanded: false }).map(b => b.textContent);

describe("Guia", () => {
  it("lista os materiais e marca os perigosos", async () => {
    api();
    renderizarNaRota(<Guia />, "/guia");

    expect(await screen.findByText("Lâmpada fluorescente")).toBeInTheDocument();
    expect(screen.getAllByText("Perigoso")).toHaveLength(2);
  });

  it("busca enquanto o usuário digita e grava o termo na URL", async () => {
    const fetch = api();
    renderizarNaRota(<Guia />, "/guia");
    await screen.findByText("Lâmpada de LED");

    await userEvent.type(screen.getByLabelText("O que você quer descartar?"), "pilha");

    await waitFor(() => expect(screen.getByTestId("url")).toHaveTextContent("/guia?busca=pilha"));
    await waitFor(() => expect(screen.queryByText("Lâmpada de LED")).not.toBeInTheDocument());
    expect(screen.getByText("Pilha comum e alcalina")).toBeInTheDocument();
    // Espera o usuário parar de digitar: não dispara uma consulta por letra.
    expect(chamadas(fetch).filter(u => u.includes("busca=")).length).toBe(1);
  });

  it("abre já filtrado quando a URL traz busca e categoria", async () => {
    const fetch = api();
    renderizarNaRota(<Guia />, "/guia?categoriaId=1&busca=led");

    expect(await screen.findByText("Lâmpada de LED")).toBeInTheDocument();
    expect(screen.getByLabelText("O que você quer descartar?")).toHaveValue("led");
    expect(screen.getByRole("button", { name: "Lâmpadas" })).toHaveAttribute("aria-pressed", "true");
    expect(chamadas(fetch)).toContain("/v1/materiais?busca=led&categoriaId=1");
  });

  it("filtra por categoria ao clicar no filtro", async () => {
    api();
    renderizarNaRota(<Guia />, "/guia");
    await screen.findByText("Pilha comum e alcalina");

    await userEvent.click(await screen.findByRole("button", { name: "Lâmpadas" }));

    await waitFor(() => expect(screen.queryByText("Pilha comum e alcalina")).not.toBeInTheDocument());
    expect(screen.getByTestId("url")).toHaveTextContent("categoriaId=1");
  });

  it("mostra instruções, cuidados e o link para o mapa ao abrir um item", async () => {
    api();
    renderizarNaRota(<Guia />, "/guia");

    await userEvent.click(await screen.findByRole("button", { name: /Pilha comum e alcalina/ }));

    expect(await screen.findByText(detalhePilha.instrucoes)).toBeInTheDocument();
    expect(screen.getByText(detalhePilha.cuidados!)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Onde levar/ })).toHaveAttribute("href", "/mapa?materialId=20");
    expect(screen.getByRole("button", { name: /Pilha comum e alcalina/ })).toHaveAttribute("aria-expanded", "true");
  });

  it("oferece limpar a busca quando nada é encontrado", async () => {
    api();
    renderizarNaRota(<Guia />, "/guia?busca=xyz");

    expect(await screen.findByText(/Nenhum item encontrado para “xyz”/)).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "veja todos os itens" }));

    expect(await screen.findByText("Lâmpada de LED")).toBeInTheDocument();
    expect(screen.getByLabelText("O que você quer descartar?")).toHaveValue("");
  });

  it("mostra erro com opção de tentar de novo quando a API falha", async () => {
    let falhar = true;
    api({ "/materiais": (url: URL) => (falhar ? falha() : filtrarMateriais(url)) });
    renderizarNaRota(<Guia />, "/guia");

    const alerta = await screen.findByRole("alert");
    falhar = false;
    await userEvent.click(within(alerta).getByRole("button", { name: /Tentar de novo/ }));

    expect(await screen.findByText("Lâmpada de LED")).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("erro ao abrir um item não esconde a lista", async () => {
    api({ "/materiais/20": falha() });
    renderizarNaRota(<Guia />, "/guia");

    await userEvent.click(await screen.findByRole("button", { name: /Pilha comum e alcalina/ }));

    expect(await screen.findByText(/Não foi possível carregar as instruções/)).toBeInTheDocument();
    expect(screen.getByText("Lâmpada de LED")).toBeInTheDocument();
    expect(itens().length).toBeGreaterThan(0);
  });
});
