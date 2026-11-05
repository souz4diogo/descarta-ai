import { useEffect, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { MapContainer, TileLayer, Marker, Popup, CircleMarker, useMap } from "react-leaflet";
import L from "leaflet";
import { Crosshair, Loader2, MapPin, Navigation } from "lucide-react";
import { api, type PontoProximo } from "../api";
import { Aviso, AvisoErro, ListaEsqueleto } from "../componentes/Estados";
import { formatarDistancia, numeroDaUrl, plural, urlRota, useConsulta } from "../util";

type Coordenada = [number, number];
type SituacaoGps = "centro" | "buscando" | "ok" | "negado";

const CENTRO_ARACATUBA: Coordenada = [-21.2089, -50.4329];
const RAIOS = [2000, 5000, 10000, 25000, 50000];
const RAIO_PADRAO = 10000;
const ZOOM_INICIAL = 13;
const ZOOM_PONTO = 16;
const DURACAO_VOO_S = 0.4;

const MENSAGEM_GPS: Record<SituacaoGps, string> = {
  centro: "Mostrando a partir do centro de Araçatuba.",
  buscando: "Obtendo sua localização…",
  ok: "Mostrando a partir da sua localização.",
  negado: "Não conseguimos sua localização (permissão negada ou indisponível). Mostrando a partir do centro.",
};

const pino = (ativo: boolean) => L.divIcon({
  className: "pino" + (ativo ? " ativo" : ""),
  html: '<svg viewBox="0 0 24 32" aria-hidden="true"><path d="M12 0C5.4 0 0 5.3 0 11.9 0 20.8 12 32 12 32s12-11.2 12-20.1C24 5.3 18.6 0 12 0z"/><circle cx="12" cy="12" r="4.5" fill="#fff"/></svg>',
  iconSize: ativo ? [36, 48] : [28, 37],
  iconAnchor: ativo ? [18, 48] : [14, 37],
  popupAnchor: [0, -36],
});

export default function Mapa() {
  const [params, setParams] = useSearchParams();
  const materialId = numeroDaUrl(params, "materialId");
  const raio = RAIOS.includes(Number(params.get("raio"))) ? Number(params.get("raio")) : RAIO_PADRAO;
  const [origem, setOrigem] = useState<Coordenada>(CENTRO_ARACATUBA);
  const [gps, setGps] = useState<SituacaoGps>("centro");
  const [selecionadoId, setSelecionadoId] = useState<string>();

  const materiais = useConsulta(s => api.materiais({}, s), []);
  const pontos = useConsulta(
    s => api.proximos({ lat: origem[0], lng: origem[1], raio, materialId }, s),
    [origem, raio, materialId]);

  // Derivado dos resultados: se o ponto sair da lista após um novo filtro, a seleção some sozinha.
  const selecionado = pontos.dados?.find(p => p.id === selecionadoId);

  const atualizar = (chave: string, valor?: number) => setParams(p => {
    const novo = new URLSearchParams(p);
    if (valor) novo.set(chave, String(valor)); else novo.delete(chave);
    return novo;
  }, { replace: true });

  const localizar = () => {
    if (!navigator.geolocation) return setGps("negado");
    setGps("buscando");
    navigator.geolocation.getCurrentPosition(
      p => { setOrigem([p.coords.latitude, p.coords.longitude]); setGps("ok"); },
      () => setGps("negado"),
      { enableHighAccuracy: true, timeout: 10000 });
  };

  const proximoRaio = RAIOS.find(r => r > raio);
  const nomeMaterial = materiais.dados?.find(m => m.id === materialId)?.nome;

  return (
    <section className="mapa-pagina">
      <h1>Pontos de descarte</h1>

      <div className="filtros">
        <div className="campo">
          <label htmlFor="f-material" className="rotulo">Material</label>
          <select id="f-material" value={materialId ?? ""} onChange={e => atualizar("materialId", Number(e.target.value))}>
            <option value="">Todos os materiais</option>
            {materiais.dados?.map(m => <option key={m.id} value={m.id}>{m.nome}</option>)}
          </select>
        </div>
        <div className="campo">
          <label htmlFor="f-raio" className="rotulo">Distância</label>
          <select id="f-raio" value={raio} onChange={e => atualizar("raio", Number(e.target.value))}>
            {RAIOS.map(r => <option key={r} value={r}>Até {formatarDistancia(r)}</option>)}
          </select>
        </div>
        <button className="botao" onClick={localizar} disabled={gps === "buscando"}>
          {gps === "buscando"
            ? <><Loader2 size={18} className="girando" aria-hidden="true" /> Localizando…</>
            : <><Crosshair size={18} aria-hidden="true" /> Usar minha localização</>}
        </button>
      </div>

      <p className="status" aria-live="polite">
        {MENSAGEM_GPS[gps]}
        {pontos.dados && !pontos.carregando && ` ${plural(pontos.dados.length, "ponto encontrado", "pontos encontrados")}.`}
      </p>

      <div className="mapa-grade">
        <MapaPontos origem={origem} pontos={pontos.dados} selecionado={selecionado} onSelecionar={p => setSelecionadoId(p.id)} />

        <div className="resultados">
          {pontos.erro ? (
            <AvisoErro mensagem="Não foi possível buscar os pontos." onTentar={pontos.tentarDeNovo} />
          ) : !pontos.dados || pontos.carregando ? (
            <ListaEsqueleto itens={3} alto />
          ) : pontos.dados.length === 0 ? (
            <Aviso>
              <p>Nenhum ponto {nomeMaterial && `que aceite “${nomeMaterial}” `}até {formatarDistancia(raio)}.</p>
              {proximoRaio && (
                <button className="botao secundario" onClick={() => atualizar("raio", proximoRaio)}>
                  Buscar até {formatarDistancia(proximoRaio)}
                </button>
              )}
            </Aviso>
          ) : (
            <ol className="lista">
              {pontos.dados.map(p => (
                <CartaoPonto key={p.id} ponto={p} selecionado={selecionado?.id === p.id} onSelecionar={() => setSelecionadoId(p.id)} />
              ))}
            </ol>
          )}
        </div>
      </div>
    </section>
  );
}

function MapaPontos({ origem, pontos, selecionado, onSelecionar }: {
  origem: Coordenada; pontos?: PontoProximo[]; selecionado?: PontoProximo; onSelecionar: (p: PontoProximo) => void;
}) {
  return (
    <div className="mapa-caixa">
      <MapContainer center={origem} zoom={ZOOM_INICIAL} className="mapa" aria-label="Mapa dos pontos de descarte">
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url="https://tile.openstreetmap.org/{z}/{x}/{y}.png" />
        <Enquadramento origem={origem} pontos={pontos} />
        <CircleMarker center={origem} radius={9}
          pathOptions={{ color: "#fff", weight: 3, fillColor: "#2563eb", fillOpacity: 1 }} />
        {pontos?.map(p => (
          <MarcadorPonto key={p.id} ponto={p} ativo={selecionado?.id === p.id} onSelecionar={() => onSelecionar(p)} />
        ))}
      </MapContainer>
    </div>
  );
}

/** Enquadra a origem e todos os pontos encontrados. */
function Enquadramento({ origem, pontos }: { origem: Coordenada; pontos?: PontoProximo[] }) {
  const mapa = useMap();
  useEffect(() => {
    if (!pontos?.length) { mapa.setView(origem, ZOOM_INICIAL); return; }
    const limites = L.latLngBounds([origem, ...pontos.map(p => [p.lat, p.lng] as Coordenada)]);
    mapa.fitBounds(limites, { padding: [40, 40], maxZoom: 15 });
  }, [origem, pontos, mapa]);
  return null;
}

/** Ao ser selecionado (na lista ou no mapa), voa até o ponto e abre o balão. */
function MarcadorPonto({ ponto, ativo, onSelecionar }: { ponto: PontoProximo; ativo: boolean; onSelecionar: () => void }) {
  const mapa = useMap();
  const marcador = useRef<L.Marker>(null);

  useEffect(() => {
    if (!ativo) return;
    mapa.flyTo([ponto.lat, ponto.lng], ZOOM_PONTO, { duration: DURACAO_VOO_S });
    mapa.once("moveend", () => marcador.current?.openPopup());
  }, [ativo, ponto, mapa]);

  return (
    <Marker ref={marcador} position={[ponto.lat, ponto.lng]} icon={pino(ativo)} eventHandlers={{ click: onSelecionar }}>
      <Popup>
        <strong>{ponto.nome}</strong><br />{ponto.endereco}<br />
        <a href={urlRota(ponto.lat, ponto.lng)} target="_blank" rel="noreferrer">Como chegar</a>
      </Popup>
    </Marker>
  );
}

function CartaoPonto({ ponto: p, selecionado, onSelecionar }: { ponto: PontoProximo; selecionado: boolean; onSelecionar: () => void }) {
  const distancia = formatarDistancia(p.distanciaMetros);
  return (
    <li className={`ponto ${selecionado ? "selecionado" : ""}`}>
      <button className="ponto-corpo" onClick={onSelecionar} aria-label={`${p.nome}, ${distancia}. Mostrar no mapa`}>
        <span className="ponto-topo">
          <strong>{p.nome}</strong>
          <span className="dist">{distancia}</span>
        </span>
        <span className="ponto-end"><MapPin size={14} aria-hidden="true" /> {p.endereco}</span>
        <span className="tags">{p.materiais.map(n => <span key={n} className="tag">{n}</span>)}</span>
      </button>
      <a className="botao secundario pequeno" href={urlRota(p.lat, p.lng)} target="_blank" rel="noreferrer">
        <Navigation size={16} aria-hidden="true" /> Como chegar
      </a>
    </li>
  );
}
