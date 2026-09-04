# check-scanner

A Blazor Server app for scanning grocery receipts (phone photo upload), parsing
line items via Azure AI Foundry `gpt-4o-mini` vision, and tracking price
history, store comparison, and spending trends. Deployed on the `home-server`
k3s cluster in its own `groceries` namespace.

## Agent skills

### Issue tracker

Issues and PRDs live as GitHub issues in `aaksionau/check-scanner`, via the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Triage labels

See `docs/agents/triage-labels.md` — uses the standard five-label vocabulary.

### Domain docs

Single-context layout — `CONTEXT.md` + `docs/adr/` at the repo root (created lazily as terms/decisions get resolved). See `docs/agents/domain.md`.
