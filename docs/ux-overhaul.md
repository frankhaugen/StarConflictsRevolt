# UX Overhaul – Star Conflicts Revolt Blazor

This document summarizes the full UX improvements applied across the Blazor client.

## Design system (`wwwroot/app.css`)

- **CSS custom properties** in `:root` for:
  - Brand & UI: `--scr-primary`, `--scr-surface`, `--scr-surface-elevated`
  - Sidebar: `--scr-sidebar-start`, `--scr-sidebar-end`, `--scr-sidebar-text`, hover/active
  - Game map: `--scr-map-bg-start/end`, `--scr-map-node-empire/rebellion/neutral`
  - Semantic: `--scr-success`, `--scr-warning`, `--scr-error`, `--scr-info`
  - Links & focus: `--scr-link`, `--scr-focus-ring`
  - Typography: `--scr-font-sans`, `--scr-radius`, `--scr-shadow`
- **Global styles** use these variables (buttons, links, validation, error boundary, utilities).
- **Utility classes**: `.scr-card`, `.scr-empty-state-icon`, `.scr-loading-spinner`, `.scr-message-*`.

## Shared UX components (`Components/Shared/`)

| Component       | Purpose |
|----------------|---------|
| **LoadingState** | Reusable loading indicator: `Message`, `SubMessage`, `Size` (Small/Medium/Large), `Inverse` (for dark backgrounds). Uses `role="status"`, `aria-live="polite"`. |
| **EmptyState**   | Empty state block: `Title`, `Description`, `IconClass`, `Compact` (for sidebars), `ChildContent` (CTA). |
| **ErrorBanner**  | Page-level error: `Message`, `Dismissible`, `OnDismiss`. Parent clears message on dismiss. |

Used on: Sessions (loading, empty, error), Galaxy (loading, sidebar empty), and available for other pages.

## Error handling

- **Routes.razor**: Route content wrapped in **framework** `Microsoft.AspNetCore.Components.Web.ErrorBoundary` with custom `ErrorContent`: “Something went wrong” card, Go Home + Reload.
- **Sessions**: `ErrorBanner` with dismiss; errors from load/join.
- **Options**: Success/error feedback for Save and Reset (dismissible alert).
- Custom `ErrorBoundary.razor` remains available for component-level use.

## Consistency across pages

- **Loading**: Sessions and Galaxy use `<LoadingState>`; Galaxy loading uses `Inverse="true"` on dark map. Single Player keeps existing “Creating your galaxy…” / “Loading galaxy…” with `aria-live`.
- **Empty**: Sessions and Galaxy sidebar use `<EmptyState>` (Galaxy uses `Compact="true"`).
- **Errors**: Sessions uses `<ErrorBanner>`; Options uses inline alert with dismiss.

## Layout and navigation

- **MainLayout.razor.css**: Sidebar uses `var(--scr-sidebar-*)`; top row uses `--scr-surface-elevated`, light link color for contrast; `#blazor-error-ui` uses `--scr-warning` and `--scr-surface`.
- **NavMenu**: Shows “In game” + session name when `GameState.CurrentSession != null`; subscribes to `GameState.StateChanged` to update; `aria-label` on toggler and nav.
- **404 (Routes.razor)**: Card uses `scr-card`, clearer copy, primary CTA.

## Visual polish

- **Single Player / Galaxy**: Inline styles use design tokens for star-system colors (rebellion/empire/neutral), message-item colors (info/success/warning/error), and map background.
- **Options**: Cards use `scr-card`; Save/Reset show dismissible success message.
- **Focus**: `:focus-visible` and `--scr-focus-ring` for buttons, links, form controls.

## Accessibility

- Loading blocks: `role="status"`, `aria-live="polite"` (Sessions, Galaxy, Single Player).
- NavMenu: `aria-label="Toggle navigation menu"`, `role="navigation"`, `aria-label="Main"`.
- Error and alert regions use `role="alert"` where appropriate.
- Dismiss buttons use `aria-label="Dismiss"`.

## Build note

If `dotnet build` fails with “file is being used by another process”, stop the running Blazor/AppHost process and rebuild. Code changes do not introduce new compile errors.
