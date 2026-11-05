import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { AlertTriangle, ChevronDown, MapPin, Search } from "lucide-react";
import { api, type Categoria, type MaterialResumo } from "../api";
import { IconeCategoria } from "../icones";
import { Aviso, AvisoErro, ListaEsqueleto } from "../componentes/Estados";
import { numeroDaUrl, plural, useConsulta } from "../util";

const ESPERA_DIGITACAO_MS = 250;

export default function Guia() {
  const [params, setParams] = useSearchParams();
  const categoriaId = numeroDaUrl(params, "categoriaId");
  const buscaUrl = params.get("busca") ?? "";
  const [busca, setBusca] = useState(buscaUrl);
  const [aberto, setAberto] = useState<number>();

  // Grava a busca na URL só depois que o usuário para de digitar.
  useEffect(() => {
    const t = setTimeout(() => setParams(p => {
      const novo = new URLSearchParams(p);
      if (busca.trim()) novo.set("busca", busca.trim()); else novo.delete("busca");
      return novo;
    }, { replace: true }), ESPERA_DIGITACAO_MS);
    return () => clearTimeout(t);
  }, [busca, setParams]);

  const categorias = useConsulta(s => api.categorias(s), []);
  const materiais = useConsulta(s => api.materiais({ busca: buscaUrl, categoriaId }, s), [buscaUrl, categoriaId]);

  const filtrar = (id?: number) => setParams(p => {
    const novo = new URLSearchParams(p);
    if (id) novo.set("categoriaId", String(id)); else novo.delete("categoriaId");
    return novo;
  }, { replace: true });

  const limpar = () => { setBusca(""); filtrar(); };

  return (
    <section>
      <h1>Como descartar cada coisa</h1>

      <label htmlFor="busca-guia" className="rotulo">O que você quer descartar?</label>
      <div className="campo-busca">
        <Search size={20} aria-hidden="true" className="busca-icone" />
        <input id="busca-guia" type="search" placeholder="Ex.: pilha, remédio, colchão" value={busca}
          maxLength={60} onChange={e => setBusca(e.target.value)} autoComplete="off" />
      </div>

      <FiltroCategorias categorias={categorias.dados ?? []} selecionada={categoriaId} onFiltrar={filtrar} />

      <div aria-live="polite" className="sr-only">
        {materiais.dados && plural(materiais.dados.length, "item encontrado", "itens encontrados")}
      </div>

      {materiais.erro ? (
        <AvisoErro mensagem="Não foi possível carregar os dados. Verifique sua conexão." onTentar={materiais.tentarDeNovo} />
      ) : !materiais.dados ? (
        <ListaEsqueleto itens={4} />
      ) : materiais.dados.length === 0 ? (
        <Aviso>
          <p>Nenhum item encontrado{buscaUrl && <> para “{buscaUrl}”</>}.</p>
          <p>Tente outra palavra ou <button className="link" onClick={limpar}>veja todos os itens</button>.</p>
        </Aviso>
      ) : (
        <ul className="lista">
          {materiais.dados.map(m => (
            <ItemMaterial key={m.id} material={m}
              icone={categorias.dados?.find(c => c.id === m.categoriaId)?.icone}
              expandido={aberto === m.id}
              onAlternar={() => setAberto(aberto === m.id ? undefined : m.id)} />
          ))}
        </ul>
      )}
    </section>
  );
}

function FiltroCategorias({ categorias, selecionada, onFiltrar }:
  { categorias: Categoria[]; selecionada?: number; onFiltrar: (id?: number) => void }) {
  return (
    <div className="chips" role="group" aria-label="Filtrar por tipo">
      <button aria-pressed={!selecionada} onClick={() => onFiltrar()}>Todos</button>
      {categorias.map(c => (
        <button key={c.id} aria-pressed={selecionada === c.id} onClick={() => onFiltrar(c.id)}>
          <IconeCategoria nome={c.icone} size={16} /> {c.nome}
        </button>
      ))}
    </div>
  );
}

function ItemMaterial({ material: m, icone, expandido, onAlternar }:
  { material: MaterialResumo; icone?: string; expandido: boolean; onAlternar: () => void }) {
  return (
    <li className={`material ${expandido ? "aberto" : ""}`}>
      <h2 className="material-titulo">
        <button className="item" onClick={onAlternar} aria-expanded={expandido} aria-controls={`det-${m.id}`}>
          <span className="item-icone"><IconeCategoria nome={icone} /></span>
          <span className="item-texto">
            <span className="item-nome">{m.nome}</span>
            <span className="item-meta">{m.categoria}</span>
          </span>
          {m.perigoso && <span className="selo"><AlertTriangle size={14} aria-hidden="true" /> Perigoso</span>}
          <ChevronDown size={20} aria-hidden="true" className="seta" />
        </button>
      </h2>
      {expandido && <DetalheMaterial id={m.id} />}
    </li>
  );
}

function DetalheMaterial({ id }: { id: number }) {
  const detalhe = useConsulta(s => api.material(id, s), [id]);
  return (
    <div id={`det-${id}`} className="detalhe">
      {detalhe.erro ? (
        <p className="erro">Não foi possível carregar as instruções. <button className="link" onClick={detalhe.tentarDeNovo}>Tentar de novo</button></p>
      ) : !detalhe.dados ? (
        <p className="carregando">Carregando…</p>
      ) : (
        <>
          <p>{detalhe.dados.instrucoes}</p>
          {detalhe.dados.cuidados && (
            <p className="cuidado">
              <AlertTriangle size={18} aria-hidden="true" />
              <span><strong>Cuidado:</strong> {detalhe.dados.cuidados}</span>
            </p>
          )}
          <Link to={`/mapa?materialId=${id}`} className="botao">
            <MapPin size={18} aria-hidden="true" /> Onde levar
          </Link>
        </>
      )}
    </div>
  );
}
