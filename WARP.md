
# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Code Architecture

This is a C# project for a "Grand Theft Auto V" multiplayer server using the GTANetworkAPI. The project is structured in a modular way, with each feature or system encapsulated in its own module within the `Module` directory.

The server uses a MySQL database to store all persistent data. The database schema is defined by the C# data models, which can be found in their respective module directories (e.g., `PlayerDb.cs`, `Vehicle.cs`, `House.cs`, etc.).

### Key Modules:

*   **`Module/Players`**: Manages player data, including authentication, inventory, and stats.
*   **`Module/Vehicles`**: Manages vehicle data, including ownership, tuning, and garages.
*   **`Module/Houses`**: Manages house data, including ownership, interiors, and tenants.
*   **`Module/Business`**: Manages business data, including ownership, members, and finances.
*   **`Module/Items`**: Manages all in-game items.

## Common Development Tasks

### Building the Project

This project uses the .NET SDK. To build the project, run the following command from the root directory:

```
dotnet build
```

### Running the Project

To run the server, execute the compiled application:

```
dotnet run
```

### Running Tests

This project uses MSTest for unit testing. To run the tests, use the following command:

```
dotnet test
```
