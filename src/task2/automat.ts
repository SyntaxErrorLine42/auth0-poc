import { writeFile } from "node:fs/promises"

export async function getToken(authority: string, audience: string, clientId: string, clientSecret: string, scope: string): Promise<string> {
    const res = await fetch(`${authority}/oauth/token`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            grant_type: "client_credentials",
            client_id: clientId,
            client_secret: clientSecret,
            audience,
            scope,
        }),
    })
    if (!res.ok) throw new Error(`getToken failed: ${res.status} ${await res.text()}`)
    const { access_token } = (await res.json()) as { access_token: string }
    return access_token
}

export async function issuePass(url: string, token: string, name: string, document: string): Promise<void> {
    const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({ name, document }),
    })
    if (res.status !== 201) throw new Error(`issuePass failed: ${res.status}`)
    const id = res.headers.get("location")!.split("/").pop()
    await writeFile(`${id}.png`, Buffer.from(await res.arrayBuffer()))
}
