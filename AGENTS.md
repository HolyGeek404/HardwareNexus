# AGENTS.md

HardwareNexus: modularna platforma e-commerce (komponenty PC). Angular 22 → brama YARP (BFF z logowaniem przez Keycloak) → Product API (MongoDB) i User API (PostgreSQL + EF Core). Cart API (Redis) istnieje, ale nie jest jeszcze w Aspire. Order API to pusty szkielet.

## Struktura
- `Aspire/Aspire.Host` – AppHost: orkiestracja lokalna (grupy Website / Api / Infrastructure). Uruchamia OpenBao i MongoDB z seedem oraz Keycloak z importem realmu `Scripts/Keycloak/hardware-nexus-realm.json`. PostgreSQL **nie** jest w AppHost – musi działać osobno na `localhost:5432`.
- `Services/{Product,User,Cart,Order}` – API; Product, User i Cart mają własne testy, Order nie ma
- `Website/Website.Client` – Angular (storefront)
- `Website/Website.Gateway` – YARP + BFF: logowanie OIDC (Keycloak, code + PKCE), sesja w cookie `hn-bff`, endpointy `/gateway/user/{login,me,signout}` w `Program.cs`
- `HardwareNexus.slnx` – solucja

## Komendy
```
npm ci --prefix Website/Website.Client
dotnet restore HardwareNexus.slnx
dotnet build HardwareNexus.slnx
dotnet run --project Aspire/Aspire.Host            # pełna platforma, wymaga Dockera
npm --prefix Website/Website.Client start          # sam frontend, port 8100, proxy z proxy.conf.json
npm --prefix Website/Website.Client run build
dotnet test Services/Cart/tests/CartApi.Application.Test/CartApi.Application.Test.csproj
dotnet test Services/User/tests/UserApi.Application.Tests/UserApi.Application.Tests.csproj
```
Uwaga: `dotnet test HardwareNexus.slnx` wymaga działającej infrastruktury (OpenBao + zasilona MongoDB dla Product, PostgreSQL na `localhost:5432` dla User). Bez niej testy integracyjne (`ProductApi.Api.Tests/Integration`, `UserApi.Presentation.Tests`) zawodzą. Uruchamiaj projekty testowe osobno albo najpierw wystartuj Aspire.

## Backend (.NET 10)
- Clean Architecture: zależności tylko do środka (Presentation → Application → Domain; Infrastructure implementuje interfejsy z Application). Domain nie zależy od niczego.
- Stan serwisów (zachowuj spójność wzorców w obrębie serwisu):
  - **Product API** – jeden projekt `ProductApi.Api` (bez podziału na warstwy): Autofac, kontroler → `ProductService` → `ProductRepositoryFactory` → repozytoria Mongo rejestrowane jako keyed services po `ProductCategory`. Bez MediatR i FluentValidation.
  - **User API** – warstwy Presentation/Application/Domain/Infrastructure, MediatR + `ValidationBehavior` (FluentValidation), EF Core + migracje (stosowane przy starcie, seed w Development).
  - **Cart API** – warstwy Presentation/Application/Domain/Infrastructure, MediatR, Redis.
  - **Order API** – tylko szkielet projektów.
- Nowe zachowanie = nowy lub zmieniony test. Handlery MediatR testuj na poziomie Application.
- Obsługa błędów: bez połykania wyjątków; walidacja wejścia w FluentValidation (tam, gdzie serwis jej używa), nie w kontrolerach.
- Kategorie produktów: enum `ProductCategory` (`Cpu`, `Gpu`, `Cooler`) w API i jego odpowiednik w `shared/models/enums.ts`; frontend wysyła w trasach `CPU`, `GPU`, `COOLER`. Dodając kategorię, zaktualizuj oba enumy, rejestrację repozytorium i trasy Angulara.
- Dodając serwis do środowiska zintegrowanego: zaktualizuj AppHost (`GroupResourceExtensions.cs`), trasy i klastry YARP w `Website.Gateway/appsettings.Development.json`, a potem README.

## Frontend (Angular + TypeScript)
- Układ: `src/app/business/<feature>/{api,components,models}` (features: `products`, `user`, `cart`) oraz `src/app/shared/{components,models,services,store}`.
- Komunikacja z bramą: klasy `*Api` dziedziczą z `shared/services/base-http.service.ts` (`get`/`post` z `withCredentials`). Adres bramy w `src/environments/environment.ts`.
- Sesja: `shared/store/session.store.ts` (NgRx Signal Store) – `initialize()` pyta `/gateway/user/me`, `login()`/`logout()` przekierowują do bramy. `authGuard` czeka na `initialize()`.
- Stan: NgRx Signal Store. Trzymaj logikę w store (`withMethods`), komponenty tylko wyświetlają i wywołują metody. (Komponenty produktów jeszcze pobierają dane same przez `toSignal` + `switchMap` – przy zmianach przenoś to do store.)
- Preferuj Signals i `resource()` do pobierania danych zależnych od stanu filtrów. Unikaj ręcznych subskrypcji; jeśli `rxMethod`, to z automatycznym zakończeniem.
- Stosuj typy bezpieczne: `keyof T` zamiast stringów literalnych.
- Nazwy mają być semantycznie precyzyjne.
- Testy: `npm --prefix Website/Website.Client test` (Karma + Jasmine); obecnie brak plików `*.spec.ts`. Brak skonfigurowanego lintera (jest tylko konfiguracja Prettier w `package.json`).

## Sekrety i bezpieczeństwo
- Nigdy nie commituj prawdziwych danych uwierzytelniających. Lokalnie: `Aspire/Aspire.Host/Properties/dev.secrets.json` (poza gitem), user secrets lub zmienne środowiskowe. Uwaga: `Keycloak:ClientSecret` bramy leży obecnie w `Website.Gateway/appsettings*.json` – do przeniesienia, nie powielaj tego wzorca.
- Docelowo aplikacje dostają tylko `OPENBAO_ADDR` oraz `OPENBAO_<SERWIS>_ROLE_ID` / `OPENBAO_<SERWIS>_SECRET_ID` (np. `OPENBAO_PRODUCT_ROLE_ID`) i resztę sekretów pobierają z OpenBao przez VaultSharp (AppRole). Tak działa Product API; User API wciąż czyta dane PostgreSQL z konfiguracji (`PostgresSql:*`).
- Skrypt seedujący OpenBao ma pozostać idempotentny (musi działać na Kubernetes).
- Uwierzytelnianie użytkownika: brama (OIDC + cookie `hn-bff`) jest źródłem sesji. Starsza ścieżka w User API (`/user/signin` wystawiające cookie `access_token`, schemat `CookieJwt`) to pozostałość. Nie zmieniaj przepływu logowania bez testów.

## Zasady pracy
- Zmiany ograniczaj do jednego serwisu, gdy to możliwe.
- Przed PR-em: build i odpowiednie testy.
- Nie dodawaj zależności bez uzasadnienia. Preferuj rozwiązania open source i bez telemetrii.
- Małe, skupione commity w stylu Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`).
