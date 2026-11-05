import { describe, expect, it } from "vitest";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import App from "./App";
import { categorias, simularApi } from "./testes/auxiliares";

const abrir = (url: string) => {
  simularApi({ "/categorias": categorias, "/materiais": [] });
  return render(<MemoryRouter initialEntries={[url]}><App /></MemoryRouter>);
};

describe("App", () => {
  it("tem link para pular direto ao conteúdo (acessibilidade)", () => {
    abrir("/");
    expect(screen.getByRole("link", { name: "Pular para o conteúdo" })).toHaveAttribute("href", "#conteudo");
  });

  it("marca a página atual no menu", () => {
    abrir("/guia");
    expect(screen.getByRole("link", { name: "Guia" })).toHaveAttribute("aria-current", "page");
    expect(screen.getByRole("link", { name: "Mapa" })).not.toHaveAttribute("aria-current");
  });

  it("rodapé leva às páginas institucionais", async () => {
    abrir("/");
    const rodape = screen.getByRole("navigation", { name: "Institucional" });

    for (const [link, titulo] of [["Sobre", "Sobre o Descarta Aí"], ["Privacidade", "Política de Privacidade"], ["Termos de uso", "Termos de Uso"]]) {
      await userEvent.click(within(rodape).getByRole("link", { name: link }));
      expect(screen.getByRole("heading", { level: 1, name: titulo })).toBeInTheDocument();
    }
  });

  it("mostra página de não encontrado para endereços inexistentes", () => {
    abrir("/pagina-que-nao-existe");
    expect(screen.getByRole("heading", { name: "Página não encontrada" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Voltar ao início" })).toHaveAttribute("href", "/");
  });
});
