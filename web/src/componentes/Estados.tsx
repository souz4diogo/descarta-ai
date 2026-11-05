import type { ReactNode } from "react";
import { RotateCw } from "lucide-react";

export function AvisoErro({ mensagem, onTentar }: { mensagem: string; onTentar: () => void }) {
  return (
    <div className="aviso erro" role="alert">
      <p>{mensagem}</p>
      <button className="botao secundario" onClick={onTentar}>
        <RotateCw size={18} aria-hidden="true" /> Tentar de novo
      </button>
    </div>
  );
}

export function Aviso({ children }: { children: ReactNode }) {
  return <div className="aviso">{children}</div>;
}

export function ListaEsqueleto({ itens, alto = false }: { itens: number; alto?: boolean }) {
  return (
    <ul className="lista" aria-hidden="true">
      {Array.from({ length: itens }, (_, i) => <li key={i} className={`esqueleto ${alto ? "alto" : ""}`} />)}
    </ul>
  );
}
