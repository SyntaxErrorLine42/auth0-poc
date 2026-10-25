import { createInterface } from "node:readline/promises";
import { config } from "dotenv";
import { getToken, issuePass } from "./automat";

config({ path: "src/task2/.env" });

const targets: Record<string, string> = {
    "1": "http://localhost:8080/passes",
    "2": "https://auth0-poc.onrender.com/passes",
};

async function main() {
    const rl = createInterface({ input: process.stdin, output: process.stdout });
    const answer = (await rl.question("1 = dev (localhost), 2 = prod (render): ")).trim();
    rl.close();

    const url = targets[answer];
    if (!url) throw new Error("Type 1 or 2");

    const token = await getToken(process.env.AUTHORITY!, process.env.AUDIENCE!, process.env.CLIENT_ID!, process.env.CLIENT_SECRET!, "create:passes");
    await issuePass(url, token, "Luka Volarević", "HR-123456789");
    console.log(`Pass created on ${url}`);
}

main();
