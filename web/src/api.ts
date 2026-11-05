const BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5158/v1";

export type Categoria = { id: number; nome: string; icone?: string; cor?: string };
export type MaterialResumo = { id: number; categoriaId: number; categoria: string; nome: string; perigoso: boolean };
export type MaterialDetalhe = MaterialResumo & { instrucoes: string; cuidados?: string; sinonimos?: string[] };
export type PontoProximo = {
  id: string; nome: string; endereco: string; distanciaMetros: number;
  lat: number; lng: number; materiais: string[];
};

async function get<T>(caminho: string): Promise<T> {
  const r = await fetch(BASE + caminho);
  if (!r.ok) throw new Error(`Erro ${r.status} ao consultar a API`);
  return r.json();
}

export const api = {
  categorias: () => get<Categoria[]>("/categorias"),
  materiais: (busca = "", categoriaId?: number) => {
    const q = new URLSearchParams();
    if (busca) q.set("busca", busca);
    if (categoriaId) q.set("categoriaId", String(categoriaId));
    return get<MaterialResumo[]>(`/materiais?${q}`);
  },
  material: (id: number) => get<MaterialDetalhe>(`/materiais/${id}`),
  proximos: (lat: number, lng: number, raio: number, materialId?: number) => {
    const q = new URLSearchParams({ lat: String(lat), lng: String(lng), raio: String(raio) });
    if (materialId) q.set("materialId", String(materialId));
    return get<PontoProximo[]>(`/pontos/proximos?${q}`);
  },
};
