import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { Privacidade, Sobre, Termos } from "./Institucional";

const abrir = (pagina: React.ReactElement) => render(<MemoryRouter>{pagina}</MemoryRouter>);

describe("Páginas institucionais", () => {
  it("Sobre identifica o responsável com CNPJ e contato", () => {
    abrir(<Sobre />);
    expect(screen.getByText(/CNPJ 00\.000\.000\/0001-00/)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "contato@teste.dev" })).toHaveAttribute("href", "mailto:contato@teste.dev");
  });

  it("Privacidade cobre o que a LGPD exige: controlador, dados, bases legais, direitos e ANPD", () => {
    abrir(<Privacidade />);
    expect(screen.getByRole("heading", { name: /responsável \(controlador\)/ })).toBeInTheDocument();
    expect(screen.getByRole("columnheader", { name: "Base legal (LGPD)" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: /Seus direitos/ })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "ANPD" })).toHaveAttribute("href", "https://www.gov.br/anpd");
    expect(screen.getByText(/Última atualização/)).toBeInTheDocument();
  });

  it("Termos apontam para a Política de Privacidade", () => {
    abrir(<Termos />);
    expect(screen.getByRole("link", { name: "Política de Privacidade" })).toHaveAttribute("href", "/privacidade");
  });
});
