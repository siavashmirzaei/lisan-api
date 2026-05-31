# Environment Variables

All variables are read from the process environment. The backend validates all required variables at startup and exits with a descriptive error if any are missing.

## Backend

### Required

| Variable | Description |
|----------|-------------|
| `DATABASE_URL` | PostgreSQL connection string. Format: `postgresql://user:password@host:port/dbname`. Supabase connection string from the project settings. |
| `CLERK_AUTHORITY` | Clerk Frontend API URL. Found in the Clerk Dashboard under **API Keys → Advanced**. Example: `https://xxxx.clerk.accounts.dev`. **Note:** The ticket spec lists `CLERK_SECRET_KEY`; the actual SDK (`Clerk.Net.AspNetCore.Security`) uses `CLERK_AUTHORITY` for JWKS-based offline validation. |
| `OPENAI_API_KEY` | OpenAI API key used for the AI conversation pipeline (chat completions). |
| `GOOGLE_TTS_API_KEY` | Google Cloud Text-to-Speech API key used for Persian speech synthesis. |
| `PRIVACY_POLICY_VERSION` | Version identifier of the active privacy policy (e.g. `v1.0`). Stored on parental consent records to track which policy version the parent accepted. Required for PIPEDA compliance. |

### Optional

| Variable | Default | Description |
|----------|---------|-------------|
| `SENTRY_DSN_BACKEND` | *(disabled)* | Sentry DSN for backend error tracking. Leave empty or unset to disable Sentry entirely. |
| `POSTHOG_API_KEY` | *(disabled)* | PostHog API key for product analytics. Leave empty or unset to disable. |
| `TRANSCRIPT_RETENTION_DAYS` | `90` | Number of days to retain `Transcript` records before the nightly deletion job removes them. Must be a positive integer. Matches the 90-day PIPEDA retention commitment. |
| `APP_VERSION` | *(none)* | Application version string included in structured log output. |
| `SYSTEM_PROMPT_KIAN` | *(none)* | System prompt for the Kian AI persona. Placeholder — not yet consumed by the pipeline. |
| `SYSTEM_PROMPT_NIKA` | *(none)* | System prompt for the Nika AI persona. Placeholder — not yet consumed by the pipeline. |
| `SYSTEM_PROMPT_YARA` | *(none)* | System prompt for the Yara AI persona. Placeholder — not yet consumed by the pipeline. |
| `TTS_VOICE_KIAN` | *(none)* | Google Cloud TTS voice ID for Kian. **Placeholder** — blocked on persona voice selection. |
| `TTS_VOICE_NIKA` | *(none)* | Google Cloud TTS voice ID for Nika. **Placeholder** — blocked on persona voice selection. |
| `TTS_VOICE_YARA` | *(none)* | Google Cloud TTS voice ID for Yara. **Placeholder** — blocked on persona voice selection. |

---

## Mobile (Expo)

All mobile variables must be prefixed with `EXPO_PUBLIC_` to be accessible in the React Native bundle.

### Required

| Variable | Description |
|----------|-------------|
| `EXPO_PUBLIC_CLERK_PUBLISHABLE_KEY` | Clerk publishable key for mobile authentication. Found in the Clerk Dashboard under **API Keys**. |
| `EXPO_PUBLIC_API_BASE_URL` | Base URL of the backend API. Example: `https://api.lisan.app` for production, `http://localhost:5000` for local dev. |

### Optional

| Variable | Default | Description |
|----------|---------|-------------|
| `EXPO_PUBLIC_SENTRY_DSN` | *(disabled)* | Sentry DSN for mobile error tracking. Leave empty or unset to disable. |
| `EXPO_PUBLIC_POSTHOG_KEY` | *(disabled)* | PostHog API key for mobile product analytics. Leave empty or unset to disable. |

---

## Notes

- **Never commit real secrets.** Both `backend/.env.example` and `mobile/.env.example` contain placeholder values only. Copy them to `.env` locally and fill in real values — `.env` files are in `.gitignore`.
- **Render deployment:** Set all required backend variables in the Render service environment settings.
- **Local development:** Copy `backend/.env.example` → `backend/.env` and fill in your own values. The backend reads variables from the process environment; the `dotnet run` CLI picks them up automatically if you use `dotnet run --launch-profile http` with a `launchSettings.json` that references the `.env` file, or export them in your shell.
