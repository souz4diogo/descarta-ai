const BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5158/v1";

export type Categoria = { id: number; nome: string; icone?: string };
export type MaterialResumo = { id: number; categoriaId: number; categoria: string; nome: string; perigoso: boolean };
export type MaterialDetalhe = MaterialResumo & { instrucoes: string; cuidados?: string; sinonimos?: string[] };
export type PontoProximo = {
  id: string; nome: string; endereco: string; distanciaMetros: number;
  lat: number; lng: number; materiais: string[];
};

async function get<T>(caminho: string, sinal?: AbortSignal): Promise<T> {
  const resposta = await fetch(BASE + caminho, { signal: sinal });
  if (!resposta.ok) throw new Error(`Erro ${resposta.status} ao consultar a API`);
  return resposta.json();
}

/** Requisição cancelada porque o componente mudou de filtro ou saiu da tela: não é erro para o usuário. */
export const foiCancelada = (e: unknown) => e instanceof DOMException && e.name === "AbortError";

const consulta = (valores: Record<string, string | number | undefined>) => {
  const q = new URLSearchParams();
  for (const [chave, valor] of Object.entries(valores))
    if (valor !== undefined && valor !== "") q.set(chave, String(valor));
  return q.toString();
};

export const api = {
  categorias: (sinal?: AbortSignal) => get<Categoria[]>("/categorias", sinal),
  materiais: (filtro: { busca?: string; categoriaId?: number } = {}, sinal?: AbortSignal) =>
    get<MaterialResumo[]>(`/materiais?${consulta(filtro)}`, sinal),
  material: (id: number, sinal?: AbortSignal) => get<MaterialDetalhe>(`/materiais/${id}`, sinal),
  proximos: (filtro: { lat: number; lng: number; raio: number; materialId?: number }, sinal?: AbortSignal) =>
    get<PontoProximo[]>(`/pontos/proximos?${consulta(filtro)}`, sinal),
};
