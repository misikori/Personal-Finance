
export type KpiCardProps = {
  label: string;
  value: string;
  sublabel?: string;
  trend?: "up" | "down" | "flat";
};