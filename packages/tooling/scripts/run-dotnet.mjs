import { spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = fileURLToPath(new URL(".", import.meta.url));
const repoRoot = join(scriptDir, "..", "..", "..");
const dotnetExe = process.platform === "win32" ? "dotnet.exe" : "dotnet";
const localDotnet = join(repoRoot, ".dotnet", dotnetExe);

if (!existsSync(localDotnet)) {
  console.error(`Missing local dotnet executable at ${localDotnet}`);
  process.exit(1);
}

const result = spawnSync(localDotnet, process.argv.slice(2), {
  cwd: repoRoot,
  stdio: "inherit",
});

if (typeof result.status === "number") {
  process.exit(result.status);
}

process.exit(1);
