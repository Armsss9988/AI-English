import { spawn, spawnSync } from "node:child_process";
import { existsSync, mkdirSync } from "node:fs";
import { join } from "node:path";

const repoRoot = process.cwd();
const apiUrl = process.env.SCHEMATHESIS_API_URL ?? "http://127.0.0.1:5128";
const schemaUrl = `${apiUrl}/swagger/v1/swagger.json`;
const reportDir = join(repoRoot, "artifacts", "schemathesis");
const dotnetScript = join(repoRoot, "apps", "api", "scripts", "run-dotnet.mjs");

mkdirSync(reportDir, { recursive: true });

const api = spawn(
  process.execPath,
  [
    dotnetScript,
    "run",
    "--project",
    "apps/api/src/EnglishCoach.Api/EnglishCoach.Api.csproj",
    "--no-build",
    "--urls",
    apiUrl,
  ],
  {
    cwd: repoRoot,
    env: {
      ...process.env,
      ASPNETCORE_ENVIRONMENT: "Development",
      ASPNETCORE_URLS: apiUrl,
    },
    stdio: "inherit",
  }
);

let shuttingDown = false;

const stopApi = () => {
  if (!shuttingDown && !api.killed) {
    shuttingDown = true;
    api.kill();
  }
};

process.on("exit", stopApi);
process.on("SIGINT", () => {
  stopApi();
  process.exit(130);
});
process.on("SIGTERM", () => {
  stopApi();
  process.exit(143);
});

try {
  await waitForSchema(schemaUrl);
  const schemathesis = findSchemathesisExecutable();
  const result = spawnSync(
    schemathesis,
    [
      "run",
      schemaUrl,
      "--checks",
      "not_a_server_error",
      "--max-examples",
      process.env.SCHEMATHESIS_MAX_EXAMPLES ?? "10",
      "--generation-database",
      ":memory:",
      "--report",
      "junit",
      "--report-junit-path",
      join(reportDir, "junit.xml"),
      "--no-color",
    ],
    {
      cwd: repoRoot,
      env: {
        ...process.env,
        PYTHONUTF8: "1",
        PYTHONIOENCODING: "utf-8",
      },
      stdio: "inherit",
    }
  );

  process.exitCode = typeof result.status === "number" ? result.status : 1;
} finally {
  stopApi();
}

async function waitForSchema(url) {
  const deadline = Date.now() + 30_000;
  let lastError;

  while (Date.now() < deadline) {
    try {
      const response = await fetch(url);

      if (response.ok) {
        return;
      }

      lastError = new Error(`Schema returned HTTP ${response.status}`);
    } catch (error) {
      lastError = error;
    }

    await new Promise((resolve) => setTimeout(resolve, 500));
  }

  throw new Error(
    `Timed out waiting for ${url}: ${lastError?.message ?? "no response"}`
  );
}

function findSchemathesisExecutable() {
  const script = [
    "import os, sysconfig",
    "name = 'schemathesis.exe' if os.name == 'nt' else 'schemathesis'",
    "print(os.path.join(sysconfig.get_path('scripts'), name))",
  ].join("; ");
  const result = spawnSync("python", ["-c", script], {
    cwd: repoRoot,
    encoding: "utf8",
  });

  if (result.status === 0 && existsSync(result.stdout.trim())) {
    return result.stdout.trim();
  }

  const userScript = [
    "import os, sysconfig",
    "name = 'schemathesis.exe' if os.name == 'nt' else 'schemathesis'",
    "print(os.path.join(sysconfig.get_path('scripts', scheme='nt_user' if os.name == 'nt' else 'posix_user'), name))",
  ].join("; ");
  const userResult = spawnSync("python", ["-c", userScript], {
    cwd: repoRoot,
    encoding: "utf8",
  });

  if (userResult.status === 0 && existsSync(userResult.stdout.trim())) {
    return userResult.stdout.trim();
  }

  return "schemathesis";
}
