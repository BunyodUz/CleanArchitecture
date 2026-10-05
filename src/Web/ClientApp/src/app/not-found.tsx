import { NotFoundView } from "@/views/not-found";
import { AppFrame } from "@/widgets/app-shell";

// Exported as 404.html; the ASP.NET fallback serves it (with a real 404) for unknown URLs.
export default function NotFound() {
  return (
    <AppFrame variant="main">
      <NotFoundView />
    </AppFrame>
  );
}
