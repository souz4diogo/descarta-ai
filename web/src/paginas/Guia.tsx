import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { AlertTriangle, ChevronDown, MapPin, RotateCw, Search } from "lucide-react";
import { api, type Categoria, type MaterialDetalhe, type MaterialResumo } from "../api";
import { IconeCategoria } from "../icones";

export default function Guia() {
  const [params, setParams] = useSearchParams();
  const categoriaId = Number(params.get("categoriaId")) || undefined;
  const [busca, setBusca] = useState(params.get("busca") ?? "");
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [materiais, setMateriais] = useState<MaterialResumo[]>();
  const [detalhes, setDetalhes] = useState<Record<number, MaterialDetalhe>>({});
  const [aberto, setAberto] = useState<number>();
  const [erro, setErro] = useState(false);
  const [tentativa, setTentativa] = useState(0);

  useEffect(() => { api.categorias().then(setCategorias).catch(() => setErro(true)); }, [tentativa]);

  useEffect(() => {
    const t = setTimeout(() => {
      setErro(false);
      api.materiais(busca.trim(), categoriaId).then(setMateriais).catch(() => setErro(true));
      const p = new URLSearchParams(params);
      if (busca.trim()) p.set("busca", busca.trim()); else p.delete("busca");
      setParams(p, { replace: true });
    }, 250);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [busca, categoriaId, tentativa]);

  const filtrar = (id?: number) => {
    const p = new URLSearchParams(params);
    if (id) p.set("categoriaId", String(id)); else p.delete("categoriaId");
    setParams(p, { replace: true });
  };

  const alternar = (id: number) => {
    setAberto(aberto === id ? undefined : id);
    if (!detalhes[id]) api.material(id).then(d => setDetalhes(x => ({ ...x, [id]: d }))).catch(() => setErro(true));
  };

  return (
    <section>
      <h1>Como descartar cada coisa</h1>

      <label htmlFor="busca-guia" className="rotulo">O que você quer descartar?</label>
      <div className="campo-busca">
        <Search size={20} aria-hidden="true" className="busca-icone" />
        <input id="busca-guia" type="search" placeholder="Ex.: pilha, remédio, colchão" value={busca}
          onChange={e => setBusca(e.target.value)} autoComplete="off" />
      </div>

      <div className="chips" role="group" aria-label="Filtrar por tipo">
        <button aria-pressed={!categoriaId} onClick={() => filtrar()}>Todos</button>
        {categorias.map(c => (
          <button key={c.id} aria-pressed={categoriaId === c.id} onClick={() => filtrar(c.id)}
           >
            <IconeCategoria nome={c.icone} size={16} /> {c.nome}
          </button>
        ))}
      </div>

      <div aria-live="polite" className="sr-only">
        {materiais && `${materiais.length} ${materiais.length === 1 ? "item encontrado" : "itens encontrados"}`}
      </div>

      {erro ? (
        <div className="aviso erro" role="alert">
          <p>Não foi possível carregar os dados. Verifique sua conexão.</p>
          <button className="botao secundario" onClick={() => setTentativa(t => t + 1)}>
            <RotateCw size={18} aria-hidden="true" /> Tentar de novo
          </button>
        </div>
      ) : !materiais ? (
        <ul className="lista" aria-hidden="true">{[1, 2, 3, 4].map(i => <li key={i} className="esqueleto" />)}</ul>
      ) : materiais.length === 0 ? (
        <div className="aviso">
          <p>Nenhum item encontrado para “{busca}”.</p>
          <p>Tente outra palavra ou <button className="link" onClick={() => { setBusca(""); filtrar(); }}>veja todos os itens</button>.</p>
        </div>
      ) : (
        <ul className="lista">
          {materiais.map(m => {
            const d = detalhes[m.id];
            const cat = categorias.find(c => c.id === m.categoriaId);
            const expandido = aberto === m.id;
            return (
              <li key={m.id} className={`material ${expandido ? "aberto" : ""}`}>
                <h2 className="material-titulo">
                  <button className="item" onClick={() => alternar(m.id)} aria-expanded={expandido}
                    aria-controls={`det-${m.id}`}>
                    <span className="item-icone">
                      <IconeCategoria nome={cat?.icone} />
                    </span>
                    <span className="item-texto">
                      <span className="item-nome">{m.nome}</span>
                      <span className="item-meta">{m.categoria}</span>
                    </span>
                    {m.perigoso && <span className="selo"><AlertTriangle size={14} aria-hidden="true" /> Perigoso</span>}
                    <ChevronDown size={20} aria-hidden="true" className="seta" />
                  </button>
                </h2>
                {expandido && (
                  <div id={`det-${m.id}`} className="detalhe">
                    {!d ? <p className="carregando">Carregando…</p> : (
                      <>
                        <p>{d.instrucoes}</p>
                        {d.cuidados && (
                          <p className="cuidado"><AlertTriangle size={18} aria-hidden="true" /><span><strong>Cuidado:</strong> {d.cuidados}</span></p>
                        )}
                        <Link to={`/mapa?materialId=${m.id}`} className="botao">
                          <MapPin size={18} aria-hidden="true" /> Onde levar
                        </Link>
                      </>
                    )}
                  </div>
                )}
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}
