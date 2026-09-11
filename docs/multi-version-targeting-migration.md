# Migrating a Connector/Enricher to Multi-Version Targeting

This document tracks the migration of `CluedIn.Enricher.VatLayer` from a single-version build to the
multi-version targeting pattern, part of the same effort that has already migrated 28 other repos
(Dataverse.V2, Gleif, OpenCorporates, Permid, Brreg, KnowledgeGraph, CompanyHouse, CVR, ClearBit,
DuckDuckGo, RestApi, BvD, GoogleImages, libpostal, AzureOpenAI, ArtificialIntelligence, DnB,
SqlServer, AzureServiceBus, Connector.Http, AzureDedicatedSqlPool, PowerApps,
EnterpriseFlows.PowerAutomate, Kafka, GoogleMaps, AzureEventHubs, AzureDataLake).

**Starting state:** this repo's local checkout was found, at the start of this whole effort, sitting
on branch `release/4.6.0` mid an unfinished `git merge origin/develop` — conflicts already resolved
in the working tree but never committed. That predates this migration entirely and wasn't touched
until the repo's owner explicitly reviewed the details and approved discarding it
(`git merge --abort`). This migration starts from a clean `origin/develop`, unrelated to that
abandoned merge.

---

## Overview

| CluedIn version | .NET TFM | Package suffix |
|---|---|---|
| 4.7.0 | net6.0 | `.470` |
| 4.8.0 | net6.0 | `.480` |
| 5.0.0-beta.* | net10.0 | `.500` |

4.6.0 excluded — no stream-repository/connector-shaped API usage in this repo (it's an
ExternalSearch enricher), matching the majority precedent across this effort.

---

## Steps 1-4 — Pipeline, `Directory.Build.props`, `Packages.props`, `NuGet.config`

Standard pattern applied: `crawler.build.jobs.yml` jobs-template with `multiVersionCluedInTargets`
(4.7.0/4.8.0/5.0.0-beta.* — independently verified `5.0.0-*` resolves to `5.0.0-beta.*` for this
repo's own feeds); `Directory.Build.props` honours `CluedInMultiVersionTargetFramework`, derives
`CLUEDIN_V47`/`V48`/`V50`; `Packages.props` `_CluedIn` guarded; `Nuget.config` renamed to
`NuGet.config` (matches the `.sln`'s own reference casing).

---

## Step 5 — Source and test-package audit

**RestSharp 106-vs-114 break**, same family every enricher in this effort has hit:
`VatLayerExternalSearchProvider.cs` — `Method.Get`/`Method.GET` (2 call sites, centralized into one
`HttpGetMethod` const) and two method signatures (`ConstructVerifyConnectionResponse`,
`WaitDueToTooManyRequests`) typed `RestResponse`/`RestResponse<T>` on the new API vs
`IRestResponse`/`IRestResponse<T>` on the old. Fixed with `#if CLUEDIN_V50` guards.

**Test projects:** `test/Directory.Build.props` unconditionally included `xunit.v3`/
`AutoFixture.Xunit3` — split into conditional `ItemGroup`s (xunit v2/AutoFixture.Xunit2 for
non-V50, xunit v3/AutoFixture.Xunit3 for V50), same pattern as every other repo with real test
projects. No source file in either `test/unit` or `test/integration` actually uses an AutoFixture
attribute or `ITestOutputHelper` directly, so no `GlobalUsings.cs` namespace split was needed.

**`CluedIn.Testing.Base` net10.0-only trap:** `test/integration`'s csproj referenced the plain
`CluedIn.Testing.Base` package ID, which only ships a net10.0 build (`NU1202` on the 4.7.0/4.8.0
legs). Switched to the version-suffixed package ID
(`CluedIn.Testing.Base.$(_CluedInPackageSuffix)`, i.e. `.470`/`.480`/`.500`), confirmed all three
suffixed IDs exist on the feed before wiring it up — same fix pattern GoogleMaps' migration
documented.

Verified for real (not just build, `dotnet test`): unit tests pass 127/127 on both net6.0 and
net10.0.

---

## Step 6 — Reset the semantic version (`GitVersion.yml`)

```yaml
next-version: 1.0
ignore:
  sha: []
  commits-before: 2026-06-20T00:00:00
```

This repo's `GitVersion.yml` already had an `ignore: sha: []` block — merged `commits-before` into
it rather than adding a second top-level `ignore:` key, which several other repos in this effort
found silently clobbers a pre-existing one.

Highest *reachable* tag (`git tag --merged HEAD`, not just `sort -V` on every tag — several
unreachable/off-branch tags exist in this repo's history) is `4.6.2` at
`2026-06-17T17:26:59+10:00`. `2026-06-20T00:00:00` sits safely after it. Verified with the pinned
`GitVersion.Tool 5.9.0`: resolves to `1.0.0-multi-version-targeting.161`, correct.

---

## Checklist

- [x] `azure-pipelines.yml` — switched to `crawler.build.jobs.yml` with `multiVersionCluedInTargets` (4.7.0, 4.8.0, 5.0.0-beta.*); pool switched to `ubuntu-22.04`
- [x] `Directory.Build.props` — `CluedInMultiVersionTargetFramework` handling, `DefineConstants`, `LangVersion` pinned to 13.0
- [x] `Packages.props` — `_CluedIn` guarded; test packages split xunit v2/v3 by `CLUEDIN_V50`; `CluedIn.Testing.Base` switched to version-suffixed package ID
- [x] `NuGet.config` — renamed from `Nuget.config`
- [x] `test/Directory.Build.props` — unconditional xunit.v3 split into conditional v2/v3 `ItemGroup`s
- [x] Source — `#if CLUEDIN_V50` guards for the RestSharp 106↔114 API break (4 call sites in `VatLayerExternalSearchProvider.cs`)
- [x] `GitVersion.yml` — `next-version: 1.0`; `commits-before` merged into the existing `ignore:` block; verified `MajorMinorPatch: 1.0.0` with the pinned GitVersion.Tool 5.9.0
- [x] Built clean (0 errors) for all three legs across every project (`src/` + both test projects); real `dotnet test` verified 127/127 passing on net6.0 and net10.0
- [ ] Push branch and confirm the actual Azure DevOps pipeline run is green end-to-end
