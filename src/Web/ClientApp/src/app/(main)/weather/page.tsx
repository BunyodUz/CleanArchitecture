import { AuthGuard } from "@/features/auth";
import { WeatherView } from "@/views/weather";

export default function Page() {
  return (
    <AuthGuard>
      <WeatherView />
    </AuthGuard>
  );
}
