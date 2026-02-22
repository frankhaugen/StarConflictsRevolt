# Copilot instructions

This repository is set up to use Aspire. Aspire is an orchestrator for the entire application and will take care of configuring dependencies, building, and running the application. The resources that make up the application are defined in `apphost.cs` including application code and external dependencies.

## General recommendations for working with Aspire
1. Before making any changes always run the apphost using `aspire run` and inspect the state of resources to make sure you are building from a known state.
1. Changes to the _apphost.cs_ file will require a restart of the application to take effect.
2. Make changes incrementally and run the aspire application using the `aspire run` command to validate changes.
3. Use the Aspire MCP tools to check the status of resources and debug issues.

## Running the application
To run the application run the following command:

```
aspire run
```

If there is already an instance of the application running it will prompt to stop the existing instance. You only need to restart the application if code in `apphost.cs` is changed, but if you experience problems it can be useful to reset everything to the starting state.

## Checking resources
To check the status of resources defined in the app model use the _list resources_ tool. This will show you the current state of each resource and if there are any issues. If a resource is not running as expected you can use the _execute resource command_ tool to restart it or perform other actions.

## Listing integrations
IMPORTANT! When a user asks you to add a resource to the app model you should first use the _list integrations_ tool to get a list of the current versions of all the available integrations. You should try to use the version of the integration which aligns with the version of the Aspire.AppHost.Sdk. Some integration versions may have a preview suffix. Once you have identified the correct integration you should always use the _get integration docs_ tool to fetch the latest documentation for the integration and follow the links to get additional guidance.

## Debugging issues
IMPORTANT! Aspire is designed to capture rich logs and telemetry for all resources defined in the app model. Use the following diagnostic tools when debugging issues with the application before making changes to make sure you are focusing on the right things.

1. _list structured logs_; use this tool to get details about structured logs.
2. _list console logs_; use this tool to get details about console logs.
3. _list traces_; use this tool to get details about traces.
4. _list trace structured logs_; use this tool to get logs related to a trace

## Other Aspire MCP tools

1. _select apphost_; use this tool if working with multiple app hosts within a workspace.
2. _list apphosts_; use this tool to get details about active app hosts.

## Playwright and playtesting

**When asked to "play" the game or verify it works**, run the automated playtest (no MCP or live browser required):

```powershell
.\scripts\playtest.ps1
```

From repo root. This runs Playwright UI tests against an in-process Blazor host. Exit code 0 = game is playable. See [operations/current-issues.md](../operations/current-issues.md) and [operations/playtest-runbook.md](../operations/playtest-runbook.md).

**If using the Playwright MCP** (e.g. server id `user-microsoft/playwright-mcp`) for interactive playtesting against the full stack:

1. **Run the stack first**: `dotnet run --project StarConflictsRevolt.Aspire.AppHost` (or `aspire run`). Wait until webapi and blazor are healthy.
2. **Use Playwright MCP** (not cursor-ide-browser) so you get full snapshots with element refs: `browser_navigate` to the Blazor URL (typically `https://localhost:7120`), then `browser_snapshot`, then `browser_click` with refs from the snapshot.
3. **Playtest flow**: Navigate to `/singleplayer` to create a session and load the galaxy, or go to `/sessions` and click Join on a session. See [operations/playtest-runbook.md](../operations/playtest-runbook.md) for the full runbook.
4. To get the Blazor URL when using Aspire, use the Aspire MCP _list resources_ tool, or assume `https://localhost:7120` if using default launch profiles.

## Updating the app host
The user may request that you update the Aspire apphost. You can do this using the `aspire update` command. This will update the apphost to the latest version and some of the Aspire specific packages in referenced projects, however you may need to manually update other packages in the solution to ensure compatibility. You can consider using the `dotnet-outdated` with the users consent. To install the `dotnet-outdated` tool use the following command:

```
dotnet tool install --global dotnet-outdated-tool
```

## Persistent containers
IMPORTANT! Consider avoiding persistent containers early during development to avoid creating state management issues when restarting the app.

## Aspire workload
IMPORTANT! The aspire workload is obsolete. You should never attempt to install or use the Aspire workload.

## Official documentation
IMPORTANT! Always prefer official documentation when available. The following sites contain the official documentation for Aspire and related components

1. https://aspire.dev
2. https://learn.microsoft.com/dotnet/aspire
3. https://nuget.org (for specific integration package details)