import test from "node:test";
import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const currentDir = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(currentDir, "..", "..", "..");

const requiredPaths = [
  "apps/web/package.json",
  "apps/api/EnglishCoach.sln",
  "apps/api/src/EnglishCoach.Api/EnglishCoach.Api.csproj",
  "apps/api/src/EnglishCoach.Application/EnglishCoach.Application.csproj",
  "apps/api/src/EnglishCoach.Domain/EnglishCoach.Domain.csproj",
  "apps/api/src/EnglishCoach.Infrastructure/EnglishCoach.Infrastructure.csproj",
  "apps/api/src/EnglishCoach.Contracts/EnglishCoach.Contracts.csproj",
  "apps/api/src/EnglishCoach.SharedKernel/EnglishCoach.SharedKernel.csproj",
  "apps/api/scripts/run-dotnet.mjs",
  "packages/contracts/src/index.ts",
  "packages/shared-kernel/src/index.ts",
  "packages/ui/src/index.ts",
  "packages/tooling/test/foundation.test.mjs"
];

test("foundation scaffold creates the required monorepo paths", () => {
  for (const relativePath of requiredPaths) {
    assert.equal(
      existsSync(join(repoRoot, relativePath)),
      true,
      `Expected ${relativePath} to exist`
    );
  }
});
