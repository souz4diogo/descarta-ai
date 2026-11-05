import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { MapPin, Search, BookOpen, Navigation } from "lucide-react";
import { api, type Categoria } from "../api";
import { IconeCategoria } from "../icones";

export default function Inicio() {
  const [busca, setBusca] = useState("");
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const navegar = useNavigate();

  useEffect(() => { api.categorias().then(setCategorias).catch(() => {}); }, []);

  const buscar = (e: FormEvent) => {
    e.preventDefault();
    navegar(busca.trim() ? `/guia?busca=${encodeURIComponent(busca.trim())}` : "/guia");
  };

  return (
    <>
      <section className="hero">
        <h1>Onde descartar isso?</h1>
        <p className="lead">
          Descubra como descartar cada item e encontre o ponto de coleta mais próximo em Araçatuba e região.
        </p>
        <form className="busca-hero" onSubmit={buscar} role="search">
          <label htmlFor="busca-inicio" className="sr-only">O que você quer descartar?</label>
          <Search size={20} aria-hidden="true" className="busca-icone" />
          <input id="busca-inicio" type="search" placeholder="Ex.: pilha, óleo de cozinha, colchão"
            value={busca} onChange={e => setBusca(e.target.value)} autoComplete="off" />
          <button type="submit" className="botao">Buscar</button>
        </form>
        <Link to="/mapa" className="link-acao"><MapPin size={18} aria-hidden="true" /> Ver pontos perto de mim</Link>
      </section>

      {categorias.length > 0 && (
        <section aria-labelledby="titulo-cat" className="secao">
          <h2 id="titulo-cat">Escolha pelo tipo de resíduo</h2>
          <ul className="grade-cat">
            {categorias.map(c => (
              <li key={c.id}>
                <Link to={`/guia?categoriaId=${c.id}`} className="cat">
                  <span className="cat-icone"><IconeCategoria nome={c.icone} size={24} /></span>
                  {c.nome}
                </Link>
              </li>
            ))}
          </ul>
        </section>
      )}

      <section aria-labelledby="titulo-como" className="secao">
        <h2 id="titulo-como">Como funciona</h2>
        <ol className="passos">
          <li><BookOpen aria-hidden="true" /><div><strong>Escolha o item</strong><span>Veja como preparar e os cuidados de descarte.</span></div></li>
          <li><MapPin aria-hidden="true" /><div><strong>Encontre o ponto</strong><span>Mostramos os locais que aceitam o item, do mais perto ao mais longe.</span></div></li>
          <li><Navigation aria-hidden="true" /><div><strong>Leve até lá</strong><span>Abra a rota no Google Maps com um toque.</span></div></li>
        </ol>
      </section>
    </>
  );
}
