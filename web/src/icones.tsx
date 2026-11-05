import {
  Battery, Cpu, Droplet, Hammer, Lightbulb, Pill, Recycle, Package, type LucideIcon,
} from "lucide-react";

const mapa: Record<string, LucideIcon> = {
  droplet: Droplet, battery: Battery, cpu: Cpu, lightbulb: Lightbulb,
  pill: Pill, hammer: Hammer, recycle: Recycle,
};

export function IconeCategoria({ nome, size = 20 }: { nome?: string; size?: number }) {
  const Icone = (nome && mapa[nome]) || Package;
  return <Icone size={size} aria-hidden="true" />;
}
