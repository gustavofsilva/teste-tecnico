import { mkdir, writeFile } from 'node:fs/promises';

const rawApiUrl = (process.env.API_URL ?? '').trim().replace(/\/$/, '');
if (rawApiUrl && !/^https?:\/\//i.test(rawApiUrl)) {
  throw new Error('API_URL deve ser uma URL HTTP(S) absoluta.');
}

const directory = new URL('../src/environments/', import.meta.url);
const destination = new URL('environment.generated.ts', directory);
await mkdir(directory, { recursive: true });
await writeFile(
  destination,
  `// Gerado automaticamente; não adicione segredos neste arquivo.\nexport const environment = { apiUrl: ${JSON.stringify(rawApiUrl)} } as const;\n`,
  'utf8'
);
