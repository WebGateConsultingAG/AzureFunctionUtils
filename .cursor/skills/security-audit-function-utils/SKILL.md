---
name: security-audit-function-utils
description: >-
  Security audit for WebGate.Azure.FunctionsUtils: Easy Auth, local Entra JWT
  (API + SPA ID token), claim mapping, fail-closed identity, env flags, and
  consumer misuse. Use when the user asks for security review, threat model,
  auth audit, JWT validation review, PR security gate, or «Security» on
  AzureFunctionUtils / FunctionRunContext changes.
disable-model-invocation: true
---

# Security Audit — WebGate.Azure.FunctionsUtils

Structured security review for this **library only** (identity from HTTP headers / validated JWT). Not a full Azure landing-zone audit.

**Pflicht-Lektüre bei Audit:** [README.md](../../README.md) (Security model) · [docs/architecture-function-run-context.md](../../docs/architecture-function-run-context.md) · [docs/local-entra-jwt-validation.md](../../docs/local-entra-jwt-validation.md) · [AGENTS.md](../../AGENTS.md)

Detail-Checkliste: [reference.md](reference.md)

## Modus

| User-Signal | Modus |
|-------------|--------|
| «audit», «prüfen», «findings», «threat», «Security Review» | **Audit** — Report, kein Code-Fix |
| «fix», «beheben», «umsetzen» | **Implement** — minimaler Fix + Tests |
| «commit» / «PR» | Git nach User-Regeln |

**Scope:** Diff oder genannte Dateien unter `AzureFunctionsUtils/` und `AzureFunctionsUtils.Tests/`. Keine Consumer-Apps auditieren, ausser der User nennt sie explizit als Integrationskontext.

## Workflow

1. **Scope:** `git diff` oder User-Pfad; betroffene Parser / `ClaimsPrincipalResolver` / Public API identifizieren.
2. **Trust model:** Cloud = Easy Auth only? Local JWT nur mit `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT=true`?
3. **Checkliste** (reference.md) durchgehen — jede Zeile: OK / Finding / N/A.
4. **Tests:** Fehlen Regression-Tests für neue Auth-Zweige oder fail-closed Verhalten?
5. **Docs:** README / `docs/*` / XML-Docs konsistent mit Verhalten?
6. **Audit-Report** (Deutsch, siehe Template). **Implement:** Rot → Grün → Refactor; `dotnet test AzureFunctionUtils.sln -c Release`.

Optional: User verlangt explizit **Security Review**-Subagent → Cursor `@review-security` auf dieses Repo (`uncommitted changes` oder `branch changes`).

## Report-Template

```markdown
# Security Audit — WebGate.Azure.FunctionsUtils

## Scope
[Diff / Dateien / Version]

## Executive summary
[1 Absatz: Gesamtrisiko, wichtigste 0–2 Punkte]

## Findings

| P | Bereich | Finding | Empfehlung |
|---|---------|---------|------------|
| P0 | … | … | … |

P0 = Exploit / Auth bypass / trust boundary break · P1 = Significant misconfig or weak validation · P2 = Hardening / docs / tests

## Positiv / bereits korrekt
- …

## Nächste Schritte
- [ ] …
```

## Quick red flags (sofort P0/P1 prüfen)

- Bearer oder Custom-Header **ohne** Signatur/Platform-Validation in **Cloud**-Pfad
- `FUNCTIONS_UTILS_LOCAL_DEVELOPMENT` in Doku als Azure App Setting empfohlen
- Neuer Code vertraut `x-ms-*`, `Authorization`, oder Body-Feldern für UserId **ohne** Resolver-Kette
- Audience/Issuer-Validierung gelockert oder optional gemacht
- Exceptions/Fehler geben Token- oder Claim-Inhalte nach aussen
- `IsAuthenticated()` true ohne aufgelöste UserId
- Rollen aus ungeprüften Quellen (nicht Principal nach Parser)

## Nach Implement-Fix

- `dotnet test AzureFunctionUtils.sln -c Release` grün
- Security-relevante Änderung: README + betroffene `docs/*` + XML-Docs
- Kein Consumer-Projekt als «typisches Beispiel» in Library-Docs einpfropfen
