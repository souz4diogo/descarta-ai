import { useEffect, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { MapContainer, TileLayer, Marker, Popup, CircleMarker, useMap } from "react-leaflet";
import L from "leaflet";
import { Crosshair, Loader2, MapPin, Navigation, RotateCw } from "lucide-react";
import { api, type MaterialResumo, type PontoProximo } from "../api";

const pino = (ativo: boolean) => L.divIcon({
  className: "pino" + (ativo ? " ativo" : ""),
  html: '<svg viewBox="0 0 24 32" aria-hidden="true"><path d="M12 0C5.4 0 0 5.3 0 11.9 0 20.8 12 32 12 32s12-11.2 12-20.1C24 5.3 18.6 0 12 0z"/><circle cx="12" cy="12" r="4.5" fill="#fff"/></svg>',
  iconSize: ativo ? [36, 48] : [28, 37],
  iconAnchor: ativo ? [18, 48] : [14, 37],
  popupAnchor: [0, -36],
});

const CENTRO_ARACATUBA: [number, number] = [-21.2089, -50.4329];
const RAIOS = [2000, 5000, 10000, 25000, 50000];
const km = (m: number) => (m < 1000 ? `${Math.round(m)} m` : `${(m / 1000).toFixed(1).replace(".", ",")} km`);
const rota = (p: PontoProximo) => `https://www.google.com/maps/dir/?api=1&destination=${p.lat},${p.lng}`;

function Controle({ origem, foco, pontos }: { origem: [number, number]; foco?: PontoProximo; pontos?: PontoProximo[] }) {
  const mapa = useMap();
  // Enquadra a origem e todos os pontos encontrados.
  useEffect(() => {
    if (!pontos?.length) { mapa.setView(origem, 13); return; }
    mapa.fitBounds(L.latLngBounds([origem, ...pontos.map(p => [p.lat, p.lng] as [number, number])]),
      { padding: [40, 40], maxZoom: 15 });
  }, [origem, pontos, mapa]);
  useEffect(() => { if (foco) mapa.flyTo([foco.lat, foco.lng], 16, { duration: 0.4 }); }, [foco, mapa]);
  return null;
}

export default function Mapa() {
  const [params, setParams] = useSearchParams();
  const materialId = Number(params.get("materialId")) || undefined;
  const raio = Number(params.get("raio")) || 10000;
  const [materiais, setMateriais] = useState<MaterialResumo[]>([]);
  const [origem, setOrigem] = useState<[number, number]>(CENTRO_ARACATUBA);
  const [gps, setGps] = useState<"centro" | "buscando" | "ok" | "negado">("centro");
  const [pontos, setPontos] = useState<PontoProximo[]>();
  const [erro, setErro] = useState(false);
  const [tentativa, setTentativa] = useState(0);
  const [selecionado, setSelecionado] = useState<PontoProximo>();
  const marcadores = useRef<Record<string, L.Marker>>({});

  useEffect(() => { api.materiais().then(setMateriais).catch(() => {}); }, []);
  useEffect(() => {
    setPontos(undefined); setErro(false); setSelecionado(undefined);
    api.proximos(origem[0], origem[1], raio, materialId).then(setPontos).catch(() => setErro(true));
  }, [origem, raio, materialId, tentativa]);

  const atualizar = (chave: string, valor?: number) => {
    const p = new URLSearchParams(params);
    if (valor) p.set(chave, String(valor)); else p.delete(chave);
    setParams(p, { replace: true });
  };

  const localizar = () => {
    if (!navigator.geolocation) return setGps("negado");
    setGps("buscando");
    navigator.geolocation.getCurrentPosition(
      p => { setOrigem([p.coords.latitude, p.coords.longitude]); setGps("ok"); },
      () => setGps("negado"),
      { enableHighAccuracy: true, timeout: 10000 });
  };

  const selecionar = (p: PontoProximo) => {
    setSelecionado(p);
    setTimeout(() => marcadores.current[p.id]?.openPopup(), 450);
  };

  const proximoRaio = RAIOS.find(r => r > raio);
  const nomeMaterial = materiais.find(m => m.id === materialId)?.nome;

  return (
    <section className="mapa-pagina">
      <h1>Pontos de descarte</h1>

      <div className="filtros">
        <div className="campo">
          <label htmlFor="f-material" className="rotulo">Material</label>
          <select id="f-material" value={materialId ?? ""} onChange={e => atualizar("materialId", Number(e.target.value))}>
            <option value="">Todos os materiais</option>
            {materiais.map(m => <option key={m.id} value={m.id}>{m.nome}</option>)}
          </select>
        </div>
        <div className="campo">
          <label htmlFor="f-raio" className="rotulo">Distância</label>
          <select id="f-raio" value={raio} onChange={e => atualizar("raio", Number(e.target.value))}>
            {RAIOS.map(r => <option key={r} value={r}>Até {km(r)}</option>)}
          </select>
        </div>
        <button className="botao" onClick={localizar} disabled={gps === "buscando"}>
          {gps === "buscando"
            ? <><Loader2 size={18} className="girando" aria-hidden="true" /> Localizando…</>
            : <><Crosshair size={18} aria-hidden="true" /> Usar minha localização</>}
        </button>
      </div>

      <p className="status" aria-live="polite">
        {gps === "ok" && "Mostrando a partir da sua localização."}
        {gps === "centro" && "Mostrando a partir do centro de Araçatuba."}
        {gps === "negado" && "Não conseguimos sua localização (permissão negada ou indisponível). Mostrando a partir do centro."}
        {pontos && ` ${pontos.length} ${pontos.length === 1 ? "ponto encontrado" : "pontos encontrados"}.`}
      </p>

      <div className="mapa-grade">
        <div className="mapa-caixa">
          <MapContainer center={origem} zoom={13} className="mapa" aria-label="Mapa dos pontos de descarte">
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
              url="https://tile.openstreetmap.org/{z}/{x}/{y}.png" />
            <Controle origem={origem} foco={selecionado} pontos={pontos} />
            <CircleMarker center={origem} radius={9}
              pathOptions={{ color: "#fff", weight: 3, fillColor: "#2563eb", fillOpacity: 1 }} />
            {pontos?.map(p => (
              <Marker key={p.id} position={[p.lat, p.lng]} icon={pino(selecionado?.id === p.id)}
                ref={m => { if (m) marcadores.current[p.id] = m; }}
                eventHandlers={{ click: () => setSelecionado(p) }}>
                <Popup>
                  <strong>{p.nome}</strong><br />{p.endereco}<br />
                  <a href={rota(p)} target="_blank" rel="noreferrer">Como chegar</a>
                </Popup>
              </Marker>
            ))}
          </MapContainer>
        </div>

        <div className="resultados">
          {erro ? (
            <div className="aviso erro" role="alert">
              <p>Não foi possível buscar os pontos.</p>
              <button className="botao secundario" onClick={() => setTentativa(t => t + 1)}>
                <RotateCw size={18} aria-hidden="true" /> Tentar de novo
              </button>
            </div>
          ) : !pontos ? (
            <ul className="lista" aria-hidden="true">{[1, 2, 3].map(i => <li key={i} className="esqueleto alto" />)}</ul>
          ) : pontos.length === 0 ? (
            <div className="aviso">
              <p>Nenhum ponto {nomeMaterial ? `que aceite “${nomeMaterial}” ` : ""}até {km(raio)}.</p>
              {proximoRaio && (
                <button className="botao secundario" onClick={() => atualizar("raio", proximoRaio)}>
                  Buscar até {km(proximoRaio)}
                </button>
              )}
            </div>
          ) : (
            <ol className="lista">
              {pontos.map(p => (
                <li key={p.id} className={`ponto ${selecionado?.id === p.id ? "selecionado" : ""}`}>
                  <button className="ponto-corpo" onClick={() => selecionar(p)}
                    aria-label={`${p.nome}, ${km(p.distanciaMetros)}. Mostrar no mapa`}>
                    <span className="ponto-topo">
                      <strong>{p.nome}</strong>
                      <span className="dist">{km(p.distanciaMetros)}</span>
                    </span>
                    <span className="ponto-end"><MapPin size={14} aria-hidden="true" /> {p.endereco}</span>
                    <span className="tags">{p.materiais.map(n => <span key={n} className="tag">{n}</span>)}</span>
                  </button>
                  <a className="botao secundario pequeno" href={rota(p)} target="_blank" rel="noreferrer">
                    <Navigation size={16} aria-hidden="true" /> Como chegar
                  </a>
                </li>
              ))}
            </ol>
          )}
        </div>
      </div>
    </section>
  );
}
