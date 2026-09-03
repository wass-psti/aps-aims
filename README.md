# APS AIMS — UI Rework

This package contains a **restyle-only** update: 13 CSS files under `client/src/`
that replace the app's visuals with a cleaner, more modern look. No `.tsx`
component files were touched, no class names were renamed, and no build
config was changed — this was verified with a full `tsc -b && vite build`
against the actual repo before packaging.

## What changed
- New design-token system (`index.css`): consistent color palette (indigo
  accent, neutral grays), spacing, radii, and shadow scale via CSS variables.
- `App.css`: refreshed topbar (pill-style nav tabs, frosted glass, gradient
  brand mark), panels, tables, forms, buttons, alerts, and the asset drawer
  (with subtle open animation).
- All the `v0.x` / `v1.0-*` files (custody, transactions, employees,
  identification/QR, service history, fullscreen asset view, image
  placeholders, inventory/reports/incidents, audit log, login, user edit)
  were restyled to match the same system — same layout/grid structure, new
  colors, spacing and micro-interactions (hover states, focus rings).
- Login page got a distinct look: soft gradient blobs behind a floating card.

## How to apply
Copy the `client/src/` folder from this zip over your project's `client/src/`
folder, overwriting the files with the same names:

```
index.css
App.css
v0.5.css
v0.6.css
v0.6-employee.css
v0.7.css
v0.8.css
v0.8-fullscreen.css
v0.8-placeholder.css
v0.9.css
v1.0-audit.css
v1.0-auth.css
v1.0-user-edit.css
```

Nothing else in the repo needs to change — `App.tsx` already imports these
files in this order, and every class name used in your components
(`.panel`, `.button.primary`, `.badge`, `.nav-tabs`, `.asset-drawer`, etc.)
is still defined with the same meaning, just restyled.

Then just run your normal build/dev commands (`npm install` if needed,
`npm run dev` or `npm run build`).
