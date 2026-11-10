# AGENTS.md

HardwareNexus: modularna platforma e-commerce (komponenty PC). Angular 22 → brama YARP → Product API (MongoDB) i User API (PostgreSQL + EF Core). Cart API (Redis) istnieje, ale nie jest jeszcze w Aspire. Order API to szkielet.

## Struktura
- `Aspire/Aspire.Host` – AppHost: orkiestracja lokalna i seedowanie infrastruktury
- `Services/{Product,User,Cart,Order}` – API, każde z własnymi testami
- `Website/Website.Client` – Angular (storefront)
- `Website/Website.Gateway` – YARP, walidacja JWT z cookie `access_token`
- `HardwareNexus.slnx` – solucja

## Komendy
```
npm ci --prefix Website/Website.Client
dotnet restore HardwareNexus.slnx
dotnet build HardwareNexus.slnx
dotnet run --project Aspire/Aspire.Host            # pełna platforma, wymaga Dockera
npm --prefix Website/Website.Client start          # sam frontend, port 8100
npm --prefix Website/Website.Client run build
dotnet test Services/Cart/tests/CartApi.Application.Test/CartApi.Application.Test.csproj
```
Uwaga: `dotnet test HardwareNexus.slnx` wymaga działającej infrastruktury (OpenBao + zasilona MongoDB dla Product, PostgreSQL na `localhost:5432` dla User). Bez niej testy integracyjne zawodzą. Uruchamiaj projekty testowe osobno albo najpierw wystartuj Aspire.

## Backend (.NET 10)
- Clean Architecture: zależności tylko do środka (Api → Application → Domain; Infrastructure implementuje interfejsy z Application). Domain nie zależy od niczego.
- Product API: MediatR, FluentValidation, Autofac. User API: EF Core + migracje. Zachowuj spójność wzorców w obrębie serwisu.
- Nowe zachowanie = nowy lub zmieniony test. Handlery MediatR testuj na poziomie Application.
- Obsługa błędów: bez połykania wyjątków; walidacja wejścia w FluentValidation, nie w kontrolerach.
- Kategorie produktów w trasach są wrażliwe na wielkość liter: `CPU`, `GPU`, `COOLER`.
- Dodając serwis do środowiska zintegrowanego: zaktualizuj AppHost i trasy YARP, a potem README.

## Frontend (Angular + TypeScript)
- Stan: NgRx Signal Store. Trzymaj logikę w store (`withMethods`), komponenty tylko wyświetlają i wywołują metody.
- Preferuj Signals i `resource()` do pobierania danych zależnych od stanu filtrów. Unikaj ręcznych subskrypcji; jeśli `rxMethod`, to z automatycznym zakończeniem.
- Stosuj typy bezpieczne: `keyof T` zamiast stringów literalnych.
- Nazwy mają być semantycznie precyzyjne.
- TODO: ścieżki do features / store / shared, komenda lintu i testów (`ng test`?).

## Sekrety i bezpieczeństwo
- Nigdy nie commituj prawdziwych danych uwierzytelniających. Lokalnie: `Aspire/Aspire.Host/Properties/dev.secrets.json` (poza gitem), user secrets lub zmienne środowiskowe.
- Aplikacje dostają tylko `OPENBAO_ADDR`, `OPENBAO_ROLE_ID`, `OPENBAO_SECRET_ID` i resztę sekretów pobierają z OpenBao przez VaultSharp (AppRole).
- Skrypt seedujący OpenBao ma pozostać idempotentny (musi działać na Kubernetes).
- Cookie `access_token` wystawia User API; brama go waliduje. Nie zmieniaj tego bez testów.

## Zasady pracy
- Zmiany ograniczaj do jednego serwisu, gdy to możliwe.
- Przed PR-em: build i odpowiednie testy.
- Nie dodawaj zależności bez uzasadnienia. Preferuj rozwiązania open source i bez telemetrii.
- Małe, skupione commity w stylu Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`).
