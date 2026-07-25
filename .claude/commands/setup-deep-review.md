# ── Config (edit here) ──
REVIEW_DIR: code-review
# Wherever ${REVIEW_DIR} appears below, use that path.

We're doing a deep, project-by-project code review of the forge-of-games solution, one project per session to keep context small.

Do the setup only — don't review any code yet:

1. If Serena hasn't onboarded this project, run onboarding; otherwise read existing Serena memories for context.
2. Read the .sln and list every project in it.
3. Create `${REVIEW_DIR}/README.md`: a table of all projects each with a status (⬜ pending) and a notes column, plus a "Cross-project patterns to check" section at the bottom (start it empty).
4. Propose a review order and show it to me — foundation/leaf projects first (Shared, *Models*, generated contracts), then Infrastructure/persistence, then application/API, then Blazor WASM UI, then Azure Functions — so issues in shared types are found before the code that depends on them.

Stop after showing the order and the tracker.