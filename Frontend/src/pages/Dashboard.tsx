import ConfigInfo from "../components/ConfigInfo";

export default function Dashboard() {
  return (
    <div style={{ display: "grid", gap: 24 }}>
      <h1>Dashboard</h1>
      <ConfigInfo />
    </div>
  );
}