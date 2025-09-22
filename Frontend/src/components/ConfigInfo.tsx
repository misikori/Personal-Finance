import { useAppConfig } from "../config/ConfigProvider";

export default function ConfigInfo() {
  const cfg = useAppConfig();

  return (
    <section
      style={{
        border: "1px solid #1f0202ff",
        borderRadius: 12,
        padding: 16,
        maxWidth: 640,
        background: "black",
      }}
    >
      <h2 style={{ marginTop: 0 }}>Runtime App Config</h2>

      <dl style={{ display: "grid", gridTemplateColumns: "200px 1fr", rowGap: 8, columnGap: 12 }}>
        <dt>API Base URL</dt>
        <dd>{cfg.API_BASE_URL}</dd>

        <dt>Mock Mode</dt>
        <dd>
          <strong style={{ color: cfg.MOCK ? "hsla(124, 63%, 5%, 1.00)" : "#ebe3e3ff" }}>
            {cfg.MOCK ? "ON" : "OFF"}
          </strong>
        </dd>

        <dt>Default Currency</dt>
        <dd>{cfg.DEFAULT_CURRENCY}</dd>

        <dt>Locale</dt>
        <dd>{cfg.LOCALE}</dd>

        <dt>Features</dt>
        <dd>
          <ul style={{ margin: 0, paddingLeft: 18 }}>
            <li>import: {cfg.FEATURES?.import ? "enabled" : "disabled"}</li>
            <li>portfolios: {cfg.FEATURES?.portfolios ? "enabled" : "disabled"}</li>
          </ul>
        </dd>
      </dl>

      {cfg.MOCK && (
        <p style={{ marginTop: 12, fontSize: 14, color: "white" }}>
        <em>Mock mode is enabled.</em> API calls should use stubs/MSW handlers.
        </p>
      )}

      <details style={{ marginTop: 12 }}>
        <summary>Show raw JSON</summary>
        <pre
          style={{
            background: "#d41e1eff",
            padding: 12,
            borderRadius: 8,
            overflowX: "auto",
          }}
        >
{JSON.stringify(cfg, null, 2)}
        </pre>
      </details>
    </section>
  );
}
