import { spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = fileURLToPath(new URL(".", import.meta.url));
const repoRoot = join(scriptDir, "..", "..", "..");
const dotnetExe = process.platform === "win32" ? "dotnet.exe" : "dotnet";
const localApiDotnet = join(repoRoot, "apps", "api", ".dotnet", dotnetExe);
const localRootDotnet = join(repoRoot, ".dotnet", dotnetExe);

let dotnetCommand = "dotnet";

if (existsSync(localApiDotnet)) {
  dotnetCommand = localApiDotnet;
} else if (existsSync(localRootDotnet)) {
  dotnetCommand = localRootDotnet;
}

const result = spawnSync(dotnetCommand, process.argv.slice(2), {
  cwd: process.cwd(),
  stdio: "inherit",
});

if (typeof result.status === "number") {
  process.exit(result.status);
}

process.exit(1);
